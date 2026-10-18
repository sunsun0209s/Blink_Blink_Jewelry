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
}
