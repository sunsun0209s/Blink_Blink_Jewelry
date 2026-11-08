using System;
using System.Drawing;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Admin.Views.UserControls.Catalog;
using BlinkBlink.Admin.Views.UserControls.Identity;
using BlinkBlink.Admin.Views.UserControls.Orders;
using BlinkBlink.Admin.Views.UserControls.Reporting;
using BlinkBlink.Admin.Views.UserControls.Warranty;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Admin.Forms;

/// <summary>
/// Giao diện chính của hệ thống quản trị Blink Blink Jewelry trên nền Windows Forms.
/// Thiết kế chuẩn thanh lịch, sang trọng phù hợp với ngành kim hoàn theo Taste Skill.
/// Hỗ trợ phân quyền nghiêm ngặt 3 vai trò: Quản trị viên (ADMIN), Nhân viên bán hàng (SALE), Nhân viên kho (WAREHOUSE).
/// </summary>
public partial class MainForm : Form
{
    private Button? _currentActiveButton;
    private UserControl? _currentView;

    // Cache các UserControl phân hệ để tối ưu hiệu năng
    private OrdersView? _ordersView;
    private OfflineSalesView? _offlineSalesView;
    private ProductsAndInventoryContainerView? _productsContainerView;
    private WarrantyView? _warrantyView;
    private UsersView? _usersView;
    private ReportingView? _reportingView;

    // Nút sidebar bổ sung cho bán hàng Offline tại quầy
    private Button _btnOfflineSales = null!;
    private Button _btnLogout = null!;
    private Button _btnChangePassword = null!;

    // Người dùng đang đăng nhập
    private NguoiDung _currentUser = null!;

    /// <summary>
    /// Cờ thông báo phiên làm việc kết thúc do người dùng chủ động Đăng Xuất.
    /// </summary>
    public bool IsLogoutRequested { get; private set; }

    // Vai trò hiện tại: "ADMIN", "SALE", "WAREHOUSE"
    private string _currentRole = "ADMIN";

