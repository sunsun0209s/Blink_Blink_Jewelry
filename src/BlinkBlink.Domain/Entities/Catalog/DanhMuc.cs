using System;
using System.Collections.Generic;
using System.Text;

namespace BlinkBlink.Domain.Entities.Catalog;
/// Thực thể Danh mục quản lý phân loại sản phẩm trang sức (Nhẫn, Dây chuyền, Bông tai...)
public class DanhMuc
{
    public string MaDanhMuc { get; set; } = string.Empty;
    public string TenDanhMuc { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public virtual ICollection<SanPhamTrangSuc> SanPhamTrangSucs { get; set; } = new List<SanPhamTrangSuc>();
    public DanhMuc() { }
    public DanhMuc(string maDanhMuc, string tenDanhMuc, string moTa = "")
    {
        MaDanhMuc = maDanhMuc;
        TenDanhMuc = tenDanhMuc;
        MoTa = moTa;
    }
    public List<SanPhamTrangSuc> LayDanhSachSanPham()
    {
        return SanPhamTrangSucs.ToList();
    }
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
