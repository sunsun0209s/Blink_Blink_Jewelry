using System;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Identity;

/// <summary>
/// Use Case quản lý thông tin và tài khoản khách hàng trên hệ thống.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - QuanLyKhachHangUseCase
/// </summary>
public class QuanLyKhachHangUseCase
{
    private readonly IKhachHangRepository _khachHangRepo;

    /// <summary>
    /// Khởi tạo Use Case với repository quản lý khách hàng
    /// </summary>
    /// <param name="khachHangRepo">Kho lưu trữ dữ liệu khách hàng</param>
    public QuanLyKhachHangUseCase(IKhachHangRepository khachHangRepo)
    {
        _khachHangRepo = khachHangRepo ?? throw new ArgumentNullException(nameof(khachHangRepo));
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ cá nhân của khách hàng
    /// </summary>
    /// <param name="request">Dữ liệu thông tin khách hàng cần cập nhật</param>
    /// <returns>True nếu cập nhật thành công; ngược lại False</returns>
    public bool CapNhatThongTin(CapNhatKhachHangDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaKhachHang) || string.IsNullOrWhiteSpace(request.HoTen))
        {
            return false;
        }

        var khachHang = _khachHangRepo.TimTheoMa(request.MaKhachHang.Trim());
        if (khachHang == null)
        {
            return false;
        }

        khachHang.HoTen = request.HoTen.Trim();
        khachHang.SoDienThoai = request.SoDienThoai?.Trim() ?? string.Empty;
        khachHang.NgaySinh = request.NgaySinh;

        _khachHangRepo.LuuKhachHang(khachHang);
        return true;
    }

    /// <summary>
    /// Khóa tài khoản khách hàng trong trường hợp vi phạm chính sách hoặc theo yêu cầu
    /// </summary>
    /// <param name="maKh">Mã khách hàng cần khóa tài khoản</param>
    /// <returns>True nếu khóa tài khoản thành công; ngược lại False</returns>
    public bool KhoaTaiKhoanKhachHang(string maKh)
    {
        if (string.IsNullOrWhiteSpace(maKh))
        {
            return false;
        }

        var khachHang = _khachHangRepo.TimTheoMa(maKh.Trim());
        if (khachHang == null)
        {
            return false;
        }

        if (khachHang.TrangThai)
        {
            khachHang.KhoaTaiKhoan();
        }
        else
        {
            khachHang.TrangThai = true;
        }

        _khachHangRepo.LuuKhachHang(khachHang);
        return true;
    }

    /// <summary>
    /// Xóa hồ sơ khách hàng khỏi cơ sở dữ liệu nếu khách hàng chưa từng phát sinh bất kỳ đơn hàng nào.
    /// Nếu đã có lịch sử mua hàng thì chặn xóa, chỉ cho phép khóa tài khoản.
    /// </summary>
    /// <param name="maKh">Mã khách hàng cần xóa</param>
    /// <param name="message">Thông báo phản hồi kết quả</param>
    /// <returns>True nếu xóa thành công, False nếu vi phạm ràng buộc</returns>
    public bool XoaKhachHang(string maKh, out string message)
    {
        if (string.IsNullOrWhiteSpace(maKh))
        {
            message = "Mã khách hàng không hợp lệ.";
            return false;
        }

        var khachHang = _khachHangRepo.TimTheoMa(maKh.Trim());
        if (khachHang == null)
        {
            message = "Không tìm thấy hồ sơ khách hàng trong hệ thống.";
            return false;
        }

        if (!_khachHangRepo.KiemTraCoTheXoa(maKh.Trim()))
        {
            message = $"Khách hàng [{maKh}] đã có lịch sử giao dịch mua hàng hoặc bảo hành trang sức. Không thể xóa khỏi CSDL để bảo lưu hóa đơn tài chính, chỉ cho phép khóa tài khoản!";
            return false;
        }

        var ok = _khachHangRepo.XoaKhachHang(maKh.Trim());
        if (ok)
        {
            message = $"Đã xóa thành công hồ sơ khách hàng [{maKh}] khỏi cơ sở dữ liệu.";
            return true;
        }

        message = "Xóa khách hàng thất bại do lỗi hệ thống dữ liệu.";
        return false;
    }
}
