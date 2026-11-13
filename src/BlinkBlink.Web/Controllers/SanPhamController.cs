using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Application.Services.Catalog;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các tác vụ quản lý sản phẩm trang sức.
/// Khớp 100% với Class Diagram (Interface Adapters / Presentation Layer).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SanPhamController : ControllerBase
{
    private readonly QuanLySanPhamUseCase _quanLySanPhamUseCase;
    private readonly ISanPhamRepository _sanPhamRepository;
    private readonly IAppLogger _logger;

    public SanPhamController(
        QuanLySanPhamUseCase quanLySanPhamUseCase,
        ISanPhamRepository sanPhamRepository,
        IAppLogger? logger = null)
    {
        _quanLySanPhamUseCase = quanLySanPhamUseCase ?? throw new ArgumentNullException(nameof(quanLySanPhamUseCase));
        _sanPhamRepository = sanPhamRepository ?? throw new ArgumentNullException(nameof(sanPhamRepository));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Thêm sản phẩm trang sức mới.
    /// Khớp hàm: + HttpPost TaoSanPham(request: SanPhamDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin sản phẩm cần tạo</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>HTTP Status 200/201 nếu thành công, 400 nếu dữ liệu lỗi</returns>
    [HttpPost]
    public async Task<IActionResult> TaoSanPham([FromBody] SanPhamDTO request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu sản phẩm không hợp lệ.", _logger);
        }

        var result = await _quanLySanPhamUseCase.ThemSanPhamMoiAsync(request, cancellationToken);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.DuplicateKey, "Không thể tạo sản phẩm. Kiểm tra lại thông tin hoặc mã sản phẩm đã tồn tại.", _logger);
        }

        return Ok(new { Success = true, Message = "Tạo mới sản phẩm trang sức thành công." });
    }

    /// <summary>
    /// Cập nhật giá bán sản phẩm trang sức.
    /// Khớp hàm: + HttpPut DoiGiaBan(maSanPham: string, request: DoiGiaDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm</param>
    /// <param name="request">DTO chứa giá bán mới</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 hoặc 404 nếu lỗi</returns>
    [HttpPut("{maSanPham}/doi-gia")]
    public async Task<IActionResult> DoiGiaBan(string maSanPham, [FromBody] DoiGiaDTO request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maSanPham) || request == null || request.GiaMoi <= 0)
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidPriceOrQuantity, "Mã sản phẩm hoặc giá bán mới không hợp lệ.", _logger);
        }

        var result = await _quanLySanPhamUseCase.CapNhatGiaBanAsync(maSanPham, request.GiaMoi, cancellationToken);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 404, ErrorCodes.ProductNotFound, $"Không tìm thấy sản phẩm mã '{maSanPham}' để điều chỉnh giá.", _logger);
        }

        _logger.LogInfo(nameof(SanPhamController), $"Đã điều chỉnh giá bán sản phẩm '{maSanPham}' thành {request.GiaMoi:N0} VNĐ.");
        return Ok(new { Success = true, Message = $"Đã cập nhật giá bán mới thành {request.GiaMoi:N0} VNĐ cho sản phẩm '{maSanPham}'." });
    }

    /// <summary>
    /// Cập nhật thông tin chi tiết sản phẩm trang sức (tên, danh mục, chất liệu, kích cỡ).
    /// Khớp hàm: + HttpPut CapNhatThongTin(request: CapNhatSanPhamDTO): HttpResponse trong Diagram.
    /// </summary>
    /// <param name="request">DTO chứa thông tin sản phẩm cập nhật</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>HTTP Status 200 nếu thành công, 400 hoặc 404 nếu lỗi</returns>
    [HttpPut("cap-nhat-thong-tin")]
    public async Task<IActionResult> CapNhatThongTin([FromBody] CapNhatSanPhamDTO request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaSanPham))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Thông tin cập nhật sản phẩm không hợp lệ.", _logger);
        }

        var result = await _quanLySanPhamUseCase.CapNhatThongTinAsync(request, cancellationToken);
        if (!result)
        {
            return ApiResponseHelper.Fail(this, 404, ErrorCodes.ProductNotFound, $"Không tìm thấy sản phẩm mã '{request.MaSanPham}' để cập nhật thông tin.", _logger);
        }

        return Ok(new { Success = true, Message = $"Cập nhật thông tin sản phẩm '{request.MaSanPham}' thành công." });
    }

    /// <summary>
    /// Lấy toàn bộ danh sách sản phẩm trang sức từ CSDL kèm danh mục, % giảm giá, giá sau giảm và ảnh thực tế.
    /// Phục vụ hiển thị động cho trang Web bán hàng của khách hàng.
    /// </summary>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>Danh sách sản phẩm trang sức</returns>
    [HttpGet]
    public async Task<IActionResult> LayDanhSach(CancellationToken cancellationToken)
    {
        var items = await _sanPhamRepository.LayTatCaAsync(cancellationToken);
        var response = items.Select(sp =>
        {
            decimal giaSauGiam = sp.PhanTramGiamGia > 0
                ? Math.Round(sp.GiaBan * (100 - sp.PhanTramGiamGia) / 100m, 0)
                : sp.GiaBan;

            return new
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                MaDanhMuc = sp.MaDanhMuc,
                TenDanhMuc = sp.DanhMuc?.TenDanhMuc ?? sp.MaDanhMuc,
                ChatLieu = sp.ChatLieu,
                Size = sp.Size,
                GiaBan = sp.GiaBan,
                PhanTramGiamGia = sp.PhanTramGiamGia,
                GiaSauGiam = giaSauGiam,
                TrangThai = sp.TrangThai,
                HinhAnh = sp.HinhAnh,
                SoLuongTon = sp.SoLuongTon,
                MaNhom = string.IsNullOrWhiteSpace(sp.MaNhom) ? "0" : sp.MaNhom,
                TenNhom = sp.NhomSanPham?.TenNhom ?? string.Empty,
                ConHang = sp.SoLuongTon > 0 && sp.TrangThai == "SanSang"
            };
        });

        return Ok(new { Success = true, Data = response });
    }

    /// <summary>
    /// Lấy chi tiết một sản phẩm trang sức theo mã sản phẩm.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm trang sức</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>Dữ liệu chi tiết sản phẩm</returns>
    /// <summary>
    /// Lấy danh sách sản phẩm trang sức cùng nhóm mẫu nhưng khác size.
    /// </summary>
    /// <param name="maNhom">Mã nhóm sản phẩm</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>Danh sách sản phẩm trong nhóm</returns>
    [HttpGet("nhom/{maNhom}")]
    public async Task<IActionResult> LayTheoNhom(string maNhom, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maNhom) || maNhom == "0")
        {
            return Ok(new { Success = true, Data = Array.Empty<object>() });
        }

        var items = await _sanPhamRepository.LayTheoNhomAsync(maNhom, cancellationToken);
        var response = items.Select(sp =>
        {
            decimal giaSauGiam = sp.PhanTramGiamGia > 0
                ? Math.Round(sp.GiaBan * (100 - sp.PhanTramGiamGia) / 100m, 0)
                : sp.GiaBan;

            return new
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                MaDanhMuc = sp.MaDanhMuc,
                TenDanhMuc = sp.DanhMuc?.TenDanhMuc ?? sp.MaDanhMuc,
                MaNhom = string.IsNullOrWhiteSpace(sp.MaNhom) ? "0" : sp.MaNhom,
                TenNhom = sp.NhomSanPham?.TenNhom ?? string.Empty,
                ChatLieu = sp.ChatLieu,
                Size = sp.Size,
                GiaBan = sp.GiaBan,
                PhanTramGiamGia = sp.PhanTramGiamGia,
                GiaSauGiam = giaSauGiam,
                TrangThai = sp.TrangThai,
                SoLuongTon = sp.SoLuongTon,
                ConHang = sp.SoLuongTon > 0 && sp.TrangThai == "SanSang",
                HinhAnh = sp.HinhAnh
            };
        });

        return Ok(new { Success = true, Data = response });
    }

    [HttpGet("{maSanPham}")]
    public async Task<IActionResult> LayChiTiet(string maSanPham, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maSanPham))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã sản phẩm không hợp lệ.", _logger);
        }

        var sp = await _sanPhamRepository.TimTheoIdAsync(maSanPham, cancellationToken);
        if (sp == null)
        {
            return ApiResponseHelper.Fail(this, 404, ErrorCodes.ProductNotFound, $"Không tìm thấy sản phẩm mã '{maSanPham}'.", _logger);
        }

        decimal giaSauGiam = sp.PhanTramGiamGia > 0
            ? Math.Round(sp.GiaBan * (100 - sp.PhanTramGiamGia) / 100m, 0)
            : sp.GiaBan;

        return Ok(new
        {
            Success = true,
            Data = new
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                MaDanhMuc = sp.MaDanhMuc,
                TenDanhMuc = sp.DanhMuc?.TenDanhMuc ?? sp.MaDanhMuc,
                ChatLieu = sp.ChatLieu,
                Size = sp.Size,
                GiaBan = sp.GiaBan,
                PhanTramGiamGia = sp.PhanTramGiamGia,
                GiaSauGiam = giaSauGiam,
                TrangThai = sp.TrangThai,
                HinhAnh = sp.HinhAnh,
                SoLuongTon = sp.SoLuongTon,
                MaNhom = string.IsNullOrWhiteSpace(sp.MaNhom) ? "0" : sp.MaNhom,
                TenNhom = sp.NhomSanPham?.TenNhom ?? string.Empty,
                ConHang = sp.SoLuongTon > 0 && sp.TrangThai == "SanSang"
            }
        });
    }
}
