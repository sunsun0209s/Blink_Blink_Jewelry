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
    public DateTime NgayKiemKe { get; set; } = DateTime.Now;
    public string ViTriKiemKe { get; set; } = string.Empty;
    public string MaNhanVienKho { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "ChoDuyet"; // ChoDuyet, DaDuyet, VoHieuHoa
    public string GhiChu { get; set; } = string.Empty;
    public DateTime? NgayDuyet { get; set; }

    /// <summary>
    /// Thuộc tính đồng bộ với Diagram cũ (true khi DaDuyet, false khi ChoDuyet hoặc VoHieuHoa).
    /// </summary>
    public bool TrangThaiDuyet
    {
        get => string.Equals(TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase);
        set => TrangThai = value ? "DaDuyet" : "ChoDuyet";
    }

    // Quan hệ hợp thành (Composition) 1-N: 1 Phiếu kiểm kê sở hữu danh sách Chi tiết kiểm kê
    public virtual ICollection<ChiTietKiemKe> ChiTietKiemKes { get; set; } = new List<ChiTietKiemKe>();

    public PhieuKiemKe() { }

    public PhieuKiemKe(string maPhieuKiemKe, string viTriKiemKe, string maNhanVienKho, DateTime? ngayKiemKe = null, string trangThai = "ChoDuyet", string ghiChu = "")
    {
        MaPhieuKiemKe = maPhieuKiemKe;
        ViTriKiemKe = viTriKiemKe;
        MaNhanVienKho = maNhanVienKho;
        NgayKiemKe = ngayKiemKe ?? DateTime.Now;
        TrangThai = trangThai;
        GhiChu = ghiChu;
    }
}
