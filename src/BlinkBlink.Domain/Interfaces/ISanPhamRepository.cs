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
    bool LuuSanPham(SanPhamTrangSuc sp);

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

    /// <summary>
    /// Lấy danh sách sản phẩm trang sức thuộc về một nhóm.
    /// </summary>
    List<SanPhamTrangSuc> LayTheoNhom(string maNhom);

    /// <summary>
    /// Kiểm tra xem sản phẩm đã từng phát sinh giao dịch (nhập kho, bán hàng, kiểm kê, bảo hành) hay chưa.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm</param>
    /// <param name="lyDo">Lý do chi tiết nếu đã phát sinh giao dịch</param>
    /// <returns>True nếu đã có giao dịch, False nếu chưa từng có giao dịch</returns>
    bool KiemTraPhatSinhGiaoDich(string maSanPham, out string lyDo);

    /// <summary>
    /// Xóa hoàn toàn sản phẩm khỏi hệ thống (chỉ áp dụng khi sản phẩm chưa từng phát sinh giao dịch).
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm cần xóa</param>
    bool XoaSanPham(string maSanPham);

    // Mở rộng Task bất đồng bộ chuẩn Clean Architecture .NET 8
    Task<bool> LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken cancellationToken = default);
    Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken cancellationToken = default);
    Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken cancellationToken = default);
    Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken cancellationToken = default);
    Task<(bool CoGiaoDich, string LyDo)> KiemTraPhatSinhGiaoDichAsync(string maSanPham, CancellationToken cancellationToken = default);
    Task<bool> XoaSanPhamAsync(string maSanPham, CancellationToken cancellationToken = default);
}
