namespace BlinkBlink.Application.DTOs.Orders;

/// <summary>
/// DTO chứa thông tin cập nhật trạng thái đơn hàng do nhân viên thực hiện.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class CapNhatDonHangDTO
{
    /// <summary>
    /// Mã hóa đơn cần cập nhật
    /// </summary>
    public string MaHoaDon { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái đơn hàng mới (DangDongGoi, DangGiao, DaGiao, DaHuy)
    /// </summary>
    public string TrangThaiMoi { get; set; } = string.Empty;

    /// <summary>
    /// Mã nhân viên thực hiện thao tác cập nhật
    /// </summary>
    public string MaNhanVien { get; set; } = string.Empty;
}
