using System;
using System.Collections.Generic;

namespace BlinkBlink.Application.Common.Logging;

/// <summary>
/// Mức độ nghiêm trọng của bản ghi nhật ký hệ thống.
/// </summary>
public enum AppLogLevel
{
    Debug = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// Ngữ cảnh vận hành hiện tại giúp bộ ghi nhật ký thích ứng theo kênh chạy, môi trường và chế độ dữ liệu.
/// </summary>
public sealed class AppLogContext
{
    public string Channel { get; init; } = "Core";
    public string EnvironmentName { get; init; } = "Production";
    public string DataMode { get; set; } = "SqlServer";
    public string? CorrelationId { get; init; }
    public string? UserId { get; init; }
    public string? UserRole { get; init; }
}

/// <summary>
/// Hợp đồng ghi nhật ký thích ứng độc lập với nền tảng giao diện và công nghệ lưu trữ.
/// </summary>
public interface IAppLogger
{
    /// <summary>
    /// Ngữ cảnh mặc định gắn liền với phiên chạy hiện tại.
    /// </summary>
    AppLogContext DefaultContext { get; }

    /// <summary>
    /// Ghi một bản ghi nhật ký có cấu trúc.
    /// </summary>
    /// <param name="level">Mức độ nhật ký</param>
    /// <param name="source">Tên lớp hoặc phân hệ phát sinh</param>
    /// <param name="message">Nội dung thông điệp</param>
    /// <param name="errorCode">Mã lỗi chuẩn hóa nếu có</param>
    /// <param name="exception">Ngoại lệ kỹ thuật nếu có</param>
    /// <param name="overrideContext">Ngữ cảnh ghi đè theo từng yêu cầu hoặc phiên người dùng</param>
    /// <param name="metadata">Thông tin bổ sung dạng khóa-giá trị</param>
    void Log(
        AppLogLevel level,
        string source,
        string message,
        string? errorCode = null,
        Exception? exception = null,
        AppLogContext? overrideContext = null,
        IReadOnlyDictionary<string, object?>? metadata = null);
}

/// <summary>
/// Các phương thức mở rộng giúp ghi nhật ký ngắn gọn tại tầng nghiệp vụ và giao diện.
/// </summary>
public static class AppLoggerExtensions
{
    public static void LogDebug(this IAppLogger logger, string source, string message, AppLogContext? context = null, IReadOnlyDictionary<string, object?>? metadata = null)
        => logger.Log(AppLogLevel.Debug, source, message, null, null, context, metadata);

    public static void LogInfo(this IAppLogger logger, string source, string message, AppLogContext? context = null, IReadOnlyDictionary<string, object?>? metadata = null)
        => logger.Log(AppLogLevel.Information, source, message, null, null, context, metadata);

    public static void LogWarning(this IAppLogger logger, string source, string message, string? errorCode = null, Exception? exception = null, AppLogContext? context = null, IReadOnlyDictionary<string, object?>? metadata = null)
        => logger.Log(AppLogLevel.Warning, source, message, errorCode, exception, context, metadata);

    public static void LogError(this IAppLogger logger, string source, string message, string? errorCode = null, Exception? exception = null, AppLogContext? context = null, IReadOnlyDictionary<string, object?>? metadata = null)
        => logger.Log(AppLogLevel.Error, source, message, errorCode, exception, context, metadata);

    public static void LogCritical(this IAppLogger logger, string source, string message, string? errorCode = null, Exception? exception = null, AppLogContext? context = null, IReadOnlyDictionary<string, object?>? metadata = null)
        => logger.Log(AppLogLevel.Critical, source, message, errorCode, exception, context, metadata);
}

/// <summary>
/// Bộ ghi nhật ký rỗng dùng làm mặc định an toàn trong kiểm thử đơn vị khi không cần ghi ra đĩa.
/// </summary>
public sealed class NullAppLogger : IAppLogger
{
    public static NullAppLogger Instance { get; } = new();

    public AppLogContext DefaultContext { get; } = new()
    {
        Channel = "Test",
        EnvironmentName = "Development",
        DataMode = "Mock"
    };

    private NullAppLogger() { }

    public void Log(
        AppLogLevel level,
        string source,
        string message,
        string? errorCode = null,
        Exception? exception = null,
        AppLogContext? overrideContext = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
    }
}

