using System.Collections.Generic;
using BlinkBlink.Domain.Entities.Customers;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện kho dữ liệu truy xuất và quản lý hồ sơ Khách hàng.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - IKhachHangRepository
/// </summary>
public interface IKhachHangRepository
{
    /// <summary>
    /// Lưu mới hồ sơ khách hàng vào cơ sở dữ liệu
    /// </summary>
    /// <param name="kh">Thực thể khách hàng cần lưu</param>
    void LuuKhachHang(KhachHang kh);

    /// <summary>
    /// Tìm kiếm khách hàng theo mã khách hàng
    /// </summary>
    /// <param name="maKH">Mã khách hàng (ví dụ: KH001)</param>
    /// <returns>Thực thể KhachHang nếu tìm thấy; ngược lại null</returns>
    KhachHang? TimTheoMa(string maKH);

    /// <summary>
    /// Tìm kiếm khách hàng theo số điện thoại
    /// </summary>
    /// <param name="soDienThoai">Số điện thoại khách hàng</param>
    /// <returns>Thực thể KhachHang nếu tìm thấy; ngược lại null</returns>
    KhachHang? TimTheoSoDienThoai(string soDienThoai);

    /// <summary>
    /// Lấy toàn bộ danh sách khách hàng trong hệ thống phục vụ quản lý Admin
    /// </summary>
    /// <returns>Danh sách thực thể khách hàng</returns>
    List<KhachHang> LayTatCa();

    /// <summary>
    /// Kiểm tra xem khách hàng có thể xóa khỏi CSDL hay không (chưa từng có bất kỳ lịch sử mua hàng nào).
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <returns>True nếu chưa từng phát sinh đơn hàng</returns>
    bool KiemTraCoTheXoa(string maKH);

    /// <summary>
    /// Xóa vĩnh viễn khách hàng và tài khoản người dùng khỏi hệ thống nếu chưa từng mua hàng
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <returns>True nếu xóa thành công</returns>
    bool XoaKhachHang(string maKH);
}
