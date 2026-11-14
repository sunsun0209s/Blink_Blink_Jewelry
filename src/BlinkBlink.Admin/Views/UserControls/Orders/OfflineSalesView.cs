using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.DTOs.Orders;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;

namespace BlinkBlink.Admin.Views.UserControls.Orders;

/// <summary>
/// Giao diện Bán hàng tại quầy (POS Bán hàng Offline) của Blink Blink Jewelry.
/// Dành cho Quản trị viên (ADMIN) và Nhân viên bán hàng (SALE).
/// Tạo mã đơn tự động HD_OFFLINE_..., liên kết khách hàng qua SĐT (nếu không có thì mã 0),
/// bảng tạm tính tiền & % giảm giá tức thời, popup thanh toán Tiền mặt hoặc Quét QR.
/// </summary>
public class OfflineSalesView : UserControl
{
    private readonly AdminServiceScope _services;
    private readonly CultureInfo _viVn = new("vi-VN");

    // Khách hàng
    private TextBox _txtCustName = null!;
    private TextBox _txtCustPhone = null!;
    private TextBox _txtCustAddress = null!;
    private TextBox _txtOrderNote = null!;
    private Label _lblCustomerStatus = null!;

    // Tìm kiếm và thêm sản phẩm
    private TextBox _txtSearchProduct = null!;
    private ComboBox _cboProducts = null!;
    private ComboBox _cboProductSize = null!;
    private bool _isSyncingPosSize;
    private NumericUpDown _numQuantity = null!;
    private Label _lblProductDiscountInfo = null!;
    private Button _btnAddItem = null!;

    // Danh sách mặt hàng mua tạm
    private DataGridView _dgvCart = null!;
    private readonly List<ChiTietBanOfflineDTO> _cartItems = new();
    private List<SanPhamTrangSuc> _allAvailableProducts = new();
    private bool _isUpdatingCartGrid;

    // Tóm tắt tiền
    private Label _lblSubTotal = null!;
    private Label _lblTotalDiscount = null!;
    private Label _lblGrandTotal = null!;
    private Button _btnCheckout = null!;
    private Button _btnClearCart = null!;

