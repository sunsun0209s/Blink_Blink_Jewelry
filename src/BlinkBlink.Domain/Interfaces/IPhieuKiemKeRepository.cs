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
    void LuuPhieuKiemKe(PhieuKiemKe phieu);

    /// <summary>
    /// Tìm phiếu kiểm kê theo mã.
    /// </summary>
    /// <param name="maPhieu">Mã phiếu kiểm kê</param>
    /// <returns>Thực thể phiếu kiểm kê</returns>
    PhieuKiemKe? TimTheoId(string maPhieu);

    // Mở rộng bất đồng bộ chuẩn .NET 8
    Task LuuPhieuKiemKeAsync(PhieuKiemKe phieu, CancellationToken cancellationToken = default);
    Task<PhieuKiemKe?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default);
}
