using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Người dùng thông qua Entity Framework Core.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NguoiDungRepository
/// </summary>
public class NguoiDungRepository : INguoiDungRepository
{
    private readonly AppDbContext _dbContext;

    public NguoiDungRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Tìm người dùng theo tên đăng nhập kèm thông tin Vai trò
    /// </summary>
    /// <param name="tenDN">Tên đăng nhập hệ thống</param>
    /// <returns>Đối tượng NguoiDung hoặc null nếu không tồn tại</returns>
    public NguoiDung? TimTheoTenDangNhap(string tenDN)
    {
        if (string.IsNullOrWhiteSpace(tenDN))
        {
            return null;
        }

        return _dbContext.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefault(u => u.TenDangNhap == tenDN.Trim());
    }

    /// <summary>
    /// Cập nhật thông tin hoặc trạng thái người dùng vào SQL Server
    /// </summary>
    /// <param name="nd">Thực thể người dùng</param>
    public void CapNhat(NguoiDung nd)
    {
        if (nd == null)
        {
            return;
        }

        _dbContext.NguoiDungs.Update(nd);
        _dbContext.SaveChanges();
    }
}
