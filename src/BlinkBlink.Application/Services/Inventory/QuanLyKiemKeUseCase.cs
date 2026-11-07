using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Application.DTOs.Inventory;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Inventory;

/// <summary>
/// Use Case Quản Lý Kiểm Kê Kho Hàng (QuanLyKiemKeUseCase).
/// Quản lý toàn bộ quy trình: Lập phiếu, chỉnh sửa, xóa phiếu chờ duyệt,
/// phê duyệt cân bằng tồn kho (kiểm tra xung đột tồn kho sổ sách)
/// và vô hiệu hóa hoàn tác delta (kiểm tra ràng buộc chuỗi kiểm kê mới nhất chống chồng chéo).
/// </summary>
public class QuanLyKiemKeUseCase
{
    private readonly IPhieuKiemKeRepository _kiemKeRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public QuanLyKiemKeUseCase(IPhieuKiemKeRepository kiemKeRepo, ISanPhamRepository sanPhamRepo)
    {
        _kiemKeRepo = kiemKeRepo ?? throw new ArgumentNullException(nameof(kiemKeRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
    }

    /// <summary>
    /// Lấy danh sách toàn bộ phiếu kiểm kê kho sắp xếp theo thời gian mới nhất.
    /// </summary>
    public List<PhieuKiemKe> LayDanhSachPhieuKiemKe()
    {
        return _kiemKeRepo.LayTatCa().OrderByDescending(p => p.NgayKiemKe).ToList();
    }

    /// <summary>
    /// Lấy thông tin chi tiết một phiếu kiểm kê bao gồm danh sách các mặt hàng đối chiếu.
    /// </summary>
    public PhieuKiemKe? LayChiTiet(string maPhieu)
    {
        if (string.IsNullOrWhiteSpace(maPhieu)) return null;
        return _kiemKeRepo.TimTheoId(maPhieu.Trim());
    }

    /// <summary>
    /// Lập biên bản kiểm kê kho hàng mới với trạng thái ban đầu là ChoDuyet.
    /// Tồn kho thực tế chưa bị trừ cho đến khi Quản lý duyệt.
    /// </summary>
    public bool LapPhieuKiemKe(TaoPhieuKiemKeDTO request, out string error)
    {
        error = string.Empty;
        if (request == null)
        {
            error = "Dữ liệu yêu cầu kiểm kê không được để trống.";
            return false;
        }

        if (request.DanhSachChiTiet == null || request.DanhSachChiTiet.Count == 0)
        {
            error = "Vui lòng chọn ít nhất một sản phẩm cần kiểm kê.";
            return false;
        }

        var maPhieu = string.IsNullOrWhiteSpace(request.MaPhieuKiemKe)
            ? $"PKK_{DateTime.UtcNow.Ticks % 1000000:D6}"
            : request.MaPhieuKiemKe.Trim();

        var phieu = new PhieuKiemKe(
            maPhieuKiemKe: maPhieu,
            viTriKiemKe: request.ViTriKiemKe?.Trim() ?? "Kho Tổng",
            maNhanVienKho: request.MaNhanVienKho?.Trim() ?? "NV001",
            ngayKiemKe: DateTime.UtcNow,
            trangThai: "ChoDuyet",
            ghiChu: request.GhiChu?.Trim() ?? string.Empty
        );

        foreach (var item in request.DanhSachChiTiet)
        {
            var chiTiet = new ChiTietKiemKe(
                maPhieuKiemKe: maPhieu,
                maSanPham: item.MaSanPham.Trim(),
                tonSoSach: item.TonSoSach,
                tonThucTe: item.TonThucTe
            )
            {
                LyDoChenhLech = item.LyDoChenhLech?.Trim() ?? string.Empty
            };
            chiTiet.XacDinhTrangThai();
            phieu.ChiTietKiemKes.Add(chiTiet);
        }

        _kiemKeRepo.LuuPhieuKiemKe(phieu);
        return true;
    }

    /// <summary>
    /// Cập nhật phiếu kiểm kê khi còn ở trạng thái ChoDuyet.
    /// </summary>
    public bool CapNhatPhieuKiemKe(CapNhatPhieuKiemKeDTO request, out string error)
    {
        error = string.Empty;
        if (request == null || string.IsNullOrWhiteSpace(request.MaPhieuKiemKe))
        {
            error = "Mã phiếu kiểm kê không hợp lệ.";
            return false;
        }

        var phieu = _kiemKeRepo.TimTheoId(request.MaPhieuKiemKe);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu kiểm kê #{request.MaPhieuKiemKe}.";
            return false;
        }

        if (!string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            error = "Chỉ được phép chỉnh sửa phiếu kiểm kê khi ở trạng thái Chờ duyệt.";
            return false;
        }

        if (request.DanhSachChiTiet == null || request.DanhSachChiTiet.Count == 0)
        {
            error = "Phiếu kiểm kê phải có ít nhất một mặt hàng.";
            return false;
        }

        phieu.ViTriKiemKe = request.ViTriKiemKe?.Trim() ?? phieu.ViTriKiemKe;
        phieu.GhiChu = request.GhiChu?.Trim() ?? string.Empty;
        phieu.ChiTietKiemKes.Clear();

        foreach (var item in request.DanhSachChiTiet)
        {
            var chiTiet = new ChiTietKiemKe(
                maPhieuKiemKe: phieu.MaPhieuKiemKe,
                maSanPham: item.MaSanPham.Trim(),
                tonSoSach: item.TonSoSach,
                tonThucTe: item.TonThucTe
            )
            {
                LyDoChenhLech = item.LyDoChenhLech?.Trim() ?? string.Empty
            };
            chiTiet.XacDinhTrangThai();
            phieu.ChiTietKiemKes.Add(chiTiet);
        }

        _kiemKeRepo.LuuPhieuKiemKe(phieu);
        return true;
    }

    /// <summary>
    /// Xóa phiếu kiểm kê chưa duyệt (dành cho cả nhân viên kho và quản trị viên).
    /// </summary>
    public bool XoaPhieuKiemKe(string maPhieu, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            error = "Mã phiếu kiểm kê không hợp lệ.";
            return false;
        }

        var phieu = _kiemKeRepo.TimTheoId(maPhieu);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu kiểm kê #{maPhieu}.";
            return false;
        }

