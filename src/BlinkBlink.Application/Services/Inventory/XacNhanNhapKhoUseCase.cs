using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Inventory;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Inventory;

/// <summary>
/// Use Case Xác nhận nhập kho (XacNhanNhapKhoUseCase).
/// Xử lý tạo phiếu nhập kho, thêm các dòng chi tiết sản phẩm, tính toán giá trị và cập nhật tồn kho.
/// Khớp 100% với Class Diagram.
/// </summary>
public class XacNhanNhapKhoUseCase
{
    private readonly IPhieuNhapKhoRepository _nhapKhoRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public XacNhanNhapKhoUseCase(IPhieuNhapKhoRepository nhapKhoRepo, ISanPhamRepository sanPhamRepo)
    {
        _nhapKhoRepo = nhapKhoRepo ?? throw new ArgumentNullException(nameof(nhapKhoRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
    }

    /// <summary>
    /// Thực thi quy trình lập và xác nhận phiếu nhập kho.
    /// Khớp hàm: + Execute(request: NhapKhoDTO): void trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin phiếu nhập và danh sách sản phẩm nhập kho</param>
    public void Execute(NhapKhoDTO request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Yêu cầu nhập kho không được để trống.");
        }

        var maPhieu = string.IsNullOrWhiteSpace(request.MaPhieuNhap)
            ? $"PNK_{DateTime.UtcNow.Ticks % 1000000:D6}"
            : request.MaPhieuNhap.Trim();

        var phieu = new PhieuNhapKho(
            maPhieuNhap: maPhieu,
            nhaCungCap: request.NhaCungCap?.Trim() ?? string.Empty,
            maNhanVien: request.MaNhanVien?.Trim() ?? string.Empty,
            ngayNhap: DateTime.UtcNow,
            trangThai: "ChoDuyet"
        );

        if (request.DanhSachSanPham != null)
        {
            foreach (var item in request.DanhSachSanPham)
            {
                var chiTiet = new ChiTietNhapKho(
                    maPhieuNhap: maPhieu,
                    maSanPham: item.MaSanPham?.Trim() ?? string.Empty,
                    soLuong: item.SoLuong,
                    giaVonNhap: item.GiaVonNhap,
                    soChungTu: item.SoChungTu?.Trim() ?? string.Empty
                );

                phieu.ThemChiTiet(chiTiet);
            }
        }

        // Xác nhận tính toán tổng tiền và số lượng
        phieu.XacNhanNhapKho();

        // Lưu phiếu và danh sách chi tiết ở trạng thái "ChoDuyet" (chưa tác động tồn kho và giá)
        _nhapKhoRepo.LuuPhieuNhap(phieu);
    }

    /// <summary>
    /// Phê duyệt phiếu nhập kho (dành riêng cho Quản trị viên Admin).
    /// Khi duyệt thành công, hệ thống mới chính thức cộng dồn số lượng tồn kho,
    /// tính toán lại giá nhập bình quân gia quyền và tự động cập nhật lại đơn giá bán theo % lợi nhuận.
    /// </summary>
    /// <param name="maPhieuNhap">Mã phiếu nhập cần duyệt</param>
    /// <param name="maAdmin">Mã nhân sự quản trị duyệt phiếu</param>
    /// <param name="error">Thông báo lỗi nếu không thể duyệt</param>
    /// <returns>True nếu duyệt thành công</returns>
    public bool DuyetPhieuNhap(string maPhieuNhap, string maAdmin, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(maPhieuNhap))
        {
            error = "Mã phiếu nhập không hợp lệ.";
            return false;
        }

        var phieu = _nhapKhoRepo.TimTheoId(maPhieuNhap);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu nhập kho #{maPhieuNhap}.";
            return false;
        }

        if (!string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Phiếu nhập kho #{maPhieuNhap} không ở trạng thái Chờ duyệt (Trạng thái hiện tại: {phieu.TrangThai}).";
            return false;
        }

        // 1. Cập nhật trạng thái phiếu nhập sang "DaDuyet"
        _nhapKhoRepo.DuyetPhieuNhap(maPhieuNhap);

        // 2. Lấy toàn bộ phiếu đã duyệt (bao gồm phiếu vừa được duyệt) để tính lại tồn kho và giá bình quân
        var allApprovedReceipts = _nhapKhoRepo.LayTatCa()
            .Where(p => string.Equals(p.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) || p.MaPhieuNhap == maPhieuNhap)
            .ToList();

