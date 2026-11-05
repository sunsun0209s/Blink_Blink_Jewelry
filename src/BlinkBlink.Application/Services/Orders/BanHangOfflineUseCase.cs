using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Orders;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Orders;

/// <summary>
/// Trường hợp sử dụng Bán hàng trực tiếp tại quầy POS offline (BanHangOfflineUseCase).
/// Phục vụ nhân viên bán hàng và Admin tạo đơn bán trực tiếp, áp dụng giảm giá,
/// ghi nhận thanh toán tức thì (tiền mặt / chuyển khoản) và cập nhật số lượng tồn kho.
/// </summary>
public class BanHangOfflineUseCase
{
    private readonly IHoaDonRepository _hoaDonRepo;
    private readonly ISanPhamRepository _sanPhamRepo;
    private readonly IKhachHangRepository _khachHangRepo;

    public BanHangOfflineUseCase(
        IHoaDonRepository hoaDonRepo,
        ISanPhamRepository sanPhamRepo,
        IKhachHangRepository khachHangRepo)
    {
        _hoaDonRepo = hoaDonRepo ?? throw new ArgumentNullException(nameof(hoaDonRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
        _khachHangRepo = khachHangRepo ?? throw new ArgumentNullException(nameof(khachHangRepo));
    }

    /// <summary>
    /// Thực hiện lập hóa đơn bán lẻ tại quầy và xác nhận thanh toán (đồng bộ).
    /// </summary>
    /// <param name="request">Thông tin khách hàng và danh sách sản phẩm mua</param>
    /// <param name="maHoaDonMoi">Mã hóa đơn vừa sinh (HD_OFFLINE_...)</param>
    /// <returns>True nếu tạo và thanh toán hóa đơn thành công</returns>
    public bool Execute(TaoHoaDonOfflineDTO request, out string maHoaDonMoi)
    {
        maHoaDonMoi = string.Empty;
        if (request == null || request.DanhSachSanPham == null || !request.DanhSachSanPham.Any())
        {
            return false;
        }

        // 1. Xác định mã khách hàng gắn vào hóa đơn:
        string maKhachHang = "0";
        if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
        {
            var khachHang = _khachHangRepo.TimTheoSoDienThoai(request.SoDienThoai.Trim());
            if (khachHang != null)
            {
                maKhachHang = khachHang.MaKhachHang;
            }
        }

        // 2. Sinh mã hóa đơn chuẩn theo tiền tố HD_OFFLINE_
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var rand = new Random().Next(100, 999);
        maHoaDonMoi = $"HD_OFFLINE_{timestamp}{rand}";

        // 3. Khởi tạo đối tượng Hóa đơn với trạng thái đã thanh toán tại quầy
        var hoaDon = new HoaDon(
            maHoaDon: maHoaDonMoi,
            ngayLap: DateTime.Now,
            maKhachHang: maKhachHang,
            diaChiGiaoHang: string.IsNullOrWhiteSpace(request.DiaChi) ? "Mua trực tiếp tại quầy" : request.DiaChi.Trim(),
            soDienThoaiNhan: string.IsNullOrWhiteSpace(request.SoDienThoai) ? "0000000000" : request.SoDienThoai.Trim(),
            phuongThucThanhToan: string.IsNullOrWhiteSpace(request.PhuongThucThanhToan) ? "TienMat" : request.PhuongThucThanhToan.Trim(),
            trangThaiDonHang: "DaThanhToan",
            ghiChu: request.GhiChu ?? string.Empty
        );

        // 4. Thêm từng mặt hàng vào hóa đơn, cập nhật trạng thái sản phẩm
        foreach (var item in request.DanhSachSanPham)
        {
            var sp = _sanPhamRepo.TimTheoId(item.MaSanPham);
            if (sp == null)
            {
                continue;
            }

            var phanTramGiam = Math.Clamp(item.PhanTramGiamGia, 0, 100);
            var ct = new ChiTietHoaDon(
                soHoaDon: maHoaDonMoi,
                maSanPham: item.MaSanPham,
                donGia: item.DonGia > 0 ? item.DonGia : sp.GiaBan,
                soLuong: item.SoLuong > 0 ? item.SoLuong : 1,
                phanTramGiamGia: phanTramGiam
            );
            hoaDon.ChiTietHoaDons.Add(ct);

            // Cập nhật trạng thái sản phẩm trang sức kim hoàn sang đã bán
            sp.CapNhatTrangThai("DaBan");
            _sanPhamRepo.LuuSanPham(sp);
        }

        // Tính lại tổng tiền sau khi đã chiết khấu toàn bộ mặt hàng
        hoaDon.TinhTongTien();

        // 5. Lưu vào kho dữ liệu
        _hoaDonRepo.LuuHoaDon(hoaDon);
        return true;
    }

    /// <summary>
    /// Thực hiện lập hóa đơn bán lẻ tại quầy và xác nhận thanh toán (bất đồng bộ).
    /// </summary>
    public Task<bool> ExecuteAsync(TaoHoaDonOfflineDTO request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute(request, out _));
    }
}
