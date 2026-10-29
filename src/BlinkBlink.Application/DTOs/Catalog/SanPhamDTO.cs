namespace BlinkBlink.Application.DTOs.Catalog;

/// <summary>
/// Data Transfer Object cho thông tin tạo mới/cập nhật sản phẩm trang sức.
/// Khớp 100% với Class Diagram.
/// </summary>
public class SanPhamDTO
{
    public string MaSanPham { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string MaDanhMuc { get; set; } = string.Empty;
    public string ChatLieu { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public string HinhAnh { get; set; } = string.Empty;
    public int PhanTramGiamGia { get; set; } = 0;
}
