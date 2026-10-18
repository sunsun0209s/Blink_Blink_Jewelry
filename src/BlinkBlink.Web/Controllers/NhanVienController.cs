using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý quản lý nhân sự cửa hàng và phân quyền tài khoản
/// Tương ứng với NhanVienController trong swimlane Interface Adapters (Presentation) của Diagram
/// </summary>
public class NhanVienController : Controller
{
    // Thuộc tính phụ thuộc theo đúng Diagram
    private readonly QuanLyNhanVienUseCase _quanLyNhanVienUseCase;

    /// <summary>
    /// Khởi tạo NhanVienController với Use Case quản lý nhân sự được tiêm qua DI
    /// </summary>
    /// <param name="quanLyNhanVienUseCase">Use Case xử lý tạo nhân viên và khóa tài khoản</param>
    public NhanVienController(QuanLyNhanVienUseCase quanLyNhanVienUseCase)
    {
        _quanLyNhanVienUseCase = quanLyNhanVienUseCase;
    }

    /// <summary>
    /// Tiếp nhận yêu cầu thêm mới nhân viên và phân quyền vai trò
    /// Khớp 100% với phương thức HttpPost TaoNhanVien(request: TaoNhanVienDTO) trong Diagram
    /// </summary>
    /// <param name="request">DTO thông tin nhân viên mới</param>
    /// <returns>HttpResponse (JSON hoặc Redirect theo trạng thái kết quả)</returns>
    [HttpPost]
    public IActionResult TaoNhanVien([FromBody] TaoNhanVienDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { ThanhCong = false, ThongDiep = "Dữ liệu đầu vào không hợp lệ", Loi = ModelState });
        }

        try
        {
            var ketQua = _quanLyNhanVienUseCase.TaoNhanVien(request);
            if (ketQua)
            {
                return Ok(new { ThanhCong = true, ThongDiep = "Tạo mới tài khoản nhân viên thành công!" });
            }

            return BadRequest(new { ThanhCong = false, ThongDiep = "Không thể tạo tài khoản nhân viên." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { ThanhCong = false, ThongDiep = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ThanhCong = false, ThongDiep = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    /// <summary>
    /// Tiếp nhận yêu cầu khóa tài khoản của một nhân viên
    /// Khớp 100% với phương thức HttpPut KhoaTaiKhoan(maNv: string) trong Diagram
    /// </summary>
    /// <param name="maNv">Mã nhân viên cần khóa</param>
    /// <returns>HttpResponse (JSON phản hồi kết quả)</returns>
    [HttpPut]
    public IActionResult KhoaTaiKhoan(string maNv)
    {
        if (string.IsNullOrWhiteSpace(maNv))
        {
            return BadRequest(new { ThanhCong = false, ThongDiep = "Mã nhân viên không được để trống." });
        }

        try
        {
            var ketQua = _quanLyNhanVienUseCase.KhoaTaiKhoanNhanVien(maNv);
            if (ketQua)
            {
                return Ok(new { ThanhCong = true, ThongDiep = $"Đã khóa tài khoản nhân viên {maNv} thành công." });
            }

            return NotFound(new { ThanhCong = false, ThongDiep = $"Không tìm thấy nhân viên với mã: {maNv}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ThanhCong = false, ThongDiep = $"Lỗi hệ thống: {ex.Message}" });
        }
    }
}
