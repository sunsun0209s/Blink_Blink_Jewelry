using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Domain.Entities.Orders;

/// <summary>
/// Thực thể Hóa đơn / Đơn đặt hàng (HoaDon) trong phân hệ Quản lý bán hàng.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class HoaDon
{
    /// <summary>
    /// Mã định danh hóa đơn (Khóa chính, ví dụ: HD20261022001)
    /// </summary>
    public string MaHoaDon { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm lập hóa đơn hoặc đặt hàng
    /// </summary>
    public DateTime NgayLap { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mã nhân viên phụ trách duyệt / xử lý đơn (cho phép null/rỗng khi khách đặt hàng online)
    /// </summary>
    public string? MaNhanVien { get; set; }

    /// <summary>
    /// Mã khách hàng đặt mua
    /// </summary>
    public string MaKhachHang { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ nhận hàng chi tiết của khách
    /// </summary>
    public string DiaChiGiaoHang { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ nhận hàng
    /// </summary>
    public string SoDienThoaiNhan { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức thanh toán (COD hoặc ChuyenKhoan)
    /// </summary>
    public string PhuongThucThanhToan { get; set; } = "COD";

    /// <summary>
    /// Trạng thái đơn hàng: ChoDuyet, DangDongGoi, DangGiao, DaGiao, DaHuy
    /// </summary>
    public string TrangThaiDonHang { get; set; } = "ChoDuyet";

    /// <summary>
    /// Tổng tiền hàng trước chiết khấu/giảm giá
    /// </summary>
    public decimal TongTienHang { get; set; }

    /// <summary>
    /// Số tiền ưu đãi giảm giá
    /// </summary>
    public decimal TienGiamGia { get; set; }

    /// <summary>
    /// Tổng số tiền thực tế khách cần thanh toán
    /// </summary>
    public decimal TongThanhToan { get; set; }

    /// <summary>
    /// Ghi chú kèm theo đơn hàng
    /// </summary>
    public string GhiChu { get; set; } = string.Empty;

    // Quan hệ điều hướng (Navigation properties)
    public virtual KhachHang? KhachHang { get; set; }
    public virtual NhanVien? NhanVien { get; set; }
    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public HoaDon() { }

    public HoaDon(
        string maHoaDon,
        DateTime ngayLap,
        string maKhachHang,
        string diaChiGiaoHang,
        string soDienThoaiNhan,
        string phuongThucThanhToan = "COD",
        string trangThaiDonHang = "ChoDuyet",
        string ghiChu = "",
        string? maNhanVien = null)
    {
        MaHoaDon = maHoaDon;
        NgayLap = ngayLap;
        MaKhachHang = maKhachHang;
        DiaChiGiaoHang = diaChiGiaoHang;
        SoDienThoaiNhan = soDienThoaiNhan;
        PhuongThucThanhToan = phuongThucThanhToan;
        TrangThaiDonHang = trangThaiDonHang;
        GhiChu = ghiChu;
        MaNhanVien = maNhanVien;
    }

    /// <summary>
    /// Tính toán tổng tiền hàng và tổng thanh toán dựa trên danh sách chi tiết hóa đơn.
    /// Khớp phương thức: + TinhTongTien(): decimal trong Diagram.
    /// </summary>
    /// <returns>Tổng tiền thanh toán cuối cùng sau khi trừ giảm giá</returns>
    public decimal TinhTongTien()
    {
        TongTienHang = ChiTietHoaDons?.Sum(c => c.TinhThanhTien()) ?? 0;
        TongThanhToan = Math.Max(0, TongTienHang - TienGiamGia);
        return TongThanhToan;
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng và gán nhân viên phụ trách xử lý.
    /// Khớp phương thức: + CapNhatTrangThai(trangThai: string, maNV: string): bool trong Diagram.
    /// </summary>
    /// <param name="trangThai">Trạng thái mới của đơn hàng</param>
    /// <param name="maNV">Mã nhân viên thực hiện cập nhật</param>
    /// <returns>True nếu cập nhật hợp lệ, False nếu trạng thái rỗng hoặc đơn đã hủy</returns>
    public bool CapNhatTrangThai(string trangThai, string maNV)
    {
        if (string.IsNullOrWhiteSpace(trangThai))
        {
            return false;
        }

        // Không cho phép cập nhật khi đơn hàng đã hoàn tất hoặc đã hủy
        if (string.Equals(TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        TrangThaiDonHang = trangThai.Trim();

        if (!string.IsNullOrWhiteSpace(maNV))
        {
            MaNhanVien = maNV.Trim();
        }

        return true;
    }

    /// <summary>
    /// Hủy hóa đơn / đơn đặt hàng.
    /// Khớp phương thức: + HuyHoaDon(): bool trong Diagram.
    /// </summary>
    /// <returns>True nếu hủy thành công, False nếu đơn đã giao hoặc đã bị hủy trước đó</returns>
    public bool HuyHoaDon()
    {
        if (string.Equals(TrangThaiDonHang, "DaGiao", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        TrangThaiDonHang = "DaHuy";
        return true;
    }
}
