using System;
using System.Collections.Generic;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Identity;

/// <summary>
/// Bộ kiểm thử đơn vị cho Use Case Đổi mật khẩu tài khoản người dùng.
/// </summary>
public class DoiMatKhauUseCaseTests
{
    private class FakeNguoiDungRepository : INguoiDungRepository
    {
        public readonly Dictionary<string, NguoiDung> Data = new(StringComparer.OrdinalIgnoreCase);

        public NguoiDung? TimTheoTenDangNhap(string tenDN)
        {
            return Data.TryGetValue(tenDN.Trim(), out var user) ? user : null;
        }

        public void CapNhat(NguoiDung nd)
        {
            Data[nd.TenDangNhap] = nd;
        }
    }

    private static KhachHang TaoKhachHangMau(string tenDangNhap = "khach1", string matKhau = "123456", bool trangThai = true)
    {
        return new KhachHang(
            maNguoiDung: "ND_001",
            hoTen: "Nguyễn Văn A",
            soDienThoai: "0901234567",
            tenDangNhap: tenDangNhap,
            matKhau: matKhau,
            maVaiTro: "KHACHHANG",
            maKhachHang: "KH_001",
            ngaySinh: new DateTime(1995, 1, 1),
            trangThai: trangThai
        );
    }

    [Fact]
    public void Execute_ThongTinHopLe_DoiMatKhauThanhCong()
    {
        var repo = new FakeNguoiDungRepository();
        var user = TaoKhachHangMau("qhy", "oldpass123");
        repo.Data[user.TenDangNhap] = user;

        var useCase = new DoiMatKhauUseCase(repo);
        var result = useCase.Execute(new DoiMatKhauDTO
        {
            TenDangNhap = "qhy",
            MatKhauHienTai = "oldpass123",
            MatKhauMoi = "newpass123"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("newpass123", repo.Data["qhy"].MatKhau);
    }

    [Fact]
    public void Execute_SaiMatKhauHienTai_TraVeInvalidCredentials()
    {
        var repo = new FakeNguoiDungRepository();
        var user = TaoKhachHangMau("qhy", "oldpass123");
        repo.Data[user.TenDangNhap] = user;

        var useCase = new DoiMatKhauUseCase(repo);
        var result = useCase.Execute(new DoiMatKhauDTO
        {
            TenDangNhap = "qhy",
            MatKhauHienTai = "wrongpass",
            MatKhauMoi = "newpass123"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidCredentials, result.ErrorCode);
        Assert.Equal("oldpass123", repo.Data["qhy"].MatKhau);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("  123  ")]
    public void Execute_MatKhauMoiDuoi6KyTu_TraVeInvalidInput(string matKhauNgan)
    {
        var repo = new FakeNguoiDungRepository();
        var user = TaoKhachHangMau("qhy", "oldpass123");
        repo.Data[user.TenDangNhap] = user;

        var useCase = new DoiMatKhauUseCase(repo);
        var result = useCase.Execute(new DoiMatKhauDTO
        {
            TenDangNhap = "qhy",
            MatKhauHienTai = "oldpass123",
            MatKhauMoi = matKhauNgan
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidInput, result.ErrorCode);
        Assert.Equal("oldpass123", repo.Data["qhy"].MatKhau);
    }

    [Fact]
    public void Execute_MatKhauMoiTrungMatKhauCu_TraVeInvalidInput()
    {
        var repo = new FakeNguoiDungRepository();
        var user = TaoKhachHangMau("qhy", "oldpass123");
        repo.Data[user.TenDangNhap] = user;

        var useCase = new DoiMatKhauUseCase(repo);
        var result = useCase.Execute(new DoiMatKhauDTO
        {
            TenDangNhap = "qhy",
            MatKhauHienTai = "oldpass123",
            MatKhauMoi = "oldpass123"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidInput, result.ErrorCode);
    }

    [Fact]
    public void Execute_TaiKhoanBiKhoa_TraVeAccountLocked()
    {
        var repo = new FakeNguoiDungRepository();
        var user = TaoKhachHangMau("qhy", "oldpass123", trangThai: false);
        repo.Data[user.TenDangNhap] = user;

        var useCase = new DoiMatKhauUseCase(repo);
        var result = useCase.Execute(new DoiMatKhauDTO
        {
            TenDangNhap = "qhy",
            MatKhauHienTai = "oldpass123",
            MatKhauMoi = "newpass123"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.AccountLocked, result.ErrorCode);
    }

    [Fact]
    public void Execute_KhongTimThayNguoiDung_TraVeResourceNotFound()
    {
        var repo = new FakeNguoiDungRepository();
        var useCase = new DoiMatKhauUseCase(repo);

        var result = useCase.Execute(new DoiMatKhauDTO
        {
            TenDangNhap = "khong_ton_tai",
            MatKhauHienTai = "oldpass123",
            MatKhauMoi = "newpass123"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ResourceNotFound, result.ErrorCode);
    }
}
