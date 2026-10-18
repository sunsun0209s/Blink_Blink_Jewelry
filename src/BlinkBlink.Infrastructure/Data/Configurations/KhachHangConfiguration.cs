using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlinkBlink.Domain.Entities.Customers;

namespace BlinkBlink.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể con KhachHang (Bảng riêng trong chiến lược TPT).
/// </summary>
public class KhachHangConfiguration : IEntityTypeConfiguration<KhachHang>
{
    public void Configure(EntityTypeBuilder<KhachHang> builder)
    {
        builder.ToTable("KhachHang");

        // Mã khách hàng định danh duy nhất
        builder.Property(kh => kh.MaKhachHang)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(kh => kh.MaKhachHang)
            .IsUnique();

        // Ngày sinh
        builder.Property(kh => kh.NgaySinh)
            .HasColumnType("date");
    }
}
