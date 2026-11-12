using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Orders;
using BlinkBlink.Application.Services.Orders;
using BlinkBlink.Domain.Entities.Cart;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Orders;

public class DatHangUseCaseTests
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

    private class FakeGioHangRepository : IGioHangRepository
    {
        public readonly Dictionary<string, GioHang> Data = new();

        public GioHang? LayGioHang(string maKH)
        {
            return Data.TryGetValue(maKH, out var gh) ? gh : null;
        }

        public Task<GioHang?> LayGioHangAsync(string maKH, CancellationToken cancellationToken = default)
        {
            Data.TryGetValue(maKH, out var gh);
            return Task.FromResult(gh);
        }

        public void CapNhatGioHang(GioHang gh)
        {
            Data[gh.MaKhachHang] = gh;
        }

        public Task CapNhatGioHangAsync(GioHang gh, CancellationToken cancellationToken = default)
        {
            Data[gh.MaKhachHang] = gh;
            return Task.CompletedTask;
        }

        public Task LuuGioHangAsync(GioHang gh, CancellationToken cancellationToken = default)
        {
            Data[gh.MaKhachHang] = gh;
            return Task.CompletedTask;
        }
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
    public async Task DatHang_ThanhCong_KhiGioHangHopLeVaSanPhamConHang()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var gioHangRepo = new FakeGioHangRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp1 = new SanPhamTrangSuc("SP001", "Nhẫn kim cương Solitaire", "DM01", "Vàng trắng 18K", "Size 10", 15000000m, 25000000m, "SanSang");
        var sp2 = new SanPhamTrangSuc("SP002", "Dây chuyền vàng Ý", "DM02", "Vàng 24K", "Size 45cm", 8000000m, 12000000m, "SanSang");
        sanPhamRepo.Data[sp1.MaSanPham] = sp1;
        sanPhamRepo.Data[sp2.MaSanPham] = sp2;

        var gioHang = new GioHang("GH_KH01", "KH01");
        gioHang.ThemSanPham(sp1.MaSanPham, 1);
        gioHang.ThemSanPham(sp2.MaSanPham, 2);
        gioHangRepo.Data["KH01"] = gioHang;

        var useCase = new DatHangUseCase(hoaDonRepo, gioHangRepo, sanPhamRepo);
        var request = new DatHangDTO
        {
            MaKhachHang = "KH01",
            DiaChiGiaoHang = "123 Lê Lợi, Quận 1, TP.HCM",
            SoDienThoaiNhan = "0909123456",
            PhuongThucThanhToan = "COD",
            GhiChu = "Giao hàng giờ hành chính"
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra
        Assert.True(result);
        Assert.Single(hoaDonRepo.Data);
        var hoaDon = hoaDonRepo.Data.Values.First();
        Assert.Equal("KH01", hoaDon.MaKhachHang);
        Assert.Equal("ChoDuyet", hoaDon.TrangThaiDonHang);
        Assert.Equal(2, hoaDon.ChiTietHoaDons.Count);
        // 1 * 25tr + 2 * 12tr = 49tr
        Assert.Equal(49000000m, hoaDon.TongThanhToan);
        // Giỏ hàng phải được làm trống sau khi tạo đơn
        Assert.Empty(gioHang.DanhSachSanPham);
        Assert.Equal(0, gioHang.SoLuong);
    }

    [Fact]
    public async Task DatHang_ThatBai_KhiGioHangRong()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var gioHangRepo = new FakeGioHangRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var gioHang = new GioHang("GH_KH01", "KH01");
        gioHangRepo.Data["KH01"] = gioHang;

        var useCase = new DatHangUseCase(hoaDonRepo, gioHangRepo, sanPhamRepo);
        var request = new DatHangDTO
        {
            MaKhachHang = "KH01",
            DiaChiGiaoHang = "123 Lê Lợi, Quận 1, TP.HCM",
            SoDienThoaiNhan = "0909123456"
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra
        Assert.False(result);
        Assert.Empty(hoaDonRepo.Data);
    }

    [Fact]
    public async Task DatHang_ThatBai_KhiSanPhamTrongGioKhongConSanSang()
    {
        // Sắp đặt
        var hoaDonRepo = new FakeHoaDonRepository();
        var gioHangRepo = new FakeGioHangRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        // Sản phẩm đã bán
        var sp1 = new SanPhamTrangSuc("SP001", "Nhẫn kim cương", "DM01", "Vàng trắng", "10", 10000000m, 20000000m, "DaBan");
        sanPhamRepo.Data[sp1.MaSanPham] = sp1;

        var gioHang = new GioHang("GH_KH01", "KH01");
        gioHang.ThemSanPham(sp1.MaSanPham, 1);
        gioHangRepo.Data["KH01"] = gioHang;

        var useCase = new DatHangUseCase(hoaDonRepo, gioHangRepo, sanPhamRepo);
        var request = new DatHangDTO
        {
            MaKhachHang = "KH01",
            DiaChiGiaoHang = "123 Lê Lợi, Quận 1, TP.HCM",
            SoDienThoaiNhan = "0909123456"
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);
        var detailedResult = await useCase.ExecuteWithResultAsync(request);

        // Kiểm tra
        Assert.False(result);
        Assert.False(detailedResult.IsSuccess);
        Assert.Equal(ErrorCodes.OutOfStock, detailedResult.ErrorCode);
        Assert.Empty(hoaDonRepo.Data);
    }

    [Fact]
    public async Task DatHang_TraVeMaLoiGioHangRong_KhiKhongCoSanPham()
    {
        var useCase = new DatHangUseCase(
            new FakeHoaDonRepository(),
            new FakeGioHangRepository(),
            new FakeSanPhamRepository());

        var result = await useCase.ExecuteWithResultAsync(new DatHangDTO
        {
            MaKhachHang = "KH_EMPTY",
            DiaChiGiaoHang = "123 Lê Lợi",
            SoDienThoaiNhan = "0909123456"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.CartEmpty, result.ErrorCode);
    }
}
