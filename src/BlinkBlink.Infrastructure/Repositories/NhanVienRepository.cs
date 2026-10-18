using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Nhân viên thông qua Entity Framework Core.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NhanVienRepository
/// </summary>
public class NhanVienRepository : INhanVienRepository
{
    private readonly AppDbContext _dbContext;

    public NhanVienRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Lưu mới hoặc cập nhật thông tin hồ sơ nhân viên vào cơ sở dữ liệu
    /// </summary>
    /// <param name="nv">Thực thể nhân viên cần lưu</param>
    public void LuuNhanVien(NhanVien nv)
    {
        if (nv == null)
        {
            return;
        }

        var existing = _dbContext.NhanViens.Find(nv.MaNguoiDung);
        if (existing == null)
        {
            _dbContext.NhanViens.Add(nv);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(nv);
        }

        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Tìm kiếm nhân viên theo mã nhân viên nội bộ
    /// </summary>
    /// <param name="maNV">Mã nhân viên (ví dụ: NV001)</param>
    /// <returns>Đối tượng NhanVien kèm VaiTro hoặc null</returns>
    public NhanVien? TimTheoMa(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV))
        {
            return null;
        }

        return _dbContext.NhanViens
            .Include(nv => nv.VaiTro)
            .FirstOrDefault(nv => nv.MaNhanVien == maNV.Trim());
    }
}
