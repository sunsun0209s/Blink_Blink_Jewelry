using System;
using System.Drawing;
using System.Windows.Forms;
using BlinkBlink.Admin.Services;
using BlinkBlink.Admin.Views.Helpers;
using BlinkBlink.Domain.Entities.Identity;

namespace BlinkBlink.Admin.Forms;

/// <summary>
/// Hộp thoại Đổi Mật Khẩu cho người dùng đang đăng nhập hệ thống.
/// Thiết kế thanh lịch, bảo mật, cập nhật trực tiếp vào cơ sở dữ liệu.
/// </summary>
public class ChangePasswordDialog : Form
{
    private readonly NguoiDung _user;
    private TextBox _txtOldPass = null!;
    private TextBox _txtNewPass = null!;
    private TextBox _txtConfirmPass = null!;
    private Label _lblError = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;

    public ChangePasswordDialog(NguoiDung user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Đổi Mật Khẩu - Blink Blink Jewelry";
        this.Size = new Size(420, 430);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;
        this.Font = UITheme.RegularFont;

        // Top Header
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = UITheme.DarkHeader,
            Padding = new Padding(20, 12, 20, 10)
        };

        var lblTitle = new Label
        {
            Text = "🔑 THIẾT LẬP MẬT KHẨU MỚI",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UITheme.PrimaryGold,
            Dock = DockStyle.Top,
            Height = 24
        };

        var lblUser = new Label
        {
            Text = $"Tài khoản: {_user.TenDangNhap} ({_user.HoTen})",
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(203, 213, 225),
            Dock = DockStyle.Top,
            Height = 20
        };

        pnlHeader.Controls.Add(lblUser);
        pnlHeader.Controls.Add(lblTitle);

        // Body
        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 15, 30, 15),
            BackColor = Color.White
        };

        var lblOld = new Label
        {
            Text = "Mật khẩu hiện tại:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Location = new Point(30, 15),
            AutoSize = true
        };

        _txtOldPass = new TextBox
        {
            Location = new Point(30, 36),
            Width = 345,
            Height = 28,
            Font = new Font("Segoe UI", 10.5F),
            UseSystemPasswordChar = true
        };

        var lblNew = new Label
        {
            Text = "Mật khẩu mới:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Location = new Point(30, 75),
            AutoSize = true
        };

        _txtNewPass = new TextBox
        {
            Location = new Point(30, 96),
            Width = 345,
            Height = 28,
            Font = new Font("Segoe UI", 10.5F),
            UseSystemPasswordChar = true
        };

        var lblConfirm = new Label
        {
            Text = "Xác nhận mật khẩu mới:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Location = new Point(30, 135),
            AutoSize = true
        };

        _txtConfirmPass = new TextBox
        {
            Location = new Point(30, 156),
            Width = 345,
            Height = 28,
            Font = new Font("Segoe UI", 10.5F),
            UseSystemPasswordChar = true
        };

        _lblError = new Label
        {
            Location = new Point(30, 195),
            Width = 345,
            Height = 40,
            ForeColor = UITheme.DangerColor,
            Font = new Font("Segoe UI", 8.5F),
            Visible = false
        };

        _btnSave = new Button
        {
            Text = "✓ CẬP NHẬT MẬT KHẨU",
            Location = new Point(30, 245),
            Width = 345,
            Height = 40,
            BackColor = UITheme.PrimaryGold,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;

        _btnCancel = new Button
        {
            Text = "Hủy Bỏ",
            Location = new Point(30, 292),
            Width = 345,
            Height = 30,
            BackColor = Color.White,
            ForeColor = UITheme.TextSecondary,
            Font = new Font("Segoe UI", 9F),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => this.Close();

        pnlBody.Controls.Add(_btnCancel);
        pnlBody.Controls.Add(_btnSave);
        pnlBody.Controls.Add(_lblError);
        pnlBody.Controls.Add(_txtConfirmPass);
        pnlBody.Controls.Add(lblConfirm);
        pnlBody.Controls.Add(_txtNewPass);
        pnlBody.Controls.Add(lblNew);
        pnlBody.Controls.Add(_txtOldPass);
        pnlBody.Controls.Add(lblOld);

        this.Controls.Add(pnlBody);
        this.Controls.Add(pnlHeader);

        this.AcceptButton = _btnSave;
        this.CancelButton = _btnCancel;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        _lblError.Visible = false;
        var oldPass = _txtOldPass.Text.Trim();
        var newPass = _txtNewPass.Text.Trim();
        var confirmPass = _txtConfirmPass.Text.Trim();

        if (string.IsNullOrWhiteSpace(oldPass))
        {
            ShowError("Vui lòng nhập mật khẩu hiện tại.");
            _txtOldPass.Focus();
            return;
        }

        if (oldPass != _user.MatKhau)
        {
            ShowError("Mật khẩu hiện tại không đúng.");
            _txtOldPass.SelectAll();
            _txtOldPass.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 4)
        {
            ShowError("Mật khẩu mới phải có ít nhất 4 ký tự.");
            _txtNewPass.Focus();
            return;
        }

        if (newPass != confirmPass)
        {
            ShowError("Mật khẩu xác nhận không khớp.");
            _txtConfirmPass.SelectAll();
            _txtConfirmPass.Focus();
            return;
        }

        try
        {
            _user.MatKhau = newPass;
            AdminServiceScope.Instance.NguoiDungRepo.CapNhat(_user);

            MessageBox.Show(
                "Mật khẩu đã được thay đổi thành công!",
                "Thành Công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Lỗi cập nhật mật khẩu: {ex.Message}");
        }
    }

    private void ShowError(string msg)
    {
        _lblError.Text = "⚠️ " + msg;
        _lblError.Visible = true;
    }
}
