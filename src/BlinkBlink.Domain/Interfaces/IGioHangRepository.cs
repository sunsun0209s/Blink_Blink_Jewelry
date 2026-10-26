using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Cart;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể GioHang (Giỏ hàng của khách hàng).
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public interface IGioHangRepository
{
    /// <summary>
    /// Lấy thông tin giỏ hàng của khách hàng theo mã khách hàng.
    /// Khớp phương thức: + LayGioHang(maKH: string): GioHang trong Diagram.
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <returns>Thực thể giỏ hàng kèm các mặt hàng đã chọn</returns>
    GioHang? LayGioHang(string maKH);

    /// <summary>
    /// Cập nhật thông tin giỏ hàng vào cơ sở dữ liệu.
    /// Khớp phương thức: + CapNhatGioHang(gh: GioHang): void trong Diagram.
    /// </summary>
    /// <param name="gh">Thực thể giỏ hàng cần cập nhật</param>
    void CapNhatGioHang(GioHang gh);

    // Mở rộng Task bất đồng bộ chuẩn Clean Architecture .NET 8
    Task<GioHang?> LayGioHangAsync(string maKH, CancellationToken cancellationToken = default);
    Task CapNhatGioHangAsync(GioHang gh, CancellationToken cancellationToken = default);
    Task LuuGioHangAsync(GioHang gh, CancellationToken cancellationToken = default);
}
