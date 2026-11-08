using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Admin.Views.UserControls.Catalog;

/// <summary>
/// Màn hình Quản lý Sản phẩm & Tồn kho trang sức Blink Blink Jewelry.
/// Hỗ trợ tìm kiếm, lọc danh mục, xem chi tiết thuộc tính kim hoàn, thêm sản phẩm mới (có ảnh) và chỉnh sửa toàn diện thông tin sản phẩm.
/// </summary>
public class ProductsView : UserControl
{
    private readonly AdminServiceScope _services;
    private List<SanPhamTrangSuc> _allProducts = new();
    private List<DanhMuc> _allCategories = new();

    private TextBox _txtSearch = null!;
    private ComboBox _cboCategory = null!;
    private Button _btnAddProduct = null!;
    private Button _btnEditProduct = null!;
    private Button _btnRefresh = null!;

    private Label _lblKpiTotalSku = null!;
    private Label _lblKpiInStock = null!;
    private Label _lblKpiOutOfStock = null!;
    private Label _lblKpiTotalValue = null!;

    private DataGridView _dgvProducts = null!;

    public ProductsView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadProductsData();
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
            Width = 585,
            Height = 85,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Mẫu (SKU)", out _lblKpiTotalSku, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Sẵn Sàng Bán", out _lblKpiInStock, UITheme.SuccessColor));
        pnlKpis.Controls.Add(CreateKpiCard("Chờ Nhập / Hết", out _lblKpiOutOfStock, UITheme.WarningColor));
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Giá Trị (VND)", out _lblKpiTotalValue, UITheme.PrimaryGold));

        var pnlTitleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 15, 0)
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ SẢN PHẨM & TỒN KHO KIM HOÀN",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Danh mục trang sức cao cấp, chất liệu, kích cỡ, kiểm soát tồn kho thực tế và cập nhật giá niêm yết",
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
            Width = 190,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "Tên, mã SP, chất liệu...",
            Margin = new Padding(0, 4, 15, 4)
        };
        _txtSearch.TextChanged += (s, e) => FilterProducts();

        var lblCat = new Label
        {
            Text = "Danh mục:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 6, 0)
        };

        _cboCategory = new ComboBox
        {
            Width = 170,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 4, 15, 4)
        };
        _cboCategory.SelectedIndexChanged += (s, e) => FilterProducts();

        _btnAddProduct = UITheme.CreateFlatButton("+ Thêm Sản Phẩm Mới", UITheme.DarkHeader, Color.White);
        _btnAddProduct.Margin = new Padding(0, 2, 8, 2);
        _btnAddProduct.Click += (s, e) => OpenProductEditDialog(isEdit: false);

        _btnEditProduct = UITheme.CreateFlatButton("✏ Chỉnh Sửa Chi Tiết", UITheme.PrimaryGold, Color.Black);
        _btnEditProduct.Margin = new Padding(0, 2, 8, 2);
        _btnEditProduct.Click += (s, e) => EditSelectedProduct();

        _btnRefresh = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (s, e) => LoadProductsData();

        toolbarPanel.Controls.Add(lblSearch);
        toolbarPanel.Controls.Add(_txtSearch);
        toolbarPanel.Controls.Add(lblCat);
        toolbarPanel.Controls.Add(_cboCategory);
        toolbarPanel.Controls.Add(_btnAddProduct);
        toolbarPanel.Controls.Add(_btnEditProduct);
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

        _dgvProducts = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvProducts);
        ConfigureGridColumns();
        _dgvProducts.CellDoubleClick += (s, e) => EditSelectedProduct();

        pnlWhiteCard.Controls.Add(_dgvProducts);
        pnlGridContainer.Controls.Add(pnlWhiteCard);

        this.Controls.Add(pnlGridContainer);
        this.Controls.Add(toolbarPanel);
        this.Controls.Add(topHeaderPanel);
    }

    private Panel CreateKpiCard(string title, out Label lblValue, Color valueColor)
    {
        var card = new Panel
        {
            Width = 140,
            Height = 75,
            BackColor = Color.White,
            Margin = new Padding(3),
            Padding = new Padding(8, 8, 8, 8)
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
        _dgvProducts.Columns.Clear();
        // Cấu hình không tự động co ép cột (None) để khi nội dung dài hoặc màn hình hẹp sẽ tự kích hoạt thanh cuộn ngang mượt mà
        _dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _dgvProducts.ScrollBars = ScrollBars.Both;

        var colMa = new DataGridViewTextBoxColumn 
        { 
            Name = "MaSanPham", 
            HeaderText = "MÃ SP", 
            Width = 95 
        };
        colMa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colTen = new DataGridViewTextBoxColumn 
        { 
            Name = "TenSanPham", 
            HeaderText = "TÊN TRANG SỨC", 
            Width = 240 
        };
        colTen.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colTen.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colMaNhom = new DataGridViewTextBoxColumn 
        { 
            Name = "MaNhom", 
            HeaderText = "MÃ NHÓM", 
            Width = 120,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        };
        colMaNhom.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var colDm = new DataGridViewTextBoxColumn 
        { 
            Name = "MaDanhMuc", 
            HeaderText = "DANH MỤC", 
            Width = 110,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        };
        colDm.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var colChatLieu = new DataGridViewTextBoxColumn 
        { 
            Name = "ChatLieu", 
            HeaderText = "CHẤT LIỆU", 
            Width = 160 
        };
        colChatLieu.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colChatLieu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colSize = new DataGridViewTextBoxColumn 
        { 
            Name = "Size", 
            HeaderText = "KÍCH CỠ", 
            Width = 95,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        };
        colSize.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var colGiaNhap = new DataGridViewTextBoxColumn 
        { 
            Name = "GiaNhap", 
            HeaderText = "GIÁ NHẬP BÌNH QUÂN\n(VND)", 
            Width = 175, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } 
        };
        colGiaNhap.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colGiaBan = new DataGridViewTextBoxColumn 
        { 
            Name = "GiaBan", 
            HeaderText = "ĐƠN GIÁ BÁN\n(VND)", 
            Width = 160, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } 
        };
        colGiaBan.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colGiamGia = new DataGridViewTextBoxColumn 
        { 
            Name = "PhanTramGiamGia", 
            HeaderText = "% GIẢM GIÁ", 
            Width = 115, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } 
        };
        colGiamGia.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var colTrangThai = new DataGridViewTextBoxColumn 
        { 
            Name = "TrangThai", 
            HeaderText = "TRẠNG THÁI KHO", 
            Width = 140 
        };
        colTrangThai.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colTrangThai.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colTonKho = new DataGridViewTextBoxColumn 
        { 
            Name = "SoLuongTon", 
            HeaderText = "TỒN KHO", 
            Width = 95, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } 
        };
        colTonKho.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var colHinhAnh = new DataGridViewTextBoxColumn 
        { 
            Name = "HinhAnh", 
            HeaderText = "HÌNH ẢNH", 
            Width = 95, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
        };
        colHinhAnh.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _dgvProducts.Columns.AddRange(colMa, colTen, colMaNhom, colDm, colChatLieu, colSize, colGiaNhap, colGiaBan, colGiamGia, colTonKho, colTrangThai, colHinhAnh);
    }

    public void LoadProductsData()
    {
        _allProducts = _services.SanPhamRepo.LayTatCa();
        _allCategories = _services.DanhMucRepo.LayTatCa();

        PopulateCategoryDropdown();
        UpdateKpis();
        FilterProducts();
    }

    private void PopulateCategoryDropdown()
    {
        var previousSelected = _cboCategory.SelectedItem?.ToString();
        _cboCategory.Items.Clear();
        _cboCategory.Items.Add("Tất Cả Danh Mục");

        foreach (var c in _allCategories)
        {
            _cboCategory.Items.Add(c.MaDanhMuc);
        }

        if (!string.IsNullOrEmpty(previousSelected) && _cboCategory.Items.Contains(previousSelected))
        {
            _cboCategory.SelectedItem = previousSelected;
        }
        else
        {
            _cboCategory.SelectedIndex = 0;
        }
    }

    private void UpdateKpis()
    {
        var viVn = new CultureInfo("vi-VN");
        var inStock = _allProducts.Count(p => string.Equals(p.TrangThai, "SanSang", StringComparison.OrdinalIgnoreCase));
        var outOfStock = _allProducts.Count(p => string.Equals(p.TrangThai, "HetHang", StringComparison.OrdinalIgnoreCase));

        _lblKpiTotalSku.Text = _allProducts.Count.ToString("N0", viVn);
        _lblKpiInStock.Text = inStock.ToString("N0", viVn);
        _lblKpiOutOfStock.Text = outOfStock.ToString("N0", viVn);
        _lblKpiTotalValue.Text = _allProducts.Sum(p => p.GiaBan).ToString("N0", viVn);
    }

    private void FilterProducts()
    {
        var query = _txtSearch.Text.Trim().ToLowerInvariant();
        var selectedCat = _cboCategory.SelectedItem?.ToString() ?? "Tất Cả Danh Mục";

        var filtered = _allProducts.Where(p =>
        {
            var matchSearch = string.IsNullOrEmpty(query) ||
                              p.MaSanPham.ToLowerInvariant().Contains(query) ||
                              p.TenSanPham.ToLowerInvariant().Contains(query) ||
                              p.ChatLieu.ToLowerInvariant().Contains(query);

            var matchCat = selectedCat == "Tất Cả Danh Mục" ||
                           string.Equals(p.MaDanhMuc, selectedCat, StringComparison.OrdinalIgnoreCase);

            return matchSearch && matchCat;
        }).ToList();

        _dgvProducts.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");

        foreach (var p in filtered)
        {
            var hasImage = "-";
            if (!string.IsNullOrWhiteSpace(p.HinhAnh))
            {
                if (p.HinhAnh.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    p.HinhAnh.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    hasImage = "🌐 Link web";
                }
                else if (p.HinhAnh.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                {
                    hasImage = "📁 Ảnh kho";
                }
                else
                {
                    hasImage = "🖼 File máy";
                }
            }
            var discountText = p.PhanTramGiamGia > 0 ? $"{p.PhanTramGiamGia}%" : "0%";
            var maNhomText = string.IsNullOrWhiteSpace(p.MaNhom) || p.MaNhom == "0" ? "0" : p.MaNhom;
            var rowIdx = _dgvProducts.Rows.Add(
                p.MaSanPham,
                p.TenSanPham,
                maNhomText,
                p.MaDanhMuc,
                p.ChatLieu,
                p.Size,
                p.GiaNhap.ToString("N0", viVn),
                p.GiaBan.ToString("N0", viVn),
                discountText,
                p.SoLuongTon.ToString("N0", viVn),
                FormatStatusText(p.TrangThai),
                hasImage
            );

            var cellDiscount = _dgvProducts.Rows[rowIdx].Cells["PhanTramGiamGia"];
            if (p.PhanTramGiamGia > 0)
            {
                cellDiscount.Style.ForeColor = UITheme.DangerColor;
                cellDiscount.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }

            var cellTonKho = _dgvProducts.Rows[rowIdx].Cells["SoLuongTon"];
            if (p.SoLuongTon > 0)
            {
                cellTonKho.Style.ForeColor = Color.FromArgb(16, 185, 129); // Xanh lục ngọc
                cellTonKho.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            else
            {
                cellTonKho.Style.ForeColor = UITheme.DangerColor;
                cellTonKho.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }

            var cellStatus = _dgvProducts.Rows[rowIdx].Cells["TrangThai"];
            if (string.Equals(p.TrangThai, "SanSang", StringComparison.OrdinalIgnoreCase))
                cellStatus.Style.ForeColor = UITheme.SuccessColor;
            else if (string.Equals(p.TrangThai, "HetHang", StringComparison.OrdinalIgnoreCase))
                cellStatus.Style.ForeColor = UITheme.WarningColor;
            else
                cellStatus.Style.ForeColor = UITheme.TextSecondary;
        }

        if (_dgvProducts.Rows.Count > 0)
        {
            _dgvProducts.Rows[0].Selected = true;
        }
    }

    private static string FormatStatusText(string status)
    {
        return status switch
        {
            "SanSang" => "Sẵn Sàng Bán",
            "HetHang" => "Hết Hàng (Tồn 0)",
            "DaBan" => "Đã Bán Hết",
            "DangBaoHanh" => "Đang Bảo Hành",
            _ => status
        };
    }

    private void EditSelectedProduct()
    {
        if (_dgvProducts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maSP = _dgvProducts.SelectedRows[0].Cells["MaSanPham"].Value?.ToString();
        var targetProduct = _allProducts.FirstOrDefault(p => p.MaSanPham == maSP);
        if (targetProduct == null) return;

        OpenProductEditDialog(isEdit: true, targetProduct);
    }

    private void OpenProductEditDialog(bool isEdit, SanPhamTrangSuc? target = null)
    {
        using var dialog = new ProductEditDialog(isEdit, target);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadProductsData();
        }
    }
}
