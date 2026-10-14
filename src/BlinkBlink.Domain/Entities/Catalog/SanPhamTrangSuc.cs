using System;
using System.Collections.Generic;
using System.Text;

namespace BlinkBlink.Domain.Entities.Catalog
{
    public class SanPhamTrangSuc
    {
        public string MaSanPham { get; set; } = string.Empty; //khoá chính
        public string TenSanPham { get; set; } = string.Empty;
        //Khoá ngoại đến danh mục
        public string MaDanhMuc { get; set; } = string.Empty;
        public virtual DanhMuc? DanhMuc { get; set; }
        //
        public string ChatLieu { get; set; } = string.Empty;
        public float Size { get; set; }
        public string TrangThai { get; set; } = "SanSang"; // SanSang, DaBan, DangBaoHanh
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
    }
}
