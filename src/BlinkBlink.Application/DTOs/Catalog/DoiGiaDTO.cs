namespace BlinkBlink.Application.DTOs.Catalog;

/// <summary>
/// Data Transfer Object chuyên biệt cho yêu cầu cập nhật giá bán sản phẩm.
/// Khớp 100% với Class Diagram.
/// </summary>
public class DoiGiaDTO
{
    /// <summary>
    /// Giá bán mới của sản phẩm trang sức
    /// </summary>
    public decimal GiaMoi { get; set; }
}
