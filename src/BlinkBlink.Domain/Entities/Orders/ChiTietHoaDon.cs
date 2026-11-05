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

    /// <summary>
    /// Phần trăm giảm giá ưu đãi tại thời điểm mua (ví dụ: 10 = giảm 10%)
    /// Khớp thuộc tính: - _phanTramGiamGia: int trong Diagram
    /// </summary>
    public int PhanTramGiamGia { get; set; }

    // Quan hệ điều hướng (Navigation properties)
    public virtual HoaDon? HoaDon { get; set; }
    public virtual SanPhamTrangSuc? SanPham { get; set; }

    public ChiTietHoaDon() { }

    public ChiTietHoaDon(string soHoaDon, string maSanPham, decimal donGia, int soLuong, int phanTramGiamGia = 0)
    {
        SoHoaDon = soHoaDon;
        MaSanPham = maSanPham;
        DonGia = donGia;
        SoLuong = soLuong;
        PhanTramGiamGia = phanTramGiamGia;
    }

    /// <summary>
    /// Tính thành tiền sau khi đã trừ tỷ lệ giảm giá sản phẩm tại thời điểm mua.
    /// Khớp phương thức: + TinhThanhTien(): decimal trong Diagram.
    /// </summary>
    /// <returns>Thành tiền = Đơn giá * Số lượng * (100 - % Giảm) / 100</returns>
    public decimal TinhThanhTien()
    {
        var tongGoc = DonGia * SoLuong;
        if (PhanTramGiamGia > 0 && PhanTramGiamGia <= 100)
        {
            return tongGoc * (100 - PhanTramGiamGia) / 100m;
        }
        return tongGoc;
    }
}
