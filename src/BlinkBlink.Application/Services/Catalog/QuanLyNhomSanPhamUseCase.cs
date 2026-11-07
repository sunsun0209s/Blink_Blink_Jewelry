using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Catalog;

/// <summary>
/// Use Case quản lý Nhóm sản phẩm (gom nhóm các trang sức cùng mẫu nhưng khác Size).
/// Xử lý toàn bộ nghiệp vụ thêm/sửa/xóa nhóm, ràng buộc trùng size, và đồng bộ tên sản phẩm trong nhóm.
/// </summary>
public class QuanLyNhomSanPhamUseCase
{
    private readonly INhomSanPhamRepository _nhomRepo;
    private readonly ISanPhamRepository _sanPhamRepo;

    public QuanLyNhomSanPhamUseCase(INhomSanPhamRepository nhomRepo, ISanPhamRepository sanPhamRepo)
    {
        _nhomRepo = nhomRepo ?? throw new ArgumentNullException(nameof(nhomRepo));
        _sanPhamRepo = sanPhamRepo ?? throw new ArgumentNullException(nameof(sanPhamRepo));
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các nhóm sản phẩm kèm số lượng sản phẩm trong từng nhóm.
    /// </summary>
    public List<NhomSanPhamDTO> LayDanhSachNhom()
    {
        var nhoms = _nhomRepo.LayTatCa();
        var allSanPhams = _sanPhamRepo.LayTatCa();

        return nhoms.Select(n => new NhomSanPhamDTO
        {
            MaNhom = n.MaNhom,
            TenNhom = n.TenNhom,
            MoTa = n.MoTa,
            SoLuongSanPham = allSanPhams.Count(s => s.MaNhom == n.MaNhom)
        }).ToList();
    }

    /// <summary>
    /// Lấy danh sách sản phẩm thuộc một nhóm cụ thể.
    /// </summary>
    public List<SanPhamTrangSuc> LayDanhSachSanPhamTrongNhom(string maNhom)
    {
        if (string.IsNullOrWhiteSpace(maNhom) || maNhom == "0") return new List<SanPhamTrangSuc>();
        return _sanPhamRepo.LayTheoNhom(maNhom);
    }

    /// <summary>
    /// Tạo nhóm sản phẩm mới.
    /// </summary>
    public NhomSanPham? TaoNhom(TaoNhomDTO req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TenNhom)) return null;

