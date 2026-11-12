using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Domain.Entities.Orders;

namespace BlinkBlink.Admin.Views.UserControls.Orders;

/// <summary>
/// Màn hình Quản lý Bán hàng & Phê duyệt đơn hàng dành cho Nhân viên và Quản trị viên.
/// Thực thi các nghiệp vụ: Phê duyệt đơn đặt hàng online, chuyển trạng thái giao hàng, hủy đơn và hoàn kho.
/// </summary>
public class OrdersView : UserControl
{
    private readonly AdminServiceScope _services;
    private List<HoaDon> _allOrders = new();

    private TextBox _txtSearch = null!;
    private ComboBox _cboOrderSource = null!;
    private ComboBox _cboStatus = null!;
    private Button _btnApprove = null!;
    private Button _btnShipping = null!;
    private Button _btnDelivered = null!;
    private Button _btnCancel = null!;
    private Button _btnRefresh = null!;

    private Label _lblKpiTotal = null!;
    private Label _lblKpiPending = null!;
    private Label _lblKpiDelivered = null!;

    private DataGridView _dgvOrders = null!;
    private DataGridView _dgvOrderItems = null!;
    private Label _lblDetailTitle = null!;

    public OrdersView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadOrderData();
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // Panel tiêu đề và thẻ chỉ số KPI tóm tắt
        var topHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(20, 15, 20, 10),
            BackColor = UITheme.CanvasBackground
        };

        // Khối 3 thẻ chỉ số nhanh
        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 520,
            Height = 80,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Đơn", out _lblKpiTotal, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Chờ Duyệt Online", out _lblKpiPending, UITheme.WarningColor));
        pnlKpis.Controls.Add(CreateKpiCard("Đã Giao / Hoàn Tất", out _lblKpiDelivered, UITheme.SuccessColor));

        var pnlTitleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 15, 0)
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ ĐƠN HÀNG",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Quản lý tập trung toàn bộ đơn hàng Online và Offline tại quầy, theo dõi trạng thái và kiểm soát luồng giao nhận",
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

        // Thanh công cụ tác vụ và tìm kiếm dạng Responsive chống tràn chữ
        var toolbarPanel = UITheme.CreateResponsiveToolbar();

        var lblSearch = new Label
        {
            Text = "Tìm kiếm:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 4, 0)
        };

        _txtSearch = new TextBox
        {
            Width = 150,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "Mã đơn, SĐT, Khách...",
            Margin = new Padding(0, 4, 10, 4)
        };
        _txtSearch.TextChanged += (s, e) => FilterOrders();

        var lblSource = new Label
        {
            Text = "Nguồn đơn:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 4, 0)
        };

        _cboOrderSource = new ComboBox
        {
            Width = 145,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 4, 10, 4)
        };
        _cboOrderSource.Items.AddRange(new object[] { "Tất Cả Nguồn Đơn", "🌐 Đơn Online", "🏬 Đơn Offline (Quầy)" });
        _cboOrderSource.SelectedIndex = 0;
        _cboOrderSource.SelectedIndexChanged += (s, e) => FilterOrders();

        var lblStatusFilter = new Label
        {
            Text = "Trạng thái:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 4, 0)
        };

        _cboStatus = new ComboBox
        {
            Width = 135,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 4, 10, 4)
        };
        _cboStatus.Items.AddRange(new object[] { "Tất Cả Trạng Thái", "ChoDuyet", "DangDongGoi", "DangGiao", "DaGiao", "DaThanhToan", "DaHuy" });
        _cboStatus.SelectedIndex = 0;
        _cboStatus.SelectedIndexChanged += (s, e) => FilterOrders();

        // Nút hành động
        _btnApprove = UITheme.CreateFlatButton("✓ Duyệt Đơn", UITheme.PrimaryGold, Color.Black);
        _btnApprove.Margin = new Padding(0, 2, 6, 2);
        _btnApprove.Click += BtnApprove_Click;

        _btnShipping = UITheme.CreateFlatButton("🚚 Đang Giao", UITheme.DarkHeader, Color.White);
        _btnShipping.ForeColor = Color.White;
        _btnShipping.Margin = new Padding(0, 2, 6, 2);
        _btnShipping.Click += (s, e) => UpdateOrderStatus("DangGiao");

        // Đảm bảo chữ và biểu tượng của nút Đang Giao luôn luôn hiển thị màu trắng rõ nét trên nền tối trong mọi trạng thái
        _btnShipping.Paint += (s, e) =>
        {
            var btn = (Button)s!;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bg = btn.Enabled
                ? (btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)) ? Color.FromArgb(45, 55, 72) : UITheme.DarkHeader)
                : Color.FromArgb(40, 48, 62);

            using (var bgBrush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(bgBrush, btn.ClientRectangle);
            }

            var textColor = btn.Enabled ? Color.White : Color.FromArgb(240, 245, 255);
            using (var textBrush = new SolidBrush(textColor))
            {
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(btn.Text, btn.Font, textBrush, btn.ClientRectangle, sf);
            }
        };
        _btnShipping.EnabledChanged += (s, e) => _btnShipping.Invalidate();

        _btnDelivered = UITheme.CreateFlatButton("★ Đã Giao", UITheme.SuccessColor, Color.White);
        _btnDelivered.Margin = new Padding(0, 2, 6, 2);
        _btnDelivered.Click += (s, e) => UpdateOrderStatus("DaGiao");

        _btnCancel = UITheme.CreateFlatButton("✕ Hủy Đơn", UITheme.DangerColor, Color.White);
        _btnCancel.Margin = new Padding(0, 2, 6, 2);
        _btnCancel.Click += BtnCancel_Click;

        _btnRefresh = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (s, e) => LoadOrderData();

        toolbarPanel.Controls.Add(lblSearch);
        toolbarPanel.Controls.Add(_txtSearch);
        toolbarPanel.Controls.Add(lblSource);
        toolbarPanel.Controls.Add(_cboOrderSource);
        toolbarPanel.Controls.Add(lblStatusFilter);
        toolbarPanel.Controls.Add(_cboStatus);
        toolbarPanel.Controls.Add(_btnApprove);
        toolbarPanel.Controls.Add(_btnShipping);
        toolbarPanel.Controls.Add(_btnDelivered);
        toolbarPanel.Controls.Add(_btnCancel);
        toolbarPanel.Controls.Add(_btnRefresh);

        // Vùng hiển thị phân chia (Split Container)
        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 8,
            BackColor = UITheme.CanvasBackground,
            Padding = new Padding(20, 10, 20, 10)
        };

        // Tự động căn chỉnh khoảng cách phân chia an toàn sau khi UserControl đã có kích thước thực tế
        void AdjustSplitter()
        {
            if (splitContainer.Height > 300)
            {
                try
                {
                    var min1 = Math.Min(160, splitContainer.Height / 3);
                    var min2 = Math.Min(140, splitContainer.Height / 3);
                    splitContainer.Panel1MinSize = min1;
                    splitContainer.Panel2MinSize = min2;

                    var target = (int)(splitContainer.Height * 0.55);
                    if (target >= min1 && target <= splitContainer.Height - min2)
                    {
                        splitContainer.SplitterDistance = target;
                    }
                }
                catch
                {
                    // Bỏ qua ngoại lệ kích thước tạm thời khi cửa sổ đang co giãn
                }
            }
        }

        this.Load += (s, e) => AdjustSplitter();
        this.Resize += (s, e) => AdjustSplitter();

        // Bảng danh sách đơn hàng
        _dgvOrders = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvOrders);
        ConfigureOrdersGrid();
        _dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

        var pnlOrdersGrid = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        pnlOrdersGrid.Controls.Add(_dgvOrders);
        splitContainer.Panel1.Controls.Add(pnlOrdersGrid);

        // Vùng chi tiết đơn hàng
        var pnlDetailContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };

        _lblDetailTitle = new Label
        {
            Text = "DANH SÁCH MẶT HÀNG TRANG SỨC TRONG ĐƠN",
            Font = UITheme.SubHeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            UseMnemonic = false
        };

        _dgvOrderItems = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvOrderItems);
        ConfigureOrderItemsGrid();

        pnlDetailContainer.Controls.Add(_dgvOrderItems);
        pnlDetailContainer.Controls.Add(_lblDetailTitle);
        _lblDetailTitle.SendToBack();
        _dgvOrderItems.BringToFront();
        splitContainer.Panel2.Controls.Add(pnlDetailContainer);

        this.Controls.Add(splitContainer);
        this.Controls.Add(toolbarPanel);
        this.Controls.Add(topHeaderPanel);
    }

    private Panel CreateKpiCard(string title, out Label lblValue, Color valueColor)
    {
        var card = new Panel
        {
            Width = 160,
            Height = 75,
            BackColor = Color.White,
            Margin = new Padding(5, 5, 5, 5),
            Padding = new Padding(10, 8, 10, 8)
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

    private void ConfigureOrdersGrid()
    {
        // 1. Tắt tự động co giãn để cho phép đặt chiều cao cố định
        _dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        // 2. Chỉnh chiều cao mong muốn
        _dgvOrders.ColumnHeadersHeight = 38;
        _dgvOrders.ScrollBars = ScrollBars.Both; // Kích hoạt thanh cuộn khi > 4 dòng
        _dgvOrders.Columns.Clear();
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaHoaDon", HeaderText = "MÃ ĐƠN HÀNG", FillWeight = 95, MinimumWidth = 110 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgayLap", HeaderText = "NGÀY LẬP", FillWeight = 90, MinimumWidth = 95 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaKhachHang", HeaderText = "KHÁCH HÀNG", FillWeight = 80, MinimumWidth = 90 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoDienThoai", HeaderText = "SỐ ĐT NHẬN", FillWeight = 90, MinimumWidth = 105 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiaChi", HeaderText = "ĐỊA CHỈ GIAO HÀNG", FillWeight = 160, MinimumWidth = 180 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThanhToan", HeaderText = "PHƯƠNG THỨC", FillWeight = 85, MinimumWidth = 100 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongHang", HeaderText = "TIỀN HÀNG (VND)", FillWeight = 100, MinimumWidth = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiamGia", HeaderText = "GIẢM GIÁ (VND)", FillWeight = 85, MinimumWidth = 105, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongCong", HeaderText = "TỔNG TT (VND)", FillWeight = 115, MinimumWidth = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 85, MinimumWidth = 100 });
    }

    private void ConfigureOrderItemsGrid()
    {
        _dgvOrderItems.ScrollBars = ScrollBars.Both; // Kích hoạt thanh cuộn khi > 4 mặt hàng
        _dgvOrderItems.Columns.Clear();
        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 90, MinimumWidth = 120 });
        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "SỐ LƯỢNG", FillWeight = 60, MinimumWidth = 85, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonGia", HeaderText = "ĐƠN GIÁ (VND)", FillWeight = 100, MinimumWidth = 130, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } });
        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiamGia", HeaderText = "% GIẢM", FillWeight = 60, MinimumWidth = 75, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "THÀNH TIỀN (VND)", FillWeight = 110, MinimumWidth = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });
    }

    private void LoadOrderData()
    {
        _allOrders = _services.HoaDonRepo.LayDanhSachDonHang();
        UpdateKpis();
        FilterOrders();
    }

    private void UpdateKpis()
    {
        var viVn = new CultureInfo("vi-VN");
        _lblKpiTotal.Text = _allOrders.Count.ToString("N0", viVn);
        _lblKpiPending.Text = _allOrders.Count(o => string.Equals(o.TrangThaiDonHang, "ChoDuyet", StringComparison.OrdinalIgnoreCase)).ToString("N0", viVn);
        _lblKpiDelivered.Text = _allOrders.Count(o => string.Equals(o.TrangThaiDonHang, "DaGiao", StringComparison.OrdinalIgnoreCase) || string.Equals(o.TrangThaiDonHang, "DaThanhToan", StringComparison.OrdinalIgnoreCase)).ToString("N0", viVn);
    }

    private void FilterOrders()
    {
        var query = _txtSearch.Text.Trim().ToLowerInvariant();
        var selectedSource = _cboOrderSource.SelectedIndex; // 0: Tất cả, 1: Online, 2: Offline
        var selectedStatus = _cboStatus.SelectedItem?.ToString() ?? "Tất Cả Trạng Thái";

        var filtered = _allOrders.Where(o =>
        {
            var matchSearch = string.IsNullOrEmpty(query) ||
                              o.MaHoaDon.ToLowerInvariant().Contains(query) ||
                              o.SoDienThoaiNhan.Contains(query) ||
                              o.MaKhachHang.ToLowerInvariant().Contains(query) ||
                              o.DiaChiGiaoHang.ToLowerInvariant().Contains(query);

            var matchSource = true;
            if (selectedSource == 1) // Online
            {
                matchSource = o.MaHoaDon.StartsWith("HD_ONLINE_", StringComparison.OrdinalIgnoreCase);
            }
            else if (selectedSource == 2) // Offline
            {
                matchSource = o.MaHoaDon.StartsWith("HD_OFFLINE_", StringComparison.OrdinalIgnoreCase);
            }

            var matchStatus = selectedStatus == "Tất Cả Trạng Thái" ||
                              string.Equals(o.TrangThaiDonHang, selectedStatus, StringComparison.OrdinalIgnoreCase);

            return matchSearch && matchSource && matchStatus;
        }).ToList();

        _dgvOrders.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");

        foreach (var hd in filtered)
        {
            var rowIdx = _dgvOrders.Rows.Add(
                hd.MaHoaDon,
                hd.NgayLap.ToString("dd/MM/yyyy HH:mm"),
                hd.MaKhachHang,
                hd.SoDienThoaiNhan,
                hd.DiaChiGiaoHang,
                hd.PhuongThucThanhToan,
                hd.TongTienHang.ToString("N0", viVn),
                hd.TienGiamGia.ToString("N0", viVn),
                hd.TongThanhToan.ToString("N0", viVn),
                hd.TrangThaiDonHang
            );

            // Gán màu sắc trực quan cho trạng thái
            var cellStatus = _dgvOrders.Rows[rowIdx].Cells["TrangThai"];
            if (string.Equals(hd.TrangThaiDonHang, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
                cellStatus.Style.ForeColor = UITheme.WarningColor;
            else if (string.Equals(hd.TrangThaiDonHang, "DaGiao", StringComparison.OrdinalIgnoreCase) || string.Equals(hd.TrangThaiDonHang, "DaThanhToan", StringComparison.OrdinalIgnoreCase))
                cellStatus.Style.ForeColor = UITheme.SuccessColor;
            else if (string.Equals(hd.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase))
                cellStatus.Style.ForeColor = UITheme.DangerColor;
        }

        if (_dgvOrders.Rows.Count > 0)
        {
            _dgvOrders.Rows[0].Selected = true;
            ShowSelectedOrderDetails();
        }
        else
        {
            _dgvOrderItems.Rows.Clear();
            _lblDetailTitle.Text = "DANH SÁCH MẶT HÀNG TRANG SỨC (Chưa chọn đơn)";
            _btnApprove.Enabled = false;
            _btnShipping.Enabled = false;
            _btnDelivered.Enabled = false;
            _btnCancel.Enabled = false;
        }
    }

    private void DgvOrders_SelectionChanged(object? sender, EventArgs e)
    {
        ShowSelectedOrderDetails();
    }

    private void ShowSelectedOrderDetails()
    {
        if (_dgvOrders.SelectedRows.Count == 0) return;

        var maHoaDon = _dgvOrders.SelectedRows[0].Cells["MaHoaDon"].Value?.ToString();
        if (string.IsNullOrEmpty(maHoaDon)) return;

        var hd = _allOrders.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
        if (hd == null) return;

        bool isOffline = hd.MaHoaDon.StartsWith("HD_OFFLINE_", StringComparison.OrdinalIgnoreCase);

        // Với đơn Offline: Vô hiệu hóa các nút Duyệt đơn, Đang giao, Đã giao, Hủy đơn
        if (isOffline)
        {
            _btnApprove.Enabled = false;
            _btnShipping.Enabled = false;
            _btnDelivered.Enabled = false;
            _btnCancel.Enabled = false;
            _lblDetailTitle.Text = $"DANH SÁCH MẶT HÀNG TRANG SỨC - ĐƠN QUẦY #{hd.MaHoaDon} (Đã thanh toán tại quầy)";
        }
        else
        {
            _btnApprove.Enabled = string.Equals(hd.TrangThaiDonHang, "ChoDuyet", StringComparison.OrdinalIgnoreCase);
            _btnShipping.Enabled = string.Equals(hd.TrangThaiDonHang, "DangDongGoi", StringComparison.OrdinalIgnoreCase);
            _btnDelivered.Enabled = string.Equals(hd.TrangThaiDonHang, "DangGiao", StringComparison.OrdinalIgnoreCase);
            _btnCancel.Enabled = !string.Equals(hd.TrangThaiDonHang, "DaGiao", StringComparison.OrdinalIgnoreCase) && !string.Equals(hd.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase);
            _lblDetailTitle.Text = $"DANH SÁCH MẶT HÀNG TRANG SỨC - ĐƠN ONLINE #{hd.MaHoaDon} ({hd.TrangThaiDonHang})";
        }

        _dgvOrderItems.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");
        if (hd.ChiTietHoaDons != null)
        {
            foreach (var item in hd.ChiTietHoaDons)
            {
                _dgvOrderItems.Rows.Add(
                    item.MaSanPham,
                    item.SoLuong.ToString("N0", viVn),
                    item.DonGia.ToString("N0", viVn),
                    item.PhanTramGiamGia + "%",
                    item.TinhThanhTien().ToString("N0", viVn)
                );
            }
        }
    }

    private void BtnApprove_Click(object? sender, EventArgs e)
    {
        if (_dgvOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn đơn hàng cần duyệt trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maHoaDon = _dgvOrders.SelectedRows[0].Cells["MaHoaDon"].Value?.ToString();
        if (string.IsNullOrEmpty(maHoaDon)) return;

        if (maHoaDon.StartsWith("HD_OFFLINE_", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Đơn hàng offline được lập và thanh toán trực tiếp tại quầy, không thể thực hiện phê duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var hd = _allOrders.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
        if (hd == null) return;

        if (!string.Equals(hd.TrangThaiDonHang, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show($"Đơn hàng #{maHoaDon} đang ở trạng thái '{hd.TrangThaiDonHang}', chỉ có thể duyệt đơn 'ChoDuyet'.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Thực thi Use Case phê duyệt đơn
            var success = _services.XuLyDonHang.DuyetDonHang(maHoaDon, "USER_NV001");
            if (success)
            {
                MessageBox.Show($"Đã phê duyệt thành công đơn hàng #{maHoaDon}. Trạng thái đã chuyển sang: DangDongGoi.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadOrderData();
            }
            else
            {
                MessageBox.Show("Không thể duyệt đơn hàng này do lỗi ràng buộc nghiệp vụ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            AdminErrorHandler.Handle(ex, $"{nameof(OrdersView)}.DuyetDonHang({maHoaDon})");
        }
    }

    private void UpdateOrderStatus(string newStatus)
    {
        if (_dgvOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn đơn hàng cần cập nhật trạng thái.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maHoaDon = _dgvOrders.SelectedRows[0].Cells["MaHoaDon"].Value?.ToString();
        if (string.IsNullOrEmpty(maHoaDon)) return;

        if (maHoaDon.StartsWith("HD_OFFLINE_", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Đơn hàng offline không hỗ trợ chuyển đổi trạng thái giao nhận trực tuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var success = _services.XuLyDonHang.CapNhatTrangThai(maHoaDon, newStatus, "USER_NV001");
            if (success)
            {
                MessageBox.Show($"Đã cập nhật trạng thái đơn #{maHoaDon} sang '{newStatus}'.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadOrderData();
            }
            else
            {
                MessageBox.Show("Không thể cập nhật trạng thái đơn hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            AdminErrorHandler.Handle(ex, $"{nameof(OrdersView)}.CapNhatTrangThai({maHoaDon})");
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        if (_dgvOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn đơn hàng muốn hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maHoaDon = _dgvOrders.SelectedRows[0].Cells["MaHoaDon"].Value?.ToString();
        if (string.IsNullOrEmpty(maHoaDon)) return;

        if (maHoaDon.StartsWith("HD_OFFLINE_", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Đơn hàng offline đã thanh toán tại quầy không thể hủy qua màn hình này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show($"Bạn có chắc chắn muốn hủy đơn hàng #{maHoaDon} không? Các sản phẩm đã bán sẽ được hoàn kho.", "Xác nhận hủy đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var success = _services.XuLyDonHang.HuyDonHang(maHoaDon);
            if (success)
            {
                MessageBox.Show($"Đơn hàng #{maHoaDon} đã hủy thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadOrderData();
            }
            else
            {
                MessageBox.Show("Không thể hủy đơn hàng này.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            AdminErrorHandler.Handle(ex, $"{nameof(OrdersView)}.HuyDonHang({maHoaDon})");
        }
    }
}
