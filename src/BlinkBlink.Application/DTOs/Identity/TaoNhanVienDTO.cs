namespace BlinkBlink.Application.DTOs.Identity;

/// <summary>
/// DTO chứa thông tin tạo mới hồ sơ và tài khoản nhân viên từ Admin WPF Desktop.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - TaoNhanVienDTO
/// </summary>
public class TaoNhanVienDTO
{
    /// <summary>
    /// Họ và tên nhân viên
    /// </summary>
    public string HoTen { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ
    /// </summary>
    public string SoDienThoai { get; set; } = string.Empty;

    /// <summary>
    /// Tên đăng nhập được cấp cho nhân viên
    /// </summary>
    public string TenDangNhap { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu khởi tạo
    /// </summary>
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Mức lương cơ bản thỏa thuận
    /// </summary>
    public decimal LuongCoBan { get; set; }

    /// <summary>
    /// Mã vai trò được chỉ định (ADMIN, SALE, WAREHOUSE...)
    /// </summary>
    public string MaVaiTro { get; set; } = string.Empty;
}
