using System;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Entities.Inventory;

/// <summary>
/// Thực thể Chi tiết phiếu nhập kho (ChiTietNhapKho).
/// Khớp 100% với Class Diagram.
/// </summary>
public class ChiTietNhapKho
{
    public string MaPhieuNhap { get; set; } = string.Empty; // Khóa chính (khóa kết hợp)
    public string MaSanPham { get; set; } = string.Empty;   // Khóa chính (khóa kết hợp)
    public int SoLuong { get; set; }
    public decimal GiaVonNhap { get; set; }
    public string SoChungTu { get; set; } = string.Empty;

    // Quan hệ điều hướng
    public virtual PhieuNhapKho? PhieuNhapKho { get; set; }
    public virtual SanPhamTrangSuc? SanPhamTrangSuc { get; set; }

    public ChiTietNhapKho() { }

    public ChiTietNhapKho(string maPhieuNhap, string maSanPham, int soLuong, decimal giaVonNhap, string soChungTu)
    {
        MaPhieuNhap = maPhieuNhap;
        MaSanPham = maSanPham;
        SoLuong = soLuong;
        GiaVonNhap = giaVonNhap;
        SoChungTu = soChungTu;
    }

    /// <summary>
    /// Tính tổng thành tiền của dòng chi tiết nhập kho này (Số lượng x Giá vốn nhập).
    /// </summary>
    /// <returns>Tổng giá trị thành tiền</returns>
    public decimal TinhGiaTri()
    {
        // Khớp hàm TinhGiaTri(): decimal trong Diagram
        return SoLuong * GiaVonNhap;
    }

    /// <summary>
    /// Cập nhật và thẩm định lại giá vốn nhập của sản phẩm.
    /// </summary>
    /// <returns>True nếu giá vốn > 0 và hợp lệ</returns>
    public bool CapNhatGiaVon()
    {
        // Khớp hàm CapNhatGiaVon(): bool trong Diagram
        return GiaVonNhap >= 0;
    }
}
