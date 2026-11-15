using System;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Bộ điều khiển RESTful API quản lý hồ sơ và tài khoản nhân viên.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NhanVienController
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NhanVienController : ControllerBase
{
    private readonly QuanLyNhanVienUseCase _quanLyNhanVienUseCase;
    private readonly IAppLogger _logger;

    public NhanVienController(
        QuanLyNhanVienUseCase quanLyNhanVienUseCase,
        IAppLogger? logger = null)
    {
        _quanLyNhanVienUseCase = quanLyNhanVienUseCase ?? throw new ArgumentNullException(nameof(quanLyNhanVienUseCase));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Tạo mới tài khoản và hồ sơ nhân viên trong hệ thống.
    /// Khớp phương thức: + HttpPost TaoNhanVien(request: TaoNhanVienDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin nhân viên mới</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu thất bại</returns>
    [HttpPost("tao-moi")]
    public IActionResult TaoNhanVien([FromBody] TaoNhanVienDTO request)
    {
        if (request == null)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu nhân viên không được để trống.", _logger);
        }

        if (string.IsNullOrWhiteSpace(request.HoTen) ||
            string.IsNullOrWhiteSpace(request.TenDangNhap) ||
            string.IsNullOrWhiteSpace(request.MatKhau) ||
            string.IsNullOrWhiteSpace(request.MaVaiTro))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Vui lòng nhập đầy đủ các trường thông tin bắt buộc.", _logger);
        }

        if (request.MatKhau.Trim().Length < 6)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mật khẩu nhân viên phải có độ dài từ 6 ký tự trở lên.", _logger);
        }

        try
        {
            var result = _quanLyNhanVienUseCase.TaoNhanVien(request);
            if (!result)
            {
                return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Không thể tạo tài khoản nhân viên. Vai trò không hợp lệ hoặc thông tin không đúng.", _logger);
            }

            _logger.LogInfo(nameof(NhanVienController), $"Tạo tài khoản nhân viên '{request.TenDangNhap}' thành công.");

            return Ok(new
            {
                Success = true,
                Message = "Tạo tài khoản nhân viên mới thành công.",
                Data = new
                {
                    HoTen = request.HoTen,
                    TenDangNhap = request.TenDangNhap,
                    MaVaiTro = request.MaVaiTro
                }
            });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return ApiResponseHelper.Fail(this, 409, ErrorCodes.DuplicateUsername, "Tên đăng nhập này đã tồn tại trong hệ thống.", _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(NhanVienController), "Lỗi máy chủ khi tạo nhân viên.", ErrorCodes.UnhandledSystemError, ex);
            return ApiResponseHelper.Fail(this, 500, ErrorCodes.UnhandledSystemError, "Lỗi máy chủ khi tạo nhân viên.", _logger);
        }
    }

    /// <summary>
    /// Khóa hoặc mở khóa tài khoản nhân viên.
    /// Khớp phương thức: + HttpPut KhoaTaiKhoan(maNv: string): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="maNv">Mã nhân viên</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu thất bại</returns>
    [HttpPut("khoa/{maNv}")]
    public IActionResult KhoaTaiKhoan(string maNv)
    {
        if (string.IsNullOrWhiteSpace(maNv))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã nhân viên không hợp lệ.", _logger);
        }

        var result = _quanLyNhanVienUseCase.KhoaTaiKhoanNhanVien(maNv);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.ResourceNotFound, "Không tìm thấy nhân viên hoặc không thể thay đổi trạng thái tài khoản.", _logger);
        }

        _logger.LogInfo(nameof(NhanVienController), $"Cập nhật trạng thái khóa tài khoản cho nhân viên '{maNv}' thành công.");

        return Ok(new
        {
            Success = true,
            Message = $"Thay đổi trạng thái tài khoản nhân viên '{maNv}' thành công."
        });
    }

    /// <summary>
    /// Đặt lại mật khẩu mặc định cho nhân viên.
    /// Khớp phương thức: + HttpPut ResetMatKhau(maNv: string): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="maNv">Mã nhân viên</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu thất bại</returns>
    [HttpPut("reset-mat-khau/{maNv}")]
    public IActionResult ResetMatKhau(string maNv)
    {
        if (string.IsNullOrWhiteSpace(maNv))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã nhân viên không hợp lệ.", _logger);
        }

        var result = _quanLyNhanVienUseCase.ResetMatKhau(maNv);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.ResourceNotFound, "Không tìm thấy nhân viên cần đặt lại mật khẩu.", _logger);
        }

        _logger.LogInfo(nameof(NhanVienController), $"Đặt lại mật khẩu cho nhân viên '{maNv}' thành công.");

        return Ok(new
        {
            Success = true,
            Message = $"Đặt lại mật khẩu mặc định cho nhân viên '{maNv}' thành công."
        });
    }
}
