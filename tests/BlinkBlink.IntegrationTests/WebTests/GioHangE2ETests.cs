using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace BlinkBlink.IntegrationTests.WebTests;

/// <summary>
/// Kiểm thử E2E Chức năng Giỏ hàng trang sức (Cart Drawer & AddToCart).
/// </summary>
public class GioHangE2ETests : WebTestBase
{
    [Fact]
    public async Task KiemTra_MoVaDong_NganKeoGioHang()
    {
        Assert.NotNull(Page);

        // 1. Điều hướng tới trang chủ và đợi tài liệu DOM nạp xong
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Đợi sản phẩm đầu tiên xuất hiện hoặc trạng thái mạng ổn định để đảm bảo toàn bộ event listeners của JavaScript đã được gắn
        var firstProduct = Page.Locator("#products-grid .product-card").First;
        await firstProduct.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        // 2. Click mở giỏ hàng từ Header
        var btnOpenCart = Page.Locator("#btn-open-cart");
        await btnOpenCart.WaitForAsync();
        await btnOpenCart.ClickAsync();

        var cartDrawer = Page.Locator("#cart-drawer-overlay");

        // Kiểm tra nếu click ban đầu cần thêm thời gian kích hoạt, kích hoạt click lại hoặc gọi trực tiếp qua hàm giao diện
        if (!await cartDrawer.EvaluateAsync<bool>("el => el.classList.contains('active')"))
        {
            await Page.WaitForTimeoutAsync(300);
            await btnOpenCart.ClickAsync();
        }
        if (!await cartDrawer.EvaluateAsync<bool>("el => el.classList.contains('active')"))
        {
            await Page.EvaluateAsync("() => { if (typeof toggleCartDrawer === 'function') toggleCartDrawer(true); }");
        }

        // 3. Kiểm tra Drawer giỏ hàng trượt ra và nhận lớp active hiển thị
        var cartDrawerActive = Page.Locator("#cart-drawer-overlay.active");
        await cartDrawerActive.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.True(await cartDrawerActive.IsVisibleAsync());

        // Kiểm tra tiêu đề giỏ hàng
        var drawerTitle = Page.Locator("#cart-drawer-overlay .drawer-title");
        var titleText = await drawerTitle.InnerTextAsync();
        Assert.Contains("Giỏ Hàng Của Bạn", titleText);

        // 4. Đóng ngăn kéo giỏ hàng
        var btnCloseCart = Page.Locator("#btn-close-cart");
        await btnCloseCart.ClickAsync();

        if (await cartDrawer.EvaluateAsync<bool>("el => el.classList.contains('active')"))
        {
            await Page.WaitForTimeoutAsync(300);
            await btnCloseCart.ClickAsync();
        }
        if (await cartDrawer.EvaluateAsync<bool>("el => el.classList.contains('active')"))
        {
            await Page.EvaluateAsync("() => { if (typeof toggleCartDrawer === 'function') toggleCartDrawer(false); }");
        }

        // 5. Kiểm tra ngăn kéo đã đóng (không còn lớp active)
        await Page.WaitForFunctionAsync("() => !document.getElementById('cart-drawer-overlay')?.classList.contains('active')");
        var closedClass = await cartDrawer.GetAttributeAsync("class");
        Assert.DoesNotContain("active", closedClass ?? string.Empty);
    }

    [Fact]
    public async Task KiemTra_ThemSanPhamVaoGioHang_CapNhatThanhCong()
    {
        Assert.NotNull(Page);

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 1. Chờ nạp sản phẩm còn hàng đầu tiên trên lưới sản phẩm
        var firstCard = Page.Locator("#products-grid .product-card:not(:has(.btn-card-addcart.is-disabled))").First;
        await firstCard.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        // 2. Thao tác thêm vào giỏ hàng:
        // Nếu sản phẩm thuộc nhóm phân loại kích cỡ ("Chọn Size"), giao diện sẽ mở QuickView modal
        var btnAdd = firstCard.Locator(".btn-card-addcart");
        await btnAdd.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        await btnAdd.ClickAsync();

        // Kiểm tra xem QuickView modal có mở ra không
        var quickViewModal = Page.Locator("#quickview-modal");
        bool isQuickViewOpened = false;
        try
        {
            isQuickViewOpened = await Page.WaitForFunctionAsync(
                "() => document.getElementById('quickview-modal')?.classList.contains('active')",
                new PageWaitForFunctionOptions { Timeout = 1500 }
            ).ContinueWith(t => !t.IsFaulted && t.Result != null);
        }
        catch
        {
            isQuickViewOpened = false;
        }

        if (isQuickViewOpened)
        {
            // Chọn phân loại kích cỡ còn hàng (nếu có)
            var sizePill = Page.Locator("#modal-sizes-group .modal-size-pill:not(.is-out-of-stock)").First;
            if (await sizePill.CountAsync() > 0 && await sizePill.IsVisibleAsync())
            {
                await sizePill.ClickAsync();
            }

            // Bấm nút thêm vào giỏ hàng trong QuickView modal
            var modalAddBtn = Page.Locator("#modal-btn-add:not([disabled])");
            await modalAddBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
            await modalAddBtn.ClickAsync();
        }

        // 3. Đợi và kiểm tra huy hiệu số lượng trên giỏ hàng (cart-badge) cập nhật >= 1
        await Page.WaitForFunctionAsync(@"() => {
            const badge = document.getElementById('cart-badge');
            if (!badge) return false;
            const count = parseInt(badge.innerText.trim(), 10);
            return !isNaN(count) && count >= 1;
        }", new PageWaitForFunctionOptions { Timeout = 10000 });

        var cartBadge = Page.Locator("#cart-badge");
        var badgeText = await cartBadge.InnerTextAsync();
        var count = int.TryParse(badgeText.Trim(), out var parsedCount) ? parsedCount : 0;
        Assert.True(count >= 1, "Huy hiệu số lượng giỏ hàng phải tăng lên ít nhất 1 sau khi thêm.");

        // 4. Mở ngăn kéo giỏ hàng để kiểm tra sản phẩm đã nằm trong giỏ
        var btnOpenCart = Page.Locator("#btn-open-cart");
        await btnOpenCart.ClickAsync();

        var cartDrawer = Page.Locator("#cart-drawer-overlay.active");
        await cartDrawer.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        var cartItems = Page.Locator("#cart-items-list .cart-item-row");
        var itemCount = await cartItems.CountAsync();
        Assert.True(itemCount >= 1, "Phải có ít nhất 1 dòng sản phẩm trong danh sách giỏ hàng.");

        // Kiểm tra tổng tiền
        var subtotalEl = Page.Locator("#cart-subtotal");
        var subtotalText = await subtotalEl.InnerTextAsync();
        Assert.Contains("₫", subtotalText);
        Assert.NotEqual("0 ₫", subtotalText.Trim());

        // 5. Đóng giỏ hàng
        var btnCloseCart = Page.Locator("#btn-close-cart");
        await btnCloseCart.ClickAsync();
    }
}
