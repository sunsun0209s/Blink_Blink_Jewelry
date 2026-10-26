using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Orders;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Orders;

/// <summary>
/// Trường hợp sử dụng Đặt hàng trực tuyến (DatHangUseCase) dành cho khách hàng.
/// Chuyển đổi giỏ hàng hiện tại thành hóa đơn bán hàng và làm sạch giỏ hàng.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class DatHangUseCase
{
    private readonly IHoaDonRepository _hoaDonRepo;
    private readonly IGioHangRepository _gioHangRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public DatHangUseCase(
        IHoaDonRepository hoaDonRepo,
        IGioHangRepository gioHangRepo,
        ISanPhamRepository sanPhamRepo)
    {
        _hoaDonRepo = hoaDonRepo ?? throw new ArgumentNullException(nameof(hoaDonRepo));
        _gioHangRepo = gioHangRepo ?? throw new ArgumentNullException(nameof(gioHangRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
    }

    /// <summary>
    /// Thực hiện tiếp nhận và tạo đơn đặt hàng từ giỏ hàng hiện tại (đồng bộ).
    /// Khớp phương thức: + Execute(request: DatHangDTO): bool trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin giao nhận và thanh toán của khách hàng</param>
    /// <returns>True nếu đặt hàng thành công, False nếu giỏ hàng rỗng hoặc thông tin không hợp lệ</returns>
    public bool Execute(DatHangDTO request)
    {
        return ExecuteAsync(request).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Thực hiện tiếp nhận và tạo đơn đặt hàng từ giỏ hàng hiện tại (bất đồng bộ).
    /// </summary>
    /// <param name="request">Thông tin giao nhận và thanh toán của khách hàng</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>True nếu tạo hóa đơn thành công và làm sạch giỏ hàng, False nếu dữ liệu không hợp lệ</returns>
    public async Task<bool> ExecuteAsync(DatHangDTO request, CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaKhachHang))
        {
            return false;
        }

        // Kiểm tra thông tin liên lạc bắt buộc để đảm bảo giao nhận trang sức giá trị cao chính xác
        if (string.IsNullOrWhiteSpace(request.DiaChiGiaoHang) || string.IsNullOrWhiteSpace(request.SoDienThoaiNhan))
        {
            return false;
        }

        var gioHang = await _gioHangRepo.LayGioHangAsync(request.MaKhachHang, cancellationToken);
        if (gioHang == null || gioHang.DanhSachSanPham == null || gioHang.DanhSachSanPham.Count == 0)
        {
            return false;
        }

        // Sinh mã hóa đơn duy nhất kết hợp tiền tố HD, dấu thời gian và số ngẫu nhiên
        var maHoaDon = $"HD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        var hoaDon = new HoaDon(
            maHoaDon: maHoaDon,
            ngayLap: DateTime.UtcNow,
            maKhachHang: request.MaKhachHang,
            diaChiGiaoHang: request.DiaChiGiaoHang.Trim(),
            soDienThoaiNhan: request.SoDienThoaiNhan.Trim(),
            phuongThucThanhToan: string.IsNullOrWhiteSpace(request.PhuongThucThanhToan) ? "COD" : request.PhuongThucThanhToan.Trim(),
            trangThaiDonHang: "ChoDuyet",
            ghiChu: request.GhiChu ?? string.Empty
        );

        // Chuyển từng món trong giỏ sang chi tiết hóa đơn, bảo đảm lấy đơn giá niêm yết hiện thời
        foreach (var item in gioHang.DanhSachSanPham)
        {
            var sp = await _sanPhamRepo.TimTheoIdAsync(item.MaSanPham, cancellationToken);
            if (sp == null || !sp.KiemTraTonKho())
            {
                // Ngăn chặn tạo đơn nếu có sản phẩm đã hết hàng hoặc không khả dụng
                return false;
            }

            var chiTiet = new ChiTietHoaDon(
                soHoaDon: maHoaDon,
                maSanPham: item.MaSanPham,
                donGia: sp.GiaBan,
                soLuong: item.SoLuongSanPham
            );

            hoaDon.ChiTietHoaDons.Add(chiTiet);
        }

        // Tính tổng tiền đơn hàng trước khi lưu trữ
        hoaDon.TinhTongTien();

        // Lưu đơn hàng mới vào cơ sở dữ liệu
        await _hoaDonRepo.LuuHoaDonAsync(hoaDon, cancellationToken);

        // Làm sạch giỏ hàng của khách sau khi đã chuyển thành công thành hóa đơn
        gioHang.DonDepGioHang();
        await _gioHangRepo.CapNhatGioHangAsync(gioHang, cancellationToken);

        return true;
    }
}
