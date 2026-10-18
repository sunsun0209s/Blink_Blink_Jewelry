using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Khách hàng thông qua Entity Framework Core.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - KhachHangRepository
/// </summary>
public class KhachHangRepository : IKhachHangRepository
{
    private readonly AppDbContext _dbContext;

    public KhachHangRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Lưu mới hoặc cập nhật hồ sơ khách hàng vào cơ sở dữ liệu
    /// </summary>
    /// <param name="kh">Thực thể khách hàng cần lưu</param>
    public void LuuKhachHang(KhachHang kh)
    {
        if (kh == null)
        {
            return;
        }

        var existing = _dbContext.KhachHangs.Find(kh.MaNguoiDung);
        if (existing == null)
        {
            _dbContext.KhachHangs.Add(kh);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(kh);
        }

        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Tìm kiếm khách hàng theo mã khách hàng
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <returns>Đối tượng KhachHang hoặc null</returns>
    public KhachHang? TimTheoMa(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return null;
        }

        return _dbContext.KhachHangs
            .Include(kh => kh.VaiTro)
            .FirstOrDefault(kh => kh.MaKhachHang == maKH.Trim());
    }
}
