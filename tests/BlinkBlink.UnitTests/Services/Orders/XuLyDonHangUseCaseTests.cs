using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.Services.Orders;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Orders;

public class XuLyDonHangUseCaseTests
{
    private class FakeHoaDonRepository : IHoaDonRepository
    {
        public readonly Dictionary<string, HoaDon> Data = new();

        public void LuuHoaDon(HoaDon hd)
        {
            Data[hd.MaHoaDon] = hd;
        }

        public Task LuuHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default)
        {
            Data[hd.MaHoaDon] = hd;
            return Task.CompletedTask;
        }

        public HoaDon? TimTheoMa(string maHoaDon)
        {
            return Data.TryGetValue(maHoaDon, out var hd) ? hd : null;
        }

        public Task<HoaDon?> TimTheoMaAsync(string maHoaDon, CancellationToken cancellationToken = default)
        {
            Data.TryGetValue(maHoaDon, out var hd);
            return Task.FromResult(hd);
        }

        public List<HoaDon> LayTheoKhachHang(string maKH)
        {
            return Data.Values.Where(h => h.MaKhachHang == maKH).ToList();
        }

        public Task<List<HoaDon>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Data.Values.Where(h => h.MaKhachHang == maKH).ToList());
        }

        public List<HoaDon> LayDanhSachDonHang()
        {
            return Data.Values.ToList();
        }

        public Task<List<HoaDon>> LayDanhSachDonHangAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Data.Values.ToList());
        }

        public void CapNhatHoaDon(HoaDon hd)
        {
            Data[hd.MaHoaDon] = hd;
        }

        public Task CapNhatHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default)
        {
            Data[hd.MaHoaDon] = hd;
            return Task.CompletedTask;
        }

        public void CapNhatKhachHangTheoSoDienThoai(string soDienThoai, string maKhachHangMoi) { }
    }

    private class FakeSanPhamRepository : ISanPhamRepository
    {
        public readonly Dictionary<string, SanPhamTrangSuc> Data = new();

        public void LuuSanPham(SanPhamTrangSuc sp)
        {
            Data[sp.MaSanPham] = sp;
        }

        public Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken cancellationToken = default)
        {
            Data[sp.MaSanPham] = sp;
            return Task.CompletedTask;
        }

        public SanPhamTrangSuc? TimTheoId(string maSanPham)
        {
            return Data.TryGetValue(maSanPham, out var sp) ? sp : null;
        }

        public Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken cancellationToken = default)
        {
            Data.TryGetValue(maSanPham, out var sp);
            return Task.FromResult(sp);
        }

        public List<SanPhamTrangSuc> LayTatCa() => new List<SanPhamTrangSuc>(Data.Values);
        public List<SanPhamTrangSuc> LayTheoNhom(string maNhom) => Data.Values.Where(s => s.MaNhom == maNhom).ToList();
        public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken cancellationToken = default) => Task.FromResult(LayTatCa());
        public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken cancellationToken = default) => Task.FromResult(LayTheoNhom(maNhom));
    }

    [Fact]
    public async Task DuyetDonHang_ThanhCong_ChuyenTrangThaiVaCapNhatSanPham()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP01", "Vòng tay thạch anh", "DM01", "Vàng hồng", "16cm", 5000000m, 9000000m, "SanSang");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var hd = new HoaDon("HD100", DateTime.UtcNow, "KH01", "123 Lê Lợi", "0909123456", "COD", "ChoDuyet");
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD100", "SP01", 9000000m, 1));
        hd.TinhTongTien();
        hoaDonRepo.Data[hd.MaHoaDon] = hd;

        var useCase = new XuLyDonHangUseCase(hoaDonRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.DuyetDonHangAsync("HD100", "NV001");

        // Kiểm tra
        Assert.True(result);
        Assert.Equal("DangDongGoi", hd.TrangThaiDonHang);
        Assert.Equal("NV001", hd.MaNhanVien);
        Assert.Equal("DaBan", sp.TrangThai);
    }

    [Fact]
    public async Task CapNhatTrangThai_ThanhCong_ChuyenSangDangGiao()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var hd = new HoaDon("HD100", DateTime.UtcNow, "KH01", "123 Lê Lợi", "0909123456", "COD", "DangDongGoi", maNhanVien: "NV001");
        hoaDonRepo.Data[hd.MaHoaDon] = hd;

        var useCase = new XuLyDonHangUseCase(hoaDonRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.CapNhatTrangThaiAsync("HD100", "DangGiao", "NV001");

        // Kiểm tra
        Assert.True(result);
        Assert.Equal("DangGiao", hd.TrangThaiDonHang);
    }

    [Fact]
    public async Task HuyDonHang_ThanhCong_ChuyenTrangThaiVaHoanSanPham()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP01", "Nhẫn kim cương", "DM01", "Bạch kim", "12", 20000000m, 35000000m, "DaBan");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var hd = new HoaDon("HD200", DateTime.UtcNow, "KH01", "123 Lê Lợi", "0909123456", "COD", "ChoDuyet");
        hd.ChiTietHoaDons.Add(new ChiTietHoaDon("HD200", "SP01", 35000000m, 1));
        hoaDonRepo.Data[hd.MaHoaDon] = hd;

        var useCase = new XuLyDonHangUseCase(hoaDonRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.HuyDonHangAsync("HD200");

        // Kiểm tra
        Assert.True(result);
        Assert.Equal("DaHuy", hd.TrangThaiDonHang);
        Assert.Equal("SanSang", sp.TrangThai);
    }

    [Fact]
    public async Task HuyDonHang_ThatBai_KhiDonDaGiao()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var hd = new HoaDon("HD300", DateTime.UtcNow, "KH01", "123 Lê Lợi", "0909123456", "COD", "DaGiao");
        hoaDonRepo.Data[hd.MaHoaDon] = hd;

        var useCase = new XuLyDonHangUseCase(hoaDonRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.HuyDonHangAsync("HD300");

        // Kiểm tra
        Assert.False(result);
        Assert.Equal("DaGiao", hd.TrangThaiDonHang);
    }
}
