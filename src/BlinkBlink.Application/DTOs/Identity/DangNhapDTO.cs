namespace BlinkBlink.Application.DTOs.Identity;

/// <summary>
/// DTO chứa thông tin yêu cầu đăng nhập hệ thống.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - DangNhapDTO
/// </summary>
public class DangNhapDTO
{
    /// <summary>
    /// Tên tài khoản đăng nhập
    /// </summary>
    public string TenDangNhap { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu người dùng nhập vào
    /// </summary>
    public string MatKhau { get; set; } = string.Empty;
}
