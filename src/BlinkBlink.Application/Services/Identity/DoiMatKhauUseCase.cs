using System;
using BlinkBlink.Application.Common;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Identity;

/// <summary>
/// Use Case xử lý nghiệp vụ Đổi mật khẩu tài khoản người dùng trên hệ thống.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NguoiDung.DoiMatKhau
/// </summary>
public class DoiMatKhauUseCase
{
    private readonly INguoiDungRepository _nguoiDungRepo;

    /// <summary>
    /// Khởi tạo Use Case với Repository người dùng
    /// </summary>
    /// <param name="nguoiDungRepo">Kho dữ liệu người dùng</param>
    public DoiMatKhauUseCase(INguoiDungRepository nguoiDungRepo)
    {
        _nguoiDungRepo = nguoiDungRepo ?? throw new ArgumentNullException(nameof(nguoiDungRepo));
    }

    /// <summary>
    /// Thực thi quy trình xác thực mật khẩu hiện tại và cập nhật mật khẩu mới
    /// </summary>
    /// <param name="request">Thông tin tên đăng nhập, mật khẩu hiện tại và mật khẩu mới</param>
    /// <returns>AppResult cho biết kết quả thành công hoặc mã lỗi nghiệp vụ cụ thể</returns>
    public AppResult<bool> Execute(DoiMatKhauDTO request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.TenDangNhap) ||
            string.IsNullOrWhiteSpace(request.MatKhauHienTai) ||
            string.IsNullOrWhiteSpace(request.MatKhauMoi))
        {
            return AppResult<bool>.Failure(ErrorCodes.InvalidInput, "Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới.");
        }

        if (request.MatKhauMoi.Trim().Length < 6)
        {
            return AppResult<bool>.Failure(ErrorCodes.InvalidInput, "Mật khẩu mới phải có độ dài từ 6 ký tự trở lên.");
        }

        var user = _nguoiDungRepo.TimTheoTenDangNhap(request.TenDangNhap.Trim());
        if (user == null)
        {
            return AppResult<bool>.Failure(ErrorCodes.ResourceNotFound, "Không tìm thấy tài khoản người dùng.");
        }

        if (!user.TrangThai)
        {
            return AppResult<bool>.Failure(ErrorCodes.AccountLocked, "Tài khoản của quý khách hiện đang bị tạm khóa. Vui lòng liên hệ bộ phận hỗ trợ.");
        }

        if (!user.DangNhap(request.TenDangNhap, request.MatKhauHienTai))
        {
            return AppResult<bool>.Failure(ErrorCodes.InvalidCredentials, "Mật khẩu hiện tại không chính xác.");
        }

        if (request.MatKhauMoi == request.MatKhauHienTai)
        {
            return AppResult<bool>.Failure(ErrorCodes.InvalidInput, "Mật khẩu mới phải khác mật khẩu hiện tại.");
        }

        if (!user.DoiMatKhau(request.MatKhauMoi))
        {
            return AppResult<bool>.Failure(ErrorCodes.InvalidInput, "Mật khẩu mới không hợp lệ.");
        }

        _nguoiDungRepo.CapNhat(user);
        return AppResult<bool>.Success(true, "Đổi mật khẩu thành công.");
    }
}
