using System.Collections.Generic;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện kho dữ liệu truy xuất và quản lý danh mục Vai trò phân quyền.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - IVaiTroRepository
/// </summary>
public interface IVaiTroRepository
{
    /// <summary>
    /// Tìm kiếm vai trò theo mã vai trò
    /// </summary>
    /// <param name="maVaiTro">Mã vai trò (ví dụ: ADMIN, SALE)</param>
    /// <returns>Thực thể VaiTro nếu tìm thấy; ngược lại null</returns>
    VaiTro? TimTheoMa(string maVaiTro);

    /// <summary>
    /// Lấy toàn bộ danh sách các vai trò hiện có trong hệ thống
    /// </summary>
    /// <returns>Danh sách các đối tượng VaiTro</returns>
    List<VaiTro> LayTatCa();

    /// <summary>
    /// Lưu mới hoặc cập nhật vai trò vào cơ sở dữ liệu
    /// </summary>
    /// <param name="vt">Thực thể vai trò cần lưu</param>
    void LuuVaiTro(VaiTro vt);
}
