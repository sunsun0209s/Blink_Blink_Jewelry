using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlinkBlink.Domain.Entities.Catalog;

/// <summary>
/// Thực thể Danh mục quản lý phân loại sản phẩm trang sức (Nhẫn, Dây chuyền, Bông tai...).
/// Khớp 100% với Class Diagram.
/// </summary>
public class DanhMuc
{
    public string MaDanhMuc { get; set; } = string.Empty; // Khóa chính
    public string TenDanhMuc { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public bool IsHidden { get; set; } = false;

    // Quan hệ 1-N: 1 Danh mục có nhiều sản phẩm trang sức
    public virtual ICollection<SanPhamTrangSuc> SanPhamTrangSucs { get; set; } = new List<SanPhamTrangSuc>();

    public DanhMuc() { }

    public DanhMuc(string maDanhMuc, string tenDanhMuc, string moTa = "", bool isHidden = false)
    {
        MaDanhMuc = maDanhMuc;
        TenDanhMuc = tenDanhMuc;
        MoTa = moTa;
        IsHidden = isHidden;
    }

    /// <summary>
    /// Cập nhật thông tin chi tiết danh mục với dữ liệu mới.
    /// </summary>
    /// <param name="tenMoi">Tên mới của danh mục</param>
    /// <param name="moTaMoi">Mô tả mới</param>
    /// <returns>True nếu cập nhật thành công, False nếu tên mới rỗng</returns>
    public bool CapNhatDanhMuc(string tenMoi, string moTaMoi)
    {
        if (string.IsNullOrWhiteSpace(tenMoi))
        {
            return false;
        }

        TenDanhMuc = tenMoi.Trim();
        MoTa = moTaMoi?.Trim() ?? string.Empty;
        return true;
    }
}
