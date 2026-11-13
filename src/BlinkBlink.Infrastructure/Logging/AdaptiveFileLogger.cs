using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlinkBlink.Application.Common.Logging;

namespace BlinkBlink.Infrastructure.Logging;

/// <summary>
/// Bộ ghi nhật ký thích ứng ra tệp xoay vòng theo ngày ở định dạng JSON Lines UTF-8.
/// Tự động điều chỉnh mức độ chi tiết theo môi trường Development/Production, kênh Web/WinForms và chế độ CSDL.
/// </summary>
public sealed class AdaptiveFileLogger : IAppLogger
{
    private readonly string _logDirectory;
    private readonly string _filePrefix;
    private readonly bool _writeToConsole;
    private readonly AppLogLevel _minimumLevel;
    private readonly object _syncLock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public AppLogContext DefaultContext { get; }

    /// <summary>
    /// Đường dẫn thư mục đang dùng để lưu tệp nhật ký.
    /// </summary>
    public string LogDirectory => _logDirectory;

    /// <summary>
    /// Khởi tạo bộ ghi nhật ký thích ứng với ngữ cảnh vận hành cụ thể.
    /// </summary>
    /// <param name="defaultContext">Ngữ cảnh mặc định (kênh, môi trường, chế độ dữ liệu)</param>
    /// <param name="preferredDirectory">Thư mục ưu tiên lưu log</param>
    /// <param name="filePrefix">Tiền tố tên tệp log (ví dụ: web hoặc admin)</param>
    /// <param name="writeToConsole">Bật ghi thêm ra Console/Debug khi cần chẩn đoán</param>
    public AdaptiveFileLogger(
        AppLogContext defaultContext,
        string? preferredDirectory = null,
        string filePrefix = "app",
        bool writeToConsole = false)
    {
        DefaultContext = defaultContext ?? new AppLogContext();
        _filePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "app" : filePrefix.Trim().ToLowerInvariant();
        _writeToConsole = writeToConsole;

        var isDevelopment = string.Equals(DefaultContext.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase);
        _minimumLevel = isDevelopment ? AppLogLevel.Debug : AppLogLevel.Information;
        _logDirectory = ResolveWritableDirectory(preferredDirectory);
    }

    public void Log(
        AppLogLevel level,
        string source,
        string message,
        string? errorCode = null,
        Exception? exception = null,
        AppLogContext? overrideContext = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        if (level < _minimumLevel)
        {
            return;
        }

        var ctx = overrideContext ?? DefaultContext;
        var isDevelopment = string.Equals(ctx.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase);

        // Chế độ thích ứng: chỉ đính kèm StackTrace đầy đủ khi ở Development hoặc lỗi mức Error/Critical
        string? stackTrace = null;
        if (exception != null && (isDevelopment || level >= AppLogLevel.Error))
        {
            stackTrace = exception.ToString();
        }

        var record = new
        {
            timestamp = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
            level = level.ToString().ToUpperInvariant(),
            channel = ctx.Channel,
            environment = ctx.EnvironmentName,
            dataMode = ctx.DataMode,
            correlationId = ctx.CorrelationId,
            userId = ctx.UserId,
            userRole = ctx.UserRole,
            errorCode,
            source,
            message,
            exceptionType = exception?.GetType().FullName,
            exceptionMessage = exception?.Message,
            stackTrace,
            metadata
        };

        var jsonLine = JsonSerializer.Serialize(record, JsonOptions);

        try
        {
            var filePath = Path.Combine(_logDirectory, $"{_filePrefix}-{DateTime.Now:yyyyMMdd}.log");
            lock (_syncLock)
            {
                Directory.CreateDirectory(_logDirectory);
                using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                writer.WriteLine(jsonLine);
            }
        }
        catch (Exception ioEx)
        {
            // Tránh ném ngoại lệ ngược từ logger làm gián đoạn luồng nghiệp vụ chính
            Debug.WriteLine($"[AdaptiveFileLogger] Không thể ghi tệp log: {ioEx.Message}");
        }

        if (_writeToConsole || isDevelopment)
        {
            var shortLine = $"[{level}] [{ctx.Channel}|{ctx.DataMode}] {source}: {message}" +
                            (string.IsNullOrWhiteSpace(errorCode) ? string.Empty : $" (Code={errorCode})");
            Debug.WriteLine(shortLine);
            if (_writeToConsole)
            {
                Console.WriteLine(shortLine);
            }
        }
    }

    /// <summary>
    /// Che mật khẩu hoặc chuỗi nhạy cảm trong chuỗi kết nối trước khi ghi log.
    /// </summary>
    public static string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        return Regex.Replace(
            connectionString,
            @"(Password|Pwd)\s*=\s*[^;]*",
            "$1=***",
            RegexOptions.IgnoreCase);
    }

    private static string ResolveWritableDirectory(string? preferredDirectory)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(preferredDirectory))
        {
            candidates.Add(preferredDirectory);
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            candidates.Add(Path.Combine(localAppData, "BlinkBlink", "logs"));
        }

        candidates.Add(Path.Combine(AppContext.BaseDirectory, "logs"));
        candidates.Add(Path.Combine(Path.GetTempPath(), "BlinkBlink", "logs"));

        foreach (var dir in candidates)
        {
            try
            {
                Directory.CreateDirectory(dir);
                return dir;
            }
            catch
            {
                // Chuyển sang thư mục dự phòng nếu thiếu quyền ghi
            }
        }

        return AppContext.BaseDirectory;
    }
}

