namespace BlinkBlink.Application.DTOs.Reporting;

/// <summary>
/// DTO chứa thông tin báo cáo định giá tồn kho và chênh lệch kiểm kê của món trang sức.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public class BaoCaoTonKhoDTO
{
    /// <summary>
    /// Mã định danh sản phẩm trang sức
    /// </summary>
    public string MaSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Tên gọi món trang sức
    /// </summary>
    public string TenSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng sản phẩm thực tế hiện có trong kho
    /// </summary>
    public int SoLuongTon { get; set; }

    /// <summary>
    /// Tổng giá trị tồn kho quy đổi theo đơn giá niêm yết
    /// </summary>
    public decimal GiaTriTonKho { get; set; }

    /// <summary>
    /// Số lượng chênh lệch thừa/thiếu ghi nhận từ các đợt kiểm kê gần nhất
    /// </summary>
    public int ChenhLechKiemKe { get; set; }
}
