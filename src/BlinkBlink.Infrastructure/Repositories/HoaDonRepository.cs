using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể HoaDon bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class HoaDonRepository : IHoaDonRepository
{
    private readonly DatabaseHelper _dbHelper;

    public HoaDonRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public HoaDonRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuHoaDon(HoaDon hd)
    {
        if (hd == null)
        {
            throw new ArgumentNullException(nameof(hd), "Hóa đơn không được để trống.");
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();

        try
        {
            const string sqlHoaDon = @"
                IF EXISTS (SELECT 1 FROM dbo.HoaDon WHERE MaHoaDon = @MaHoaDon)
                BEGIN
                    UPDATE dbo.HoaDon
                    SET NgayLap = @NgayLap,
                        MaKhachHang = CASE 
                            WHEN EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = @MaKhachHang) THEN @MaKhachHang
                            WHEN EXISTS (SELECT 1 FROM dbo.KhachHang WHERE MaKhachHang = @MaKhachHang) THEN (SELECT TOP 1 MaNguoiDung FROM dbo.KhachHang WHERE MaKhachHang = @MaKhachHang)
                            ELSE @MaKhachHang
                        END,
                        MaNhanVien = CASE 
                            WHEN @MaNhanVien IS NULL THEN NULL
                            WHEN EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = @MaNhanVien) THEN @MaNhanVien
                            WHEN EXISTS (SELECT 1 FROM dbo.NhanVien WHERE MaNhanVien = @MaNhanVien) THEN (SELECT TOP 1 MaNguoiDung FROM dbo.NhanVien WHERE MaNhanVien = @MaNhanVien)
                            ELSE @MaNhanVien
                        END,
                        DiaChiGiaoHang = @DiaChiGiaoHang,
                        SoDienThoaiNhan = @SoDienThoaiNhan,
                        PhuongThucThanhToan = @PhuongThucThanhToan,
                        TrangThaiDonHang = @TrangThaiDonHang,
                        TongTienHang = @TongTienHang,
                        TienGiamGia = @TienGiamGia,
                        TongThanhToan = @TongThanhToan,
                        GhiChu = @GhiChu
                    WHERE MaHoaDon = @MaHoaDon;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.HoaDon (MaHoaDon, NgayLap, MaKhachHang, MaNhanVien, DiaChiGiaoHang, SoDienThoaiNhan, PhuongThucThanhToan, TrangThaiDonHang, TongTienHang, TienGiamGia, TongThanhToan, GhiChu)
                    VALUES (@MaHoaDon, @NgayLap, 
                        CASE 
                            WHEN EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = @MaKhachHang) THEN @MaKhachHang
                            WHEN EXISTS (SELECT 1 FROM dbo.KhachHang WHERE MaKhachHang = @MaKhachHang) THEN (SELECT TOP 1 MaNguoiDung FROM dbo.KhachHang WHERE MaKhachHang = @MaKhachHang)
                            ELSE @MaKhachHang
                        END, 
                        CASE 
                            WHEN @MaNhanVien IS NULL THEN NULL
                            WHEN EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = @MaNhanVien) THEN @MaNhanVien
                            WHEN EXISTS (SELECT 1 FROM dbo.NhanVien WHERE MaNhanVien = @MaNhanVien) THEN (SELECT TOP 1 MaNguoiDung FROM dbo.NhanVien WHERE MaNhanVien = @MaNhanVien)
                            ELSE @MaNhanVien
                        END, 
                        @DiaChiGiaoHang, @SoDienThoaiNhan, @PhuongThucThanhToan, @TrangThaiDonHang, @TongTienHang, @TienGiamGia, @TongThanhToan, @GhiChu);
                END";

            using (var cmdHd = new SqlCommand(sqlHoaDon, conn, tx))
            {
                cmdHd.Parameters.Add(new SqlParameter("@MaHoaDon", SqlDbType.VarChar, 50) { Value = hd.MaHoaDon });
                cmdHd.Parameters.Add(new SqlParameter("@NgayLap", SqlDbType.DateTime2) { Value = hd.NgayLap });
                cmdHd.Parameters.Add(new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = hd.MaKhachHang });
                cmdHd.Parameters.Add(new SqlParameter("@MaNhanVien", SqlDbType.VarChar, 50) { Value = (object?)hd.MaNhanVien ?? DBNull.Value });
                cmdHd.Parameters.Add(new SqlParameter("@DiaChiGiaoHang", SqlDbType.NVarChar, 500) { Value = hd.DiaChiGiaoHang });
                cmdHd.Parameters.Add(new SqlParameter("@SoDienThoaiNhan", SqlDbType.VarChar, 20) { Value = hd.SoDienThoaiNhan });
                cmdHd.Parameters.Add(new SqlParameter("@PhuongThucThanhToan", SqlDbType.NVarChar, 50) { Value = hd.PhuongThucThanhToan });
                cmdHd.Parameters.Add(new SqlParameter("@TrangThaiDonHang", SqlDbType.NVarChar, 50) { Value = hd.TrangThaiDonHang });
                cmdHd.Parameters.Add(new SqlParameter("@TongTienHang", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = hd.TongTienHang });
                cmdHd.Parameters.Add(new SqlParameter("@TienGiamGia", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = hd.TienGiamGia });
                cmdHd.Parameters.Add(new SqlParameter("@TongThanhToan", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = hd.TongThanhToan });
                cmdHd.Parameters.Add(new SqlParameter("@GhiChu", SqlDbType.NVarChar, 500) { Value = (object?)hd.GhiChu ?? DBNull.Value });
                cmdHd.ExecuteNonQuery();
            }

            // Xóa và ghi lại các dòng chi tiết
            const string sqlDeleteCt = "DELETE FROM dbo.ChiTietHoaDon WHERE SoHoaDon = @SoHoaDon;";
            using (var cmdDel = new SqlCommand(sqlDeleteCt, conn, tx))
            {
                cmdDel.Parameters.Add(new SqlParameter("@SoHoaDon", SqlDbType.VarChar, 50) { Value = hd.MaHoaDon });
                cmdDel.ExecuteNonQuery();
            }

            if (hd.ChiTietHoaDons != null && hd.ChiTietHoaDons.Count > 0)
            {
                const string sqlInsertCt = @"
                    INSERT INTO dbo.ChiTietHoaDon (SoHoaDon, MaSanPham, DonGia, SoLuong)
                    VALUES (@SoHoaDon, @MaSanPham, @DonGia, @SoLuong);";

                foreach (var ct in hd.ChiTietHoaDons)
                {
                    using var cmdCt = new SqlCommand(sqlInsertCt, conn, tx);
                    cmdCt.Parameters.Add(new SqlParameter("@SoHoaDon", SqlDbType.VarChar, 50) { Value = hd.MaHoaDon });
                    cmdCt.Parameters.Add(new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = ct.MaSanPham });
                    cmdCt.Parameters.Add(new SqlParameter("@DonGia", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = ct.DonGia });
                    cmdCt.Parameters.Add(new SqlParameter("@SoLuong", SqlDbType.Int) { Value = ct.SoLuong });
                    cmdCt.ExecuteNonQuery();
                }
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public HoaDon? TimTheoMa(string maHoaDon)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon))
        {
            return null;
        }

        const string sql = @"
            SELECT h.MaHoaDon, h.NgayLap, h.MaKhachHang, h.MaNhanVien, h.DiaChiGiaoHang, h.SoDienThoaiNhan,
                   h.PhuongThucThanhToan, h.TrangThaiDonHang, h.TongTienHang, h.TienGiamGia, h.TongThanhToan, h.GhiChu,
                   uKh.HoTen AS TenKhachHang, uKh.SoDienThoai AS SdtKhachHang, kh.MaKhachHang AS MaDinhDanhKhachHang, kh.NgaySinh AS NgaySinhKhachHang,
                   uNv.HoTen AS TenNhanVien, nv.MaNhanVien AS MaDinhDanhNhanVien
            FROM dbo.HoaDon h
            LEFT JOIN dbo.NguoiDung uKh ON h.MaKhachHang = uKh.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON uKh.MaNguoiDung = kh.MaNguoiDung
            LEFT JOIN dbo.NguoiDung uNv ON h.MaNhanVien = uNv.MaNguoiDung
            LEFT JOIN dbo.NhanVien nv ON uNv.MaNguoiDung = nv.MaNguoiDung
            WHERE h.MaHoaDon = @MaHoaDon;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaHoaDon", SqlDbType.VarChar, 50) { Value = maHoaDon.Trim() });

        HoaDon? hd = null;
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                hd = MapReaderToHoaDon(reader);
            }
        }

        if (hd != null)
        {
            hd.ChiTietHoaDons = LayChiTietHoaDon(hd.MaHoaDon);
        }

        return hd;
    }

    public List<HoaDon> LayTheoKhachHang(string maKH)
    {
        var list = new List<HoaDon>();
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return list;
        }

        const string sql = @"
            SELECT h.MaHoaDon, h.NgayLap, h.MaKhachHang, h.MaNhanVien, h.DiaChiGiaoHang, h.SoDienThoaiNhan,
                   h.PhuongThucThanhToan, h.TrangThaiDonHang, h.TongTienHang, h.TienGiamGia, h.TongThanhToan, h.GhiChu,
                   uKh.HoTen AS TenKhachHang, uKh.SoDienThoai AS SdtKhachHang, kh.MaKhachHang AS MaDinhDanhKhachHang, kh.NgaySinh AS NgaySinhKhachHang,
                   uNv.HoTen AS TenNhanVien, nv.MaNhanVien AS MaDinhDanhNhanVien
            FROM dbo.HoaDon h
            LEFT JOIN dbo.NguoiDung uKh ON h.MaKhachHang = uKh.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON uKh.MaNguoiDung = kh.MaNguoiDung
            LEFT JOIN dbo.NguoiDung uNv ON h.MaNhanVien = uNv.MaNguoiDung
            LEFT JOIN dbo.NhanVien nv ON uNv.MaNguoiDung = nv.MaNguoiDung
            WHERE h.MaKhachHang = @MaKhachHang OR kh.MaKhachHang = @MaKhachHang
            ORDER BY h.NgayLap DESC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = maKH.Trim() });

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                list.Add(MapReaderToHoaDon(reader));
            }
        }

        foreach (var hd in list)
        {
            hd.ChiTietHoaDons = LayChiTietHoaDon(hd.MaHoaDon);
        }

        return list;
    }

    public List<HoaDon> LayDanhSachDonHang()
    {
        var list = new List<HoaDon>();

        const string sql = @"
            SELECT h.MaHoaDon, h.NgayLap, h.MaKhachHang, h.MaNhanVien, h.DiaChiGiaoHang, h.SoDienThoaiNhan,
                   h.PhuongThucThanhToan, h.TrangThaiDonHang, h.TongTienHang, h.TienGiamGia, h.TongThanhToan, h.GhiChu,
                   uKh.HoTen AS TenKhachHang, uKh.SoDienThoai AS SdtKhachHang, kh.MaKhachHang AS MaDinhDanhKhachHang, kh.NgaySinh AS NgaySinhKhachHang,
                   uNv.HoTen AS TenNhanVien, nv.MaNhanVien AS MaDinhDanhNhanVien
            FROM dbo.HoaDon h
            LEFT JOIN dbo.NguoiDung uKh ON h.MaKhachHang = uKh.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON uKh.MaNguoiDung = kh.MaNguoiDung
            LEFT JOIN dbo.NguoiDung uNv ON h.MaNhanVien = uNv.MaNguoiDung
            LEFT JOIN dbo.NhanVien nv ON uNv.MaNguoiDung = nv.MaNguoiDung
            ORDER BY h.NgayLap DESC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                list.Add(MapReaderToHoaDon(reader));
            }
        }

        foreach (var hd in list)
        {
            hd.ChiTietHoaDons = LayChiTietHoaDon(hd.MaHoaDon);
        }

        return list;
    }

    public void CapNhatHoaDon(HoaDon hd)
    {
        if (hd == null)
        {
            return;
        }

        const string sql = @"
            UPDATE dbo.HoaDon
            SET MaNhanVien = CASE 
                    WHEN @MaNhanVien IS NULL THEN NULL
                    WHEN EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = @MaNhanVien) THEN @MaNhanVien
                    WHEN EXISTS (SELECT 1 FROM dbo.NhanVien WHERE MaNhanVien = @MaNhanVien) THEN (SELECT TOP 1 MaNguoiDung FROM dbo.NhanVien WHERE MaNhanVien = @MaNhanVien)
                    ELSE @MaNhanVien
                END,
                TrangThaiDonHang = @TrangThaiDonHang,
                GhiChu = @GhiChu,
                TongTienHang = @TongTienHang,
                TienGiamGia = @TienGiamGia,
                TongThanhToan = @TongThanhToan
            WHERE MaHoaDon = @MaHoaDon;";

        var parameters = new[]
        {
            new SqlParameter("@MaHoaDon", SqlDbType.VarChar, 50) { Value = hd.MaHoaDon },
            new SqlParameter("@MaNhanVien", SqlDbType.VarChar, 50) { Value = (object?)hd.MaNhanVien ?? DBNull.Value },
            new SqlParameter("@TrangThaiDonHang", SqlDbType.NVarChar, 50) { Value = hd.TrangThaiDonHang },
            new SqlParameter("@GhiChu", SqlDbType.NVarChar, 500) { Value = (object?)hd.GhiChu ?? DBNull.Value },
            new SqlParameter("@TongTienHang", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = hd.TongTienHang },
            new SqlParameter("@TienGiamGia", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = hd.TienGiamGia },
            new SqlParameter("@TongThanhToan", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = hd.TongThanhToan }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    public Task LuuHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default)
    {
        LuuHoaDon(hd);
        return Task.CompletedTask;
    }

    public Task<HoaDon?> TimTheoMaAsync(string maHoaDon, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoMa(maHoaDon));
    }

    public Task<List<HoaDon>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTheoKhachHang(maKH));
    }

    public Task<List<HoaDon>> LayDanhSachDonHangAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayDanhSachDonHang());
    }

    public Task CapNhatHoaDonAsync(HoaDon hd, CancellationToken cancellationToken = default)
    {
        CapNhatHoaDon(hd);
        return Task.CompletedTask;
    }

    private List<ChiTietHoaDon> LayChiTietHoaDon(string soHoaDon)
    {
        var list = new List<ChiTietHoaDon>();
        const string sql = @"
            SELECT c.SoHoaDon, c.MaSanPham, c.DonGia, c.SoLuong,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai, s.HinhAnh, s.PhanTramGiamGia
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
                    trangThai: reader.GetString(reader.GetOrdinal("TrangThai")),
                    hinhAnh: reader.IsDBNull(reader.GetOrdinal("HinhAnh")) ? string.Empty : reader.GetString(reader.GetOrdinal("HinhAnh")),
                    phanTramGiamGia: reader.IsDBNull(reader.GetOrdinal("PhanTramGiamGia")) ? 0 : reader.GetInt32(reader.GetOrdinal("PhanTramGiamGia"))
                );
            }

            list.Add(ct);
        }

        return list;
    }

    private static HoaDon MapReaderToHoaDon(SqlDataReader reader)
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

        return hd;
    }
}
