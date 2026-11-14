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
        NgayKiemKe = ngayKiemKe ?? DateTime.UtcNow;
        TrangThai = trangThai;
        GhiChu = ghiChu;
    }

    /// <summary>
    /// Bắt đầu đợt kiểm kê thực tế tại khu vực quầy/kho.
    /// </summary>
    public void BatDauKiemKe()
    {
        // Khớp hàm BatDauKiemKe(): void trong Diagram
        TrangThai = "ChoDuyet";
        NgayKiemKe = DateTime.UtcNow;
    }

    /// <summary>
    /// Kết thúc quy trình kiểm kê và chốt số liệu khi Quản lý duyệt phiếu.
    /// </summary>
    public void KetThucKiemKe()
    {
        // Khớp hàm KetThucKiemKe(): void trong Diagram
        foreach (var chiTiet in ChiTietKiemKes)
        {
            chiTiet.XacDinhTrangThai();
        }
        TrangThai = "DaDuyet";
        NgayDuyet = DateTime.UtcNow;
    }

    /// <summary>
    /// Phương thức khớp 100% với Class Diagram.
    /// Nghiệp vụ vô hiệu hóa phiếu kiểm kê đã được gỡ bỏ khỏi hệ thống.
    /// </summary>
    public void VoHieuHoa()
    {
        // Giữ rỗng để đảm bảo tính toàn vẹn với sơ đồ lớp Class Diagram
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
