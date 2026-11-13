using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Warranty;

public class XuLyBaoHanhUseCaseTests
{
    private class FakeBaoHanhRepository : IBaoHanhRepository
    {
        public readonly Dictionary<string, PhieuBaoHanh> Data = new();

        public void LuuPhieu(PhieuBaoHanh pbh)
        {
            Data[pbh.SoPhieuBH] = pbh;
        }

        public Task LuuPhieuAsync(PhieuBaoHanh pbh, CancellationToken cancellationToken = default)
        {
            Data[pbh.SoPhieuBH] = pbh;
            return Task.CompletedTask;
        }

        public PhieuBaoHanh? TimTheoSoPhieu(string soPhieu)
        {
            return Data.TryGetValue(soPhieu, out var pbh) ? pbh : null;
        }

        public Task<PhieuBaoHanh?> TimTheoSoPhieuAsync(string soPhieu, CancellationToken cancellationToken = default)
        {
            Data.TryGetValue(soPhieu, out var pbh);
            return Task.FromResult(pbh);
        }

        public List<PhieuBaoHanh> LayTheoKhachHang(string maKH)
        {
            return Data.Values.Where(p => p.MaKhachHang == maKH).ToList();
        }

        public Task<List<PhieuBaoHanh>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Data.Values.Where(p => p.MaKhachHang == maKH).ToList());
        }

        public List<PhieuBaoHanh> LayDanhSachPhieu()
        {
            return Data.Values.ToList();
        }

        public Task<List<PhieuBaoHanh>> LayDanhSachPhieuAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Data.Values.ToList());
        }

        public void CapNhatPhieu(PhieuBaoHanh pbh)
        {
            Data[pbh.SoPhieuBH] = pbh;
        }

        public void CapNhatKhachHangTheoSoDienThoai(string soDienThoai, string maKhachHangMoi)
        {
            if (string.IsNullOrWhiteSpace(soDienThoai) || string.IsNullOrWhiteSpace(maKhachHangMoi)) return;
            foreach (var p in Data.Values.Where(p => p.SoDienThoai == soDienThoai && (p.MaKhachHang == "0" || p.MaKhachHang == "KH_0")))
            {
                p.MaKhachHang = maKhachHangMoi;
            }
        }

        public Task CapNhatPhieuAsync(PhieuBaoHanh pbh, CancellationToken cancellationToken = default)
        {
            Data[pbh.SoPhieuBH] = pbh;
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
    public async Task CapNhatTienDo_ThanhCong_ChuyenSangDangXuLy()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var phieu = new PhieuBaoHanh("BH001", "KH01", "SP01", "Làm mới", 0m, "DangBaoHanh");
        baoHanhRepo.Data[phieu.SoPhieuBH] = phieu;

        var useCase = new XuLyBaoHanhUseCase(baoHanhRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.CapNhatTienDoAsync("BH001", "DangXuLy");

        // Kiểm tra
        Assert.True(result);
        Assert.Equal("DangXuLy", phieu.TrangThaiXuLy);
    }

    [Fact]
    public async Task HoanTatBaoHanh_ThanhCong_ChuyenDaHoanTatVaCapNhatChiPhi()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP01", "Vòng tay thạch anh", "DM01", "Vàng hồng", "16cm", 5000000m, 9000000m, "DangBaoHanh");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var phieu = new PhieuBaoHanh("BH002", "KH01", "SP01", "Gắn đá rơi", 0m, "DangBaoHanh");
        baoHanhRepo.Data[phieu.SoPhieuBH] = phieu;

        var useCase = new XuLyBaoHanhUseCase(baoHanhRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.HoanTatBaoHanhAsync("BH002", 250000m);

        // Kiểm tra
        Assert.True(result);
        Assert.Equal("DaTraKhach", phieu.TrangThaiXuLy);
        Assert.Equal(250000m, phieu.ChiPhiBaoHanh);
        Assert.Equal(250000m, phieu.TinhPhiBaoHanh());
        // Trạng thái sản phẩm được hoàn lại DaBan
        Assert.Equal("DaBan", sp.TrangThai);
    }

    [Fact]
    public async Task HoanTatBaoHanh_ThatBai_KhiPhieuKhongTonTai()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var useCase = new XuLyBaoHanhUseCase(baoHanhRepo, sanPhamRepo);

        // Thực thi
        var result = await useCase.HoanTatBaoHanhAsync("BH_KHONG_TON_TAI", 100000m);

        // Kiểm tra
        Assert.False(result);
    }
}
