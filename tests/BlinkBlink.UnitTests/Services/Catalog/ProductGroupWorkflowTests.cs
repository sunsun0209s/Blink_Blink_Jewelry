using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Application.Services.Catalog;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.UnitTests.Services.Catalog;

/// <summary>
/// Kiểm thử luồng nghiệp vụ Quản lý Nhóm sản phẩm (QuanLyNhomSanPhamUseCase).
/// </summary>
public class ProductGroupWorkflowTests
{
    private class FakeNhomSanPhamRepository : INhomSanPhamRepository
    {
        public readonly Dictionary<string, NhomSanPham> Data = new();

        public void LuuNhom(NhomSanPham nhom) => Data[nhom.MaNhom] = nhom;
        public void CapNhatNhom(NhomSanPham nhom) => LuuNhom(nhom);
        public bool XoaNhom(string maNhom) => Data.Remove(maNhom);
        public NhomSanPham? TimTheoId(string maNhom) => Data.TryGetValue(maNhom, out var n) ? n : null;
        public List<NhomSanPham> LayTatCa() => Data.Values.ToList();

        public Task LuuNhomAsync(NhomSanPham nhom, CancellationToken ct = default) { LuuNhom(nhom); return Task.CompletedTask; }
        public Task CapNhatNhomAsync(NhomSanPham nhom, CancellationToken ct = default) { CapNhatNhom(nhom); return Task.CompletedTask; }
        public Task<bool> XoaNhomAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(XoaNhom(maNhom));
        public Task<NhomSanPham?> TimTheoIdAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(TimTheoId(maNhom));
        public Task<List<NhomSanPham>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
    }

    private class FakeSanPhamRepository : ISanPhamRepository
    {
        public readonly Dictionary<string, SanPhamTrangSuc> Data = new();

        public void LuuSanPham(SanPhamTrangSuc sp) => Data[sp.MaSanPham] = sp;
        public SanPhamTrangSuc? TimTheoId(string maSanPham) => Data.TryGetValue(maSanPham, out var sp) ? sp : null;
        public List<SanPhamTrangSuc> LayTatCa() => Data.Values.ToList();
        public List<SanPhamTrangSuc> LayTheoNhom(string maNhom) => Data.Values.Where(s => s.MaNhom == maNhom).ToList();

