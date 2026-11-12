using System;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý xác thực đăng nhập người dùng cho Web và POS.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - XacThucController
/// Khớp 100% với Class Diagram kỹ thuật.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class XacThucController : ControllerBase
{
    private readonly DangNhapUseCase _dangNhapUseCase;
    private readonly INguoiDungRepository _nguoiDungRepo;
    private readonly IKhachHangRepository _khachHangRepo;
    private readonly IAppLogger _logger;

    public XacThucController(
        DangNhapUseCase dangNhapUseCase,
        INguoiDungRepository nguoiDungRepo,
        IKhachHangRepository khachHangRepo,
        IAppLogger? logger = null)
    {
        _dangNhapUseCase = dangNhapUseCase ?? throw new ArgumentNullException(nameof(dangNhapUseCase));
        _nguoiDungRepo = nguoiDungRepo ?? throw new ArgumentNullException(nameof(nguoiDungRepo));
        _khachHangRepo = khachHangRepo ?? throw new ArgumentNullException(nameof(khachHangRepo));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Tiếp nhận và xử lý yêu cầu đăng nhập hệ thống.
    /// Khớp phương thức: + HttpPost DangNhap(request: DangNhapDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin đăng nhập gồm tên đăng nhập và mật khẩu</param>
    /// <returns>HTTP Status 200 kèm mã người dùng và thông tin khách hàng nếu thành công; 400 hoặc 401 nếu thất bại</returns>
    [HttpPost("dang-nhap")]
    public IActionResult DangNhap([FromBody] DangNhapDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.", _logger);
        }

        var userId = _dangNhapUseCase.Execute(request);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponseHelper.Fail(this, 401, ErrorCodes.InvalidCredentials, "Tên đăng nhập hoặc mật khẩu không chính xác, hoặc tài khoản đã bị tạm khóa.", _logger);
        }

        // Lấy thông tin tài khoản người dùng và hồ sơ khách hàng
        var user = _nguoiDungRepo.TimTheoTenDangNhap(request.TenDangNhap.Trim());
        var khachHang = _khachHangRepo.TimTheoMa(userId);

        _logger.LogInfo(nameof(XacThucController), $"Người dùng '{request.TenDangNhap.Trim()}' đăng nhập thành công.");

        return Ok(new
        {
            Success = true,
            Message = "Đăng nhập thành công.",
            Data = new
            {
                MaNguoiDung = user?.MaNguoiDung ?? userId,
                HoTen = user?.HoTen ?? request.TenDangNhap,
                TenDangNhap = user?.TenDangNhap ?? request.TenDangNhap,
                SoDienThoai = user?.SoDienThoai ?? string.Empty,
                MaVaiTro = user?.MaVaiTro ?? "KHACHHANG",
                MaKhachHang = khachHang?.MaKhachHang ?? user?.MaNguoiDung ?? userId
            }
        });
    }
}
