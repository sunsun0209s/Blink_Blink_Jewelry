using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.DTOs.Inventory;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;

namespace BlinkBlink.Admin.Views.UserControls.Inventory;

/// <summary>
/// Giao diện Quản lý Kiểm kê Kho hàng Trang Sức Blink Blink Jewelry.
/// Cho phép thủ kho lập biên bản kiểm kê, quản trị viên duyệt phiếu và cân bằng tồn kho.
/// Áp dụng thuật toán Anti-Conflict: Kiểm soát lệch tồn thời điểm duyệt và chuỗi kiểm kê khi vô hiệu hóa.
/// </summary>
public class InventoryAuditView : UserControl
{
    private readonly AdminServiceScope _services;
    private List<PhieuKiemKe> _allAudits = new();

    private TextBox _txtSearch = null!;
    private Button _btnCreateAudit = null!;
    private Button _btnViewDetails = null!;
    private Button _btnDeleteAudit = null!;
    private Button _btnRefresh = null!;

    private Label _lblKpiTotalAudits = null!;
    private Label _lblKpiPendingAudits = null!;
    private Label _lblKpiApprovedAudits = null!;
    private Label _lblKpiInvalidatedAudits = null!;

    private DataGridView _dgvAudits = null!;

    public InventoryAuditView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadAuditData();
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // 1. Top Header Panel
        var topHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(20, 15, 20, 10),
            BackColor = UITheme.CanvasBackground
        };

        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 640,
            Height = 85,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Biên Bản", out _lblKpiTotalAudits, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Chờ Phê Duyệt", out _lblKpiPendingAudits, Color.FromArgb(217, 119, 6))); // amber-600
        pnlKpis.Controls.Add(CreateKpiCard("Đã Phê Duyệt", out _lblKpiApprovedAudits, Color.FromArgb(22, 101, 52))); // green-800
        pnlKpis.Controls.Add(CreateKpiCard("Đã Vô Hiệu Hóa", out _lblKpiInvalidatedAudits, Color.FromArgb(153, 27, 27))); // red-800

        var pnlTitleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 15, 0)
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ KIỂM KÊ KHO HÀNG TRANG SỨC",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Lập biên bản đối chiếu tồn kho sổ sách và tồn kho thực tế, xử lý chênh lệch số lượng và kiểm soát rủi ro",
            Font = UITheme.RegularFont,
            ForeColor = UITheme.TextSecondary,
            Dock = DockStyle.Top,
            Height = 40,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        pnlTitleContainer.Controls.Add(lblSubTitle);
        pnlTitleContainer.Controls.Add(lblTitle);

        topHeaderPanel.Controls.Add(pnlTitleContainer);
        topHeaderPanel.Controls.Add(pnlKpis);
        pnlKpis.SendToBack();

        // 2. Toolbar Panel Responsive
        var toolbarPanel = UITheme.CreateResponsiveToolbar();

        var lblSearch = new Label
        {
            Text = "Tìm kiếm:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 6, 0)
        };

        _txtSearch = new TextBox
        {
            Width = 220,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "Mã phiếu, vị trí, người lập...",
            Margin = new Padding(0, 4, 15, 4)
        };
        _txtSearch.TextChanged += (s, e) => FilterAudits();

        _btnCreateAudit = UITheme.CreateFlatButton("+ Lập Phiếu Kiểm Kê", UITheme.PrimaryGold, Color.Black);
        _btnCreateAudit.Margin = new Padding(0, 2, 8, 2);
        _btnCreateAudit.Click += (s, e) => ShowCreateAuditDialog();

        _btnViewDetails = UITheme.CreateFlatButton("👁 Xem Chi Tiết", Color.FromArgb(51, 65, 85), Color.White);
        _btnViewDetails.Margin = new Padding(0, 2, 8, 2);
        _btnViewDetails.Click += (s, e) => ShowAuditDetailsDialog();

        _btnDeleteAudit = UITheme.CreateFlatButton("🗑️ Xóa Phiếu", Color.FromArgb(239, 68, 68), Color.White);
        _btnDeleteAudit.Margin = new Padding(0, 2, 8, 2);
        _btnDeleteAudit.Enabled = false; // Chỉ sáng với phiếu chưa duyệt
        _btnDeleteAudit.Click += (s, e) => DeleteSelectedAudit();

        _btnRefresh = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (s, e) => LoadAuditData();

        toolbarPanel.Controls.Add(lblSearch);
        toolbarPanel.Controls.Add(_txtSearch);
        toolbarPanel.Controls.Add(_btnCreateAudit);
        toolbarPanel.Controls.Add(_btnViewDetails);
        toolbarPanel.Controls.Add(_btnDeleteAudit);
        toolbarPanel.Controls.Add(_btnRefresh);

        // 3. Grid Container
        var pnlGridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 20),
            BackColor = UITheme.CanvasBackground
        };

        var pnlGridCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        _dgvAudits = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvAudits);
        ConfigureAuditGridColumns();
        _dgvAudits.SelectionChanged += DgvAudits_SelectionChanged;
        _dgvAudits.CellDoubleClick += (s, e) => ShowAuditDetailsDialog();

        pnlGridCard.Controls.Add(_dgvAudits);
        pnlGridContainer.Controls.Add(pnlGridCard);

        this.Controls.Add(pnlGridContainer);
        this.Controls.Add(toolbarPanel);
        this.Controls.Add(topHeaderPanel);
    }

    private Panel CreateKpiCard(string title, out Label lblValue, Color valueColor)
    {
        var card = new Panel
        {
            Width = 150,
            Height = 75,
            BackColor = Color.White,
            Margin = new Padding(4),
            Padding = new Padding(12, 8, 12, 8)
        };

        var lblT = new Label
        {
            Text = title,
            ForeColor = UITheme.TextSecondary,
            Font = new Font(FontFamily.GenericSansSerif, 8.5F, FontStyle.Regular),
            Dock = DockStyle.Top,
            Height = 20
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

    private bool IsCurrentAdmin()
    {
        var staffRole = _services.CurrentStaff?.MaVaiTro?.ToUpperInvariant();
        var userRole = _services.CurrentUser?.MaVaiTro?.ToUpperInvariant();
        if (staffRole == "ADMIN" || userRole == "ADMIN" || (staffRole == null && userRole == null))
        {
            return true;
        }
        return false;
    }

    private void ConfigureAuditGridColumns()
    {
        _dgvAudits.Columns.Clear();

        var colMa = new DataGridViewTextBoxColumn { Name = "MaPhieuKiemKe", HeaderText = "MÃ BIÊN BẢN", FillWeight = 110, MinimumWidth = 120 };
        colMa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colNgay = new DataGridViewTextBoxColumn { Name = "NgayKiemKe", HeaderText = "NGÀY KIỂM KÊ", FillWeight = 120, MinimumWidth = 130 };
        colNgay.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colNgay.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colViTri = new DataGridViewTextBoxColumn { Name = "ViTriKiemKe", HeaderText = "VỊ TRÍ / KHO HÀNG", FillWeight = 160, MinimumWidth = 150 };
        colViTri.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colViTri.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colNguoiLap = new DataGridViewTextBoxColumn { Name = "MaNhanVienKho", HeaderText = "NGƯỜI KIỂM KÊ", FillWeight = 100, MinimumWidth = 110 };
        colNguoiLap.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colNguoiLap.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colSoLuong = new DataGridViewTextBoxColumn { Name = "SoMatHang", HeaderText = "SỐ MẶT HÀNG", FillWeight = 90, MinimumWidth = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        colSoLuong.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colTrangThai = new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 115, MinimumWidth = 120 };
        colTrangThai.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colTrangThai.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var colGhiChu = new DataGridViewTextBoxColumn { Name = "GhiChu", HeaderText = "GHI CHÚ", FillWeight = 180, MinimumWidth = 160 };
        colGhiChu.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colGhiChu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colNgayDuyet = new DataGridViewTextBoxColumn { Name = "NgayDuyet", HeaderText = "NGÀY DUYỆT", FillWeight = 110, MinimumWidth = 120 };
        colNgayDuyet.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colNgayDuyet.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        _dgvAudits.Columns.AddRange(colMa, colNgay, colViTri, colNguoiLap, colSoLuong, colTrangThai, colGhiChu, colNgayDuyet);
    }

    public void LoadAuditData()
    {
        _allAudits = _services.QuanLyKiemKe.LayDanhSachPhieuKiemKe();
        UpdateKpis();
        FilterAudits();
    }

    private void UpdateKpis()
    {
        var viVn = new CultureInfo("vi-VN");
        _lblKpiTotalAudits.Text = _allAudits.Count.ToString("N0", viVn);
        _lblKpiPendingAudits.Text = _allAudits.Count(a => string.Equals(a.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase)).ToString("N0", viVn);
        _lblKpiApprovedAudits.Text = _allAudits.Count(a => string.Equals(a.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase)).ToString("N0", viVn);
        _lblKpiInvalidatedAudits.Text = _allAudits.Count(a => string.Equals(a.TrangThai, "VoHieuHoa", StringComparison.OrdinalIgnoreCase)).ToString("N0", viVn);
    }

    private void FilterAudits()
    {
        var query = _txtSearch.Text.Trim().ToLowerInvariant();

        var filtered = _allAudits.Where(a =>
            string.IsNullOrEmpty(query) ||
            a.MaPhieuKiemKe.ToLowerInvariant().Contains(query) ||
            (a.ViTriKiemKe ?? string.Empty).ToLowerInvariant().Contains(query) ||
            a.MaNhanVienKho.ToLowerInvariant().Contains(query) ||
            (a.GhiChu ?? string.Empty).ToLowerInvariant().Contains(query)
        ).ToList();

        _dgvAudits.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");

        foreach (var a in filtered)
        {
            var countItems = a.ChiTietKiemKes?.Count ?? 0;
            string statusDisplay;
            Color statusFg;
            Color statusBg;

            if (string.Equals(a.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase))
            {
                statusDisplay = "✓ Đã duyệt";
                statusFg = Color.FromArgb(22, 101, 52);
                statusBg = Color.FromArgb(220, 252, 231);
            }
            else if (string.Equals(a.TrangThai, "VoHieuHoa", StringComparison.OrdinalIgnoreCase))
            {
                statusDisplay = "🚫 Đã vô hiệu hóa";
                statusFg = Color.FromArgb(153, 27, 27);
                statusBg = Color.FromArgb(254, 226, 226);
            }
            else
            {
                statusDisplay = "⏳ Chờ duyệt";
                statusFg = Color.FromArgb(146, 64, 14);
                statusBg = Color.FromArgb(254, 243, 199);
            }

            var rowIndex = _dgvAudits.Rows.Add(
                a.MaPhieuKiemKe,
                a.NgayKiemKe.ToString("dd/MM/yyyy HH:mm"),
                a.ViTriKiemKe,
                a.MaNhanVienKho,
                countItems.ToString("N0", viVn),
                statusDisplay,
                string.IsNullOrEmpty(a.GhiChu) ? "-" : a.GhiChu,
                a.NgayDuyet.HasValue ? a.NgayDuyet.Value.ToString("dd/MM/yyyy HH:mm") : "-"
            );

            var cellTrangThai = _dgvAudits.Rows[rowIndex].Cells["TrangThai"];
            cellTrangThai.Style.ForeColor = statusFg;
            cellTrangThai.Style.BackColor = statusBg;
            cellTrangThai.Style.Font = new Font(UITheme.RegularFont, FontStyle.Bold);
        }

        UpdateDeleteButtonState();
    }

    private void DgvAudits_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateDeleteButtonState();
    }

    private void UpdateDeleteButtonState()
    {
        if (_dgvAudits.SelectedRows.Count == 0)
        {
            _btnDeleteAudit.Enabled = false;
            return;
        }

        var maPhieu = _dgvAudits.SelectedRows[0].Cells["MaPhieuKiemKe"].Value?.ToString();
        var phieu = _allAudits.FirstOrDefault(a => a.MaPhieuKiemKe == maPhieu);
        if (phieu != null)
        {
            // Nút xóa chỉ sáng đối với các phiếu chưa được duyệt (ChoDuyet)
            _btnDeleteAudit.Enabled = string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            _btnDeleteAudit.Enabled = false;
        }
    }

    private void DeleteSelectedAudit()
    {
        if (_dgvAudits.SelectedRows.Count == 0) return;

        var maPhieu = _dgvAudits.SelectedRows[0].Cells["MaPhieuKiemKe"].Value?.ToString();
        var phieu = _allAudits.FirstOrDefault(a => a.MaPhieuKiemKe == maPhieu);
        if (phieu == null) return;

        if (!string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Chỉ được phép xóa phiếu kiểm kê ở trạng thái Chờ duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa phiếu kiểm kê [{phieu.MaPhieuKiemKe}] không?\nThao tác này sẽ xóa toàn bộ danh mục đối chiếu trong biên bản.",
            "Xác nhận xóa phiếu kiểm kê",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm != DialogResult.Yes) return;

        var success = _services.QuanLyKiemKe.XoaPhieuKiemKe(phieu.MaPhieuKiemKe, out var error);
        if (success)
        {
            MessageBox.Show($"Đã xóa thành công phiếu kiểm kê [{phieu.MaPhieuKiemKe}].", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadAuditData();
        }
        else
        {
            MessageBox.Show($"Không thể xóa phiếu kiểm kê:\n{error}", "Lỗi xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // =========================================================================
    // MODAL LẬP BIÊN BẢN KIỂM KÊ KHO HÀNG
    // =========================================================================
    private void ShowCreateAuditDialog()
    {
        var allProducts = _services.SanPhamRepo.LayTatCa();
        if (allProducts.Count == 0)
        {
            MessageBox.Show("Hiện chưa có sản phẩm nào trong hệ thống để thực hiện kiểm kê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new Form
        {
            Text = "Lập Biên Bản Kiểm Kê Kho Hàng - Blink Blink Jewelry",
            Width = 1040,
            Height = 720,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = UITheme.RegularFont
        };

        // Header
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = UITheme.DarkHeader, Padding = new Padding(24, 14, 24, 10) };
        var lblTitle = new Label { Text = "LẬP BIÊN BẢN KIỂM KÊ KHO HÀNG", Font = UITheme.HeaderFont, ForeColor = UITheme.PrimaryGold, Dock = DockStyle.Top, Height = 24 };
        var lblSub = new Label { Text = "Đối chiếu số lượng tồn sổ sách và số lượng tồn thực tế. Phiếu tạo mới sẽ ở trạng thái 'Chờ duyệt'", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(203, 213, 225), Dock = DockStyle.Fill };
        pnlHead.Controls.Add(lblSub);
        pnlHead.Controls.Add(lblTitle);

        // Footer buttons
        var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 65, BackColor = UITheme.CanvasBackground, Padding = new Padding(24, 14, 24, 14) };
        var flpFoot = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };

        var btnCancel = UITheme.CreateFlatButton("Hủy", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        btnCancel.Width = 110;
        btnCancel.Height = 36;
        btnCancel.Margin = new Padding(12, 0, 0, 0);
        btnCancel.Click += (s, e) => dialog.Close();

        var btnSubmit = UITheme.CreateFlatButton("Thêm biên bản kiểm kê", UITheme.PrimaryGold, Color.Black);
        btnSubmit.Width = 200;
        btnSubmit.Height = 36;
        btnSubmit.Margin = new Padding(0);

        flpFoot.Controls.Add(btnCancel);
        flpFoot.Controls.Add(btnSubmit);
        pnlFoot.Controls.Add(flpFoot);

        // Body Container
        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 12, 24, 10) };

        // Top info controls: Mã phiếu, Vị trí, Người kiểm kê
        var pnlTopInfo = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 68, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        var lblMaP = new Label { Text = "Mã Biên Bản:", AutoSize = true, ForeColor = UITheme.TextSecondary, Margin = new Padding(0, 6, 8, 0) };
        var txtMaP = new TextBox
        {
            Text = $"PKK{DateTime.Now:yyyyMMdd}_{_allAudits.Count + 1:D2}",
            Width = 160,
            ReadOnly = true,
            BackColor = Color.FromArgb(241, 245, 249),
            Margin = new Padding(0, 2, 25, 0)
        };

        var lblViTri = new Label { Text = "Vị Trí Kiểm Kê (*):", AutoSize = true, ForeColor = UITheme.TextSecondary, Margin = new Padding(0, 6, 8, 0) };
        var txtViTri = new TextBox { Text = "Kho Tổng Blink Blink", Width = 200, Margin = new Padding(0, 2, 25, 0) };

        var currentStaffId = _services.CurrentStaff?.MaNhanVien ?? _services.CurrentUser?.MaNguoiDung ?? "NV_KHO";
        var lblNguoiLap = new Label { Text = "Thủ Kho Lập:", AutoSize = true, ForeColor = UITheme.TextSecondary, Margin = new Padding(0, 6, 8, 0) };
        var txtNguoiLap = new TextBox { Text = currentStaffId, Width = 150, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), Margin = new Padding(0, 2, 0, 0) };

        pnlTopInfo.Controls.AddRange(new Control[] { lblMaP, txtMaP, lblViTri, txtViTri, lblNguoiLap, txtNguoiLap });

        // Search and Select Product Panel
        var pnlSelectSp = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(14, 10, 14, 10) };

        var lblSearchSp = new Label { Text = "Tìm sản phẩm:", Location = new Point(12, 12), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var txtSearchSp = new TextBox { Location = new Point(110, 9), Width = 175, PlaceholderText = "Mã hoặc tên..." };

        var lblCboSp = new Label { Text = "Sản phẩm:", Location = new Point(305, 12), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var cboSp = new ComboBox { Location = new Point(375, 9), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };

        // Textbox chỉ đọc tồn sổ sách và textbox nhập tồn thực tế
        var lblTonSoSach = new Label { Text = "Tồn sổ sách:", Location = new Point(12, 54), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var txtTonSoSach = new TextBox { Location = new Point(110, 50), Width = 85, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), Font = UITheme.TabularNumericFont };

        var lblTonThucTe = new Label { Text = "Tồn thực tế (*):", Location = new Point(210, 54), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var txtTonThucTe = new TextBox { Location = new Point(310, 50), Width = 85, Font = UITheme.TabularNumericFont };

        var lblLyDo = new Label { Text = "Lý do lệch:", Location = new Point(415, 54), AutoSize = true, ForeColor = UITheme.TextSecondary };
        var txtLyDo = new TextBox { Location = new Point(485, 50), Width = 220, PlaceholderText = "Thất thoát, thừa hàng..." };

        var btnAddToList = UITheme.CreateFlatButton("+ Thêm vào danh sách", UITheme.PrimaryGold, Color.Black);
        btnAddToList.Location = new Point(725, 47);
        btnAddToList.Size = new Size(220, 34);

        pnlSelectSp.Controls.AddRange(new Control[] { lblSearchSp, txtSearchSp, lblCboSp, cboSp, lblTonSoSach, txtTonSoSach, lblTonThucTe, txtTonThucTe, lblLyDo, txtLyDo, btnAddToList });

        // Helper populate combobox
        void PopulateCombobox(string filter)
        {
            cboSp.Items.Clear();
            var q = filter.Trim().ToLowerInvariant();
            var matches = allProducts.Where(p =>
                string.IsNullOrEmpty(q) ||
                p.MaSanPham.ToLowerInvariant().Contains(q) ||
                p.TenSanPham.ToLowerInvariant().Contains(q)
            ).ToList();

            foreach (var sp in matches)
            {
                var sizeText = string.IsNullOrEmpty(sp.Size) ? "" : $" ({sp.Size})";
                cboSp.Items.Add(new ProductComboItem(sp, $"[{sp.MaSanPham}] {sp.TenSanPham}{sizeText}"));
            }

            if (cboSp.Items.Count > 0)
            {
                cboSp.SelectedIndex = 0;
            }
            else
            {
                txtTonSoSach.Text = "0";
                txtTonThucTe.Text = "0";
            }
        }

        PopulateCombobox(string.Empty);

        cboSp.SelectedIndexChanged += (s, e) =>
        {
            if (cboSp.SelectedItem is ProductComboItem selected)
            {
                txtTonSoSach.Text = selected.Product.SoLuongTon.ToString();
                txtTonThucTe.Text = selected.Product.SoLuongTon.ToString();
            }
        };

        // Khi gõ ô search: lọc combobox, sản phẩm tìm thấy đầu tiên hiện trong combobox
        txtSearchSp.TextChanged += (s, e) =>
        {
            PopulateCombobox(txtSearchSp.Text);
        };

        // DataGridView danh sách mặt hàng kiểm kê
        var pnlGridHolder = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 10) };
        var dgvItems = new DataGridView();
        UITheme.ApplyGridViewStyle(dgvItems);

        var colItmMa = new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 110, MinimumWidth = 110 };
        var colItmTen = new DataGridViewTextBoxColumn { Name = "TenSanPham", HeaderText = "TÊN TRANG SỨC", FillWeight = 220, MinimumWidth = 200 };
        var colItmSize = new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "SIZE", FillWeight = 80, MinimumWidth = 80 };
        var colItmTonSS = new DataGridViewTextBoxColumn { Name = "TonSoSach", HeaderText = "TỒN SỔ SÁCH", FillWeight = 100, MinimumWidth = 105, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colItmTonTT = new DataGridViewTextBoxColumn { Name = "TonThucTe", HeaderText = "TỒN THỰC TẾ", FillWeight = 100, MinimumWidth = 105, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colItmChenhLech = new DataGridViewTextBoxColumn { Name = "ChenhLech", HeaderText = "CHÊNH LỆCH", FillWeight = 100, MinimumWidth = 105, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colItmLyDo = new DataGridViewTextBoxColumn { Name = "LyDoChenhLech", HeaderText = "LÝ DO CHÊNH LỆCH", FillWeight = 180, MinimumWidth = 150 };

        dgvItems.Columns.AddRange(colItmMa, colItmTen, colItmSize, colItmTonSS, colItmTonTT, colItmChenhLech, colItmLyDo);
        pnlGridHolder.Controls.Add(dgvItems);

        var auditItemsList = new List<ChiTietKiemKeItemDTO>();

        void RefreshGridItems()
        {
            dgvItems.Rows.Clear();
            foreach (var item in auditItemsList)
            {
                var rIdx = dgvItems.Rows.Add(
                    item.MaSanPham,
                    item.TenSanPham,
                    item.Size,
                    item.TonSoSach,
                    item.TonThucTe,
                    item.ChenhLech > 0 ? $"+{item.ChenhLech}" : item.ChenhLech.ToString(),
                    item.LyDoChenhLech
                );

                var clr = dgvItems.Rows[rIdx].Cells["ChenhLech"];
                if (item.ChenhLech > 0) clr.Style.ForeColor = Color.FromArgb(22, 101, 52); // Thừa hàng: xanh
                else if (item.ChenhLech < 0) clr.Style.ForeColor = Color.FromArgb(153, 27, 27); // Thiếu/thất thoát: đỏ
                else clr.Style.ForeColor = UITheme.TextSecondary;
            }
        }

        // Nút thêm sản phẩm vào danh sách kiểm kê
        btnAddToList.Click += (s, e) =>
        {
            if (cboSp.SelectedItem is not ProductComboItem selected)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần kiểm kê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtTonThucTe.Text.Trim(), out var tonThucTe) || tonThucTe < 0)
            {
                MessageBox.Show("Số lượng tồn kho thực tế phải là số nguyên không âm.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTonThucTe.Focus();
                return;
            }

            var sp = selected.Product;
            var existing = auditItemsList.FirstOrDefault(x => x.MaSanPham == sp.MaSanPham);
            if (existing != null)
            {
                existing.TonSoSach = sp.SoLuongTon;
                existing.TonThucTe = tonThucTe;
                existing.LyDoChenhLech = txtLyDo.Text.Trim();
            }
            else
            {
                auditItemsList.Add(new ChiTietKiemKeItemDTO
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    Size = sp.Size ?? string.Empty,
                    TonSoSach = sp.SoLuongTon,
                    TonThucTe = tonThucTe,
                    LyDoChenhLech = txtLyDo.Text.Trim()
                });
            }

            RefreshGridItems();

            // Yêu cầu: làm trống search (mặc định combobox khi ô search trống sẽ hiển thị tất cả sản phẩm)
            txtSearchSp.Text = string.Empty;
            txtLyDo.Text = string.Empty;
        };

        // Panel dưới bảng: Nút loại khỏi danh sách và Ghi chú
        var pnlBottomActions = new Panel { Dock = DockStyle.Bottom, Height = 95, Padding = new Padding(0, 5, 0, 0) };

        var btnRemoveItem = UITheme.CreateFlatButton("➖ Loại khỏi danh sách", Color.FromArgb(254, 226, 226), Color.FromArgb(153, 27, 27));
        btnRemoveItem.Dock = DockStyle.Top;
        btnRemoveItem.Height = 30;
        btnRemoveItem.Click += (s, e) =>
        {
            if (dgvItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng trong bảng để loại bỏ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var maSp = dgvItems.SelectedRows[0].Cells["MaSanPham"].Value?.ToString();
            auditItemsList.RemoveAll(x => x.MaSanPham == maSp);
            RefreshGridItems();
        };

        var pnlGhiChu = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(0, 6, 0, 0) };
        var lblGhiChu = new Label { Text = "Ghi chú kiểm kê:", Dock = DockStyle.Left, Width = 135, ForeColor = UITheme.TextSecondary };
        var txtGhiChu = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 48, ScrollBars = ScrollBars.Vertical };
        pnlGhiChu.Controls.Add(txtGhiChu);
        pnlGhiChu.Controls.Add(lblGhiChu);

        pnlBottomActions.Controls.Add(btnRemoveItem);
        pnlBottomActions.Controls.Add(pnlGhiChu);

        pnlBody.Controls.Add(pnlGridHolder);
        pnlBody.Controls.Add(pnlBottomActions);
        pnlBody.Controls.Add(pnlSelectSp);
        pnlBody.Controls.Add(pnlTopInfo);

        dialog.Controls.Add(pnlBody);
        dialog.Controls.Add(pnlFoot);
        dialog.Controls.Add(pnlHead);

        // Submit Lập biên bản kiểm kê
        btnSubmit.Click += (s, e) =>
        {
            if (auditItemsList.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một mặt hàng trang sức vào biên bản kiểm kê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new TaoPhieuKiemKeDTO
            {
                MaPhieuKiemKe = txtMaP.Text.Trim(),
                ViTriKiemKe = txtViTri.Text.Trim(),
                MaNhanVienKho = txtNguoiLap.Text.Trim(),
                GhiChu = txtGhiChu.Text.Trim(),
                DanhSachChiTiet = auditItemsList
            };

            var success = _services.QuanLyKiemKe.LapPhieuKiemKe(dto, out var error);
            if (success)
            {
                MessageBox.Show($"Đã tạo thành công biên bản kiểm kê [{dto.MaPhieuKiemKe}] ở trạng thái 'Chờ duyệt'.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.Close();
                LoadAuditData();
            }
            else
            {
                MessageBox.Show($"Không thể tạo biên bản kiểm kê:\n{error}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        dialog.ShowDialog(this);
    }

    // =========================================================================
    // MODAL XEM CHI TIẾT & PHÊ DUYỆT / VÔ HIỆU HÓA BIÊN BẢN KIỂM KÊ
    // =========================================================================
    private void ShowAuditDetailsDialog()
    {
        if (_dgvAudits.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một biên bản kiểm kê để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maPhieu = _dgvAudits.SelectedRows[0].Cells["MaPhieuKiemKe"].Value?.ToString();
        var phieu = _services.QuanLyKiemKe.LayChiTiet(maPhieu ?? string.Empty);
        if (phieu == null) return;

        var isPending = string.Equals(phieu.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase);
        var isApproved = string.Equals(phieu.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase);
        var isInvalidated = string.Equals(phieu.TrangThai, "VoHieuHoa", StringComparison.OrdinalIgnoreCase);
        var isAdmin = IsCurrentAdmin();

        using var dialog = new Form
        {
            Text = $"Chi Tiết Biên Bản Kiểm Kê #{phieu.MaPhieuKiemKe} - Blink Blink Jewelry",
            Width = 980,
            Height = 720,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = UITheme.RegularFont
        };

        // Header
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 95, BackColor = UITheme.DarkHeader, Padding = new Padding(24, 14, 24, 10) };
        var lblTitle = new Label { Text = $"BIÊN BẢN KIỂM KÊ #{phieu.MaPhieuKiemKe}", Font = UITheme.HeaderFont, ForeColor = UITheme.PrimaryGold, Dock = DockStyle.Top, Height = 24 };

        string statusText = isApproved ? "ĐÃ PHÊ DUYỆT (TỒN KHO ĐÃ CÂN BẰNG)" : (isInvalidated ? "ĐÃ VÔ HIỆU HÓA" : "CHỜ PHÊ DUYỆT (TỒN KHO CHƯA THAY ĐỔI)");
        var lblSub = new Label
        {
            Text = $"Trạng thái: {statusText} | Ngày lập: {phieu.NgayKiemKe:dd/MM/yyyy HH:mm} | Thủ kho: {phieu.MaNhanVienKho} | Vị trí: {phieu.ViTriKiemKe}" +
                   (phieu.NgayDuyet.HasValue ? $" | Duyệt ngày: {phieu.NgayDuyet:dd/MM/yyyy HH:mm}" : ""),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Fill
        };
        pnlHead.Controls.Add(lblSub);
        pnlHead.Controls.Add(lblTitle);

        // Footer buttons container
        var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 65, BackColor = UITheme.CanvasBackground, Padding = new Padding(24, 14, 24, 14) };
        var flpFoot = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };

        var btnClose = UITheme.CreateFlatButton("Đóng", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        btnClose.Width = 110;
        btnClose.Height = 36;
        btnClose.Margin = new Padding(12, 0, 0, 0);
        btnClose.Click += (s, e) => dialog.Close();
        flpFoot.Controls.Add(btnClose);

        Button? btnApprove = null;
        Button? btnInvalidate = null;

        // Phân quyền hiển thị các nút hành động (Theo yêu cầu mới: Bỏ tính năng chỉnh sửa phiếu kiểm kê chưa duyệt, nếu sai thì xóa tạo lại)
        if (isPending)
        {
            // Với Admin: có nút duyệt phiếu
            if (isAdmin)
            {
                btnApprove = UITheme.CreateFlatButton("✓ Duyệt Phiếu", Color.FromArgb(22, 101, 52), Color.White);
                btnApprove.Width = 150;
                btnApprove.Height = 36;
                btnApprove.Margin = new Padding(0);
                flpFoot.Controls.Add(btnApprove);
            }
        }
        else if (isApproved && isAdmin)
        {
            // Khi đã duyệt: chỉ có admin có thêm nút Vô hiệu hóa
            btnInvalidate = UITheme.CreateFlatButton("🚫 Vô Hiệu Hóa", Color.FromArgb(239, 68, 68), Color.White);
            btnInvalidate.Width = 160;
            btnInvalidate.Height = 36;
            btnInvalidate.Margin = new Padding(0);
            flpFoot.Controls.Add(btnInvalidate);
        }

        pnlFoot.Controls.Add(flpFoot);

        // Body Content
        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 12, 24, 10) };

        // Grid chi tiết (toàn bộ cột chỉ đọc - không cho phép sửa)
        var pnlGridHolder = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 10) };
        var dgv = new DataGridView();
        UITheme.ApplyGridViewStyle(dgv);

        var colMa = new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 110, MinimumWidth = 110, ReadOnly = true };
        var colTen = new DataGridViewTextBoxColumn { Name = "TenSanPham", HeaderText = "TÊN TRANG SỨC", FillWeight = 220, MinimumWidth = 200, ReadOnly = true };
        var colSize = new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "SIZE", FillWeight = 80, MinimumWidth = 80, ReadOnly = true };
        var colTonSS = new DataGridViewTextBoxColumn { Name = "TonSoSach", HeaderText = "TỒN SỔ SÁCH", FillWeight = 100, MinimumWidth = 105, ReadOnly = true, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colTonTT = new DataGridViewTextBoxColumn { Name = "TonThucTe", HeaderText = "TỒN THỰC TẾ", FillWeight = 100, MinimumWidth = 105, ReadOnly = true, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colChenhLech = new DataGridViewTextBoxColumn { Name = "ChenhLech", HeaderText = "CHÊNH LỆCH", FillWeight = 100, MinimumWidth = 105, ReadOnly = true, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colLyDo = new DataGridViewTextBoxColumn { Name = "LyDoChenhLech", HeaderText = "LÝ DO CHÊNH LỆCH", FillWeight = 180, MinimumWidth = 150, ReadOnly = true };

        dgv.Columns.AddRange(colMa, colTen, colSize, colTonSS, colTonTT, colChenhLech, colLyDo);

        var currentDetails = phieu.ChiTietKiemKes ?? new List<ChiTietKiemKe>();

        void LoadDetailsToGrid()
        {
            dgv.Rows.Clear();
            foreach (var ct in currentDetails)
            {
                var tenSp = ct.SanPhamTrangSuc?.TenSanPham ?? ct.MaSanPham;
                var size = ct.SanPhamTrangSuc?.Size ?? string.Empty;
                var delta = ct.TonThucTe - ct.TonSoSach;

                var rIdx = dgv.Rows.Add(
                    ct.MaSanPham,
                    tenSp,
                    size,
                    ct.TonSoSach,
                    ct.TonThucTe,
                    delta > 0 ? $"+{delta}" : delta.ToString(),
                    ct.LyDoChenhLech
                );

                var clr = dgv.Rows[rIdx].Cells["ChenhLech"];
                if (delta > 0) clr.Style.ForeColor = Color.FromArgb(22, 101, 52);
                else if (delta < 0) clr.Style.ForeColor = Color.FromArgb(153, 27, 27);
                else clr.Style.ForeColor = UITheme.TextSecondary;
            }
        }

        LoadDetailsToGrid();

        pnlGridHolder.Controls.Add(dgv);

        // Panel Ghi chú phía dưới
        var pnlGhiChu = new Panel { Dock = DockStyle.Bottom, Height = 75, Padding = new Padding(0, 8, 0, 0) };
        var lblGhiChu = new Label { Text = "Ghi chú kiểm kê:", Dock = DockStyle.Left, Width = 115, ForeColor = UITheme.TextSecondary };
        var txtGhiChu = new TextBox
        {
            Text = phieu.GhiChu,
            Dock = DockStyle.Fill,
            Multiline = true,
            Height = 60,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };
        pnlGhiChu.Controls.Add(txtGhiChu);
        pnlGhiChu.Controls.Add(lblGhiChu);

        pnlBody.Controls.Add(pnlGridHolder);
        pnlBody.Controls.Add(pnlGhiChu);

        dialog.Controls.Add(pnlBody);
        dialog.Controls.Add(pnlFoot);
        dialog.Controls.Add(pnlHead);

        // 1. Xử lý nút DUYỆT PHIẾU (dành cho Admin)
        if (btnApprove != null)
        {
            btnApprove.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
                    $"Bạn có chắc chắn muốn phê duyệt biên bản kiểm kê [{phieu.MaPhieuKiemKe}] không?\n\n" +
                    "Hệ thống sẽ:\n" +
                    "1. Kiểm tra tính toàn vẹn tồn kho hiện tại so với tồn sổ sách lúc lập biên bản.\n" +
                    "2. Tự động cân bằng số lượng tồn kho theo số lượng thực tế kiểm đếm.\n" +
                    "3. Chuyển trạng thái phiếu sang 'Đã duyệt' và khóa chỉnh sửa.",
                    "Xác nhận phê duyệt kiểm kê",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm != DialogResult.Yes) return;

                var adminId = _services.CurrentStaff?.MaNhanVien ?? _services.CurrentUser?.MaNguoiDung ?? "ADMIN";
                var success = _services.QuanLyKiemKe.DuyetPhieuKiemKe(phieu.MaPhieuKiemKe, adminId, out var error);

                if (success)
                {
                    MessageBox.Show(
                        $"Phê duyệt biên bản kiểm kê [{phieu.MaPhieuKiemKe}] thành công!\nTồn kho thực tế của các sản phẩm đã được cập nhật chính xác.",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    dialog.Close();
                    LoadAuditData();
                }
                else
                {
                    // Từ chối phê duyệt do xung đột tồn kho sổ sách
                    MessageBox.Show(
                        $"TỪ CHỐI PHÊ DUYỆT:\n\n{error}",
                        "Ràng buộc chống xung đột tồn kho",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
        }

        // 3. Xử lý nút VÔ HIỆU HÓA (dành cho Admin khi đã duyệt nhầm)
        if (btnInvalidate != null)
        {
            btnInvalidate.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
                    $"Bạn có chắc chắn muốn vô hiệu hóa biên bản kiểm kê [{phieu.MaPhieuKiemKe}] không?\n\n" +
                    "Hệ thống sẽ:\n" +
                    "1. Kiểm tra chuỗi kiểm kê: Chỉ cho phép vô hiệu hóa nếu đây là đợt kiểm kê mới nhất đối với từng sản phẩm.\n" +
                    "2. Hoàn tác số lượng tồn kho theo đúng chênh lệch đã kiểm đếm (bảo toàn các đơn hàng bán phát sinh sau khi duyệt).\n" +
                    "3. Đổi trạng thái biên bản sang 'Đã vô hiệu hóa'.",
                    "Xác nhận vô hiệu hóa biên bản kiểm kê",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (confirm != DialogResult.Yes) return;

                var adminId = _services.CurrentStaff?.MaNhanVien ?? _services.CurrentUser?.MaNguoiDung ?? "ADMIN";
                var success = _services.QuanLyKiemKe.VoHieuHoaPhieuKiemKe(phieu.MaPhieuKiemKe, adminId, out var error);

                if (success)
                {
                    MessageBox.Show(
                        $"Vô hiệu hóa biên bản kiểm kê [{phieu.MaPhieuKiemKe}] thành công!\nSố lượng tồn kho đã được hoàn tác chính xác theo chênh lệch ban đầu.",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    dialog.Close();
                    LoadAuditData();
                }
                else
                {
                    // Từ chối vô hiệu hóa do đã có phiếu kiểm kê mới hơn
                    MessageBox.Show(
                        $"TỪ CHỐI VÔ HIỆU HÓA:\n\n{error}",
                        "Ràng buộc chuỗi kiểm kê",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
        }

        dialog.ShowDialog(this);
    }

    private class ProductComboItem
    {
        public SanPhamTrangSuc Product { get; }
        public string DisplayText { get; }

        public ProductComboItem(SanPhamTrangSuc product, string displayText)
        {
            Product = product;
            DisplayText = displayText;
        }

        public override string ToString() => DisplayText;
    }
}