        var maNhom = $"NHOM_{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var nhom = new NhomSanPham(maNhom, req.TenNhom.Trim(), req.MoTa?.Trim() ?? string.Empty);
        _nhomRepo.LuuNhom(nhom);
        return nhom;
    }

    /// <summary>
    /// Cập nhật tên nhóm sản phẩm.
    /// Quy ước: Thay đổi tên nhóm sẽ tự động cập nhật tên của tất cả sản phẩm trong nhóm.
    /// </summary>
    public bool CapNhatTenNhom(string maNhom, string tenNhomMoi, string moTaMoi = "")
    {
        if (string.IsNullOrWhiteSpace(maNhom) || string.IsNullOrWhiteSpace(tenNhomMoi)) return false;

        var nhom = _nhomRepo.TimTheoId(maNhom);
        if (nhom == null) return false;

        nhom.TenNhom = tenNhomMoi.Trim();
        if (!string.IsNullOrEmpty(moTaMoi)) nhom.MoTa = moTaMoi.Trim();
        _nhomRepo.CapNhatNhom(nhom);

        // Đồng bộ tên của toàn bộ sản phẩm trong nhóm
        var sanPhams = _sanPhamRepo.LayTheoNhom(maNhom);
        foreach (var sp in sanPhams)
        {
            sp.TenSanPham = nhom.TenNhom;
            _sanPhamRepo.LuuSanPham(sp);
        }

        return true;
    }

    /// <summary>
    /// Xóa nhóm sản phẩm.
    /// Quy ước: Khi nhóm bị xóa, tất cả sản phẩm trong nhóm đó sẽ được đặt lại mã nhóm thành "0".
    /// </summary>
    public bool XoaNhom(string maNhom)
    {
        if (string.IsNullOrWhiteSpace(maNhom)) return false;

        // Reset mã nhóm của tất cả sản phẩm trong nhóm về "0"
        var sanPhams = _sanPhamRepo.LayTheoNhom(maNhom);
        foreach (var sp in sanPhams)
        {
            sp.MaNhom = "0";
            _sanPhamRepo.LuuSanPham(sp);
        }

        return _nhomRepo.XoaNhom(maNhom);
    }

    /// <summary>
    /// Thêm sản phẩm vào nhóm.
    /// Kiểm tra:
    /// 1. Không trùng Size với bất kỳ sản phẩm nào trong nhóm.
    /// 2. Tự động đồng bộ tên sản phẩm theo tên nhóm.
    /// </summary>
    /// <param name="maNhom">Mã nhóm cần thêm</param>
    /// <param name="maSP">Mã sản phẩm thêm vào</param>
    /// <param name="errorMessage">Thông báo lỗi chi tiết nếu thất bại</param>
    /// <returns>True nếu thêm thành công</returns>
    public bool ThemSanPhamVaoNhom(string maNhom, string maSP, out string errorMessage)
    {
        errorMessage = string.Empty;

        var nhom = _nhomRepo.TimTheoId(maNhom);
        if (nhom == null)
        {
            errorMessage = "Nhóm sản phẩm không tồn tại.";
            return false;
        }

        var sp = _sanPhamRepo.TimTheoId(maSP);
        if (sp == null)
        {
            errorMessage = "Sản phẩm không tồn tại.";
            return false;
        }

        // Kiểm tra xem sản phẩm có thuộc nhóm khác không
        if (!string.IsNullOrWhiteSpace(sp.MaNhom) && sp.MaNhom != "0" && sp.MaNhom != maNhom)
        {
            var nhomCu = _nhomRepo.TimTheoId(sp.MaNhom);
            var tenNhomCu = nhomCu?.TenNhom ?? sp.MaNhom;
            errorMessage = $"Sản phẩm này đang thuộc về nhóm '{tenNhomCu}', vui lòng hủy nhóm của sản phẩm trước khi thêm.";
            return false;
        }

        // Kiểm tra trùng Size với các sản phẩm đã có trong nhóm
        var sanPhamsTrongNhom = _sanPhamRepo.LayTheoNhom(maNhom);
        var targetSize = sp.Size?.Trim().ToLowerInvariant() ?? string.Empty;

        var isDuplicateSize = sanPhamsTrongNhom.Any(s =>
            s.MaSanPham != maSP &&
            string.Equals(s.Size?.Trim().ToLowerInvariant(), targetSize, StringComparison.OrdinalIgnoreCase));

        if (isDuplicateSize)
        {
            errorMessage = $"Size '{sp.Size}' của sản phẩm đã bị trùng với một sản phẩm khác trong nhóm. Trong một nhóm chỉ cho phép các size khác nhau.";
            return false;
        }

        // Đồng bộ tên sản phẩm theo tên nhóm và gán mã nhóm
        sp.TenSanPham = nhom.TenNhom;
        sp.MaNhom = maNhom;
        _sanPhamRepo.LuuSanPham(sp);

        return true;
    }

    /// <summary>
    /// Loại sản phẩm khỏi nhóm. Đặt mã nhóm về "0" (mặc định chưa gom nhóm).
    /// </summary>
    public bool LoaiSanPhamKhoiNhom(string maSP)
    {
        var sp = _sanPhamRepo.TimTheoId(maSP);
        if (sp == null) return false;

        sp.MaNhom = "0";
        _sanPhamRepo.LuuSanPham(sp);
        return true;
    }

    /// <summary>
    /// Kiểm tra khi người dùng sửa Size của một sản phẩm có bị trùng với sản phẩm nào khác trong cùng nhóm hay không.
    /// </summary>
    public bool KiemTraTrungSizeTrongNhom(string maSP, string sizeMoi, out string errorMessage)
    {
        errorMessage = string.Empty;
        var sp = _sanPhamRepo.TimTheoId(maSP);
        if (sp == null || string.IsNullOrWhiteSpace(sp.MaNhom) || sp.MaNhom == "0")
        {
            return false; // Không thuộc nhóm nào thì không kiểm tra
        }

        var normalizedSize = sizeMoi?.Trim().ToLowerInvariant() ?? string.Empty;
        var sanPhamsCungNhom = _sanPhamRepo.LayTheoNhom(sp.MaNhom);

        var isDuplicate = sanPhamsCungNhom.Any(s =>
            s.MaSanPham != maSP &&
            string.Equals(s.Size?.Trim().ToLowerInvariant(), normalizedSize, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            errorMessage = $"Size '{sizeMoi}' của sản phẩm đã bị trùng với sản phẩm khác trong nhóm. Trong một nhóm chỉ cho phép các size khác nhau.";
            return true;
        }

        return false;
    }

    /// <summary>
    /// Khi đổi tên một sản phẩm đang thuộc nhóm, cập nhật tên toàn bộ các sản phẩm khác cùng nhóm và cập nhật tên nhóm.
    /// </summary>
    public bool DoiTenToanBoNhomTheoSanPham(string maSP, string tenSanPhamMoi)
    {
        if (string.IsNullOrWhiteSpace(tenSanPhamMoi)) return false;
        var sp = _sanPhamRepo.TimTheoId(maSP);
        if (sp == null || string.IsNullOrWhiteSpace(sp.MaNhom) || sp.MaNhom == "0")
        {
            if (sp != null)
            {
                sp.TenSanPham = tenSanPhamMoi.Trim();
                _sanPhamRepo.LuuSanPham(sp);
                return true;
            }
            return false;
        }

        return CapNhatTenNhom(sp.MaNhom, tenSanPhamMoi.Trim());
    }
}

