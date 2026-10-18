using System.Security.Claims;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các yêu cầu xác thực tài khoản người dùng
/// Tương ứng với XacThucController trong swimlane Interface Adapters (Presentation) của Diagram
/// </summary>
public class XacThucController : Controller
{
    // Thuộc tính phụ thuộc theo đúng Diagram
    private readonly DangNhapUseCase _dangNhapUseCase;

    /// <summary>
    /// Khởi tạo XacThucController với Use Case đăng nhập được tiêm qua Dependency Injection
    /// </summary>
    /// <param name="dangNhapUseCase">Use Case xử lý đăng nhập</param>
    public XacThucController(DangNhapUseCase dangNhapUseCase)
    {
        _dangNhapUseCase = dangNhapUseCase;
    }

    /// <summary>
    /// Hiển thị giao diện màn hình Đăng nhập
    /// </summary>
    /// <returns>View đăng nhập</returns>
    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    /// <summary>
    /// Xử lý gửi biểu mẫu đăng nhập từ người dùng
    /// Khớp 100% với phương thức HttpPost DangNhap(request: DangNhapDTO) trong Diagram
    /// </summary>
    /// <param name="request">DTO chứa thông tin đăng nhập (TenDangNhap, MatKhau)</param>
    /// <returns>Chuyển hướng về trang chủ nếu thành công, hoặc trả lại View kèm thông báo lỗi</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap([FromForm] DangNhapDTO request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            // Thực thi ca sử dụng đăng nhập (Step 1 trong flow xác thực)
            var tokenOrRole = _dangNhapUseCase.Execute(request);

            // Thiết lập phiên Claims Cookie Authentication cho người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.TenDangNhap),
                new Claim(ClaimTypes.Role, tokenOrRole)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Lưu tên đăng nhập vào Session để hiển thị trên giao diện
            HttpContext.Session.SetString("TenDangNhap", request.TenDangNhap);
            HttpContext.Session.SetString("VaiTro", tokenOrRole);

            TempData["ThongBaoThanhCong"] = "Đăng nhập thành công! Chào mừng bạn quay trở lại.";
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
            return View(request);
        }
    }

    /// <summary>
    /// Xử lý đăng xuất tài khoản và xóa phiên làm việc
    /// </summary>
    /// <returns>Chuyển hướng về trang chủ</returns>
    [HttpGet]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        TempData["ThongBaoThanhCong"] = "Đã đăng xuất tài khoản an toàn.";
        return RedirectToAction("Index", "Home");
    }
}
