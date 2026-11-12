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

public class ThongKeKhoHangUseCaseTests
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
    public async Task LayBaoCaoTonKho_ThanhCong_TongHopGiaTriVaChenhLechKiemKe()
    {
        // Sắp đặt
        var repo = new FakeThongKeRepository();

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn kim cương", "DM01", "Vàng trắng", "10", 15000000m, 20000000m, "SanSang");
        var sp2 = new SanPhamTrangSuc("SP02", "Lắc tay bạc", "DM03", "Bạc 925", "17cm", 1000000m, 2000000m, "SanSang");
        repo.SanPhams.AddRange(new[] { sp1, sp2 });

        // Có chênh lệch kiểm kê: SP01 hao hụt -1 (Sổ sách 5, thực tế 4)
        var kk = new ChiTietKiemKe("PKK01", "SP01", 5, 4);
        repo.ChiTietKiemKes.Add(kk);

        var useCase = new ThongKeKhoHangUseCase(repo);

        // Thực thi
        var result = await useCase.LayBaoCaoTonKhoAsync();

        // Kiểm tra
        Assert.Equal(2, result.Count);
        var r1 = result.First(r => r.MaSanPham == "SP01");
        Assert.Equal("Nhẫn kim cương", r1.TenSanPham);
        Assert.Equal(1, r1.SoLuongTon);
        Assert.Equal(20000000m, r1.GiaTriTonKho);
        Assert.Equal(-1, r1.ChenhLechKiemKe);
    }

    [Fact]
    public async Task LayTopBanChay_ThanhCong_XepHangTheoDoanhSoGiamDan()
    {
        // Sắp đặt
        var repo = new FakeThongKeRepository();

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn kim cương", "DM01", "Vàng trắng", "10", 10000000m, 20000000m);
        var sp2 = new SanPhamTrangSuc("SP02", "Dây chuyền vàng", "DM02", "Vàng 24K", "45cm", 5000000m, 10000000m);
        var sp3 = new SanPhamTrangSuc("SP03", "Bông tai ngọc trai", "DM04", "Ngọc trai", "F", 2000000m, 4000000m);

        var hd = new HoaDon("HD01", DateTime.UtcNow, "KH01", "123 Lê Lợi", "0909123456", "COD", "DaGiao");
        // SP01 bán 2 cái = 40tr
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD01", "SP01", 20000000m, 2) { SanPham = sp1 });
        // SP02 bán 5 cái = 50tr
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD01", "SP02", 10000000m, 5) { SanPham = sp2 });
        // SP03 bán 1 cái = 4tr
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD01", "SP03", 4000000m, 1) { SanPham = sp3 });
        hd.TinhTongTien();
        repo.HoaDons.Add(hd);

        var useCase = new ThongKeKhoHangUseCase(repo);

        // Thực thi: Lấy top 2
        var result = await useCase.LayTopBanChayAsync(2);

        // Kiểm tra: SP02 (50tr) đứng thứ 1, SP01 (40tr) đứng thứ 2
        Assert.Equal(2, result.Count);
        Assert.Equal("SP02", result[0].MaSanPham);
        Assert.Equal(50000000m, result[0].TongTienThu);
        Assert.Equal(5, result[0].SoLuongDaBan);

        Assert.Equal("SP01", result[1].MaSanPham);
        Assert.Equal(40000000m, result[1].TongTienThu);
        Assert.Equal(2, result[1].SoLuongDaBan);
    }

    [Fact]
    public async Task LaySanPhamDoanhThuThap_ThanhCong_XepHangDoanhSoThapNhat()
    {
        // Sắp đặt
        var repo = new FakeThongKeRepository();

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn kim cương", "DM01", "Vàng trắng", "10", 10000000m, 20000000m);
        var sp2 = new SanPhamTrangSuc("SP02", "Dây chuyền vàng", "DM02", "Vàng 24K", "45cm", 5000000m, 10000000m);
        var sp3 = new SanPhamTrangSuc("SP03", "Kẹp cà vạt bạc", "DM05", "Bạc", "F", 500000m, 1000000m);
        repo.SanPhams.AddRange(new[] { sp1, sp2, sp3 });

        var hd = new HoaDon("HD01", DateTime.UtcNow, "KH01", "123 Lê Lợi", "0909123456", "COD", "DaGiao");
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD01", "SP01", 20000000m, 2) { SanPham = sp1 });
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD01", "SP02", 10000000m, 5) { SanPham = sp2 });
        // SP03 chưa bán được cái nào (doanh thu = 0)
        hd.TinhTongTien();
        repo.HoaDons.Add(hd);

        var useCase = new ThongKeKhoHangUseCase(repo);

        // Thực thi: Lấy 1 sản phẩm doanh thu thấp nhất
        var result = await useCase.LaySanPhamDoanhThuThapAsync(1);

        // Kiểm tra: SP03 có doanh thu thấp nhất (0 VNĐ)
        Assert.Single(result);
        Assert.Equal("SP03", result[0].MaSanPham);
        Assert.Equal(0m, result[0].TongTienThu);
        Assert.Equal(0, result[0].SoLuongDaBan);
    }
}
