using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Warranty;

/// <summary>
/// Trường hợp sử dụng Xử lý bảo hành (XuLyBaoHanhUseCase) dành cho nhân viên kỹ thuật / quản trị viên.
/// Thực hiện cập nhật tiến độ xử lý và hoàn tất quyết toán dịch vụ bảo hành trang sức.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public class XuLyBaoHanhUseCase
{
    private readonly IBaoHanhRepository _baoHanhRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public XuLyBaoHanhUseCase(
        IBaoHanhRepository baoHanhRepo,
        ISanPhamRepository sanPhamRepo)
    {
        _baoHanhRepo = baoHanhRepo ?? throw new ArgumentNullException(nameof(baoHanhRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
    }

    /// <summary>
    /// Cập nhật tiến độ xử lý của phiếu bảo hành (đồng bộ).
    /// Khớp phương thức: + CapNhatTienDo(soPhieu: string, trangThaiMoi: string): bool trong Diagram.
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành</param>
    /// <param name="trangThaiMoi">Trạng thái mới: DangKiemDinh, DangXuLy, ChoKhachNhan</param>
    /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy phiếu</returns>
    public bool CapNhatTienDo(string soPhieu, string trangThaiMoi)
    {
        return CapNhatTienDoAsync(soPhieu, trangThaiMoi).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Cập nhật tiến độ xử lý của phiếu bảo hành (bất đồng bộ).
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành</param>
    /// <param name="trangThaiMoi">Trạng thái mới</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy phiếu</returns>
    public async Task<bool> CapNhatTienDoAsync(string soPhieu, string trangThaiMoi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(soPhieu) || string.IsNullOrWhiteSpace(trangThaiMoi))
        {
            return false;
        }

        var phieu = await _baoHanhRepo.TimTheoSoPhieuAsync(soPhieu, cancellationToken);
        if (phieu == null)
        {
            return false;
        }

        var success = phieu.CapNhatTrangThai(trangThaiMoi);
        if (!success)
        {
            return false;
        }

        await _baoHanhRepo.CapNhatPhieuAsync(phieu, cancellationToken);
        return true;
    }

    /// <summary>
    /// Quyết toán chi phí sửa chữa thực tế, chuyển trạng thái sang DaXuLy (bảo hành xong chờ khách nhận).
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành</param>
    /// <param name="chiPhiThucTe">Chi phí thực tế phát sinh</param>
    /// <returns>True nếu quyết toán thành công</returns>
    public bool QuyetToanBaoHanh(string soPhieu, decimal chiPhiThucTe)
    {
        return QuyetToanBaoHanhAsync(soPhieu, chiPhiThucTe).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Quyết toán chi phí sửa chữa thực tế (bất đồng bộ).
    /// </summary>
    public async Task<bool> QuyetToanBaoHanhAsync(string soPhieu, decimal chiPhiThucTe, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(soPhieu) || chiPhiThucTe < 0)
        {
            return false;
        }

        var phieu = await _baoHanhRepo.TimTheoSoPhieuAsync(soPhieu, cancellationToken);
        if (phieu == null)
        {
            return false;
        }

        phieu.QuyetToan(chiPhiThucTe);
        await _baoHanhRepo.CapNhatPhieuAsync(phieu, cancellationToken);
        return true;
    }

    /// <summary>
    /// Hoàn tất dịch vụ bảo hành khi khách hàng đến nhận lại trang sức (đồng bộ).
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành</param>
    /// <param name="chiPhiBaoHanh">Chi phí thực tế (nếu chưa quyết toán trước đó)</param>
    /// <returns>True nếu hoàn tất thành công, False nếu phiếu không tồn tại</returns>
    public bool HoanTatBaoHanh(string soPhieu, decimal chiPhiBaoHanh = 0m)
    {
        return HoanTatBaoHanhAsync(soPhieu, chiPhiBaoHanh).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Hoàn tất dịch vụ bảo hành khi khách hàng đến nhận lại trang sức (bất đồng bộ).
    /// </summary>
    public async Task<bool> HoanTatBaoHanhAsync(string soPhieu, decimal chiPhiBaoHanh = 0m, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(soPhieu))
        {
            return false;
        }

        var phieu = await _baoHanhRepo.TimTheoSoPhieuAsync(soPhieu, cancellationToken);
        if (phieu == null)
        {
            return false;
        }

        if (chiPhiBaoHanh > 0 && phieu.ChiPhiThucTe == 0)
        {
            phieu.ChiPhiThucTe = chiPhiBaoHanh;
        }

        phieu.HoanTatBaoHanh();

        // Hoàn trả trạng thái của món trang sức từ DangBaoHanh về DaBan
        var sanPham = await _sanPhamRepo.TimTheoIdAsync(phieu.MaSanPham, cancellationToken);
        if (sanPham != null && string.Equals(sanPham.TrangThai, "DangBaoHanh", StringComparison.OrdinalIgnoreCase))
        {
            sanPham.CapNhatTrangThai("DaBan");
            await _sanPhamRepo.LuuSanPhamAsync(sanPham, cancellationToken);
        }

        await _baoHanhRepo.CapNhatPhieuAsync(phieu, cancellationToken);
        return true;
    }
}
