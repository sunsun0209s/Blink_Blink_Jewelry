using System;

namespace BlinkBlink.Application.DTOs.Warranty;

/// <summary>
/// DTO chứa thông tin tiếp nhận sản phẩm trang sức gửi bảo hành.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public class TaoBaoHanhDTO
{
    /// <summary>
    /// Mã khách hàng gửi sản phẩm bảo hành
    /// </summary>
    public string MaKhachHang { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ của khách hàng (có thể để trống)
    /// </summary>
    public string SoDienThoai { get; set; } = string.Empty;

    /// <summary>
    /// Mã sản phẩm trang sức cần bảo hành
    /// </summary>
    public string MaSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Lý do bảo hành hoặc yêu cầu dịch vụ (đánh bóng, thu/nới size, gắn đá rơi)
    /// </summary>
    public string LyDoBaoHanh { get; set; } = string.Empty;

    /// <summary>
    /// Tình trạng sản phẩm khi tiếp nhận (ví dụ: móp méo, trầy xước, rơi đá...)
    /// </summary>
    public string TinhTrangTiepNhan { get; set; } = string.Empty;

    /// <summary>
    /// Ghi chú bổ sung khi lập phiếu bảo hành
    /// </summary>
    public string GhiChu { get; set; } = string.Empty;

    /// <summary>
    /// Chi phí bảo hành ước tính (có thể để trống hoặc 0 khi vừa tiếp nhận)
    /// </summary>
    public decimal ChiPhiBaoHanh { get; set; }

    /// <summary>
    /// Ngày hẹn trả sản phẩm lại cho khách
    /// </summary>
    public DateTime? NgayHenTra { get; set; }
}
