using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.DTOs.Inventory;
using BlinkBlink.Application.Services.Inventory;
using BlinkBlink.Domain.Exceptions;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các tác vụ liên quan đến nhập kho sản phẩm.
/// Khớp 100% với Class Diagram (Interface Adapters / Presentation Layer).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhieuNhapKhoController : ControllerBase
{
    private readonly XacNhanNhapKhoUseCase _usecase;
    private readonly IAppLogger _logger;

    public PhieuNhapKhoController(XacNhanNhapKhoUseCase usecase, IAppLogger? logger = null)
    {
        _usecase = usecase ?? throw new ArgumentNullException(nameof(usecase));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Thực hiện lập và xác nhận nhập kho sản phẩm.
    /// Khớp hàm: + NhapKho(request: NhapKhoDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">DTO phiếu nhập và các mặt hàng</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu dữ liệu lỗi</returns>
    [HttpPost("nhap-kho")]
    public async Task<IActionResult> NhapKho([FromBody] NhapKhoDTO request, CancellationToken cancellationToken)
    {
        if (request == null || request.DanhSachSanPham == null || request.DanhSachSanPham.Count == 0)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Danh sách sản phẩm nhập kho không được để trống.", _logger);
        }

        await _usecase.ExecuteAsync(request, cancellationToken);
        _logger.LogInfo(nameof(PhieuNhapKhoController), $"Lập phiếu nhập kho thành công từ '{request.NhaCungCap}'.");
        return Ok(new { Success = true, Message = "Xác nhận nhập kho thành công." });
    }
}
