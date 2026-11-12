using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Entities.Cart;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Web.Controllers;

/// <summary>
/// Controller xử lý các tác vụ giỏ hàng trực tuyến cho khách hàng đã xác thực.
/// Bản vẽ kỹ thuật: Phân hệ quản lý bán hàng.xml - IGioHangRepository & GioHang
/// Khớp 100% với Class Diagram kỹ thuật và triết lý Clean Architecture.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GioHangController : ControllerBase
{
    private readonly IGioHangRepository _gioHangRepo;
    private readonly ISanPhamRepository _sanPhamRepo;
    private readonly IKhachHangRepository _khachHangRepo;
    private readonly IAppLogger _logger;

    public GioHangController(
        IGioHangRepository gioHangRepo,
        ISanPhamRepository sanPhamRepo,
        IKhachHangRepository khachHangRepo,
        IAppLogger? logger = null)
    {
        _gioHangRepo = gioHangRepo ?? throw new ArgumentNullException(nameof(gioHangRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
        _khachHangRepo = khachHangRepo ?? throw new ArgumentNullException(nameof(khachHangRepo));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Lấy thông tin giỏ hàng của khách hàng từ cơ sở dữ liệu.
    /// Khớp hàm: + LayGioHang(maKH: string): GioHang trong Diagram.
    /// </summary>
    /// <param name="maKhachHang">Mã khách hàng hoặc mã người dùng</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>Dữ liệu giỏ hàng và danh sách chi tiết các món trang sức đã lưu</returns>
    [HttpGet("{maKhachHang}")]
    public async Task<IActionResult> LayGioHang(string maKhachHang, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maKhachHang))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã khách hàng không hợp lệ.", _logger);
        }

        var maUser = ResolveMaNguoiDung(maKhachHang);
        var gh = await _gioHangRepo.LayGioHangAsync(maUser, cancellationToken);
        if (gh == null || gh.DanhSachSanPham == null || gh.DanhSachSanPham.Count == 0)
        {
            return Ok(new
            {
                Success = true,
                Data = new
                {
                    MaGioHang = "GH_" + maUser,
                    MaKhachHang = maKhachHang.Trim(),
                    SoLuong = 0,
                    DanhSachSanPham = new List<object>()
                }
            });
        }

        var items = gh.DanhSachSanPham.Select(item => new
        {
            MaSanPham = item.MaSanPham,
            TenSanPham = item.SanPham?.TenSanPham ?? item.MaSanPham,
            ChatLieu = item.SanPham?.ChatLieu ?? string.Empty,
            Size = item.SanPham?.Size ?? string.Empty,
            GiaBan = item.SanPham?.GiaBan ?? 0,
            SoLuong = item.SoLuongSanPham,
            HinhAnh = GetProductImage(item.MaSanPham)
        }).ToList();

        return Ok(new
        {
            Success = true,
            Data = new
            {
                MaGioHang = gh.MaGioHang,
                MaKhachHang = gh.MaKhachHang,
                SoLuong = gh.SoLuong,
                DanhSachSanPham = items
            }
        });
    }

    /// <summary>
    /// Cập nhật hoặc lưu trữ giỏ hàng của khách hàng vào cơ sở dữ liệu.
    /// Khớp hàm: + CapNhatGioHang(gh: GioHang): void trong Diagram.
    /// </summary>
    /// <param name="request">Dữ liệu giỏ hàng gửi lên từ trình duyệt</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>Trạng thái cập nhật thành công</returns>
    [HttpPost("cap-nhat")]
    public async Task<IActionResult> CapNhatGioHang([FromBody] CapNhatGioHangRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaKhachHang))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Dữ liệu giỏ hàng hoặc mã khách hàng không hợp lệ.", _logger);
        }

        // Kiểm tra quyền truy cập: Chặn khách hàng bị quản trị viên khóa tài khoản
        var khach = _khachHangRepo.TimTheoMa(request.MaKhachHang);
        if (khach != null && !khach.TrangThai)
        {
            return ApiResponseHelper.Fail(this, 403, ErrorCodes.AccountLocked, "Tài khoản của quý khách hiện đang bị tạm khóa. Vui lòng liên hệ bộ phận hỗ trợ khách hàng Blink Blink.", _logger);
        }

        var maUser = ResolveMaNguoiDung(request.MaKhachHang);
        var existingCart = await _gioHangRepo.LayGioHangAsync(maUser, cancellationToken);
        var maGh = existingCart?.MaGioHang ?? (!string.IsNullOrWhiteSpace(request.MaGioHang) ? request.MaGioHang.Trim() : "GH_" + maUser);

        var gh = new GioHang(maGh, maUser);

        if (request.DanhSachSanPham != null && request.DanhSachSanPham.Count > 0)
        {
            foreach (var item in request.DanhSachSanPham)
            {
                if (!string.IsNullOrWhiteSpace(item.MaSanPham) && item.SoLuong > 0)
                {
                    gh.DanhSachSanPham.Add(new ChiTietGioHang(maGh, item.MaSanPham.Trim(), item.SoLuong));
                }
            }
        }

        gh.DatHang(); // Đồng bộ tổng số lượng

        await _gioHangRepo.CapNhatGioHangAsync(gh, cancellationToken);

        return Ok(new
        {
            Success = true,
            Message = "Đồng bộ giỏ hàng với máy chủ thành công.",
            TongSoLuong = gh.SoLuong
        });
    }

    /// <summary>
    /// Dọn sạch giỏ hàng của khách hàng sau khi thanh toán hoặc khi chọn xóa hết.
    /// </summary>
    /// <param name="maKhachHang">Mã khách hàng cần dọn giỏ</param>
    /// <param name="cancellationToken">Token hủy tác vụ</param>
    /// <returns>Trạng thái dọn dẹp thành công</returns>
    [HttpPost("xoa-sach/{maKhachHang}")]
    public async Task<IActionResult> XoaSachGioHang(string maKhachHang, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maKhachHang))
        {
            return ApiResponseHelper.Fail(this, 400, ErrorCodes.InvalidInput, "Mã khách hàng không hợp lệ.", _logger);
        }

        var maUser = ResolveMaNguoiDung(maKhachHang);
        var gh = new GioHang("GH_" + maUser, maUser);
        gh.DonDepGioHang();
        await _gioHangRepo.CapNhatGioHangAsync(gh, cancellationToken);

        return Ok(new { Success = true, Message = "Đã dọn sạch giỏ hàng trên máy chủ." });
    }

    /// <summary>
    /// Tìm mã người dùng cha trong bảng NguoiDung từ mã khách hàng hoặc mã người dùng đầu vào
    /// nhằm thỏa mãn chính xác ràng buộc khóa ngoại FK_GioHang_KhachHang
    /// </summary>
    private string ResolveMaNguoiDung(string maKh)
    {
        if (string.IsNullOrWhiteSpace(maKh)) return maKh;
        var kh = _khachHangRepo.TimTheoMa(maKh.Trim());
        return kh?.MaNguoiDung ?? maKh.Trim();
    }

    /// <summary>
    /// Ánh xạ ảnh sản phẩm minh họa theo mã trang sức để hiển thị trên giao diện giỏ hàng
    /// </summary>
    private static string GetProductImage(string maSanPham)
    {
        return maSanPham switch
        {
            "SP001" => "https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=800&q=80",
            "SP002" => "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?auto=format&fit=crop&w=800&q=80",
            "SP003" => "https://images.unsplash.com/photo-1630019852942-f89202989a59?auto=format&fit=crop&w=800&q=80",
            "SP004" => "https://images.unsplash.com/photo-1611591475152-47354900a359?auto=format&fit=crop&w=800&q=80",
            "SP005" => "https://images.unsplash.com/photo-1603561591411-07134e71a2a9?auto=format&fit=crop&w=800&q=80",
            "SP006" => "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=800&q=80",
            _ => "https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=800&q=80"
        };
    }
}

/// <summary>
/// Yêu cầu cập nhật giỏ hàng từ Web Client gửi lên máy chủ
/// </summary>
public class CapNhatGioHangRequest
{
    public string MaGioHang { get; set; } = string.Empty;
    public string MaKhachHang { get; set; } = string.Empty;
    public List<GioHangItemDTO> DanhSachSanPham { get; set; } = new();
}

/// <summary>
/// Chi tiết món hàng trong giỏ hàng
/// </summary>
public class GioHangItemDTO
{
    public string MaSanPham { get; set; } = string.Empty;
    public int SoLuong { get; set; } = 1;
}
