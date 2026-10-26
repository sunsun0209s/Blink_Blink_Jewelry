using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Khách hàng thông qua ADO.NET thuần.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - KhachHangRepository
/// </summary>
public class KhachHangRepository : IKhachHangRepository
{
    private readonly DatabaseHelper _dbHelper;

    public KhachHangRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public KhachHangRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuKhachHang(KhachHang kh)
    {
        if (kh == null)
        {
            throw new ArgumentNullException(nameof(kh), "Hồ sơ khách hàng không được để trống.");
        }

        using var conn = _dbHelper.GetConnection();
        using var transaction = conn.BeginTransaction();

        try
        {
            const string sqlNguoiDung = @"
                IF EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = @MaNguoiDung)
                BEGIN
                    UPDATE dbo.NguoiDung
                    SET HoTen = @HoTen,
                        SoDienThoai = @SoDienThoai,
                        TenDangNhap = @TenDangNhap,
                        MatKhau = @MatKhau,
                        MaVaiTro = @MaVaiTro,
                        TrangThai = @TrangThai
                    WHERE MaNguoiDung = @MaNguoiDung;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.NguoiDung (MaNguoiDung, HoTen, SoDienThoai, TenDangNhap, MatKhau, MaVaiTro, TrangThai)
                    VALUES (@MaNguoiDung, @HoTen, @SoDienThoai, @TenDangNhap, @MatKhau, @MaVaiTro, @TrangThai);
                END";

            using (var cmdUser = new SqlCommand(sqlNguoiDung, conn, transaction))
            {
                cmdUser.Parameters.Add(new SqlParameter("@MaNguoiDung", SqlDbType.VarChar, 50) { Value = kh.MaNguoiDung });
                cmdUser.Parameters.Add(new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = kh.HoTen });
                cmdUser.Parameters.Add(new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 20) { Value = (object?)kh.SoDienThoai ?? DBNull.Value });
                cmdUser.Parameters.Add(new SqlParameter("@TenDangNhap", SqlDbType.VarChar, 50) { Value = kh.TenDangNhap });
                cmdUser.Parameters.Add(new SqlParameter("@MatKhau", SqlDbType.NVarChar, 255) { Value = kh.MatKhau });
                cmdUser.Parameters.Add(new SqlParameter("@MaVaiTro", SqlDbType.VarChar, 20) { Value = kh.MaVaiTro });
                cmdUser.Parameters.Add(new SqlParameter("@TrangThai", SqlDbType.Bit) { Value = kh.TrangThai });
                cmdUser.ExecuteNonQuery();
            }

            const string sqlKhachHang = @"
                IF EXISTS (SELECT 1 FROM dbo.KhachHang WHERE MaNguoiDung = @MaNguoiDung)
                BEGIN
                    UPDATE dbo.KhachHang
                    SET MaKhachHang = @MaKhachHang,
                        NgaySinh = @NgaySinh
                    WHERE MaNguoiDung = @MaNguoiDung;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.KhachHang (MaNguoiDung, MaKhachHang, NgaySinh)
                    VALUES (@MaNguoiDung, @MaKhachHang, @NgaySinh);
                END";

            using (var cmdKh = new SqlCommand(sqlKhachHang, conn, transaction))
            {
                cmdKh.Parameters.Add(new SqlParameter("@MaNguoiDung", SqlDbType.VarChar, 50) { Value = kh.MaNguoiDung });
                cmdKh.Parameters.Add(new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = kh.MaKhachHang });
                cmdKh.Parameters.Add(new SqlParameter("@NgaySinh", SqlDbType.Date) { Value = kh.NgaySinh });
                cmdKh.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public KhachHang? TimTheoMa(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return null;
        }

        const string sql = @"
            SELECT u.MaNguoiDung, u.HoTen, u.SoDienThoai, u.TenDangNhap, u.MatKhau, u.MaVaiTro, u.TrangThai,
                   kh.MaKhachHang, kh.NgaySinh,
                   v.TenVaiTro, v.MoTa AS MoTaVaiTro
            FROM dbo.KhachHang kh
            INNER JOIN dbo.NguoiDung u ON kh.MaNguoiDung = u.MaNguoiDung
            LEFT JOIN dbo.VaiTro v ON u.MaVaiTro = v.MaVaiTro
            WHERE kh.MaKhachHang = @MaKH OR kh.MaNguoiDung = @MaKH;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaKH", SqlDbType.VarChar, 50) { Value = maKH.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var kh = new KhachHang(
                maNguoiDung: reader.GetString(reader.GetOrdinal("MaNguoiDung")),
                hoTen: reader.GetString(reader.GetOrdinal("HoTen")),
                soDienThoai: reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                tenDangNhap: reader.GetString(reader.GetOrdinal("TenDangNhap")),
                matKhau: reader.GetString(reader.GetOrdinal("MatKhau")),
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang")),
                ngaySinh: reader.GetDateTime(reader.GetOrdinal("NgaySinh")),
                trangThai: reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenVaiTro")))
            {
                kh.VaiTro = new VaiTro(
                    maVaiTro: kh.MaVaiTro,
                    tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaVaiTro")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaVaiTro"))
                );
            }

            return kh;
        }

        return null;
    }

    public KhachHang? TimTheoSoDienThoai(string soDienThoai)
    {
        if (string.IsNullOrWhiteSpace(soDienThoai))
        {
            return null;
        }

        const string sql = @"
            SELECT u.MaNguoiDung, u.HoTen, u.SoDienThoai, u.TenDangNhap, u.MatKhau, u.MaVaiTro, u.TrangThai,
                   kh.MaKhachHang, kh.NgaySinh,
                   v.TenVaiTro, v.MoTa AS MoTaVaiTro
            FROM dbo.KhachHang kh
            INNER JOIN dbo.NguoiDung u ON kh.MaNguoiDung = u.MaNguoiDung
            LEFT JOIN dbo.VaiTro v ON u.MaVaiTro = v.MaVaiTro
            WHERE u.SoDienThoai = @SoDienThoai;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 20) { Value = soDienThoai.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var kh = new KhachHang(
                maNguoiDung: reader.GetString(reader.GetOrdinal("MaNguoiDung")),
                hoTen: reader.GetString(reader.GetOrdinal("HoTen")),
                soDienThoai: reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                tenDangNhap: reader.GetString(reader.GetOrdinal("TenDangNhap")),
                matKhau: reader.GetString(reader.GetOrdinal("MatKhau")),
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang")),
                ngaySinh: reader.GetDateTime(reader.GetOrdinal("NgaySinh")),
                trangThai: reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenVaiTro")))
            {
                kh.VaiTro = new VaiTro(
                    maVaiTro: kh.MaVaiTro,
                    tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaVaiTro")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaVaiTro"))
                );
            }

            return kh;
        }

        return null;
    }

    public List<KhachHang> LayTatCa()
    {
        var list = new List<KhachHang>();
        const string sql = @"
            SELECT u.MaNguoiDung, u.HoTen, u.SoDienThoai, u.TenDangNhap, u.MatKhau, u.MaVaiTro, u.TrangThai,
                   kh.MaKhachHang, kh.NgaySinh,
                   v.TenVaiTro, v.MoTa AS MoTaVaiTro
            FROM dbo.KhachHang kh
            INNER JOIN dbo.NguoiDung u ON kh.MaNguoiDung = u.MaNguoiDung
            LEFT JOIN dbo.VaiTro v ON u.MaVaiTro = v.MaVaiTro
            ORDER BY kh.MaKhachHang ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var kh = new KhachHang(
                maNguoiDung: reader.GetString(reader.GetOrdinal("MaNguoiDung")),
                hoTen: reader.GetString(reader.GetOrdinal("HoTen")),
                soDienThoai: reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                tenDangNhap: reader.GetString(reader.GetOrdinal("TenDangNhap")),
                matKhau: reader.GetString(reader.GetOrdinal("MatKhau")),
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang")),
                ngaySinh: reader.GetDateTime(reader.GetOrdinal("NgaySinh")),
                trangThai: reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenVaiTro")))
            {
                kh.VaiTro = new VaiTro(
                    maVaiTro: kh.MaVaiTro,
                    tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaVaiTro")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaVaiTro"))
                );
            }

            list.Add(kh);
        }

        return list;
    }

    public bool KiemTraCoTheXoa(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH)) return false;

        const string sqlCheck = @"
            DECLARE @MaND VARCHAR(50);
            SELECT @MaND = MaNguoiDung FROM dbo.KhachHang WHERE MaKhachHang = @MaKH OR MaNguoiDung = @MaKH;
            IF @MaND IS NULL SET @MaND = @MaKH;

            DECLARE @CountHD INT = 0;
            DECLARE @CountBH INT = 0;

            SELECT @CountHD = COUNT(1) FROM dbo.HoaDon WHERE MaKhachHang = @MaND OR MaKhachHang = @MaKH;
            SELECT @CountBH = COUNT(1) FROM dbo.PhieuBaoHanh WHERE MaKhachHang = @MaND OR MaKhachHang = @MaKH;

            SELECT (@CountHD + @CountBH) AS TongPhatSinh;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sqlCheck, conn);
        cmd.Parameters.Add(new SqlParameter("@MaKH", SqlDbType.VarChar, 50) { Value = maKH.Trim() });

        var result = cmd.ExecuteScalar();
        if (result != null && int.TryParse(result.ToString(), out var count))
        {
            return count == 0;
        }

        return false;
    }

    public bool XoaKhachHang(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH) || !KiemTraCoTheXoa(maKH))
        {
            return false;
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            const string sqlDelete = @"
                DECLARE @MaND VARCHAR(50);
                SELECT @MaND = MaNguoiDung FROM dbo.KhachHang WHERE MaKhachHang = @MaKH OR MaNguoiDung = @MaKH;
                IF @MaND IS NULL SET @MaND = @MaKH;

                -- Xóa giỏ hàng phụ thuộc nếu có
                DELETE FROM dbo.ChiTietGioHang WHERE MaGioHang IN (SELECT MaGioHang FROM dbo.GioHang WHERE MaKhachHang = @MaND);
                DELETE FROM dbo.GioHang WHERE MaKhachHang = @MaND;

                DELETE FROM dbo.KhachHang WHERE MaNguoiDung = @MaND OR MaKhachHang = @MaKH;
                DELETE FROM dbo.NguoiDung WHERE MaNguoiDung = @MaND;";

            using var cmd = new SqlCommand(sqlDelete, conn, tx);
            cmd.Parameters.Add(new SqlParameter("@MaKH", SqlDbType.VarChar, 50) { Value = maKH.Trim() });
            cmd.ExecuteNonQuery();

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }
}
