using System;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Identity;

/// <summary>
/// Use Case xử lý nghiệp vụ Đăng ký tài khoản Khách hàng mới trên Web MVC.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - DangKyKhachHangUseCase
/// </summary>
public class DangKyKhachHangUseCase
{
    private readonly IKhachHangRepository _khachHangRepo;
    private readonly IHoaDonRepository? _hoaDonRepo;
    private readonly IBaoHanhRepository? _baoHanhRepo;

    /// <summary>
    /// Khởi tạo Use Case với Repository khách hàng, hóa đơn và bảo hành
    /// </summary>
    /// <param name="khachHangRepo">Kho dữ liệu khách hàng</param>
    /// <param name="hoaDonRepo">Kho dữ liệu hóa đơn (tự động mapping đơn cũ theo SĐT)</param>
    /// <param name="baoHanhRepo">Kho dữ liệu bảo hành (tự động mapping phiếu bảo hành cũ theo SĐT)</param>
    public DangKyKhachHangUseCase(
        IKhachHangRepository khachHangRepo,
        IHoaDonRepository? hoaDonRepo = null,
        IBaoHanhRepository? baoHanhRepo = null)
    {
        _khachHangRepo = khachHangRepo ?? throw new ArgumentNullException(nameof(khachHangRepo));
        _hoaDonRepo = hoaDonRepo;
        _baoHanhRepo = baoHanhRepo;
    }

    /// <summary>
    /// Thực thi quy trình tạo mới tài khoản và hồ sơ khách hàng
    /// </summary>
    /// <param name="request">Thông tin đăng ký khách hàng</param>
    /// <returns>True nếu đăng ký thành công; ngược lại False</returns>
    public bool Execute(DangKyKhachHangDTO request)
    {
        if (request == null)
        {
            return false;
        }

        // Ràng buộc dữ liệu đầu vào cơ bản
        if (string.IsNullOrWhiteSpace(request.HoTen) ||
            string.IsNullOrWhiteSpace(request.TenDangNhap) ||
            string.IsNullOrWhiteSpace(request.MatKhau) ||
            string.IsNullOrWhiteSpace(request.SoDienThoai))
        {
            return false;
        }

        // Tạo định danh tự động cho khách hàng mới
        var maDinhDanh = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var maKhachHang = $"KH_{maDinhDanh}";
        var maNguoiDung = $"ND_{maDinhDanh}";

        // Mặc định khách hàng mới mang vai trò KHACHHANG và tài khoản được kích hoạt
        var khachHangMoi = new KhachHang(
            maNguoiDung: maNguoiDung,
            hoTen: request.HoTen.Trim(),
            soDienThoai: request.SoDienThoai.Trim(),
            tenDangNhap: request.TenDangNhap.Trim(),
            matKhau: request.MatKhau, // Trong production sẽ qua PasswordHasher
            maVaiTro: "KHACHHANG",
            maKhachHang: maKhachHang,
            ngaySinh: request.NgaySinh,
            trangThai: true
        );

        _khachHangRepo.LuuKhachHang(khachHangMoi);

        // Tự động đối chiếu SĐT với CSDL hóa đơn mua hàng và phiếu bảo hành trước đó, gắn mã khách hàng vào các chứng từ cũ nếu có
        try
        {
            if (!string.IsNullOrWhiteSpace(khachHangMoi.SoDienThoai))
            {
                _hoaDonRepo?.CapNhatKhachHangTheoSoDienThoai(khachHangMoi.SoDienThoai, khachHangMoi.MaKhachHang);
                _baoHanhRepo?.CapNhatKhachHangTheoSoDienThoai(khachHangMoi.SoDienThoai, khachHangMoi.MaKhachHang);
            }
        }
        catch
        {
            // Bỏ qua lỗi cập nhật phụ nếu có để đảm bảo luồng đăng ký tài khoản không bị gián đoạn
        }

        return true;
    }
}
