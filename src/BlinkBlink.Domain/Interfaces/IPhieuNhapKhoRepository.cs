using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Inventory;

namespace BlinkBlink.Domain.Interfaces;

/// <summary>
/// Giao diện Repository quản lý thực thể PhieuNhapKho.
/// Khớp 100% với Class Diagram.
/// </summary>
public interface IPhieuNhapKhoRepository
{
    /// <summary>
    /// Lưu phiếu nhập kho cùng toàn bộ chi tiết xuống CSDL.
    /// </summary>
    /// <param name="phieu">Thực thể phiếu nhập kho</param>
    void LuuPhieuNhap(PhieuNhapKho phieu);

    /// <summary>
    /// Lấy danh sách toàn bộ phiếu nhập kho trong hệ thống.
    /// </summary>
    List<PhieuNhapKho> LayTatCa();

    // Mở rộng bất đồng bộ chuẩn .NET 8
    Task LuuPhieuNhapAsync(PhieuNhapKho phieu, CancellationToken cancellationToken = default);
    Task<PhieuNhapKho?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default);
    Task<List<PhieuNhapKho>> LayTatCaAsync(CancellationToken cancellationToken = default);
}
