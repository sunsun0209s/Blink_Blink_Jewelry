using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.DTOs.Orders;
using BlinkBlink.Application.Services.Orders;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các tác vụ nghiệp vụ Quản lý bán hàng và Đơn hàng.
/// Cung cấp Restful API cho giao diện Web bán lẻ trang sức và WinForms Admin.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BanHangController : ControllerBase
{
    private readonly DatHangUseCase _datHangUseCase;
    private readonly XuLyDonHangUseCase _xuLyDonHangUseCase;
    private readonly IHoaDonRepository _hoaDonRepo;
    private readonly IKhachHangRepository _khachHangRepo;
    private readonly IAppLogger _logger;

    public BanHangController(
        DatHangUseCase datHangUseCase,
        XuLyDonHangUseCase xuLyDonHangUseCase,
        IHoaDonRepository hoaDonRepo,
        IKhachHangRepository khachHangRepo,
        IAppLogger? logger = null)
    {
        _datHangUseCase = datHangUseCase ?? throw new ArgumentNullException(nameof(datHangUseCase));
        _xuLyDonHangUseCase = xuLyDonHangUseCase ?? throw new ArgumentNullException(nameof(xuLyDonHangUseCase));
        _hoaDonRepo = hoaDonRepo ?? throw new ArgumentNullException(nameof(hoaDonRepo));
        _khachHangRepo = khachHangRepo ?? throw new ArgumentNullException(nameof(khachHangRepo));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Đặt hàng trực tuyến từ giỏ hàng.
    /// Khớp phương thức: + HttpPost DatHang(request: DatHangDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin đặt hàng</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu dữ liệu không hợp lệ</returns>
    [HttpPost("dat-hang")]
    public async Task<IActionResult> DatHang([FromBody] DatHangDTO request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu đặt hàng không được để trống.", _logger);
        }

        // Kiểm tra quyền truy cập: Chặn khách hàng bị quản trị viên khóa tài khoản
        var khach = _khachHangRepo.TimTheoMa(request.MaKhachHang);
        if (khach != null && !khach.TrangThai)
        {
            return ApiResponseHelper.Fail(this, 403, ErrorCodes.AccountLocked, "Tài khoản của quý khách hiện đang bị tạm khóa. Vui lòng liên hệ bộ phận hỗ trợ khách hàng Blink Blink.", _logger);
        }

        var result = await _datHangUseCase.ExecuteWithResultAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ApiResponseHelper.Fail(this, 400, result.ErrorCode ?? ErrorCodes.InvalidInput, result.Message, _logger);
        }

        var donMoi = (await _hoaDonRepo.LayTheoKhachHangAsync(request.MaKhachHang, cancellationToken)).FirstOrDefault();
        _logger.LogInfo(nameof(BanHangController), $"Khách hàng '{request.MaKhachHang}' đặt hàng trực tuyến thành công (Mã đơn: {result.Data}).");

        return Ok(new { 
            Success = true, 
            Message = "Đặt hàng thành công. Đơn hàng đang chờ nhân viên duyệt.",
            Data = donMoi
        });
    }

    /// <summary>
    /// Tra cứu lịch sử đơn hàng của một khách hàng cụ thể.
    /// Khớp phương thức: + HttpGet LayDonTheoKhachHang(maKH: string): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách đơn hàng của khách</returns>
    [HttpGet("khach-hang/{maKH}")]
    public async Task<IActionResult> LayDonTheoKhachHang(string maKH, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã khách hàng không hợp lệ.", _logger);
        }

        var danhSach = await _hoaDonRepo.LayTheoKhachHangAsync(maKH, cancellationToken);
        return Ok(new { Success = true, Data = danhSach });
    }

    /// <summary>
    /// Lấy toàn bộ danh sách đơn đặt hàng trong hệ thống dành cho quản trị viên và nhân viên.
    /// Khớp phương thức: + HttpGet LayDanhSachDonHang(): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách tất cả hóa đơn</returns>
    [HttpGet("danh-sach")]
    public async Task<IActionResult> LayDanhSachDonHang(CancellationToken cancellationToken)
    {
        var danhSach = await _hoaDonRepo.LayDanhSachDonHangAsync(cancellationToken);
        return Ok(new { Success = true, Data = danhSach });
    }

    /// <summary>
    /// Cập nhật tiến độ hoặc trạng thái đơn hàng.
    /// Khớp phương thức: + HttpPut CapNhatTrangThai(request: CapNhatDonHangDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin trạng thái mới và mã nhân viên</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu thất bại</returns>
    [HttpPut("trang-thai")]
    public async Task<IActionResult> CapNhatTrangThai([FromBody] CapNhatDonHangDTO request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaHoaDon))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu cập nhật đơn hàng không hợp lệ.", _logger);
        }

        var result = await _xuLyDonHangUseCase.CapNhatTrangThaiAsync(request.MaHoaDon, request.TrangThaiMoi, request.MaNhanVien, cancellationToken);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidStateTransition, "Không thể cập nhật trạng thái đơn hàng. Kiểm tra mã đơn hoặc trạng thái chuyển đổi.", _logger);
        }

        return Ok(new { Success = true, Message = "Cập nhật trạng thái đơn hàng thành công." });
    }

    /// <summary>
    /// Hủy đơn hàng và hoàn lại trạng thái sản phẩm vào kho.
    /// Khớp phương thức: + HttpPut HuyDonHang(maHoaDon: string): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn cần hủy</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu không thể hủy</returns>
    [HttpPut("{maHoaDon}/huy")]
    public async Task<IActionResult> HuyDonHang(string maHoaDon, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã hóa đơn không hợp lệ.", _logger);
        }

        var result = await _xuLyDonHangUseCase.HuyDonHangAsync(maHoaDon, cancellationToken);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidStateTransition, "Không thể hủy đơn hàng. Đơn có thể đã hoàn tất hoặc không tồn tại.", _logger);
        }

        return Ok(new { Success = true, Message = "Hủy đơn hàng thành công." });
    }
}
