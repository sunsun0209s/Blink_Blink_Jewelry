using System;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Entities.Cart;

/// <summary>
/// Thực thể Chi tiết giỏ hàng (ChiTietGioHang) lưu trữ mặt hàng khách đã chọn.
/// Khóa chính kết hợp: MaGioHang + MaSanPham.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class ChiTietGioHang
{
    /// <summary>
    /// Mã giỏ hàng (Khóa ngoại tham chiếu đến GioHang)
    /// </summary>
    public string MaGioHang { get; set; } = string.Empty;

    /// <summary>
    /// Mã sản phẩm trang sức (Khóa ngoại tham chiếu đến SanPhamTrangSuc)
    /// </summary>
    public string MaSanPham { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng sản phẩm khách chọn trong giỏ
    /// </summary>
    public int SoLuongSanPham { get; set; } = 1;

    // Quan hệ điều hướng (Navigation properties)
    public virtual GioHang? GioHang { get; set; }
    public virtual SanPhamTrangSuc? SanPham { get; set; }

    public ChiTietGioHang() { }

    public ChiTietGioHang(string maGioHang, string maSanPham, int soLuongSanPham = 1)
    {
        MaGioHang = maGioHang;
        MaSanPham = maSanPham;
        SoLuongSanPham = Math.Max(1, soLuongSanPham);
    }

    /// <summary>
    /// Tăng số lượng sản phẩm trong dòng giỏ hàng.
    /// Khớp phương thức: + TangSoLuong(): void trong Diagram.
    /// </summary>
    public void TangSoLuong()
    {
        SoLuongSanPham++;
    }

    /// <summary>
    /// Giảm số lượng sản phẩm trong dòng giỏ hàng (tối thiểu là 1).
    /// Khớp phương thức: + GiamSoLuong(): void trong Diagram.
    /// </summary>
    public void GiamSoLuong()
    {
        if (SoLuongSanPham > 1)
        {
            SoLuongSanPham--;
        }
    }
}
