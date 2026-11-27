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
    /// <returns>True nếu lưu thành công</returns>
    bool LuuPhieuNhap(PhieuNhapKho phieu);

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
    /// <returns>True nếu có bản ghi được cập nhật</returns>
    bool VoHieuHoaPhieuNhap(string maPhieuNhap);

    /// <summary>
    /// Phê duyệt phiếu nhập kho (chuyển trạng thái sang DaDuyet).
    /// </summary>
    /// <returns>True nếu có bản ghi được phê duyệt</returns>
    bool DuyetPhieuNhap(string maPhieuNhap);

    /// <summary>
    /// Xóa phiếu nhập kho chưa duyệt khỏi hệ thống.
    /// </summary>
    /// <returns>True nếu có bản ghi được xóa</returns>
    bool XoaPhieuNhap(string maPhieuNhap);

    // Mở rộng bất đồng bộ chuẩn .NET 8
    Task<bool> LuuPhieuNhapAsync(PhieuNhapKho phieu, CancellationToken cancellationToken = default);
    Task<PhieuNhapKho?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default);
    Task<List<PhieuNhapKho>> LayTatCaAsync(CancellationToken cancellationToken = default);
    Task<bool> DuyetPhieuNhapAsync(string maPhieuNhap, CancellationToken cancellationToken = default);
    Task<bool> XoaPhieuNhapAsync(string maPhieuNhap, CancellationToken cancellationToken = default);
}
