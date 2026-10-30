using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Warranty;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Warranty;

/// <summary>
/// Trường hợp sử dụng Tiếp nhận bảo hành (TiepNhanBaoHanhUseCase) dành cho nhân viên.
/// Thực hiện tạo phiếu bảo hành mới và chuyển trạng thái món trang sức sang DangBaoHanh.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public class TiepNhanBaoHanhUseCase
{
    private readonly IBaoHanhRepository _baoHanhRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public TiepNhanBaoHanhUseCase(
        IBaoHanhRepository baoHanhRepo,
        ISanPhamRepository sanPhamRepo)
    {
        _baoHanhRepo = baoHanhRepo ?? throw new ArgumentNullException(nameof(baoHanhRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
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

        // Bắt buộc phải có mã khách hàng, mã sản phẩm và lý do tiếp nhận bảo hành
        if (string.IsNullOrWhiteSpace(request.MaKhachHang) ||
            string.IsNullOrWhiteSpace(request.MaSanPham) ||
            string.IsNullOrWhiteSpace(request.LyDoBaoHanh))
        {
            return false;
        }

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
            maKhachHang: request.MaKhachHang.Trim(),
            maSanPham: request.MaSanPham.Trim(),
            lyDoBaoHanh: request.LyDoBaoHanh.Trim(),
            chiPhiUocTinh: Math.Max(0m, request.ChiPhiBaoHanh),
            trangThaiXuLy: "DangBaoHanh",
            ngayTiepNhan: DateTime.UtcNow,
            ngayHenTra: request.NgayHenTra
        );

        // Chuyển trạng thái vòng đời sản phẩm trang sức sang đang bảo hành để ngăn chặn bán/giao dịch
        sanPham.CapNhatTrangThai("DangBaoHanh");
        await _sanPhamRepo.LuuSanPhamAsync(sanPham, cancellationToken);

        // Lưu phiếu bảo hành vào cơ sở dữ liệu
        await _baoHanhRepo.LuuPhieuAsync(phieuBaoHanh, cancellationToken);

        return true;
    }
}
