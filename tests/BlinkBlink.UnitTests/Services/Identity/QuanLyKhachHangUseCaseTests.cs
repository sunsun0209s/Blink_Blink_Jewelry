using System;
using System.Collections.Generic;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Identity;

public class QuanLyKhachHangUseCaseTests
{
    private class FakeKhachHangRepository : IKhachHangRepository
    {
        public readonly Dictionary<string, KhachHang> Data = new();

        public void LuuKhachHang(KhachHang kh)
        {
            Data[kh.MaKhachHang] = kh;
        }

        public KhachHang? TimTheoMa(string maKH)
        {
            return Data.TryGetValue(maKH, out var kh) ? kh : null;
        }

        public KhachHang? TimTheoSoDienThoai(string soDienThoai)
        {
            return Data.Values.FirstOrDefault(k => k.SoDienThoai == soDienThoai);
        }

        public List<KhachHang> LayTatCa()
        {
            return new List<KhachHang>(Data.Values);
        }

        public bool KiemTraCoTheXoa(string maKH) => true;

        public bool XoaKhachHang(string maKH)
        {
            return Data.Remove(maKH);
        }
    }

    [Fact]
    public void CapNhatThongTin_ValidRequest_UpdatesCustomerAndReturnsTrue()
    {
        // Arrange
        var fakeRepo = new FakeKhachHangRepository();
        var initialCustomer = new KhachHang(
            maNguoiDung: "ND_01",
            hoTen: "Nguyen Van A",
            soDienThoai: "0901234567",
            tenDangNhap: "nguyenvana",
            matKhau: "hash123",
            maVaiTro: "Customer",
            maKhachHang: "KH_01",
            ngaySinh: new DateTime(1995, 5, 20)
        );
        fakeRepo.LuuKhachHang(initialCustomer);

        var useCase = new QuanLyKhachHangUseCase(fakeRepo);
        var request = new CapNhatKhachHangDTO
        {
            MaKhachHang = "KH_01",
            HoTen = "Nguyen Van A Cap Nhat",
            SoDienThoai = "0987654321",
            NgaySinh = new DateTime(1996, 6, 21)
        };

        // Act
        var result = useCase.CapNhatThongTin(request);

        // Assert
        Assert.True(result);
        var updated = fakeRepo.TimTheoMa("KH_01");
        Assert.NotNull(updated);
        Assert.Equal("Nguyen Van A Cap Nhat", updated.HoTen);
        Assert.Equal("0987654321", updated.SoDienThoai);
        Assert.Equal(new DateTime(1996, 6, 21), updated.NgaySinh);
    }

    [Fact]
    public void CapNhatThongTin_CustomerNotFound_ReturnsFalse()
    {
        // Arrange
        var fakeRepo = new FakeKhachHangRepository();
        var useCase = new QuanLyKhachHangUseCase(fakeRepo);
        var request = new CapNhatKhachHangDTO
        {
            MaKhachHang = "KH_NONEXIST",
            HoTen = "Test",
            SoDienThoai = "0123456789"
        };

        // Act
        var result = useCase.CapNhatThongTin(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void KhoaTaiKhoanKhachHang_ValidCustomer_LocksAccount()
    {
        // Arrange
        var fakeRepo = new FakeKhachHangRepository();
        var initialCustomer = new KhachHang(
            maNguoiDung: "ND_02",
            hoTen: "Tran Thi B",
            soDienThoai: "0912345678",
            tenDangNhap: "tranthib",
            matKhau: "hash456",
            maVaiTro: "Customer",
            maKhachHang: "KH_02",
            ngaySinh: new DateTime(2000, 1, 1),
            trangThai: true
        );
        fakeRepo.LuuKhachHang(initialCustomer);

        var useCase = new QuanLyKhachHangUseCase(fakeRepo);

        // Act
        var result = useCase.KhoaTaiKhoanKhachHang("KH_02");

        // Assert
        Assert.True(result);
        var updated = fakeRepo.TimTheoMa("KH_02");
        Assert.NotNull(updated);
        Assert.False(updated.TrangThai);
    }
}
