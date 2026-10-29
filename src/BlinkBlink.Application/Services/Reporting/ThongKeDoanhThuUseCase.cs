using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Reporting;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Application.Services.Reporting;

/// <summary>
/// Trường hợp sử dụng Thống kê doanh thu (ThongKeDoanhThuUseCase) dành cho Quản trị viên.
/// Tổng hợp doanh thu, số lượng đơn và lợi nhuận ước tính theo từng ngày trong khoảng thời gian.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public class ThongKeDoanhThuUseCase
{
    private readonly IThongKeRepository _thongKeRepo;

    public ThongKeDoanhThuUseCase(IThongKeRepository thongKeRepo)
    {
        _thongKeRepo = thongKeRepo ?? throw new ArgumentNullException(nameof(thongKeRepo));
    }

    /// <summary>
    /// Thực hiện trích xuất và tổng hợp báo cáo doanh thu theo mốc thời gian (đồng bộ).
    /// Khớp phương thức: + Execute(tuNgay: DateTime, denNgay: DateTime): List&lt;DoanhThuThongKeDTO&gt; trong Diagram.
    /// </summary>
    /// <param name="tuNgay">Ngày bắt đầu</param>
    /// <param name="denNgay">Ngày kết thúc</param>
    /// <returns>Danh sách thống kê doanh thu theo ngày</returns>
    public List<DoanhThuThongKeDTO> Execute(DateTime tuNgay, DateTime denNgay)
    {
        return ExecuteAsync(tuNgay, denNgay).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Thực hiện trích xuất và tổng hợp báo cáo doanh thu theo mốc thời gian (bất đồng bộ).
    /// </summary>
    /// <param name="tuNgay">Ngày bắt đầu</param>
    /// <param name="denNgay">Ngày kết thúc</param>
    /// <param name="cancellationToken">Mã token theo dõi hủy tác vụ</param>
    /// <returns>Danh sách thống kê doanh thu theo ngày</returns>
    public async Task<List<DoanhThuThongKeDTO>> ExecuteAsync(DateTime tuNgay, DateTime denNgay, CancellationToken cancellationToken = default)
    {
        // Điều chỉnh mốc thời gian: đảm bảo tuNgay <= denNgay
        if (tuNgay > denNgay)
        {
            (tuNgay, denNgay) = (denNgay, tuNgay);
        }

        var danhSachHoaDon = await _thongKeRepo.TruyVanDoanhThuAsync(tuNgay, denNgay, cancellationToken);
        if (danhSachHoaDon == null || danhSachHoaDon.Count == 0)
        {
            return new List<DoanhThuThongKeDTO>();
        }

        // Loại bỏ các đơn hàng bị hủy khi tính toán doanh thu thực tế
        var hopLe = danhSachHoaDon.Where(h => !string.Equals(h.TrangThaiDonHang, "DaHuy", StringComparison.OrdinalIgnoreCase));

        // Gom nhóm hóa đơn theo ngày tạo đơn
        var ketQua = hopLe
            .GroupBy(h => h.NgayLap.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var tongDoanhThu = g.Sum(h => h.TongThanhToan);

                // Tính tổng giá vốn nhập của tất cả món trang sức trong các đơn hàng của ngày đó
                decimal tongGiaVon = 0m;
                foreach (var hd in g)
                {
                    if (hd.ChiTietHoaDons != null)
                    {
                        foreach (var ct in hd.ChiTietHoaDons)
                        {
                            var giaNhap = ct.SanPham?.GiaNhap ?? 0m;
                            tongGiaVon += giaNhap * ct.SoLuong;
                        }
                    }
                }

                return new DoanhThuThongKeDTO
                {
                    NgayThang = g.Key,
                    SoLuongDon = g.Count(),
                    TongDoanhThu = tongDoanhThu,
                    LoiNhuanUocTinh = Math.Max(0m, tongDoanhThu - tongGiaVon)
                };
            })
            .ToList();

        return ketQua;
    }
}
