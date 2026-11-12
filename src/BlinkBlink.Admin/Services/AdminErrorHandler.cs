using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Infrastructure.Data;
using BlinkBlink.Infrastructure.Logging;

namespace BlinkBlink.Admin.Services;

/// <summary>
/// Bộ xử lý lỗi và ghi nhật ký thích ứng trung tâm cho ứng dụng Desktop WinForms.
/// Phân loại lỗi nghiệp vụ, lỗi CSDL và lỗi hệ thống để phản hồi phù hợp trên giao diện mà không làm sập phiên bán hàng.
/// </summary>
public static class AdminErrorHandler
{
    /// <summary>
    /// Đăng ký các bộ lắng nghe ngoại lệ toàn cục cho tiến trình WinForms.
    /// </summary>
    public static void RegisterGlobalHandlers()
    {
        System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        System.Windows.Forms.Application.ThreadException += (_, args) =>
        {
            Handle(args.Exception, "WinForms.UIThread");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                Handle(ex, "WinForms.AppDomain", isTerminating: args.IsTerminating);
            }
        };

        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Handle(args.Exception, "WinForms.BackgroundTask", showDialog: false);
            args.SetObserved();
        };
    }

    /// <summary>
    /// Xử lý ngoại lệ theo cơ chế thích ứng: ghi nhật ký có cấu trúc và hiển thị cảnh báo phù hợp với ngữ cảnh.
    /// </summary>
    /// <param name="rawException">Ngoại lệ phát sinh</param>
    /// <param name="source">Tên màn hình hoặc tác vụ phát sinh lỗi</param>
    /// <param name="showDialog">Có hiển thị hộp thoại thông báo cho người dùng hay không</param>
    /// <param name="isTerminating">Cờ báo hiệu tiến trình sắp kết thúc</param>
    /// <returns>Mã truy vết 8 ký tự để đối soát với tệp nhật ký</returns>
    public static string Handle(Exception rawException, string source, bool showDialog = true, bool isTerminating = false)
    {
        var scope = AdminServiceScope.Instance;
        var traceId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        Exception effective = rawException is SqlException sqlEx
            ? SqlExceptionTranslator.Translate(sqlEx, source)
            : rawException;

        var (level, errorCode, title, userMessage, icon) = Classify(effective, traceId, isTerminating);

        var ctx = new AppLogContext
        {
            Channel = scope.Logger.DefaultContext.Channel,
            EnvironmentName = scope.Logger.DefaultContext.EnvironmentName,
            DataMode = scope.IsDatabaseConnected ? "SqlServer" : "InMemory",
            CorrelationId = traceId,
            UserId = scope.CurrentUser?.MaNguoiDung,
            UserRole = scope.CurrentUser?.MaVaiTro
        };

        var metadata = new Dictionary<string, object?>
        {
            ["isTerminating"] = isTerminating
        };

        scope.Logger.Log(level, source, effective.Message, errorCode, rawException, ctx, metadata);

        if (showDialog && Environment.UserInteractive)
        {
            var isDev = string.Equals(ctx.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase) || Debugger.IsAttached;
            var fullMessage = isDev && level >= AppLogLevel.Error
                ? $"{userMessage}\n\n[Chẩn đoán Dev]: {rawException.Message}"
                : userMessage;

            MessageBox.Show(fullMessage, title, MessageBoxButtons.OK, icon);
        }

        return traceId;
    }

    private static (AppLogLevel Level, string ErrorCode, string Title, string Message, MessageBoxIcon Icon) Classify(
        Exception ex,
        string traceId,
        bool isTerminating)
    {
        var logDir = (AdminServiceScope.Instance.Logger as AdaptiveFileLogger)?.LogDirectory ?? "logs";

        return ex switch
        {
            ValidationDomainException v => (AppLogLevel.Warning, v.ErrorCode, "Dữ liệu chưa hợp lệ", v.Message, MessageBoxIcon.Warning),
            BusinessRuleException b => (AppLogLevel.Warning, b.ErrorCode, "Cảnh báo nghiệp vụ", b.Message, MessageBoxIcon.Warning),
            NotFoundDomainException n => (AppLogLevel.Warning, n.ErrorCode, "Không tìm thấy dữ liệu", n.Message, MessageBoxIcon.Warning),
            ConflictDomainException c => (AppLogLevel.Warning, c.ErrorCode, "Xung đột dữ liệu", c.Message, MessageBoxIcon.Warning),
            ForbiddenDomainException f => (AppLogLevel.Warning, f.ErrorCode, "Truy cập bị từ chối", f.Message, MessageBoxIcon.Stop),
            UnauthorizedDomainException u => (AppLogLevel.Warning, u.ErrorCode, "Xác thực không thành công", u.Message, MessageBoxIcon.Warning),
            DataAccessException d => (
                AppLogLevel.Error,
                d.ErrorCode,
                "Sự cố kết nối dữ liệu",
                $"{d.Message}\n\nMã lỗi: {d.ErrorCode} | Mã tra cứu: {traceId}",
                MessageBoxIcon.Error),
            ArgumentException a => (AppLogLevel.Warning, ErrorCodes.InvalidInput, "Dữ liệu chưa hợp lệ", a.Message, MessageBoxIcon.Warning),
            _ => (
                isTerminating ? AppLogLevel.Critical : AppLogLevel.Error,
                ErrorCodes.UnhandledSystemError,
                "Lỗi hệ thống",
                $"Đã xảy ra sự cố không mong muốn trong quá trình xử lý.\nMã lỗi: {ErrorCodes.UnhandledSystemError}\nMã tra cứu: {traceId}\nThư mục nhật ký: {logDir}",
                MessageBoxIcon.Error)
        };
    }
}
