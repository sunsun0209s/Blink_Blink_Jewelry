using System;
using System.Collections.Generic;

namespace BlinkBlink.Domain.Entities.Identity;

/// <summary>
/// Thực thể Vai trò định nghĩa nhóm quyền hạn của người dùng trong hệ thống (Admin, NhanVienBanHang, NhanVienKho, KhachHang...).
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - VaiTro
/// </summary>
public class VaiTro
{
    /// <summary>
    /// Mã vai trò (Khoá chính, ví dụ: VT01, ADMIN, SALE)
    /// </summary>
    public string MaVaiTro { get; set; } = string.Empty;

    /// <summary>
    /// Tên gọi hiển thị của vai trò
    /// </summary>
    public string TenVaiTro { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết phạm vi quyền hạn của vai trò
    /// </summary>
    public string MoTa { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách mã quyền được gán cho vai trò này
    /// </summary>
    public virtual ICollection<string> QuyenHans { get; set; } = new List<string>();

    /// <summary>
    /// Danh sách người dùng được phân vai trò này
    /// </summary>
    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();

    public VaiTro() { }

    /// <summary>
    /// Khởi tạo đối tượng VaiTro với các thông tin cơ bản
    /// </summary>
    /// <param name="maVaiTro">Mã vai trò định danh duy nhất</param>
    /// <param name="tenVaiTro">Tên vai trò hiển thị</param>
    /// <param name="moTa">Mô tả quyền hạn</param>
    public VaiTro(string maVaiTro, string tenVaiTro, string moTa = "")
    {
        MaVaiTro = maVaiTro;
        TenVaiTro = tenVaiTro;
        MoTa = moTa;
    }

    /// <summary>
    /// Gán thêm một mã quyền hạn mới vào vai trò nếu chưa tồn tại
    /// </summary>
    /// <param name="maQuyen">Mã quyền hạn cần thêm (ví dụ: Q_BANHANG, Q_NHAPKHO)</param>
    public void ThemQuyen(string maQuyen)
    {
        if (string.IsNullOrWhiteSpace(maQuyen))
        {
            return;
        }

        var normalizedCode = maQuyen.Trim().ToUpperInvariant();
        if (!QuyenHans.Contains(normalizedCode))
        {
            QuyenHans.Add(normalizedCode);
        }
    }

    /// <summary>
    /// Thu hồi một quyền hạn khỏi vai trò
    /// </summary>
    /// <param name="maQuyen">Mã quyền hạn cần thu hồi</param>
    public void XoaQuyen(string maQuyen)
    {
        if (string.IsNullOrWhiteSpace(maQuyen))
        {
            return;
        }

        var normalizedCode = maQuyen.Trim().ToUpperInvariant();
        QuyenHans.Remove(normalizedCode);
    }
}
