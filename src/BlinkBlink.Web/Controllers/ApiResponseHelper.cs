using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Tiện ích tạo phản hồi lỗi chuẩn hóa có đính kèm mã lỗi và mã truy vết cho các API Controller.
/// </summary>
internal static class ApiResponseHelper
{
    public static ObjectResult Fail(ControllerBase controller, int statusCode, string errorCode, string message, IAppLogger? logger = null, string? source = null)
    {
        var traceId = controller.HttpContext?.TraceIdentifier ?? string.Empty;
        logger?.LogWarning(
            source ?? controller.GetType().Name,
            message,
            errorCode,
            context: new AppLogContext
            {
                Channel = logger.DefaultContext.Channel,
                EnvironmentName = logger.DefaultContext.EnvironmentName,
                DataMode = logger.DefaultContext.DataMode,
                CorrelationId = traceId
            });

        return controller.StatusCode(statusCode, new
        {
            Success = false,
            Code = errorCode,
            Message = message,
            TraceId = traceId
        });
    }
}