        if (!string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            error = "Chỉ được phép xóa phiếu kiểm kê khi ở trạng thái Chờ duyệt.";
            return false;
        }

        _kiemKeRepo.XoaPhieuKiemKe(maPhieu);
        return true;
    }

    /// <summary>
    /// Phê duyệt phiếu kiểm kê kho hàng (dành cho Quản trị viên / Quản lý).
    /// Ràng buộc: Kiểm tra tồn kho thực tế hiện tại của từng sản phẩm có còn khớp với TonSoSach ghi trong phiếu hay không.
    /// Nếu lệch -> từ chối duyệt để tránh ghi đè sai lệch dữ liệu.
    /// Nếu khớp -> tự động cân bằng tồn kho theo delta chênh lệch và chuyển trạng thái sang DaDuyet.
    /// </summary>
    public bool DuyetPhieuKiemKe(string maPhieu, string maAdmin, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            error = "Mã phiếu kiểm kê không hợp lệ.";
            return false;
        }

        var phieu = _kiemKeRepo.TimTheoId(maPhieu);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu kiểm kê #{maPhieu}.";
            return false;
        }

        if (!string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Phiếu kiểm kê #{maPhieu} đang ở trạng thái '{phieu.TrangThai}', không thể thực hiện phê duyệt.";
            return false;
        }

        if (phieu.ChiTietKiemKes == null || phieu.ChiTietKiemKes.Count == 0)
        {
            error = "Phiếu kiểm kê không có sản phẩm nào.";
            return false;
        }

        // Bước 1: Ràng buộc kiểm tra tính hợp lệ của tồn kho sổ sách trước khi duyệt
        foreach (var ct in phieu.ChiTietKiemKes)
        {
            var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
            var tonKhoHienTai = sp?.SoLuongTon ?? 0;

            if (tonKhoHienTai != ct.TonSoSach)
            {
                error = $"Số lượng tồn kho được ghi nhận của sản phẩm [{ct.MaSanPham}] không chính xác so với hệ thống hiện tại (Hệ thống: {tonKhoHienTai}, Phiếu ghi: {ct.TonSoSach}). Vui lòng cập nhật lại phiếu kiểm kê trước khi duyệt.";
                return false;
            }
        }

        // Bước 2: Cân bằng tồn kho thực tế (áp dụng delta = TonThucTe - TonSoSach)
        foreach (var ct in phieu.ChiTietKiemKes)
        {
            var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
            if (sp != null)
            {
                var delta = ct.TonThucTe - ct.TonSoSach;
                sp.SoLuongTon = Math.Max(0, sp.SoLuongTon + delta);
                _sanPhamRepo.LuuSanPham(sp);
            }
        }

        // Bước 3: Cập nhật trạng thái phiếu sang DaDuyet
        _kiemKeRepo.CapNhatTrangThai(maPhieu, "DaDuyet", DateTime.UtcNow);
        return true;
    }

    /// <summary>
    /// Vô hiệu hóa phiếu kiểm kê đã duyệt (dành riêng cho Quản trị viên Admin).
    /// Ràng buộc chống chồng chéo: Chỉ cho phép vô hiệu hóa nếu đây là phiếu kiểm kê mới nhất đối với tất cả sản phẩm trong phiếu.
    /// Hoàn tác số lượng tồn kho theo đúng delta chênh lệch, bảo toàn các giao dịch phát sinh sau thời điểm duyệt.
    /// </summary>
    public bool VoHieuHoaPhieuKiemKe(string maPhieu, string maAdmin, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            error = "Mã phiếu kiểm kê không hợp lệ.";
            return false;
        }

        var phieu = _kiemKeRepo.TimTheoId(maPhieu);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu kiểm kê #{maPhieu}.";
            return false;
        }

        if (!string.Equals(phieu.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Phiếu kiểm kê #{maPhieu} không ở trạng thái Đã duyệt để có thể vô hiệu hóa.";
            return false;
        }

        // Bước 1: Kiểm tra ràng buộc thứ tự kiểm kê (Anti-conflict audit chain)
        if (phieu.ChiTietKiemKes != null)
        {
            foreach (var ct in phieu.ChiTietKiemKes)
            {
                var coPhieuMoiHon = _kiemKeRepo.KiemTraCoPhieuKiemKeMoiHon(maPhieu, ct.MaSanPham);
                if (coPhieuMoiHon)
                {
                    error = $"Chỉ cho phép vô hiệu hóa phiếu có chỉnh sửa mới nhất đối với sản phẩm [{ct.MaSanPham}]. Quản trị viên có thể lập phiếu kiểm kê khác để bù lại số lượng hoặc vô hiệu hóa phiếu kiểm kê mới hơn trước.";
                    return false;
                }
            }

            // Bước 2: Hoàn tác tồn kho theo delta: TonKhoMoi = TonKhoHienTai - (TonThucTe - TonSoSach)
            foreach (var ct in phieu.ChiTietKiemKes)
            {
                var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
                if (sp != null)
                {
                    var delta = ct.TonThucTe - ct.TonSoSach;
                    sp.SoLuongTon = Math.Max(0, sp.SoLuongTon - delta);
                    _sanPhamRepo.LuuSanPham(sp);
                }
            }
        }

        // Bước 3: Đánh dấu trạng thái phiếu sang VoHieuHoa
        _kiemKeRepo.CapNhatTrangThai(maPhieu, "VoHieuHoa");
        return true;
    }
}