    public OfflineSalesView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadProductsList();
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // Header Panel cố định ở đỉnh với chiều cao 70px
        var topHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(20, 12, 20, 8),
            BackColor = Color.White
        };

        var lblTitle = new Label
        {
            Text = "BÁN HÀNG TẠI QUẦY (POS OFFLINE) - BLINK BLINK JEWELRY",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(20, 10),
            AutoSize = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Lập hóa đơn bán lẻ tại quầy, chiết khấu khuyến mãi tức thì và xác nhận thanh toán trực tiếp",
            Font = UITheme.RegularFont,
            ForeColor = UITheme.TextSecondary,
            Location = new Point(20, 36),
            AutoSize = true,
            UseMnemonic = false
        };

        topHeaderPanel.Controls.Add(lblTitle);
        topHeaderPanel.Controls.Add(lblSubTitle);

        // Khung chính chứa 2 cột: Trái (Khách hàng & Chọn SP), Phải (Bảng giỏ hàng & Tổng thanh toán)
        var mainContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = UITheme.CanvasBackground
        };

        // Panel Trái: Cố định 490px để các ô nhập liệu rộng rãi thông thoáng
        var leftPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 490,
            Padding = new Padding(0, 0, 10, 0),
            BackColor = Color.Transparent
        };

        // Thanh phân cách co giãn
        var splitter = new Splitter
        {
            Dock = DockStyle.Left,
            Width = 6,
            MinSize = 420,
            MinExtra = 450,
            BackColor = UITheme.BorderColor
        };

        // Panel Phải: Tự động Fill
        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 0, 0, 0),
            BackColor = Color.Transparent
        };

        // ==================== PANEL TRÁI: KHÁCH HÀNG & CHỌN SẢN PHẨM ====================
        var leftScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };

        // 1. Panel Khách hàng dùng TableLayoutPanel 2 cột chống triệt để lỗi đè chữ
        var pnlCustomer = new GroupBox
        {
            Text = " 👤 THÔNG TIN KHÁCH HÀNG ",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            BackColor = Color.White,
            Dock = DockStyle.Top,
            Height = 260,
            Padding = new Padding(12, 10, 12, 10)
        };

        var tblCustomer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.Transparent
        };
        tblCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135f));
        tblCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        tblCustomer.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        tblCustomer.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
        tblCustomer.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        tblCustomer.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        tblCustomer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var lblPhone = new Label { Text = "Số Điện Thoại:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _txtCustPhone = new TextBox { Dock = DockStyle.Fill, Font = UITheme.RegularFont, PlaceholderText = "Nhập SĐT để tự động nhận diện..." };
        _txtCustPhone.TextChanged += (s, e) => LookupCustomerByPhone();
        _txtCustPhone.Leave += (s, e) => LookupCustomerByPhone();

        _lblCustomerStatus = new Label
        {
            Text = "(Khách vãng lai / Mã 0)",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };

        var lblName = new Label { Text = "Tên Khách Hàng:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _txtCustName = new TextBox { Dock = DockStyle.Fill, Font = UITheme.RegularFont, PlaceholderText = "Khách vãng lai" };

        var lblAddress = new Label { Text = "Địa Chỉ:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _txtCustAddress = new TextBox { Dock = DockStyle.Fill, Font = UITheme.RegularFont, PlaceholderText = "Mua trực tiếp tại cửa hàng" };

        var lblNote = new Label { Text = "Ghi Chú Đơn:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
        _txtOrderNote = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = UITheme.RegularFont };

        tblCustomer.Controls.Add(lblPhone, 0, 0);
        tblCustomer.Controls.Add(_txtCustPhone, 1, 0);
        tblCustomer.Controls.Add(new Panel { Height = 0 }, 0, 1);
        tblCustomer.Controls.Add(_lblCustomerStatus, 1, 1);
        tblCustomer.Controls.Add(lblName, 0, 2);
        tblCustomer.Controls.Add(_txtCustName, 1, 2);
        tblCustomer.Controls.Add(lblAddress, 0, 3);
        tblCustomer.Controls.Add(_txtCustAddress, 1, 3);
        tblCustomer.Controls.Add(lblNote, 0, 4);
        tblCustomer.Controls.Add(_txtOrderNote, 1, 4);

        pnlCustomer.Controls.Add(tblCustomer);

        // 2. Panel Chọn Sản Phẩm dùng TableLayoutPanel 2 cột (Ưu đãi lấy tự động từ CSDL, chọn Size thông minh)
        var pnlAddProduct = new GroupBox
        {
            Text = " 💎 THÊM SẢN PHẨM VÀO ĐƠN ",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            BackColor = Color.White,
            Dock = DockStyle.Top,
            Height = 300,
            Padding = new Padding(12, 10, 12, 10)
        };

        var tblAddProduct = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Color.Transparent
        };
        tblAddProduct.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115f));
        tblAddProduct.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        tblAddProduct.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        tblAddProduct.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        tblAddProduct.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        tblAddProduct.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        tblAddProduct.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        tblAddProduct.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));

        var lblSearch = new Label { Text = "Tìm Kiếm:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _txtSearchProduct = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = UITheme.RegularFont,
            PlaceholderText = "🔍 Gõ tên hoặc mã SP để lọc nhanh..."
        };
        _txtSearchProduct.TextChanged += (s, e) => FilterProductsList(_txtSearchProduct.Text.Trim());

        var lblP = new Label { Text = "Sản Phẩm:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _cboProducts = new ComboBox
        {
            Dock = DockStyle.Fill,
            Font = UITheme.RegularFont,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = 520
        };
        _cboProducts.SelectedIndexChanged += (s, e) =>
        {
            UpdateProductDiscountDisplay();
            UpdateProductSizeDisplay();
        };

        var lblSize = new Label { Text = "Kích Cỡ / Size:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _cboProductSize = new ComboBox
        {
            Dock = DockStyle.Fill,
            Font = UITheme.RegularFont,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboProductSize.SelectedIndexChanged += (s, e) => OnProductSizeChanged();

        var lblDiscountTag = new Label { Text = "Ưu Đãi SP:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _lblProductDiscountInfo = new Label
        {
            Text = "🏷️ Không giảm giá",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };

        var lblQty = new Label { Text = "Số Lượng:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _numQuantity = new NumericUpDown
        {
            Width = 100,
            Font = UITheme.RegularFont,
            Minimum = 1,
            Maximum = 9999,
            Value = 1,
            Anchor = AnchorStyles.Left
        };

        _btnAddItem = UITheme.CreateFlatButton("+ THÊM VÀO ĐƠN", UITheme.PrimaryGold, Color.Black);
        _btnAddItem.Dock = DockStyle.Fill;
        _btnAddItem.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnAddItem.Click += BtnAddItem_Click;

        tblAddProduct.Controls.Add(lblSearch, 0, 0);
        tblAddProduct.Controls.Add(_txtSearchProduct, 1, 0);
        tblAddProduct.Controls.Add(lblP, 0, 1);
        tblAddProduct.Controls.Add(_cboProducts, 1, 1);
        tblAddProduct.Controls.Add(lblSize, 0, 2);
        tblAddProduct.Controls.Add(_cboProductSize, 1, 2);
        tblAddProduct.Controls.Add(lblDiscountTag, 0, 3);
        tblAddProduct.Controls.Add(_lblProductDiscountInfo, 1, 3);
        tblAddProduct.Controls.Add(lblQty, 0, 4);
        tblAddProduct.Controls.Add(_numQuantity, 1, 4);
        tblAddProduct.Controls.Add(new Panel { Height = 0 }, 0, 5);
        tblAddProduct.Controls.Add(_btnAddItem, 1, 5);

        pnlAddProduct.Controls.Add(tblAddProduct);

        var spacingPanel = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent };

        leftScroll.Controls.Add(pnlAddProduct);
        leftScroll.Controls.Add(spacingPanel);
        leftScroll.Controls.Add(pnlCustomer);
        leftPanel.Controls.Add(leftScroll);

        // ==================== PANEL PHẢI: BẢNG TẠM VÀ THANH TOÁN ====================
        // Grid mặt hàng
        _dgvCart = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvCart);
        _dgvCart.Dock = DockStyle.Fill;
        _dgvCart.AllowUserToAddRows = false;
        _dgvCart.ReadOnly = false;
        _dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvCart.ScrollBars = ScrollBars.Both;

        var colMa = new DataGridViewTextBoxColumn { Name = "MaSP", HeaderText = "MÃ SP", ReadOnly = true, FillWeight = 85, MinimumWidth = 80 };
        var colTen = new DataGridViewTextBoxColumn { Name = "TenSP", HeaderText = "TÊN TRANG SỨC", ReadOnly = true, FillWeight = 200, MinimumWidth = 140 };
        var colGia = new DataGridViewTextBoxColumn { Name = "DonGia", HeaderText = "ĐƠN GIÁ (VND)", ReadOnly = true, FillWeight = 110, MinimumWidth = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colSl = new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "SỐ LƯỢNG", ReadOnly = false, FillWeight = 65, MinimumWidth = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } };
        var colGiam = new DataGridViewTextBoxColumn { Name = "PhanTramGiamGia", HeaderText = "% GIẢM", ReadOnly = true, FillWeight = 65, MinimumWidth = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } };
        var colTienGiam = new DataGridViewTextBoxColumn { Name = "TienGiam", HeaderText = "TIỀN GIẢM (VND)", ReadOnly = true, FillWeight = 110, MinimumWidth = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colThanhTien = new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "THÀNH TIỀN (VND)", ReadOnly = true, FillWeight = 125, MinimumWidth = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } };
        var colXoa = new DataGridViewButtonColumn { Name = "Xoa", HeaderText = "XÓA", Text = "🗑️", UseColumnTextForButtonValue = true, FillWeight = 50, MinimumWidth = 45 };

        _dgvCart.Columns.AddRange(colMa, colTen, colGia, colSl, colGiam, colTienGiam, colThanhTien, colXoa);
        _dgvCart.CellContentClick += DgvCart_CellContentClick;
        _dgvCart.CellValueChanged += DgvCart_CellValueChanged;

        // Bottom Summary Panel dùng TableLayoutPanel 4 hàng rõ ràng
        var pnlSummary = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 185,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(16, 12, 16, 12)
        };

        var tblSummary = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        tblSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180f));
        tblSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        tblSummary.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        tblSummary.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        tblSummary.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
        tblSummary.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));

        var lblSubTitle1 = new Label { Text = "Tổng Tiền Hàng:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _lblSubTotal = new Label { Text = "0 VND", Font = UITheme.TabularNumericFont, ForeColor = UITheme.TextPrimary, AutoSize = true, Anchor = AnchorStyles.Left };

        var lblSubTitle2 = new Label { Text = "Tổng Giảm Giá:", Font = UITheme.RegularFont, ForeColor = UITheme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left };
        _lblTotalDiscount = new Label { Text = "0 VND", Font = UITheme.TabularNumericFont, ForeColor = UITheme.DangerColor, AutoSize = true, Anchor = AnchorStyles.Left };

        var lblSubTitle3 = new Label { Text = "TỔNG THANH TOÁN:", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = UITheme.TextPrimary, AutoSize = true, Anchor = AnchorStyles.Left };
        _lblGrandTotal = new Label { Text = "0 VND", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = UITheme.PrimaryGold, AutoSize = true, Anchor = AnchorStyles.Left };

        _btnClearCart = UITheme.CreateFlatButton("HỦY ĐƠN", Color.FromArgb(241, 245, 249), UITheme.DangerColor);
        _btnClearCart.Size = new Size(110, 38);
        _btnClearCart.Margin = new Padding(0, 0, 12, 0);
        _btnClearCart.Click += (s, e) => ClearCart();

        _btnCheckout = UITheme.CreateFlatButton("💳 THANH TOÁN NGAY", UITheme.PrimaryGold, Color.Black);
        _btnCheckout.Size = new Size(240, 38);
        _btnCheckout.Margin = new Padding(0);
        _btnCheckout.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        _btnCheckout.Click += BtnCheckout_Click;

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 6, 0, 0)
        };
        pnlButtons.Controls.Add(_btnClearCart);
        pnlButtons.Controls.Add(_btnCheckout);

        tblSummary.Controls.Add(lblSubTitle1, 0, 0);
        tblSummary.Controls.Add(_lblSubTotal, 1, 0);
        tblSummary.Controls.Add(lblSubTitle2, 0, 1);
        tblSummary.Controls.Add(_lblTotalDiscount, 1, 1);
        tblSummary.Controls.Add(lblSubTitle3, 0, 2);
        tblSummary.Controls.Add(_lblGrandTotal, 1, 2);
        tblSummary.Controls.Add(pnlButtons, 0, 3);
        tblSummary.SetColumnSpan(pnlButtons, 2);

        pnlSummary.Controls.Add(tblSummary);

        rightPanel.Controls.Add(_dgvCart);
        rightPanel.Controls.Add(pnlSummary);
        _dgvCart.BringToFront();

        mainContainer.Controls.Add(rightPanel);
        mainContainer.Controls.Add(splitter);
        mainContainer.Controls.Add(leftPanel);
        leftPanel.SendToBack();
        splitter.SendToBack();
        rightPanel.BringToFront();

        this.Controls.Add(mainContainer);
        this.Controls.Add(topHeaderPanel);
        topHeaderPanel.SendToBack();
        mainContainer.BringToFront();
    }

    public void LoadProductsList()
    {
        // Chỉ nạp các sản phẩm được phép kinh doanh có tồn kho khả dụng (> 0) và không ngừng kinh doanh
        _allAvailableProducts = _services.SanPhamRepo.LayTatCa()
            .Where(p => p.SoLuongTon > 0 && !string.Equals(p.TrangThai, "NgungKinhDoanh", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Đồng bộ trạng thái SanSang cho các sản phẩm còn tồn kho khả dụng
        foreach (var p in _allAvailableProducts)
        {
            if (p.TrangThai != "SanSang")
            {
                p.TrangThai = "SanSang";
            }
        }

        FilterProductsList(string.Empty);
    }

    /// <summary>
    /// Lọc động danh sách sản phẩm khi nhân viên gõ từ khóa vào ô tìm kiếm.
    /// Cho phép tìm theo tên, mã sản phẩm hoặc chất liệu không phân biệt hoa thường.
    /// </summary>
    private void FilterProductsList(string keyword)
    {
        _cboProducts.Items.Clear();

        var filtered = string.IsNullOrWhiteSpace(keyword)
            ? _allAvailableProducts
            : _allAvailableProducts.Where(p =>
                (p.TenSanPham != null && p.TenSanPham.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (p.MaSanPham != null && p.MaSanPham.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (p.ChatLieu != null && p.ChatLieu.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();

        foreach (var p in filtered)
        {
            var display = $"[{p.MaSanPham}] {p.TenSanPham}";
            _cboProducts.Items.Add(new ProductComboItem(p.MaSanPham, display));
        }

        if (_cboProducts.Items.Count > 0)
        {
            _cboProducts.SelectedIndex = 0;
            UpdateProductDiscountDisplay();
            UpdateProductSizeDisplay();
        }
        else
        {
            _lblProductDiscountInfo.Text = "Không có SP phù hợp";
            _lblProductDiscountInfo.ForeColor = UITheme.DangerColor;
            _cboProductSize.Items.Clear();
        }
    }

    /// <summary>
    /// Cập nhật hiển thị mức % giảm giá tự động từ CSDL khi nhân viên chọn sản phẩm.
    /// Có giảm giá thì hiện % giảm giá, không có giảm giá thì ghi "Không giảm giá".
    /// </summary>
    private void UpdateProductDiscountDisplay()
    {
        if (_cboProducts.SelectedItem is ProductComboItem item)
        {
            var sp = _allAvailableProducts.FirstOrDefault(p => p.MaSanPham == item.Key) 
                     ?? _services.SanPhamRepo.TimTheoId(item.Key);
            if (sp != null)
            {
                if (sp.PhanTramGiamGia > 0)
                {
                    _lblProductDiscountInfo.Text = $"🏷️ Giảm {sp.PhanTramGiamGia}%";
                    _lblProductDiscountInfo.ForeColor = UITheme.SuccessColor;
                }
                else
                {
                    _lblProductDiscountInfo.Text = "🏷️ Không giảm giá";
                    _lblProductDiscountInfo.ForeColor = UITheme.TextSecondary;
                }
            }
        }
    }

    /// <summary>
    /// Cập nhật ComboBox Size thông minh theo nhóm sản phẩm tại quầy POS.
    /// Nếu sản phẩm thuộc nhóm, nạp các size của nhóm; nếu không có nhóm, hiển thị 1 size.
    /// </summary>
    private void UpdateProductSizeDisplay()
    {
        if (_isSyncingPosSize) return;
        if (_cboProducts.SelectedItem is not ProductComboItem item) return;

        var sp = _allAvailableProducts.FirstOrDefault(p => p.MaSanPham == item.Key)
                 ?? _services.SanPhamRepo.TimTheoId(item.Key);
        if (sp == null) return;

        _isSyncingPosSize = true;
        try
        {
            _cboProductSize.Items.Clear();
            if (!string.IsNullOrWhiteSpace(sp.MaNhom) && sp.MaNhom != "0")
            {
                var groupMembers = _services.SanPhamRepo.LayTheoNhom(sp.MaNhom);
                foreach (var m in groupMembers)
                {
                    var sText = string.IsNullOrWhiteSpace(m.Size) ? "Tiêu chuẩn" : m.Size;
                    if (!_cboProductSize.Items.Contains(sText))
                    {
                        _cboProductSize.Items.Add(sText);
                    }
                }

                var currentSizeText = string.IsNullOrWhiteSpace(sp.Size) ? "Tiêu chuẩn" : sp.Size;
                var matchIdx = _cboProductSize.Items.IndexOf(currentSizeText);
                _cboProductSize.SelectedIndex = matchIdx >= 0 ? matchIdx : 0;
            }
            else
            {
                var singleSize = string.IsNullOrWhiteSpace(sp.Size) ? "Tiêu chuẩn" : sp.Size;
                _cboProductSize.Items.Add(singleSize);
                _cboProductSize.SelectedIndex = 0;
            }
        }
        finally
        {
            _isSyncingPosSize = false;
        }
    }

    /// <summary>
    /// Khi nhân viên đổi size trong ComboBox, tự động chuyển đổi sản phẩm trong ComboBox tương ứng với mã SP của size đó.
    /// </summary>
    private void OnProductSizeChanged()
    {
        if (_isSyncingPosSize) return;
        if (_cboProducts.SelectedItem is not ProductComboItem item) return;

        var sp = _allAvailableProducts.FirstOrDefault(p => p.MaSanPham == item.Key)
                 ?? _services.SanPhamRepo.TimTheoId(item.Key);
        if (sp == null || string.IsNullOrWhiteSpace(sp.MaNhom) || sp.MaNhom == "0") return;

        var selectedSize = _cboProductSize.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selectedSize)) return;

        var groupMembers = _services.SanPhamRepo.LayTheoNhom(sp.MaNhom);
        var matchSp = groupMembers.FirstOrDefault(m =>
            string.Equals(string.IsNullOrWhiteSpace(m.Size) ? "Tiêu chuẩn" : m.Size, selectedSize, StringComparison.OrdinalIgnoreCase)
        );

        if (matchSp != null && matchSp.MaSanPham != sp.MaSanPham)
        {
            _isSyncingPosSize = true;
            try
            {
                // Làm trống ô tìm kiếm nếu đang có ký tự để nạp đầy đủ danh sách sản phẩm trong nhóm
                if (!string.IsNullOrWhiteSpace(_txtSearchProduct.Text))
                {
                    _txtSearchProduct.Text = string.Empty;
                    FilterProductsList(string.Empty);
                }

                for (int i = 0; i < _cboProducts.Items.Count; i++)
                {
                    if ((_cboProducts.Items[i] as ProductComboItem)?.Key == matchSp.MaSanPham)
                    {
                        _cboProducts.SelectedIndex = i;
                        UpdateProductDiscountDisplay();
                        break;
                    }
                }
            }
            finally
            {
                _isSyncingPosSize = false;
            }
        }
    }

    private void LookupCustomerByPhone()
    {
        var phone = _txtCustPhone.Text.Trim();
        if (string.IsNullOrWhiteSpace(phone))
        {
            _lblCustomerStatus.Text = "(Khách vãng lai / Mã 0)";
            _lblCustomerStatus.ForeColor = UITheme.TextSecondary;
            return;
        }

        var kh = _services.KhachHangRepo.TimTheoSoDienThoai(phone);
        if (kh != null)
        {
            _txtCustName.Text = kh.HoTen;
            _lblCustomerStatus.Text = $"✓ Đã liên kết: [{kh.MaKhachHang}] {kh.HoTen}";
            _lblCustomerStatus.ForeColor = UITheme.SuccessColor;
        }
        else
        {
            _lblCustomerStatus.Text = "(Chưa có tài khoản. Sẽ tự động liên kết khi khách đăng ký)";
            _lblCustomerStatus.ForeColor = UITheme.PrimaryGold;
        }
    }

    private void BtnAddItem_Click(object? sender, EventArgs e)
    {
        if (_cboProducts.SelectedItem is not ProductComboItem selectedItem)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm cần thêm vào đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sp = _allAvailableProducts.FirstOrDefault(p => p.MaSanPham == selectedItem.Key) ?? _services.SanPhamRepo.TimTheoId(selectedItem.Key);
        if (sp == null || !sp.KiemTraTonKho() || sp.SoLuongTon <= 0)
        {
            MessageBox.Show("Sản phẩm không khả dụng hoặc đã hết hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var qty = (int)_numQuantity.Value;
        if (qty <= 0)
        {
            MessageBox.Show("Số lượng mua phải lớn hơn 0.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existing = _cartItems.FirstOrDefault(c => c.MaSanPham == sp.MaSanPham);
        var currentInCart = existing?.SoLuong ?? 0;
        var totalRequested = currentInCart + qty;

        // Ràng buộc tồn kho: Không cho phép vượt quá số lượng tồn hiện có
        if (totalRequested > sp.SoLuongTon)
        {
            var remainingCanAdd = sp.SoLuongTon - currentInCart;
            if (remainingCanAdd > 0)
            {
                MessageBox.Show(
                    $"Số lượng yêu cầu ({totalRequested}) vượt quá số lượng tồn kho ({sp.SoLuongTon}) của sản phẩm [{sp.MaSanPham}] {sp.TenSanPham}.\n" +
                    $"Hiện trong đơn đã có {currentInCart} món, bạn chỉ có thể thêm tối đa {remainingCanAdd} món nữa.",
                    "Cảnh Báo Vượt Tồn Kho",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            else
            {
                MessageBox.Show(
                    $"Sản phẩm [{sp.MaSanPham}] {sp.TenSanPham} chỉ có {sp.SoLuongTon} món trong kho và đơn hàng hiện đã chứa đủ số lượng tối đa này.",
                    "Hết Tồn Kho Khả Dụng",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            return;
        }

        // Mức chiết khấu % giảm giá được lấy tự động 100% từ cấu hình sản phẩm trong CSDL
        var discount = sp.PhanTramGiamGia;

        if (existing != null)
        {
            existing.SoLuong += qty;
            existing.PhanTramGiamGia = discount;
        }
        else
        {
            _cartItems.Add(new ChiTietBanOfflineDTO
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                DonGia = sp.GiaBan,
                SoLuong = qty,
                PhanTramGiamGia = discount
            });
        }

        RefreshCartGrid();
    }

    private void RefreshCartGrid()
    {
        _isUpdatingCartGrid = true;
        try
        {
            _dgvCart.Rows.Clear();
            decimal totalGoc = 0;
            decimal totalTienGiam = 0;
            decimal totalThanhTien = 0;

            foreach (var item in _cartItems)
            {
                var goc = item.DonGia * item.SoLuong;
                var thanhTien = item.ThanhTien;
                var giam = item.TienGiam;

                totalGoc += goc;
                totalTienGiam += giam;
                totalThanhTien += thanhTien;

                _dgvCart.Rows.Add(
                    item.MaSanPham,
                    item.TenSanPham,
                    item.DonGia.ToString("N0", _viVn),
                    item.SoLuong,
                    item.PhanTramGiamGia,
                    giam.ToString("N0", _viVn),
                    thanhTien.ToString("N0", _viVn)
                );
            }

            _lblSubTotal.Text = totalGoc.ToString("N0", _viVn) + " VND";
            _lblTotalDiscount.Text = totalTienGiam.ToString("N0", _viVn) + " VND";
            _lblGrandTotal.Text = totalThanhTien.ToString("N0", _viVn) + " VND";
        }
        finally
        {
            _isUpdatingCartGrid = false;
        }
    }

    private void DgvCart_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex == _dgvCart.Columns["Xoa"].Index)
        {
            var maSp = _dgvCart.Rows[e.RowIndex].Cells["MaSP"].Value?.ToString();
            var item = _cartItems.FirstOrDefault(c => c.MaSanPham == maSp);
            if (item != null)
            {
                _cartItems.Remove(item);
                RefreshCartGrid();
            }
        }
    }

    private void DgvCart_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_isUpdatingCartGrid) return;
        if (e.RowIndex < 0 || e.RowIndex >= _cartItems.Count) return;

        var colName = _dgvCart.Columns[e.ColumnIndex].Name;
        var row = _dgvCart.Rows[e.RowIndex];
        var item = _cartItems[e.RowIndex];

        if (colName == "SoLuong")
        {
            if (int.TryParse(row.Cells["SoLuong"].Value?.ToString(), out var newQty) && newQty > 0)
            {
                var sp = _allAvailableProducts.FirstOrDefault(p => p.MaSanPham == item.MaSanPham)
                         ?? _services.SanPhamRepo.TimTheoId(item.MaSanPham);

                var maxTon = sp?.SoLuongTon ?? 0;
                if (maxTon > 0 && newQty > maxTon)
                {
                    MessageBox.Show(
                        $"Số lượng nhập ({newQty}) vượt quá số lượng tồn kho ({maxTon}) của sản phẩm [{item.MaSanPham}] {item.TenSanPham}.\nHệ thống sẽ khôi phục lại số lượng trước đó ({item.SoLuong}).",
                        "Vượt Quá Tồn Kho",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    _isUpdatingCartGrid = true;
                    row.Cells["SoLuong"].Value = item.SoLuong;
                    _isUpdatingCartGrid = false;
                    return;
                }

                item.SoLuong = newQty;
            }
            else
            {
                MessageBox.Show("Số lượng phải là số nguyên dương lớn hơn 0.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _isUpdatingCartGrid = true;
                row.Cells["SoLuong"].Value = item.SoLuong;
                _isUpdatingCartGrid = false;
            }
            RefreshCartGrid();
        }
    }

    private void ClearCart()
    {
        if (_cartItems.Count == 0) return;
        var confirm = MessageBox.Show("Bạn có muốn hủy toàn bộ các món trong đơn hàng hiện tại?", "Xác nhận hủy đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            _cartItems.Clear();
            _txtCustName.Clear();
            _txtCustPhone.Clear();
            _txtCustAddress.Clear();
            _txtOrderNote.Clear();
            _lblCustomerStatus.Text = "(Chưa liên kết / Khách mới)";
            _lblCustomerStatus.ForeColor = UITheme.TextSecondary;
            RefreshCartGrid();
        }
    }

    private void BtnCheckout_Click(object? sender, EventArgs e)
    {
        if (!_cartItems.Any())
        {
            MessageBox.Show("Giỏ hàng đang trống! Vui lòng thêm ít nhất một sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal grandTotal = _cartItems.Sum(c => c.ThanhTien);

        // Hiển thị Popup Thanh Toán Offline
        using var payForm = new Form
        {
            Text = "XÁC NHẬN THANH TOÁN ĐƠN HÀNG OFFLINE - BLINK BLINK JEWELRY",
            Width = 540,
            Height = 640,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(25, 15, 25, 10) };
        var lblT1 = new Label { Text = "SỐ TIỀN CẦN THANH TOÁN:", Location = new Point(25, 12), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = UITheme.TextSecondary };
        var lblAmount = new Label { Text = grandTotal.ToString("N0", _viVn) + " VND", Location = new Point(25, 35), AutoSize = true, Font = new Font("Segoe UI", 19F, FontStyle.Bold), ForeColor = UITheme.PrimaryGold };
        pnlTop.Controls.Add(lblT1);
        pnlTop.Controls.Add(lblAmount);

        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25, 15, 25, 15) };

        // 2 Lựa chọn thanh toán xếp theo chiều dọc độc lập chống triệt để đè chữ
        var pnlOptions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 78,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        var rdoCash = new RadioButton
        {
            Text = "💵 Thanh toán Tiền Mặt tại quầy",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Checked = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        var rdoQr = new RadioButton
        {
            Text = "📲 Quét mã QR",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Checked = false,
            Margin = new Padding(0)
        };

        pnlOptions.Controls.Add(rdoCash);
        pnlOptions.Controls.Add(rdoQr);

        // Khung chi tiết nội dung
        var pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };

        // Panel tiền mặt: Gọn gàng xác nhận thu tiền trực tiếp, không cần nhập tiền thối
        var pnlCashDetails = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 253, 244),
            Padding = new Padding(20),
            Visible = true
        };

        var lblCashNotice = new Label
        {
            Text = $"✓ Đã xác nhận thu tiền mặt trực tiếp tại quầy:\n\n   Tổng thanh toán:  {grandTotal.ToString("N0", _viVn)} VND\n\nVui lòng nhấn nút 'XÁC NHẬN THANH TOÁN' bên dưới để xuất hóa đơn.",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(22, 101, 52)
        };
        pnlCashDetails.Controls.Add(lblCashNotice);

        // Panel QR Code
        var pnlQrDetails = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Visible = false
        };

        var picQr = new PictureBox
        {
            Size = new Size(310, 310),
            Location = new Point(85, 5),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            Image = LoadQrImage()
        };

        var lblQrGuide = new Label
        {
            Text = "Quý khách vui lòng quét mã QR để chuyển khoản chính xác số tiền.",
            Location = new Point(35, 325),
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic)
        };

        pnlQrDetails.Controls.Add(picQr);
        pnlQrDetails.Controls.Add(lblQrGuide);

        rdoCash.CheckedChanged += (s, ev) =>
        {
            pnlCashDetails.Visible = rdoCash.Checked;
            pnlQrDetails.Visible = !rdoCash.Checked;
        };

        rdoQr.CheckedChanged += (s, ev) =>
        {
            pnlQrDetails.Visible = rdoQr.Checked;
            pnlCashDetails.Visible = !rdoQr.Checked;
        };

        pnlContent.Controls.Add(pnlCashDetails);
        pnlContent.Controls.Add(pnlQrDetails);

        pnlBody.Controls.Add(pnlContent);
        pnlBody.Controls.Add(pnlOptions);

        var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 65, Padding = new Padding(25, 10, 25, 10), BackColor = Color.FromArgb(248, 250, 252) };
        var btnConfirm = UITheme.CreateFlatButton("✓ XÁC NHẬN THANH TOÁN", UITheme.PrimaryGold, Color.Black);
        btnConfirm.Dock = DockStyle.Fill;
        btnConfirm.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);

        btnConfirm.Click += (s, ev) =>
        {
            var req = new TaoHoaDonOfflineDTO
            {
                TenKhachHang = _txtCustName.Text.Trim(),
                SoDienThoai = _txtCustPhone.Text.Trim(),
                DiaChi = _txtCustAddress.Text.Trim(),
                GhiChu = _txtOrderNote.Text.Trim(),
                PhuongThucThanhToan = rdoCash.Checked ? "TienMat" : "ChuyenKhoan",
                DanhSachSanPham = _cartItems.ToList()
            };

            var ok = _services.BanHangOffline.Execute(req, out var maHoaDonMoi);
            if (ok)
            {
                MessageBox.Show(
                    $"Đã tạo hóa đơn [{maHoaDonMoi}] và ghi nhận thanh toán thành công!\nTrạng thái đơn: DaThanhToan.",
                    "Thanh toán hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                payForm.DialogResult = DialogResult.OK;
                payForm.Close();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi tạo hóa đơn. Vui lòng thử lại.", "Lỗi thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        pnlBottom.Controls.Add(btnConfirm);

        payForm.Controls.Add(pnlBody);
        payForm.Controls.Add(pnlBottom);
        payForm.Controls.Add(pnlTop);

        if (payForm.ShowDialog() == DialogResult.OK)
        {
            // Làm sạch sau khi thanh toán xong
            _cartItems.Clear();
            _txtCustName.Clear();
            _txtCustPhone.Clear();
            _txtCustAddress.Clear();
            _txtOrderNote.Clear();
            _lblCustomerStatus.Text = "(Chưa liên kết / Khách mới)";
            _lblCustomerStatus.ForeColor = UITheme.TextSecondary;
            RefreshCartGrid();
            LoadProductsList(); // Cập nhật lại tồn kho mới nhất
        }
    }

    private Image? LoadQrImage()
    {
        var candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRChuyenKhoan.jpg"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "QRChuyenKhoan.jpg"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "QRChuyenKhoan.jpg"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "QRChuyenKhoan.jpg"),
            Path.GetFullPath("QRChuyenKhoan.jpg")
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                try
                {
                    return Image.FromFile(path);
                }
                catch
                {
                    // Thử ứng viên tiếp theo nếu lỗi nạp
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Lớp hỗ trợ lưu cặp (Mã SP, Chuỗi hiển thị) trong ComboBox, ghi đè ToString để tránh lặp mã sản phẩm.
    /// </summary>
    private sealed class ProductComboItem
    {
        public string Key { get; }
        public string Value { get; }

        public ProductComboItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString() => Value;
    }
}
