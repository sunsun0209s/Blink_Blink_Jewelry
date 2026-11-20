using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace BlinkBlink.IntegrationTests.WebTests;

/// <summary>
/// Kiểm thử E2E Chức năng Xác thực & Quản lý Tài khoản (Auth Modal, Login, Register).
/// </summary>
public class XacThucE2ETests : WebTestBase
{
    [Fact]
    public async Task KiemTra_MoHopThoai_ChuyenDoiTab_DangNhap_DangKy()
    {
        Assert.NotNull(Page);

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 1. Click nút Đăng Nhập / Đăng Ký tại Header
        var btnAuth = Page.Locator("#btn-auth-user");
        await btnAuth.WaitForAsync();
        await btnAuth.ClickAsync();

        // 2. Kiểm tra Modal mở và có class active
        var authModal = Page.Locator("#auth-modal");
        await authModal.WaitForAsync();
        var modalClass = await authModal.GetAttributeAsync("class");
        Assert.Contains("active", modalClass ?? string.Empty);

        // 3. Tab Đăng Nhập phải active mặc định
        var tabLogin = Page.Locator("#tab-btn-login");
        var paneLogin = Page.Locator("#pane-login");
        Assert.Contains("active", await tabLogin.GetAttributeAsync("class") ?? string.Empty);
        Assert.Contains("active", await paneLogin.GetAttributeAsync("class") ?? string.Empty);

        // Kiểm tra các trường nhập liệu Đăng nhập
        var inputUser = Page.Locator("#login-username");
        var inputPass = Page.Locator("#login-password");
        Assert.True(await inputUser.IsVisibleAsync());
        Assert.True(await inputPass.IsVisibleAsync());

        // 4. Chuyển sang Tab Đăng Ký
        var tabRegister = Page.Locator("#tab-btn-register");
        await tabRegister.ClickAsync();

        var paneRegister = Page.Locator("#pane-register");
        Assert.Contains("active", await tabRegister.GetAttributeAsync("class") ?? string.Empty);
        Assert.Contains("active", await paneRegister.GetAttributeAsync("class") ?? string.Empty);

        // Kiểm tra các trường nhập liệu Đăng ký
        var regFullname = Page.Locator("#reg-fullname");
        var regPhone = Page.Locator("#reg-phone");
        var regDob = Page.Locator("#reg-dob");
        var regUser = Page.Locator("#reg-username");
        var regPass = Page.Locator("#reg-password");

        Assert.True(await regFullname.IsVisibleAsync());
        Assert.True(await regPhone.IsVisibleAsync());
        Assert.True(await regDob.IsVisibleAsync());
        Assert.True(await regUser.IsVisibleAsync());
        Assert.True(await regPass.IsVisibleAsync());

        // 5. Đóng Modal xác thực
        var btnClose = Page.Locator("#btn-close-auth");
        await btnClose.ClickAsync();

        var closedClass = await authModal.GetAttributeAsync("class");
        Assert.DoesNotContain("active", closedClass ?? string.Empty);
    }

    [Fact]
    public async Task KiemTra_DangNhap_BaoLoiKhiSaiThongTin()
    {
        Assert.NotNull(Page);

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 1. Mở Modal Đăng Nhập
        var btnAuth = Page.Locator("#btn-auth-user");
        await btnAuth.WaitForAsync();
        await btnAuth.ClickAsync();

        var authModal = Page.Locator("#auth-modal");
        await authModal.WaitForAsync();

        // 2. Điền thông tin đăng nhập không tồn tại
        var inputUser = Page.Locator("#login-username");
        var inputPass = Page.Locator("#login-password");
        await inputUser.FillAsync("user_khong_hop_le_99999");
        await inputPass.FillAsync("SaiMatKhau123!");

        // 3. Nhấn nút Đăng Nhập Ngay
        var submitBtn = Page.Locator("#form-login button[type='submit']");
        await submitBtn.ClickAsync();

        // 4. Kiểm tra banner phản hồi lỗi xuất hiện
        var feedbackBanner = Page.Locator("#login-feedback-banner");
        await feedbackBanner.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.True(await feedbackBanner.IsVisibleAsync());

        var feedbackText = Page.Locator("#login-feedback-text");
        var message = await feedbackText.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(message), "Thông báo lỗi không được để trống.");

        // 5. Đóng Modal
        var btnClose = Page.Locator("#btn-close-auth");
        await btnClose.ClickAsync();
    }
}

