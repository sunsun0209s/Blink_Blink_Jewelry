using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlinkBlink.Admin.Views.Helpers;

/// <summary>
/// Lớp quy chuẩn giao diện mỹ thuật toàn hệ thống Blink Blink Jewelry theo Taste Skill.
/// Thiết lập đồng nhất bảng màu kim hoàn sang trọng, kiểu chữ số liệu căn gióng chính xác và loại bỏ các thành phần rườm rà.
/// </summary>
public static class UITheme
{
    // Bảng màu kim hoàn cao cấp định nghĩa theo Taste Skill
    public static readonly Color PrimaryGold = Color.FromArgb(212, 175, 55);         // Vàng Champagne Gold điểm nhấn
    public static readonly Color DarkSidebar = Color.FromArgb(24, 28, 36);          // Xám than đậm cho thanh điều hướng
    public static readonly Color DarkHeader = Color.FromArgb(32, 38, 48);           // Xám than vừa cho thanh tiêu đề
    public static readonly Color DarkCard = Color.FromArgb(24, 28, 36);             // Xám than đậm cho khối giao diện tối
    public static readonly Color CanvasBackground = Color.FromArgb(245, 247, 250); // Nền xám ngọc trai dịu mắt
    public static readonly Color CardBackground = Color.FromArgb(255, 255, 255);    // Trắng tinh khiết cho thẻ dữ liệu
    public static readonly Color BorderColor = Color.FromArgb(226, 232, 240);       // Viền thanh mảnh phân tách khối
    public static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);          // Chữ đen than cho nội dung chính
    public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);     // Chữ xám cho nhãn phụ và chú thích
    public static readonly Color SuccessColor = Color.FromArgb(46, 125, 50);        // Xanh ngọc lục bảo biểu thị hoàn tất/đã giao
    public static readonly Color WarningColor = Color.FromArgb(237, 108, 2);        // Cam hổ phách biểu thị chờ xử lý
    public static readonly Color DangerColor = Color.FromArgb(211, 47, 47);         // Đỏ gạch biểu thị hủy đơn/lỗi

    // Font chữ quy chuẩn
    public static readonly Font HeaderFont = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font SubHeaderFont = new("Segoe UI", 10.5F, FontStyle.Bold);
    public static readonly Font RegularFont = new("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font TabularNumericFont = new("Consolas", 9.5F, FontStyle.Regular);
    public static readonly Font KpiNumberFont = new("Consolas", 18F, FontStyle.Bold);

    /// <summary>
    /// Định dạng DataGridView theo phong cách sang trọng, hiển thị số liệu căn gióng thẳng hàng.
    /// </summary>
    /// <param name="dgv">Bảng dữ liệu cần định dạng</param>
    public static void ApplyGridViewStyle(DataGridView dgv)
    {
        if (dgv == null) return;

        dgv.Dock = DockStyle.Fill;
        dgv.BackgroundColor = CardBackground;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = BorderColor;
        dgv.EnableHeadersVisualStyles = false;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.ScrollBars = ScrollBars.Both;
        dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dgv.RowTemplate.Height = 42;

        // Tiêu đề cột: Tự động đo đạc và giãn chiều cao theo nội dung thực tế (AutoSize)
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = DarkHeader;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
        dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        // Nội dung hàng xen kẽ: Tự động xuống dòng theo nội dung và giãn chiều cao theo độ dài văn bản
        dgv.DefaultCellStyle.BackColor = Color.White;
        dgv.DefaultCellStyle.ForeColor = TextPrimary;
        dgv.DefaultCellStyle.Font = RegularFont;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 48, 62);
        dgv.DefaultCellStyle.SelectionForeColor = PrimaryGold;
        dgv.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
        dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 48, 62);
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = PrimaryGold;
        dgv.AlternatingRowsDefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
        dgv.AlternatingRowsDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        // Giữ Alignment là NotSet để các dòng xen kẽ kế thừa 100% căn lề (trái/phải/giữa) của từng cột tương ứng
    }

    /// <summary>
    /// Tạo nút bấm phẳng sang trọng có hiệu ứng thị giác tinh tế.
    /// </summary>
    /// <param name="text">Nội dung nút</param>
    /// <param name="backColor">Màu nền</param>
    /// <param name="foreColor">Màu chữ</param>
    /// <returns>Đối tượng nút WinForms đã được cấu hình</returns>
    public static Button CreateFlatButton(string text, Color backColor, Color foreColor)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = backColor,
            ForeColor = foreColor,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Height = 36,
            Cursor = Cursors.Hand,
            AutoSize = true,
            UseMnemonic = false,
            Padding = new Padding(12, 4, 12, 4)
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    /// <summary>
    /// Tạo thanh công cụ tác vụ dạng FlowLayoutPanel linh hoạt, tự co giãn và chống tràn chữ khi thay đổi độ phân giải.
    /// </summary>
    /// <returns>Đối tượng FlowLayoutPanel chuẩn giao diện Blink Blink Jewelry</returns>
    public static FlowLayoutPanel CreateResponsiveToolbar()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(20, 10, 20, 10),
            BackColor = Color.White
        };
    }
}
