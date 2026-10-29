using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể SanPhamTrangSuc.
/// Khớp 100% với Class Diagram (cung cấp cả chữ ký đồng bộ và mở rộng bất đồng bộ).
/// </summary>
public interface ISanPhamRepository
{
    /// <summary>
    /// Lưu thông tin sản phẩm trang sức vào CSDL.
    /// </summary>
    /// <param name="sp">Thực thể sản phẩm trang sức</param>
    void LuuSanPham(SanPhamTrangSuc sp);

    /// <summary>
    /// Tìm kiếm thông tin sản phẩm theo mã sản phẩm.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm</param>
    /// <returns>Thực thể sản phẩm hoặc null</returns>
    SanPhamTrangSuc? TimTheoId(string maSanPham);

    /// <summary>
    /// Lấy toàn bộ danh sách sản phẩm trang sức trong hệ thống.
    /// </summary>
    List<SanPhamTrangSuc> LayTatCa();

    // Mở rộng Task bất đồng bộ chuẩn Clean Architecture .NET 8
    Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken cancellationToken = default);
    Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken cancellationToken = default);
    Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken cancellationToken = default);
}
