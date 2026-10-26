using System;

namespace BlinkBlink.Application.DTOs.Identity;

/// <summary>
/// DTO chứa dữ liệu cập nhật thông tin cá nhân khách hàng.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - CapNhatKhachHangDTO
/// </summary>
public class CapNhatKhachHangDTO
{
    /// <summary>
    /// Mã định danh duy nhất của khách hàng trong hệ thống
    /// </summary>
    public string MaKhachHang { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên khách hàng cần cập nhật
    /// </summary>
    public string HoTen { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ chính
    /// </summary>
    public string SoDienThoai { get; set; } = string.Empty;

    /// <summary>
    /// Ngày tháng năm sinh của khách hàng
    /// </summary>
    public DateTime NgaySinh { get; set; }
}
