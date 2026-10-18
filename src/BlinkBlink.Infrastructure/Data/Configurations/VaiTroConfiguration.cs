using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho bảng VaiTro trong SQL Server.
/// </summary>
public class VaiTroConfiguration : IEntityTypeConfiguration<VaiTro>
{
    public void Configure(EntityTypeBuilder<VaiTro> builder)
    {
        builder.ToTable("VaiTro");

        // Khóa chính
        builder.HasKey(v => v.MaVaiTro);
        builder.Property(v => v.MaVaiTro)
            .HasMaxLength(20)
            .IsUnicode(false);

        // Thuộc tính tên vai trò
        builder.Property(v => v.TenVaiTro)
            .IsRequired()
            .HasMaxLength(100);

        // Thuộc tính mô tả
        builder.Property(v => v.MoTa)
            .HasMaxLength(255);

        // Quan hệ 1-N: Một vai trò có nhiều người dùng
        builder.HasMany(v => v.NguoiDungs)
            .WithOne(u => u.VaiTro)
            .HasForeignKey(u => u.MaVaiTro)
            .OnDelete(DeleteBehavior.Restrict);

        // Dữ liệu mồi (Seed Data) các vai trò chuẩn của hệ thống
        builder.HasData(
            new VaiTro("ADMIN", "Quản trị viên", "Quản trị toàn quyền hệ thống"),
            new VaiTro("SALE", "Nhân viên bán hàng", "Nhân viên thu ngân bán hàng tại quầy POS"),
            new VaiTro("WAREHOUSE", "Thủ kho", "Nhân viên quản lý kho và lập phiếu kiểm kê"),
            new VaiTro("KHACHHANG", "Khách hàng", "Khách hàng mua sắm trên cổng Web MVC")
        );
    }
}
