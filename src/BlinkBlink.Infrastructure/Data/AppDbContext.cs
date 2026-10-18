using Microsoft.EntityFrameworkCore;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Entities.Customers;

namespace BlinkBlink.Infrastructure.Data;

/// <summary>
/// Lớp ngữ cảnh cơ sở dữ liệu Entity Framework Core trung tâm cho toàn bộ hệ thống Blink Blink Jewelry.
/// Phục vụ kết nối SQL Server và ánh xạ các thực thể sang các bảng dữ liệu quan hệ.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // =========================================================================
    // PHÂN HỆ NGƯỜI DÙNG & PHÂN QUYỀN 
    // =========================================================================
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();

    // =========================================================================
    // HƯỚNG DẪN CHO CÁC PHÂN HỆ KHÁC (Sản phẩm & Kho, Bán hàng, Dịch vụ & Bảo hành):
    // 1. Khai báo thêm DbSet<TEntity> của phân hệ bạn tại đây.
    // 2. Tạo file cấu hình Fluent API riêng trong thư mục Data/Configurations/ kế thừa IEntityTypeConfiguration<TEntity>.
    // 3. Hàm ApplyConfigurationsFromAssembly bên dưới sẽ TỰ ĐỘNG nạp cấu hình của bạn mà KHÔNG cần chỉnh sửa OnModelCreating.
    // =========================================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tự động quét và áp dụng toàn bộ EntityTypeConfiguration trong Assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
