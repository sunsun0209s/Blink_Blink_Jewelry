using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Admin.Views.UserControls.Catalog;

/// <summary>
/// Màn hình Quản lý Danh mục Trang sức Blink Blink Jewelry.
/// Hỗ trợ xem danh sách, thống kê số lượng mẫu sản phẩm theo danh mục, thêm mới và hiệu chỉnh danh mục.
/// </summary>
public class CategoryView : UserControl
{
    private readonly AdminServiceScope _services;
    private List<DanhMuc> _allCategories = new();
    private List<SanPhamTrangSuc> _allProducts = new();

    private TextBox _txtSearch = null!;
    private Button _btnAddCategory = null!;
    private Button _btnEditCategory = null!;
    private Button _btnRefresh = null!;

    private Label _lblKpiTotalCats = null!;
    private Label _lblKpiTotalItems = null!;

    private DataGridView _dgvCategories = null!;

    public CategoryView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadCategoryData();
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // Top Header
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
            Width = 400,
            Height = 85,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Danh Mục", out _lblKpiTotalCats, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Mẫu SP Liên Kết", out _lblKpiTotalItems, UITheme.PrimaryGold));

        var pnlTitleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 15, 0)
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ DANH MỤC TRANG SỨC",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Phân loại các dòng sản phẩm kim hoàn: Nhẫn kim cương, Dây chuyền, Bông tai, Lắc tay cao cấp",
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

        // Toolbar Panel Responsive chống tràn chữ
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
            Width = 200,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "Tên hoặc mã danh mục...",
            Margin = new Padding(0, 4, 15, 4)
        };
        _txtSearch.TextChanged += (s, e) => FilterCategories();

        _btnAddCategory = UITheme.CreateFlatButton("+ Thêm Danh Mục Mới", UITheme.DarkHeader, Color.White);
        _btnAddCategory.Margin = new Padding(0, 2, 8, 2);
        _btnAddCategory.Click += (s, e) => ShowCategoryModal(isEdit: false);

        _btnEditCategory = UITheme.CreateFlatButton("✏ Chỉnh Sửa Danh Mục", UITheme.PrimaryGold, Color.Black);
        _btnEditCategory.Margin = new Padding(0, 2, 8, 2);
        _btnEditCategory.Click += (s, e) => EditSelectedCategory();

        _btnRefresh = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (s, e) => LoadCategoryData();

        toolbarPanel.Controls.Add(lblSearch);
        toolbarPanel.Controls.Add(_txtSearch);
        toolbarPanel.Controls.Add(_btnAddCategory);
        toolbarPanel.Controls.Add(_btnEditCategory);
        toolbarPanel.Controls.Add(_btnRefresh);

        // Grid Container
        var pnlGridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 20),
            BackColor = UITheme.CanvasBackground
        };

        var pnlWhiteCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        _dgvCategories = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvCategories);
        ConfigureGridColumns();
        _dgvCategories.CellDoubleClick += (s, e) => EditSelectedCategory();

        pnlWhiteCard.Controls.Add(_dgvCategories);
        pnlGridContainer.Controls.Add(pnlWhiteCard);

        this.Controls.Add(pnlGridContainer);
        this.Controls.Add(toolbarPanel);
        this.Controls.Add(topHeaderPanel);
    }

    private Panel CreateKpiCard(string title, out Label lblValue, Color valueColor)
    {
        var card = new Panel
        {
            Width = 180,
            Height = 75,
            BackColor = Color.White,
            Margin = new Padding(5),
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

    private void ConfigureGridColumns()
    {
        _dgvCategories.Columns.Clear();
        var colMa = new DataGridViewTextBoxColumn { Name = "MaDanhMuc", HeaderText = "MÃ DANH MỤC", FillWeight = 90, MinimumWidth = 110 };
        colMa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colTen = new DataGridViewTextBoxColumn { Name = "TenDanhMuc", HeaderText = "TÊN DANH MỤC TRANG SỨC", FillWeight = 200, MinimumWidth = 220 };
        colTen.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colTen.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colMoTa = new DataGridViewTextBoxColumn { Name = "MoTa", HeaderText = "MÔ TẢ CHI TIẾT", FillWeight = 320, MinimumWidth = 300 };
        colMoTa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMoTa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colSl = new DataGridViewTextBoxColumn { Name = "SoLuongSP", HeaderText = "SỐ MẪU TRANG SỨC", FillWeight = 110, MinimumWidth = 130, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } };
        colSl.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _dgvCategories.Columns.AddRange(colMa, colTen, colMoTa, colSl);
    }

    public void LoadCategoryData()
    {
        _allCategories = _services.DanhMucRepo.LayTatCa();
        _allProducts = _services.ThongKeRepo.TruyVanSanPhamTonKho();

        UpdateKpis();
        FilterCategories();
    }

    private void UpdateKpis()
    {
        var viVn = new CultureInfo("vi-VN");
        _lblKpiTotalCats.Text = _allCategories.Count.ToString("N0", viVn);
        _lblKpiTotalItems.Text = _allProducts.Count.ToString("N0", viVn);
    }

    private void FilterCategories()
    {
        var query = _txtSearch.Text.Trim().ToLowerInvariant();

        var filtered = _allCategories.Where(c =>
            string.IsNullOrEmpty(query) ||
            c.MaDanhMuc.ToLowerInvariant().Contains(query) ||
            c.TenDanhMuc.ToLowerInvariant().Contains(query) ||
            c.MoTa.ToLowerInvariant().Contains(query)
        ).ToList();

        _dgvCategories.Rows.Clear();
        foreach (var c in filtered)
        {
            var countSp = _allProducts.Count(p => string.Equals(p.MaDanhMuc, c.MaDanhMuc, StringComparison.OrdinalIgnoreCase));
            _dgvCategories.Rows.Add(
                c.MaDanhMuc,
                c.TenDanhMuc,
                string.IsNullOrEmpty(c.MoTa) ? "(Chưa có mô tả)" : c.MoTa,
                $"{countSp} mẫu"
            );
        }

        if (_dgvCategories.Rows.Count > 0)
        {
            _dgvCategories.Rows[0].Selected = true;
        }
    }

    private void EditSelectedCategory()
    {
        if (_dgvCategories.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn danh mục cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maDM = _dgvCategories.SelectedRows[0].Cells["MaDanhMuc"].Value?.ToString();
        var targetCat = _allCategories.FirstOrDefault(c => c.MaDanhMuc == maDM);
        if (targetCat == null) return;

        ShowCategoryModal(isEdit: true, targetCat);
    }

    private void ShowCategoryModal(bool isEdit, DanhMuc? target = null)
    {
        using var dialog = new Form
        {
            Text = isEdit ? "Chỉnh Sửa Danh Mục - Blink Blink Jewelry" : "Thêm Danh Mục Trang Sức - Blink Blink Jewelry",
            Width = 680,
            Height = 520,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = UITheme.RegularFont
        };

        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = UITheme.DarkHeader, Padding = new Padding(24, 14, 24, 10) };
        var lblTitle = new Label
        {
            Text = isEdit ? "CHỈNH SỬA THÔNG TIN DANH MỤC" : "THÊM MỚI DANH MỤC TRANG SỨC",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.PrimaryGold,
            Dock = DockStyle.Top,
            Height = 24
        };
        var lblSub = new Label
        {
            Text = "Danh mục giúp phân nhóm các dòng trang sức cao cấp tại quầy và website",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Fill
        };
        pnlHead.Controls.Add(lblSub);
        pnlHead.Controls.Add(lblTitle);

        var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = UITheme.CanvasBackground, Padding = new Padding(24, 16, 24, 16) };
        var flpBtns = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };

        var btnCancel = UITheme.CreateFlatButton("Hủy Bỏ", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        btnCancel.Width = 115;
        btnCancel.Height = 38;
        btnCancel.Margin = new Padding(12, 0, 0, 0);
        btnCancel.Click += (s, e) => dialog.Close();

        var btnSave = UITheme.CreateFlatButton("Lưu Danh Mục", UITheme.PrimaryGold, Color.Black);
        btnSave.Width = 150;
        btnSave.Height = 38;
        btnSave.Margin = new Padding(0);

        flpBtns.Controls.Add(btnCancel);
        flpBtns.Controls.Add(btnSave);
        pnlFoot.Controls.Add(flpBtns);

        var pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 24, 30, 16) };

        var lblMa = new Label { Text = "Mã Danh Mục (*):", Location = new Point(24, 24), Width = 150, ForeColor = UITheme.TextSecondary };
        var txtMa = new TextBox
        {
            Text = isEdit ? target!.MaDanhMuc : $"DM{_allCategories.Count + 1:D2}",
            Location = new Point(180, 20),
            Width = 430,
            Height = 30,
            ReadOnly = isEdit,
            BackColor = isEdit ? Color.FromArgb(241, 245, 249) : Color.White
        };

        var lblTen = new Label { Text = "Tên Danh Mục (*):", Location = new Point(24, 76), Width = 150, ForeColor = UITheme.TextSecondary };
        var txtTen = new TextBox
        {
            Text = isEdit ? target!.TenDanhMuc : "",
            Location = new Point(180, 72),
            Width = 430,
            Height = 30,
            PlaceholderText = "Ví dụ: Nhẫn Cưới & Đính Hôn"
        };

        var lblMoTa = new Label { Text = "Mô Tả Danh Mục:", Location = new Point(24, 128), Width = 150, ForeColor = UITheme.TextSecondary };
        var txtMoTa = new TextBox
        {
            Text = isEdit ? target!.MoTa : "",
            Location = new Point(180, 124),
            Width = 430,
            Height = 140,
            Multiline = true,
            PlaceholderText = "Mô tả phân loại đặc thù của dòng trang sức này..."
        };

        btnSave.Click += (s, e) =>
        {
            var ma = txtMa.Text.Trim();
            var ten = txtTen.Text.Trim();
            var moTa = txtMoTa.Text.Trim();

            if (string.IsNullOrWhiteSpace(ma))
            {
                MessageBox.Show("Mã danh mục không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMa.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Tên danh mục không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTen.Focus();
                return;
            }

            var category = new DanhMuc(ma, ten, moTa);
            _services.DanhMucRepo.LuuDanhMuc(category);

            MessageBox.Show($"Lưu thành công danh mục [{ma}] - {ten}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        };

        pnlContent.Controls.Add(lblMa);
        pnlContent.Controls.Add(txtMa);
        pnlContent.Controls.Add(lblTen);
        pnlContent.Controls.Add(txtTen);
        pnlContent.Controls.Add(lblMoTa);
        pnlContent.Controls.Add(txtMoTa);

        dialog.Controls.Add(pnlContent);
        dialog.Controls.Add(pnlFoot);
        dialog.Controls.Add(pnlHead);

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadCategoryData();
        }
    }
}
