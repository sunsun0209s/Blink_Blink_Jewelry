namespace BlinkBlink.Application.DTOs.Inventory;

/// <summary>
/// Data Transfer Object cho từng dòng chi tiết sản phẩm trong phiếu nhập kho.
/// Khớp 100% với Class Diagram.
/// </summary>
public class ChiTietNhapKhoDTO
{
    public string MaSanPham { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal GiaVonNhap { get; set; }
    public string SoChungTu { get; set; } = string.Empty;
}
