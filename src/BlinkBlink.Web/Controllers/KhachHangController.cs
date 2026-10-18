using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các chức năng liên quan đến tài khoản khách hàng
/// Tương ứng với KhachHangController trong swimlane Interface Adapters (Presentation) của Diagram
/// </summary>
public class KhachHangController : Controller
{
    // Thuộc tính phụ thuộc theo đúng Diagram
    private readonly DangKyKhachHangUseCase _dangKyUseCase;

    /// <summary>
    /// Khởi tạo KhachHangController với Use Case đăng ký được tiêm qua DI
    /// </summary>
    /// <param name="dangKyUseCase">Use Case xử lý đăng ký tài khoản khách hàng</param>
    public KhachHangController(DangKyKhachHangUseCase dangKyUseCase)
    {
        _dangKyUseCase = dangKyUseCase;
    }

    /// <summary>
    /// Hiển thị giao diện biểu mẫu Đăng ký thành viên mới
    /// </summary>
    /// <returns>View đăng ký</returns>
    [HttpGet]
    public IActionResult DangKy()
    {
        return View();
    }

    /// <summary>
    /// Tiếp nhận và xử lý biểu mẫu Đăng ký tài khoản khách hàng
    /// Khớp 100% với phương thức HttpPost DangKy(request: DangKyKhachHangDTO) trong Diagram
    /// </summary>
    /// <param name="request">DTO chứa thông tin đăng ký (HoTen, SoDienThoai, TenDangNhap, MatKhau, NgaySinh)</param>
    /// <returns>Chuyển hướng đến màn hình Đăng nhập khi thành công, hoặc trả lại View kèm thông báo lỗi</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DangKy([FromForm] DangKyKhachHangDTO request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            // Thực thi ca sử dụng đăng ký khách hàng (Step 1 trong flow tạo khách hàng mới)
            var ketQua = _dangKyUseCase.Execute(request);
            if (ketQua)
            {
                TempData["ThongBaoThanhCong"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("DangNhap", "XacThuc");
            }

            ModelState.AddModelError(string.Empty, "Đăng ký không thành công. Vui lòng kiểm tra lại thông tin.");
            return View(request);
        }
        catch (InvalidOperationException ex)
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
}
