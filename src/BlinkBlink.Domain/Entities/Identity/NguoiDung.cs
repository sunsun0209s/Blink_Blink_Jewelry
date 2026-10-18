using System;

namespace BlinkBlink.Domain.Entities.Identity;

/// <summary>
/// Thực thể trừu tượng đại diện cho một người dùng trong hệ thống.
/// Là lớp cơ sở (Base Class) cho Nhân viên (NhanVien) và Khách hàng (KhachHang).
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NguoiDung
/// </summary>
public abstract class NguoiDung
{
    /// <summary>
    /// Mã người dùng (Khoá chính định danh tài khoản)
    /// </summary>
    public string MaNguoiDung { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên đầy đủ của người dùng
    /// </summary>
    public string HoTen { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ
    /// </summary>
    public string SoDienThoai { get; set; } = string.Empty;

    /// <summary>
    /// Tên tài khoản dùng để đăng nhập hệ thống
    /// </summary>
    public string TenDangNhap { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đăng nhập (Lưu chuỗi băm/hash để bảo mật)
    /// </summary>
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Mã vai trò phân quyền (Khoá ngoại liên kết bảng VaiTro)
    /// </summary>
    public string MaVaiTro { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái hoạt động của tài khoản (true: Đang hoạt động, false: Bị khoá)
    /// </summary>
    public bool TrangThai { get; set; } = true;

    /// <summary>
    /// Thông tin vai trò của người dùng (Navigation Property)
    /// </summary>
    public virtual VaiTro? VaiTro { get; set; }

    protected NguoiDung() { }

    /// <summary>
    /// Khởi tạo người dùng với các thông tin định danh ban đầu
    /// </summary>
    /// <param name="maNguoiDung">Mã định danh tài khoản</param>
    /// <param name="hoTen">Họ tên người dùng</param>
    /// <param name="soDienThoai">Số điện thoại liên lạc</param>
    /// <param name="tenDangNhap">Tên đăng nhập hệ thống</param>
    /// <param name="matKhau">Mật khẩu (đã băm)</param>
    /// <param name="maVaiTro">Mã vai trò của người dùng</param>
    /// <param name="trangThai">Trạng thái kích hoạt ban đầu</param>
    protected NguoiDung(
        string maNguoiDung,
        string hoTen,
        string soDienThoai,
        string tenDangNhap,
        string matKhau,
        string maVaiTro,
        bool trangThai = true)
    {
        MaNguoiDung = maNguoiDung;
        HoTen = hoTen;
        SoDienThoai = soDienThoai;
        TenDangNhap = tenDangNhap;
        MatKhau = matKhau;
        MaVaiTro = maVaiTro;
        TrangThai = trangThai;
    }

    /// <summary>
    /// Kiểm tra xác thực thông tin đăng nhập của người dùng
    /// </summary>
    /// <param name="tenDN">Tên đăng nhập cần kiểm tra</param>
    /// <param name="mk">Mật khẩu (chuỗi đã hash để so khớp)</param>
    /// <returns>True nếu tài khoản đang hoạt động và thông tin khớp; ngược lại False</returns>
    public virtual bool DangNhap(string tenDN, string mk)
    {
        if (string.IsNullOrWhiteSpace(tenDN) || string.IsNullOrWhiteSpace(mk))
        {
            return false;
        }

        // Tài khoản phải đang hoạt động mới cho phép đăng nhập
        if (!TrangThai)
        {
            return false;
        }

        return TenDangNhap == tenDN.Trim() && MatKhau == mk;
    }

    /// <summary>
    /// Đổi mật khẩu tài khoản người dùng
    /// </summary>
    /// <param name="mkMoi">Mật khẩu mới (đã hash)</param>
    /// <returns>True nếu đổi thành công; ngược lại False</returns>
    public virtual bool DoiMatKhau(string mkMoi)
    {
        if (string.IsNullOrWhiteSpace(mkMoi))
        {
            return false;
        }

        MatKhau = mkMoi;
        return true;
    }

    /// <summary>
    /// Cập nhật thông tin họ tên và số điện thoại liên hệ
    /// </summary>
    /// <param name="hoTen">Họ tên mới</param>
    /// <param name="sdt">Số điện thoại mới</param>
    /// <returns>True nếu cập nhật thành công</returns>
    public virtual bool CapNhatThongTin(string hoTen, string sdt)
    {
        if (string.IsNullOrWhiteSpace(hoTen))
        {
            return false;
        }

        HoTen = hoTen.Trim();
        SoDienThoai = sdt?.Trim() ?? string.Empty;
        return true;
    }

    /// <summary>
    /// Khóa quyền truy cập của tài khoản trên toàn hệ thống
    /// </summary>
    /// <returns>True nếu trạng thái được chuyển thành bị khóa</returns>
    public virtual bool KhoaTaiKhoan()
    {
        TrangThai = false;
        return true;
    }
}
