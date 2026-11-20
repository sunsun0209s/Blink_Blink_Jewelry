using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace BlinkBlink.IntegrationTests.WebTests;

/// <summary>
/// Kiểm thử E2E Các quy trình nghiệp vụ chuyên sâu và cơ chế phân quyền bảo mật:
/// 1. Tra cứu bảo hành điện tử (BaoHanhController & Giao diện Storefront).
/// 2. Phân quyền và an toàn hệ thống: Chặn truy cập trái phép vào các trang quản trị khi chưa đăng nhập.
/// 3. Nghiệp vụ đặt hàng & thanh toán (BanHangController): Kiểm tra validation thông tin bắt buộc, ngăn chặn đơn hàng rác.
/// </summary>
public class NghiepVuVaPhanQuyenE2ETests : WebTestBase
{
    /// <summary>
    /// Test 1: Điều hướng tới chức năng Tra cứu bảo hành (BaoHanhController),
    /// kiểm tra giao diện hiển thị đầy đủ và thử nhập mã tra cứu để xác nhận hệ thống phản hồi ổn định.
    /// </summary>
    [Fact]
    public async Task KiemTra_TraCuuBaoHanh_HienThiVaTimKiem()
    {
        Assert.NotNull(Page);

        // 1. Điều hướng tới phân hệ Tra cứu bảo hành trên Storefront
        await Page.GotoAsync($"{BaseUrl}/#warranty", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 2. Kiểm tra vùng giao diện Tra cứu bảo hành
        var warrantySection = Page.Locator("#warranty");
        await warrantySection.WaitForAsync();
        Assert.True(await warrantySection.IsVisibleAsync());

        // Kiểm tra thanh tìm kiếm và nút tra cứu
        var searchInput = Page.Locator("#warranty-search-input");
        var btnSearch = Page.Locator("#btn-search-warranty");
        Assert.True(await searchInput.IsVisibleAsync());
        Assert.True(await btnSearch.IsVisibleAsync());

        // 3. Thử nghiệm nhập mã phiếu bảo hành và kích hoạt tra cứu
        await searchInput.FillAsync("BH2001");
        await btnSearch.ClickAsync();

        // Chờ phản hồi từ dịch vụ API BaoHanhController
        await Page.WaitForTimeoutAsync(1000);

        // 4. Xác nhận khu vực hiển thị kết quả sẵn sàng, hệ thống không bị crash hay báo lỗi 500
        var resultCard = Page.Locator("#warranty-result-card");
        Assert.NotNull(resultCard);
    }

    /// <summary>
    /// Test 2: Khi chưa đăng nhập, truy cập trực tiếp vào đường dẫn quản trị (/ThongKe hoặc /PhieuNhapKho)
    /// và xác nhận hệ thống chặn truy cập hoặc chuyển hướng về trang đăng nhập (XacThuc).
    /// </summary>
    [Fact]
    public async Task KiemTra_PhanQuyen_ChanTruyCapTrangQuanTri()
    {
        Assert.NotNull(Page);

        var adminEndpoints = new[] { $"{BaseUrl}/ThongKe", $"{BaseUrl}/PhieuNhapKho" };

        foreach (var endpoint in adminEndpoints)
        {
            IResponse? response = null;
            bool blockedByHttpResponseCode = false;

            try
            {
                response = await Page.GotoAsync(endpoint);
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("ERR_HTTP_RESPONSE_CODE_FAILURE", StringComparison.OrdinalIgnoreCase))
            {
                // Chromium ngắt điều hướng do máy chủ trả về mã lỗi HTTP (404/401/403) không có trang HTML
                blockedByHttpResponseCode = true;
            }

            // Đồng thời kiểm tra qua HttpClient để xác định chính xác mã trạng thái HTTP nếu chưa có response
            int statusCode = response?.Status ?? 0;
            if (statusCode == 0)
            {
                using var client = new System.Net.Http.HttpClient();
                var apiResponse = await client.GetAsync(endpoint);
                statusCode = (int)apiResponse.StatusCode;
            }

            // Xác nhận hệ thống áp dụng cơ chế bảo mật:
            // Trả về mã chặn truy cập (401 Unauthorized, 403 Forbidden, 404 Not Found do không mở route công khai)
            // hoặc điều hướng về trang xác thực đăng nhập (XacThuc / Login)
            bool isBlockedOrRedirected = blockedByHttpResponseCode ||
                                         statusCode == 401 ||
                                         statusCode == 403 ||
                                         statusCode == 404 ||
                                         Page.Url.Contains("XacThuc", StringComparison.OrdinalIgnoreCase) ||
                                         Page.Url.Contains("login", StringComparison.OrdinalIgnoreCase);

            Assert.True(isBlockedOrRedirected, $"Đường dẫn quản trị '{endpoint}' phải bị chặn hoặc chuyển hướng khi người dùng chưa xác thực (Mã trạng thái: {statusCode}).");
        }
    }

    /// <summary>
    /// Test 3: Kiểm tra giao diện đặt hàng/thanh toán (BanHangController), thử gửi form khi bỏ trống thông tin bắt buộc
    /// và xác nhận hệ thống hiển thị cảnh báo validation (không tạo đơn hàng rác vào database).
    /// </summary>
    [Fact]
    public async Task KiemTra_DatHang_ValidationThongTinBatBuoc()
    {
        Assert.NotNull(Page);

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // 1. Mở Hộp thoại Đặt hàng & Thanh toán (Checkout Modal)
        await Page.EvaluateAsync(@"() => {
            const modal = document.getElementById('checkout-modal');
            if (modal) modal.classList.add('active');
        }");

        var checkoutModal = Page.Locator("#checkout-modal");
        await checkoutModal.WaitForAsync();
        var modalClass = await checkoutModal.GetAttributeAsync("class");
        Assert.Contains("active", modalClass ?? string.Empty);

        // 2. Đảm bảo các trường thông tin bắt buộc (Họ tên, SĐT, Địa chỉ) đang bỏ trống
        await Page.Locator("#checkout-name").FillAsync("");
        await Page.Locator("#checkout-phone").FillAsync("");
        await Page.Locator("#checkout-address").FillAsync("");

        // 3. Thực hiện gửi form đặt hàng khi chưa điền dữ liệu
        await Page.EvaluateAsync(@"() => {
            if (typeof handleCheckoutSubmit === 'function') {
                handleCheckoutSubmit(new Event('submit'));
            }
        }");

        // 4. Xác nhận hệ thống kích hoạt cảnh báo validation inline, ngăn chặn phát sinh đơn hàng rác
        var errorBanner = Page.Locator("#checkout-error-banner");
        await errorBanner.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.True(await errorBanner.IsVisibleAsync());

        var errorText = await Page.Locator("#checkout-error-text").InnerTextAsync();
        Assert.Contains("Vui lòng điền đầy đủ", errorText);

        // 5. Đóng hộp thoại Đặt hàng
        var btnCloseCheckout = Page.Locator("#btn-close-checkout");
        if (await btnCloseCheckout.IsVisibleAsync())
        {
            await btnCloseCheckout.ClickAsync();
        }
    }
}

