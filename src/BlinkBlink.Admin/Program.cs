using System;
using BlinkBlink.Admin.Forms;
using WinFormsApp = System.Windows.Forms.Application;

namespace BlinkBlink.Admin;

/// <summary>
/// Điểm khởi chạy chính của ứng dụng quản trị Windows Forms Blink Blink Jewelry.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Hàm Main khởi tạo ứng dụng desktop
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Khởi tạo cấu hình ứng dụng WinForms trong .NET 8 LTS (hỗ trợ High DPI, Visual Styles)
        ApplicationConfiguration.Initialize();

        // Vòng lặp xác thực đăng nhập: khi người dùng đăng xuất sẽ quay lại màn hình LoginForm
        while (true)
        {
            using var loginForm = new LoginForm();
            var loginResult = loginForm.ShowDialog();

            if (loginResult != System.Windows.Forms.DialogResult.OK || loginForm.AuthenticatedUser == null)
            {
                // Người dùng đóng cửa sổ hoặc bấm Thoát ứng dụng
                break;
            }

            using var mainForm = new MainForm(loginForm.AuthenticatedUser);
            WinFormsApp.Run(mainForm);

            // Nếu người dùng không yêu cầu đăng xuất (tắt cửa sổ X trực tiếp), thoát ứng dụng
            if (!mainForm.IsLogoutRequested)
            {
                break;
            }
        }
    }
}
