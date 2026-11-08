using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Warranty;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Warranty;

/// <summary>
/// Trường hợp sử dụng Tiếp nhận bảo hành (TiepNhanBaoHanhUseCase) dành cho nhân viên bán hàng.
/// Thực hiện tạo phiếu bảo hành mới với trạng thái TiepNhan và ghi nhận tình trạng sản phẩm.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public class TiepNhanBaoHanhUseCase
{
    private readonly IBaoHanhRepository _baoHanhRepo;
    private readonly ISanPhamRepository _sanPhamRepo;
    private readonly IKhachHangRepository? _khachHangRepo;

    public TiepNhanBaoHanhUseCase(
        IBaoHanhRepository baoHanhRepo,
        ISanPhamRepository sanPhamRepo,
        IKhachHangRepository? khachHangRepo = null)
    {
        _baoHanhRepo = baoHanhRepo ?? throw new ArgumentNullException(nameof(baoHanhRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
        _khachHangRepo = khachHangRepo;
    }

    /// <summary>
    /// Tiếp nhận sản phẩm và lập phiếu bảo hành mới (đồng bộ).
    /// Khớp phương thức: + Execute(request: TaoBaoHanhDTO): bool trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu bảo hành</param>
    /// <returns>True nếu tiếp nhận thành công, False nếu dữ liệu không hợp lệ</returns>
    public bool Execute(TaoBaoHanhDTO request)
    {
        return ExecuteAsync(request).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Tiếp nhận sản phẩm và lập phiếu bảo hành mới (bất đồng bộ).
    /// </summary>
    /// <param name="request">Thông tin yêu cầu bảo hành</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>True nếu tiếp nhận thành công, False nếu dữ liệu không hợp lệ</returns>
    public async Task<bool> ExecuteAsync(TaoBaoHanhDTO request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return false;
        }

        // Bắt buộc phải có mã sản phẩm và lý do tiếp nhận bảo hành
        if (string.IsNullOrWhiteSpace(request.MaSanPham) ||
            string.IsNullOrWhiteSpace(request.LyDoBaoHanh))
        {
            return false;
        }

        // Xác định mã khách hàng: Nếu để trống hoặc là '0', coi là khách vãng lai
        var maKhachHang = string.IsNullOrWhiteSpace(request.MaKhachHang) ? "0" : request.MaKhachHang.Trim();
        var soDienThoai = request.SoDienThoai?.Trim() ?? string.Empty;

        // Nếu là khách vãng lai và có cung cấp số điện thoại, thử tìm xem khách đã có hồ sơ chưa
        if (maKhachHang == "0" && !string.IsNullOrWhiteSpace(soDienThoai) && _khachHangRepo != null)
        {
            var existingKh = _khachHangRepo.TimTheoSoDienThoai(soDienThoai);
            if (existingKh != null)
            {
                maKhachHang = !string.IsNullOrWhiteSpace(existingKh.MaNguoiDung) ? existingKh.MaNguoiDung : existingKh.MaKhachHang;
            }
        }
        else if (maKhachHang != "0" && _khachHangRepo != null)
        {
            var existingKh = _khachHangRepo.TimTheoMa(maKhachHang);
            if (existingKh != null && !string.IsNullOrWhiteSpace(existingKh.MaNguoiDung))
            {
                maKhachHang = existingKh.MaNguoiDung;
            }
        }

        try
        {
            // Kiểm tra sản phẩm có tồn tại trong hệ thống hay không
        var sanPham = await _sanPhamRepo.TimTheoIdAsync(request.MaSanPham, cancellationToken);
        if (sanPham == null)
        {
            return false;
        }

        // Sinh số phiếu bảo hành duy nhất: Tiền tố BH + NămThángNgàyGiờPhútGiây + Số ngẫu nhiên
        var soPhieu = $"BH{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        var phieuBaoHanh = new PhieuBaoHanh(
            soPhieuBH: soPhieu,
            maKhachHang: maKhachHang,
            maSanPham: request.MaSanPham.Trim(),
            lyDoBaoHanh: request.LyDoBaoHanh.Trim(),
            chiPhiUocTinh: Math.Max(0m, request.ChiPhiBaoHanh),
            trangThaiXuLy: "TiepNhan",
            ngayTiepNhan: DateTime.UtcNow,
            ngayHenTra: request.NgayHenTra,
            chiPhiThucTe: 0m,
            tinhTrangTiepNhan: request.TinhTrangTiepNhan?.Trim() ?? string.Empty,
            ghiChu: request.GhiChu?.Trim() ?? string.Empty,
            soDienThoai: soDienThoai
        );

        // Chuyển trạng thái vòng đời sản phẩm trang sức kim hoàn sang đang bảo hành
        sanPham.CapNhatTrangThai("DangBaoHanh");
        await _sanPhamRepo.LuuSanPhamAsync(sanPham, cancellationToken);

        // Lưu phiếu bảo hành vào cơ sở dữ liệu
        await _baoHanhRepo.LuuPhieuAsync(phieuBaoHanh, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
