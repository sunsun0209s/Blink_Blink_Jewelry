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
