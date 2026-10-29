namespace BlinkBlink.Application.DTOs.Reporting;

/// <summary>
/// DTO chứa thông tin xếp hạng sản phẩm bán chạy hoặc sản phẩm có doanh thu thấp.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public class TopSanPhamBanChayDTO
{
    /// <summary>
    /// Mã sản phẩm trang sức
    /// </summary>
    public string MaSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Tên gọi món trang sức
    /// </summary>
    public string TenSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số lượng món trang sức đã bán
    /// </summary>
    public int SoLuongDaBan { get; set; }

    /// <summary>
    /// Tổng số tiền thu được từ sản phẩm này
    /// </summary>
    public decimal TongTienThu { get; set; }
}
