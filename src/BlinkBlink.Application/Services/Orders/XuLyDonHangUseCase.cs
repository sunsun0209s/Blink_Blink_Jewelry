using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Orders;

/// <summary>
/// Trường hợp sử dụng Xử lý đơn hàng (XuLyDonHangUseCase) dành cho Nhân viên / Quản trị viên.
/// Thực hiện các nghiệp vụ: Phê duyệt đơn, Chuyển trạng thái giao vận và Hủy đơn.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class XuLyDonHangUseCase
{
    private readonly IHoaDonRepository _hoaDonRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public XuLyDonHangUseCase(
        IHoaDonRepository hoaDonRepo,
        ISanPhamRepository sanPhamRepo)
    {
        _hoaDonRepo = hoaDonRepo ?? throw new ArgumentNullException(nameof(hoaDonRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
    }

    /// <summary>
    /// Phê duyệt đơn đặt hàng online của khách hàng (đồng bộ).
    /// Khớp phương thức: + DuyetDonHang(maHoaDon: string, maNhanVien: string): bool trong Diagram.
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn cần duyệt</param>
    /// <param name="maNhanVien">Mã nhân viên duyệt đơn</param>
    /// <returns>True nếu duyệt thành công, False nếu hóa đơn không tồn tại hoặc đã xử lý</returns>
    public bool DuyetDonHang(string maHoaDon, string maNhanVien)
    {
        return DuyetDonHangAsync(maHoaDon, maNhanVien).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Phê duyệt đơn đặt hàng online của khách hàng (bất đồng bộ).
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn cần duyệt</param>
    /// <param name="maNhanVien">Mã nhân viên duyệt đơn</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>True nếu duyệt thành công, False nếu dữ liệu không hợp lệ</returns>
    public async Task<bool> DuyetDonHangAsync(string maHoaDon, string maNhanVien, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon) || string.IsNullOrWhiteSpace(maNhanVien))
        {
            return false;
        }

        var hoaDon = await _hoaDonRepo.TimTheoMaAsync(maHoaDon, cancellationToken);
        if (hoaDon == null)
        {
            return false;
        }

        // Chỉ duyệt các đơn đang ở trạng thái chờ duyệt
        if (!string.Equals(hoaDon.TrangThaiDonHang, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Chuyển sang trạng thái đang đóng gói và gán nhân viên duyệt
        hoaDon.CapNhatTrangThai("DangDongGoi", maNhanVien);

        // Đánh dấu chuyển trạng thái các sản phẩm trong đơn sang Đã bán
        if (hoaDon.ChiTietHoaDons != null)
        {
            foreach (var item in hoaDon.ChiTietHoaDons)
            {
                var sp = await _sanPhamRepo.TimTheoIdAsync(item.MaSanPham, cancellationToken);
                if (sp != null)
                {
                    sp.CapNhatTrangThai("DaBan");
                    await _sanPhamRepo.LuuSanPhamAsync(sp, cancellationToken);
                }
            }
        }

        await _hoaDonRepo.CapNhatHoaDonAsync(hoaDon, cancellationToken);
        return true;
    }

    /// <summary>
    /// Cập nhật tiến độ giao vận hoặc trạng thái đơn hàng (đồng bộ).
    /// Khớp phương thức: + CapNhatTrangThai(maHoaDon: string, trangThaiMoi: string, maNhanVien: string): bool trong Diagram.
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn</param>
    /// <param name="trangThaiMoi">Trạng thái mới: DangDongGoi, DangGiao, DaGiao, DaHuy</param>
    /// <param name="maNhanVien">Mã nhân viên cập nhật</param>
    /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy đơn</returns>
    public bool CapNhatTrangThai(string maHoaDon, string trangThaiMoi, string maNhanVien)
    {
        return CapNhatTrangThaiAsync(maHoaDon, trangThaiMoi, maNhanVien).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Cập nhật tiến độ giao vận hoặc trạng thái đơn hàng (bất đồng bộ).
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn</param>
    /// <param name="trangThaiMoi">Trạng thái mới: DangDongGoi, DangGiao, DaGiao, DaHuy</param>
    /// <param name="maNhanVien">Mã nhân viên cập nhật</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy đơn</returns>
    public async Task<bool> CapNhatTrangThaiAsync(string maHoaDon, string trangThaiMoi, string maNhanVien, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon) || string.IsNullOrWhiteSpace(trangThaiMoi))
        {
            return false;
        }

        var hoaDon = await _hoaDonRepo.TimTheoMaAsync(maHoaDon, cancellationToken);
        if (hoaDon == null)
        {
            return false;
        }

        // Nếu trạng thái mới là hủy đơn, điều hướng về quy trình hủy để xử lý hoàn kho
        if (string.Equals(trangThaiMoi, "DaHuy", StringComparison.OrdinalIgnoreCase))
        {
            return await HuyDonHangAsync(maHoaDon, cancellationToken);
        }

        var success = hoaDon.CapNhatTrangThai(trangThaiMoi, maNhanVien);
        if (!success)
        {
            return false;
        }

        await _hoaDonRepo.CapNhatHoaDonAsync(hoaDon, cancellationToken);
        return true;
    }

    /// <summary>
    /// Hủy đơn đặt hàng và hoàn trả trạng thái sản phẩm về sẵn sàng (đồng bộ).
    /// Khớp phương thức: + HuyDonHang(maHoaDon: string): bool trong Diagram.
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn cần hủy</param>
    /// <returns>True nếu hủy thành công, False nếu đơn không tồn tại hoặc đã giao</returns>
    public bool HuyDonHang(string maHoaDon)
    {
        return HuyDonHangAsync(maHoaDon).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Hủy đơn đặt hàng và hoàn trả trạng thái sản phẩm về sẵn sàng (bất đồng bộ).
    /// </summary>
    /// <param name="maHoaDon">Mã hóa đơn cần hủy</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>True nếu hủy thành công, False nếu đơn không tồn tại hoặc đã hoàn tất</returns>
    public async Task<bool> HuyDonHangAsync(string maHoaDon, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon))
        {
            return false;
        }

        var hoaDon = await _hoaDonRepo.TimTheoMaAsync(maHoaDon, cancellationToken);
        if (hoaDon == null)
        {
            return false;
        }

        var success = hoaDon.HuyHoaDon();
        if (!success)
        {
            return false;
        }

        // Hoàn lại trạng thái sẵn sàng trong kho cho các món trang sức của đơn hủy
        if (hoaDon.ChiTietHoaDons != null)
        {
            foreach (var item in hoaDon.ChiTietHoaDons)
            {
                var sp = await _sanPhamRepo.TimTheoIdAsync(item.MaSanPham, cancellationToken);
                if (sp != null && string.Equals(sp.TrangThai, "DaBan", StringComparison.OrdinalIgnoreCase))
                {
                    sp.CapNhatTrangThai("SanSang");
                    await _sanPhamRepo.LuuSanPhamAsync(sp, cancellationToken);
                }
            }
        }

        await _hoaDonRepo.CapNhatHoaDonAsync(hoaDon, cancellationToken);
        return true;
    }
}
