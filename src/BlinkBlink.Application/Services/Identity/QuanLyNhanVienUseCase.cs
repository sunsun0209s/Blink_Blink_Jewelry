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
        if (nhanVien.TrangThai)
        {
            nhanVien.KhoaTaiKhoan();
        }
        else
        {
            nhanVien.TrangThai = true;
        }
        
        // Lưu lại thay đổi vào cơ sở dữ liệu
        _nhanVienRepo.LuuNhanVien(nhanVien);
        return true;
    }

    /// <summary>
    /// Chỉnh sửa thông tin hồ sơ nhân viên và chọn lại vai trò phân quyền
    /// </summary>
    /// <param name="maNv">Mã nhân viên cần cập nhật</param>
    /// <param name="hoTen">Họ và tên mới</param>
    /// <param name="soDienThoai">Số điện thoại liên lạc</param>
    /// <param name="luongCoBan">Mức lương cơ bản</param>
    /// <param name="maVaiTro">Mã vai trò mới (ADMIN, SALE, WAREHOUSE)</param>
    /// <returns>True nếu cập nhật thành công</returns>
    public bool CapNhatThongTinNhanVien(string maNv, string hoTen, string soDienThoai, decimal luongCoBan, string maVaiTro)
    {
        if (string.IsNullOrWhiteSpace(maNv) || string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(maVaiTro))
        {
            return false;
        }

        var nhanVien = _nhanVienRepo.TimTheoMa(maNv.Trim());
        if (nhanVien == null)
        {
            return false;
        }

        nhanVien.HoTen = hoTen.Trim();
        nhanVien.SoDienThoai = soDienThoai?.Trim() ?? string.Empty;
        nhanVien.LuongCoBan = luongCoBan;
        nhanVien.MaVaiTro = maVaiTro.Trim();

        _nhanVienRepo.CapNhatNhanVien(nhanVien);
        return true;
    }

    /// <summary>
    /// Đặt lại mật khẩu mặc định 123456789 cho nhân viên theo quy định hệ thống mới
    /// </summary>
    /// <param name="maNv">Mã định danh nhân viên cần đặt lại mật khẩu</param>
    /// <returns>True nếu đặt lại mật khẩu thành công; ngược lại False</returns>
    public bool ResetMatKhau(string maNv)
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

        // Mật khẩu mặc định khởi tạo ban đầu theo quy định hệ thống chuẩn: 123456789
        const string matKhauMacDinh = "123456789";
        nhanVien.DoiMatKhau(matKhauMacDinh);

        _nhanVienRepo.LuuNhanVien(nhanVien);
        return true;
    }

    /// <summary>
    /// Xóa nhân viên khỏi hệ thống nếu nhân viên chưa từng phát sinh dữ liệu nghiệp vụ (phiếu nhập, hóa đơn, kiểm kê).
    /// Nếu đã từng phát sinh dữ liệu thì chặn xóa và yêu cầu khóa tài khoản.
    /// </summary>
    /// <param name="maNv">Mã nhân viên cần xóa</param>
    /// <param name="message">Thông báo kết quả phản hồi</param>
    /// <returns>True nếu xóa thành công, False nếu vi phạm ràng buộc dữ liệu</returns>
    public bool XoaNhanVien(string maNv, out string message)
    {
        if (string.IsNullOrWhiteSpace(maNv))
        {
            message = "Mã nhân viên không hợp lệ.";
            return false;
        }

        var nhanVien = _nhanVienRepo.TimTheoMa(maNv.Trim());
        if (nhanVien == null)
        {
            message = "Không tìm thấy thông tin nhân viên trong hệ thống.";
            return false;
        }

        if (!_nhanVienRepo.KiemTraCoTheXoa(maNv.Trim()))
        {
            message = $"Nhân viên [{maNv}] đã từng thực hiện tạo phiếu nhập kho, lập hóa đơn bán hàng hoặc xử lý đơn hàng. Hệ thống không cho phép xóa khỏi CSDL để bảo toàn chứng từ, chỉ cho phép khóa tài khoản!";
            return false;
        }

        var ok = _nhanVienRepo.XoaNhanVien(maNv.Trim());
        if (ok)
        {
            message = $"Đã xóa thành công nhân viên [{maNv}] và tài khoản liên quan khỏi cơ sở dữ liệu.";
            return true;
        }

        message = "Xóa nhân viên thất bại do lỗi hệ thống dữ liệu.";
        return false;
    }
}
