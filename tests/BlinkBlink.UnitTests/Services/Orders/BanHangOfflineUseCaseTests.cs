using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Orders;
using BlinkBlink.Application.Services.Orders;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Orders;

public class BanHangOfflineUseCaseTests
{
    private class FakeHoaDonRepository : IHoaDonRepository
    {
        public readonly Dictionary<string, HoaDon> Data = new();
        public void LuuHoaDon(HoaDon hd) => Data[hd.MaHoaDon] = hd;
        public Task LuuHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default) { Data[hd.MaHoaDon] = hd; return Task.CompletedTask; }
        public HoaDon? TimTheoMa(string maHoaDon) => Data.TryGetValue(maHoaDon, out var hd) ? hd : null;
        public Task<HoaDon?> TimTheoMaAsync(string maHoaDon, CancellationToken cancellationToken = default) => Task.FromResult(TimTheoMa(maHoaDon));
        public List<HoaDon> LayTheoKhachHang(string maKH) => Data.Values.Where(h => h.MaKhachHang == maKH).ToList();
        public Task<List<HoaDon>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default) => Task.FromResult(LayTheoKhachHang(maKH));
        public List<HoaDon> LayDanhSachDonHang() => Data.Values.ToList();
        public Task<List<HoaDon>> LayDanhSachDonHangAsync(CancellationToken cancellationToken = default) => Task.FromResult(LayDanhSachDonHang());
        public void CapNhatHoaDon(HoaDon hd) => Data[hd.MaHoaDon] = hd;
        public Task CapNhatHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default) { Data[hd.MaHoaDon] = hd; return Task.CompletedTask; }
        public void CapNhatKhachHangTheoSoDienThoai(string soDienThoai, string maKhachHangMoi) { }
    }

    private class FakeSanPhamRepository : ISanPhamRepository
    {
        public readonly Dictionary<string, SanPhamTrangSuc> Data = new();
        public void LuuSanPham(SanPhamTrangSuc sp) => Data[sp.MaSanPham] = sp;
        public SanPhamTrangSuc? TimTheoId(string maSanPham) => Data.TryGetValue(maSanPham, out var sp) ? sp : null;
        public List<SanPhamTrangSuc> LayTatCa() => Data.Values.ToList();
        public List<SanPhamTrangSuc> LayTheoNhom(string maNhom) => Data.Values.Where(s => s.MaNhom == maNhom).ToList();
        public Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken cancellationToken = default) { Data[sp.MaSanPham] = sp; return Task.CompletedTask; }
        public Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken cancellationToken = default) => Task.FromResult(TimTheoId(maSanPham));
        public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken cancellationToken = default) => Task.FromResult(LayTatCa());
        public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken cancellationToken = default) => Task.FromResult(LayTheoNhom(maNhom));
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

    [Fact]
    public void Execute_ValidOfflineOrder_GeneratesOfflineCodeAndMarksPaid()
    {
        // Arrange
        var fakeHoaDon = new FakeHoaDonRepository();
        var fakeSanPham = new FakeSanPhamRepository();
        var fakeKhachHang = new FakeKhachHangRepository();

        var sp1 = new SanPhamTrangSuc(
            maSanPham: "SP_NHAN_01",
            tenSanPham: "Nhẫn Kim Cương Solitaire",
            maDanhMuc: "DM01",
            chatLieu: "Vàng trắng 18k",
            size: "12",
            giaNhap: 10000000m,
            giaBan: 20000000m,
            trangThai: "SanSang"
        );
        fakeSanPham.LuuSanPham(sp1);

        var kh = new KhachHang(
            maNguoiDung: "ND_01",
            hoTen: "Tran Thi B",
            soDienThoai: "0911223344",
            tenDangNhap: "tranthib",
            matKhau: "pass123",
            maVaiTro: "CUSTOMER",
            maKhachHang: "KH_01",
            ngaySinh: new DateTime(1995, 5, 20)
        );
        fakeKhachHang.LuuKhachHang(kh);

        var useCase = new BanHangOfflineUseCase(fakeHoaDon, fakeSanPham, fakeKhachHang);
        var request = new TaoHoaDonOfflineDTO
        {
            TenKhachHang = "Tran Thi B",
            SoDienThoai = "0911223344",
            DiaChi = "123 Le Loi, Q1",
            PhuongThucThanhToan = "TienMat",
            DanhSachSanPham = new List<ChiTietBanOfflineDTO>
            {
                new()
                {
                    MaSanPham = "SP_NHAN_01",
                    TenSanPham = "Nhẫn Kim Cương Solitaire",
                    DonGia = 20000000m,
                    SoLuong = 1,
                    PhanTramGiamGia = 10 // Giảm 10%
                }
            }
        };

        // Act
        var result = useCase.Execute(request, out var maHoaDonMoi);

        // Assert
        Assert.True(result);
        Assert.StartsWith("HD_OFFLINE_", maHoaDonMoi);

        var hoaDon = fakeHoaDon.TimTheoMa(maHoaDonMoi);
        Assert.NotNull(hoaDon);
        Assert.Equal("DaThanhToan", hoaDon.TrangThaiDonHang);
        Assert.Equal("KH_01", hoaDon.MaKhachHang); // Tự động link qua SĐT
        Assert.Equal(18000000m, hoaDon.TongThanhToan); // 20tr - 10% = 18tr

        // Kiểm tra sản phẩm đã chuyển trạng thái sang DaBan
        var spSauBan = fakeSanPham.TimTheoId("SP_NHAN_01");
        Assert.NotNull(spSauBan);
        Assert.Equal("DaBan", spSauBan.TrangThai);
    }

    [Fact]
    public void Execute_WalkInCustomerWithoutPhone_UsesDefaultCustomerZero()
    {
        // Arrange
        var fakeHoaDon = new FakeHoaDonRepository();
        var fakeSanPham = new FakeSanPhamRepository();
        var fakeKhachHang = new FakeKhachHangRepository();

        var sp = new SanPhamTrangSuc(
            maSanPham: "SP_DAY_01",
            tenSanPham: "Dây Chuyền Bạch Kim",
            maDanhMuc: "DM02",
            chatLieu: "Bạch kim",
            size: "45cm",
            giaNhap: 5000000m,
            giaBan: 8000000m,
            trangThai: "SanSang"
        );
        fakeSanPham.LuuSanPham(sp);

        var useCase = new BanHangOfflineUseCase(fakeHoaDon, fakeSanPham, fakeKhachHang);
        var request = new TaoHoaDonOfflineDTO
        {
            TenKhachHang = "Khách vãng lai",
            SoDienThoai = "", // Không có số điện thoại
            PhuongThucThanhToan = "ChuyenKhoan",
            DanhSachSanPham = new List<ChiTietBanOfflineDTO>
            {
                new() { MaSanPham = "SP_DAY_01", TenSanPham = "Dây Chuyền Bạch Kim", DonGia = 8000000m, SoLuong = 1, PhanTramGiamGia = 0 }
            }
        };

        // Act
        var result = useCase.Execute(request, out var maHoaDonMoi);

        // Assert
        Assert.True(result);
        var hoaDon = fakeHoaDon.TimTheoMa(maHoaDonMoi);
        Assert.NotNull(hoaDon);
        Assert.Equal("0", hoaDon.MaKhachHang); // Mã 0 chuẩn tránh lỗi DB constraint
        Assert.Equal("ChuyenKhoan", hoaDon.PhuongThucThanhToan);
    }
}
