using System;

namespace BlinkBlink.Domain.Entities.Identity;

/// <summary>
/// Thực thể Nhân viên quản lý thông tin các nhân sự làm việc tại cửa hàng trang sức (POS/Admin).
/// Kế thừa từ thực thể NguoiDung.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NhanVien
/// </summary>
public class NhanVien : NguoiDung
{
    /// <summary>
    /// Mã nhân viên (Định danh nhân viên trong tổ chức, ví dụ: NV001)
    /// </summary>
    public string MaNhanVien { get; set; } = string.Empty;

    /// <summary>
    /// Mức lương cơ bản của nhân viên
    /// </summary>
    public decimal LuongCoBan { get; set; }

    public NhanVien() : base() { }

    /// <summary>
    /// Khởi tạo nhân viên mới với đầy đủ thông tin tài khoản và nghiệp vụ nhân sự
    /// </summary>
    /// <param name="maNguoiDung">Mã định danh người dùng chung</param>
    /// <param name="hoTen">Họ và tên nhân viên</param>
    /// <param name="soDienThoai">Số điện thoại liên hệ</param>
    /// <param name="tenDangNhap">Tên tài khoản đăng nhập</param>
    /// <param name="matKhau">Mật khẩu tài khoản (đã hash)</param>
    /// <param name="maVaiTro">Mã vai trò chức vụ</param>
    /// <param name="maNhanVien">Mã nhân viên nội bộ</param>
    /// <param name="luongCoBan">Mức lương cơ bản</param>
    /// <param name="trangThai">Trạng thái tài khoản ban đầu</param>
    public NhanVien(
        string maNguoiDung,
        string hoTen,
        string soDienThoai,
        string tenDangNhap,
        string matKhau,
        string maVaiTro,
        string maNhanVien,
        decimal luongCoBan = 0,
        bool trangThai = true)
        : base(maNguoiDung, hoTen, soDienThoai, tenDangNhap, matKhau, maVaiTro, trangThai)
    {
        MaNhanVien = maNhanVien;
        LuongCoBan = luongCoBan;
    }
}
