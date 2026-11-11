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

    /// <summary>
    /// Tìm phiếu nhập kho theo mã.
    /// </summary>
    PhieuNhapKho? TimTheoId(string maPhieu);

    /// <summary>
    /// Đánh dấu vô hiệu hóa phiếu nhập kho (dành riêng cho Admin).
    /// </summary>
    void VoHieuHoaPhieuNhap(string maPhieuNhap);

    /// <summary>
    /// Phê duyệt phiếu nhập kho (chuyển trạng thái sang DaDuyet).
    /// </summary>
    void DuyetPhieuNhap(string maPhieuNhap);

    /// <summary>
    /// Xóa phiếu nhập kho chưa duyệt khỏi hệ thống.
    /// </summary>
    void XoaPhieuNhap(string maPhieuNhap);

    // Mở rộng bất đồng bộ chuẩn .NET 8
    Task LuuPhieuNhapAsync(PhieuNhapKho phieu, CancellationToken cancellationToken = default);
    Task<PhieuNhapKho?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default);
    Task<List<PhieuNhapKho>> LayTatCaAsync(CancellationToken cancellationToken = default);
    Task DuyetPhieuNhapAsync(string maPhieuNhap, CancellationToken cancellationToken = default);
    Task XoaPhieuNhapAsync(string maPhieuNhap, CancellationToken cancellationToken = default);
}
