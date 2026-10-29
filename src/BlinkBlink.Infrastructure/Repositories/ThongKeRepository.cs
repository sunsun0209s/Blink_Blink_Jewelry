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

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@TuNgay", SqlDbType.DateTime2) { Value = tuNgay });
        cmd.Parameters.Add(new SqlParameter("@DenNgay", SqlDbType.DateTime2) { Value = denNgay });

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

    public List<SanPhamTrangSuc> TruyVanSanPhamTonKho()
    {
        var list = new List<SanPhamTrangSuc>();
        const string sql = @"
            SELECT s.MaSanPham, s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai,
                   d.TenDanhMuc, d.MoTa AS MoTaDanhMuc
            FROM dbo.SanPhamTrangSuc s
            LEFT JOIN dbo.DanhMuc d ON s.MaDanhMuc = d.MaDanhMuc
            WHERE s.TrangThai = 'SanSang';";

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

    public Task<List<SanPhamTrangSuc>> TruyVanSanPhamTonKhoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TruyVanSanPhamTonKho());
    }

    public List<ChiTietKiemKe> TruyVanChenhLechKiemKe()
    {
        var list = new List<ChiTietKiemKe>();
        const string sql = @"
            SELECT c.MaPhieuKiemKe, c.MaSanPham, c.TonSoSach, c.TonThucTe, c.ChenhLech, c.LyDoChenhLech,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai
            FROM dbo.ChiTietKiemKe c
            LEFT JOIN dbo.SanPhamTrangSuc s ON c.MaSanPham = s.MaSanPham
            WHERE c.ChenhLech <> 0;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
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
