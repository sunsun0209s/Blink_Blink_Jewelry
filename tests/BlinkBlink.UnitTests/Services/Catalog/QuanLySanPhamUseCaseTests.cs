using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Application.Services.Catalog;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Catalog;

public class QuanLySanPhamUseCaseTests
{
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

        public SanPhamTrangSuc? TimTheoId(string id)
        {
            return Data.TryGetValue(id, out var sp) ? sp : null;
        }

        public Task<SanPhamTrangSuc?> TimTheoIdAsync(string id, CancellationToken cancellationToken = default)
        {
            Data.TryGetValue(id, out var sp);
            return Task.FromResult(sp);
        }

        public List<SanPhamTrangSuc> LayTatCa() => new List<SanPhamTrangSuc>(Data.Values);
        public List<SanPhamTrangSuc> LayTheoNhom(string maNhom) => Data.Values.Where(s => s.MaNhom == maNhom).ToList();
        public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken cancellationToken = default) => Task.FromResult(LayTatCa());
        public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken cancellationToken = default) => Task.FromResult(LayTheoNhom(maNhom));
    }

    private class FakeDanhMucRepository : IDanhMucRepository
    {
        public readonly Dictionary<string, DanhMuc> Data = new();

        public void LuuDanhMuc(DanhMuc dm)
        {
            Data[dm.MaDanhMuc] = dm;
        }

        public Task LuuDanhMucAsync(DanhMuc dm, CancellationToken cancellationToken = default)
        {
            Data[dm.MaDanhMuc] = dm;
            return Task.CompletedTask;
        }

        public DanhMuc? TimTheoId(string id)
        {
            return Data.TryGetValue(id, out var dm) ? dm : null;
        }

        public Task<DanhMuc?> TimTheoIdAsync(string id, CancellationToken cancellationToken = default)
        {
            Data.TryGetValue(id, out var dm);
            return Task.FromResult(dm);
        }

        public List<DanhMuc> LayTatCa() => new List<DanhMuc>(Data.Values);
        public Task<List<DanhMuc>> LayTatCaAsync(CancellationToken cancellationToken = default) => Task.FromResult(LayTatCa());
    }

    [Fact]
    public void CapNhatThongTin_ValidRequest_UpdatesProductInformation()
    {
        // Arrange
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository();
        var product = new SanPhamTrangSuc(
            maSanPham: "SP_01",
            tenSanPham: "Nhan Kim Cuong 1",
            maDanhMuc: "DM_NHAN",
            chatLieu: "Vang 18K",
            size: "12",
            giaNhap: 5000000,
            giaBan: 8000000
        );
        fakeSpRepo.LuuSanPham(product);

        var useCase = new QuanLySanPhamUseCase(fakeSpRepo, fakeDmRepo);
        var request = new CapNhatSanPhamDTO
        {
            MaSanPham = "SP_01",
            TenSanPham = "Nhan Kim Cuong Solitaire",
            MaDanhMuc = "DM_NHAN_CUOI",
            ChatLieu = "Bach Kim PT950",
            Size = "14"
        };

        // Act
        var result = useCase.CapNhatThongTin(request);

        // Assert
        Assert.True(result);
        var updated = fakeSpRepo.TimTheoId("SP_01");
        Assert.NotNull(updated);
        Assert.Equal("Nhan Kim Cuong Solitaire", updated.TenSanPham);
        Assert.Equal("DM_NHAN_CUOI", updated.MaDanhMuc);
        Assert.Equal("Bach Kim PT950", updated.ChatLieu);
        Assert.Equal("14", updated.Size);
    }

    [Fact]
    public void CapNhatThongTin_ProductNotFound_ReturnsFalse()
    {
        // Arrange
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(fakeSpRepo, fakeDmRepo);
        var request = new CapNhatSanPhamDTO
        {
            MaSanPham = "SP_NONEXIST",
            TenSanPham = "Test",
            MaDanhMuc = "DM_01"
        };

        // Act
        var result = useCase.CapNhatThongTin(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ThemSanPhamMoi_KhoiTaoThanhCong_CoTrangThaiHetHangVaLuuHinhAnh()
    {
        // Arrange
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(fakeSpRepo, fakeDmRepo);

        var dto = new SanPhamDTO
        {
            MaSanPham = "SP_NEW01",
            TenSanPham = "Bông Tai Kim Cương Hoàng Gia",
            MaDanhMuc = "DM03",
            ChatLieu = "Bạch kim Platin 950",
            Size = "Tiêu chuẩn",
            GiaNhap = 12000000,
            GiaBan = 18500000,
            HinhAnh = "C:\\images\\bongtai.png"
        };

        // Act
        var result = useCase.ThemSanPhamMoi(dto);

        // Assert
        Assert.True(result);
        var created = fakeSpRepo.TimTheoId("SP_NEW01");
        Assert.NotNull(created);
        Assert.Equal("HetHang", created.TrangThai); // Ban đầu tồn kho = 0, chưa nhập kho
        Assert.Equal("C:\\images\\bongtai.png", created.HinhAnh);
        Assert.Equal(12000000, created.GiaNhap);
        Assert.Equal(18500000, created.GiaBan);
    }

    [Fact]
    public void CapNhatThongTin_CapNhatGiaNhapGiaBanVaHinhAnh()
    {
        // Arrange
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository();
        var sp = new SanPhamTrangSuc("SP_02", "Nhẫn Cũ", "DM01", "Vàng 14K", "10", 3000000, 5000000, "HetHang", "");
        fakeSpRepo.LuuSanPham(sp);

        var useCase = new QuanLySanPhamUseCase(fakeSpRepo, fakeDmRepo);
        var dto = new CapNhatSanPhamDTO
        {
            MaSanPham = "SP_02",
            TenSanPham = "Nhẫn Cưới Kim Cương Vàng Ý",
            MaDanhMuc = "DM02",
            ChatLieu = "Vàng Ý 750",
            Size = "15",
            GiaNhap = 6000000,
            GiaBan = 9500000,
            HinhAnh = "C:\\images\\nhan_moi.jpg"
        };

        // Act
        var result = useCase.CapNhatThongTin(dto);

        // Assert
        Assert.True(result);
        var updated = fakeSpRepo.TimTheoId("SP_02");
        Assert.NotNull(updated);
        Assert.Equal("Nhẫn Cưới Kim Cương Vàng Ý", updated.TenSanPham);
        Assert.Equal(6000000, updated.GiaNhap);
        Assert.Equal(9500000, updated.GiaBan);
        Assert.Equal("C:\\images\\nhan_moi.jpg", updated.HinhAnh);
    }

    [Fact]
    public void ThemSanPhamMoi_KhoiTaoMacDinhPhanTramGiamGiaVaHetHang()
    {
        // Arrange
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(fakeSpRepo, fakeDmRepo);
        var dto = new SanPhamDTO
        {
            MaSanPham = "SP_TEST_DISCOUNT",
            TenSanPham = "Lắc Tay Bạc Ý Cao Cấp",
            MaDanhMuc = "DM_LACTAY",
            ChatLieu = "Bạc Ý 925",
            Size = "17cm",
            GiaNhap = 0,
            GiaBan = 0,
            PhanTramGiamGia = 0
        };

        // Act
        var result = useCase.ThemSanPhamMoi(dto);

        // Assert
        Assert.True(result);
        var created = fakeSpRepo.TimTheoId("SP_TEST_DISCOUNT");
        Assert.NotNull(created);
        Assert.Equal(0, created.PhanTramGiamGia);
        Assert.Equal("HetHang", created.TrangThai);
        Assert.Equal(0, created.GiaNhap);
        Assert.Equal(0, created.GiaBan);
    }

    [Fact]
    public void CapNhatThongTin_CapNhatPhanTramGiamGiaHopLe()
    {
        // Arrange
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository();
        var sp = new SanPhamTrangSuc("SP_03", "Dây Chuyền Vàng", "DM_DAYCHUYEN", "Vàng 18K", "45cm", 10000000, 15000000, "SanSang", "", 0);
        fakeSpRepo.LuuSanPham(sp);

        var useCase = new QuanLySanPhamUseCase(fakeSpRepo, fakeDmRepo);
        var dto = new CapNhatSanPhamDTO
        {
            MaSanPham = "SP_03",
            TenSanPham = "Dây Chuyền Vàng Ý",
            MaDanhMuc = "DM_DAYCHUYEN",
            ChatLieu = "Vàng 18K",
            Size = "45cm",
            GiaBan = 14000000,
            PhanTramGiamGia = 15 // Giảm giá 15%
        };

        // Act
        var result = useCase.CapNhatThongTin(dto);

        // Assert
        Assert.True(result);
        var updated = fakeSpRepo.TimTheoId("SP_03");
        Assert.NotNull(updated);
        Assert.Equal(15, updated.PhanTramGiamGia);
        Assert.Equal(14000000, updated.GiaBan);
    }

    [Fact]
    public void SanPhamTrangSuc_KhoiTaoSoLuongTon_MacDinhBangKhongVaCapNhatThanhCong()
    {
        // Arrange
        var sp = new SanPhamTrangSuc("SP_TON_01", "Nhẫn Kim Cương", "DM01", "Vàng trắng 18K", "Size 14", 10000000, 15000000);

        // Assert ban đầu
        Assert.Equal(0, sp.SoLuongTon);

        // Act
        sp.SoLuongTon = 5;

        // Assert sau cập nhật
        Assert.Equal(5, sp.SoLuongTon);
    }
}
