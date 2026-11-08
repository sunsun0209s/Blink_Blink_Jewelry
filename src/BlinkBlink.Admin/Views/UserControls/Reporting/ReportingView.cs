using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.DTOs.Reporting;

namespace BlinkBlink.Admin.Views.UserControls.Reporting;

/// <summary>
/// Màn hình Báo Cáo & Thống Kê doanh thu, kiểm kê kho hàng trang sức.
/// Triển khai khớp 100% bản vẽ kỹ thuật FormBaoCaoThongKe trong XML/3 Layers.xml:
/// Các thành phần: dtpTuNgay, dtpDenNgay, dgvKetQua, btnXemDoanhThu_Click, btnXemTonKho_Click, btnXuatBaoCao_Click.
/// </summary>
public class ReportingView : UserControl
{
    private readonly AdminServiceScope _services;

    // Các thành phần giao diện theo đúng sơ đồ FormBaoCaoThongKe
    private DateTimePicker _dtpTuNgay = null!;
    private DateTimePicker _dtpDenNgay = null!;
    private DataGridView _dgvKetQua = null!;
    private Button _btnXemDoanhThu = null!;
    private Button _btnXemTonKho = null!;
    private Button _btnXemTopBanChay = null!;
    private Button _btnXuatBaoCao = null!;

    // Thẻ chỉ số tổng hợp
    private Label _lblKpiRevenue = null!;
    private Label _lblKpiProfit = null!;
    private Label _lblKpiOrderCount = null!;
    private Label _lblKpiStockValue = null!;
    private Label _lblReportMode = null!;

    private string _currentReportType = "DoanhThu";

    public ReportingView()
    {
        _services = AdminServiceScope.Instance;
        InitializeComponents();
        // Nạp báo cáo doanh thu mặc định khi vừa mở màn hình
        BtnXemDoanhThu_Click(this, EventArgs.Empty);
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = UITheme.CanvasBackground;
        this.Font = UITheme.RegularFont;

        // Top Header Panel
        var topHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(20, 15, 20, 10),
            BackColor = UITheme.CanvasBackground
        };

        var lblTitle = new Label
        {
            Text = "BÁO CÁO TÀI CHÍNH & THỐNG KÊ KHO HÀNG",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(20, 12),
            AutoSize = true,
            UseMnemonic = false
        };

