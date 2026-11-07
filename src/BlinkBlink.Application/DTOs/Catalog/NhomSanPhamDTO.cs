using System;
using System.Collections.Generic;

namespace BlinkBlink.Application.DTOs.Catalog;

/// <summary>
/// DTO chứa thông tin chi tiết của Nhóm sản phẩm.
/// </summary>
public class NhomSanPhamDTO
{
    public string MaNhom { get; set; } = string.Empty;
    public string TenNhom { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public int SoLuongSanPham { get; set; }
}

/// <summary>
/// DTO yêu cầu tạo nhóm sản phẩm mới.
/// </summary>
public class TaoNhomDTO
{
    public string TenNhom { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
}

/// <summary>
/// DTO yêu cầu cập nhật tên/mô tả nhóm sản phẩm.
/// </summary>
public class CapNhatNhomDTO
{
    public string MaNhom { get; set; } = string.Empty;
    public string TenNhom { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
}

