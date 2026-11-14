using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Application.DTOs.Inventory;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Inventory;

/// <summary>
/// Use Case Quản Lý Kiểm Kê Kho Hàng (QuanLyKiemKeUseCase).
/// Quản lý toàn bộ quy trình: Lập phiếu, xóa phiếu chờ duyệt,
/// và phê duyệt cân bằng tồn kho (kiểm tra tính hợp lệ của tồn kho sổ sách).
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
    /// Chốt chặn 1: Kiểm tra tính hợp lệ của biên bản và đối chiếu số lượng tồn sổ sách với CSDL tại thời điểm lập.
    /// </summary>
    public bool LapPhieuKiemKe(TaoPhieuKiemKeDTO request, out string error)
    {
        error = string.Empty;
        if (request == null)
        {
            error = "Dữ liệu yêu cầu kiểm kê không được để trống.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.ViTriKiemKe))
        {
            error = "Vị trí kiểm kê không được để trống.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.MaNhanVienKho))
        {
            error = "Mã nhân viên kiểm kê không được để trống.";
            return false;
        }

        if (request.DanhSachChiTiet == null || request.DanhSachChiTiet.Count == 0)
        {
            error = "Phiếu kiểm kê phải có ít nhất một mặt hàng trang sức.";
            return false;
        }

        // 1. Kiểm tra chống trùng lặp mã sản phẩm trong cùng một biên bản kiểm kê
        var duplicate = request.DanhSachChiTiet
            .Where(x => !string.IsNullOrWhiteSpace(x.MaSanPham))
            .GroupBy(x => x.MaSanPham.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            error = $"Sản phẩm [{duplicate.Key}] bị trùng lặp trong biên bản kiểm kê. Mỗi sản phẩm chỉ được ghi nhận một dòng duy nhất.";
            return false;
        }

        // 2. Kiểm tra tính hợp lệ của từng mặt hàng và đối chiếu số lượng tồn sổ sách
        foreach (var item in request.DanhSachChiTiet)
        {
            if (string.IsNullOrWhiteSpace(item.MaSanPham))
            {
                error = "Mã sản phẩm trong danh sách kiểm kê không được để trống.";
                return false;
            }

            var sp = _sanPhamRepo.TimTheoId(item.MaSanPham.Trim());
            if (sp == null)
            {
                error = $"Sản phẩm [{item.MaSanPham}] không tồn tại trong danh mục hệ thống.";
                return false;
            }

            if (item.TonThucTe < 0)
            {
                error = $"Số lượng tồn thực tế của sản phẩm [{item.MaSanPham} - {sp.TenSanPham}] phải là số nguyên không âm (>= 0).";
                return false;
            }

            // Đối chiếu tồn sổ sách gửi lên phải khớp 100% với tồn kho thực tế của hệ thống tại thời điểm bấm lưu
            if (item.TonSoSach != sp.SoLuongTon)
            {
                error = $"Tồn sổ sách của sản phẩm [{item.MaSanPham} - {sp.TenSanPham}] không khớp với hệ thống tại thời điểm lập phiếu (Phiếu ghi: {item.TonSoSach}, Hệ thống thực tế: {sp.SoLuongTon}). Vui lòng tải lại dữ liệu mới nhất trước khi lưu biên bản.";
                return false;
            }
        }

        var maPhieu = string.IsNullOrWhiteSpace(request.MaPhieuKiemKe)
            ? $"PKK_{DateTime.UtcNow.Ticks % 1000000:D6}"
            : request.MaPhieuKiemKe.Trim();

        var phieu = new PhieuKiemKe(
            maPhieuKiemKe: maPhieu,
            viTriKiemKe: request.ViTriKiemKe.Trim(),
            maNhanVienKho: request.MaNhanVienKho.Trim(),
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

        // Bước 1: Ràng buộc kiểm tra tính hợp lệ của tồn kho sổ sách trước khi duyệt (Anti-Conflict)
        foreach (var ct in phieu.ChiTietKiemKes)
        {
            var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
            if (sp == null)
            {
                error = $"Sản phẩm [{ct.MaSanPham}] không còn tồn tại trong danh mục hệ thống. Không thể phê duyệt biên bản kiểm kê này.";
                return false;
            }

            var tonKhoHienTai = sp.SoLuongTon;

            if (tonKhoHienTai != ct.TonSoSach)
            {
                var tenSp = !string.IsNullOrWhiteSpace(sp.TenSanPham) ? sp.TenSanPham : ct.MaSanPham;
                error = $"Tồn kho sản phẩm [{ct.MaSanPham} - {tenSp}] đã có sự biến động trên hệ thống (Lúc lập phiếu ghi nhận: {ct.TonSoSach} chiếc, Tồn kho thực tế hiện tại: {tonKhoHienTai} chiếc do phát sinh giao dịch bán hàng hoặc nhập kho mới).\n\n" +
                        "Để bảo vệ tính toàn vẹn số liệu kế toán và không xóa mất lịch sử giao dịch phát sinh, hệ thống từ chối duyệt phiếu này.\n" +
                        "Vui lòng từ chối/hủy phiếu và lập biên bản kiểm kê mới đối với số liệu tồn kho hiện hành.";
                return false;
            }
        }

        // Bước 2: Cập nhật trạng thái phiếu sang DaDuyet trong kho lưu trữ
        _kiemKeRepo.CapNhatTrangThai(maPhieu, "DaDuyet", DateTime.UtcNow);

        // Bước 3: Đồng bộ số lượng tồn kho và cập nhật trạng thái kinh doanh tương ứng của sản phẩm
        foreach (var ct in phieu.ChiTietKiemKes)
        {
            var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
            if (sp != null)
            {
                var newStock = Math.Max(0, ct.TonThucTe);
                sp.SoLuongTon = newStock;

                // Tự động đồng bộ trạng thái kinh doanh tương ứng với số lượng tồn thực tế sau kiểm kê
                if (newStock == 0)
                {
                    sp.CapNhatTrangThai("HetHang");
                }
                else if (newStock > 0 && (string.Equals(sp.TrangThai, "HetHang", StringComparison.OrdinalIgnoreCase) || string.Equals(sp.TrangThai, "DaBan", StringComparison.OrdinalIgnoreCase)))
                {
                    sp.CapNhatTrangThai("SanSang");
                }

                _sanPhamRepo.LuuSanPham(sp);
            }
        }

        return true;
    }
}
