using System;
using System.Drawing;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Exceptions;

namespace BlinkBlink.Admin.Forms;

/// <summary>
/// Màn hình Đăng Nhập hệ thống quản trị và bán hàng POS Blink Blink Jewelry.
/// Thiết kế thanh lịch, ánh kim sang trọng theo chuẩn Taste Skill.
/// Xác thực người dùng nội bộ (ADMIN, SALE, WAREHOUSE, TECHNICIAN) từ CSDL SQL Server.
/// </summary>
public class LoginForm : Form
{
    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;
    private Label _lblInstruction = null!;
    private Label _lblError = null!;
    private Button _btnLogin = null!;
    private Button _btnRetry = null!;
    private Button _btnExit = null!;

    /// <summary>
    /// Đối tượng người dùng đã xác thực thành công.
    /// </summary>
    public NguoiDung? AuthenticatedUser { get; private set; }

    public LoginForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Đăng Nhập - Blink Blink Jewelry Management";
        // Sử dụng ClientSize để đảm bảo diện tích bên trong luôn cố định trên mọi tỷ lệ DPI / viền cửa sổ
        this.ClientSize = new Size(440, 480);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = true;
        this.BackColor = Color.White;
        this.Font = UITheme.RegularFont;

        // Header Panel sang trọng
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = UITheme.DarkHeader,
            Padding = new Padding(20, 15, 20, 10)
        };

        var lblBrand = new Label
        {
            Text = "BLINK BLINK JEWELRY",
            Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
            ForeColor = UITheme.PrimaryGold,
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblSubtitle = new Label
        {
            Text = "HỆ THỐNG QUẢN TRỊ & BÁN HÀNG KIM HOÀN",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Top,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _lblInstruction = new Label
        {
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            Dock = DockStyle.Top,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter
        };

        pnlHeader.Controls.Add(_lblInstruction);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblBrand);

        // Body Panel chứa các trường nhập liệu
        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(35, 15, 35, 15),
            BackColor = Color.White
        };

        var lblUserTitle = new Label
        {
            Text = "Tên đăng nhập:",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Location = new Point(35, 15),
            AutoSize = true,
            TabStop = false
        };

        _txtUsername = new TextBox
        {
            Location = new Point(35, 38),
            Width = 370,
            Height = 30,
            Font = new Font("Segoe UI", 10.5F),
            PlaceholderText = "Nhập tên đăng nhập...",
            TabIndex = 0,
            TabStop = true
        };

        var lblPassTitle = new Label
        {
            Text = "Mật khẩu:",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Location = new Point(35, 78),
            AutoSize = true,
            TabStop = false
        };

        _txtPassword = new TextBox
        {
            Location = new Point(35, 101),
            Width = 370,
            Height = 30,
            Font = new Font("Segoe UI", 10.5F),
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu...",
            TabIndex = 1,
            TabStop = true
        };

        _lblError = new Label
        {
            Location = new Point(35, 138),
            Width = 370,
            Height = 32,
            ForeColor = UITheme.DangerColor,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Visible = false,
            TabStop = false
        };

        _btnLogin = new Button
        {
            Text = "ĐĂNG NHẬP HỆ THỐNG",
            Location = new Point(35, 175),
            Width = 370,
            Height = 44,
            BackColor = UITheme.PrimaryGold,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabIndex = 2,
            TabStop = true
        };
        _btnLogin.FlatAppearance.BorderSize = 0;
        _btnLogin.Click += BtnLogin_Click;

        _btnRetry = new Button
        {
            Text = "🔄 Thử kết nối lại CSDL",
            Location = new Point(35, 227),
            Width = 370,
            Height = 36,
            BackColor = UITheme.DarkHeader,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabIndex = 3,
            TabStop = true,
            Visible = false
        };
        _btnRetry.FlatAppearance.BorderSize = 0;
        _btnRetry.Click += BtnRetry_Click;

        _btnExit = new Button
        {
            Text = "Thoát Ứng Dụng",
            Location = new Point(35, 271),
            Width = 370,
            Height = 34,
            BackColor = Color.White,
            ForeColor = UITheme.TextSecondary,
            Font = new Font("Segoe UI", 9F),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabIndex = 4,
            TabStop = true
        };
        _btnExit.FlatAppearance.BorderSize = 0;
        _btnExit.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        pnlBody.Controls.Add(lblUserTitle);
        pnlBody.Controls.Add(_txtUsername);
        pnlBody.Controls.Add(lblPassTitle);
        pnlBody.Controls.Add(_txtPassword);
        pnlBody.Controls.Add(_lblError);
        pnlBody.Controls.Add(_btnLogin);
        pnlBody.Controls.Add(_btnRetry);
        pnlBody.Controls.Add(_btnExit);

        this.Controls.Add(pnlBody);
        this.Controls.Add(pnlHeader);

        // Nhấn Enter để đăng nhập tự động
        this.AcceptButton = _btnLogin;
        this.CancelButton = _btnExit;

        // Cập nhật trạng thái hiển thị theo kết nối CSDL hiện hành
        UpdateDatabaseConnectionState(AdminServiceScope.Instance.IsDatabaseConnected);

        // Focus vào ô username khi mở
        this.Shown += (s, e) =>
        {
            if (AdminServiceScope.Instance.IsDatabaseConnected)
            {
                _txtUsername.Focus();
            }
            else
            {
                _btnRetry.Focus();
            }
        };
    }

    /// <summary>
    /// Cập nhật giao diện đồng bộ theo trạng thái kết nối cơ sở dữ liệu
    /// </summary>
    private void UpdateDatabaseConnectionState(bool isConnected)
    {
        if (isConnected)
        {
            _lblInstruction.Text = "Vui lòng đăng nhập tài khoản nội bộ để tiếp tục";
            _lblInstruction.ForeColor = Color.FromArgb(148, 163, 184);
            _btnLogin.Enabled = true;
            _btnLogin.BackColor = UITheme.PrimaryGold;
            _btnLogin.Cursor = Cursors.Hand;
            _btnRetry.Visible = false;
            _lblError.Visible = false;
            _txtUsername.Enabled = true;
            _txtPassword.Enabled = true;
        }
        else
        {
            _lblInstruction.Text = "🔴 Không thể kết nối cơ sở dữ liệu SQL Server!";
            _lblInstruction.ForeColor = UITheme.DangerColor;
            _btnLogin.Enabled = false;
            _btnLogin.BackColor = Color.FromArgb(226, 232, 240);
            _btnLogin.Cursor = Cursors.No;
            _btnRetry.Visible = true;
            _txtUsername.Enabled = false;
            _txtPassword.Enabled = false;
            ShowError("Chưa kết nối máy chủ CSDL. Bật SQL Server/Docker rồi bấm 'Thử kết nối lại'.");
        }
    }

    private void BtnRetry_Click(object? sender, EventArgs e)
    {
        _btnRetry.Enabled = false;
        _btnRetry.Text = "Đang kiểm tra kết nối...";
        System.Windows.Forms.Application.DoEvents();

        var connected = AdminServiceScope.Instance.TryConnectOrProvision();
        UpdateDatabaseConnectionState(connected);
        _btnRetry.Enabled = true;
        _btnRetry.Text = "🔄 Thử kết nối lại CSDL";

        if (connected)
        {
            MessageBox.Show(
                "Kết nối cơ sở dữ liệu SQL Server thành công!\nBạn có thể tiến hành đăng nhập.",
                "Kết nối thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            _txtUsername.Focus();
        }
        else
        {
            ShowError("Không tìm thấy máy chủ SQL Server khả dụng. Vui lòng kiểm tra Docker hoặc SQL Server.");
        }
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        _lblError.Visible = false;

        if (!AdminServiceScope.Instance.IsDatabaseConnected)
        {
            ShowError("Không thể đăng nhập: Hệ thống chưa kết nối được máy chủ cơ sở dữ liệu.");
            return;
        }

        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Vui lòng nhập tên đăng nhập.");
            _txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Vui lòng nhập mật khẩu.");
            _txtPassword.Focus();
            return;
        }

        try
        {
            var user = AdminServiceScope.Instance.NguoiDungRepo.TimTheoTenDangNhap(username);
            if (user == null || user.MatKhau != password)
            {
                AdminServiceScope.Instance.Logger.LogWarning(nameof(LoginForm), $"Đăng nhập thất bại cho tài khoản '{_txtUsername.Text.Trim()}'.", ErrorCodes.InvalidCredentials);
                ShowError("Tên đăng nhập hoặc mật khẩu không chính xác.");
                _txtPassword.SelectAll();
                _txtPassword.Focus();
                return;
            }

            if (!user.TrangThai)
            {
                AdminServiceScope.Instance.Logger.LogWarning(nameof(LoginForm), $"Từ chối đăng nhập tài khoản bị khóa '{user.TenDangNhap}'.", ErrorCodes.AccountLocked);
                ShowError("Tài khoản này hiện đang bị khóa quyền truy cập.");
                return;
            }

            var roleCode = user.MaVaiTro?.ToUpperInvariant();
            if (roleCode != "ADMIN" && roleCode != "SALE" && roleCode != "WAREHOUSE" && roleCode != "TECHNICIAN")
            {
                AdminServiceScope.Instance.Logger.LogWarning(nameof(LoginForm), $"Tài khoản '{user.TenDangNhap}' không có quyền truy cập WinForms ({roleCode}).", ErrorCodes.AccountLocked);
                ShowError("Tài khoản của bạn không có quyền truy cập vào phần mềm quản trị.");
                return;
            }

            // Ghi nhận phiên đăng nhập thành công
            AuthenticatedUser = user;
            AdminServiceScope.Instance.CurrentUser = user;
            if (user is NhanVien nv)
            {
                AdminServiceScope.Instance.CurrentStaff = nv;
            }

            AdminServiceScope.Instance.Logger.LogInfo(nameof(LoginForm), $"Nhân sự '{user.TenDangNhap}' [{user.MaVaiTro}] đăng nhập thành công.");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            var traceId = AdminErrorHandler.Handle(ex, nameof(LoginForm), showDialog: false);
            ShowError($"Lỗi xác thực hệ thống (Mã tra cứu: {traceId}).");
        }
    }

    private void ShowError(string msg)
    {
        _lblError.Text = "⚠️ " + msg;
        _lblError.Visible = true;
    }
}
