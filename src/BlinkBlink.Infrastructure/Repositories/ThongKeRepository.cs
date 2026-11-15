using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Kho lưu trữ báo cáo & thống kê (ThongKeRepository) sử dụng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ báo cáo & thống kê.
/// </summary>
public class ThongKeRepository : IThongKeRepository
{
    private readonly DatabaseHelper _dbHelper;

    public ThongKeRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public ThongKeRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public List<HoaDon> TruyVanDoanhThu(DateTime tuNgay, DateTime denNgay)
    {
        var list = new List<HoaDon>();
        const string sql = @"
            SELECT h.MaHoaDon, h.NgayLap, h.MaKhachHang, h.MaNhanVien, h.DiaChiGiaoHang, h.SoDienThoaiNhan,
                   h.PhuongThucThanhToan, h.TrangThaiDonHang, h.TongTienHang, h.TienGiamGia, h.TongThanhToan, h.GhiChu
            FROM dbo.HoaDon h
            WHERE h.NgayLap >= @TuNgay AND h.NgayLap <= @DenNgay
            ORDER BY h.NgayLap ASC;";

        // Chuẩn hóa cận thời gian an toàn, chống lỗi tràn số khi denNgay là DateTime.MaxValue (ví dụ khi gọi Top Bán Chạy)
        var tuNgayChuan = tuNgay <= DateTime.MinValue.AddDays(1) ? DateTime.MinValue : tuNgay.Date;
        var denNgayChuan = denNgay >= DateTime.MaxValue.AddDays(-1) ? DateTime.MaxValue : denNgay.Date.AddDays(1).AddTicks(-1);

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@TuNgay", SqlDbType.DateTime2) { Value = tuNgayChuan });
        cmd.Parameters.Add(new SqlParameter("@DenNgay", SqlDbType.DateTime2) { Value = denNgayChuan });

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var hd = new HoaDon(
                    maHoaDon: reader.GetString(reader.GetOrdinal("MaHoaDon")),
                    ngayLap: reader.GetDateTime(reader.GetOrdinal("NgayLap")),
                    maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang")),
                    diaChiGiaoHang: reader.GetString(reader.GetOrdinal("DiaChiGiaoHang")),
                    soDienThoaiNhan: reader.GetString(reader.GetOrdinal("SoDienThoaiNhan")),
                    phuongThucThanhToan: reader.GetString(reader.GetOrdinal("PhuongThucThanhToan")),
                    trangThaiDonHang: reader.GetString(reader.GetOrdinal("TrangThaiDonHang")),
                    ghiChu: reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? string.Empty : reader.GetString(reader.GetOrdinal("GhiChu")),
                    maNhanVien: reader.IsDBNull(reader.GetOrdinal("MaNhanVien")) ? null : reader.GetString(reader.GetOrdinal("MaNhanVien"))
                )
                {
                    TongTienHang = reader.GetDecimal(reader.GetOrdinal("TongTienHang")),
                    TienGiamGia = reader.GetDecimal(reader.GetOrdinal("TienGiamGia")),
                    TongThanhToan = reader.GetDecimal(reader.GetOrdinal("TongThanhToan"))
                };

                list.Add(hd);
            }
        }

        foreach (var hd in list)
        {
            hd.ChiTietHoaDons = LayChiTietHoaDon(hd.MaHoaDon);
        }

        return list;
    }

    public Task<List<HoaDon>> TruyVanDoanhThuAsync(DateTime tuNgay, DateTime denNgay, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanDoanhThu(tuNgay, denNgay));
    }

    public List<SanPhamTrangSuc> TruyVanSanPhamTonKho() => TruyVanSanPhamTonKho(null);

    public List<SanPhamTrangSuc> TruyVanSanPhamTonKho(DateTime? denNgay)
    {
        var list = new List<SanPhamTrangSuc>();
        const string sql = @"
            SELECT s.MaSanPham, s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai,
                   d.TenDanhMuc, d.MoTa AS MoTaDanhMuc,
                   (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk 
                            INNER JOIN dbo.PhieuNhapKho pn ON nk.MaPhieuNhap = pn.MaPhieuNhap 
                            WHERE nk.MaSanPham = s.MaSanPham 
                              AND (pn.TrangThai = 'DaDuyet' OR pn.TrangThai = '1')
                              AND (@DenNgay IS NULL OR pn.NgayNhap <= @DenNgay)), 0)
                    - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd 
                             INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon 
                             WHERE chd.MaSanPham = s.MaSanPham 
                               AND hd.TrangThaiDonHang != 'DaHuy'
                               AND (@DenNgay IS NULL OR hd.NgayLap <= @DenNgay)), 0)
                    + ISNULL((SELECT SUM(ctk.ChenhLech) FROM dbo.ChiTietKiemKe ctk 
                             INNER JOIN dbo.PhieuKiemKe pk ON ctk.MaPhieuKiemKe = pk.MaPhieuKiemKe 
                             WHERE ctk.MaSanPham = s.MaSanPham 
                               AND (pk.TrangThai = 'DaDuyet' OR pk.TrangThaiDuyet = 1)
                               AND (@DenNgay IS NULL OR pk.NgayKiemKe <= @DenNgay)), 0)) AS SoLuongTonKho
            FROM dbo.SanPhamTrangSuc s
            LEFT JOIN dbo.DanhMuc d ON s.MaDanhMuc = d.MaDanhMuc
            ORDER BY s.MaSanPham ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@DenNgay", SqlDbType.DateTime) { Value = (object?)denNgay ?? DBNull.Value });
        using var reader = cmd.ExecuteReader();

        var ordTon = reader.GetOrdinal("SoLuongTonKho");
        while (reader.Read())
        {
            var sp = new SanPhamTrangSuc(
                maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
                tenSanPham: reader.GetString(reader.GetOrdinal("TenSanPham")),
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                chatLieu: reader.GetString(reader.GetOrdinal("ChatLieu")),
                size: reader.IsDBNull(reader.GetOrdinal("Size")) ? string.Empty : reader.GetString(reader.GetOrdinal("Size")),
                giaNhap: reader.GetDecimal(reader.GetOrdinal("GiaNhap")),
                giaBan: reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                trangThai: reader.GetString(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenDanhMuc")))
            {
                sp.DanhMuc = new DanhMuc(
                    maDanhMuc: sp.MaDanhMuc,
                    tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaDanhMuc")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaDanhMuc"))
                );
            }

            if (!reader.IsDBNull(ordTon))
            {
                sp.SoLuongTon = reader.GetInt32(ordTon);
            }

            list.Add(sp);
        }

        return list;
    }

    public Task<List<SanPhamTrangSuc>> TruyVanSanPhamTonKhoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanSanPhamTonKho());
    }

    public Task<List<SanPhamTrangSuc>> TruyVanSanPhamTonKhoAsync(DateTime? denNgay, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanSanPhamTonKho(denNgay));
    }

    public List<ChiTietKiemKe> TruyVanChenhLechKiemKe() => TruyVanChenhLechKiemKe(null, null);

    public List<ChiTietKiemKe> TruyVanChenhLechKiemKe(DateTime? tuNgay, DateTime? denNgay)
    {
        var list = new List<ChiTietKiemKe>();
        const string sql = @"
            SELECT c.MaPhieuKiemKe, c.MaSanPham, c.TonSoSach, c.TonThucTe, c.ChenhLech, c.LyDoChenhLech,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai
            FROM dbo.ChiTietKiemKe c
            INNER JOIN dbo.PhieuKiemKe pk ON c.MaPhieuKiemKe = pk.MaPhieuKiemKe
            LEFT JOIN dbo.SanPhamTrangSuc s ON c.MaSanPham = s.MaSanPham
            WHERE c.ChenhLech <> 0
              AND (pk.TrangThai = 'DaDuyet' OR pk.TrangThaiDuyet = 1)
              AND (@TuNgay IS NULL OR pk.NgayKiemKe >= @TuNgay)
              AND (@DenNgay IS NULL OR pk.NgayKiemKe <= @DenNgay);";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@TuNgay", SqlDbType.DateTime) { Value = (object?)tuNgay ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@DenNgay", SqlDbType.DateTime) { Value = (object?)denNgay ?? DBNull.Value });
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var ct = new ChiTietKiemKe(
                maPhieuKiemKe: reader.GetString(reader.GetOrdinal("MaPhieuKiemKe")),
                maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
                tonSoSach: reader.GetInt32(reader.GetOrdinal("TonSoSach")),
                tonThucTe: reader.GetInt32(reader.GetOrdinal("TonThucTe"))
            )
            {
                ChenhLech = reader.GetInt32(reader.GetOrdinal("ChenhLech")),
                LyDoChenhLech = reader.IsDBNull(reader.GetOrdinal("LyDoChenhLech")) ? string.Empty : reader.GetString(reader.GetOrdinal("LyDoChenhLech"))
            };

            if (!reader.IsDBNull(reader.GetOrdinal("TenSanPham")))
            {
                ct.SanPhamTrangSuc = new SanPhamTrangSuc(
                    maSanPham: ct.MaSanPham,
                    tenSanPham: reader.GetString(reader.GetOrdinal("TenSanPham")),
                    maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                    chatLieu: reader.GetString(reader.GetOrdinal("ChatLieu")),
                    size: reader.IsDBNull(reader.GetOrdinal("Size")) ? string.Empty : reader.GetString(reader.GetOrdinal("Size")),
                    giaNhap: reader.GetDecimal(reader.GetOrdinal("GiaNhap")),
                    giaBan: reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    trangThai: reader.GetString(reader.GetOrdinal("TrangThai"))
                );
            }

            list.Add(ct);
        }

        return list;
    }

    public Task<List<ChiTietKiemKe>> TruyVanChenhLechKiemKeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanChenhLechKiemKe());
    }

    public Task<List<ChiTietKiemKe>> TruyVanChenhLechKiemKeAsync(DateTime? tuNgay, DateTime? denNgay, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanChenhLechKiemKe(tuNgay, denNgay));
    }

    public List<SanPhamTrangSuc> TruyVanSanPhamDoanhThuThap()
    {
        var list = new List<SanPhamTrangSuc>();
        const string sql = @"
            SELECT s.MaSanPham, s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai,
                   d.TenDanhMuc, d.MoTa AS MoTaDanhMuc
            FROM dbo.SanPhamTrangSuc s
            LEFT JOIN dbo.DanhMuc d ON s.MaDanhMuc = d.MaDanhMuc;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var sp = new SanPhamTrangSuc(
                maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
                tenSanPham: reader.GetString(reader.GetOrdinal("TenSanPham")),
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                chatLieu: reader.GetString(reader.GetOrdinal("ChatLieu")),
                size: reader.IsDBNull(reader.GetOrdinal("Size")) ? string.Empty : reader.GetString(reader.GetOrdinal("Size")),
                giaNhap: reader.GetDecimal(reader.GetOrdinal("GiaNhap")),
                giaBan: reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                trangThai: reader.GetString(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenDanhMuc")))
            {
                sp.DanhMuc = new DanhMuc(
                    maDanhMuc: sp.MaDanhMuc,
                    tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaDanhMuc")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaDanhMuc"))
                );
            }

            list.Add(sp);
        }

        return list;
    }

    public Task<List<SanPhamTrangSuc>> TruyVanSanPhamDoanhThuThapAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanSanPhamDoanhThuThap());
    }

    private List<ChiTietHoaDon> LayChiTietHoaDon(string soHoaDon)
    {
        var list = new List<ChiTietHoaDon>();
        const string sql = @"
            SELECT c.SoHoaDon, c.MaSanPham, c.DonGia, c.SoLuong,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai
            FROM dbo.ChiTietHoaDon c
            LEFT JOIN dbo.SanPhamTrangSuc s ON c.MaSanPham = s.MaSanPham
            WHERE c.SoHoaDon = @SoHoaDon;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SoHoaDon", SqlDbType.VarChar, 50) { Value = soHoaDon });

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var ct = new ChiTietHoaDon(
                soHoaDon: reader.GetString(reader.GetOrdinal("SoHoaDon")),
                maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
                donGia: reader.GetDecimal(reader.GetOrdinal("DonGia")),
                soLuong: reader.GetInt32(reader.GetOrdinal("SoLuong"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenSanPham")))
            {
                ct.SanPham = new SanPhamTrangSuc(
                    maSanPham: ct.MaSanPham,
                    tenSanPham: reader.GetString(reader.GetOrdinal("TenSanPham")),
                    maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                    chatLieu: reader.GetString(reader.GetOrdinal("ChatLieu")),
                    size: reader.IsDBNull(reader.GetOrdinal("Size")) ? string.Empty : reader.GetString(reader.GetOrdinal("Size")),
                    giaNhap: reader.GetDecimal(reader.GetOrdinal("GiaNhap")),
                    giaBan: reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                    trangThai: reader.GetString(reader.GetOrdinal("TrangThai"))
                );
            }

            list.Add(ct);
        }

        return list;
    }
}
