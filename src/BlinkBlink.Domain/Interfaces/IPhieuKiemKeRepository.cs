using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Inventory;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể PhieuKiemKe.
/// Khớp với Class Diagram mở rộng.
/// </summary>
public interface IPhieuKiemKeRepository
{
    /// <summary>
    /// Lưu phiếu kiểm kê và biên bản chênh lệch vào CSDL.
    /// </summary>
    /// <param name="phieu">Thực thể phiếu kiểm kê</param>
    /// <returns>True nếu lưu thành công</returns>
    bool LuuPhieuKiemKe(PhieuKiemKe phieu);

    /// <summary>
    /// Tìm phiếu kiểm kê theo mã.
    /// </summary>
    /// <param name="maPhieu">Mã phiếu kiểm kê</param>
    /// <returns>Thực thể phiếu kiểm kê</returns>
    PhieuKiemKe? TimTheoId(string maPhieu);

    /// <summary>
    /// Lấy danh sách toàn bộ phiếu kiểm kê kho trong hệ thống (sắp xếp theo thứ tự mới nhất).
    /// </summary>
    List<PhieuKiemKe> LayTatCa();

    /// <summary>
    /// Xóa phiếu kiểm kê (chỉ cho phép khi phiếu ở trạng thái ChoDuyet).
    /// </summary>
    /// <returns>True nếu có bản ghi được xóa</returns>
    bool XoaPhieuKiemKe(string maPhieu);

    /// <summary>
    /// Cập nhật trạng thái phiếu kiểm kê (ChoDuyet, DaDuyet).
    /// </summary>
    /// <returns>True nếu có bản ghi được cập nhật</returns>
    bool CapNhatTrangThai(string maPhieu, string trangThaiMoi, DateTime? ngayDuyet = null);

    // Mở rộng bất đồng bộ chuẩn .NET 8
    Task<bool> LuuPhieuKiemKeAsync(PhieuKiemKe phieu, CancellationToken cancellationToken = default);
    Task<PhieuKiemKe?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default);
}
