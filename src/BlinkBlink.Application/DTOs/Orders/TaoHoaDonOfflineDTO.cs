using System;
using System.Collections.Generic;

namespace BlinkBlink.Application.DTOs.Orders;

/// <summary>
/// DTO chứa thông tin sản phẩm bán lẻ offline tại quầy thu ngân.
/// </summary>
public class ChiTietBanOfflineDTO
{
    public string MaSanPham { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public int PhanTramGiamGia { get; set; }

    public decimal ThanhTien
    {
        get
        {
            var goc = DonGia * SoLuong;
            if (PhanTramGiamGia > 0 && PhanTramGiamGia <= 100)
            {
                return goc * (100 - PhanTramGiamGia) / 100m;
            }
            return goc;
        }
    }

    public decimal TienGiam => (DonGia * SoLuong) - ThanhTien;
}

/// <summary>
/// DTO tiếp nhận yêu cầu lập hóa đơn bán hàng trực tiếp tại quầy POS offline.
/// </summary>
public class TaoHoaDonOfflineDTO
{
    public string TenKhachHang { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string PhuongThucThanhToan { get; set; } = "TienMat"; // TienMat, ChuyenKhoan
    public string MaNhanVien { get; set; } = string.Empty;
    public List<ChiTietBanOfflineDTO> DanhSachSanPham { get; set; } = new();
}
