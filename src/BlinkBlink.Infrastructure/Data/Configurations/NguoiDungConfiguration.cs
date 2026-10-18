using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể cơ sở NguoiDung (Bảng cha theo chiến lược TPT).
/// </summary>
public class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> builder)
    {
        // Áp dụng chiến lược Table-per-Type (TPT) để NhanVien và KhachHang là các bảng con riêng biệt
        builder.UseTptMappingStrategy();
        builder.ToTable("NguoiDung");

        // Khóa chính
        builder.HasKey(u => u.MaNguoiDung);
        builder.Property(u => u.MaNguoiDung)
            .HasMaxLength(50)
            .IsUnicode(false);

        // Họ tên người dùng
        builder.Property(u => u.HoTen)
            .IsRequired()
            .HasMaxLength(100);

        // Số điện thoại
        builder.Property(u => u.SoDienThoai)
            .HasMaxLength(20)
            .IsUnicode(false);

        // Tên đăng nhập (Duy nhất - Unique Index)
        builder.Property(u => u.TenDangNhap)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(u => u.TenDangNhap)
            .IsUnique();

        // Mật khẩu (Chuỗi băm an toàn)
        builder.Property(u => u.MatKhau)
            .IsRequired()
            .HasMaxLength(255);

        // Khóa ngoại đến vai trò
        builder.Property(u => u.MaVaiTro)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        // Trạng thái tài khoản
        builder.Property(u => u.TrangThai)
            .HasDefaultValue(true);
    }
}
