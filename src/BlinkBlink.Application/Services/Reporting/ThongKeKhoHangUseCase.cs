using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Reporting;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Reporting;

/// <summary>
/// Trường hợp sử dụng Thống kê kho hàng (ThongKeKhoHangUseCase) dành cho Quản trị viên.
/// Thực hiện kết xuất báo cáo tồn kho, top sản phẩm bán chạy và danh sách sản phẩm doanh thu thấp.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public class ThongKeKhoHangUseCase
{
    private readonly IThongKeRepository _thongKeRepo;

    public ThongKeKhoHangUseCase(IThongKeRepository thongKeRepo)
    {
        _thongKeRepo = thongKeRepo ?? throw new ArgumentNullException(nameof(thongKeRepo));
    }

    /// <summary>
    /// Lấy báo cáo tồn kho hiện tại kèm theo chênh lệch kiểm kê (đồng bộ).
    /// Khớp phương thức: + LayBaoCaoTonKho(): List&lt;BaoCaoTonKhoDTO&gt; trong Diagram.
    /// </summary>
    /// <returns>Danh sách báo cáo tồn kho</returns>
    public List<BaoCaoTonKhoDTO> LayBaoCaoTonKho()
    {
        return LayBaoCaoTonKhoAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Lấy báo cáo tồn kho hiện tại kèm theo chênh lệch kiểm kê (bất đồng bộ).
    /// </summary>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>Danh sách báo cáo tồn kho</returns>
    public async Task<List<BaoCaoTonKhoDTO>> LayBaoCaoTonKhoAsync(CancellationToken cancellationToken = default)
    {
        var sanPhamTonKho = await _thongKeRepo.TruyVanSanPhamTonKhoAsync(cancellationToken);
        var danhSachKiemKe = await _thongKeRepo.TruyVanChenhLechKiemKeAsync(cancellationToken);

        // Tạo bản đồ tra cứu tổng chênh lệch kiểm kê theo từng mã sản phẩm
        var mapChenhLech = danhSachKiemKe?
            .GroupBy(k => k.MaSanPham)
            .ToDictionary(g => g.Key, g => g.Sum(k => k.ChenhLech))
            ?? new Dictionary<string, int>();

        var result = new List<BaoCaoTonKhoDTO>();

        if (sanPhamTonKho != null)
        {
            // Nhóm theo mã sản phẩm hoặc lấy danh sách từng dòng trang sức
            var nhomSanPham = sanPhamTonKho.GroupBy(s => s.MaSanPham);

            foreach (var group in nhomSanPham)
            {
                var dauTien = group.First();
                var soLuong = group.Count();
                var giaTri = group.Sum(s => s.GiaBan);

                mapChenhLech.TryGetValue(dauTien.MaSanPham, out var chenhLech);

                result.Add(new BaoCaoTonKhoDTO
                {
                    MaSanPham = dauTien.MaSanPham,
                    TenSanPham = dauTien.TenSanPham,
                    SoLuongTon = soLuong,
                    GiaTriTonKho = giaTri,
                    ChenhLechKiemKe = chenhLech
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Lấy danh sách Top sản phẩm bán chạy nhất theo doanh thu và số lượng (đồng bộ).
    /// Khớp phương thức: + LayTopBanChay(top: int): List&lt;TopSanPhamBanChayDTO&gt; trong Diagram.
    /// </summary>
    /// <param name="top">Số lượng sản phẩm cần lấy (ví dụ: top 5, top 10)</param>
    /// <returns>Danh sách top sản phẩm bán chạy</returns>
    public List<TopSanPhamBanChayDTO> LayTopBanChay(int top)
    {
        return LayTopBanChayAsync(top).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Lấy danh sách Top sản phẩm bán chạy nhất theo doanh thu và số lượng (bất đồng bộ).
    /// </summary>
    /// <param name="top">Số lượng sản phẩm cần lấy</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>Danh sách top sản phẩm bán chạy</returns>
    public async Task<List<TopSanPhamBanChayDTO>> LayTopBanChayAsync(int top, CancellationToken cancellationToken = default)
    {
        var soLuongTop = Math.Max(1, top);

        // Lấy toàn bộ đơn hàng hợp lệ để tổng hợp số lượng bán
        var hoaDons = await _thongKeRepo.TruyVanDoanhThuAsync(DateTime.MinValue, DateTime.MaxValue, cancellationToken);
        if (hoaDons == null || hoaDons.Count == 0)
        {
            return new List<TopSanPhamBanChayDTO>();
        }

        var danhSachBanChay = hoaDons
            .Where(h => !string.Equals(h.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.ChiTietHoaDons ?? Enumerable.Empty<Domain.Entities.Orders.ChiTietHoaDon>())
            .GroupBy(ct => ct.MaSanPham)
            .Select(g => new TopSanPhamBanChayDTO
            {
                MaSanPham = g.Key,
                TenSanPham = g.First().SanPham?.TenSanPham ?? g.Key,
                SoLuongDaBan = g.Sum(ct => ct.SoLuong),
                TongTienThu = g.Sum(ct => ct.TinhThanhTien())
            })
            .OrderByDescending(x => x.TongTienThu)
            .ThenByDescending(x => x.SoLuongDaBan)
            .Take(soLuongTop)
            .ToList();

        return danhSachBanChay;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm trang sức có doanh thu thấp nhất hoặc bán chậm (đồng bộ).
    /// Khớp phương thức: + LaySanPhamDoanhThuThap(top: int): List&lt;TopSanPhamBanChayDTO&gt; trong Diagram.
    /// </summary>
    /// <param name="top">Số lượng sản phẩm cần lấy</param>
    /// <returns>Danh sách sản phẩm doanh thu thấp</returns>
    public List<TopSanPhamBanChayDTO> LaySanPhamDoanhThuThap(int top)
    {
        return LaySanPhamDoanhThuThapAsync(top).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Lấy danh sách sản phẩm trang sức có doanh thu thấp nhất hoặc bán chậm (bất đồng bộ).
    /// </summary>
    /// <param name="top">Số lượng sản phẩm cần lấy</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>Danh sách sản phẩm doanh thu thấp</returns>
    public async Task<List<TopSanPhamBanChayDTO>> LaySanPhamDoanhThuThapAsync(int top, CancellationToken cancellationToken = default)
    {
        var soLuongTop = Math.Max(1, top);

        // Lấy danh sách các sản phẩm có doanh thu thấp từ kho lưu trữ
        var spDoanhThuThap = await _thongKeRepo.TruyVanSanPhamDoanhThuThapAsync(cancellationToken);

        // Lấy doanh số bán của tất cả đơn hàng
        var hoaDons = await _thongKeRepo.TruyVanDoanhThuAsync(DateTime.MinValue, DateTime.MaxValue, cancellationToken);
        var doanhSoMap = hoaDons?
            .Where(h => !string.Equals(h.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.ChiTietHoaDons ?? Enumerable.Empty<Domain.Entities.Orders.ChiTietHoaDon>())
            .GroupBy(ct => ct.MaSanPham)
            .ToDictionary(
                g => g.Key,
                g => (SoLuong: g.Sum(ct => ct.SoLuong), Tien: g.Sum(ct => ct.TinhThanhTien()))
            ) ?? new Dictionary<string, (int SoLuong, decimal Tien)>();

        var list = new List<TopSanPhamBanChayDTO>();

        if (spDoanhThuThap != null)
        {
            foreach (var sp in spDoanhThuThap)
            {
                doanhSoMap.TryGetValue(sp.MaSanPham, out var stat);
                list.Add(new TopSanPhamBanChayDTO
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    SoLuongDaBan = stat.SoLuong,
                    TongTienThu = stat.Tien
                });
            }
        }

        return list
            .OrderBy(x => x.TongTienThu)
            .ThenBy(x => x.SoLuongDaBan)
            .Take(soLuongTop)
            .ToList();
    }
}
