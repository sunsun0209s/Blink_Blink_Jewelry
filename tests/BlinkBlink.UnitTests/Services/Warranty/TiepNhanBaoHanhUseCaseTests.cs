using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.DTOs.Warranty;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Warranty;

public class TiepNhanBaoHanhUseCaseTests
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
    public async Task TiepNhanBaoHanh_ThanhCong_TaoPhieuVaBaoToanTrangThaiSanPham()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP001", "Nhẫn kim cương Solitaire", "DM01", "Vàng trắng 18K", "Size 10", 15000000m, 25000000m, "DaBan");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "KH001",
            MaSanPham = "SP001",
            LyDoBaoHanh = "Gắn lại chấu đính kim cương và đánh bóng",
            ChiPhiBaoHanh = 0m,
            NgayHenTra = DateTime.UtcNow.AddDays(3)
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra
        Assert.True(result);
        Assert.Single(baoHanhRepo.Data);
        var phieu = baoHanhRepo.Data.Values.First();
        Assert.Equal("KH001", phieu.MaKhachHang);
        Assert.Equal("SP001", phieu.MaSanPham);
        Assert.Equal("TiepNhan", phieu.TrangThaiXuLy);
        Assert.Equal(0m, phieu.ChiPhiBaoHanh);
        // Trạng thái kinh doanh của sản phẩm được bảo toàn, không bị ép thành DangBaoHanh
        Assert.Equal("DaBan", sp.TrangThai);
    }

    [Fact]
    public async Task TiepNhanBaoHanh_SanPhamDangKinhDoanhConTonKho_BaoToanTrangThaiSanSang()
    {
        // Sắp đặt: Sản phẩm SP007 đang còn 5 chiếc trong kho để bán, trạng thái SanSang
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP007", "Dây chuyền vàng Ý", "DM02", "Vàng 18K", "45cm", 10000000m, 15000000m, "SanSang")
        {
            SoLuongTon = 5
        };
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "KH002",
            MaSanPham = "SP007",
            LyDoBaoHanh = "Đánh bóng siêu âm và làm mới",
            ChiPhiBaoHanh = 0m
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra: Phiếu tạo thành công nhưng trạng thái kinh doanh của SP007 trong kho vẫn là SanSang, tồn kho vẫn là 5
        Assert.True(result);
        Assert.Equal("SanSang", sp.TrangThai);
        Assert.Equal(5, sp.SoLuongTon);
    }

    [Fact]
    public async Task TiepNhanBaoHanh_ThatBai_KhiSanPhamKhongTonTai()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "KH001",
            MaSanPham = "SP_KHONG_TON_TAI",
            LyDoBaoHanh = "Chỉnh size"
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra
        Assert.False(result);
        Assert.Empty(baoHanhRepo.Data);
    }

    [Fact]
    public async Task TiepNhanBaoHanh_ThatBai_KhiThieuLyDoBaoHanh()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP001", "Dây chuyền vàng Ý", "DM02", "Vàng 24K", "Size 45cm", 8000000m, 12000000m, "DaBan");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "KH001",
            MaSanPham = "SP001",
            LyDoBaoHanh = "   " // Rỗng
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra
        Assert.False(result);
        Assert.Empty(baoHanhRepo.Data);
    }

    [Fact]
    public async Task TiepNhanBaoHanh_KhachVangLai_ThanhCong_KhongCanMaKhachHang()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP005", "Nhẫn kim cương Solitaire", "DM01", "Vàng trắng 18K", "Size 12", 20000000m, 32000000m, "DaBan");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "", // Khách vãng lai chưa có mã tài khoản
            MaSanPham = "SP005",
            LyDoBaoHanh = "Gắn lại chấu kim cương",
            SoDienThoai = "0988776655"
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra: Tạo thành công, gán mã khách vãng lai "0" và lưu số điện thoại
        Assert.True(result);
        Assert.Single(baoHanhRepo.Data);
        var ticket = baoHanhRepo.Data.Values.First();
        Assert.Equal("0", ticket.MaKhachHang);
        Assert.Equal("0988776655", ticket.SoDienThoai);
        Assert.Equal("TiepNhan", ticket.TrangThaiXuLy);
    }

    [Fact]
    public async Task TiepNhanBaoHanh_KhachVangLai_BoTrongSoDienThoai_VanThanhCong()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();

        var sp = new SanPhamTrangSuc("SP006", "Lắc tay vàng hồng", "DM03", "Vàng hồng 18K", "Size 16cm", 15000000m, 22000000m, "DaBan");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "0", // Khách vãng lai
            MaSanPham = "SP006",
            LyDoBaoHanh = "Đánh bóng làm mới",
            SoDienThoai = "" // Khách không để lại SĐT
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra
        Assert.True(result);
        var ticket = baoHanhRepo.Data.Values.First();
        Assert.Equal("0", ticket.MaKhachHang);
        Assert.Empty(ticket.SoDienThoai);
    }

    [Fact]
    public async Task TiepNhanBaoHanh_KhachThanhVien_MapSangMaNguoiDung_ThanhCong()
    {
        // Sắp đặt
        var baoHanhRepo = new FakeBaoHanhRepository();
        var sanPhamRepo = new FakeSanPhamRepository();
        var khachHangRepo = new FakeKhachHangRepository();

        var kh = new KhachHang { MaKhachHang = "KH001", MaNguoiDung = "USER_KH001", HoTen = "Trần Văn An", SoDienThoai = "0912345678" };
        khachHangRepo.Data[kh.MaKhachHang] = kh;

        var sp = new SanPhamTrangSuc("SP001", "Nhẫn Kim Cương", "DM01", "Vàng trắng", "14", 10000000m, 15000000m, "DaBan");
        sanPhamRepo.Data[sp.MaSanPham] = sp;

        var useCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo, khachHangRepo);
        var request = new TaoBaoHanhDTO
        {
            MaKhachHang = "KH001", // Truyền mã khách hàng nghiệp vụ
            MaSanPham = "SP001",
            LyDoBaoHanh = "Gắn lại chấu đá",
            SoDienThoai = "0912345678"
        };

        // Thực thi
        var result = await useCase.ExecuteAsync(request);

        // Kiểm tra: Đã tự động map sang USER_KH001 để khớp FK NguoiDung
        Assert.True(result);
        var ticket = baoHanhRepo.Data.Values.First();
        Assert.Equal("USER_KH001", ticket.MaKhachHang);
    }

    [Fact]
    public void DangKyKhachHang_TuDongLienKetPhieuBaoHanhCu_CungSoDienThoai_BoQuaPhieuKhongCoSDT()
    {
        // Sắp đặt: Có 2 phiếu bảo hành khách vãng lai trước đó
        var baoHanhRepo = new FakeBaoHanhRepository();
        var khachHangRepo = new FakeKhachHangRepository();

        var ticket1 = new PhieuBaoHanh("BH_TEST_01", "0", "SP001", "Làm sạch", soDienThoai: "0933221100");
        var ticket2 = new PhieuBaoHanh("BH_TEST_02", "0", "SP002", "Xi mạ", soDienThoai: ""); // Không có SĐT
        var ticket3 = new PhieuBaoHanh("BH_TEST_03", "0", "SP003", "Hàn móc", soDienThoai: "0999999999"); // SĐT khác

        baoHanhRepo.LuuPhieu(ticket1);
        baoHanhRepo.LuuPhieu(ticket2);
        baoHanhRepo.LuuPhieu(ticket3);

        // Khi khách hàng đăng ký tài khoản với số điện thoại 0933221100
        var registerUseCase = new DangKyKhachHangUseCase(khachHangRepo, null, baoHanhRepo);
        var dto = new DangKyKhachHangDTO
        {
            HoTen = "Nguyễn Văn Khách",
            SoDienThoai = "0933221100",
            TenDangNhap = "khach.moi",
            MatKhau = "123456789"
        };

        // Thực thi
        var ok = registerUseCase.Execute(dto);

        // Kiểm tra
        Assert.True(ok);
        var createdKh = khachHangRepo.Data.Values.First();

        // Phiếu 1 có cùng SĐT -> Tự động chuyển sang mã khách hàng mới
        Assert.Equal(createdKh.MaKhachHang, ticket1.MaKhachHang);

        // Phiếu 2 (không có SĐT) và Phiếu 3 (khác SĐT) -> Giữ nguyên mã "0"
        Assert.Equal("0", ticket2.MaKhachHang);
        Assert.Equal("0", ticket3.MaKhachHang);
    }

    private class FakeKhachHangRepository : IKhachHangRepository
    {
        public readonly Dictionary<string, KhachHang> Data = new();
        public void LuuKhachHang(KhachHang kh) => Data[kh.MaKhachHang] = kh;
        public KhachHang? TimTheoMa(string maKH) => Data.TryGetValue(maKH, out var kh) ? kh : null;
        public KhachHang? TimTheoSoDienThoai(string soDienThoai) => Data.Values.FirstOrDefault(k => k.SoDienThoai == soDienThoai);
        public List<KhachHang> LayTatCa() => Data.Values.ToList();
        public bool KiemTraCoTheXoa(string maKH) => true;
        public bool XoaKhachHang(string maKH) => Data.Remove(maKH);
    }
}
