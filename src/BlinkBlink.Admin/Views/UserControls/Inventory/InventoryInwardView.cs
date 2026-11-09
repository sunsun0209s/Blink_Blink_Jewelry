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
/// Màn hình Quản lý Nhập Kho Trang Sức Blink Blink Jewelry.
/// Cho phép lập phiếu nhập kho, theo dõi lịch sử nhập hàng và tự động cập nhật tồn kho sẵn sàng bán.
/// </summary>
public class InventoryInwardView : UserControl
{
    private readonly AdminServiceScope _services;
    private List<PhieuNhapKho> _allReceipts = new();

    private TextBox _txtSearch = null!;
    private Button _btnCreateReceipt = null!;
    private Button _btnViewDetails = null!;
    private Button _btnApprove = null!;
    private Button _btnDeleteReceipt = null!;
    private Button _btnInvalidate = null!;
    private Button _btnRefresh = null!;

    private Label _lblKpiTotalReceipts = null!;
    private Label _lblKpiTotalQuantity = null!;
    private Label _lblKpiTotalValue = null!;

    private DataGridView _dgvReceipts = null!;

    public InventoryInwardView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        LoadReceiptsData();
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
            Width = 570,
            Height = 85,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Phiếu Nhập", out _lblKpiTotalReceipts, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Món Đã Nhập", out _lblKpiTotalQuantity, UITheme.SuccessColor));
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Giá Trị Nhập (VND)", out _lblKpiTotalValue, UITheme.PrimaryGold));

        var pnlTitleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 15, 0)
        };

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ NHẬP KHO & TIẾP NHẬN TRANG SỨC",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Lập phiếu nhập kho từ nhà cung cấp, kiểm đếm số lượng và đưa sản phẩm vào trạng thái sẵn sàng giao dịch",
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
            Width = 220,
            Height = 28,
            Font = UITheme.RegularFont,
            PlaceholderText = "Mã phiếu, nhà cung cấp...",
            Margin = new Padding(0, 4, 15, 4)
        };
        _txtSearch.TextChanged += (s, e) => FilterReceipts();

        _btnCreateReceipt = UITheme.CreateFlatButton("+ Lập Phiếu Nhập Kho", UITheme.PrimaryGold, Color.Black);
        _btnCreateReceipt.Margin = new Padding(0, 2, 8, 2);
        _btnCreateReceipt.Click += (s, e) => ShowCreateReceiptDialog();

        _btnViewDetails = UITheme.CreateFlatButton("👁 Xem Chi Tiết", Color.FromArgb(51, 65, 85), Color.White);
        _btnViewDetails.Margin = new Padding(0, 2, 8, 2);
        _btnViewDetails.Click += (s, e) => ShowReceiptDetailsDialog();

        _btnApprove = UITheme.CreateFlatButton("✓ Duyệt Phiếu", Color.FromArgb(22, 101, 52), Color.White);
        _btnApprove.Margin = new Padding(0, 2, 8, 2);
        _btnApprove.Enabled = false;
        _btnApprove.Click += (s, e) => ApproveSelectedReceipt();

        _btnDeleteReceipt = UITheme.CreateFlatButton("🗑️ Xóa Phiếu", Color.FromArgb(239, 68, 68), Color.White);
        _btnDeleteReceipt.Margin = new Padding(0, 2, 8, 2);
        _btnDeleteReceipt.Enabled = false;
        _btnDeleteReceipt.Click += (s, e) => DeleteSelectedReceipt();

        _btnInvalidate = UITheme.CreateFlatButton("🚫 Vô Hiệu Hóa", Color.FromArgb(185, 28, 28), Color.White);
        _btnInvalidate.Margin = new Padding(0, 2, 8, 2);
        _btnInvalidate.Enabled = false;
        _btnInvalidate.Click += (s, e) => InvalidateSelectedReceipt();

        _btnRefresh = UITheme.CreateFlatButton("⟳ Nạp Lại", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (s, e) => LoadReceiptsData();

        toolbarPanel.Controls.Add(lblSearch);
        toolbarPanel.Controls.Add(_txtSearch);
        toolbarPanel.Controls.Add(_btnCreateReceipt);
        toolbarPanel.Controls.Add(_btnViewDetails);

        // Duyệt phiếu dành riêng cho Quản trị viên ADMIN khi phiếu ở trạng thái Chờ duyệt
        if (IsCurrentAdmin())
        {
            toolbarPanel.Controls.Add(_btnApprove);
        }

        // Xóa phiếu tồn kho: cả nhân viên và admin đều dùng được khi phiếu chưa duyệt
        toolbarPanel.Controls.Add(_btnDeleteReceipt);

        // Nút vô hiệu hóa phiếu nhập kho dành riêng cho tài khoản Quản trị viên ADMIN
        if (IsCurrentAdmin())
        {
            toolbarPanel.Controls.Add(_btnInvalidate);
        }

        toolbarPanel.Controls.Add(_btnRefresh);

        // Bảng phiếu nhập kho chiếm toàn bộ diện tích
        var pnlGridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 20),
            BackColor = UITheme.CanvasBackground
        };

        var pnlReceiptsCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
        _dgvReceipts = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvReceipts);
        ConfigureReceiptGridColumns();
        _dgvReceipts.CellDoubleClick += (s, e) => ShowReceiptDetailsDialog();
        _dgvReceipts.SelectionChanged += (s, e) => UpdateButtonStates();
        pnlReceiptsCard.Controls.Add(_dgvReceipts);

        pnlGridContainer.Controls.Add(pnlReceiptsCard);

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

    private void ConfigureReceiptGridColumns()
    {
        _dgvReceipts.Columns.Clear();
        var colMa = new DataGridViewTextBoxColumn { Name = "MaPhieuNhap", HeaderText = "MÃ PHIẾU NHẬP", FillWeight = 110, MinimumWidth = 120 };
        colMa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colNgay = new DataGridViewTextBoxColumn { Name = "NgayNhap", HeaderText = "NGÀY NHẬP", FillWeight = 120, MinimumWidth = 130 };
        colNgay.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colNgay.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colNcc = new DataGridViewTextBoxColumn { Name = "NhaCungCap", HeaderText = "NHÀ CUNG CẤP TRANG SỨC", FillWeight = 200, MinimumWidth = 180 };
        colNcc.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colNcc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colNv = new DataGridViewTextBoxColumn { Name = "MaNhanVien", HeaderText = "NGƯỜI TIẾP NHẬN", FillWeight = 100, MinimumWidth = 110 };
        colNv.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        colNv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var colSl = new DataGridViewTextBoxColumn { Name = "TongSoLuong", HeaderText = "TỔNG SỐ LƯỢNG", FillWeight = 95, MinimumWidth = 115, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        colSl.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colTong = new DataGridViewTextBoxColumn { Name = "TongGiaTri", HeaderText = "TỔNG TIỀN NHẬP (VND)", FillWeight = 140, MinimumWidth = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } };
        colTong.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colTrangThai = new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 110, MinimumWidth = 120 };
        colTrangThai.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colTrangThai.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _dgvReceipts.Columns.AddRange(colMa, colNgay, colNcc, colNv, colSl, colTong, colTrangThai);
    }

    public void LoadReceiptsData()
    {
        _allReceipts = _services.PhieuNhapKhoRepo.LayTatCa();
        UpdateKpis();
        FilterReceipts();
    }

    private void UpdateKpis()
    {
        var viVn = new CultureInfo("vi-VN");
        bool IsApproved(PhieuNhapKho r) => string.Equals(r.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) || r.TrangThai == "1" || r.TrangThai == "True";

        _lblKpiTotalReceipts.Text = _allReceipts.Count.ToString("N0", viVn);
        _lblKpiTotalQuantity.Text = _allReceipts.Where(IsApproved).Sum(r => r.TongSoSanPham).ToString("N0", viVn);
        _lblKpiTotalValue.Text = _allReceipts.Where(IsApproved).Sum(r => r.TongGiaTri).ToString("N0", viVn);
    }

    private void FilterReceipts()
    {
        var query = _txtSearch.Text.Trim().ToLowerInvariant();

        var filtered = _allReceipts.Where(r =>
            string.IsNullOrEmpty(query) ||
            r.MaPhieuNhap.ToLowerInvariant().Contains(query) ||
            r.NhaCungCap.ToLowerInvariant().Contains(query) ||
            r.MaNhanVien.ToLowerInvariant().Contains(query)
        ).ToList();

        _dgvReceipts.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");

        foreach (var r in filtered)
        {
            string statusDisplay;
            Color statusFg;
            Color statusBg;

            if (string.Equals(r.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) || r.TrangThai == "1" || r.TrangThai == "True")
            {
                statusDisplay = "✓ Đã duyệt";
                statusFg = Color.FromArgb(22, 101, 52);
                statusBg = Color.FromArgb(220, 252, 231);
            }
            else if (string.Equals(r.TrangThai, "DaHuy", StringComparison.OrdinalIgnoreCase) || string.Equals(r.TrangThai, "VoHieuHoa", StringComparison.OrdinalIgnoreCase) || r.TrangThai == "0" || r.TrangThai == "False")
            {
                statusDisplay = "🚫 Đã hủy";
                statusFg = Color.FromArgb(153, 27, 27);
                statusBg = Color.FromArgb(254, 226, 226);
            }
            else
            {
                statusDisplay = "⏳ Chờ duyệt";
                statusFg = Color.FromArgb(146, 64, 14);
                statusBg = Color.FromArgb(254, 243, 199);
            }

            var rowIndex = _dgvReceipts.Rows.Add(
                r.MaPhieuNhap,
                r.NgayNhap.ToString("dd/MM/yyyy HH:mm"),
                r.NhaCungCap,
                r.MaNhanVien,
                r.TongSoSanPham.ToString("N0", viVn),
                r.TongGiaTri.ToString("N0", viVn),
                statusDisplay
            );

            var cellTrangThai = _dgvReceipts.Rows[rowIndex].Cells["TrangThai"];
            cellTrangThai.Style.ForeColor = statusFg;
            cellTrangThai.Style.BackColor = statusBg;
            cellTrangThai.Style.Font = new Font(UITheme.RegularFont, FontStyle.Bold);
        }

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (_dgvReceipts.SelectedRows.Count == 0)
        {
            _btnApprove.Enabled = false;
            _btnDeleteReceipt.Enabled = false;
            _btnInvalidate.Enabled = false;
            return;
        }

        var maPhieu = _dgvReceipts.SelectedRows[0].Cells["MaPhieuNhap"].Value?.ToString();
        var receipt = _allReceipts.FirstOrDefault(r => r.MaPhieuNhap == maPhieu);
        if (receipt == null)
        {
            _btnApprove.Enabled = false;
            _btnDeleteReceipt.Enabled = false;
            _btnInvalidate.Enabled = false;
            return;
        }

        bool isChoDuyet = string.Equals(receipt.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase);
        bool isDaDuyet = string.Equals(receipt.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) || receipt.TrangThai == "1" || receipt.TrangThai == "True";

        // Xóa phiếu tồn kho: cả nhân viên và admin đều dùng được khi phiếu chưa duyệt
        _btnDeleteReceipt.Enabled = isChoDuyet;

        // Phê duyệt phiếu: chỉ Admin khi phiếu ở trạng thái Chờ duyệt
        _btnApprove.Enabled = IsCurrentAdmin() && isChoDuyet;

        // Vô hiệu hóa phiếu: chỉ Admin khi phiếu đã được duyệt
        _btnInvalidate.Enabled = IsCurrentAdmin() && isDaDuyet;
    }

    private void ApproveSelectedReceipt()
    {
        if (!IsCurrentAdmin())
        {
            MessageBox.Show("Chức năng duyệt phiếu nhập kho chỉ dành riêng cho tài khoản Quản trị viên (ADMIN).", "Quyền truy cập bị từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_dgvReceipts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một phiếu nhập kho cần phê duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maPhieu = _dgvReceipts.SelectedRows[0].Cells["MaPhieuNhap"].Value?.ToString();
        var receipt = _allReceipts.FirstOrDefault(r => r.MaPhieuNhap == maPhieu);
        if (receipt == null) return;

        if (!string.Equals(receipt.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show($"Phiếu nhập kho [{receipt.MaPhieuNhap}] không ở trạng thái Chờ duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn phê duyệt phiếu nhập kho [{receipt.MaPhieuNhap}]?\n\n" +
            "Sau khi duyệt, hệ thống sẽ:\n" +
            "1. Chính thức cộng dồn số lượng hàng hóa vào tồn kho.\n" +
            "2. Tự động tính toán lại đơn giá vốn bình quân gia quyền.\n" +
            "3. Tự động cập nhật lại đơn giá bán niêm yết theo % lợi nhuận.",
            "Xác nhận phê duyệt phiếu nhập kho",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm != DialogResult.Yes) return;

        var adminId = _services.CurrentStaff?.MaNhanVien ?? _services.CurrentUser?.MaNguoiDung ?? "ADMIN";
        var success = _services.XacNhanNhapKho.DuyetPhieuNhap(receipt.MaPhieuNhap, adminId, out var error);
        if (success)
        {
            MessageBox.Show($"Phê duyệt thành công phiếu nhập kho [{receipt.MaPhieuNhap}]. Tồn kho và giá bán đã được cập nhật!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadReceiptsData();
        }
        else
        {
            MessageBox.Show($"Không thể phê duyệt phiếu nhập kho:\n{error}", "Lỗi xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedReceipt()
    {
        if (_dgvReceipts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một phiếu nhập kho cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maPhieu = _dgvReceipts.SelectedRows[0].Cells["MaPhieuNhap"].Value?.ToString();
        var receipt = _allReceipts.FirstOrDefault(r => r.MaPhieuNhap == maPhieu);
        if (receipt == null) return;

        if (!string.Equals(receipt.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Chỉ được phép xóa phiếu nhập kho chưa được phê duyệt (trạng thái Chờ duyệt).", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa phiếu nhập kho [{receipt.MaPhieuNhap}] không?\nThao tác này sẽ xóa hoàn toàn phiếu chưa duyệt này khỏi hệ thống.",
            "Xác nhận xóa phiếu nhập kho",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (confirm != DialogResult.Yes) return;

        var success = _services.XacNhanNhapKho.XoaPhieuNhap(receipt.MaPhieuNhap, out var error);
        if (success)
        {
            MessageBox.Show($"Đã xóa thành công phiếu nhập kho [{receipt.MaPhieuNhap}].", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadReceiptsData();
        }
        else
        {
            MessageBox.Show($"Không thể xóa phiếu nhập kho:\n{error}", "Lỗi xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowReceiptDetailsDialog()
    {
        if (_dgvReceipts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một phiếu nhập kho để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maPhieu = _dgvReceipts.SelectedRows[0].Cells["MaPhieuNhap"].Value?.ToString();
        var receipt = _allReceipts.FirstOrDefault(r => r.MaPhieuNhap == maPhieu);
        if (receipt == null) return;

        using var dialog = new Form
        {
            Text = $"Chi Tiết Phiếu Nhập Kho #{receipt.MaPhieuNhap} - Blink Blink Jewelry",
            Size = new Size(880, 560),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = UITheme.CanvasBackground,
            Font = UITheme.RegularFont
        };

        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.White,
            Padding = new Padding(20, 15, 20, 15)
        };

        var lblTitle = new Label
        {
            Text = $"PHIẾU NHẬP KHO: {receipt.MaPhieuNhap}",
            Font = UITheme.SubHeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 25
        };

        var viVn = new CultureInfo("vi-VN");
        string statusText;
        if (string.Equals(receipt.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) || receipt.TrangThai == "1" || receipt.TrangThai == "True")
        {
            statusText = "ĐÃ DUYỆT (TỒN KHO ĐÃ CẬP NHẬT)";
        }
        else if (string.Equals(receipt.TrangThai, "DaHuy", StringComparison.OrdinalIgnoreCase) || string.Equals(receipt.TrangThai, "VoHieuHoa", StringComparison.OrdinalIgnoreCase) || receipt.TrangThai == "0" || receipt.TrangThai == "False")
        {
            statusText = "ĐÃ HỦY / VÔ HIỆU HÓA";
        }
        else
        {
            statusText = "CHỜ PHÊ DUYỆT (TỒN KHO CHƯA THAY ĐỔI)";
        }

        var lblInfo = new Label
        {
            Text = $"Ngày nhập: {receipt.NgayNhap:dd/MM/yyyy HH:mm}   |   Nhà cung cấp: {receipt.NhaCungCap}   |   Nhân viên: {receipt.MaNhanVien}\n" +
                   $"Tổng số lượng: {receipt.TongSoSanPham:N0} món   |   Tổng giá trị: {receipt.TongGiaTri.ToString("N0", viVn)} VND   |   Trạng thái: {statusText}",
            Font = UITheme.RegularFont,
            ForeColor = UITheme.TextSecondary,
            Dock = DockStyle.Fill
        };

        pnlHeader.Controls.Add(lblInfo);
        pnlHeader.Controls.Add(lblTitle);

        var pnlGrid = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 10),
            BackColor = UITheme.CanvasBackground
        };

        var dgv = new DataGridView();
        UITheme.ApplyGridViewStyle(dgv);

        var colMa = new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 100, MinimumWidth = 100 };
        var colTen = new DataGridViewTextBoxColumn { Name = "TenSanPham", HeaderText = "TÊN TRANG SỨC", FillWeight = 220, MinimumWidth = 200 };
        var colSl = new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "SỐ LƯỢNG", FillWeight = 85, MinimumWidth = 95, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colGiaVon = new DataGridViewTextBoxColumn { Name = "GiaVonNhap", HeaderText = "ĐƠN GIÁ VỐN (VND)", FillWeight = 130, MinimumWidth = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        var colThanhTien = new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "THÀNH TIỀN (VND)", FillWeight = 140, MinimumWidth = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } };
        var colChungTu = new DataGridViewTextBoxColumn { Name = "SoChungTu", HeaderText = "SỐ CHỨNG TỪ", FillWeight = 110, MinimumWidth = 110 };

        dgv.Columns.AddRange(colMa, colTen, colSl, colGiaVon, colThanhTien, colChungTu);

        if (receipt.ChiTietNhapKhos != null)
        {
            foreach (var ct in receipt.ChiTietNhapKhos)
            {
                var tenSp = ct.SanPhamTrangSuc?.TenSanPham ?? ct.MaSanPham;
                var thanhTien = ct.TinhGiaTri();
                dgv.Rows.Add(
                    ct.MaSanPham,
                    tenSp,
                    ct.SoLuong.ToString("N0", viVn),
                    ct.GiaVonNhap.ToString("N0", viVn),
                    thanhTien.ToString("N0", viVn),
                    string.IsNullOrEmpty(ct.SoChungTu) ? "-" : ct.SoChungTu
                );
            }
        }

        var pnlCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
        pnlCard.Controls.Add(dgv);
        pnlGrid.Controls.Add(pnlCard);

        var pnlFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(20, 12, 20, 12)
        };

        var btnClose = UITheme.CreateFlatButton("Đóng", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        btnClose.Dock = DockStyle.Right;
        btnClose.Width = 110;
        btnClose.Click += (s, e) => dialog.Close();

        pnlFooter.Controls.Add(btnClose);

        dialog.Controls.Add(pnlGrid);
        dialog.Controls.Add(pnlFooter);
        dialog.Controls.Add(pnlHeader);

        dialog.ShowDialog(this);
    }

    private void InvalidateSelectedReceipt()
    {
        if (!IsCurrentAdmin())
        {
            MessageBox.Show("Chức năng vô hiệu hóa phiếu nhập kho chỉ dành riêng cho tài khoản Quản trị viên (ADMIN).", "Quyền truy cập bị từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_dgvReceipts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một phiếu nhập kho cần vô hiệu hóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var maPhieu = _dgvReceipts.SelectedRows[0].Cells["MaPhieuNhap"].Value?.ToString();
        var receipt = _allReceipts.FirstOrDefault(r => r.MaPhieuNhap == maPhieu);
        if (receipt == null) return;

        bool isDaDuyet = string.Equals(receipt.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase) || receipt.TrangThai == "1" || receipt.TrangThai == "True";
        if (!isDaDuyet)
        {
            MessageBox.Show($"Chỉ có thể vô hiệu hóa phiếu nhập kho đã được phê duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn vô hiệu hóa phiếu nhập kho [{receipt.MaPhieuNhap}]?\n\n" +
            "Hệ thống sẽ:\n" +
            "1. Đánh dấu phiếu nhập là vô hiệu hóa (không xóa lịch sử chứng từ).\n" +
            "2. Trừ số lượng tồn kho tương ứng của các mặt hàng trong phiếu.\n" +
            "3. Tính lại giá vốn nhập bình quân của sản phẩm như khi phiếu nhập này chưa từng tồn tại.\n\n" +
            "Thao tác này KHÔNG THỂ khôi phục trực tiếp!",
            "Xác nhận vô hiệu hóa phiếu nhập",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2
        );

        if (confirm != DialogResult.Yes) return;

        var adminId = _services.CurrentStaff?.MaNhanVien ?? _services.CurrentUser?.MaNguoiDung ?? "ADMIN";
        var success = _services.XacNhanNhapKho.VoHieuHoaPhieuNhap(receipt.MaPhieuNhap, adminId, out var error);

        if (success)
        {
            MessageBox.Show($"Đã vô hiệu hóa thành công phiếu nhập kho [{receipt.MaPhieuNhap}]. Tồn kho và giá vốn đã được cập nhật lại chính xác.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadReceiptsData();
        }
        else
        {
            MessageBox.Show($"Không thể vô hiệu hóa phiếu nhập kho:\n{error}", "Lỗi xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowCreateReceiptDialog()
    {
        var allProducts = _services.SanPhamRepo.LayTatCa();
        if (allProducts.Count == 0)
        {
            MessageBox.Show("Hiện chưa có sản phẩm nào trong danh mục. Vui lòng thêm sản phẩm trước khi lập phiếu nhập kho.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new Form
        {
            Text = "Lập Phiếu Nhập Kho Trang Sức Mới - Blink Blink Jewelry",
            Width = 1060,
            Height = 740,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = UITheme.RegularFont
        };

        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = UITheme.DarkHeader, Padding = new Padding(24, 14, 24, 10) };
        var lblTitle = new Label { Text = "LẬP PHIẾU NHẬP KHO KIM HOÀN", Font = UITheme.HeaderFont, ForeColor = UITheme.PrimaryGold, Dock = DockStyle.Top, Height = 24 };
        var lblSub = new Label { Text = "Xác nhận nhập số lượng trang sức vào kho hàng và tự động cập nhật trạng thái 'Sẵn sàng giao dịch'", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(203, 213, 225), Dock = DockStyle.Fill };
        pnlHead.Controls.Add(lblSub);
        pnlHead.Controls.Add(lblTitle);

        var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = UITheme.CanvasBackground, Padding = new Padding(24, 16, 24, 16) };
        var flpFoot = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };

        var btnCancel = UITheme.CreateFlatButton("Hủy Bỏ", Color.FromArgb(226, 232, 240), UITheme.TextPrimary);
        btnCancel.Width = 115;
        btnCancel.Height = 38;
        btnCancel.Margin = new Padding(12, 0, 0, 0);
        btnCancel.Click += (s, e) => dialog.Close();

        var btnConfirm = UITheme.CreateFlatButton("Xác Nhận Nhập Kho", UITheme.PrimaryGold, Color.Black);
        btnConfirm.Width = 180;
        btnConfirm.Height = 38;
        btnConfirm.Margin = new Padding(0);

        flpFoot.Controls.Add(btnCancel);
        flpFoot.Controls.Add(btnConfirm);
        pnlFoot.Controls.Add(flpFoot);

        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 15, 24, 10) };

        // Thông tin phiếu: Bố cục FlowLayoutPanel 2 hàng chống tràn và cắt cụt chữ
        var pnlInfo = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 96,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0)
        };

        var infoRow1 = new FlowLayoutPanel
        {
            Width = 980,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        var lblMaP = new Label
        {
            Text = "Mã Phiếu Nhập:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 8, 0)
        };
        var txtMaP = new TextBox
        {
            Text = $"PNK{DateTime.Now:yyyyMMdd}_{_allReceipts.Count + 1:D2}",
            Width = 190,
            Height = 30,
            ReadOnly = true,
            BackColor = Color.FromArgb(241, 245, 249),
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 2, 35, 0)
        };

        var lblNcc = new Label
        {
            Text = "Nhà Cung Cấp (*):",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 8, 0)
        };
        var txtNcc = new TextBox
        {
            Text = "",
            PlaceholderText = "Nhập tên nhà cung cấp...",
            Width = 360,
            Height = 30,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 2, 0, 0)
        };

        infoRow1.Controls.Add(lblMaP);
        infoRow1.Controls.Add(txtMaP);
        infoRow1.Controls.Add(lblNcc);
        infoRow1.Controls.Add(txtNcc);

        var infoRow2 = new FlowLayoutPanel
        {
            Width = 980,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        var lblNv = new Label
        {
            Text = "Mã Nhân Viên:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 8, 0)
        };
        var txtNv = new TextBox
        {
            Text = "USER_NV002",
            Width = 190,
            Height = 30,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 2, 35, 0)
        };

        var lblChungTu = new Label
        {
            Text = "Số Chứng Từ / HĐ:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 8, 0)
        };
        var txtChungTu = new TextBox
        {
            Text = "",
            PlaceholderText = "Nhập số hóa đơn / chứng từ...",
            Width = 360,
            Height = 30,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 2, 0, 0)
        };

        infoRow2.Controls.Add(lblNv);
        infoRow2.Controls.Add(txtNv);
        infoRow2.Controls.Add(lblChungTu);
        infoRow2.Controls.Add(txtChungTu);

        pnlInfo.Controls.Add(infoRow1);
        pnlInfo.Controls.Add(infoRow2);

        // Thêm mặt hàng nhập: Có ô tìm kiếm, chọn sản phẩm, chọn size thông minh và nút thêm
        var pnlAddItem = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 120,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(16, 10, 16, 10),
            Margin = new Padding(0, 0, 0, 10)
        };

        var itemRow1 = new FlowLayoutPanel
        {
            Width = 980,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        var lblSearchSp = new Label
        {
            Text = "Tìm SP:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 6, 0)
        };

        var txtSearchSp = new TextBox
        {
            Width = 130,
            Height = 30,
            Font = UITheme.RegularFont,
            PlaceholderText = "🔍 Tìm SP...",
            Margin = new Padding(0, 2, 16, 0)
        };

        var lblSp = new Label
        {
            Text = "Sản Phẩm Nhập (*):",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 6, 0)
        };

        var cboSp = new ComboBox
        {
            Width = 320,
            Height = 30,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 2, 16, 0)
        };

        var lblSize = new Label
        {
            Text = "Size (*):",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 6, 0)
        };

        var cboSize = new ComboBox
        {
            Width = 145,
            Height = 30,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 2, 0, 0)
        };

        itemRow1.Controls.Add(lblSearchSp);
        itemRow1.Controls.Add(txtSearchSp);
        itemRow1.Controls.Add(lblSp);
        itemRow1.Controls.Add(cboSp);
        itemRow1.Controls.Add(lblSize);
        itemRow1.Controls.Add(cboSize);

        var itemRow2 = new FlowLayoutPanel
        {
            Width = 980,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        var lblSl = new Label
        {
            Text = "Số Lượng (*):",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 6, 0)
        };

        var nudSl = new NumericUpDown
        {
            Width = 90,
            Height = 30,
            Minimum = 1,
            Maximum = 1000,
            Value = 1,
            Font = UITheme.TabularNumericFont,
            Margin = new Padding(0, 2, 20, 0)
        };

        var lblGiaVon = new Label
        {
            Text = "Đơn Giá Vốn (VND) (*):",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Font = UITheme.RegularFont,
            Margin = new Padding(0, 6, 6, 0)
        };

        var txtGiaVon = new TextBox
        {
            Width = 200,
            Height = 30,
            Font = UITheme.TabularNumericFont,
            PlaceholderText = "Nhập đơn giá vốn...",
            Margin = new Padding(0, 2, 25, 0)
        };

        var btnAddRow = UITheme.CreateFlatButton("+ Thêm Vào Phiếu Nhập", UITheme.DarkHeader, Color.White);
        btnAddRow.Height = 34;
        btnAddRow.AutoSize = true;
        btnAddRow.Padding = new Padding(16, 2, 16, 2);
        btnAddRow.Margin = new Padding(0, 0, 0, 0);

        itemRow2.Controls.Add(lblSl);
        itemRow2.Controls.Add(nudSl);
        itemRow2.Controls.Add(lblGiaVon);
        itemRow2.Controls.Add(txtGiaVon);
        itemRow2.Controls.Add(btnAddRow);

        pnlAddItem.Controls.Add(itemRow1);
        pnlAddItem.Controls.Add(itemRow2);

        // Logic nạp và đồng bộ giữa sản phẩm và size
        bool isSyncingSelection = false;

        void PopulateProductDropdown(string filterKeyword = "")
        {
            var previousSelectedMa = (cboSp.SelectedItem as ProductComboItem)?.MaSP;
            cboSp.Items.Clear();

            var query = filterKeyword.Trim().ToLowerInvariant();
            var matches = allProducts.Where(p =>
                string.IsNullOrWhiteSpace(query) ||
                p.MaSanPham.ToLowerInvariant().Contains(query) ||
                p.TenSanPham.ToLowerInvariant().Contains(query)
            ).ToList();

            foreach (var sp in matches)
            {
                cboSp.Items.Add(new ProductComboItem(sp.MaSanPham, sp.TenSanPham, sp.Size ?? "", sp.MaNhom ?? "0", sp.GiaNhap));
            }

            if (cboSp.Items.Count > 0)
            {
                var matchIdx = -1;
                if (!string.IsNullOrEmpty(previousSelectedMa))
                {
                    for (int i = 0; i < cboSp.Items.Count; i++)
                    {
                        if ((cboSp.Items[i] as ProductComboItem)?.MaSP == previousSelectedMa)
                        {
                            matchIdx = i;
                            break;
                        }
                    }
                }
                cboSp.SelectedIndex = matchIdx >= 0 ? matchIdx : 0;
            }
            else
            {
                cboSize.Items.Clear();
                txtGiaVon.Text = string.Empty;
            }
        }

        void UpdateSizeOptionsForSelectedProduct()
        {
            if (isSyncingSelection) return;
            if (cboSp.SelectedItem is not ProductComboItem currentItem) return;

            isSyncingSelection = true;
            try
            {
                cboSize.Items.Clear();

                if (!string.IsNullOrWhiteSpace(currentItem.MaNhom) && currentItem.MaNhom != "0")
                {
                    // Sản phẩm thuộc nhóm: Nạp toàn bộ size của nhóm
                    var groupMembers = allProducts.Where(p => p.MaNhom == currentItem.MaNhom).ToList();
                    foreach (var m in groupMembers)
                    {
                        var sizeText = string.IsNullOrWhiteSpace(m.Size) ? "Tiêu chuẩn" : m.Size;
                        if (!cboSize.Items.Contains(sizeText))
                        {
                            cboSize.Items.Add(sizeText);
                        }
                    }

                    var currentSizeText = string.IsNullOrWhiteSpace(currentItem.Size) ? "Tiêu chuẩn" : currentItem.Size;
                    var matchIdx = cboSize.Items.IndexOf(currentSizeText);
                    cboSize.SelectedIndex = matchIdx >= 0 ? matchIdx : 0;
                }
                else
                {
                    // Sản phẩm không có nhóm: Chỉ hiển thị 1 size của chính nó
                    var singleSize = string.IsNullOrWhiteSpace(currentItem.Size) ? "Tiêu chuẩn" : currentItem.Size;
                    cboSize.Items.Add(singleSize);
                    cboSize.SelectedIndex = 0;
                }

                txtGiaVon.Text = currentItem.DefaultGiaNhap > 0 ? currentItem.DefaultGiaNhap.ToString("0") : "";
            }
            finally
            {
                isSyncingSelection = false;
            }
        }

        txtSearchSp.TextChanged += (s, e) => PopulateProductDropdown(txtSearchSp.Text);

        cboSp.SelectedIndexChanged += (s, e) =>
        {
            UpdateSizeOptionsForSelectedProduct();
        };

        cboSize.SelectedIndexChanged += (s, e) =>
        {
            if (isSyncingSelection) return;
            if (cboSp.SelectedItem is not ProductComboItem currentItem) return;
            var selectedSize = cboSize.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selectedSize)) return;

            // Nếu sản phẩm thuộc nhóm, tìm sản phẩm tương ứng với size vừa chọn
            if (!string.IsNullOrWhiteSpace(currentItem.MaNhom) && currentItem.MaNhom != "0")
            {
                var matchSp = allProducts.FirstOrDefault(p =>
                    p.MaNhom == currentItem.MaNhom &&
                    string.Equals(string.IsNullOrWhiteSpace(p.Size) ? "Tiêu chuẩn" : p.Size, selectedSize, StringComparison.OrdinalIgnoreCase)
                );

                if (matchSp != null && matchSp.MaSanPham != currentItem.MaSP)
                {
                    isSyncingSelection = true;
                    try
                    {
                        for (int i = 0; i < cboSp.Items.Count; i++)
                        {
                            if ((cboSp.Items[i] as ProductComboItem)?.MaSP == matchSp.MaSanPham)
                            {
                                cboSp.SelectedIndex = i;
                                txtGiaVon.Text = matchSp.GiaNhap > 0 ? matchSp.GiaNhap.ToString("0") : "";
                                break;
                            }
                        }
                    }
                    finally
                    {
                        isSyncingSelection = false;
                    }
                }
            }
        };

        PopulateProductDropdown();

        // Bảng các dòng chi tiết tạm
        var dgvTemp = new DataGridView();
        UITheme.ApplyGridViewStyle(dgvTemp);
        
        var tempColMa = new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 95, MinimumWidth = 100 };
        tempColMa.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        tempColMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var tempColTen = new DataGridViewTextBoxColumn { Name = "TenSanPham", HeaderText = "TÊN TRANG SỨC", FillWeight = 200, MinimumWidth = 200 };
        tempColTen.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        tempColTen.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        var tempColSize = new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "SIZE", FillWeight = 85, MinimumWidth = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } };
        tempColSize.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        var tempColSl = new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "SỐ LƯỢNG", FillWeight = 80, MinimumWidth = 85, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        tempColSl.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var tempColGiaVon = new DataGridViewTextBoxColumn { Name = "GiaVon", HeaderText = "GIÁ VỐN (VND)", FillWeight = 115, MinimumWidth = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont } };
        tempColGiaVon.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var tempColThanhTien = new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "THÀNH TIỀN (VND)", FillWeight = 125, MinimumWidth = 135, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } };
        tempColThanhTien.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

        var tempColXoa = new DataGridViewButtonColumn
        {
            Name = "Xoa",
            HeaderText = "HÀNH ĐỘNG",
            Text = "🗑️ Xóa",
            UseColumnTextForButtonValue = true,
            Width = 90,
            FlatStyle = FlatStyle.Flat
        };
        tempColXoa.DefaultCellStyle.ForeColor = Color.FromArgb(239, 68, 68);
        tempColXoa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        dgvTemp.Columns.AddRange(tempColMa, tempColTen, tempColSize, tempColSl, tempColGiaVon, tempColThanhTien, tempColXoa);

        var tempItems = new List<ChiTietNhapKhoDTO>();
        var viVn = new CultureInfo("vi-VN");

        var pnlTotalBar = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(12, 10, 12, 4) };
        var lblSummary = new Label { Text = "Tổng Số Lượng: 0  |  Tổng Tiền Nhập: 0 VND", Font = UITheme.SubHeaderFont, ForeColor = UITheme.PrimaryGold, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
        pnlTotalBar.Controls.Add(lblSummary);

        void RefreshTempGrid()
        {
            dgvTemp.Rows.Clear();
            decimal totalVal = 0;
            int totalQty = 0;

            foreach (var item in tempItems)
            {
                var p = allProducts.FirstOrDefault(s => s.MaSanPham == item.MaSanPham);
                var tt = item.SoLuong * item.GiaVonNhap;
                totalQty += item.SoLuong;
                totalVal += tt;

                dgvTemp.Rows.Add(
                    item.MaSanPham,
                    p?.TenSanPham ?? item.MaSanPham,
                    p?.Size ?? "-",
                    item.SoLuong.ToString("N0", viVn),
                    item.GiaVonNhap.ToString("N0", viVn),
                    tt.ToString("N0", viVn)
                );
            }

            lblSummary.Text = $"Tổng Số Lượng: {totalQty:N0}  |  Tổng Tiền Nhập: {totalVal:N0} VND";
        }

        dgvTemp.CellContentClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvTemp.Columns["Xoa"].Index)
            {
                if (e.RowIndex < tempItems.Count)
                {
                    tempItems.RemoveAt(e.RowIndex);
                    RefreshTempGrid();
                }
            }
        };

        btnAddRow.Click += (s, e) =>
        {
            if (cboSp.SelectedItem is not ProductComboItem item)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!decimal.TryParse(txtGiaVon.Text.Trim(), out var gv) || gv <= 0)
            {
                MessageBox.Show("Vui lòng nhập giá vốn hợp lệ (> 0).", "Lỗi nhập giá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGiaVon.Focus();
                return;
            }

            var qty = (int)nudSl.Value;

            // Kiểm tra trùng mã sản phẩm trong danh sách đã chọn
            var existing = tempItems.FirstOrDefault(x => x.MaSanPham == item.MaSP);
            if (existing != null)
            {
                existing.SoLuong += qty;
                existing.GiaVonNhap = gv;
            }
            else
            {
                tempItems.Add(new ChiTietNhapKhoDTO
                {
                    MaSanPham = item.MaSP,
                    SoLuong = qty,
                    GiaVonNhap = gv,
                    SoChungTu = txtChungTu.Text.Trim()
                });
            }

            RefreshTempGrid();
        };

        btnConfirm.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtNcc.Text))
            {
                MessageBox.Show("Vui lòng nhập Nhà cung cấp.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNcc.Focus();
                return;
            }

            if (tempItems.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất 1 dòng sản phẩm vào phiếu nhập.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = new NhapKhoDTO
            {
                MaPhieuNhap = txtMaP.Text.Trim(),
                NhaCungCap = txtNcc.Text.Trim(),
                MaNhanVien = txtNv.Text.Trim(),
                DanhSachSanPham = tempItems
            };

            try
            {
                _services.XacNhanNhapKho.Execute(request);
                MessageBox.Show($"Lập phiếu nhập kho [{request.MaPhieuNhap}] thành công!\n\nPhiếu đang ở trạng thái 'Chờ duyệt'. Sau khi Quản trị viên (Admin) phê duyệt, số lượng hàng sẽ được tính vào tồn kho và tự động cập nhật đơn giá bán theo % lợi nhuận.", "Lập phiếu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong quá trình nhập kho: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        var pnlGridWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 10) };
        pnlGridWrapper.Controls.Add(dgvTemp);

        pnlBody.Controls.Add(pnlGridWrapper);
        pnlBody.Controls.Add(pnlTotalBar);
        pnlBody.Controls.Add(pnlAddItem);
        pnlBody.Controls.Add(pnlInfo);

        dialog.Controls.Add(pnlBody);
        dialog.Controls.Add(pnlFoot);
        dialog.Controls.Add(pnlHead);

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadReceiptsData();
        }
    }

    private class ProductComboItem
    {
        public string MaSP { get; }
        public string TenSP { get; }
        public string Size { get; }
        public string MaNhom { get; }
        public decimal DefaultGiaNhap { get; }

        public ProductComboItem(string maSp, string tenSp, string size, string maNhom, decimal defaultGiaNhap)
        {
            MaSP = maSp;
            TenSP = tenSp;
            Size = size ?? string.Empty;
            MaNhom = maNhom ?? "0";
            DefaultGiaNhap = defaultGiaNhap;
        }

        public override string ToString() => $"[{MaSP}] {TenSP}";
    }
}
