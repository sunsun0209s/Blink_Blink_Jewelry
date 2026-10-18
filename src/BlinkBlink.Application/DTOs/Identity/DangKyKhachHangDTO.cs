using System;

namespace BlinkBlink.Application.DTOs.Identity;

/// <summary>
/// DTO chứa thông tin yêu cầu đăng ký tài khoản khách hàng mới qua Web MVC.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - DangKyKhachHangDTO
/// </summary>
public class DangKyKhachHangDTO
{
    /// <summary>
    /// Họ tên khách hàng
    /// </summary>
    public string HoTen { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ
    /// </summary>
    public string SoDienThoai { get; set; } = string.Empty;

    /// <summary>
    /// Tên đăng nhập mong muốn
    /// </summary>
    public string TenDangNhap { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đăng nhập
    /// </summary>
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Ngày tháng năm sinh của khách hàng
    /// </summary>
    public DateTime NgaySinh { get; set; }
}
