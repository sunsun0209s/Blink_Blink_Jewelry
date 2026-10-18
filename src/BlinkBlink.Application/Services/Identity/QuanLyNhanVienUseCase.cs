using System;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Identity;

/// <summary>
/// Use Case quản lý nghiệp vụ Nhân sự và Phân quyền nội bộ trên WPF Admin Desktop.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - QuanLyNhanVienUseCase
/// </summary>
public class QuanLyNhanVienUseCase
{
    private readonly INhanVienRepository _nhanVienRepo;
    private readonly IVaiTroRepository _vaiTroRepo;

    /// <summary>
    /// Khởi tạo Use Case với các Repository phụ thuộc
    /// </summary>
    /// <param name="nhanVienRepo">Kho dữ liệu nhân viên</param>
    /// <param name="vaiTroRepo">Kho dữ liệu vai trò phân quyền</param>
    public QuanLyNhanVienUseCase(INhanVienRepository nhanVienRepo, IVaiTroRepository vaiTroRepo)
    {
        _nhanVienRepo = nhanVienRepo ?? throw new ArgumentNullException(nameof(nhanVienRepo));
        _vaiTroRepo = vaiTroRepo ?? throw new ArgumentNullException(nameof(vaiTroRepo));
    }

    /// <summary>
    /// Tạo mới một hồ sơ và tài khoản nhân viên vào hệ thống
    /// </summary>
    /// <param name="request">Thông tin nhân viên mới</param>
    /// <returns>True nếu tạo thành công; ngược lại False</returns>
    public bool TaoNhanVien(TaoNhanVienDTO request)
    {
        if (request == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.HoTen) ||
            string.IsNullOrWhiteSpace(request.TenDangNhap) ||
            string.IsNullOrWhiteSpace(request.MatKhau) ||
            string.IsNullOrWhiteSpace(request.MaVaiTro))
        {
            return false;
        }

        // Kiểm tra xem vai trò được gán có hợp lệ trong hệ thống hay không
        var vaiTro = _vaiTroRepo.TimTheoMa(request.MaVaiTro.Trim());
        if (vaiTro == null)
        {
            return false;
        }

        var maDinhDanh = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var maNhanVien = $"NV_{maDinhDanh}";
        var maNguoiDung = $"ND_{maDinhDanh}";

        var nhanVienMoi = new NhanVien(
            maNguoiDung: maNguoiDung,
            hoTen: request.HoTen.Trim(),
            soDienThoai: request.SoDienThoai?.Trim() ?? string.Empty,
            tenDangNhap: request.TenDangNhap.Trim(),
            matKhau: request.MatKhau, // Trong production sẽ qua PasswordHasher
            maVaiTro: vaiTro.MaVaiTro,
            maNhanVien: maNhanVien,
            luongCoBan: request.LuongCoBan,
            trangThai: true
        );

        _nhanVienRepo.LuuNhanVien(nhanVienMoi);
        return true;
    }

    /// <summary>
    /// Khóa tài khoản nhân viên khi nhân viên nghỉ việc hoặc có cảnh báo bảo mật
    /// </summary>
    /// <param name="maNv">Mã nhân viên cần khóa</param>
    /// <returns>True nếu khóa thành công; ngược lại False</returns>
    public bool KhoaTaiKhoanNhanVien(string maNv)
    {
        if (string.IsNullOrWhiteSpace(maNv))
        {
            return false;
        }

        var nhanVien = _nhanVienRepo.TimTheoMa(maNv.Trim());
        if (nhanVien == null)
        {
            return false;
        }

        // Gọi phương thức nghiệp vụ của Domain Entity NguoiDung
        nhanVien.KhoaTaiKhoan();
        
        // Lưu lại thay đổi vào cơ sở dữ liệu
        _nhanVienRepo.LuuNhanVien(nhanVien);
        return true;
    }
}
