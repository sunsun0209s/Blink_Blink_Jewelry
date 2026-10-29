namespace BlinkBlink.Application.DTOs.Catalog;

/// <summary>
/// Data Transfer Object chứa dữ liệu cập nhật thông tin sản phẩm trang sức.
/// Bản vẽ kỹ thuật: Phân hệ quản lý sản phẩm & kho.xml - CapNhatSanPhamDTO
/// </summary>
public class CapNhatSanPhamDTO
{
    /// <summary>
    /// Mã định danh sản phẩm cần cập nhật
    /// </summary>
    public string MaSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Tên sản phẩm trang sức mới
    /// </summary>
    public string TenSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Mã danh mục mới mà sản phẩm thuộc về
    /// </summary>
    public string MaDanhMuc { get; set; } = string.Empty;

    /// <summary>
    /// Chất liệu chế tác (ví dụ: Vàng 18K, Bạc Ý, Bạch kim)
    /// </summary>
    public string ChatLieu { get; set; } = string.Empty;

    /// <summary>
    /// Kích cỡ của sản phẩm (ví dụ: Size nhẫn 14, Chiều dài dây 45cm)
    /// </summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// Đơn giá vốn nhập (VND)
    /// </summary>
    public decimal GiaNhap { get; set; }

    /// <summary>
    /// Đơn giá niêm yết bán (VND)
    /// </summary>
    public decimal GiaBan { get; set; }

    /// <summary>
    /// Đường dẫn hoặc URL hình ảnh sản phẩm
    /// </summary>
    public string HinhAnh { get; set; } = string.Empty;

    /// <summary>
    /// Tỉ lệ phần trăm giảm giá ưu đãi (0 - 100%)
    /// </summary>
    public int PhanTramGiamGia { get; set; } = 0;
}