    public MainForm(NguoiDung loggedInUser)
    {
        _currentUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser));
        InitializeComponent();
        SetupCustomSidebarButtons();
        RegisterEventHandlers();
        CheckDatabaseStatus();
        ApplyRolePermissions(_currentUser.MaVaiTro);
    }

    public MainForm()
    {
        InitializeComponent();
        SetupCustomSidebarButtons();
        RegisterEventHandlers();
        CheckDatabaseStatus();

        _currentUser ??= AdminServiceScope.Instance.CurrentUser 
                         ?? new AdminDefaultUser("USER_NV001", "Quản Trị Viên", "0900000000", "admin", "admin123", "ADMIN");

        ApplyRolePermissions(_currentUser.MaVaiTro);
    }

    private void CheckDatabaseStatus()
    {
        // Hiển thị trạng thái kết nối nguồn dữ liệu trên StatusStrip
        var isDbConnected = AdminServiceScope.Instance.IsDatabaseConnected;
        if (isDbConnected)
        {
            statusLabel.Text = "Hệ thống kết nối CSDL SQL Server thành công | Clean Architecture .NET 8 LTS";
            statusLabel.ForeColor = UITheme.SuccessColor;
        }
        else
        {
            statusLabel.Text = "Đang kích hoạt chế độ Dữ Liệu Trực Quan (In-Memory Studio Mode) | Clean Architecture .NET 8 LTS";
            statusLabel.ForeColor = UITheme.WarningColor;
        }
    }

    private void SetupCustomSidebarButtons()
    {
        // Đổi tên nút bán hàng thành Quản lý đơn hàng
        btnSales.Text = "🛒 Quản Lý Đơn Hàng";

        // Tạo nút POS Bán Hàng Tại Quầy
        _btnOfflineSales = new Button
        {
            BackColor = Color.FromArgb(24, 28, 36),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 },
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(220, 225, 235),
            Location = new Point(0, 168),
            Name = "btnOfflineSales",
            Padding = new Padding(22, 0, 10, 0),
            Size = new Size(350, 58),
            TabIndex = 2,
            Text = "🏪 Bán Hàng Tại Quầy (POS)",
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false,
            UseVisualStyleBackColor = false
        };

        // Sắp xếp lại vị trí các nút sidebar gọn gàng
        btnSales.Location = new Point(0, 106);
        btnSales.Size = new Size(350, 58);

        _btnOfflineSales.Location = new Point(0, 168);
        _btnOfflineSales.Size = new Size(350, 58);

        btnProducts.Location = new Point(0, 230);
        btnProducts.Size = new Size(350, 58);

        btnWarranty.Location = new Point(0, 292);
        btnWarranty.Size = new Size(350, 58);

        btnUsers.Location = new Point(0, 354);
        btnUsers.Size = new Size(350, 58);

        btnReports.Location = new Point(0, 416);
        btnReports.Size = new Size(350, 58);

        panelSidebar.Controls.Add(_btnOfflineSales);

        // Nút Đăng Xuất trên thanh tiêu đề Header (góc phải)
        _btnLogout = UITheme.CreateFlatButton("🚪 Đăng Xuất", Color.FromArgb(220, 38, 38), Color.White);
        _btnLogout.Dock = DockStyle.Right;
        _btnLogout.Width = 115;
        _btnLogout.Margin = new Padding(0, 15, 15, 15);
        _btnLogout.Click += BtnLogout_Click;

        // Nút Đổi Mật Khẩu trên thanh tiêu đề Header (kế bên nút đăng xuất)
        _btnChangePassword = UITheme.CreateFlatButton("🔑 Đổi Mật Khẩu", Color.FromArgb(51, 65, 85), Color.FromArgb(241, 245, 249));
        _btnChangePassword.Dock = DockStyle.Right;
        _btnChangePassword.Width = 135;
        _btnChangePassword.Margin = new Padding(0, 15, 8, 15);
        _btnChangePassword.Click += BtnChangePassword_Click;

        panelHeader.Controls.Add(_btnLogout);
        panelHeader.Controls.Add(_btnChangePassword);
        _btnLogout.BringToFront();
        _btnChangePassword.BringToFront();
    }

    private void RegisterEventHandlers()
    {
        btnSales.Click += (s, e) => ShowView(GetOrdersView(), "Quản Lý Đơn Hàng Online & Offline", btnSales);
        _btnOfflineSales.Click += (s, e) => ShowView(GetOfflineSalesView(), "Bán Hàng Tại Quầy (POS Bán Lẻ)", _btnOfflineSales);
        btnProducts.Click += (s, e) => ShowView(GetProductsContainerView(), "Quản Lý Sản Phẩm, Danh Mục & Kho Hàng", btnProducts);
        btnWarranty.Click += (s, e) => ShowView(GetWarrantyView(), "Dịch Vụ & Tiếp Nhận Bảo Hành Kim Hoàn", btnWarranty);
        btnUsers.Click += (s, e) => ShowView(GetUsersView(), "Quản Lý Khách Hàng & Phân Quyền Nhân Sự", btnUsers);
        btnReports.Click += (s, e) => ShowView(GetReportingView(), "Báo Cáo Doanh Thu & Thống Kê Kho Hàng", btnReports);
    }

    private OrdersView GetOrdersView() => _ordersView ??= new OrdersView();
    private OfflineSalesView GetOfflineSalesView() => _offlineSalesView ??= new OfflineSalesView();
    private ProductsAndInventoryContainerView GetProductsContainerView() => _productsContainerView ??= new ProductsAndInventoryContainerView();
    private WarrantyView GetWarrantyView()
    {
        _warrantyView ??= new WarrantyView();
        _warrantyView.ApplyPermissions(_currentRole);
        return _warrantyView;
    }
    private UsersView GetUsersView() => _usersView ??= new UsersView();
    private ReportingView GetReportingView() => _reportingView ??= new ReportingView();

    private void ShowView(UserControl view, string moduleTitle, Button navButton)
    {
        if (_currentView == view) return;

        lblHeaderTitle.Text = moduleTitle;
        HighlightNavButton(navButton);

        panelContent.SuspendLayout();
        panelContent.Controls.Clear();
        view.Dock = DockStyle.Fill;
        panelContent.Controls.Add(view);
        panelContent.ResumeLayout(true);

        _currentView = view;
    }

    private void HighlightNavButton(Button btn)
    {
        if (_currentActiveButton != null)
        {
            _currentActiveButton.BackColor = UITheme.DarkSidebar;
            _currentActiveButton.ForeColor = Color.FromArgb(220, 225, 235);
        }

        _currentActiveButton = btn;
        _currentActiveButton.BackColor = Color.FromArgb(40, 48, 62);
        _currentActiveButton.ForeColor = UITheme.PrimaryGold; // Highlight Vàng Champagne
    }

    /// <summary>
    /// Áp dụng ma trận phân quyền 3 vai trò: ADMIN, SALE, WAREHOUSE.
    /// Định dạng dòng trạng thái: Tài khoản: tên đăng nhập [vai trò]
    /// </summary>
    public void ApplyRolePermissions(string roleCode)
    {
        _currentRole = roleCode?.ToUpperInvariant() ?? "ADMIN";
        var username = _currentUser?.TenDangNhap ?? "admin";
        var shortRole = _currentRole.ToLowerInvariant(); // admin, sale, warehouse

        lblUserRole.Text = $"Tài khoản: {username} [{shortRole}]";

        switch (_currentRole)
        {
            case "ADMIN":
                // 1. Quản trị viên: Toàn quyền sử dụng tất cả chức năng
                btnSales.Visible = true;
                _btnOfflineSales.Visible = true;
                btnProducts.Visible = true;
                btnWarranty.Visible = true;
                btnUsers.Visible = true;
                btnReports.Visible = true;

                lblUserRole.ForeColor = UITheme.PrimaryGold;
                ShowView(GetOrdersView(), "Quản Lý Đơn Hàng Online & Offline", btnSales);
                break;

            case "SALE":
                // 2. Nhân viên bán hàng: Chỉ quản lý đơn hàng & khách hàng, KHÔNG quản lý kho và báo cáo
                btnSales.Visible = true;
                _btnOfflineSales.Visible = true;
                btnWarranty.Visible = true;
                btnUsers.Visible = true;

                btnProducts.Visible = false; // Chặn kho hàng
                btnReports.Visible = false;  // Chặn báo cáo quản trị

                lblUserRole.ForeColor = UITheme.SuccessColor;
                ShowView(GetOfflineSalesView(), "Bán Hàng Tại Quầy (POS Bán Lẻ)", _btnOfflineSales);
                break;

            case "WAREHOUSE":
                // 3. Nhân viên kho: Chỉ quản lý kho hàng, KHÔNG quản lý đơn hàng & khách hàng
                btnProducts.Visible = true; // Chỉ bật quản lý sản phẩm, kho hàng, nhập kho & kiểm kê

                btnSales.Visible = false;
                _btnOfflineSales.Visible = false;
                btnWarranty.Visible = false;
                btnUsers.Visible = false;
                btnReports.Visible = false;

                lblUserRole.ForeColor = Color.FromArgb(56, 189, 248);
                ShowView(GetProductsContainerView(), "Quản Lý Sản Phẩm, Danh Mục & Kho Hàng", btnProducts);
                break;

            case "TECHNICIAN":
                // 4. Nhân viên kỹ thuật: Phụ trách tiếp nhận xử lý, sửa chữa và hoàn thiện bảo hành trang sức
                btnWarranty.Visible = true; // Phân hệ trọng tâm của kỹ thuật viên
                btnProducts.Visible = true; // Cho phép xem danh mục sản phẩm, chất liệu

                btnSales.Visible = false;
                _btnOfflineSales.Visible = false;
                btnUsers.Visible = false;
                btnReports.Visible = false;

                lblUserRole.ForeColor = Color.FromArgb(245, 158, 11); // Màu cam hổ phách kỹ thuật
                ShowView(GetWarrantyView(), "Dịch Vụ & Tiếp Nhận Bảo Hành Kim Hoàn", btnWarranty);
                break;
        }
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống Blink Blink Jewelry không?",
            "Xác Nhận Đăng Xuất",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm == DialogResult.Yes)
        {
            IsLogoutRequested = true;
            this.Close();
        }
    }

    private void BtnChangePassword_Click(object? sender, EventArgs e)
    {
        if (_currentUser != null)
        {
            using var dlg = new ChangePasswordDialog(_currentUser);
            dlg.ShowDialog(this);
        }
    }

    private sealed class AdminDefaultUser : NguoiDung
    {
        public AdminDefaultUser(string ma, string ten, string sdt, string user, string pass, string role)
            : base(ma, ten, sdt, user, pass, role, true)
        {
        }
    }
}
