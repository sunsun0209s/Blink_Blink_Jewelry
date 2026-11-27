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

        public bool LuuSanPham(SanPhamTrangSuc sp)
        {
            Data[sp.MaSanPham] = sp;
            return true;
        }

        public Task<bool> LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken cancellationToken = default)
        {
            Data[sp.MaSanPham] = sp;
            return Task.FromResult(true);
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

        public HashSet<string> ProductsWithTransactions = new();

        public bool KiemTraPhatSinhGiaoDich(string maSanPham, out string lyDo)
        {
            if (ProductsWithTransactions.Contains(maSanPham))
            {
                lyDo = "Sản phẩm đã phát sinh giao dịch trong hệ thống.";
                return true;
            }
            lyDo = string.Empty;
            return false;
        }

        public bool XoaSanPham(string maSanPham)
        {
            return Data.Remove(maSanPham);
        }

        public Task<(bool CoGiaoDich, string LyDo)> KiemTraPhatSinhGiaoDichAsync(string maSanPham, CancellationToken cancellationToken = default)
        {
            var hasTx = KiemTraPhatSinhGiaoDich(maSanPham, out var lyDo);
            return Task.FromResult((hasTx, lyDo));
        }

        public Task<bool> XoaSanPhamAsync(string maSanPham, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(XoaSanPham(maSanPham));
        }
    }

    private class FakeDanhMucRepository : IDanhMucRepository
    {
        public readonly Dictionary<string, DanhMuc> Data = new();
        private readonly FakeSanPhamRepository? _spRepo;

        public FakeDanhMucRepository(FakeSanPhamRepository? spRepo = null)
        {
            _spRepo = spRepo;
        }

        public bool LuuDanhMuc(DanhMuc dm)
        {
            Data[dm.MaDanhMuc] = dm;
            return true;
        }

        public Task<bool> LuuDanhMucAsync(DanhMuc dm, CancellationToken cancellationToken = default)
        {
            Data[dm.MaDanhMuc] = dm;
            return Task.FromResult(true);
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

        public bool XoaDanhMuc(string maDanhMuc)
        {
            if (string.Equals(maDanhMuc, "DM00", StringComparison.OrdinalIgnoreCase)) return false;
            var removed = Data.Remove(maDanhMuc);
            if (_spRepo != null)
            {
                foreach (var sp in _spRepo.Data.Values.Where(s => string.Equals(s.MaDanhMuc, maDanhMuc, StringComparison.OrdinalIgnoreCase)))
                {
                    sp.MaDanhMuc = "DM00";
                }
            }
            return removed;
        }

        public Task<bool> XoaDanhMucAsync(string maDanhMuc, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(XoaDanhMuc(maDanhMuc));
        }
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
        Assert.Equal("ChuaNhap", created.TrangThai); // Ban đầu chưa nhập hàng
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
        Assert.Equal("ChuaNhap", created.TrangThai);
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

    [Fact]
    public void XoaDanhMuc_ChuyenSanPhamThuocDanhMucBiXoaSangDM00_VaChanXoaDM00()
    {
        // Sắp đặt
        var fakeSpRepo = new FakeSanPhamRepository();
        var fakeDmRepo = new FakeDanhMucRepository(fakeSpRepo);

        var dmChuaPhanLoai = new DanhMuc("DM00", "Chưa phân loại", "Mặc định");
        var dmNhan = new DanhMuc("DM_NHAN", "Nhẫn thời trang", "Danh mục nhẫn");
        fakeDmRepo.LuuDanhMuc(dmChuaPhanLoai);
        fakeDmRepo.LuuDanhMuc(dmNhan);

        var sp1 = new SanPhamTrangSuc("SP_01", "Nhẫn vàng 18K", "DM_NHAN", "Vàng 18K", "12", 10000000, 15000000);
        var sp2 = new SanPhamTrangSuc("SP_02", "Nhẫn kim cương", "DM_NHAN", "Bạch kim", "14", 20000000, 30000000);
        fakeSpRepo.LuuSanPham(sp1);
        fakeSpRepo.LuuSanPham(sp2);

        // Hành động 1: Xóa DM_NHAN
        fakeDmRepo.XoaDanhMuc("DM_NHAN");

        // Kiểm tra 1: DM_NHAN đã bị xóa, hai sản phẩm được chuyển sang DM00
        Assert.Null(fakeDmRepo.TimTheoId("DM_NHAN"));
        Assert.Equal("DM00", fakeSpRepo.TimTheoId("SP_01")!.MaDanhMuc);
        Assert.Equal("DM00", fakeSpRepo.TimTheoId("SP_02")!.MaDanhMuc);

        // Hành động 2: Cố gắng xóa DM00
        fakeDmRepo.XoaDanhMuc("DM00");

        // Kiểm tra 2: DM00 được bảo vệ và không bị xóa
        Assert.NotNull(fakeDmRepo.TimTheoId("DM00"));
    }

    [Fact]
    public void SanPhamTrangSuc_KhoiTaoIsHidden_MacDinhFalseVaChoPhepCapNhatAnHien()
    {
        // Sắp đặt
        var sp = new SanPhamTrangSuc("SP_AN_01", "Dây Chuyền Ngọc Trai", "DM02", "Vàng 18K", "Size 45cm", 15000000, 22000000);

        // Kiểm tra mặc định là không ẩn (IsHidden = false)
        Assert.False(sp.IsHidden);

        // Hành động: Đổi trạng thái sang ẩn
        sp.IsHidden = true;

        // Kiểm tra đã được đánh dấu ẩn thành công
        Assert.True(sp.IsHidden);
    }

    [Fact]
    public void QuanLySanPham_LocBoSanPhamBiAn_KhongXuatHienKhiBanHangVaStorefront()
    {
        // Sắp đặt: Có 2 sản phẩm (1 hiện, 1 bị ẩn)
        var spHien = new SanPhamTrangSuc("SP_HIEN", "Nhẫn Kim Cương", "DM01", "Vàng trắng", "14", 10000000, 15000000, "SanSang", isHidden: false) { SoLuongTon = 5 };
        var spAn = new SanPhamTrangSuc("SP_AN", "Lắc Tay Bạc", "DM04", "Bạc 925", "16", 2000000, 3000000, "SanSang", isHidden: true) { SoLuongTon = 3 };

        var danhSachTatCa = new List<SanPhamTrangSuc> { spHien, spAn };

        // Hành động: Lọc danh sách sản phẩm hiển thị trên Web và POS Bán hàng Offline
        var danhSachKhaDung = danhSachTatCa.Where(p => !p.IsHidden && p.SoLuongTon > 0).ToList();

        // Kiểm tra: Chỉ có sản phẩm hiển thị (spHien), sản phẩm ẩn bị loại trừ
        Assert.Single(danhSachKhaDung);
        Assert.Equal("SP_HIEN", danhSachKhaDung[0].MaSanPham);
        Assert.DoesNotContain(danhSachKhaDung, p => p.MaSanPham == "SP_AN");
    }

    [Fact]
    public void XoaSanPham_ThanhCong_KhiChuaPhatSinhGiaoDich()
    {
        // Sắp đặt: Sản phẩm SP_NEW chưa từng phát sinh chứng từ hay giao dịch
        var spRepo = new FakeSanPhamRepository();
        var dmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(spRepo, dmRepo);

        var sp = new SanPhamTrangSuc("SP_NEW", "Nhẫn mới tạo", "DM01", "Vàng 18K", "12", 5000000, 7500000, "ChuaNhap");
        spRepo.LuuSanPham(sp);

        // Hành động: Xóa sản phẩm
        var result = useCase.XoaSanPham("SP_NEW", out var thongBao);

        // Kiểm tra: Xóa thành công và không còn trong repo
        Assert.True(result);
        Assert.Contains("Đã xóa vĩnh viễn", thongBao);
        Assert.Null(spRepo.TimTheoId("SP_NEW"));
    }

    [Fact]
    public void XoaSanPham_BiChan_KhiDaPhatSinhGiaoDich()
    {
        // Sắp đặt: Sản phẩm SP_OLD đã từng phát sinh giao dịch nhập kho / bán hàng
        var spRepo = new FakeSanPhamRepository();
        var dmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(spRepo, dmRepo);

        var sp = new SanPhamTrangSuc("SP_OLD", "Nhẫn kim cương đã bán", "DM01", "Bạch kim", "14", 10000000, 15000000, "SanSang");
        spRepo.LuuSanPham(sp);
        spRepo.ProductsWithTransactions.Add("SP_OLD");

        // Hành động: Cố gắng xóa sản phẩm đã có giao dịch
        var result = useCase.XoaSanPham("SP_OLD", out var thongBao);

        // Kiểm tra: Bị từ chối xóa và sản phẩm vẫn còn nguyên
        Assert.False(result);
        Assert.Contains("Sản phẩm chỉ có thể ẨN khỏi hệ thống thay vì xóa", thongBao);
        Assert.NotNull(spRepo.TimTheoId("SP_OLD"));
    }

    [Fact]
    public async Task XoaSanPhamAsync_ThanhCong_KhiChuaPhatSinhGiaoDich()
    {
        // Sắp đặt: Sản phẩm SP_ASYNC chưa từng phát sinh chứng từ
        var spRepo = new FakeSanPhamRepository();
        var dmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(spRepo, dmRepo);

        var sp = new SanPhamTrangSuc("SP_ASYNC", "Dây chuyền vàng", "DM02", "Vàng 24K", "45cm", 12000000, 18000000, "ChuaNhap");
        await spRepo.LuuSanPhamAsync(sp);

        // Hành động: Xóa bất đồng bộ
        var (success, msg) = await useCase.XoaSanPhamAsync("SP_ASYNC");

        // Kiểm tra: Thành công
        Assert.True(success);
        Assert.Contains("Đã xóa vĩnh viễn", msg);
        Assert.Null(await spRepo.TimTheoIdAsync("SP_ASYNC"));
    }

    [Fact]
    public async Task XoaSanPhamAsync_BiChan_KhiDaPhatSinhGiaoDich()
    {
        // Sắp đặt: Sản phẩm SP_ASYNC_TX đã phát sinh giao dịch
        var spRepo = new FakeSanPhamRepository();
        var dmRepo = new FakeDanhMucRepository();
        var useCase = new QuanLySanPhamUseCase(spRepo, dmRepo);

        var sp = new SanPhamTrangSuc("SP_ASYNC_TX", "Lắc tay vàng", "DM03", "Vàng trắng", "17cm", 8000000, 12000000, "SanSang");
        await spRepo.LuuSanPhamAsync(sp);
        spRepo.ProductsWithTransactions.Add("SP_ASYNC_TX");

        // Hành động: Cố gắng xóa
        var (success, msg) = await useCase.XoaSanPhamAsync("SP_ASYNC_TX");

        // Kiểm tra: Bị từ chối
        Assert.False(success);
        Assert.Contains("Sản phẩm chỉ có thể ẨN khỏi hệ thống thay vì xóa", msg);
        Assert.NotNull(await spRepo.TimTheoIdAsync("SP_ASYNC_TX"));
    }
}
