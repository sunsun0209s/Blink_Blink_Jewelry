using System;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Identity;

/// <summary>
/// Use Case xử lý quy trình nghiệp vụ Đăng nhập hệ thống cho cả Web MVC và WPF Admin.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - DangNhapUseCase
/// </summary>
public class DangNhapUseCase
{
    private readonly INguoiDungRepository _nguoiDungRepo;

    /// <summary>
    /// Khởi tạo Use Case với Repository qua Dependency Injection
    /// </summary>
    /// <param name="nguoiDungRepo">Kho dữ liệu người dùng</param>
    public DangNhapUseCase(INguoiDungRepository nguoiDungRepo)
    {
        _nguoiDungRepo = nguoiDungRepo ?? throw new ArgumentNullException(nameof(nguoiDungRepo));
    }

    /// <summary>
    /// Thực thi quy trình xác thực đăng nhập người dùng
    /// </summary>
    /// <param name="request">Thông tin đăng nhập gồm tên đăng nhập và mật khẩu</param>
    /// <returns>Mã người dùng hoặc mã định danh phiên nếu thành công; chuỗi rỗng nếu thất bại</returns>
    public string Execute(DangNhapDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
        {
            return string.Empty;
        }

        var user = _nguoiDungRepo.TimTheoTenDangNhap(request.TenDangNhap.Trim());
        if (user == null)
        {
            return string.Empty;
        }

        // Gọi phương thức nghiệp vụ của chính thực thể Domain NguoiDung để xác thực
        var isAuthenticated = user.DangNhap(request.TenDangNhap, request.MatKhau);
        if (!isAuthenticated)
        {
            return string.Empty;
        }

        // Trả về mã người dùng thành công (hoặc token chuỗi)
        return user.MaNguoiDung;
    }
}
