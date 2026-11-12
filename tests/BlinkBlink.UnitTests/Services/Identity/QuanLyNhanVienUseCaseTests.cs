using System;
using System.Collections.Generic;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Identity;

public class QuanLyNhanVienUseCaseTests
{
    private class FakeNhanVienRepository : INhanVienRepository
    {
        public readonly Dictionary<string, NhanVien> Data = new();

        public void LuuNhanVien(NhanVien nv)
        {
            Data[nv.MaNhanVien] = nv;
        }

        public NhanVien? TimTheoMa(string maNV)
        {
            return Data.TryGetValue(maNV, out var nv) ? nv : null;
        }

        public List<NhanVien> LayTatCa()
        {
            return new List<NhanVien>(Data.Values);
        }

        public void CapNhatNhanVien(NhanVien nv)
        {
            Data[nv.MaNhanVien] = nv;
        }

        public bool KiemTraCoTheXoa(string maNV) => true;

        public bool XoaNhanVien(string maNV)
        {
            return Data.Remove(maNV);
        }
    }

    private class FakeVaiTroRepository : IVaiTroRepository
    {
        public readonly Dictionary<string, VaiTro> Data = new();

        public void LuuVaiTro(VaiTro vt)
        {
            Data[vt.MaVaiTro] = vt;
        }

        public VaiTro? TimTheoMa(string maVT)
        {
            return Data.TryGetValue(maVT, out var vt) ? vt : null;
        }

        public List<VaiTro> LayTatCa()
        {
            return new List<VaiTro>(Data.Values);
        }
    }

    [Fact]
    public void ResetMatKhau_ValidEmployee_ResetsPasswordToDefault()
    {
        // Arrange
        var fakeNvRepo = new FakeNhanVienRepository();
        var fakeVtRepo = new FakeVaiTroRepository();
        var employee = new NhanVien(
            maNguoiDung: "ND_NV01",
            hoTen: "Nhan Vien C",
            soDienThoai: "0933112233",
            tenDangNhap: "nhanvienc",
            matKhau: "OldPasswordSecret",
            maVaiTro: "Staff",
            maNhanVien: "NV_01",
            luongCoBan: 10000000
        );
        fakeNvRepo.LuuNhanVien(employee);

        var useCase = new QuanLyNhanVienUseCase(fakeNvRepo, fakeVtRepo);

        // Act
        var result = useCase.ResetMatKhau("NV_01");

        // Assert
        Assert.True(result);
        var updated = fakeNvRepo.TimTheoMa("NV_01");
        Assert.NotNull(updated);
        Assert.Equal("123456789", updated.MatKhau);
    }

    [Fact]
    public void ResetMatKhau_NonExistingEmployee_ReturnsFalse()
    {
        // Arrange
        var fakeNvRepo = new FakeNhanVienRepository();
        var fakeVtRepo = new FakeVaiTroRepository();
        var useCase = new QuanLyNhanVienUseCase(fakeNvRepo, fakeVtRepo);

        // Act
        var result = useCase.ResetMatKhau("NV_NONEXIST");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void QuanLyNhanVien_DemSoLuongAdmin_ChinhXacChoCaAdminVaVT01()
    {
        // Arrange
        var fakeNvRepo = new FakeNhanVienRepository();
        fakeNvRepo.LuuNhanVien(new NhanVien("ND1", "Admin Chính", "0911", "admin", "pass", "ADMIN", "NV01"));
        fakeNvRepo.LuuNhanVien(new NhanVien("ND2", "Admin Cũ", "0922", "admin_old", "pass", "VT01", "NV02"));
        fakeNvRepo.LuuNhanVien(new NhanVien("ND3", "Nhân Viên Sale", "0933", "sale", "pass", "SALE", "NV03"));
        fakeNvRepo.LuuNhanVien(new NhanVien("ND4", "Nhân Viên Kho", "0944", "warehouse", "pass", "WAREHOUSE", "NV04"));

        var allStaff = fakeNvRepo.LayTatCa();

        // Act
        var adminCount = allStaff.Count(n => 
            string.Equals(n.MaVaiTro, "ADMIN", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(n.MaVaiTro, "VT01", StringComparison.OrdinalIgnoreCase)
        );

        // Assert
        Assert.Equal(2, adminCount);
        Assert.Equal(4, allStaff.Count);
    }
}
