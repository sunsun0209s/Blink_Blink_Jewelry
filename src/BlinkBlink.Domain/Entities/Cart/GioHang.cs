using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Domain.Entities.Customers;

namespace BlinkBlink.Domain.Entities.Cart;

/// <summary>
/// Thực thể Giỏ hàng (GioHang) của khách hàng.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class GioHang
{
    /// <summary>
    /// Mã giỏ hàng (Khóa chính, ví dụ: GH_KH001)
    /// </summary>
    public string MaGioHang { get; set; } = string.Empty;

    /// <summary>
    /// Mã khách hàng sở hữu giỏ hàng
    /// </summary>
    public string MaKhachHang { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số lượng các sản phẩm có trong giỏ hàng
    /// </summary>
    public int SoLuong { get; set; }

    /// <summary>
    /// Danh sách chi tiết các mặt hàng trong giỏ.
    /// Khớp thuộc tính: - _danhSachSanPham: List&lt;ChiTietGioHang&gt; trong Diagram.
    /// </summary>
    public virtual List<ChiTietGioHang> DanhSachSanPham { get; set; } = new();

    // Quan hệ điều hướng (Navigation properties)
    public virtual KhachHang? KhachHang { get; set; }

    public GioHang() { }

    public GioHang(string maGioHang, string maKhachHang)
    {
        MaGioHang = maGioHang;
        MaKhachHang = maKhachHang;
        SoLuong = 0;
        DanhSachSanPham = new List<ChiTietGioHang>();
    }

    /// <summary>
    /// Dọn dẹp làm trống giỏ hàng sau khi đặt hàng thành công.
    /// Khớp phương thức: + DonDepGioHang(): void trong Diagram.
    /// </summary>
    public void DonDepGioHang()
    {
        DanhSachSanPham.Clear();
        SoLuong = 0;
    }

    /// <summary>
    /// Phương thức nghiệp vụ đánh dấu chuẩn bị đặt hàng từ giỏ hàng.
    /// Khớp phương thức: + DatHang(): void trong Diagram.
    /// </summary>
    public void DatHang()
    {
        CapNhatTongSoLuong();
    }

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng (signature mặc định từ sơ đồ).
    /// Khớp phương thức: + ThemSanPham(): void trong Diagram.
    /// </summary>
    public void ThemSanPham()
    {
        CapNhatTongSoLuong();
    }

    /// <summary>
    /// Thêm hoặc tăng số lượng món trang sức trong giỏ.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm trang sức</param>
    /// <param name="soLuong">Số lượng thêm vào</param>
    public void ThemSanPham(string maSanPham, int soLuong = 1)
    {
        if (string.IsNullOrWhiteSpace(maSanPham) || soLuong <= 0)
        {
            return;
        }

        var existingItem = DanhSachSanPham.FirstOrDefault(c => c.MaSanPham == maSanPham);
        if (existingItem != null)
        {
            existingItem.SoLuongSanPham += soLuong;
        }
        else
        {
            DanhSachSanPham.Add(new ChiTietGioHang(MaGioHang, maSanPham, soLuong));
        }

        CapNhatTongSoLuong();
    }

    /// <summary>
    /// Xóa toàn bộ sản phẩm khỏi giỏ (signature mặc định từ sơ đồ).
    /// Khớp phương thức: + XoaSanPham(): void trong Diagram.
    /// </summary>
    public void XoaSanPham()
    {
        DonDepGioHang();
    }

    /// <summary>
    /// Xóa một món trang sức cụ thể khỏi giỏ hàng theo mã sản phẩm.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm trang sức cần gỡ</param>
    public void XoaSanPham(string maSanPham)
    {
        if (string.IsNullOrWhiteSpace(maSanPham))
        {
            return;
        }

        var item = DanhSachSanPham.FirstOrDefault(c => c.MaSanPham == maSanPham);
        if (item != null)
        {
            DanhSachSanPham.Remove(item);
            CapNhatTongSoLuong();
        }
    }

    /// <summary>
    /// Đồng bộ lại tổng số lượng mặt hàng trong giỏ.
    /// </summary>
    private void CapNhatTongSoLuong()
    {
        SoLuong = DanhSachSanPham.Sum(c => c.SoLuongSanPham);
    }
}
