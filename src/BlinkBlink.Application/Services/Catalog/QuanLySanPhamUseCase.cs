using System;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Catalog;

/// <summary>
/// Use Case Quản lý sản phẩm trang sức (QuanLySanPhamUseCase).
/// Đảm nhận nghiệp vụ thêm mới sản phẩm, điều chỉnh thông tin và cập nhật giá bán.
/// Bản vẽ kỹ thuật: Phân hệ quản lý sản phẩm & kho.xml - QuanLySanPhamUseCase
/// </summary>
public class QuanLySanPhamUseCase
{
    private readonly ISanPhamRepository _sanPhamRepo;
    private readonly IDanhMucRepository _danhMucRepo;

    public QuanLySanPhamUseCase(ISanPhamRepository sanPhamRepo, IDanhMucRepository danhMucRepo)
    {
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
        _danhMucRepo = danhMucRepo ?? throw new ArgumentNullException(nameof(danhMucRepo));
    }

    /// <summary>
    /// Thêm sản phẩm trang sức mới vào danh mục tương ứng.
    /// Khớp hàm: + ThemSanPhamMoi(request: SanPhamDTO): bool trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin sản phẩm cần tạo</param>
    /// <returns>True nếu tạo thành công, False nếu dữ liệu không hợp lệ hoặc trùng lặp</returns>
    public bool ThemSanPhamMoi(SanPhamDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TenSanPham))
        {
            return false;
        }

        // Tạo mã sản phẩm nếu chưa có
        var maSp = string.IsNullOrWhiteSpace(request.MaSanPham)
            ? $"SP_{DateTime.UtcNow.Ticks % 1000000:D6}"
            : request.MaSanPham.Trim();

        // Kiểm tra xem sản phẩm đã tồn tại hay chưa
        var existing = _sanPhamRepo.TimTheoId(maSp);
        if (existing != null)
        {
            return false;
        }

        var entity = new SanPhamTrangSuc(
            maSanPham: maSp,
            tenSanPham: request.TenSanPham.Trim(),
            maDanhMuc: request.MaDanhMuc?.Trim() ?? string.Empty,
            chatLieu: request.ChatLieu?.Trim() ?? string.Empty,
            size: request.Size?.Trim() ?? string.Empty,
            giaNhap: request.GiaNhap,
            giaBan: request.GiaBan,
            trangThai: "HetHang",
            hinhAnh: request.HinhAnh?.Trim() ?? string.Empty,
            phanTramGiamGia: Math.Clamp(request.PhanTramGiamGia, 0, 100)
        );

        _sanPhamRepo.LuuSanPham(entity);
        return true;
    }

    /// <summary>
    /// Cập nhật giá bán mới cho sản phẩm trang sức.
    /// Khớp hàm: + CapNhatGiaBan(maSanPham: string, giaMoi: decimal): bool trong Diagram.
    /// </summary>
    /// <param name="maSanPham">Mã sản phẩm cần sửa giá</param>
    /// <param name="giaMoi">Mức giá bán mới</param>
    /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy hoặc giá không hợp lệ</returns>
    public bool CapNhatGiaBan(string maSanPham, decimal giaMoi)
    {
        if (string.IsNullOrWhiteSpace(maSanPham) || giaMoi <= 0)
        {
            return false;
        }

        var sanPham = _sanPhamRepo.TimTheoId(maSanPham.Trim());
        if (sanPham == null)
        {
            return false;
        }

        sanPham.GiaBan = giaMoi;
        _sanPhamRepo.LuuSanPham(sanPham);
        return true;
    }

    /// <summary>
    /// Cập nhật thông tin chi tiết sản phẩm trang sức (tên, danh mục, chất liệu, kích cỡ).
    /// Khớp hàm: + CapNhatThongTin(request: CapNhatSanPhamDTO): bool trong Diagram.
    /// </summary>
    /// <param name="request">Thông tin sản phẩm cần cập nhật</param>
    /// <returns>True nếu cập nhật thành công, False nếu thông tin không hợp lệ</returns>
    public bool CapNhatThongTin(CapNhatSanPhamDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaSanPham) || string.IsNullOrWhiteSpace(request.TenSanPham))
        {
            return false;
        }

        var sanPham = _sanPhamRepo.TimTheoId(request.MaSanPham.Trim());
        if (sanPham == null)
        {
            return false;
        }

        sanPham.TenSanPham = request.TenSanPham.Trim();
        sanPham.MaDanhMuc = request.MaDanhMuc?.Trim() ?? string.Empty;
        sanPham.ChatLieu = request.ChatLieu?.Trim() ?? string.Empty;
        sanPham.Size = request.Size?.Trim() ?? string.Empty;
        if (request.GiaNhap >= 0) sanPham.GiaNhap = request.GiaNhap;
        if (request.GiaBan >= 0) sanPham.GiaBan = request.GiaBan;
        if (!string.IsNullOrWhiteSpace(request.HinhAnh)) sanPham.HinhAnh = request.HinhAnh.Trim();
        sanPham.PhanTramGiamGia = Math.Clamp(request.PhanTramGiamGia, 0, 100);

        _sanPhamRepo.LuuSanPham(sanPham);
        return true;
    }

    // =========================================================================
    // MỞ RỘNG BẤT ĐỒNG BỘ CHUẨN CLEAN ARCHITECTURE .NET 8 LTS
    // =========================================================================

    public async Task<bool> ThemSanPhamMoiAsync(SanPhamDTO request, CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TenSanPham))
        {
            return false;
        }

        var maSp = string.IsNullOrWhiteSpace(request.MaSanPham)
            ? $"SP_{DateTime.UtcNow.Ticks % 1000000:D6}"
            : request.MaSanPham.Trim();

        var existing = await _sanPhamRepo.TimTheoIdAsync(maSp, cancellationToken);
        if (existing != null)
        {
            return false;
        }

        var entity = new SanPhamTrangSuc(
            maSanPham: maSp,
            tenSanPham: request.TenSanPham.Trim(),
            maDanhMuc: request.MaDanhMuc?.Trim() ?? string.Empty,
            chatLieu: request.ChatLieu?.Trim() ?? string.Empty,
            size: request.Size?.Trim() ?? string.Empty,
            giaNhap: request.GiaNhap,
            giaBan: request.GiaBan,
            trangThai: "HetHang",
            hinhAnh: request.HinhAnh?.Trim() ?? string.Empty,
            phanTramGiamGia: Math.Clamp(request.PhanTramGiamGia, 0, 100)
        );

        await _sanPhamRepo.LuuSanPhamAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> CapNhatGiaBanAsync(string maSanPham, decimal giaMoi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maSanPham) || giaMoi <= 0)
        {
            return false;
        }

        var sanPham = await _sanPhamRepo.TimTheoIdAsync(maSanPham.Trim(), cancellationToken);
        if (sanPham == null)
        {
            return false;
        }

        sanPham.GiaBan = giaMoi;
        await _sanPhamRepo.LuuSanPhamAsync(sanPham, cancellationToken);
        return true;
    }

    public async Task<bool> CapNhatThongTinAsync(CapNhatSanPhamDTO request, CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaSanPham) || string.IsNullOrWhiteSpace(request.TenSanPham))
        {
            return false;
        }

        var sanPham = await _sanPhamRepo.TimTheoIdAsync(request.MaSanPham.Trim(), cancellationToken);
        if (sanPham == null)
        {
            return false;
        }

        sanPham.TenSanPham = request.TenSanPham.Trim();
        sanPham.MaDanhMuc = request.MaDanhMuc?.Trim() ?? string.Empty;
        sanPham.ChatLieu = request.ChatLieu?.Trim() ?? string.Empty;
        sanPham.Size = request.Size?.Trim() ?? string.Empty;
        if (request.GiaNhap >= 0) sanPham.GiaNhap = request.GiaNhap;
        if (request.GiaBan >= 0) sanPham.GiaBan = request.GiaBan;
        if (!string.IsNullOrWhiteSpace(request.HinhAnh)) sanPham.HinhAnh = request.HinhAnh.Trim();
        sanPham.PhanTramGiamGia = Math.Clamp(request.PhanTramGiamGia, 0, 100);

        await _sanPhamRepo.LuuSanPhamAsync(sanPham, cancellationToken);
        return true;
    }
}
