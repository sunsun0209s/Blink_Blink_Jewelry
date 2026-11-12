using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Web.Middleware;

/// <summary>
/// Middleware xử lý ngoại lệ tập trung và thích ứng cho toàn bộ ứng dụng Web.
/// Tự động ánh xạ lỗi sang HTTP JSON cho API hoặc trang lỗi MVC cho trình duyệt, đồng thời ẩn chi tiết nhạy cảm ở Production.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppLogger _appLogger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        IAppLogger appLogger,
        IHostEnvironment env)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _appLogger = appLogger ?? throw new ArgumentNullException(nameof(appLogger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = ResolveCorrelationId(context);
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
            {
                context.Response.Headers["X-Correlation-Id"] = traceId;
            }
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception rawException, string traceId)
    {
        // Chuẩn hóa SqlException sang DomainException để xử lý đồng nhất
        Exception effectiveException = rawException is SqlException sqlEx
            ? SqlExceptionTranslator.Translate(sqlEx, context.Request.Path)
            : rawException;

        var (statusCode, errorCode, safeMessage, logLevel) = MapException(effectiveException);

        var logContext = new AppLogContext
        {
            Channel = _appLogger.DefaultContext.Channel,
            EnvironmentName = _env.EnvironmentName,
            DataMode = _appLogger.DefaultContext.DataMode,
            CorrelationId = traceId,
            UserId = context.Request.Headers["X-User-Id"].ToString()
        };

        var metadata = new Dictionary<string, object?>
        {
            ["method"] = context.Request.Method,
            ["path"] = context.Request.Path.Value,
            ["statusCode"] = statusCode
        };

        _appLogger.Log(
            logLevel,
            nameof(GlobalExceptionMiddleware),
            effectiveException.Message,
            errorCode,
            rawException,
            logContext,
            metadata);

        if (context.Response.HasStarted)
        {
            return;
        }

        if (IsApiRequest(context))
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = new Dictionary<string, object?>
            {
                ["success"] = false,
                ["code"] = errorCode,
                ["message"] = safeMessage,
                ["traceId"] = traceId
            };

            // Chỉ đính kèm thông tin kỹ thuật khi chạy ở môi trường Development
            if (_env.IsDevelopment())
            {
                payload["detail"] = rawException.Message;
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.Redirect($"/Home/Error?traceId={Uri.EscapeDataString(traceId)}");
    }

    private static (int StatusCode, string ErrorCode, string SafeMessage, AppLogLevel LogLevel) MapException(Exception ex)
    {
        return ex switch
        {
            ValidationDomainException v => ((int)HttpStatusCode.BadRequest, v.ErrorCode, v.Message, AppLogLevel.Warning),
            BusinessRuleException b => ((int)HttpStatusCode.BadRequest, b.ErrorCode, b.Message, AppLogLevel.Warning),
            UnauthorizedDomainException u => ((int)HttpStatusCode.Unauthorized, u.ErrorCode, u.Message, AppLogLevel.Warning),
            ForbiddenDomainException f => ((int)HttpStatusCode.Forbidden, f.ErrorCode, f.Message, AppLogLevel.Warning),
            NotFoundDomainException n => ((int)HttpStatusCode.NotFound, n.ErrorCode, n.Message, AppLogLevel.Warning),
            ConflictDomainException c => ((int)HttpStatusCode.Conflict, c.ErrorCode, c.Message, AppLogLevel.Warning),
            DataAccessException d => (
                d.IsConnectionFailure || d.IsTransient ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.InternalServerError,
                d.ErrorCode,
                d.Message,
                AppLogLevel.Error),
            ArgumentException a => ((int)HttpStatusCode.BadRequest, ErrorCodes.InvalidInput, a.Message, AppLogLevel.Warning),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                ErrorCodes.UnhandledSystemError,
                "Hệ thống đang gặp sự cố tạm thời. Vui lòng thử lại sau hoặc cung cấp mã tra cứu cho bộ phận hỗ trợ.",
                AppLogLevel.Critical)
        };
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var headerId = context.Request.Headers["X-Correlation-Id"].ToString();
        if (!string.IsNullOrWhiteSpace(headerId))
        {
            return headerId.Trim();
        }

        return Activity.Current?.Id ?? context.TraceIdentifier;
    }

    private static bool IsApiRequest(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var accept = context.Request.Headers["Accept"].ToString();
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}

