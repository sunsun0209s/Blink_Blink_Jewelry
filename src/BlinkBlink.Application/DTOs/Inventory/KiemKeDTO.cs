using System;
using System.Collections.Generic;

namespace BlinkBlink.Application.DTOs.Inventory;

/// <summary>
/// DTO chứa thông tin từng dòng sản phẩm kiểm kê đối chiếu.
/// </summary>
public class ChiTietKiemKeItemDTO
{
    public string MaSanPham { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int TonSoSach { get; set; }
    public int TonThucTe { get; set; }
    public int ChenhLech => TonThucTe - TonSoSach;
    public string LyDoChenhLech { get; set; } = string.Empty;
}

/// <summary>
/// DTO yêu cầu tạo mới phiếu kiểm kê kho hàng.
/// </summary>
public class TaoPhieuKiemKeDTO
{
    public string MaPhieuKiemKe { get; set; } = string.Empty;
    public string ViTriKiemKe { get; set; } = string.Empty;
    public string MaNhanVienKho { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public List<ChiTietKiemKeItemDTO> DanhSachChiTiet { get; set; } = new();
}

/// <summary>
/// DTO yêu cầu cập nhật phiếu kiểm kê kho hàng khi còn ở trạng thái ChoDuyet.
/// </summary>
public class CapNhatPhieuKiemKeDTO
{
    public string MaPhieuKiemKe { get; set; } = string.Empty;
    public string ViTriKiemKe { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public List<ChiTietKiemKeItemDTO> DanhSachChiTiet { get; set; } = new();
}