        public Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken ct = default) { LuuSanPham(sp); return Task.CompletedTask; }
        public Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken ct = default) => Task.FromResult(TimTheoId(maSanPham));
        public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
        public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(LayTheoNhom(maNhom));
    }

    [Fact]
    public void TaoNhom_ThanhCong_KhiTenHopLe()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var dto = new TaoNhomDTO { TenNhom = "Nhẫn Cưới Harmony", MoTa = "Mô tả nhẫn cưới" };
        var result = useCase.TaoNhom(dto);

        Assert.NotNull(result);
        Assert.Equal("Nhẫn Cưới Harmony", result.TenNhom);
        Assert.NotEmpty(result.MaNhom);
        Assert.True(nhomRepo.Data.ContainsKey(result.MaNhom));
    }

    [Fact]
    public void ThemSanPhamVaoNhom_ThanhCong_GiuNguyenTenVaCapNhatMaNhom()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var nhom = new NhomSanPham("NHOM01", "Nhẫn Solitaire Diamond 18K", "Bộ sưu tập");
        nhomRepo.LuuNhom(nhom);

        var sp = new SanPhamTrangSuc("SP01", "Tên Gốc Cũ", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "0");
        spRepo.LuuSanPham(sp);

        var success = useCase.ThemSanPhamVaoNhom("NHOM01", "SP01", out var error);

        Assert.True(success);
        Assert.Empty(error);
        var updatedSp = spRepo.TimTheoId("SP01");
        Assert.NotNull(updatedSp);
        Assert.Equal("NHOM01", updatedSp.MaNhom);
        // Bỏ việc đổi tên sản phẩm khi thêm vào nhóm: giữ nguyên tên gốc
        Assert.Equal("Tên Gốc Cũ", updatedSp.TenSanPham);
    }

    [Fact]
    public void ThemSanPhamVaoNhom_ThatBai_KhiTrungSizeTrongNhom()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var nhom = new NhomSanPham("NHOM01", "Nhẫn Solitaire", "");
        nhomRepo.LuuNhom(nhom);

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        var sp2 = new SanPhamTrangSuc("SP02", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "0");
        spRepo.LuuSanPham(sp1);
        spRepo.LuuSanPham(sp2);

        var success = useCase.ThemSanPhamVaoNhom("NHOM01", "SP02", out var error);

        Assert.False(success);
        Assert.Contains("trùng", error.ToLower());
    }

    [Fact]
    public void LoaiSanPhamKhoiNhom_ThanhCong_DatMaNhomVe0()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var nhom = new NhomSanPham("NHOM01", "Nhẫn Solitaire", "");
        nhomRepo.LuuNhom(nhom);

        var sp = new SanPhamTrangSuc("SP01", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        spRepo.LuuSanPham(sp);

        var success = useCase.LoaiSanPhamKhoiNhom("SP01");

        Assert.True(success);
        var updatedSp = spRepo.TimTheoId("SP01");
        Assert.NotNull(updatedSp);
        Assert.Equal("0", updatedSp.MaNhom);
    }

    [Fact]
    public void XoaNhom_ThanhCong_DatTatCaSanPhamVe0VaXoaKhoiRepo()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var nhom = new NhomSanPham("NHOM01", "Nhẫn Solitaire", "");
        nhomRepo.LuuNhom(nhom);

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        var sp2 = new SanPhamTrangSuc("SP02", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 15", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        spRepo.LuuSanPham(sp1);
        spRepo.LuuSanPham(sp2);

        var success = useCase.XoaNhom("NHOM01");

        Assert.True(success);
        Assert.Null(nhomRepo.TimTheoId("NHOM01"));
        Assert.Equal("0", spRepo.TimTheoId("SP01")?.MaNhom);
        Assert.Equal("0", spRepo.TimTheoId("SP02")?.MaNhom);
    }

    [Fact]
    public void CapNhatNhom_TuyChinhTenNhomDocLap_KhongGhiDeTenSanPhamTrongNhom()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var nhom = new NhomSanPham("NHOM01", "Tên Nhóm Cũ", "");
        nhomRepo.LuuNhom(nhom);

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn Kim Cương Size 14", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        var sp2 = new SanPhamTrangSuc("SP02", "Nhẫn Kim Cương Size 15", "DM01", "Vàng 18K", "Size 15", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        spRepo.LuuSanPham(sp1);
        spRepo.LuuSanPham(sp2);

        // Cập nhật tên nhóm tùy chỉnh mới
        useCase.CapNhatTenNhom("NHOM01", "Tên Nhóm Mới Thượng Hạng");

        // Nhóm đổi tên thành công
        Assert.Equal("Tên Nhóm Mới Thượng Hạng", nhomRepo.TimTheoId("NHOM01")?.TenNhom);
        // Tên của các sản phẩm con được giữ nguyên độc lập, không bị ghi đè
        Assert.Equal("Nhẫn Kim Cương Size 14", spRepo.TimTheoId("SP01")?.TenSanPham);
        Assert.Equal("Nhẫn Kim Cương Size 15", spRepo.TimTheoId("SP02")?.TenSanPham);
    }

    [Fact]
    public void KiemTraTrungSizeTrongNhom_PhatHienSizeTrung()
    {
        var nhomRepo = new FakeNhomSanPhamRepository();
        var spRepo = new FakeSanPhamRepository();
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, spRepo);

        var sp1 = new SanPhamTrangSuc("SP01", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 14", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        var sp2 = new SanPhamTrangSuc("SP02", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 15", 1000m, 2000m, "SanSang", "", 0, "NHOM01");
        spRepo.LuuSanPham(sp1);
        spRepo.LuuSanPham(sp2);

        // Đổi SP02 thành Size 14 -> bị trùng với SP01
        var isDuplicate = useCase.KiemTraTrungSizeTrongNhom("SP02", "Size 14", out var error1);
        Assert.True(isDuplicate);
        Assert.NotEmpty(error1);

        // Đổi SP02 thành Size 16 -> không trùng
        var isNotDuplicate = useCase.KiemTraTrungSizeTrongNhom("SP02", "Size 16", out var error2);
        Assert.False(isNotDuplicate);
        Assert.Empty(error2);

        // Sản phẩm độc lập maNhom = "0" -> không bao giờ trùng
        var sp0 = new SanPhamTrangSuc("SP00", "Độc lập", "DM01", "Vàng", "Size 14", 1000m, 2000m, "SanSang", "", 0, "0");
        spRepo.LuuSanPham(sp0);
        Assert.False(useCase.KiemTraTrungSizeTrongNhom("SP00", "Size 14", out _));
    }
}
