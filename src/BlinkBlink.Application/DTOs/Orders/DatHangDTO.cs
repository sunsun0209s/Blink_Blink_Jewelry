namespace BlinkBlink.Application.DTOs.Orders;

/// <summary>
/// DTO chứa dữ liệu yêu cầu đặt hàng trực tuyến từ khách hàng.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class DatHangDTO
{
    /// <summary>
    /// Mã khách hàng đặt đơn
    /// </summary>
    public string MaKhachHang { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ nhận hàng
    /// </summary>
    public string DiaChiGiaoHang { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên lạc khi nhận hàng
    /// </summary>
    public string SoDienThoaiNhan { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức thanh toán (COD hoặc ChuyenKhoan)
    /// </summary>
    public string PhuongThucThanhToan { get; set; } = "COD";

    /// <summary>
    /// Ghi chú giao hàng của khách
    /// </summary>
    public string GhiChu { get; set; } = string.Empty;
}
