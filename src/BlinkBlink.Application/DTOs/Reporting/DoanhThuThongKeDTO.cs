using System;

namespace BlinkBlink.Application.DTOs.Reporting;

/// <summary>
/// DTO chứa số liệu thống kê doanh thu và lợi nhuận ước tính theo mốc thời gian.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public class DoanhThuThongKeDTO
{
    /// <summary>
    /// Ngày ghi nhận doanh thu
    /// </summary>
    public DateTime NgayThang { get; set; }

    /// <summary>
    /// Tổng số lượng đơn hàng hoàn tất trong ngày
    /// </summary>
    public int SoLuongDon { get; set; }

    /// <summary>
    /// Tổng số tiền doanh thu bán hàng thực thu
    /// </summary>
    public decimal TongDoanhThu { get; set; }

    /// <summary>
    /// Lợi nhuận ước tính (Doanh thu - Giá vốn nhập hàng)
    /// </summary>
    public decimal LoiNhuanUocTinh { get; set; }
}
