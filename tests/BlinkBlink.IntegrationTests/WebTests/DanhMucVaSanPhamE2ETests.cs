using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace BlinkBlink.IntegrationTests.WebTests;

/// <summary>
/// Kiểm thử E2E Chức năng Danh mục và Sản phẩm trang sức (Catalog & QuickView).
/// </summary>
public class DanhMucVaSanPhamE2ETests : WebTestBase
{
    [Fact]
    public async Task KiemTra_BoLocDanhMuc_Va_NapDanhSachSanPham()
    {
        Assert.NotNull(Page);

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 1. Kiểm tra nhóm nút lọc danh mục sản phẩm
        var filterGroup = Page.Locator("#category-filter-group");
        await filterGroup.WaitForAsync();
        Assert.True(await filterGroup.IsVisibleAsync());

        var filterButtons = Page.Locator("#category-filter-group .filter-pill-btn");
        var filterCount = await filterButtons.CountAsync();
        Assert.True(filterCount >= 4, "Phải có ít nhất 4 nút phân loại danh mục trang sức.");

        // Nút 'Tất Cả' phải được kích hoạt mặc định
        var activeBtn = Page.Locator("#category-filter-group .filter-pill-btn.active");
        Assert.Equal("Tất Cả", (await activeBtn.InnerTextAsync()).Trim());

        // 2. Chờ lưới sản phẩm nạp dữ liệu từ API CSDL
        var firstProductCard = Page.Locator("#products-grid .product-card").First;
        await firstProductCard.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        Assert.True(await firstProductCard.IsVisibleAsync());

        var productCards = Page.Locator("#products-grid .product-card");
        var count = await productCards.CountAsync();
        Assert.True(count > 0, "Lưới sản phẩm phải hiển thị ít nhất một kiệt tác trang sức.");

        // 3. Kiểm tra tương tác chuyển đổi danh mục
        var nhanKimCuongBtn = Page.Locator("#category-filter-group button[data-category='DM01']");
        if (await nhanKimCuongBtn.CountAsync() > 0)
        {
            await nhanKimCuongBtn.ClickAsync();
            var classAttr = await nhanKimCuongBtn.GetAttributeAsync("class");
            Assert.Contains("active", classAttr ?? string.Empty);
        }
    }

    [Fact]
    public async Task KiemTra_MoChiTiet_XemNhanhSanPham()
    {
        Assert.NotNull(Page);

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 1. Chờ sản phẩm đầu tiên xuất hiện
        var firstCard = Page.Locator("#products-grid .product-card").First;
        await firstCard.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        // 2. Click nút Xem Nhanh
        var btnQuickView = firstCard.Locator(".btn-card-quickview");
        await btnQuickView.WaitForAsync();
        await btnQuickView.ClickAsync();

        // 3. Kiểm tra hộp thoại QuickView hiển thị
        var quickViewModal = Page.Locator("#quickview-modal");
        await quickViewModal.WaitForAsync();
        var modalClass = await quickViewModal.GetAttributeAsync("class");
        Assert.Contains("active", modalClass ?? string.Empty);

        // Kiểm tra thông tin hiển thị trong modal
        var modalTitle = Page.Locator("#modal-title");
        var modalPrice = Page.Locator("#modal-price");
        var modalBtnAdd = Page.Locator("#modal-btn-add");

        var titleText = await modalTitle.InnerTextAsync();
        var priceText = await modalPrice.InnerTextAsync();

        Assert.False(string.IsNullOrWhiteSpace(titleText), "Tên sản phẩm trong QuickView không được rỗng.");
        Assert.Contains("₫", priceText);
        Assert.True(await modalBtnAdd.IsVisibleAsync());

        // 4. Đóng hộp thoại QuickView
        var btnClose = Page.Locator("#btn-close-modal");
        await btnClose.ClickAsync();

        // Kiểm tra modal đã đóng (không còn class active)
        var closedClass = await quickViewModal.GetAttributeAsync("class");
        Assert.DoesNotContain("active", closedClass ?? string.Empty);
    }
}

