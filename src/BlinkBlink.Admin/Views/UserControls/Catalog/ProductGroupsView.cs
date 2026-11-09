using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Domain.Entities.Catalog;

namespace BlinkBlink.Admin.Views.UserControls.Catalog;

/// <summary>
/// Màn hình Quản lý Nhóm Sản Phẩm (Gom nhóm các trang sức cùng mẫu khác size).
/// Thiết kế giao diện sang trọng chuẩn Blink Blink Jewelry theo Taste Skill.
/// </summary>
public class ProductGroupsView : UserControl
{
    private readonly AdminServiceScope _services;
    private List<NhomSanPhamDTO> _allGroups = new();
    private List<SanPhamTrangSuc> _allProducts = new();

    private TextBox _txtSearch = null!;
    private Button _btnAddGroup = null!;
    private Button _btnEditGroup = null!;
    private Button _btnRefresh = null!;

    private Label _lblKpiTotalGroups = null!;
    private Label _lblKpiGroupedProducts = null!;

    private DataGridView _dgvGroups = null!;

    public ProductGroupsView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadGroupData();
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
            Width = 460,
            Height = 85,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Nhóm Sản Phẩm", out _lblKpiTotalGroups, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Sản Phẩm Đã Gom Nhóm", out _lblKpiGroupedProducts, UITheme.PrimaryGold));

        var pnlTitleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 15, 0)
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ NHÓM SẢN PHẨM (GOM SIZE)",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Gom nhóm các sản phẩm trang sức cùng mẫu nhưng khác size, đồng bộ tên và hỗ trợ chọn size linh hoạt",
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

        // Toolbar Panel Responsive chống tràn chữ theo chuẩn UITheme
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
            Width = 260,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "Tên hoặc mã nhóm...",
            Margin = new Padding(0, 4, 15, 4)
        };
        _txtSearch.TextChanged += (s, e) => FilterGroups();

        _btnAddGroup = UITheme.CreateFlatButton("+ Thêm Nhóm Mới", UITheme.DarkHeader, Color.White);
        _btnAddGroup.Margin = new Padding(0, 2, 8, 2);
        _btnAddGroup.Click += BtnAddGroup_Click;

        _btnEditGroup = UITheme.CreateFlatButton("✏ Chỉnh Sửa Nhóm", UITheme.PrimaryGold, Color.Black);
        _btnEditGroup.Margin = new Padding(0, 2, 8, 2);
        _btnEditGroup.Click += BtnEditGroup_Click;

        _btnRefresh = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (s, e) => LoadGroupData();

        toolbarPanel.Controls.Add(lblSearch);
        toolbarPanel.Controls.Add(_txtSearch);
        toolbarPanel.Controls.Add(_btnAddGroup);
        toolbarPanel.Controls.Add(_btnEditGroup);
        toolbarPanel.Controls.Add(_btnRefresh);

        // DataGridView Panel nền sáng sang trọng
        var gridContainer = new Panel
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

        _dgvGroups = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvGroups);
        ConfigureGridColumns();
        _dgvGroups.CellDoubleClick += (s, e) => BtnEditGroup_Click(s, e);

        pnlWhiteCard.Controls.Add(_dgvGroups);
        gridContainer.Controls.Add(pnlWhiteCard);

        this.Controls.Add(gridContainer);
        this.Controls.Add(toolbarPanel);
        this.Controls.Add(topHeaderPanel);
    }

    private void ConfigureGridColumns()
    {
        _dgvGroups.Columns.Clear();

        var colMa = new DataGridViewTextBoxColumn
        {
            Name = "MaNhom",
            HeaderText = "MÃ NHÓM",
            Width = 140,
            DataPropertyName = "MaNhom"
        };
        colMa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colTen = new DataGridViewTextBoxColumn
        {
            Name = "TenNhom",
            HeaderText = "TÊN NHÓM SẢN PHẨM",
            Width = 320,
            DataPropertyName = "TenNhom"
        };
        colTen.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colTen.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colMoTa = new DataGridViewTextBoxColumn
        {
            Name = "MoTa",
            HeaderText = "MÔ TẢ",
            FillWeight = 300,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DataPropertyName = "MoTa"
        };
        colMoTa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMoTa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colSl = new DataGridViewTextBoxColumn
        {
            Name = "SoLuongSP",
            HeaderText = "SỐ LƯỢNG SP",
            Width = 150,
            DataPropertyName = "SoLuongSanPham",
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = UITheme.TabularNumericFont
            }
        };
        colSl.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _dgvGroups.Columns.AddRange(colMa, colTen, colMoTa, colSl);
    }

    private Panel CreateKpiCard(string title, out Label valueLabel, Color valueColor)
    {
        var card = new Panel
        {
            Width = 215,
            Height = 75,
            BackColor = Color.White,
            Margin = new Padding(5),
            Padding = new Padding(14, 10, 14, 8)
        };

        var lblTitle = new Label
        {
            Text = title,
            ForeColor = UITheme.TextSecondary,
            Font = new Font("Segoe UI", 8.5F),
            Dock = DockStyle.Top,
            Height = 20,
            UseMnemonic = false
        };

        valueLabel = new Label
        {
            Text = "0",
            ForeColor = valueColor,
            Font = UITheme.KpiNumberFont,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false
        };

        card.Controls.Add(valueLabel);
        card.Controls.Add(lblTitle);
        return card;
    }

    private Button CreateActionButton(string text, Color bg, Color fg)
    {
        var btn = new Button
        {
            Text = text,
            Height = 42,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = fg,
            BackColor = bg,
            Cursor = Cursors.Hand,
            UseMnemonic = false,
            Margin = new Padding(6, 0, 0, 0),
            Padding = new Padding(16, 0, 16, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public void LoadGroupData()
    {
        try
        {
            _allGroups = _services.QuanLyNhomSanPham.LayDanhSachNhom();
            _allProducts = _services.SanPhamRepo.LayTatCa();

            FilterGroups();

            _lblKpiTotalGroups.Text = _allGroups.Count.ToString("N0", CultureInfo.InvariantCulture);
            var totalGrouped = _allProducts.Count(p => !string.IsNullOrWhiteSpace(p.MaNhom) && p.MaNhom != "0");
            _lblKpiGroupedProducts.Text = totalGrouped.ToString("N0", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách nhóm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FilterGroups()
    {
        var keyword = _txtSearch.Text.Trim().ToLowerInvariant();
        var filtered = _allGroups.Where(g =>
            string.IsNullOrWhiteSpace(keyword) ||
            g.TenNhom.ToLowerInvariant().Contains(keyword) ||
            g.MaNhom.ToLowerInvariant().Contains(keyword) ||
            (g.MoTa != null && g.MoTa.ToLowerInvariant().Contains(keyword))
        ).ToList();

        _dgvGroups.DataSource = null;
        _dgvGroups.DataSource = filtered;
    }

    private void BtnAddGroup_Click(object? sender, EventArgs e)
    {
        using var dialog = new AddProductGroupModal(_services, _allProducts);
        if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
        {
            LoadGroupData();
        }
    }

    private void BtnEditGroup_Click(object? sender, EventArgs e)
    {
        if (_dgvGroups.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một nhóm sản phẩm để chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedGroup = _dgvGroups.SelectedRows[0].DataBoundItem as NhomSanPhamDTO;
        if (selectedGroup == null) return;

        using var dialog = new EditProductGroupModal(_services, selectedGroup.MaNhom);
        if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
        {
            LoadGroupData();
        }
    }
}

/// <summary>
/// Modal Thêm Nhóm Sản Phẩm Mới.
/// Cho phép nhập tên nhóm, có tính năng tìm kiếm sản phẩm có sẵn để gợi ý tên nhóm vào ô tên nhóm.
/// </summary>
public class AddProductGroupModal : Form
{
    private readonly AdminServiceScope _services;
    private readonly List<SanPhamTrangSuc> _allProducts;

    private TextBox _txtGroupName = null!;
    private TextBox _txtDescription = null!;
    private TextBox _txtSearchProduct = null!;
    private ComboBox _cboSuggestedProducts = null!;
    private Button _btnAdd = null!;
    private Button _btnCancel = null!;

    public AddProductGroupModal(AdminServiceScope services, List<SanPhamTrangSuc> allProducts)
    {
        _services = services;
        _allProducts = allProducts;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Thêm Nhóm Sản Phẩm Mới - Blink Blink Jewelry";
        this.Size = new Size(580, 520);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;
        this.ForeColor = UITheme.TextPrimary;
        this.Font = UITheme.RegularFont;

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 75,
            BackColor = UITheme.DarkHeader,
            Padding = new Padding(24, 14, 24, 10)
        };

        var lblTitle = new Label
        {
            Text = "THÊM MỚI NHÓM SẢN PHẨM (GOM SIZE)",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.PrimaryGold,
            Dock = DockStyle.Top,
            Height = 26,
            UseMnemonic = false
        };

        var lblSub = new Label
        {
            Text = "Gom các sản phẩm trang sức cùng mẫu nhưng khác size để tiện quản lý và đồng bộ",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Fill,
            UseMnemonic = false
        };

        pnlHeader.Controls.Add(lblSub);
        pnlHeader.Controls.Add(lblTitle);

        // Footer Actions
        var pnlFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = UITheme.CanvasBackground,
            Padding = new Padding(20, 12, 20, 12)
        };

        var flpBtns = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 260,
            Height = 40,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        _btnCancel = UITheme.CreateFlatButton("Hủy Bỏ", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        _btnCancel.Width = 95;
        _btnCancel.Margin = new Padding(8, 0, 0, 0);
        _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        _btnAdd = UITheme.CreateFlatButton("+ Thêm Nhóm", UITheme.PrimaryGold, Color.Black);
        _btnAdd.Width = 135;
        _btnAdd.Margin = new Padding(0);
        _btnAdd.Click += BtnAdd_Click;

        flpBtns.Controls.Add(_btnCancel);
        flpBtns.Controls.Add(_btnAdd);
        pnlFooter.Controls.Add(flpBtns);

        // Main Form Content
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(24, 15, 24, 15),
            BackColor = Color.White
        };

        // 1. Tên Nhóm
        var lblName = new Label
        {
            Text = "Tên Nhóm Sản Phẩm (*):",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Height = 22,
            Dock = DockStyle.Top
        };
        _txtGroupName = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = UITheme.RegularFont
        };

        // 2. Gợi ý từ sản phẩm có sẵn
        var lblSuggest = new Label
        {
            Text = "Gợi ý tên nhóm từ sản phẩm có sẵn (Search để tự điền tên nhóm):",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = UITheme.TextSecondary,
            Height = 20,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 8, 0, 0)
        };

        _txtSearchProduct = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 28,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = UITheme.RegularFont,
            PlaceholderText = "🔍 Nhập tên hoặc mã sản phẩm để tìm gợi ý..."
        };

        _cboSuggestedProducts = new ComboBox
        {
            Dock = DockStyle.Top,
            Height = 28,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 4, 0, 0)
        };

        _txtSearchProduct.TextChanged += (s, e) =>
        {
            var kw = _txtSearchProduct.Text.Trim().ToLowerInvariant();
            _cboSuggestedProducts.Items.Clear();
            if (!string.IsNullOrWhiteSpace(kw))
            {
                var matches = _allProducts.Where(p =>
                    p.TenSanPham.ToLowerInvariant().Contains(kw) ||
                    p.MaSanPham.ToLowerInvariant().Contains(kw)
                ).Take(15).ToList();

                foreach (var p in matches)
                {
                    _cboSuggestedProducts.Items.Add(p);
                }

                if (_cboSuggestedProducts.Items.Count > 0)
                {
                    _cboSuggestedProducts.SelectedIndex = 0;
                    var firstMatch = _cboSuggestedProducts.Items[0] as SanPhamTrangSuc;
                    if (firstMatch != null)
                    {
                        // Tên đầu tiên được đề xuất trong combobox sẽ thay đổi ô tên nhóm theo CONGVIECTIEPTHEO.md
                        _txtGroupName.Text = firstMatch.TenSanPham;
                    }
                }
            }
        };

        _cboSuggestedProducts.SelectedIndexChanged += (s, e) =>
        {
            if (_cboSuggestedProducts.SelectedItem is SanPhamTrangSuc sp)
            {
                _txtGroupName.Text = sp.TenSanPham;
            }
        };

        // 3. Mô tả
        var lblDesc = new Label
        {
            Text = "Mô Tả Nhóm:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Height = 22,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 8, 0, 0)
        };
        _txtDescription = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 65,
            Multiline = true,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = UITheme.RegularFont
        };

        mainPanel.Controls.Add(lblName);
        mainPanel.Controls.Add(_txtGroupName);
        mainPanel.Controls.Add(lblSuggest);
        mainPanel.Controls.Add(_txtSearchProduct);
        mainPanel.Controls.Add(_cboSuggestedProducts);
        mainPanel.Controls.Add(lblDesc);
        mainPanel.Controls.Add(_txtDescription);

        this.Controls.Add(mainPanel);
        this.Controls.Add(pnlFooter);
        this.Controls.Add(pnlHeader);
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var name = _txtGroupName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Vui lòng nhập tên nhóm sản phẩm.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtGroupName.Focus();
            return;
        }

        try
        {
            var dto = new TaoNhomDTO
            {
                TenNhom = name,
                MoTa = _txtDescription.Text.Trim()
            };

            var created = _services.QuanLyNhomSanPham.TaoNhom(dto);
            if (created != null)
            {
                MessageBox.Show($"Thêm nhóm '{created.TenNhom}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Không thể tạo nhóm sản phẩm. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tạo nhóm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

/// <summary>
/// Modal Chỉnh Sửa Nhóm Sản Phẩm.
/// Cung cấp:
/// - Đổi tên nhóm (đồng bộ tên toàn bộ sản phẩm trong nhóm khi lưu).
/// - Danh sách sản phẩm trong nhóm (hiển thị Mã SP, Size; có scroll nếu > 4 sản phẩm).
/// - Nút "Loại khỏi nhóm" (sáng lên khi chọn 1 SP).
/// - Ô search + ComboBox tìm sản phẩm để thêm vào nhóm (kèm Label Size: {size}).
/// - Chặn trùng size trong nhóm. Cảnh báo nếu SP thuộc nhóm khác. Xác nhận đổi tên SP khi thêm vào nhóm.
/// - Xóa nhóm (hỏi xác nhận, đặt toàn bộ SP về MaNhom = "0").
/// </summary>
public class EditProductGroupModal : Form
{
    private readonly AdminServiceScope _services;
    private readonly string _maNhom;
    private NhomSanPham? _group;
    private List<SanPhamTrangSuc> _productsInGroup = new();
    private List<SanPhamTrangSuc> _allProducts = new();

    private TextBox _txtGroupName = null!;
    private TextBox _txtDescription = null!;
    private DataGridView _dgvProductsInGroup = null!;
    private Button _btnRemoveFromGroup = null!;

    private TextBox _txtSearchToAdd = null!;
    private ComboBox _cboProductToAdd = null!;
    private Label _lblProductSize = null!;
    private Button _btnAddProductToGroup = null!;

    private Button _btnSave = null!;
    private Button _btnDeleteGroup = null!;
    private Button _btnCancel = null!;

    public EditProductGroupModal(AdminServiceScope services, string maNhom)
    {
        _services = services;
        _maNhom = maNhom;
        InitializeComponents();
        LoadGroupData();
    }

    private void InitializeComponents()
    {
        this.Text = "Chỉnh Sửa Nhóm Sản Phẩm - Blink Blink Jewelry";
        this.Size = new Size(650, 720);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;
        this.ForeColor = UITheme.TextPrimary;
        this.Font = UITheme.RegularFont;

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 75,
            BackColor = UITheme.DarkHeader,
            Padding = new Padding(24, 14, 24, 10)
        };

        var lblTitle = new Label
        {
            Text = "CHỈNH SỬA NHÓM SẢN PHẨM (GOM SIZE)",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.PrimaryGold,
            Dock = DockStyle.Top,
            Height = 26,
            UseMnemonic = false
        };

        var lblSub = new Label
        {
            Text = "Đổi tên nhóm, thêm/bớt size sản phẩm trong nhóm, đồng bộ thuộc tính mẫu mã",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Fill,
            UseMnemonic = false
        };

        pnlHeader.Controls.Add(lblSub);
        pnlHeader.Controls.Add(lblTitle);

        var pnlContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 15, 24, 15),
            AutoScroll = true,
            BackColor = Color.White
        };

        // 1. Tên Nhóm & Mô tả
        var lblName = new Label
        {
            Text = "Tên Nhóm Sản Phẩm (*):",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Top = 15,
            Left = 20,
            Width = 580,
            Height = 20
        };
        _txtGroupName = new TextBox
        {
            Top = 38,
            Left = 20,
            Width = 580,
            Height = 30,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = UITheme.RegularFont
        };

        var lblDesc = new Label
        {
            Text = "Mô Tả Nhóm:",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = UITheme.TextSecondary,
            Top = 75,
            Left = 20,
            Width = 580,
            Height = 18
        };
        _txtDescription = new TextBox
        {
            Top = 95,
            Left = 20,
            Width = 580,
            Height = 45,
            Multiline = true,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = UITheme.RegularFont
        };

        // 2. Danh sách sản phẩm trong nhóm (Hiện Mã SP & Size, có scrollbar nếu dài)
        var lblList = new Label
        {
            Text = "Danh Sách Sản Phẩm Trong Nhóm (Mã SP và Size):",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Top = 150,
            Left = 20,
            Width = 580,
            Height = 20
        };

        var pnlGridWrapper = new Panel
        {
            Top = 175,
            Left = 20,
            Width = 580,
            Height = 145,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };

        _dgvProductsInGroup = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvProductsInGroup);
        _dgvProductsInGroup.ScrollBars = ScrollBars.Vertical;
        _dgvProductsInGroup.RowTemplate.Height = 32;

        _dgvProductsInGroup.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", Width = 230, DataPropertyName = "MaSanPham" },
            new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "SIZE", Width = 320, DataPropertyName = "Size", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
        });

        pnlGridWrapper.Controls.Add(_dgvProductsInGroup);

        // Nút Loại khỏi nhóm (sáng lên khi chọn 1 SP)
        _btnRemoveFromGroup = UITheme.CreateFlatButton("➖ Loại Khỏi Nhóm", Color.FromArgb(241, 245, 249), Color.FromArgb(148, 163, 184));
        _btnRemoveFromGroup.Top = 326;
        _btnRemoveFromGroup.Left = 20;
        _btnRemoveFromGroup.Width = 160;
        _btnRemoveFromGroup.Height = 34;
        _btnRemoveFromGroup.Enabled = false;
        _btnRemoveFromGroup.Click += BtnRemoveFromGroup_Click;

        _dgvProductsInGroup.SelectionChanged += (s, e) =>
        {
            _btnRemoveFromGroup.Enabled = _dgvProductsInGroup.SelectedRows.Count > 0;
            if (_btnRemoveFromGroup.Enabled)
            {
                _btnRemoveFromGroup.BackColor = Color.FromArgb(220, 38, 38);
                _btnRemoveFromGroup.ForeColor = Color.White;
            }
            else
            {
                _btnRemoveFromGroup.BackColor = Color.FromArgb(241, 245, 249);
                _btnRemoveFromGroup.ForeColor = Color.FromArgb(148, 163, 184);
            }
        };

        // 3. Phần Thêm Sản Phẩm Vào Nhóm (Search + Combobox + Label Size + Nút Thêm)
        var lblAddSection = new Label
        {
            Text = "Thêm Sản Phẩm Khác Vào Nhóm:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Top = 370,
            Left = 20,
            Width = 580,
            Height = 20
        };

        _txtSearchToAdd = new TextBox
        {
            Top = 394,
            Left = 20,
            Width = 580,
            Height = 28,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = UITheme.RegularFont,
            PlaceholderText = "🔍 Tìm sản phẩm cần thêm vào nhóm theo tên hoặc mã SP..."
        };

        _cboProductToAdd = new ComboBox
        {
            Top = 428,
            Left = 20,
            Width = 580,
            Height = 28,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            Font = UITheme.RegularFont
        };

        _lblProductSize = new Label
        {
            Text = "Size: (chưa chọn sản phẩm)",
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = UITheme.TextSecondary,
            Top = 466,
            Left = 20,
            Width = 380,
            Height = 25
        };

        _btnAddProductToGroup = UITheme.CreateFlatButton("+ Thêm Vào Nhóm", UITheme.PrimaryGold, Color.Black);
        _btnAddProductToGroup.Top = 462;
        _btnAddProductToGroup.Left = 410;
        _btnAddProductToGroup.Width = 190;
        _btnAddProductToGroup.Height = 36;
        _btnAddProductToGroup.Click += BtnAddProductToGroup_Click;

        _txtSearchToAdd.TextChanged += (s, e) => FilterProductsToAdd();
        _cboProductToAdd.SelectedIndexChanged += (s, e) =>
        {
            if (_cboProductToAdd.SelectedItem is SanPhamTrangSuc sp)
            {
                _lblProductSize.Text = $"Size: {sp.Size}";
            }
            else
            {
                _lblProductSize.Text = "Size: (chưa chọn sản phẩm)";
            }
        };

        // 4. Bottom Action Bar: Xóa Nhóm (Trái), Lưu Thay Đổi & Hủy (Phải)
        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = UITheme.CanvasBackground,
            Padding = new Padding(20, 12, 20, 12)
        };

        _btnDeleteGroup = UITheme.CreateFlatButton("🗑 Xóa Nhóm", Color.FromArgb(220, 38, 38), Color.White);
        _btnDeleteGroup.Dock = DockStyle.Left;
        _btnDeleteGroup.Width = 130;
        _btnDeleteGroup.Height = 40;
        _btnDeleteGroup.Click += BtnDeleteGroup_Click;

        var pnlRightButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 280,
            Height = 40,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        _btnCancel = UITheme.CreateFlatButton("Đóng", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        _btnCancel.Width = 90;
        _btnCancel.Height = 40;
        _btnCancel.Margin = new Padding(8, 0, 0, 0);
        _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        _btnSave = UITheme.CreateFlatButton("💾 Lưu Thay Đổi", UITheme.PrimaryGold, Color.Black);
        _btnSave.Width = 150;
        _btnSave.Height = 40;
        _btnSave.Margin = new Padding(0);
        _btnSave.Click += BtnSave_Click;

        pnlRightButtons.Controls.Add(_btnCancel);
        pnlRightButtons.Controls.Add(_btnSave);

        bottomBar.Controls.Add(_btnDeleteGroup);
        bottomBar.Controls.Add(pnlRightButtons);

        // Add to Container
        pnlContainer.Controls.Add(lblName);
        pnlContainer.Controls.Add(_txtGroupName);
        pnlContainer.Controls.Add(lblDesc);
        pnlContainer.Controls.Add(_txtDescription);
        pnlContainer.Controls.Add(lblList);
        pnlContainer.Controls.Add(pnlGridWrapper);
        pnlContainer.Controls.Add(_btnRemoveFromGroup);
        pnlContainer.Controls.Add(lblAddSection);
        pnlContainer.Controls.Add(_txtSearchToAdd);
        pnlContainer.Controls.Add(_cboProductToAdd);
        pnlContainer.Controls.Add(_lblProductSize);
        pnlContainer.Controls.Add(_btnAddProductToGroup);

        this.Controls.Add(pnlContainer);
        this.Controls.Add(bottomBar);
        this.Controls.Add(pnlHeader);
    }

    private void LoadGroupData()
    {
        _group = _services.NhomSanPhamRepo.TimTheoId(_maNhom);
        if (_group == null)
        {
            MessageBox.Show("Không tìm thấy thông tin nhóm sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }

        _txtGroupName.Text = _group.TenNhom;
        _txtDescription.Text = _group.MoTa ?? string.Empty;

        RefreshProductsInGroup();
        LoadAllCandidateProducts();
    }

    private void RefreshProductsInGroup()
    {
        _productsInGroup = _services.QuanLyNhomSanPham.LayDanhSachSanPhamTrongNhom(_maNhom);
        _dgvProductsInGroup.DataSource = null;
        _dgvProductsInGroup.DataSource = _productsInGroup;
        _btnRemoveFromGroup.Enabled = false;
        _btnRemoveFromGroup.BackColor = Color.FromArgb(241, 245, 249);
        _btnRemoveFromGroup.ForeColor = Color.FromArgb(148, 163, 184);
    }

    private void LoadAllCandidateProducts()
    {
        _allProducts = _services.SanPhamRepo.LayTatCa();
        FilterProductsToAdd();
    }

    private void FilterProductsToAdd()
    {
        var kw = _txtSearchToAdd.Text.Trim().ToLowerInvariant();
        _cboProductToAdd.Items.Clear();

        // Loại trừ các sản phẩm đã có trong nhóm hiện tại
        var existingIds = new HashSet<string>(_productsInGroup.Select(p => p.MaSanPham));

        var candidates = _allProducts.Where(p => !existingIds.Contains(p.MaSanPham));

        if (!string.IsNullOrWhiteSpace(kw))
        {
            candidates = candidates.Where(p =>
                p.TenSanPham.ToLowerInvariant().Contains(kw) ||
                p.MaSanPham.ToLowerInvariant().Contains(kw)
            );
        }

        var list = candidates.Take(25).ToList();
        foreach (var item in list)
        {
            _cboProductToAdd.Items.Add(item);
        }

        if (_cboProductToAdd.Items.Count > 0)
        {
            _cboProductToAdd.SelectedIndex = 0;
            if (_cboProductToAdd.SelectedItem is SanPhamTrangSuc sp)
            {
                _lblProductSize.Text = $"Size: {sp.Size}";
            }
        }
        else
        {
            _lblProductSize.Text = "Size: (không tìm thấy sản phẩm)";
        }
    }

    private void BtnRemoveFromGroup_Click(object? sender, EventArgs e)
    {
        if (_dgvProductsInGroup.SelectedRows.Count == 0) return;

        var selectedSp = _dgvProductsInGroup.SelectedRows[0].DataBoundItem as SanPhamTrangSuc;
        if (selectedSp == null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn loại sản phẩm [{selectedSp.MaSanPham}] khỏi nhóm?",
            "Xác nhận loại khỏi nhóm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm == DialogResult.Yes)
        {
            _services.QuanLyNhomSanPham.LoaiSanPhamKhoiNhom(selectedSp.MaSanPham);
            RefreshProductsInGroup();
            FilterProductsToAdd();
        }
    }

    private void BtnAddProductToGroup_Click(object? sender, EventArgs e)
    {
        if (_cboProductToAdd.SelectedItem is not SanPhamTrangSuc targetSp)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm cần thêm vào nhóm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Kiểm tra xem sản phẩm có thuộc nhóm khác không
        if (!string.IsNullOrWhiteSpace(targetSp.MaNhom) && targetSp.MaNhom != "0" && targetSp.MaNhom != _maNhom)
        {
            var otherGroup = _services.NhomSanPhamRepo.TimTheoId(targetSp.MaNhom);
            var otherGroupName = otherGroup?.TenNhom ?? targetSp.MaNhom;

            MessageBox.Show(
                $"Sản phẩm này đang thuộc về nhóm '{otherGroupName}', vui lòng hủy nhóm của sản phẩm trước khi thêm.",
                "Cảnh báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            // Làm trống ô search và combobox theo đúng yêu cầu CONGVIECTIEPTHEO.md
            _txtSearchToAdd.Text = string.Empty;
            _cboProductToAdd.Items.Clear();
            _lblProductSize.Text = "Size: (chưa chọn sản phẩm)";
            return;
        }

        // Ràng buộc trùng Size: Không cho phép trùng size trong nhóm
        var normalizedSize = targetSp.Size?.Trim().ToLowerInvariant() ?? string.Empty;
        var isDuplicateSize = _productsInGroup.Any(p =>
            p.MaSanPham != targetSp.MaSanPham &&
            string.Equals(p.Size?.Trim().ToLowerInvariant(), normalizedSize, StringComparison.OrdinalIgnoreCase));

        if (isDuplicateSize)
        {
            MessageBox.Show(
                $"Size '{targetSp.Size}' của sản phẩm đã bị trùng với sản phẩm khác trong nhóm. Trong nhóm không cho phép các sản phẩm trùng size.",
                "Trùng Size",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        // Tiến hành thêm vào nhóm (giữ nguyên tên gốc của sản phẩm, không đổi theo tên nhóm)
        var success = _services.QuanLyNhomSanPham.ThemSanPhamVaoNhom(_maNhom, targetSp.MaSanPham, out var error);
        if (success)
        {
            RefreshProductsInGroup();
            FilterProductsToAdd();
        }
        else
        {
            MessageBox.Show(error, "Lỗi thêm vào nhóm", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var newName = _txtGroupName.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            MessageBox.Show("Tên nhóm không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtGroupName.Focus();
            return;
        }

        try
        {
            _services.QuanLyNhomSanPham.CapNhatTenNhom(_maNhom, newName, _txtDescription.Text.Trim());
            MessageBox.Show("Cập nhật nhóm sản phẩm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDeleteGroup_Click(object? sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa nhóm này?\nKhi xóa nhóm, toàn bộ sản phẩm trong nhóm sẽ được đặt lại mã nhóm thành 0 (mặc định chưa gom nhóm).",
            "Xác nhận xóa nhóm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (confirm == DialogResult.Yes)
        {
            try
            {
                _services.QuanLyNhomSanPham.XoaNhom(_maNhom);
                MessageBox.Show("Đã xóa nhóm sản phẩm thành công. Tất cả sản phẩm trong nhóm đã chuyển về mã nhóm 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa nhóm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

