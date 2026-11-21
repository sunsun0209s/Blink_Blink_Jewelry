using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace BlinkBlink.IntegrationTests.WebTests;

/// <summary>
/// Kiểm thử E2E Giao diện Trang Chủ (Storefront) của thương hiệu Blink Blink Haute Joaillerie.
/// </summary>
public class TrangChuE2ETests : WebTestBase
{
	[Fact]
	public async Task KiemTra_TrangChu_HienThiThanhCong_TieuDeVaThuongHieu()
	{
		Assert.NotNull(Page);

		// 1. Điều hướng đến trang chủ Storefront
		await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

		// 2. Kiểm tra tiêu đề trang chứa tên thương hiệu
		var title = await Page.TitleAsync();
		Assert.Contains("Blink Blink", title, StringComparison.OrdinalIgnoreCase);

		// 3. Kiểm tra logo thương hiệu tại Header
		var brandLogo = Page.Locator("header .brand-title");
		await brandLogo.WaitForAsync();
		var brandText = await brandLogo.InnerTextAsync();
		Assert.Contains("Blink Blink", brandText, StringComparison.OrdinalIgnoreCase);

		// 4. Kiểm tra thanh thông báo danh giá phía trên cùng (top announcement bar)
		var announcementBar = Page.Locator(".top-announcement-bar");
		Assert.True(await announcementBar.IsVisibleAsync());
		var announcementText = await announcementBar.InnerTextAsync();
		Assert.Contains("Kim Cương Thiên Nhiên", announcementText, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task KiemTra_MenuDieuHuong_Va_HeroSection()
	{
		Assert.NotNull(Page);

		await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

		// 1. Kiểm tra các mục menu điều hướng chính
		var navLinks = Page.Locator(".nav-links a");
		var count = await navLinks.CountAsync();
		Assert.True(count >= 4, "Menu chính phải có tối thiểu 4 liên kết điều hướng.");

		var allNavTexts = await navLinks.AllInnerTextsAsync();
		Assert.Contains(allNavTexts, t => t.Contains("Trang Chủ", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(allNavTexts, t => t.Contains("Bộ Sưu Tập", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(allNavTexts, t => t.Contains("Sản Phẩm", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(allNavTexts, t => t.Contains("Tra Cứu Bảo Hành", StringComparison.OrdinalIgnoreCase));

		// 2. Kiểm tra Hero Section tạp chí kim hoàn
		var heroHeading = Page.Locator(".hero-heading");
		await heroHeading.WaitForAsync();
		Assert.True(await heroHeading.IsVisibleAsync());
		var heroText = await heroHeading.InnerTextAsync();
		Assert.Contains("Tuyệt Tác Kim Hoàn", heroText, StringComparison.OrdinalIgnoreCase);

		// 3. Kiểm tra 4 trụ cột giá trị thương hiệu (Brand Value Pillars)
		var pillars = Page.Locator(".pillar-item");
		var pillarCount = await pillars.CountAsync();
		Assert.Equal(4, pillarCount);
	}

	[Fact]
	public async Task KiemTra_ThanhTimKiem_Va_NutHanhDongHeader()
	{
		Assert.NotNull(Page);

		await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

		// 1. Kiểm tra ô tìm kiếm kim hoàn ở Header
		var searchInput = Page.Locator("#header-search-input");
		Assert.NotNull(searchInput);

		// 2. Kiểm tra nút tài khoản khách hàng
		var btnAuth = Page.Locator("#btn-auth-user");
		Assert.True(await btnAuth.IsVisibleAsync());
		var authText = await btnAuth.InnerTextAsync();
		Assert.Contains("Đăng Nhập", authText, StringComparison.OrdinalIgnoreCase);

		// 3. Kiểm tra nút giỏ hàng và huy hiệu số lượng
		var btnCart = Page.Locator("#btn-open-cart");
		Assert.True(await btnCart.IsVisibleAsync());
		var cartBadge = Page.Locator("#cart-badge");
		Assert.True(await cartBadge.IsVisibleAsync());
	}
}