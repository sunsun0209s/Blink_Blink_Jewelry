using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.DTOs.Warranty;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các tác vụ quản lý dịch vụ bảo hành và hậu mãi cho trang sức.
/// Cung cấp RESTful API cho cả Web khách hàng tra cứu và WinForms quản trị.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BaoHanhController : ControllerBase
{
    private readonly TiepNhanBaoHanhUseCase _tiepNhanUseCase;
    private readonly XuLyBaoHanhUseCase _xuLyUseCase;
    private readonly IBaoHanhRepository _baoHanhRepo;
    private readonly IAppLogger _logger;

    public BaoHanhController(
        TiepNhanBaoHanhUseCase tiepNhanUseCase,
        XuLyBaoHanhUseCase xuLyUseCase,
        IBaoHanhRepository baoHanhRepo,
        IAppLogger? logger = null)
    {
        _tiepNhanUseCase = tiepNhanUseCase ?? throw new ArgumentNullException(nameof(tiepNhanUseCase));
        _xuLyUseCase = xuLyUseCase ?? throw new ArgumentNullException(nameof(xuLyUseCase));
        _baoHanhRepo = baoHanhRepo ?? throw new ArgumentNullException(nameof(baoHanhRepo));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Tiếp nhận món trang sức và tạo phiếu bảo hành mới.
    /// Khớp phương thức: + HttpPost TiepNhan(request: TaoBaoHanhDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu tiếp nhận bảo hành</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu dữ liệu lỗi</returns>
    [HttpPost("tiep-nhan")]
    public async Task<IActionResult> TiepNhan([FromBody] TaoBaoHanhDTO request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu yêu cầu bảo hành không hợp lệ.", _logger);
        }

        var result = await _tiepNhanUseCase.ExecuteAsync(request, cancellationToken);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Không thể tiếp nhận bảo hành. Kiểm tra mã sản phẩm hoặc thông tin khách hàng.", _logger);
        }

        return Ok(new { Success = true, Message = "Tiếp nhận bảo hành thành công." });
    }

    /// <summary>
    /// Cập nhật tiến độ xử lý hoặc hoàn tất quyết toán chi phí bảo hành.
    /// Khớp phương thức: + HttpPut CapNhatTrangThai(soPhieu: string, request: CapNhatTrangThaiDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành cần cập nhật</param>
    /// <param name="request">Thông tin trạng thái mới và chi phí</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 nếu lỗi</returns>
    [HttpPut("{soPhieu}/trang-thai")]
    public async Task<IActionResult> CapNhatTrangThai(string soPhieu, [FromBody] CapNhatTrangThaiDTO request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(soPhieu) || request == null)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu cập nhật không hợp lệ.", _logger);
        }

        bool result;
        if (string.Equals(request.TrangThaiMoi, "DaHoanTat", StringComparison.OrdinalIgnoreCase))
        {
            result = await _xuLyUseCase.HoanTatBaoHanhAsync(soPhieu, request.ChiPhiBaoHanh, cancellationToken);
        }
        else
        {
            result = await _xuLyUseCase.CapNhatTienDoAsync(soPhieu, request.TrangThaiMoi, cancellationToken);
        }

        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidStateTransition, "Không thể cập nhật phiếu bảo hành. Phiếu không tồn tại hoặc lỗi trạng thái.", _logger);
        }

        return Ok(new { Success = true, Message = "Cập nhật tiến độ bảo hành thành công." });
    }

    /// <summary>
    /// Tra cứu danh sách phiếu bảo hành của một khách hàng cụ thể.
    /// Khớp phương thức: + HttpGet TraCuuTheoKhachHang(maKH: string): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="maKH">Mã khách hàng</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách phiếu bảo hành của khách hàng</returns>
    [HttpGet("khach-hang/{maKH}")]
    public async Task<IActionResult> TraCuuTheoKhachHang(string maKH, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã khách hàng không hợp lệ.", _logger);
        }

        var danhSach = await _baoHanhRepo.LayTheoKhachHangAsync(maKH, cancellationToken);
        return Ok(new { Success = true, Data = danhSach });
    }

    /// <summary>
    /// Tra cứu chi tiết một phiếu bảo hành điện tử theo số phiếu.
    /// </summary>
    /// <param name="soPhieu">Số phiếu bảo hành</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Chi tiết phiếu bảo hành</returns>
    [HttpGet("{soPhieu}")]
    public async Task<IActionResult> TraCuuTheoSoPhieu(string soPhieu, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(soPhieu))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Số phiếu bảo hành không hợp lệ.", _logger);
        }

        var phieu = await _baoHanhRepo.TimTheoSoPhieuAsync(soPhieu, cancellationToken);
        if (phieu == null)
        {
            return ApiResponseHelper.Fail(this, 404, ErrorCodes.WarrantyNotFound, "Không tìm thấy thông tin phiếu bảo hành.", _logger);
        }

        return Ok(new
        {
            Success = true,
            Data = new
            {
                SoPhieu = phieu.SoPhieuBH,
                SoPhieuBH = phieu.SoPhieuBH,
                MaKhachHang = phieu.MaKhachHang,
                TenKhachHang = phieu.KhachHang?.HoTen ?? (!string.IsNullOrWhiteSpace(phieu.SoDienThoai) ? $"Khách hàng ({phieu.SoDienThoai})" : "Khách vãng lai"),
                SoDienThoai = phieu.SoDienThoai,
                MaSanPham = phieu.MaSanPham,
                TenSanPham = phieu.SanPham?.TenSanPham ?? phieu.MaSanPham,
                LyDoBaoHanh = phieu.LyDoBaoHanh,
                LyDo = phieu.LyDoBaoHanh,
                TinhTrangTiepNhan = phieu.TinhTrangTiepNhan,
                GhiChu = phieu.GhiChu,
                ChiPhiUocTinh = phieu.ChiPhiUocTinh,
                ChiPhiThucTe = phieu.ChiPhiThucTe,
                ChiPhi = phieu.ChiPhiBaoHanh,
                ChiPhiBaoHanh = phieu.ChiPhiBaoHanh,
                TrangThaiXuLy = phieu.TrangThaiXuLy,
                TrangThai = phieu.TrangThaiXuLy,
                NgayTiepNhan = phieu.NgayTiepNhan.ToString("dd/MM/yyyy"),
                NgayHenTra = phieu.NgayHenTra.HasValue ? phieu.NgayHenTra.Value.ToString("dd/MM/yyyy") : "Đang cập nhật"
            }
        });
    }

    /// <summary>
    /// Lấy toàn bộ danh sách phiếu bảo hành phục vụ quản trị và điều phối.
    /// </summary>
    /// <param name="cancellationToken">Mã token hủy tác vụ</param>
    /// <returns>Danh sách toàn bộ phiếu bảo hành</returns>
    [HttpGet("danh-sach")]
    public async Task<IActionResult> LayDanhSach(CancellationToken cancellationToken)
    {
        var danhSach = await _baoHanhRepo.LayDanhSachPhieuAsync(cancellationToken);
        return Ok(new { Success = true, Data = danhSach });
    }
}
