using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace BlinkBlink.IntegrationTests.WebTests;

/// <summary>
/// Lớp cơ sở cho các kịch bản kiểm thử E2E tự động hóa giao diện Web bằng Playwright.
/// Đảm bảo cấu hình Headless = true và IgnoreHTTPSErrors = true theo đúng yêu cầu dự án.
/// </summary>
public abstract class WebTestBase : IAsyncLifetime
{
	protected const string BaseUrl = "http://localhost:5095";
	protected IPlaywright? PlaywrightInstance;
	protected IBrowser? Browser;
	protected IBrowserContext? Context;
	protected IPage? Page;

	public virtual async Task InitializeAsync()
	{
		PlaywrightInstance = await Playwright.CreateAsync();
		// Đọc biến môi trường HEADED (mặc định là chạy ngầm Headless = true)
		bool isHeaded = Environment.GetEnvironmentVariable("HEADED") == "1";
		Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = !isHeaded,
			SlowMo = isHeaded ? 4000 : 0
		});

		Context = await Browser.NewContextAsync(new BrowserNewContextOptions
		{
			IgnoreHTTPSErrors = true,
			ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
		});

		Page = await Context.NewPageAsync();
	}

	public virtual async Task DisposeAsync()
	{
		if (Page != null)
		{
			await Page.CloseAsync();
		}
		if (Context != null)
		{
			await Context.CloseAsync();
		}
		if (Browser != null)
		{
			await Browser.CloseAsync();
		}
		PlaywrightInstance?.Dispose();
	}
}
// lệnh chạy test web: $env:HEADED="1"; dotnet test --filter "FullyQualifiedName~E2E"

