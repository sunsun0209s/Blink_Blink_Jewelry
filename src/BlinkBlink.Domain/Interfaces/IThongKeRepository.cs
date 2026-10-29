using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Entities.Orders;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository chuyên trách trích xuất dữ liệu tổng hợp phục vụ Báo cáo & Thống kê.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public interface IThongKeRepository
{
    /// <summary>
    /// Truy vấn danh sách các hóa đơn bán hàng trong khoảng thời gian để tính toán doanh thu (đồng bộ).
    /// Khớp phương thức: + TruyVanDoanhThu(tuNgay: DateTime, denNgay: DateTime): List&lt;HoaDon&gt; trong Diagram.
    /// </summary>
    /// <param name="tuNgay">Mốc thời gian bắt đầu</param>
    /// <param name="denNgay">Mốc thời gian kết thúc</param>
    /// <returns>Danh sách hóa đơn kèm chi tiết sản phẩm</returns>
    List<HoaDon> TruyVanDoanhThu(DateTime tuNgay, DateTime denNgay);

    /// <summary>
    /// Truy vấn danh sách các mặt hàng trang sức đang sẵn sàng tồn trong kho (đồng bộ).
    /// Khớp phương thức: + TruyVanSanPhamTonKho(): List&lt;SanPhamTrangSuc&gt; trong Diagram.
    /// </summary>
    /// <returns>Danh sách sản phẩm trong kho</returns>
    List<SanPhamTrangSuc> TruyVanSanPhamTonKho();

    /// <summary>
    /// Truy vấn danh sách các dòng kiểm kê có phát sinh chênh lệch hao hụt hoặc dư thừa (đồng bộ).
    /// Khớp phương thức: + TruyVanChenhLechKiemKe(): List&lt;ChiTietKiemKe&gt; trong Diagram.
    /// </summary>
    /// <returns>Danh sách chi tiết kiểm kê phát sinh chênh lệch</returns>
    List<ChiTietKiemKe> TruyVanChenhLechKiemKe();

    /// <summary>
    /// Truy vấn danh sách sản phẩm trang sức có doanh thu thấp hoặc chưa phát sinh đơn hàng (đồng bộ).
    /// Khớp phương thức: + TruyVanSanPhamDoanhThuThap(): List&lt;SanPhamTrangSuc&gt; trong Diagram.
    /// </summary>
    /// <returns>Danh sách sản phẩm doanh thu thấp</returns>
    List<SanPhamTrangSuc> TruyVanSanPhamDoanhThuThap();

    // Mở rộng Task bất đồng bộ chuẩn Clean Architecture .NET 8
    Task<List<HoaDon>> TruyVanDoanhThuAsync(DateTime tuNgay, DateTime denNgay, CancellationToken cancellationToken = default);
    Task<List<SanPhamTrangSuc>> TruyVanSanPhamTonKhoAsync(CancellationToken cancellationToken = default);
    Task<List<ChiTietKiemKe>> TruyVanChenhLechKiemKeAsync(CancellationToken cancellationToken = default);
    Task<List<SanPhamTrangSuc>> TruyVanSanPhamDoanhThuThapAsync(CancellationToken cancellationToken = default);
}
