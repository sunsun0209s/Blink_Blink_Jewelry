using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Web.Models;

namespace BlinkBlink.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Điều hướng hiển thị trang tìm kiếm kiệt tác kim hoàn và bộ lọc nâng cao riêng biệt.
    /// </summary>
    /// <returns>Giao diện trang tìm kiếm sản phẩm TimKiem.cshtml</returns>
    [HttpGet("tim-kiem")]
    [HttpGet("TimKiem")]
    public IActionResult TimKiem()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? traceId = null)
    {
        var requestId = !string.IsNullOrWhiteSpace(traceId)
            ? traceId
            : (Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
