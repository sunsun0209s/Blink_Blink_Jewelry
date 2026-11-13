using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Services.Reporting;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller cung cấp các RESTful API phục vụ Báo cáo & Thống kê doanh thu, kho hàng.
/// Cung cấp dữ liệu dạng số liệu căn gióng (tabular figures) cho màn hình Admin WinForms Dashboard.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ThongKeController : ControllerBase
{
    private readonly ThongKeDoanhThuUseCase _doanhThuUseCase;
    private readonly ThongKeKhoHangUseCase _khoHangUseCase;

    public ThongKeController(
        ThongKeDoanhThuUseCase doanhThuUseCase,
        ThongKeKhoHangUseCase khoHangUseCase)
    {
        _doanhThuUseCase = doanhThuUseCase ?? throw new ArgumentNullException(nameof(doanhThuUseCase));
        _khoHangUseCase = khoHangUseCase ?? throw new ArgumentNullException(nameof(khoHangUseCase));
    }

    /// <summary>
    /// Thống kê doanh thu và lợi nhuận ước tính theo khoảng thời gian.
    /// Khớp phương thức: + HttpGet GetDoanhThu(tuNgay: DateTime, denNgay: DateTime): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="tuNgay">Mốc ngày bắt đầu (nếu để trống, mặc định 30 ngày trước)</param>
    /// <param name="denNgay">Mốc ngày kết thúc (nếu để trống, mặc định hôm nay)</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách thống kê doanh thu theo ngày</returns>
    [HttpGet("doanh-thu")]
    public async Task<IActionResult> GetDoanhThu(
        [FromQuery] DateTime? tuNgay,
        [FromQuery] DateTime? denNgay,
        CancellationToken cancellationToken)
    {
        var start = tuNgay ?? DateTime.UtcNow.AddDays(-30);
        var end = denNgay ?? DateTime.UtcNow;

        var data = await _doanhThuUseCase.ExecuteAsync(start, end, cancellationToken);
        return Ok(new { Success = true, TuNgay = start, DenNgay = end, Data = data });
    }

    /// <summary>
    /// Báo cáo số lượng tồn kho, giá trị tồn và chênh lệch kiểm kê của từng mẫu trang sức.
    /// Khớp phương thức: + HttpGet GetBaoCaoTonKho(): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách báo cáo tồn kho</returns>
    [HttpGet("ton-kho")]
    public async Task<IActionResult> GetBaoCaoTonKho(CancellationToken cancellationToken)
    {
        var data = await _khoHangUseCase.LayBaoCaoTonKhoAsync(cancellationToken);
        return Ok(new { Success = true, Data = data });
    }

    /// <summary>
    /// Báo cáo danh sách Top các món trang sức bán chạy nhất.
    /// Khớp phương thức: + HttpGet GetTopBanChay(top: int): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="top">Số lượng sản phẩm trong bảng xếp hạng (mặc định 5)</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách top sản phẩm bán chạy</returns>
    [HttpGet("top-ban-chay")]
    public async Task<IActionResult> GetTopBanChay([FromQuery] int top = 5, CancellationToken cancellationToken = default)
    {
        var data = await _khoHangUseCase.LayTopBanChayAsync(top, cancellationToken);
        return Ok(new { Success = true, Top = top, Data = data });
    }

    /// <summary>
    /// Báo cáo danh sách sản phẩm trang sức có doanh thu thấp nhất hoặc bán chậm.
    /// Khớp phương thức: + HttpGet GetSanPhamDoanhThuThap(top: int): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="top">Số lượng sản phẩm cần lấy (mặc định 5)</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách sản phẩm doanh thu thấp</returns>
    [HttpGet("doanh-thu-thap")]
    public async Task<IActionResult> GetSanPhamDoanhThuThap([FromQuery] int top = 5, CancellationToken cancellationToken = default)
    {
        var data = await _khoHangUseCase.LaySanPhamDoanhThuThapAsync(top, cancellationToken);
        return Ok(new { Success = true, Top = top, Data = data });
    }
}
