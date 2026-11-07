using System;
using System.Collections.Generic;

namespace BlinkBlink.Domain.Entities.Catalog;

/// <summary>
/// Thực thể Nhóm sản phẩm (NhomSanPham) trong phân hệ Quản lý sản phẩm.
/// Dùng để gom nhóm các sản phẩm trang sức cùng mẫu nhưng khác kích cỡ (Size).
/// Quy ước: Các sản phẩm cùng một nhóm phải có cùng tên trang sức và khác size.
/// </summary>
public class NhomSanPham
{
    /// <summary>
    /// Mã nhóm sản phẩm (Ví dụ: NHOM01, NHOM02...).
    /// Mã nhóm mặc định "0" quy ước cho các sản phẩm độc lập chưa gom nhóm.
    /// </summary>
    public string MaNhom { get; set; } = string.Empty;

    /// <summary>
    /// Tên đại diện của nhóm sản phẩm (Đồng bộ với tên của toàn bộ sản phẩm trong nhóm).
    /// </summary>
    public string TenNhom { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả nhóm sản phẩm.
    /// </summary>
    public string MoTa { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách các sản phẩm trang sức thuộc nhóm (Navigation property).
    /// </summary>
    public virtual ICollection<SanPhamTrangSuc> SanPhams { get; set; } = new List<SanPhamTrangSuc>();

    public NhomSanPham() { }

    public NhomSanPham(string maNhom, string tenNhom, string moTa = "")
    {
        MaNhom = maNhom;
        TenNhom = tenNhom;
        MoTa = moTa;
    }
}

