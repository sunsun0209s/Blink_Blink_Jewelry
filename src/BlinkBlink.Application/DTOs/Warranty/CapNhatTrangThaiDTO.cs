namespace BlinkBlink.Application.DTOs.Warranty;

/// <summary>
/// DTO chứa thông tin cập nhật tiến độ xử lý hoặc hoàn tất chi phí bảo hành.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public class CapNhatTrangThaiDTO
{
    /// <summary>
    /// Trạng thái mới của quy trình bảo hành (DangKiemDinh, DangXuLy, DaHoanTat, DaHuy)
    /// </summary>
    public string TrangThaiMoi { get; set; } = string.Empty;

    /// <summary>
    /// Chi phí bảo hành thực tế cần thanh toán khi hoàn tất dịch vụ
    /// </summary>
    public decimal ChiPhiBaoHanh { get; set; }
}
