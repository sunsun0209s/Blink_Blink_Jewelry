using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Orders;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể HoaDon (Hóa đơn / Đơn đặt hàng).
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public interface IHoaDonRepository
{
    /// <summary>
    /// Lưu hóa đơn / đơn hàng mới vào cơ sở dữ liệu.
    /// Khớp phương thức: + LuuHoaDon(hd: HoaDon): void trong Diagram.
    /// </summary>
    /// <param name="hd">Thực thể hóa đơn cần lưu</param>
    void LuuHoaDon(HoaDon hd);

    /// <summary>
    /// Tìm kiếm hóa đơn theo mã định danh hóa đơn.
    /// Khớp phương thức: + TimTheoMa(maHoaDon: string): HoaDon trong Diagram.
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn</param>
    /// <returns>Thực thể hóa đơn hoặc null nếu không tìm thấy</returns>
    HoaDon? TimTheoMa(string maHoaDon);

    /// <summary>
    /// Lấy danh sách lịch sử các đơn hàng của một khách hàng cụ thể.
    /// Khớp phương thức: + LayTheoKhachHang(maKH: string): List&lt;HoaDon&gt; trong Diagram.
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <returns>Danh sách các hóa đơn của khách hàng</returns>
    List<HoaDon> LayTheoKhachHang(string maKH);

    /// <summary>
    /// Lấy toàn bộ danh sách đơn đặt hàng trong hệ thống (dành cho Admin quản lý bán hàng).
    /// Khớp phương thức: + LayDanhSachDonHang(): List&lt;HoaDon&gt; trong Diagram.
    /// </summary>
    /// <returns>Danh sách tất cả hóa đơn</returns>
    List<HoaDon> LayDanhSachDonHang();

    /// <summary>
    /// Cập nhật thông tin và trạng thái hóa đơn vào cơ sở dữ liệu.
    /// Khớp phương thức: + CapNhatHoaDon(hd: HoaDon): void trong Diagram.
    /// </summary>
    /// <param name="hd">Thực thể hóa đơn cần cập nhật</param>
    void CapNhatHoaDon(HoaDon hd);

    // Mở rộng Task bất đồng bộ chuẩn Clean Architecture .NET 8
    Task LuuHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default);
    Task<HoaDon?> TimTheoMaAsync(string maHoaDon, CancellationToken cancellationToken = default);
    Task<List<HoaDon>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default);
    Task<List<HoaDon>> LayDanhSachDonHangAsync(CancellationToken cancellationToken = default);
    Task CapNhatHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default);
}
