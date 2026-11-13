using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.Services.Reporting;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Reporting;

public class ThongKeDoanhThuUseCaseTests
{
    private class FakeThongKeRepository : IThongKeRepository
    {
        public readonly List<HoaDon> HoaDons = new();
        public readonly List<SanPhamTrangSuc> SanPhams = new();
        public readonly List<ChiTietKiemKe> ChiTietKiemKes = new();

        public List<HoaDon> TruyVanDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            return HoaDons.Where(h => h.NgayLap >= tuNgay && h.NgayLap <= denNgay).ToList();
        }

        public Task<List<HoaDon>> TruyVanDoanhThuAsync(DateTime tuNgay, DateTime denNgay, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HoaDons.Where(h => h.NgayLap >= tuNgay && h.NgayLap <= denNgay).ToList());
        }

        public List<SanPhamTrangSuc> TruyVanSanPhamTonKho()
        {
            return SanPhams.Where(s => s.TrangThai == "SanSang").ToList();
        }

        public Task<List<SanPhamTrangSuc>> TruyVanSanPhamTonKhoAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SanPhams.Where(s => s.TrangThai == "SanSang").ToList());
        }

        public List<ChiTietKiemKe> TruyVanChenhLechKiemKe()
        {
            return ChiTietKiemKes.Where(k => k.ChenhLech != 0).ToList();
        }

        public Task<List<ChiTietKiemKe>> TruyVanChenhLechKiemKeAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ChiTietKiemKes.Where(k => k.ChenhLech != 0).ToList());
        }

        public List<SanPhamTrangSuc> TruyVanSanPhamDoanhThuThap()
        {
            return SanPhams.ToList();
        }

        public Task<List<SanPhamTrangSuc>> TruyVanSanPhamDoanhThuThapAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SanPhams.ToList());
        }
    }

    [Fact]
    public async Task Execute_TinhDoanhThuVaLoiNhuanChinhXac_LoaiBoDonHuy()
    {
        // Sắp đặt
        var repo = new FakeThongKeRepository();

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn kim cương", "DM01", "Vàng trắng", "10", giaNhap: 15000000m, giaBan: 25000000m);
        var sp2 = new SanPhamTrangSuc("SP02", "Dây chuyền vàng", "DM02", "Vàng 24K", "45cm", giaNhap: 8000000m, giaBan: 12000000m);

        var date = new DateTime(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

        // Đơn 1: Hợp lệ (1 x SP01) -> Bán 25tr, vốn 15tr, lãi 10tr
        var hd1 = new HoaDon("HD01", date, "KH01", "123 Lê Lợi", "0909123456", "COD", "DaGiao");
        hd1.ChiTietHoaDons.Add(new ChiTietHoaDon("HD01", "SP01", 25000000m, 1) { SanPham = sp1 });
        hd1.TinhTongTien();

        // Đơn 2: Hợp lệ (2 x SP02) -> Bán 24tr, vốn 16tr, lãi 8tr
        var hd2 = new HoaDon("HD02", date.AddHours(2), "KH02", "456 Nguyễn Huệ", "0909654321", "COD", "DangGiao");
        hd2.ChiTietHoaDons.Add(new ChiTietHoaDon("HD02", "SP02", 12000000m, 2) { SanPham = sp2 });
        hd2.TinhTongTien();

        // Đơn 3: Đã hủy -> Không được tính vào doanh thu
        var hd3 = new HoaDon("HD03", date.AddHours(3), "KH03", "789 Đồng Khởi", "0909999888", "COD", "DaHuy");
        hd3.ChiTietHoaDons.Add(new ChiTietHoaDon("HD03", "SP01", 25000000m, 1) { SanPham = sp1 });
        hd3.TinhTongTien();

        repo.HoaDons.AddRange(new[] { hd1, hd2, hd3 });

        var useCase = new ThongKeDoanhThuUseCase(repo);

        // Thực thi
        var result = await useCase.ExecuteAsync(date.Date, date.Date.AddDays(1));

        // Kiểm tra
        Assert.Single(result);
        var stat = result.First();
        Assert.Equal(date.Date, stat.NgayThang);
        Assert.Equal(2, stat.SoLuongDon); // Chỉ tính 2 đơn hợp lệ, bỏ đơn hủy
        Assert.Equal(49000000m, stat.TongDoanhThu); // 25tr + 24tr = 49tr
        Assert.Equal(18000000m, stat.LoiNhuanUocTinh); // 49tr - (15tr + 16tr) = 18tr
    }

    [Fact]
    public async Task Execute_TraVeDanhSachRong_KhiKhongCoDonHangTrongKhoangThoiGian()
    {
        // Sắp đặt
        var repo = new FakeThongKeRepository();
        var useCase = new ThongKeDoanhThuUseCase(repo);

        // Thực thi
        var result = await useCase.ExecuteAsync(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

        // Kiểm tra
        Assert.Empty(result);
    }
}
