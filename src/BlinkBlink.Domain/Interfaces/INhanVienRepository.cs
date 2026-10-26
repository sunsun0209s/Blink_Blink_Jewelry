using System.Collections.Generic;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện kho dữ liệu truy xuất và quản lý hồ sơ Nhân viên.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - INhanVienRepository
/// </summary>
public interface INhanVienRepository
{
    /// <summary>
    /// Lưu mới hồ sơ nhân viên vào cơ sở dữ liệu
    /// </summary>
    /// <param name="nv">Thực thể nhân viên cần lưu</param>
    void LuuNhanVien(NhanVien nv);

    /// <summary>
    /// Tìm kiếm nhân viên theo mã nhân viên định danh nội bộ
    /// </summary>
    /// <param name="maNV">Mã nhân viên (ví dụ: NV001)</param>
    /// <returns>Thực thể NhanVien nếu tìm thấy; ngược lại null</returns>
    NhanVien? TimTheoMa(string maNV);

    /// <summary>
    /// Lấy toàn bộ danh sách nhân viên trong hệ thống phục vụ quản lý nhân sự
    /// </summary>
    /// <returns>Danh sách thực thể nhân viên</returns>
    List<NhanVien> LayTatCa();

    /// <summary>
    /// Cập nhật thông tin hồ sơ nhân viên và vai trò
    /// </summary>
    /// <param name="nv">Thực thể nhân viên với thông tin cập nhật</param>
    void CapNhatNhanVien(NhanVien nv);

    /// <summary>
    /// Kiểm tra xem nhân viên có thể xóa khỏi CSDL hay không (chưa từng tạo phiếu nhập kho, hóa đơn hoặc xử lý đơn).
    /// </summary>
    /// <param name="maNV">Mã định danh nhân viên</param>
    /// <returns>True nếu chưa từng có phát sinh nghiệp vụ liên quan</returns>
    bool KiemTraCoTheXoa(string maNV);

    /// <summary>
    /// Xóa vĩnh viễn nhân viên và tài khoản người dùng khỏi hệ thống nếu thỏa điều kiện
    /// </summary>
    /// <param name="maNV">Mã định danh nhân viên</param>
    /// <returns>True nếu xóa thành công</returns>
    bool XoaNhanVien(string maNV);
}
