namespace BlinkBlink.Application.DTOs.Identity;

/// <summary>
/// Đối tượng truyền dữ liệu yêu cầu đổi mật khẩu tài khoản người dùng.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml
/// </summary>
public class DoiMatKhauDTO
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauHienTai { get; set; } = string.Empty;
    public string MatKhauMoi { get; set; } = string.Empty;
}