        var lblSubTitle = new Label
        {
            Text = "Theo dõi doanh số bán hàng, tỷ suất lợi nhuận trang sức, biến động tồn kho và mặt hàng bán chạy",
            Font = UITheme.RegularFont,
            ForeColor = UITheme.TextSecondary,
            Location = new Point(20, 38),
            AutoSize = true,
            UseMnemonic = false
        };

        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 660,
            Height = 90,
            FlowDirection = FlowDirection.LeftToRight
        };

        pnlKpis.Controls.Add(CreateKpiCard("Tổng Doanh Thu", out _lblKpiRevenue, UITheme.PrimaryGold));
        pnlKpis.Controls.Add(CreateKpiCard("Lợi Nhuận Ước Tính", out _lblKpiProfit, UITheme.SuccessColor));
        pnlKpis.Controls.Add(CreateKpiCard("Tổng Đơn Hàng", out _lblKpiOrderCount, UITheme.TextPrimary));
        pnlKpis.Controls.Add(CreateKpiCard("Giá Trị Kho Hàng", out _lblKpiStockValue, Color.FromArgb(2, 136, 209)));

        topHeaderPanel.Controls.Add(lblTitle);
        topHeaderPanel.Controls.Add(lblSubTitle);
        topHeaderPanel.Controls.Add(pnlKpis);

        // Toolbar chứa dtpTuNgay, dtpDenNgay và các nút bấm dạng Responsive chống tràn chữ
        var toolbarPanel = UITheme.CreateResponsiveToolbar();

        var lblTuNgay = new Label
        {
            Text = "Từ ngày:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 6, 0)
        };

        _dtpTuNgay = new DateTimePicker
        {
            Width = 130,
            Format = DateTimePickerFormat.Short,
            Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            Margin = new Padding(0, 4, 15, 4)
        };

        var lblDenNgay = new Label
        {
            Text = "Đến ngày:",
            AutoSize = true,
            ForeColor = UITheme.TextSecondary,
            Margin = new Padding(0, 8, 6, 0)
        };

        _dtpDenNgay = new DateTimePicker
        {
            Width = 130,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now,
            Margin = new Padding(0, 4, 15, 4)
        };

        // Các nút bấm chính xác theo FormBaoCaoThongKe trong XML
        _btnXemDoanhThu = UITheme.CreateFlatButton("📊 Doanh Thu", UITheme.PrimaryGold, Color.Black);
        _btnXemDoanhThu.Margin = new Padding(0, 2, 8, 2);
        _btnXemDoanhThu.Click += BtnXemDoanhThu_Click;

        _btnXemTonKho = UITheme.CreateFlatButton("💎 Báo Cáo Tồn Kho", UITheme.DarkHeader, Color.White);
        _btnXemTonKho.Margin = new Padding(0, 2, 8, 2);
        _btnXemTonKho.Click += BtnXemTonKho_Click;

        _btnXemTopBanChay = UITheme.CreateFlatButton("🏆 Top Bán Chạy", UITheme.DarkHeader, Color.White);
        _btnXemTopBanChay.Margin = new Padding(0, 2, 8, 2);
        _btnXemTopBanChay.Click += BtnXemTopBanChay_Click;

        _btnXuatBaoCao = UITheme.CreateFlatButton("📥 Xuất File CSV/Excel", Color.FromArgb(241, 245, 249), UITheme.TextPrimary);
        _btnXuatBaoCao.Margin = new Padding(0, 2, 0, 2);
        _btnXuatBaoCao.Click += BtnXuatBaoCao_Click;

        toolbarPanel.Controls.Add(lblTuNgay);
        toolbarPanel.Controls.Add(_dtpTuNgay);
        toolbarPanel.Controls.Add(lblDenNgay);
        toolbarPanel.Controls.Add(_dtpDenNgay);
        toolbarPanel.Controls.Add(_btnXemDoanhThu);
        toolbarPanel.Controls.Add(_btnXemTonKho);
        toolbarPanel.Controls.Add(_btnXemTopBanChay);
        toolbarPanel.Controls.Add(_btnXuatBaoCao);

        // Vùng bảng kết quả dgvKetQua
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
            Padding = new Padding(12)
        };

        _lblReportMode = new Label
        {
            Text = "BẢNG KẾT QUẢ THỐNG KÊ DOANH THU THEO NGÀY",
            Font = UITheme.SubHeaderFont,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            UseMnemonic = false
        };

        _dgvKetQua = new DataGridView();
        UITheme.ApplyGridViewStyle(_dgvKetQua);

        pnlWhiteCard.Controls.Add(_dgvKetQua);
        pnlWhiteCard.Controls.Add(_lblReportMode);
        _lblReportMode.SendToBack();
        _dgvKetQua.BringToFront();
        pnlGridContainer.Controls.Add(pnlWhiteCard);

        this.Controls.Add(pnlGridContainer);
        this.Controls.Add(toolbarPanel);
        this.Controls.Add(topHeaderPanel);
    }

    private Panel CreateKpiCard(string title, out Label lblValue, Color valueColor)
    {
        var card = new Panel
        {
            Width = 155,
            Height = 75,
            BackColor = Color.White,
            Margin = new Padding(4, 5, 4, 5),
            Padding = new Padding(8, 8, 8, 8)
        };

        var lblT = new Label
        {
            Text = title,
            ForeColor = UITheme.TextSecondary,
            Font = new Font(FontFamily.GenericSansSerif, 8F, FontStyle.Regular),
            Dock = DockStyle.Top,
            Height = 18
        };

        lblValue = new Label
        {
            Text = "0",
            ForeColor = valueColor,
            Font = new Font("Consolas", 14F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        card.Controls.Add(lblValue);
        card.Controls.Add(lblT);
        return card;
    }

    /// <summary>
    /// Xử lý sự kiện xem doanh thu theo ngày khớp phương thức + btnXemDoanhThu_Click(): void trong sơ đồ XML.
    /// </summary>
    public async void BtnXemDoanhThu_Click(object? sender, EventArgs e)
    {
        _currentReportType = "DoanhThu";
        _lblReportMode.Text = $"BẢNG KẾT QUẢ THỐNG KÊ DOANH THU ({_dtpTuNgay.Value:dd/MM/yyyy} - {_dtpDenNgay.Value:dd/MM/yyyy})";

        _dgvKetQua.Columns.Clear();
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgayThang", HeaderText = "NGÀY THÁNG", FillWeight = 90, MinimumWidth = 110 });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongDon", HeaderText = "SỐ LƯỢNG ĐƠN", FillWeight = 75, MinimumWidth = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongDoanhThu", HeaderText = "TỔNG DOANH THU (VND)", FillWeight = 110, MinimumWidth = 160, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "LoiNhuanUocTinh", HeaderText = "LỢI NHUẬN ƯỚC TÍNH (VND)", FillWeight = 110, MinimumWidth = 170, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.SuccessColor } });

        var data = await _services.ThongKeDoanhThu.ExecuteAsync(_dtpTuNgay.Value, _dtpDenNgay.Value);

        _dgvKetQua.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");
        decimal totalRev = 0;
        decimal totalProfit = 0;
        int totalOrders = 0;

        foreach (var item in data)
        {
            _dgvKetQua.Rows.Add(
                item.NgayThang.ToString("dd/MM/yyyy"),
                item.SoLuongDon.ToString("N0", viVn),
                item.TongDoanhThu.ToString("N0", viVn),
                item.LoiNhuanUocTinh.ToString("N0", viVn)
            );
            totalRev += item.TongDoanhThu;
            totalProfit += item.LoiNhuanUocTinh;
            totalOrders += item.SoLuongDon;
        }

        _lblKpiRevenue.Text = totalRev.ToString("N0", viVn);
        _lblKpiProfit.Text = totalProfit.ToString("N0", viVn);
        _lblKpiOrderCount.Text = totalOrders.ToString("N0", viVn);

        // Cập nhật giá trị tồn kho hiện thời
        var inventoryReport = await _services.ThongKeKhoHang.LayBaoCaoTonKhoAsync();
        _lblKpiStockValue.Text = inventoryReport.Sum(i => i.GiaTriTonKho).ToString("N0", viVn);
    }

    /// <summary>
    /// Xử lý sự kiện xem tồn kho & chênh lệch kiểm kê khớp phương thức + btnXemTonKho_Click(): void trong sơ đồ XML.
    /// </summary>
    public async void BtnXemTonKho_Click(object? sender, EventArgs e)
    {
        _currentReportType = "TonKho";
        _lblReportMode.Text = "BÁO CÁO TỒN KHO & ĐỐI CHIẾU CHÊNH LỆCH KIỂM KÊ KIM HOÀN";

        _dgvKetQua.Columns.Clear();
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 85, MinimumWidth = 110 });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenSanPham", HeaderText = "TÊN TRANG SỨC", FillWeight = 190, MinimumWidth = 220 });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongTon", HeaderText = "SỐ LƯỢNG TỒN", FillWeight = 75, MinimumWidth = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "ChenhLechKiemKe", HeaderText = "CHÊNH LỆCH KIỂM KÊ", FillWeight = 85, MinimumWidth = 135, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiaTriTonKho", HeaderText = "GIÁ TRỊ TỒN KHO (VND)", FillWeight = 115, MinimumWidth = 160, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });

        var data = await _services.ThongKeKhoHang.LayBaoCaoTonKhoAsync();

        _dgvKetQua.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");
        decimal totalVal = 0;
        int totalItems = 0;

        foreach (var item in data)
        {
            var rowIdx = _dgvKetQua.Rows.Add(
                item.MaSanPham,
                item.TenSanPham,
                item.SoLuongTon.ToString("N0", viVn),
                item.ChenhLechKiemKe.ToString("+#,##0;-#,##0;0", viVn),
                item.GiaTriTonKho.ToString("N0", viVn)
            );

            if (item.ChenhLechKiemKe != 0)
            {
                _dgvKetQua.Rows[rowIdx].Cells["ChenhLechKiemKe"].Style.ForeColor = UITheme.DangerColor;
            }

            totalVal += item.GiaTriTonKho;
            totalItems += item.SoLuongTon;
        }

        _lblKpiStockValue.Text = totalVal.ToString("N0", viVn);
    }

    /// <summary>
    /// Xử lý sự kiện xem top sản phẩm trang sức bán chạy nhất.
    /// </summary>
    public async void BtnXemTopBanChay_Click(object? sender, EventArgs e)
    {
        _currentReportType = "TopBanChay";
        _lblReportMode.Text = "BÁO CÁO TOP SẢN PHẨM TRANG SỨC BÁN CHẠY NHẤT";

        _dgvKetQua.Columns.Clear();
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaSanPham", HeaderText = "MÃ SẢN PHẨM", FillWeight = 85, MinimumWidth = 110 });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenSanPham", HeaderText = "TÊN TRANG SỨC", FillWeight = 200, MinimumWidth = 220 });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongDaBan", HeaderText = "ĐÃ BÁN (CHIẾC)", FillWeight = 80, MinimumWidth = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = UITheme.TabularNumericFont } });
        _dgvKetQua.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongTienThu", HeaderText = "TỔNG TIỀN THU (VND)", FillWeight = 115, MinimumWidth = 160, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UITheme.TabularNumericFont, ForeColor = UITheme.PrimaryGold } });

        var data = await _services.ThongKeKhoHang.LayTopBanChayAsync(10);

        _dgvKetQua.Rows.Clear();
        var viVn = new CultureInfo("vi-VN");

        foreach (var item in data)
        {
            _dgvKetQua.Rows.Add(
                item.MaSanPham,
                item.TenSanPham,
                item.SoLuongDaBan.ToString("N0", viVn),
                item.TongTienThu.ToString("N0", viVn)
            );
        }
    }

    /// <summary>
    /// Xử lý sự kiện xuất báo cáo ra tệp CSV hỗ trợ mở bằng Microsoft Excel.
    /// Khớp phương thức + btnXuatBaoCao_Click(): void trong sơ đồ XML.
    /// </summary>
    public void BtnXuatBaoCao_Click(object? sender, EventArgs e)
    {
        if (_dgvKetQua.Rows.Count == 0)
        {
            MessageBox.Show("Hiện không có dữ liệu để kết xuất báo cáo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Tệp CSV (Hỗ trợ Excel)|*.csv|Tệp văn bản|*.txt",
            FileName = $"BlinkBlink_BaoCao_{_currentReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var sb = new StringBuilder();

            // Ghi tiêu đề cột
            var headers = new List<string>();
            foreach (DataGridViewColumn col in _dgvKetQua.Columns)
            {
                headers.Add($"\"{col.HeaderText.Replace("\"", "\"\"")}\"");
            }
            sb.AppendLine(string.Join(",", headers));

            // Ghi nội dung từng dòng
            foreach (DataGridViewRow row in _dgvKetQua.Rows)
            {
                var cells = new List<string>();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    var val = cell.Value?.ToString() ?? string.Empty;
                    cells.Add($"\"{val.Replace("\"", "\"\"")}\"");
                }
                sb.AppendLine(string.Join(",", cells));
            }

            // Ghi UTF-8 có BOM để Excel tiếng Việt hiển thị chính xác dấu tiếng Việt
            File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show($"Kết xuất dữ liệu thành công ra tệp:\n{sfd.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi ghi tệp: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
