using System;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Entities.Orders;

/// <summary>
/// Thực thể Chi tiết hóa đơn (ChiTietHoaDon) lưu trữ các mặt hàng trang sức trong một đơn hàng.
/// Khóa chính kết hợp: SoHoaDon + MaSanPham.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class ChiTietHoaDon
{
    /// <summary>
    /// Số hóa đơn (Khóa ngoại tham chiếu đến HoaDon)
    /// </summary>
    public string SoHoaDon { get; set; } = string.Empty;

    /// <summary>
    /// Mã sản phẩm trang sức (Khóa ngoại tham chiếu đến SanPhamTrangSuc)
    /// </summary>
    public string MaSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Đơn giá bán tại thời điểm lập đơn
    /// </summary>
    public decimal DonGia { get; set; }

    /// <summary>
    /// Số lượng sản phẩm đặt mua
    /// </summary>
    public int SoLuong { get; set; }

    // Quan hệ điều hướng (Navigation properties)
    public virtual HoaDon? HoaDon { get; set; }
    public virtual SanPhamTrangSuc? SanPham { get; set; }

    public ChiTietHoaDon() { }

    public ChiTietHoaDon(string soHoaDon, string maSanPham, decimal donGia, int soLuong)
    {
        SoHoaDon = soHoaDon;
        MaSanPham = maSanPham;
        DonGia = donGia;
        SoLuong = soLuong;
    }

    /// <summary>
    /// Tính thành tiền của dòng chi tiết sản phẩm.
    /// Khớp phương thức: + TinhThanhTien(): decimal trong Diagram.
    /// </summary>
    /// <returns>Thành tiền = Đơn giá * Số lượng</returns>
    public decimal TinhThanhTien()
    {
        return DonGia * SoLuong;
    }
}
