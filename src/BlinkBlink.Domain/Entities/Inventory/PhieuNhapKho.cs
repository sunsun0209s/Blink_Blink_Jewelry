using System;
using System.Collections.Generic;
using System.Linq;

namespace BlinkBlink.Domain.Entities.Inventory;

/// <summary>
/// Thực thể Phiếu nhập kho (PhieuNhapKho) ghi nhận thông tin nhập sản phẩm trang sức từ nhà cung cấp.
/// Khớp 100% với Class Diagram.
/// </summary>
public class PhieuNhapKho
{
    public string MaPhieuNhap { get; set; } = string.Empty; // Khóa chính
    public DateTime NgayNhap { get; set; } = DateTime.UtcNow;
    public string NhaCungCap { get; set; } = string.Empty;
    public string MaNhanVien { get; set; } = string.Empty;
    public int TongSoSanPham { get; set; }
    public decimal TongGiaTri { get; set; }
    public string TrangThai { get; set; } = "ChoDuyet"; // ChoDuyet: Chờ Admin duyệt, DaDuyet: Đã duyệt (tính kho & giá), DaHuy: Đã hủy

    // Quan hệ hợp thành (Composition) 1-N: 1 Phiếu nhập kho sở hữu nhiều Chi tiết nhập kho
    public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();

    public PhieuNhapKho() { }

    public PhieuNhapKho(string maPhieuNhap, string nhaCungCap, string maNhanVien, DateTime? ngayNhap = null, string trangThai = "ChoDuyet")
    {
        MaPhieuNhap = maPhieuNhap;
        NhaCungCap = nhaCungCap;
        MaNhanVien = maNhanVien;
        NgayNhap = ngayNhap ?? DateTime.UtcNow;
        TrangThai = string.IsNullOrWhiteSpace(trangThai) ? "ChoDuyet" : trangThai;
    }

    public PhieuNhapKho(string maPhieuNhap, string nhaCungCap, string maNhanVien, DateTime? ngayNhap, bool isApproved)
        : this(maPhieuNhap, nhaCungCap, maNhanVien, ngayNhap, isApproved ? "DaDuyet" : "DaHuy")
    {
    }

    /// <summary>
    /// Phê duyệt phiếu nhập kho (chuyển trạng thái từ ChoDuyet sang DaDuyet).
    /// </summary>
    public bool DuyetPhieu(string nguoiDuyet = "")
    {
        if (!string.Equals(TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase) || ChiTietNhapKhos.Count == 0)
        {
            return false;
        }

        TrangThai = "DaDuyet";
        return true;
    }

    /// <summary>
    /// Thêm một dòng chi tiết sản phẩm vào phiếu nhập và tự động tái tính tổng số lượng, tổng giá trị.
    /// </summary>
    /// <param name="sp">Chi tiết nhập kho của sản phẩm</param>
    public void ThemChiTiet(ChiTietNhapKho sp)
    {
        // Khớp hàm ThemChiTiet(sp): void trong Diagram
        if (sp == null)
        {
            throw new ArgumentNullException(nameof(sp), "Chi tiết nhập kho không được để trống.");
        }

        sp.MaPhieuNhap = this.MaPhieuNhap;
        ChiTietNhapKhos.Add(sp);

        // Tự động cập nhật tổng số lượng và tổng giá trị
        TongSoSanPham = ChiTietNhapKhos.Sum(x => x.SoLuong);
        TongGiaTri = ChiTietNhapKhos.Sum(x => x.TinhGiaTri());
    }

    /// <summary>
    /// Xác nhận hoàn tất nhập kho và cập nhật số lượng/giá vốn sản phẩm vào hệ thống.
    /// </summary>
    /// <returns>True nếu phiếu có chi tiết hợp lệ</returns>
    public bool XacNhanNhapKho()
    {
        // Khớp hàm XacNhanNhapKho(): bool trong Diagram
        if (ChiTietNhapKhos.Count == 0 || TongSoSanPham <= 0)
        {
            return false;
        }

        TongGiaTri = ChiTietNhapKhos.Sum(x => x.TinhGiaTri());
        return true;
    }

    /// <summary>
    /// Hủy bỏ phiếu nhập kho khi có sự cố sai lệch từ nhà cung cấp.
    /// </summary>
    /// <returns>True nếu hủy thành công</returns>
    public bool HuyPhieu()
    {
        // Khớp hàm HuyPhieu(): bool trong Diagram
        ChiTietNhapKhos.Clear();
        TongSoSanPham = 0;
        TongGiaTri = 0;
        return true;
    }
}
