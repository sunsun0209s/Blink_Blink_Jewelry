using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Vai trò phân quyền thông qua Entity Framework Core.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - VaiTroRepository
/// </summary>
public class VaiTroRepository : IVaiTroRepository
{
    private readonly AppDbContext _dbContext;

    public VaiTroRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Tìm vai trò theo mã định danh
    /// </summary>
    /// <param name="maVaiTro">Mã vai trò (ví dụ: ADMIN, SALE)</param>
    /// <returns>Đối tượng VaiTro hoặc null</returns>
    public VaiTro? TimTheoMa(string maVaiTro)
    {
        if (string.IsNullOrWhiteSpace(maVaiTro))
        {
            return null;
        }

        return _dbContext.VaiTros.Find(maVaiTro.Trim());
    }

    /// <summary>
    /// Lấy toàn bộ danh sách vai trò hiện có trong hệ thống
    /// </summary>
    /// <returns>Danh sách các đối tượng VaiTro</returns>
    public List<VaiTro> LayTatCa()
    {
        return _dbContext.VaiTros.ToList();
    }

    /// <summary>
    /// Lưu mới hoặc cập nhật vai trò vào cơ sở dữ liệu
    /// </summary>
    /// <param name="vt">Thực thể vai trò</param>
    public void LuuVaiTro(VaiTro vt)
    {
        if (vt == null)
        {
            return;
        }

        var existing = _dbContext.VaiTros.Find(vt.MaVaiTro);
        if (existing == null)
        {
            _dbContext.VaiTros.Add(vt);
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(vt);
        }

        _dbContext.SaveChanges();
    }
}