        if (phieu.ChiTietNhapKhos != null)
        {
            foreach (var ct in phieu.ChiTietNhapKhos)
            {
                var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
                if (sp != null)
                {
                    // Cộng dồn số lượng tồn kho
                    sp.SoLuongTon += ct.SoLuong;
                    if (string.Equals(sp.TrangThai, "HetHang", StringComparison.OrdinalIgnoreCase) && sp.SoLuongTon > 0)
                    {
                        sp.TrangThai = "SanSang";
                    }

                    // Tính toán lại giá nhập bình quân gia quyền từ tất cả các phiếu nhập đã duyệt
                    var validDetailsForSp = allApprovedReceipts
                        .SelectMany(r => r.ChiTietNhapKhos ?? Enumerable.Empty<ChiTietNhapKho>())
                        .Where(c => c.MaSanPham == ct.MaSanPham && c.SoLuong > 0)
                        .ToList();

                    if (validDetailsForSp.Count > 0)
                    {
                        var tongGiaTri = validDetailsForSp.Sum(c => c.SoLuong * c.GiaVonNhap);
                        var tongSoLuong = validDetailsForSp.Sum(c => c.SoLuong);
                        if (tongSoLuong > 0)
                        {
                            sp.GiaNhap = Math.Round(tongGiaTri / tongSoLuong, 2);
                        }
                    }

                    // TỰ ĐỘNG CẬP NHẬT GIÁ BÁN THEO TỶ LỆ LỢI NHUẬN:
                    // Giá bán = Giá nhập bình quân * (1 + % Lợi nhuận)
                    sp.GiaBan = sp.TinhGiaBanTheoLoiNhuan();

                    _sanPhamRepo.LuuSanPham(sp);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Xóa phiếu nhập kho chưa được duyệt (cả nhân viên kho và admin đều dùng được).
    /// </summary>
    /// <param name="maPhieuNhap">Mã phiếu nhập cần xóa</param>
    /// <param name="error">Lỗi nếu thất bại</param>
    /// <returns>True nếu xóa thành công</returns>
    public bool XoaPhieuNhap(string maPhieuNhap, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(maPhieuNhap))
        {
            error = "Mã phiếu nhập không hợp lệ.";
            return false;
        }

        var phieu = _nhapKhoRepo.TimTheoId(maPhieuNhap);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu nhập kho #{maPhieuNhap}.";
            return false;
        }

        if (!string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            error = "Chỉ được phép xóa phiếu nhập kho chưa được phê duyệt (trạng thái Chờ duyệt).";
            return false;
        }

        _nhapKhoRepo.XoaPhieuNhap(maPhieuNhap);
        return true;
    }

    /// <summary>
    /// Vô hiệu hóa / Hủy phiếu nhập kho (dành riêng cho Quản trị viên Admin).
    /// Điều kiện ràng buộc: Khi tồn kho trừ đi số lượng của phiếu nhập đó,
    /// tồn kho của bất kỳ sản phẩm nào trong phiếu sau khi trừ KHÔNG ĐƯỢC ÂM (< 0).
    /// </summary>
    /// <param name="maPhieuNhap">Mã phiếu nhập cần hủy</param>
    /// <param name="maAdmin">Mã tài khoản quản trị viên thực hiện</param>
    /// <param name="error">Thông điệp lỗi nếu thất bại</param>
    /// <returns>True nếu hủy thành công</returns>
    public bool VoHieuHoaPhieuNhap(string maPhieuNhap, string maAdmin, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(maPhieuNhap))
        {
            error = "Mã phiếu nhập không hợp lệ.";
            return false;
        }

        var phieu = _nhapKhoRepo.TimTheoId(maPhieuNhap);
        if (phieu == null)
        {
            error = $"Không tìm thấy phiếu nhập kho #{maPhieuNhap}.";
            return false;
        }

        if (string.Equals(phieu.TrangThai, "DaHuy", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Phiếu nhập kho #{maPhieuNhap} đã bị hủy trước đó.";
            return false;
        }

        // Nếu phiếu chưa duyệt (ChoDuyet) mà hủy thì chỉ cần đổi trạng thái sang DaHuy
        if (string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            _nhapKhoRepo.VoHieuHoaPhieuNhap(maPhieuNhap);
            return true;
        }

        // ĐỐI VỚI PHIẾU ĐÃ DUYỆT (DaDuyet): KIỂM TRA ĐIỀU KIỆN TỒN KHO SAU TRỪ KHÔNG ĐƯỢC ÂM
        if (phieu.ChiTietNhapKhos != null)
        {
            foreach (var ct in phieu.ChiTietNhapKhos)
            {
                var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
                var tonKhoHienTai = sp?.SoLuongTon ?? 0;
                if (tonKhoHienTai - ct.SoLuong < 0)
                {
                    error = $"Không thể hủy phiếu nhập: Sản phẩm [{ct.MaSanPham}] '{sp?.TenSanPham}' hiện chỉ còn tồn {tonKhoHienTai}, nếu trừ {ct.SoLuong} sẽ bị âm ({tonKhoHienTai - ct.SoLuong}).";
                    return false;
                }
            }
        }

        // 1. Đánh dấu hủy phiếu trong cơ sở dữ liệu
        _nhapKhoRepo.VoHieuHoaPhieuNhap(maPhieuNhap);

        // 2. Cập nhật hoàn trừ tồn kho và tính lại giá vốn cho các sản phẩm trong phiếu
        var allApprovedReceipts = _nhapKhoRepo.LayTatCa()
            .Where(p => string.Equals(p.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) && p.MaPhieuNhap != maPhieuNhap)
            .ToList();

        if (phieu.ChiTietNhapKhos != null)
        {
            foreach (var ct in phieu.ChiTietNhapKhos)
            {
                var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
                if (sp != null)
                {
                    // Trừ số lượng tồn kho tương ứng với đợt nhập bị hủy
                    sp.SoLuongTon = Math.Max(0, sp.SoLuongTon - ct.SoLuong);
                    if (sp.SoLuongTon == 0)
                    {
                        sp.TrangThai = "HetHang";
                    }

                    // Tính lại giá nhập bình quân theo các phiếu nhập đã duyệt còn lại
                    var validDetailsForSp = allApprovedReceipts
                        .SelectMany(r => r.ChiTietNhapKhos ?? Enumerable.Empty<ChiTietNhapKho>())
                        .Where(c => c.MaSanPham == ct.MaSanPham && c.SoLuong > 0)
                        .ToList();

                    if (validDetailsForSp.Count > 0)
                    {
                        var tongGiaTriConLai = validDetailsForSp.Sum(c => c.SoLuong * c.GiaVonNhap);
                        var tongSoLuongConLai = validDetailsForSp.Sum(c => c.SoLuong);
                        if (tongSoLuongConLai > 0)
                        {
                            sp.GiaNhap = Math.Round(tongGiaTriConLai / tongSoLuongConLai, 2);
                        }
                    }

                    // TÍNH LẠI GIÁ BÁN THEO TỶ LỆ LỢI NHUẬN:
                    sp.GiaBan = sp.TinhGiaBanTheoLoiNhuan();

                    _sanPhamRepo.LuuSanPham(sp);
                }
            }
        }

        return true;
    }

    // =========================================================================
    // MỞ RỘNG BẤT ĐỒNG BỘ CHUẨN CLEAN ARCHITECTURE .NET 8 LTS
    // =========================================================================

    public async Task ExecuteAsync(NhapKhoDTO request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Yêu cầu nhập kho không được để trống.");
        }

        var maPhieu = string.IsNullOrWhiteSpace(request.MaPhieuNhap)
            ? $"PNK_{DateTime.UtcNow.Ticks % 1000000:D6}"
            : request.MaPhieuNhap.Trim();

        var phieu = new PhieuNhapKho(
            maPhieuNhap: maPhieu,
            nhaCungCap: request.NhaCungCap?.Trim() ?? string.Empty,
            maNhanVien: request.MaNhanVien?.Trim() ?? string.Empty,
            ngayNhap: DateTime.UtcNow
        );

        if (request.DanhSachSanPham != null)
        {
            foreach (var item in request.DanhSachSanPham)
            {
                var chiTiet = new ChiTietNhapKho(
                    maPhieuNhap: maPhieu,
                    maSanPham: item.MaSanPham?.Trim() ?? string.Empty,
                    soLuong: item.SoLuong,
                    giaVonNhap: item.GiaVonNhap,
                    soChungTu: item.SoChungTu?.Trim() ?? string.Empty
                );

                phieu.ThemChiTiet(chiTiet);
            }
        }

        phieu.XacNhanNhapKho();
        await _nhapKhoRepo.LuuPhieuNhapAsync(phieu, cancellationToken);
    }
}

/// <summary>
/// Hỗ trợ tính giá bán theo tỷ lệ lợi nhuận an toàn khi module Catalog đang được đồng bộ song song.
/// Khi SanPhamTrangSuc được cập nhật phương thức instance, trình biên dịch C# sẽ tự động ưu tiên instance method.
/// </summary>
internal static class SanPhamTrangSucPricingExtensions
{
    public static decimal TinhGiaBanTheoLoiNhuan(this SanPhamTrangSuc sp)
    {
        var method = sp.GetType().GetMethod("TinhGiaBanTheoLoiNhuan");
        if (method != null)
        {
            return (decimal)method.Invoke(sp, null)!;
        }

        var prop = sp.GetType().GetProperty("TyLeLoiNhuan");
        if (prop != null)
        {
            var tyLe = (decimal)prop.GetValue(sp)!;
            return Math.Round(sp.GiaNhap * (1m + (tyLe / 100m)), 0);
        }

        return sp.GiaBan;
    }
}
