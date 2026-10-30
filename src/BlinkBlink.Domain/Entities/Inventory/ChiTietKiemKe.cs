using System;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Domain.Entities.Inventory;

/// <summary>
/// Thực thể Chi tiết kiểm kê kho (ChiTietKiemKe) đối chiếu giữa tồn sổ sách và tồn thực tế.
/// Khớp 100% với Class Diagram.
/// </summary>
public class ChiTietKiemKe
{
    public string MaPhieuKiemKe { get; set; } = string.Empty; // Khóa chính (khóa kết hợp)
    public string MaSanPham { get; set; } = string.Empty;     // Khóa chính (khóa kết hợp)
    public int TonSoSach { get; set; }
    public int TonThucTe { get; set; }
    public int ChenhLech { get; set; }
    public string LyDoChenhLech { get; set; } = string.Empty;

    // Quan hệ điều hướng
    public virtual PhieuKiemKe? PhieuKiemKe { get; set; }
    public virtual SanPhamTrangSuc? SanPhamTrangSuc { get; set; }

    public ChiTietKiemKe() { }

    public ChiTietKiemKe(string maPhieuKiemKe, string maSanPham, int tonSoSach, int tonThucTe)
    {
        MaPhieuKiemKe = maPhieuKiemKe;
        MaSanPham = maSanPham;
        TonSoSach = tonSoSach;
        TonThucTe = tonThucTe;
        ChenhLech = tonThucTe - tonSoSach;
    }

    /// <summary>
    /// Xác định trạng thái chênh lệch số lượng tồn kho (Khớp, Thừa, Thiếu).
    /// </summary>
    /// <returns>Chuỗi mô tả trạng thái: "Khop", "Thua", "Thieu"</returns>
    public string XacDinhTrangThai()
    {
        // Khớp hàm XacDinhTrangThai(): string trong Diagram
        ChenhLech = TonThucTe - TonSoSach;
        if (ChenhLech == 0) return "Khop";
        return ChenhLech > 0 ? "Thua" : "Thieu";
    }

    /// <summary>
    /// Ghi nhận lý do chênh lệch (ví dụ: hao hụt tự nhiên, nhầm mã, rơi vỡ).
    /// </summary>
    /// <param name="lyDo">Nguyên nhân chênh lệch</param>
    public void GhiNhanLyDo(string lyDo)
    {
        // Khớp hàm GhiNhanLyDo(lyDo): void trong Diagram
        LyDoChenhLech = lyDo ?? string.Empty;
    }

    /// <summary>
    /// Tính toán tổng giá trị hao hụt (nếu thiếu hụt thực tế so với sổ sách).
    /// </summary>
    /// <returns>Giá trị tiền hao hụt dựa theo giá nhập của sản phẩm</returns>
    public decimal TinhTongHaoHut()
    {
        // Khớp hàm TinhTongHaoHut(): decimal trong Diagram
        if (ChenhLech >= 0 || SanPhamTrangSuc == null)
        {
            return 0m;
        }

        // Hao hụt tính bằng số lượng thiếu hụt * đơn giá nhập
        return Math.Abs(ChenhLech) * SanPhamTrangSuc.GiaNhap;
    }
}
