using System;
using System.Collections.Generic;

namespace BlinkBlink.Application.DTOs.Inventory;

/// <summary>
/// Data Transfer Object cho yêu cầu xác nhận nhập kho sản phẩm trang sức.
/// Khớp 100% với Class Diagram.
/// </summary>
public class NhapKhoDTO
{
    public string MaPhieuNhap { get; set; } = string.Empty;
    public string NhaCungCap { get; set; } = string.Empty;
    public string MaNhanVien { get; set; } = string.Empty;
    public List<ChiTietNhapKhoDTO> DanhSachSanPham { get; set; } = new();
}
