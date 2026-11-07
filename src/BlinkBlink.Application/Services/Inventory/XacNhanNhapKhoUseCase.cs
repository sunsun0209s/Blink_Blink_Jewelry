using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Inventory;
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

        // Xác nhận tính toán tổng tiền và số lượng
        phieu.XacNhanNhapKho();

        // Lưu phiếu và danh sách chi tiết vào cơ sở dữ liệu
        _nhapKhoRepo.LuuPhieuNhap(phieu);
    }

    /// <summary>
    /// Vô hiệu hóa phiếu nhập kho (dành riêng cho Quản trị viên Admin).
    /// Tự động hoàn trừ số lượng tồn kho tương ứng và tính lại giá nhập bình quân như thể phiếu này chưa từng tồn tại.
    /// Tuyệt đối không cho phép chỉnh sửa phiếu nhập.
    /// </summary>
    /// <param name="maPhieuNhap">Mã phiếu nhập cần vô hiệu hóa</param>
    /// <param name="maAdmin">Mã tài khoản quản trị viên thực hiện</param>
    /// <param name="error">Thông điệp lỗi nếu thất bại</param>
    /// <returns>True nếu vô hiệu hóa thành công</returns>
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

        if (!phieu.TrangThai)
        {
            error = $"Phiếu nhập kho #{maPhieuNhap} đã bị vô hiệu hóa trước đó.";
            return false;
        }

        // 1. Đánh dấu vô hiệu hóa trong cơ sở dữ liệu
        _nhapKhoRepo.VoHieuHoaPhieuNhap(maPhieuNhap);

        // 2. Cập nhật hoàn trừ tồn kho và tính lại giá vốn cho các sản phẩm trong phiếu
        var allReceipts = _nhapKhoRepo.LayTatCa().Where(p => p.TrangThai && p.MaPhieuNhap != maPhieuNhap).ToList();

        if (phieu.ChiTietNhapKhos != null)
        {
            foreach (var ct in phieu.ChiTietNhapKhos)
            {
                var sp = _sanPhamRepo.TimTheoId(ct.MaSanPham);
                if (sp != null)
                {
                    // Trừ số lượng tồn kho tương ứng với đợt nhập bị vô hiệu hóa
                    sp.SoLuongTon = Math.Max(0, sp.SoLuongTon - ct.SoLuong);

                    // Tính lại giá nhập bình quân theo các phiếu nhập còn hợp lệ
                    var validDetailsForSp = allReceipts
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
