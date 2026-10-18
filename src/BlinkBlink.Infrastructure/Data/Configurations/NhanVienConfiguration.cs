using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể con NhanVien (Bảng riêng trong chiến lược TPT).
/// </summary>
public class NhanVienConfiguration : IEntityTypeConfiguration<NhanVien>
{
    public void Configure(EntityTypeBuilder<NhanVien> builder)
    {
        builder.ToTable("NhanVien");

        // Mã nhân viên định danh nội bộ
        builder.Property(nv => nv.MaNhanVien)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(nv => nv.MaNhanVien)
            .IsUnique();

        // Mức lương cơ bản
        builder.Property(nv => nv.LuongCoBan)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
    }
}
