using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Warranty;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể PhieuBaoHanh (Phiếu bảo hành & Dịch vụ hậu mãi).
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public interface IBaoHanhRepository
{
    /// <summary>
    /// Lưu phiếu bảo hành mới vào cơ sở dữ liệu (đồng bộ).
    /// Khớp phương thức: + LuuPhieu(pbh: PhieuBaoHanh): void trong Diagram.
    /// </summary>
    /// <param name="pbh">Thực thể phiếu bảo hành</param>
    void LuuPhieu(PhieuBaoHanh pbh);

    /// <summary>
    /// Tìm phiếu bảo hành theo số phiếu định danh (đồng bộ).
    /// Khớp phương thức: + TimTheoSoPhieu(soPhieu: string): PhieuBaoHanh trong Diagram.
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành</param>
    /// <returns>Thực thể phiếu bảo hành hoặc null</returns>
    PhieuBaoHanh? TimTheoSoPhieu(string soPhieu);

    /// <summary>
    /// Lấy danh sách các phiếu bảo hành gắn liền với khách hàng (đồng bộ).
    /// Khớp phương thức: + LayTheoKhachHang(maKH: string): List&lt;PhieuBaoHanh&gt; trong Diagram.
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <returns>Danh sách phiếu bảo hành của khách</returns>
    List<PhieuBaoHanh> LayTheoKhachHang(string maKH);

    /// <summary>
    /// Lấy toàn bộ danh sách phiếu bảo hành phục vụ quản lý, điều phối kỹ thuật (đồng bộ).
    /// </summary>
    /// <returns>Danh sách tất cả phiếu bảo hành</returns>
    List<PhieuBaoHanh> LayDanhSachPhieu();

    /// <summary>
    /// Cập nhật thông tin phiếu bảo hành vào cơ sở dữ liệu (đồng bộ).
    /// </summary>
    /// <param name="pbh">Thực thể phiếu bảo hành cần cập nhật</param>
    void CapNhatPhieu(PhieuBaoHanh pbh);

    // Mở rộng Task bất đồng bộ chuẩn Clean Architecture .NET 8
    Task LuuPhieuAsync(PhieuBaoHanh pbh, CancellationToken cancellationToken = default);
    Task<PhieuBaoHanh?> TimTheoSoPhieuAsync(string soPhieu, CancellationToken cancellationToken = default);
    Task<List<PhieuBaoHanh>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default);
    Task<List<PhieuBaoHanh>> LayDanhSachPhieuAsync(CancellationToken cancellationToken = default);
    Task CapNhatPhieuAsync(PhieuBaoHanh pbh, CancellationToken cancellationToken = default);
}
