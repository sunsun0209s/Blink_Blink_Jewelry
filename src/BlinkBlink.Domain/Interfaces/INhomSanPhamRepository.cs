using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể Nhóm sản phẩm (NhomSanPham).
/// </summary>
public interface INhomSanPhamRepository
{
    /// <summary>
    /// Lưu nhóm sản phẩm mới vào CSDL.
    /// </summary>
    void LuuNhom(NhomSanPham nhom);

    /// <summary>
    /// Cập nhật thông tin nhóm sản phẩm.
    /// </summary>
    void CapNhatNhom(NhomSanPham nhom);

    /// <summary>
    /// Xóa nhóm sản phẩm theo mã.
    /// </summary>
    bool XoaNhom(string maNhom);

    /// <summary>
    /// Tìm nhóm sản phẩm theo mã.
    /// </summary>
    NhomSanPham? TimTheoId(string maNhom);

    /// <summary>
    /// Lấy toàn bộ danh sách nhóm sản phẩm.
    /// </summary>
    List<NhomSanPham> LayTatCa();

    // Các phương thức mở rộng bất đồng bộ chuẩn .NET 8 LTS
    Task LuuNhomAsync(NhomSanPham nhom, CancellationToken cancellationToken = default);
    Task CapNhatNhomAsync(NhomSanPham nhom, CancellationToken cancellationToken = default);
    Task<bool> XoaNhomAsync(string maNhom, CancellationToken cancellationToken = default);
    Task<NhomSanPham?> TimTheoIdAsync(string maNhom, CancellationToken cancellationToken = default);
    Task<List<NhomSanPham>> LayTatCaAsync(CancellationToken cancellationToken = default);
}

