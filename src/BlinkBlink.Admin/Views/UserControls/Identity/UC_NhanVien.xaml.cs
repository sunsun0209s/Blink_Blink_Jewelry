using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Admin.Views.UserControls.Identity;

/// <summary>
/// Interaction logic cho UC_NhanVien.xaml
/// Màn hình quản lý nhân sự cửa hàng và phân quyền tài khoản (WPF Admin)
/// </summary>
public partial class UC_NhanVien : UserControl
{
    private readonly QuanLyNhanVienUseCase? _quanLyNhanVienUseCase;
    private readonly INhanVienRepository? _nhanVienRepository;
    private readonly IVaiTroRepository? _vaiTroRepository;

    private readonly ObservableCollection<NhanVienItemViewModel> _danhSachNhanVien = new();

    /// <summary>
    /// Khởi tạo mặc định (hỗ trợ XAML Designer & Khởi tạo độc lập)
    /// </summary>
    public UC_NhanVien()
    {
        InitializeComponent();
        dgvNhanVien.ItemsSource = _danhSachNhanVien;
        Loaded += UC_NhanVien_Loaded;
    }

    /// <summary>
    /// Khởi tạo có tiêm phụ thuộc DI từ ServiceProvider
    /// </summary>
    public UC_NhanVien(
        QuanLyNhanVienUseCase quanLyNhanVienUseCase,
        INhanVienRepository nhanVienRepository,
        IVaiTroRepository vaiTroRepository) : this()
    {
        _quanLyNhanVienUseCase = quanLyNhanVienUseCase;
        _nhanVienRepository = nhanVienRepository;
        _vaiTroRepository = vaiTroRepository;
    }

    private void UC_NhanVien_Loaded(object sender, RoutedEventArgs e)
    {
        NapDanhSachVaiTro();
        TaiDanhSachNhanVien();
    }

    /// <summary>
    /// Nạp danh sách vai trò vào ComboBox
    /// </summary>
    private void NapDanhSachVaiTro()
    {
        try
        {
            if (_vaiTroRepository != null)
            {
                var vaiTros = _vaiTroRepository.LayTatCa();
                if (vaiTros != null && vaiTros.Count > 0)
                {
                    cboVaiTro.ItemsSource = vaiTros;
                    return;
                }
            }
        }
        catch
        {
            // Fallback danh mục vai trò chuẩn từ Seed Data hệ thống
        }

        var danhSachMacDinh = new List<VaiTro>
        {
            new("VT_ADMIN", "Quản trị viên", "Toàn quyền hệ thống"),
            new("VT_QUANLY", "Quản lý cửa hàng", "Điều hành và báo cáo"),
            new("VT_THUNGAN", "Thu ngân", "Bán hàng POS và xuất hóa đơn"),
            new("VT_THUKHO", "Thủ kho", "Nhập hàng và kiểm kê kho")
        };
        cboVaiTro.ItemsSource = danhSachMacDinh;
    }

    /// <summary>
    /// Tải danh sách nhân viên hiển thị lên DataGrid
    /// </summary>
    private void TaiDanhSachNhanVien()
    {
        _danhSachNhanVien.Clear();

        // Thêm dữ liệu mẫu hiển thị ban đầu
        _danhSachNhanVien.Add(new NhanVienItemViewModel
        {
            MaNhanVien = "NV001",
            HoTen = "Nguyễn Văn Quản Lý",
            SoDienThoai = "0901112233",
            TenDangNhap = "quanly01",
            MaVaiTro = "VT_QUANLY",
            LuongCoBan = 15000000,
            TrangThai = true
        });

        _danhSachNhanVien.Add(new NhanVienItemViewModel
        {
            MaNhanVien = "NV002",
            HoTen = "Trần Thị Thu Ngân",
            SoDienThoai = "0902223344",
            TenDangNhap = "thungan01",
            MaVaiTro = "VT_THUNGAN",
            LuongCoBan = 9000000,
            TrangThai = true
        });
    }

    /// <summary>
    /// Sự kiện bấm nút Thêm nhân viên
    /// </summary>
    private void BtnThemNhanVien_Click(object sender, RoutedEventArgs e)
    {
        var hoTen = txtHoTen.Text.Trim();
        var sdt = txtSoDienThoai.Text.Trim();
        var tenDn = txtTenDangNhap.Text.Trim();
        var matKhau = txtMatKhau.Password;
        var vaiTro = cboVaiTro.SelectedValue?.ToString() ?? "VT_THUNGAN";

        if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(tenDn) || string.IsNullOrWhiteSpace(matKhau))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ các trường bắt buộc (*).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(txtLuongCoBan.Text.Trim(), out var luongCoBan) || luongCoBan < 0)
        {
            MessageBox.Show("Mức lương cơ bản không hợp lệ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var request = new TaoNhanVienDTO
            {
                HoTen = hoTen,
                SoDienThoai = sdt,
                TenDangNhap = tenDn,
                MatKhau = matKhau,
                LuongCoBan = luongCoBan,
                MaVaiTro = vaiTro
            };

            if (_quanLyNhanVienUseCase != null)
            {
                var thanhCong = _quanLyNhanVienUseCase.TaoNhanVien(request);
                if (!thanhCong)
                {
                    MessageBox.Show("Không thể tạo nhân viên mới vào hệ thống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Cập nhật lên DataGrid giao diện
            var maNvMoi = $"NV{_danhSachNhanVien.Count + 1:D3}";
            _danhSachNhanVien.Add(new NhanVienItemViewModel
            {
                MaNhanVien = maNvMoi,
                HoTen = hoTen,
                SoDienThoai = sdt,
                TenDangNhap = tenDn,
                MaVaiTro = vaiTro,
                LuongCoBan = luongCoBan,
                TrangThai = true
            });

            MessageBox.Show($"Thêm mới nhân viên '{hoTen}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            XoaTrangForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Sự kiện bấm nút Khóa tài khoản nhân viên được chọn
    /// </summary>
    private void BtnKhoaTaiKhoan_Click(object sender, RoutedEventArgs e)
    {
        if (dgvNhanVien.SelectedItem is not NhanVienItemViewModel selectedNv)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần khóa tài khoản.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!selectedNv.TrangThai)
        {
            MessageBox.Show($"Tài khoản nhân viên '{selectedNv.HoTen}' đã bị khóa trước đó.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn khóa tài khoản nhân viên '{selectedNv.HoTen}' ({selectedNv.MaNhanVien})?", "Xác nhận khóa tài khoản", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (xacNhan != MessageBoxResult.Yes) return;

        try
        {
            if (_quanLyNhanVienUseCase != null)
            {
                _quanLyNhanVienUseCase.KhoaTaiKhoanNhanVien(selectedNv.MaNhanVien);
            }

            selectedNv.TrangThai = false;
            dgvNhanVien.Items.Refresh();
            MessageBox.Show($"Đã khóa tài khoản nhân viên '{selectedNv.HoTen}' thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
    {
        XoaTrangForm();
    }

    private void XoaTrangForm()
    {
        txtHoTen.Clear();
        txtSoDienThoai.Clear();
        txtTenDangNhap.Clear();
        txtMatKhau.Clear();
        txtLuongCoBan.Clear();
        cboVaiTro.SelectedIndex = 0;
    }
}

/// <summary>
/// ViewModel hỗ trợ hiển thị danh sách dòng trên DataGrid WPF
/// </summary>
public class NhanVienItemViewModel
{
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string MaVaiTro { get; set; } = string.Empty;
    public decimal LuongCoBan { get; set; }
    public bool TrangThai { get; set; }
}
