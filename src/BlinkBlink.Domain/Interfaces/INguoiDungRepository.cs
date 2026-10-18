using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện kho dữ liệu truy xuất và quản lý thông tin tài khoản Người dùng chung.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - INguoiDungRepository
/// </summary>
public interface INguoiDungRepository
{
    /// <summary>
    /// Tìm kiếm thông tin người dùng theo tên đăng nhập (dùng cho xác thực tài khoản)
    /// </summary>
    /// <param name="tenDN">Tên đăng nhập hệ thống</param>
    /// <returns>Thực thể NguoiDung nếu tìm thấy; ngược lại null</returns>
    NguoiDung? TimTheoTenDangNhap(string tenDN);

    /// <summary>
    /// Cập nhật thông tin hoặc trạng thái tài khoản người dùng vào cơ sở dữ liệu
    /// </summary>
    /// <param name="nd">Thực thể người dùng với dữ liệu mới</param>
    void CapNhat(NguoiDung nd);
}
