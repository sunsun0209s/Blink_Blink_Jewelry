using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Identity;

/// <summary>
/// Bộ kiểm thử đơn vị cho Use Case Đăng ký tài khoản khách hàng.
/// Kiểm thử các ràng buộc dữ liệu đầu vào, đặc biệt là quy chuẩn mật khẩu tối thiểu 6 ký tự.
/// </summary>
public class DangKyKhachHangUseCaseTests
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

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("12345")]
    [InlineData("  123  ")]
    public void Execute_MatKhauDuoi6KyTu_TraVeFalseVaKhongLuuKhachHang(string matKhauNgan)
    {
        // Arrange
        var fakeRepo = new FakeKhachHangRepository();
        var useCase = new DangKyKhachHangUseCase(fakeRepo);
        var request = new DangKyKhachHangDTO
        {
            HoTen = "Khách Hàng Test",
            SoDienThoai = "0901234567",
            TenDangNhap = "khachhangtest",
            MatKhau = matKhauNgan,
            NgaySinh = new DateTime(1995, 1, 1)
        };

        // Act
        var result = useCase.Execute(request);

        // Assert
        Assert.False(result);
        Assert.Empty(fakeRepo.Data);
    }

    [Fact]
    public void Execute_DuLieuRongHoacNull_TraVeFalse()
    {
        // Arrange
        var fakeRepo = new FakeKhachHangRepository();
        var useCase = new DangKyKhachHangUseCase(fakeRepo);

        // Act & Assert
        Assert.False(useCase.Execute(null!));

        var requestThieuMatKhau = new DangKyKhachHangDTO
        {
            HoTen = "Khách Hàng Test",
            SoDienThoai = "0901234567",
            TenDangNhap = "khachhangtest",
            MatKhau = "",
            NgaySinh = new DateTime(1995, 1, 1)
        };
        Assert.False(useCase.Execute(requestThieuMatKhau));
    }

    [Fact]
    public void Execute_ThongTinHopLeMatKhauTu6KyTu_TraVeTrueVaLuuKhachHang()
    {
        // Arrange
        var fakeRepo = new FakeKhachHangRepository();
        var useCase = new DangKyKhachHangUseCase(fakeRepo);
        var request = new DangKyKhachHangDTO
        {
            HoTen = "Nguyễn Văn Hợp Lệ",
            SoDienThoai = "0912345678",
            TenDangNhap = "nguyenvanhople",
            MatKhau = "matkhau123",
            NgaySinh = new DateTime(1998, 8, 18)
        };

        // Act
        var result = useCase.Execute(request);

        // Assert
        Assert.True(result);
        Assert.Single(fakeRepo.Data);
        var khachHangLuu = fakeRepo.Data.Values.First();
        Assert.Equal("Nguyễn Văn Hợp Lệ", khachHangLuu.HoTen);
        Assert.Equal("0912345678", khachHangLuu.SoDienThoai);
        Assert.Equal("nguyenvanhople", khachHangLuu.TenDangNhap);
        Assert.Equal("KHACHHANG", khachHangLuu.MaVaiTro);
        Assert.True(khachHangLuu.TrangThai);
    }
}
