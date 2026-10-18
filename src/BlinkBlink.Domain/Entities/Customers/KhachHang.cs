using System;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Domain.Entities.Customers;

/// <summary>
/// Thực thể Khách hàng đại diện cho người mua hàng của Blink Blink Jewelry.
/// Kế thừa từ thực thể NguoiDung.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - KhachHang
/// </summary>
public class KhachHang : NguoiDung
{
    /// <summary>
    /// Mã khách hàng (Định danh duy nhất của khách hàng, ví dụ: KH001)
    /// </summary>
    public string MaKhachHang { get; set; } = string.Empty;

    /// <summary>
    /// Ngày tháng năm sinh của khách hàng (Phục vụ ưu đãi sinh nhật và phân loại khách hàng)
    /// </summary>
    public DateTime NgaySinh { get; set; }

    public KhachHang() : base() { }

    /// <summary>
    /// Khởi tạo đối tượng KhachHang với đầy đủ thông tin tài khoản và thông tin cá nhân
    /// </summary>
    /// <param name="maNguoiDung">Mã định danh người dùng chung</param>
    /// <param name="hoTen">Họ và tên khách hàng</param>
    /// <param name="soDienThoai">Số điện thoại liên hệ</param>
    /// <param name="tenDangNhap">Tên tài khoản khách hàng</param>
    /// <param name="matKhau">Mật khẩu (đã hash)</param>
    /// <param name="maVaiTro">Mã vai trò (thường là VaiTro khách hàng)</param>
    /// <param name="maKhachHang">Mã định danh khách hàng</param>
    /// <param name="ngaySinh">Ngày tháng năm sinh</param>
    /// <param name="trangThai">Trạng thái tài khoản ban đầu</param>
    public KhachHang(
        string maNguoiDung,
        string hoTen,
        string soDienThoai,
        string tenDangNhap,
        string matKhau,
        string maVaiTro,
        string maKhachHang,
        DateTime ngaySinh,
        bool trangThai = true)
        : base(maNguoiDung, hoTen, soDienThoai, tenDangNhap, matKhau, maVaiTro, trangThai)
    {
        MaKhachHang = maKhachHang;
        NgaySinh = ngaySinh;
    }

    /// <summary>
    /// Chuỗi định dạng hiển thị thông tin tóm tắt của khách hàng
    /// </summary>
    /// <returns>Chuỗi biểu diễn mã, họ tên và số điện thoại khách hàng</returns>
    public override string ToString()
    {
        return $"{MaKhachHang} - {HoTen} ({SoDienThoai})";
    }
}
