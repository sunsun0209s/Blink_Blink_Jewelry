using System;
using System.Collections.Generic;
using System.Linq;

namespace BlinkBlink.Domain.Entities.Inventory;

/// <summary>
/// Thực thể Phiếu kiểm kê kho (PhieuKiemKe).
/// Khớp 100% với Class Diagram.
/// </summary>
public class PhieuKiemKe
{
    public string MaPhieuKiemKe { get; set; } = string.Empty; // Khóa chính
    public DateTime NgayKiemKe { get; set; } = DateTime.UtcNow;
    public string ViTriKiemKe { get; set; } = string.Empty;
    public string MaNhanVienKho { get; set; } = string.Empty;
    public bool TrangThaiDuyet { get; set; }

    // Quan hệ hợp thành (Composition) 1-N: 1 Phiếu kiểm kê sở hữu danh sách Chi tiết kiểm kê
    public virtual ICollection<ChiTietKiemKe> ChiTietKiemKes { get; set; } = new List<ChiTietKiemKe>();

    public PhieuKiemKe() { }

    public PhieuKiemKe(string maPhieuKiemKe, string viTriKiemKe, string maNhanVienKho, DateTime? ngayKiemKe = null)
    {
        MaPhieuKiemKe = maPhieuKiemKe;
        ViTriKiemKe = viTriKiemKe;
        MaNhanVienKho = maNhanVienKho;
        NgayKiemKe = ngayKiemKe ?? DateTime.UtcNow;
        TrangThaiDuyet = false;
    }

    /// <summary>
    /// Bắt đầu đợt kiểm kê thực tế tại khu vực quầy/kho.
    /// </summary>
    public void BatDauKiemKe()
    {
        // Khớp hàm BatDauKiemKe(): void trong Diagram
        TrangThaiDuyet = false;
        NgayKiemKe = DateTime.UtcNow;
    }

    /// <summary>
    /// Kết thúc quy trình kiểm kê và chốt số liệu.
    /// </summary>
    public void KetThucKiemKe()
    {
        // Khớp hàm KetThucKiemKe(): void trong Diagram
        foreach (var chiTiet in ChiTietKiemKes)
        {
            chiTiet.XacDinhTrangThai();
        }
        TrangThaiDuyet = true;
    }

    /// <summary>
    /// Lập biên bản chênh lệch kho cho những mặt hàng thừa/thiếu.
    /// </summary>
    public void LapBienBanChenhLech()
    {
        // Khớp hàm LapBienBanChenhLech(): void trong Diagram
        var chenhLechList = ChiTietKiemKes.Where(c => c.ChenhLech != 0).ToList();
        foreach (var item in chenhLechList)
        {
            if (string.IsNullOrWhiteSpace(item.LyDoChenhLech))
            {
                item.GhiNhanLyDo("Chờ bộ phận kho xác minh nguyên nhân");
            }
        }
    }
}
