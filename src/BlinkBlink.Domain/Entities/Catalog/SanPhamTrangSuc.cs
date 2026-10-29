using System;
using System.Collections.Generic;
using System.Text;

namespace BlinkBlink.Domain.Entities.Catalog;

/// <summary>
/// Thực thể Sản phẩm trang sức (SanPhamTrangSuc) trong phân hệ Quản lý sản phẩm & Kho.
/// Khớp 100% với Class Diagram.
/// </summary>
public class SanPhamTrangSuc
{
    public string MaSanPham { get; set; } = string.Empty; // Khóa chính
    public string TenSanPham { get; set; } = string.Empty;
    public string MaDanhMuc { get; set; } = string.Empty; // Khóa ngoại đến Danh mục
    public virtual DanhMuc? DanhMuc { get; set; }
    public string ChatLieu { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty; // Chuẩn hóa string theo Diagram
    public string TrangThai { get; set; } = "HetHang"; // SanSang, DaBan, DangBaoHanh, HetHang
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public string HinhAnh { get; set; } = string.Empty; // Đường dẫn tệp ảnh hoặc URL ảnh
    public int PhanTramGiamGia { get; set; } = 0; // Tỉ lệ phần trăm giảm giá (0 - 100%)
    public int SoLuongTon { get; set; } = 0; // Số lượng hiện tại có trong kho (khớp sơ đồ Class Diagram)

    public SanPhamTrangSuc() { }

    public SanPhamTrangSuc(
        string maSanPham,
        string tenSanPham,
        string maDanhMuc,
        string chatLieu,
        string size,
        decimal giaNhap,
        decimal giaBan,
        string trangThai = "HetHang",
        string hinhAnh = "",
        int phanTramGiamGia = 0)
    {
        MaSanPham = maSanPham;
        TenSanPham = tenSanPham;
        MaDanhMuc = maDanhMuc;
        ChatLieu = chatLieu;
        Size = size;
        GiaNhap = giaNhap;
        GiaBan = giaBan;
        TrangThai = trangThai;
        HinhAnh = hinhAnh;
        PhanTramGiamGia = phanTramGiamGia;
    }

    /// <summary>
    /// Kiểm tra trạng thái sản phẩm có sẵn trong kho để giao dịch hay không.
    /// </summary>
    /// <returns>True nếu còn trong kho ("SanSang"), False nếu đã bán hoặc hết hàng</returns>
    public bool KiemTraTonKho()
    {
        // Khớp hàm KiemTraTonKho(): bool trong Diagram
        return string.Equals(TrangThai, "SanSang", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Cập nhật trạng thái của sản phẩm trang sức.
    /// </summary>
    /// <param name="tt">Trạng thái mới cần chuyển đổi</param>
    /// <returns>True nếu cập nhật hợp lệ, False nếu chuỗi rỗng</returns>
    public bool CapNhatTrangThai(string tt)
    {
        // Khớp hàm CapNhatTrangThai(tt): bool trong Diagram
        if (string.IsNullOrWhiteSpace(tt))
        {
            return false;
        }

        TrangThai = tt.Trim();
        return true;
    }

    /// <summary>
    /// Chuỗi hiển thị định danh sản phẩm.
    /// </summary>
    public override string ToString()
    {
        // Khớp hàm ToString(): string trong Diagram
        return $"[{MaSanPham}] {TenSanPham} - {ChatLieu} (Size: {Size}) - Giá: {GiaBan:N0} VNĐ";
    }
}
