using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể DanhMuc.
/// Khớp 100% với Class Diagram.
/// </summary>
public interface IDanhMucRepository
{
    /// <summary>
    /// Lưu thông tin danh mục vào CSDL.
    /// </summary>
    /// <param name="dm">Thực thể danh mục</param>
    void LuuDanhMuc(DanhMuc dm);

    /// <summary>
    /// Tìm danh mục theo mã.
    /// </summary>
    DanhMuc? TimTheoId(string maDanhMuc);

    /// <summary>
    /// Lấy toàn bộ danh sách danh mục trong hệ thống.
    /// </summary>
    List<DanhMuc> LayTatCa();

    /// <summary>
    /// Xóa một danh mục khỏi hệ thống. Toàn bộ sản phẩm thuộc danh mục sẽ được chuyển sang 'Chưa phân loại' (DM00).
    /// </summary>
    /// <param name="maDanhMuc">Mã danh mục cần xóa</param>
    void XoaDanhMuc(string maDanhMuc);

    // Mở rộng bất đồng bộ chuẩn .NET 8
    Task LuuDanhMucAsync(DanhMuc dm, CancellationToken cancellationToken = default);
    Task<DanhMuc?> TimTheoIdAsync(string maDanhMuc, CancellationToken cancellationToken = default);
    Task<List<DanhMuc>> LayTatCaAsync(CancellationToken cancellationToken = default);
    Task XoaDanhMucAsync(string maDanhMuc, CancellationToken cancellationToken = default);
}
