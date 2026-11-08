using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Admin.Views.UserControls.Identity;

/// <summary>
/// Màn hình Quản lý Khách hàng & Phân quyền Nhân sự Blink Blink Jewelry.
/// Quản lý hồ sơ nhân viên, phân quyền chức vụ, danh sách khách hàng thân thiết.
/// Khớp 100% bản vẽ kỹ thuật NhanVien và KhachHang.
/// </summary>
public class UsersView : UserControl
{
    private readonly AdminServiceScope _services;
    private TabControl _tabControl = null!;

    // Tab Nhân viên
    private List<NhanVien> _allStaff = new();
    private TextBox _txtSearchStaff = null!;
    private DataGridView _dgvStaff = null!;
    private Label _lblKpiStaffTotal = null!;
    private Label _lblKpiStaffAdmin = null!;

    // Tab Khách hàng
    private List<KhachHang> _allCustomers = new();
    private TextBox _txtSearchCustomer = null!;
    private DataGridView _dgvCustomers = null!;
    private Label _lblKpiCustomerTotal = null!;
    private Label _lblKpiCustomerActive = null!;

    public UsersView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadStaffData();
        LoadCustomerData();
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // Header Panel
        var topHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 85,
            Padding = new Padding(20, 15, 20, 10),
            BackColor = UITheme.CanvasBackground
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ KHÁCH HÀNG & PHÂN QUYỀN NHÂN SỰ",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(20, 12),
            AutoSize = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Kiểm soát tài khoản nhân viên, thiết lập vai trò phân quyền và chăm sóc thông tin khách hàng",
            Font = UITheme.RegularFont,
            ForeColor = UITheme.TextSecondary,
            Location = new Point(20, 38),
            AutoSize = true,
            UseMnemonic = false
        };

        topHeaderPanel.Controls.Add(lblTitle);
        topHeaderPanel.Controls.Add(lblSubTitle);

        // Tab Control
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Padding = new Point(16, 8)
        };

        var tabStaff = new TabPage("👥 Nhân Viên & Phân Quyền");
        tabStaff.BackColor = UITheme.CanvasBackground;
        BuildStaffTab(tabStaff);

        var tabCustomers = new TabPage("👤 Quản Lý Khách Hàng");
        tabCustomers.BackColor = UITheme.CanvasBackground;
        BuildCustomersTab(tabCustomers);

        _tabControl.TabPages.Add(tabStaff);
        _tabControl.TabPages.Add(tabCustomers);

        this.Controls.Add(_tabControl);
        this.Controls.Add(topHeaderPanel);
    }

    private void BuildStaffTab(TabPage tab)
    {
        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 80,
            Padding = new Padding(15, 5, 15, 5),
            BackColor = UITheme.CanvasBackground
        };
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Nhân Viên", out _lblKpiStaffTotal, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Quản Trị Viên (Admin)", out _lblKpiStaffAdmin, UITheme.PrimaryGold));

        var toolbar = UITheme.CreateResponsiveToolbar();

        var lblS = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = UITheme.TextSecondary, Margin = new Padding(0, 8, 6, 0) };
        _txtSearchStaff = new TextBox { Width = 200, Height = 28, PlaceholderText = "Tên, mã NV, username...", Margin = new Padding(0, 4, 15, 4) };
        _txtSearchStaff.TextChanged += (s, e) => FilterStaff();

        var btnAdd = UITheme.CreateFlatButton("+ Thêm Nhân Viên", UITheme.PrimaryGold, Color.Black);
        btnAdd.Margin = new Padding(0, 2, 8, 2);
        btnAdd.Click += BtnAddStaff_Click;

        var btnEdit = UITheme.CreateFlatButton("✏️ Sửa Nhân Viên", UITheme.PrimaryGold, Color.Black);
        btnEdit.Margin = new Padding(0, 2, 8, 2);
        btnEdit.Click += BtnEditStaff_Click;

        var btnLock = UITheme.CreateFlatButton("🔒 Khóa / Mở Khóa", UITheme.DangerColor, Color.White);
        btnLock.Margin = new Padding(0, 2, 8, 2);
        btnLock.Click += BtnLockStaff_Click;

        var btnDelete = UITheme.CreateFlatButton("🗑️ Xóa Nhân Viên", Color.FromArgb(220, 38, 38), Color.White);
        btnDelete.Margin = new Padding(0, 2, 8, 2);
        btnDelete.Click += BtnDeleteStaff_Click;

        var btnReset = UITheme.CreateFlatButton("🔑 Đặt Lại Mật Khẩu", UITheme.DarkHeader, Color.White);
        btnReset.Margin = new Padding(0, 2, 8, 2);
        btnReset.Click += BtnResetPassword_Click;

        var btnRef = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        btnRef.Margin = new Padding(0, 2, 0, 2);
        btnRef.Click += (s, e) => LoadStaffData();

        toolbar.Controls.Add(lblS);
        toolbar.Controls.Add(_txtSearchStaff);
        toolbar.Controls.Add(btnAdd);
        toolbar.Controls.Add(btnEdit);
        toolbar.Controls.Add(btnLock);
        toolbar.Controls.Add(btnDelete);
        toolbar.Controls.Add(btnReset);
        toolbar.Controls.Add(btnRef);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 10, 15, 15), BackColor = UITheme.CanvasBackground };
        var pnlWhite = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };

        _dgvStaff = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvStaff);
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaNV", HeaderText = "MÃ NHÂN VIÊN", FillWeight = 85, MinimumWidth = 100 });
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoTen", HeaderText = "HỌ VÀ TÊN", FillWeight = 170, MinimumWidth = 180 });
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenDangNhap", HeaderText = "TÊN ĐĂNG NHẬP", FillWeight = 120, MinimumWidth = 130 });
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoDienThoai", HeaderText = "SỐ ĐIỆN THOẠI", FillWeight = 110, MinimumWidth = 115 });
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaVaiTro", HeaderText = "MÃ VAI TRÒ", FillWeight = 85, MinimumWidth = 95 });
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "LuongCoBan", HeaderText = "LƯƠNG CƠ BẢN (VND)", FillWeight = 120, MinimumWidth = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });
        _dgvStaff.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 90, MinimumWidth = 100 });

        pnlWhite.Controls.Add(_dgvStaff);
        pnlGrid.Controls.Add(pnlWhite);

        tab.Controls.Add(pnlGrid);
        tab.Controls.Add(toolbar);
        tab.Controls.Add(pnlKpis);
    }

    private void BuildCustomersTab(TabPage tab)
    {
        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 80,
            Padding = new Padding(15, 5, 15, 5),
            BackColor = UITheme.CanvasBackground
        };
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Khách Hàng", out _lblKpiCustomerTotal, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Đang Hoạt Động", out _lblKpiCustomerActive, UITheme.SuccessColor));

        var toolbar = UITheme.CreateResponsiveToolbar();

        var lblS = new Label { Text = "Tìm kiếm:", AutoSize = true, ForeColor = UITheme.TextSecondary, Margin = new Padding(0, 8, 6, 0) };
        _txtSearchCustomer = new TextBox { Width = 200, Height = 28, PlaceholderText = "Tên, mã KH, SĐT...", Margin = new Padding(0, 4, 15, 4) };
        _txtSearchCustomer.TextChanged += (s, e) => FilterCustomers();

        var btnEditCust = UITheme.CreateFlatButton("✏️ Chỉnh Sửa", UITheme.PrimaryGold, Color.Black);
        btnEditCust.Margin = new Padding(0, 2, 8, 2);
        btnEditCust.Click += BtnEditCustomer_Click;

        var btnOrderHistory = UITheme.CreateFlatButton("📜 Lịch Sử Mua Hàng", UITheme.DarkHeader, Color.White);
        btnOrderHistory.Margin = new Padding(0, 2, 8, 2);
        btnOrderHistory.Click += BtnCustomerOrders_Click;

        var btnLockCust = UITheme.CreateFlatButton("🔒 Khóa / Mở Khóa", UITheme.DangerColor, Color.White);
        btnLockCust.Margin = new Padding(0, 2, 8, 2);
        btnLockCust.Click += BtnLockCustomer_Click;

        var btnDeleteCust = UITheme.CreateFlatButton("🗑️ Xóa Khách Hàng", Color.FromArgb(220, 38, 38), Color.White);
        btnDeleteCust.Margin = new Padding(0, 2, 8, 2);
        btnDeleteCust.Click += BtnDeleteCustomer_Click;

        var btnRef = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        btnRef.Margin = new Padding(0, 2, 0, 2);
        btnRef.Click += (s, e) => LoadCustomerData();

        toolbar.Controls.Add(lblS);
        toolbar.Controls.Add(_txtSearchCustomer);
        toolbar.Controls.Add(btnEditCust);
        toolbar.Controls.Add(btnOrderHistory);
        toolbar.Controls.Add(btnLockCust);
        toolbar.Controls.Add(btnDeleteCust);
        toolbar.Controls.Add(btnRef);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 10, 15, 15), BackColor = UITheme.CanvasBackground };
        var pnlWhite = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };

        _dgvCustomers = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvCustomers);
        _dgvCustomers.CellDoubleClick += (s, e) => BtnCustomerOrders_Click(s, e);
        _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaKH", HeaderText = "MÃ KHÁCH HÀNG", FillWeight = 90, MinimumWidth = 100 });
        _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoTen", HeaderText = "HỌ VÀ TÊN", FillWeight = 180, MinimumWidth = 180 });
        _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenDangNhap", HeaderText = "TÊN ĐĂNG NHẬP", FillWeight = 120, MinimumWidth = 130 });
        _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoDienThoai", HeaderText = "SỐ ĐIỆN THOẠI", FillWeight = 110, MinimumWidth = 115 });
        _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgaySinh", HeaderText = "NGÀY SINH", FillWeight = 100, MinimumWidth = 105, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 90, MinimumWidth = 100 });

        pnlWhite.Controls.Add(_dgvCustomers);
        pnlGrid.Controls.Add(pnlWhite);

        tab.Controls.Add(pnlGrid);
        tab.Controls.Add(toolbar);
        tab.Controls.Add(pnlKpis);
    }

    private Panel CreateKpiCard(string title, out Label lblValue, Color valueColor)
    {
        var card = new Panel
        {
            Width = 175,
            Height = 70,
            BackColor = Color.White,
            Margin = new Padding(5, 5, 5, 5),
            Padding = new Padding(10, 6, 10, 6)
        };

        var lblT = new Label
        {
            Text = title,
            ForeColor = UITheme.TextSecondary,
            Font = new Font(FontFamily.GenericSansSerif, 8.5F, FontStyle.Regular),
            Dock = DockStyle.Top,
            Height = 18
        };

        lblValue = new Label
        {
            Text = "0",
            ForeColor = valueColor,
            Font = UITheme.KpiNumberFont,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        card.Controls.Add(lblValue);
        card.Controls.Add(lblT);
        return card;
    }

    private void LoadStaffData()
    {
        _allStaff = _services.NhanVienRepo.LayTatCa();
        var viVn = new CultureInfo("vi-VN");
        _lblKpiStaffTotal.Text = _allStaff.Count.ToString("N0", viVn);
        _lblKpiStaffAdmin.Text = _allStaff.Count(n => 
            string.Equals(n.MaVaiTro, "ADMIN", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(n.MaVaiTro, "VT01", StringComparison.OrdinalIgnoreCase)
        ).ToString("N0", viVn);
        FilterStaff();
    }

    private void FilterStaff()
    {
        var q = _txtSearchStaff.Text.Trim().ToLowerInvariant();
        var list = _allStaff.Where(n => string.IsNullOrEmpty(q) ||
                                        n.HoTen.ToLowerInvariant().Contains(q) ||
                                        n.MaNhanVien.ToLowerInvariant().Contains(q) ||
                                        n.TenDangNhap.ToLowerInvariant().Contains(q) ||
                                        n.SoDienThoai.Contains(q)).ToList();

        _dgvStaff.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");

        foreach (var nv in list)
        {
            var rowIdx = _dgvStaff.Rows.Add(
                nv.MaNhanVien,
                nv.HoTen,
                nv.TenDangNhap,
                nv.SoDienThoai,
                nv.MaVaiTro,
                nv.LuongCoBan.ToString("N0", viVn),
                nv.TrangThai ? "Hoạt động" : "Đã khóa"
            );

            if (!nv.TrangThai)
                _dgvStaff.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.DangerColor;
            else
                _dgvStaff.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.SuccessColor;
        }
    }

    private void LoadCustomerData()
    {
        _allCustomers = _services.KhachHangRepo.LayTatCa();
        var viVn = new CultureInfo("vi-VN");
        _lblKpiCustomerTotal.Text = _allCustomers.Count.ToString("N0", viVn);
        _lblKpiCustomerActive.Text = _allCustomers.Count(c => c.TrangThai).ToString("N0", viVn);
        FilterCustomers();
    }

    private void FilterCustomers()
    {
        var q = _txtSearchCustomer.Text.Trim().ToLowerInvariant();
        var list = _allCustomers.Where(c => string.IsNullOrEmpty(q) ||
                                            c.HoTen.ToLowerInvariant().Contains(q) ||
                                            c.MaKhachHang.ToLowerInvariant().Contains(q) ||
                                            c.TenDangNhap.ToLowerInvariant().Contains(q) ||
                                            c.SoDienThoai.Contains(q)).ToList();

        _dgvCustomers.Rows.Clear();
        foreach (var c in list)
        {
            var rowIdx = _dgvCustomers.Rows.Add(
                c.MaKhachHang,
                c.HoTen,
                c.TenDangNhap,
                c.SoDienThoai,
                c.NgaySinh.ToString("dd/MM/yyyy"),
                c.TrangThai ? "Hoạt động" : "Đã khóa"
            );

            if (!c.TrangThai)
                _dgvCustomers.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.DangerColor;
            else
                _dgvCustomers.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.SuccessColor;
        }
    }

    private void BtnAddStaff_Click(object? sender, EventArgs e)
    {
        using var dialog = new Form
        {
            Text = "Thêm Nhân Viên Mới - Blink Blink Jewelry",
            Width = 460,
            Height = 440,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblHead = new Label { Text = "HỒ SƠ NHÂN SỰ VÀ PHÂN QUYỀN VAI TRÒ", Font = UITheme.SubHeaderFont, ForeColor = UITheme.TextPrimary, Location = new Point(25, 18), AutoSize = true };
        dialog.Controls.Add(lblHead);

        int top = 55;
        TextBox AddField(string labelText, string defaultVal)
        {
            var l = new Label { Text = labelText, Location = new Point(25, top), AutoSize = true, ForeColor = UITheme.TextSecondary };
            var t = new TextBox { Text = defaultVal, Location = new Point(160, top - 3), Width = 250 };
            dialog.Controls.Add(l);
            dialog.Controls.Add(t);
            top += 40;
            return t;
        }

        var txtHoTen = AddField("Họ và Tên:", "Nguyễn Hoàng Nam");
        var txtPhone = AddField("Số Điện Thoại:", "0933445566");
        var txtUser = AddField("Tên Đăng Nhập:", "nam.nguyen");
        var txtPass = AddField("Mật Khẩu Khởi Tạo:", "123456789");
        var txtLuong = AddField("Lương Cơ Bản:", "12000000");

        var lblRole = new Label { Text = "Vai Trò Nhân Sự:", Location = new Point(25, top), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var cboRole = new ComboBox { Location = new Point(160, top - 3), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        cboRole.Items.Add(new KeyValuePair<string, string>("ADMIN", "👑 Quản trị viên (Toàn quyền)"));
        cboRole.Items.Add(new KeyValuePair<string, string>("SALE", "💼 Nhân viên bán hàng (Đơn hàng & Khách)"));
        cboRole.Items.Add(new KeyValuePair<string, string>("WAREHOUSE", "📦 Nhân viên kho (Sản phẩm & Kho hàng)"));
        cboRole.Items.Add(new KeyValuePair<string, string>("TECHNICIAN", "🔧 Nhân viên kỹ thuật (Bảo dưỡng & Sửa chữa)"));
        cboRole.DisplayMember = "Value";
        cboRole.ValueMember = "Key";
        cboRole.SelectedIndex = 1;
        dialog.Controls.Add(lblRole);
        dialog.Controls.Add(cboRole);
        top += 45;

        var btnSave = UITheme.CreateFlatButton("Tạo Nhân Viên", UITheme.PrimaryGold, Color.Black);
        btnSave.Location = new Point(160, top + 10);
        btnSave.Click += (s, ev) =>
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Vui lòng điền họ tên và tên đăng nhập.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal.TryParse(txtLuong.Text, out var luong);
            var selectedRole = cboRole.SelectedItem is KeyValuePair<string, string> kvp ? kvp.Key : "SALE";
            var req = new TaoNhanVienDTO
            {
                HoTen = txtHoTen.Text.Trim(),
                SoDienThoai = txtPhone.Text.Trim(),
                TenDangNhap = txtUser.Text.Trim(),
                MatKhau = txtPass.Text.Trim(),
                LuongCoBan = luong,
                MaVaiTro = selectedRole
            };

            var ok = _services.QuanLyNhanVien.TaoNhanVien(req);
            if (ok)
            {
                MessageBox.Show("Tạo hồ sơ và tài khoản nhân viên mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            else
            {
                MessageBox.Show("Không thể tạo nhân viên. Vui lòng kiểm tra tên đăng nhập có bị trùng lặp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        dialog.Controls.Add(btnSave);

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadStaffData();
        }
    }

    private void BtnEditStaff_Click(object? sender, EventArgs e)
    {
        if (_dgvStaff.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần chỉnh sửa thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maNv = _dgvStaff.SelectedRows[0].Cells["MaNV"].Value?.ToString();
        if (string.IsNullOrEmpty(maNv)) return;

        var staff = _allStaff.FirstOrDefault(s => s.MaNhanVien == maNv) ?? _services.NhanVienRepo.TimTheoMa(maNv);
        if (staff == null)
        {
            MessageBox.Show("Không tìm thấy thông tin nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var dialog = new Form
        {
            Text = $"Chỉnh Sửa Nhân Viên - [{staff.MaNhanVien}]",
            Width = 460,
            Height = 420,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblHead = new Label { Text = "CẬP NHẬT THÔNG TIN & VAI TRÒ NHÂN SỰ", Font = UITheme.SubHeaderFont, ForeColor = UITheme.TextPrimary, Location = new Point(25, 18), AutoSize = true };
        dialog.Controls.Add(lblHead);

        int top = 55;
        TextBox AddField(string labelText, string defaultVal)
        {
            var l = new Label { Text = labelText, Location = new Point(25, top), AutoSize = true, ForeColor = UITheme.TextSecondary };
            var t = new TextBox { Text = defaultVal, Location = new Point(160, top - 3), Width = 250 };
            dialog.Controls.Add(l);
            dialog.Controls.Add(t);
            top += 40;
            return t;
        }

        var txtMaNv = AddField("Mã Nhân Viên:", staff.MaNhanVien);
        txtMaNv.ReadOnly = true;
        txtMaNv.BackColor = Color.FromArgb(245, 245, 245);

        var txtHoTen = AddField("Họ và Tên:", staff.HoTen);
        var txtPhone = AddField("Số Điện Thoại:", staff.SoDienThoai);
        var txtLuong = AddField("Lương Cơ Bản:", staff.LuongCoBan.ToString("F0"));

        var lblRole = new Label { Text = "Vai Trò Phân Quyền:", Location = new Point(25, top), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var cboRole = new ComboBox { Location = new Point(160, top - 3), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        cboRole.Items.Add(new KeyValuePair<string, string>("ADMIN", "👑 Quản trị viên (Toàn quyền)"));
        cboRole.Items.Add(new KeyValuePair<string, string>("SALE", "💼 Nhân viên bán hàng (Đơn hàng & Khách)"));
        cboRole.Items.Add(new KeyValuePair<string, string>("WAREHOUSE", "📦 Nhân viên kho (Sản phẩm & Kho hàng)"));
        cboRole.Items.Add(new KeyValuePair<string, string>("TECHNICIAN", "🔧 Nhân viên kỹ thuật (Bảo dưỡng & Sửa chữa)"));
        cboRole.DisplayMember = "Value";
        cboRole.ValueMember = "Key";

        // Chọn vai trò hiện tại
        var currentRoleCode = staff.MaVaiTro?.ToUpperInvariant() ?? "SALE";
        if (currentRoleCode == "VT01" || currentRoleCode == "ADMIN") cboRole.SelectedIndex = 0;
        else if (currentRoleCode == "VT02" || currentRoleCode == "SALE") cboRole.SelectedIndex = 1;
        else if (currentRoleCode == "VT03" || currentRoleCode == "WAREHOUSE") cboRole.SelectedIndex = 2;
        else if (currentRoleCode == "VT04" || currentRoleCode == "TECHNICIAN") cboRole.SelectedIndex = 3;
        else cboRole.SelectedIndex = 1;

        dialog.Controls.Add(lblRole);
        dialog.Controls.Add(cboRole);
        top += 45;

        var btnSave = UITheme.CreateFlatButton("Lưu Thay Đổi", UITheme.PrimaryGold, Color.Black);
        btnSave.Location = new Point(160, top + 10);
        btnSave.Click += (s, ev) =>
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Họ và tên nhân viên không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal.TryParse(txtLuong.Text, out var luong);
            var selectedRole = cboRole.SelectedItem is KeyValuePair<string, string> kvp ? kvp.Key : "SALE";

            var ok = _services.QuanLyNhanVien.CapNhatThongTinNhanVien(staff.MaNhanVien, txtHoTen.Text.Trim(), txtPhone.Text.Trim(), luong, selectedRole);
            if (ok)
            {
                MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            else
            {
                MessageBox.Show("Cập nhật thông tin thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        dialog.Controls.Add(btnSave);

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadStaffData();
        }
    }

    private void BtnDeleteStaff_Click(object? sender, EventArgs e)
    {
        if (_dgvStaff.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maNv = _dgvStaff.SelectedRows[0].Cells["MaNV"].Value?.ToString();
        var hoTen = _dgvStaff.SelectedRows[0].Cells["HoTen"].Value?.ToString() ?? maNv;
        if (string.IsNullOrEmpty(maNv)) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa nhân viên [{hoTen}] ({maNv}) khỏi hệ thống?",
            "Xác nhận xóa nhân sự",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        var ok = _services.QuanLyNhanVien.XoaNhanVien(maNv, out var thongBao);
        if (ok)
        {
            MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStaffData();
        }
        else
        {
            var askLock = MessageBox.Show(
                $"{thongBao}\n\nBạn có muốn chuyển sang KHÓA TÀI KHOẢN nhân viên này để ngừng quyền truy cập mà vẫn bảo toàn lịch sử hoạt động không?",
                "Cảnh báo ràng buộc nghiệp vụ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (askLock == DialogResult.Yes)
            {
                _services.QuanLyNhanVien.KhoaTaiKhoanNhanVien(maNv);
                LoadStaffData();
                MessageBox.Show($"Đã khóa tài khoản nhân viên [{maNv}] thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void BtnLockStaff_Click(object? sender, EventArgs e)
    {
        if (_dgvStaff.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần khóa/mở khóa tài khoản.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maNv = _dgvStaff.SelectedRows[0].Cells["MaNV"].Value?.ToString();
        if (string.IsNullOrEmpty(maNv)) return;

        var ok = _services.QuanLyNhanVien.KhoaTaiKhoanNhanVien(maNv);
        if (ok)
        {
            MessageBox.Show($"Đã thay đổi trạng thái khóa/mở khóa tài khoản nhân viên [{maNv}].", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStaffData();
        }
        else
        {
            MessageBox.Show("Không tìm thấy nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnResetPassword_Click(object? sender, EventArgs e)
    {
        if (_dgvStaff.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần đặt lại mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maNv = _dgvStaff.SelectedRows[0].Cells["MaNV"].Value?.ToString();
        if (string.IsNullOrEmpty(maNv)) return;

        var confirm = MessageBox.Show(
            $"Bạn có muốn đặt lại mật khẩu của nhân viên [{maNv}] về mật khẩu mặc định (123456789)?",
            "Xác nhận đặt lại mật khẩu",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        var ok = _services.QuanLyNhanVien.ResetMatKhau(maNv);
        if (ok)
        {
            MessageBox.Show($"Đã đặt lại mật khẩu mặc định cho nhân viên [{maNv}]: 123456789", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnDeleteCustomer_Click(object? sender, EventArgs e)
    {
        if (_dgvCustomers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn khách hàng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maKh = _dgvCustomers.SelectedRows[0].Cells["MaKH"].Value?.ToString();
        var hoTen = _dgvCustomers.SelectedRows[0].Cells["HoTen"].Value?.ToString() ?? maKh;
        if (string.IsNullOrEmpty(maKh)) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa khách hàng [{hoTen}] ({maKh}) khỏi hệ thống?",
            "Xác nhận xóa khách hàng",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        var ok = _services.QuanLyKhachHang.XoaKhachHang(maKh, out var thongBao);
        if (ok)
        {
            MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCustomerData();
        }
        else
        {
            var askLock = MessageBox.Show(
                $"{thongBao}\n\nBạn có muốn chuyển sang KHÓA TÀI KHOẢN khách hàng này để giữ trọn vẹn lịch sử giao dịch kế toán?",
                "Cảnh báo ràng buộc dữ liệu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (askLock == DialogResult.Yes)
            {
                _services.QuanLyKhachHang.KhoaTaiKhoanKhachHang(maKh);
                LoadCustomerData();
                MessageBox.Show($"Đã khóa tài khoản khách hàng [{maKh}] thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void BtnLockCustomer_Click(object? sender, EventArgs e)
    {
        if (_dgvCustomers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn khách hàng cần thao tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maKh = _dgvCustomers.SelectedRows[0].Cells["MaKH"].Value?.ToString();
        if (string.IsNullOrEmpty(maKh)) return;

        var kh = _allCustomers.FirstOrDefault(c => c.MaKhachHang == maKh);
        var actionText = (kh != null && kh.TrangThai) ? "khóa" : "mở khóa";

        var ok = _services.QuanLyKhachHang.KhoaTaiKhoanKhachHang(maKh);
        if (ok)
        {
            MessageBox.Show($"Đã {actionText} tài khoản khách hàng [{maKh}] thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCustomerData();
        }
        else
        {
            MessageBox.Show("Thao tác thất bại. Không tìm thấy khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEditCustomer_Click(object? sender, EventArgs e)
    {
        if (_dgvCustomers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn khách hàng cần chỉnh sửa thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maKh = _dgvCustomers.SelectedRows[0].Cells["MaKH"].Value?.ToString();
        if (string.IsNullOrEmpty(maKh)) return;

        var kh = _allCustomers.FirstOrDefault(c => c.MaKhachHang == maKh) ?? _services.KhachHangRepo.TimTheoMa(maKh);
        if (kh == null)
        {
            MessageBox.Show("Không tìm thấy thông tin khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var dialog = new Form
        {
            Text = $"Chỉnh Sửa Hồ Sơ Khách Hàng - [{kh.MaKhachHang}]",
            Width = 440,
            Height = 350,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblHead = new Label { Text = "CẬP NHẬT THÔNG TIN KHÁCH HÀNG", Font = UITheme.SubHeaderFont, ForeColor = UITheme.TextPrimary, Location = new Point(25, 18), AutoSize = true };
        dialog.Controls.Add(lblHead);

        int top = 55;
        TextBox AddField(string labelText, string defaultVal)
        {
            var l = new Label { Text = labelText, Location = new Point(25, top), AutoSize = true, ForeColor = UITheme.TextSecondary };
            var t = new TextBox { Text = defaultVal, Location = new Point(160, top - 3), Width = 230 };
            dialog.Controls.Add(l);
            dialog.Controls.Add(t);
            top += 40;
            return t;
        }

        var txtMaKh = AddField("Mã Khách Hàng:", kh.MaKhachHang);
        txtMaKh.ReadOnly = true;
        txtMaKh.BackColor = Color.FromArgb(245, 245, 245);

        var txtHoTen = AddField("Họ và Tên:", kh.HoTen);
        var txtPhone = AddField("Số Điện Thoại:", kh.SoDienThoai);

        var lblDob = new Label { Text = "Ngày Sinh:", Location = new Point(25, top), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var dtpDob = new DateTimePicker { Location = new Point(160, top - 3), Width = 230, Format = DateTimePickerFormat.Short, Value = kh.NgaySinh > DateTime.MinValue ? kh.NgaySinh : new DateTime(1995, 1, 1) };
        dialog.Controls.Add(lblDob);
        dialog.Controls.Add(dtpDob);
        top += 45;

        var btnSave = UITheme.CreateFlatButton("Lưu Thay Đổi", UITheme.PrimaryGold, Color.Black);
        btnSave.Location = new Point(160, top + 10);
        btnSave.Click += (s, ev) =>
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Họ và tên khách hàng không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new CapNhatKhachHangDTO
            {
                MaKhachHang = kh.MaKhachHang,
                HoTen = txtHoTen.Text.Trim(),
                SoDienThoai = txtPhone.Text.Trim(),
                NgaySinh = dtpDob.Value
            };

            var ok = _services.QuanLyKhachHang.CapNhatThongTin(dto);
            if (ok)
            {
                MessageBox.Show("Cập nhật hồ sơ khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            else
            {
                MessageBox.Show("Cập nhật thông tin thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        dialog.Controls.Add(btnSave);

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadCustomerData();
        }
    }

    private void BtnCustomerOrders_Click(object? sender, EventArgs e)
    {
        if (_dgvCustomers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn khách hàng cần xem lịch sử mua hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maKh = _dgvCustomers.SelectedRows[0].Cells["MaKH"].Value?.ToString();
        var hoTen = _dgvCustomers.SelectedRows[0].Cells["HoTen"].Value?.ToString() ?? maKh;
        if (string.IsNullOrEmpty(maKh)) return;

        var orders = _services.HoaDonRepo.LayTheoKhachHang(maKh);

        using var dialog = new Form
        {
            Text = $"Lịch Sử Đơn Hàng - Khách Hàng: {hoTen} ({maKh})",
            Width = 780,
            Height = 450,
            StartPosition = FormStartPosition.CenterParent,
            BackColor = UITheme.CanvasBackground
        };

        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(20, 15, 20, 0), BackColor = Color.White };
        var titleText = $"DANH SÁCH ĐƠN HÀNG CỦA {(hoTen ?? maKh).ToUpper()} (TỔNG CỘNG: {orders.Count} ĐƠN)";
        var lblTitle = new Label { Text = titleText, Font = UITheme.SubHeaderFont, ForeColor = UITheme.TextPrimary, AutoSize = true };
        pnlTop.Controls.Add(lblTitle);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = UITheme.CanvasBackground };
        var dgvOrders = new DataGridView();
        UITheme.ApplyGridViewStyle(dgvOrders);
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaHD", HeaderText = "MÃ ĐƠN HÀNG", FillWeight = 120, MinimumWidth = 140 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgayLap", HeaderText = "NGÀY LẬP", FillWeight = 110, MinimumWidth = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongTien", HeaderText = "TỔNG TIỀN (VND)", FillWeight = 120, MinimumWidth = 130, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhuongThuc", HeaderText = "PHƯƠNG THỨC TT", FillWeight = 110, MinimumWidth = 120 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 100, MinimumWidth = 110 });

        var viVn = new CultureInfo("vi-VN");
        foreach (var hd in orders)
        {
            var rowIdx = dgvOrders.Rows.Add(
                hd.MaHoaDon,
                hd.NgayLap.ToString("dd/MM/yyyy HH:mm"),
                hd.TongThanhToan.ToString("N0", viVn),
                hd.PhuongThucThanhToan,
                hd.TrangThaiDonHang
            );

            if (hd.TrangThaiDonHang == "HoanTat")
                dgvOrders.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.SuccessColor;
            else if (hd.TrangThaiDonHang == "DaHuy")
                dgvOrders.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.DangerColor;
            else
                dgvOrders.Rows[rowIdx].Cells["TrangThai"].Style.ForeColor = UITheme.PrimaryGold;
        }

        var pnlInner = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
        pnlInner.Controls.Add(dgvOrders);
        pnlGrid.Controls.Add(pnlInner);

        dialog.Controls.Add(pnlGrid);
        dialog.Controls.Add(pnlTop);

        dialog.ShowDialog();
    }
}
