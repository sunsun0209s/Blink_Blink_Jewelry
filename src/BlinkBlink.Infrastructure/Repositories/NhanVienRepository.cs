using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Nhân viên thông qua ADO.NET thuần.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NhanVienRepository
/// </summary>
public class NhanVienRepository : INhanVienRepository
{
    private readonly DatabaseHelper _dbHelper;

    public NhanVienRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public NhanVienRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuNhanVien(NhanVien nv)
    {
        if (nv == null)
        {
            throw new ArgumentNullException(nameof(nv), "Hồ sơ nhân viên không được để trống.");
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
                cmdUser.Parameters.Add(new SqlParameter("@MaNguoiDung", SqlDbType.VarChar, 50) { Value = nv.MaNguoiDung });
                cmdUser.Parameters.Add(new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = nv.HoTen });
                cmdUser.Parameters.Add(new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 20) { Value = (object?)nv.SoDienThoai ?? DBNull.Value });
                cmdUser.Parameters.Add(new SqlParameter("@TenDangNhap", SqlDbType.VarChar, 50) { Value = nv.TenDangNhap });
                cmdUser.Parameters.Add(new SqlParameter("@MatKhau", SqlDbType.NVarChar, 255) { Value = nv.MatKhau });
                cmdUser.Parameters.Add(new SqlParameter("@MaVaiTro", SqlDbType.VarChar, 20) { Value = nv.MaVaiTro });
                cmdUser.Parameters.Add(new SqlParameter("@TrangThai", SqlDbType.Bit) { Value = nv.TrangThai });
                cmdUser.ExecuteNonQuery();
            }

            const string sqlNhanVien = @"
                IF EXISTS (SELECT 1 FROM dbo.NhanVien WHERE MaNguoiDung = @MaNguoiDung)
                BEGIN
                    UPDATE dbo.NhanVien
                    SET MaNhanVien = @MaNhanVien,
                        LuongCoBan = @LuongCoBan
                    WHERE MaNguoiDung = @MaNguoiDung;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.NhanVien (MaNguoiDung, MaNhanVien, LuongCoBan)
                    VALUES (@MaNguoiDung, @MaNhanVien, @LuongCoBan);
                END";

            using (var cmdNv = new SqlCommand(sqlNhanVien, conn, transaction))
            {
                cmdNv.Parameters.Add(new SqlParameter("@MaNguoiDung", SqlDbType.VarChar, 50) { Value = nv.MaNguoiDung });
                cmdNv.Parameters.Add(new SqlParameter("@MaNhanVien", SqlDbType.VarChar, 50) { Value = nv.MaNhanVien });
                cmdNv.Parameters.Add(new SqlParameter("@LuongCoBan", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = nv.LuongCoBan });
                cmdNv.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public NhanVien? TimTheoMa(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV))
        {
            return null;
        }

        const string sql = @"
            SELECT u.MaNguoiDung, u.HoTen, u.SoDienThoai, u.TenDangNhap, u.MatKhau, u.MaVaiTro, u.TrangThai,
                   nv.MaNhanVien, nv.LuongCoBan,
                   v.TenVaiTro, v.MoTa AS MoTaVaiTro
            FROM dbo.NhanVien nv
            INNER JOIN dbo.NguoiDung u ON nv.MaNguoiDung = u.MaNguoiDung
            LEFT JOIN dbo.VaiTro v ON u.MaVaiTro = v.MaVaiTro
            WHERE nv.MaNhanVien = @MaNV OR nv.MaNguoiDung = @MaNV;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaNV", SqlDbType.VarChar, 50) { Value = maNV.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var nv = new NhanVien(
                maNguoiDung: reader.GetString(reader.GetOrdinal("MaNguoiDung")),
                hoTen: reader.GetString(reader.GetOrdinal("HoTen")),
                soDienThoai: reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                tenDangNhap: reader.GetString(reader.GetOrdinal("TenDangNhap")),
                matKhau: reader.GetString(reader.GetOrdinal("MatKhau")),
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                maNhanVien: reader.GetString(reader.GetOrdinal("MaNhanVien")),
                luongCoBan: reader.GetDecimal(reader.GetOrdinal("LuongCoBan")),
                trangThai: reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenVaiTro")))
            {
                nv.VaiTro = new VaiTro(
                    maVaiTro: nv.MaVaiTro,
                    tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaVaiTro")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaVaiTro"))
                );
            }

            return nv;
        }

        return null;
    }

    public List<NhanVien> LayTatCa()
    {
        var list = new List<NhanVien>();
        const string sql = @"
            SELECT u.MaNguoiDung, u.HoTen, u.SoDienThoai, u.TenDangNhap, u.MatKhau, u.MaVaiTro, u.TrangThai,
                   nv.MaNhanVien, nv.LuongCoBan,
                   v.TenVaiTro, v.MoTa AS MoTaVaiTro
            FROM dbo.NhanVien nv
            INNER JOIN dbo.NguoiDung u ON nv.MaNguoiDung = u.MaNguoiDung
            LEFT JOIN dbo.VaiTro v ON u.MaVaiTro = v.MaVaiTro
            ORDER BY nv.MaNhanVien ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var nv = new NhanVien(
                maNguoiDung: reader.GetString(reader.GetOrdinal("MaNguoiDung")),
                hoTen: reader.GetString(reader.GetOrdinal("HoTen")),
                soDienThoai: reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoDienThoai")),
                tenDangNhap: reader.GetString(reader.GetOrdinal("TenDangNhap")),
                matKhau: reader.GetString(reader.GetOrdinal("MatKhau")),
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                maNhanVien: reader.GetString(reader.GetOrdinal("MaNhanVien")),
                luongCoBan: reader.GetDecimal(reader.GetOrdinal("LuongCoBan")),
                trangThai: reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("TenVaiTro")))
            {
                nv.VaiTro = new VaiTro(
                    maVaiTro: nv.MaVaiTro,
                    tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaVaiTro")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaVaiTro"))
                );
            }

            list.Add(nv);
        }

        return list;
    }

    public void CapNhatNhanVien(NhanVien nv)
    {
        LuuNhanVien(nv);
    }

    public bool KiemTraCoTheXoa(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV)) return false;

        const string sqlCheck = @"
            DECLARE @MaND VARCHAR(50);
            SELECT @MaND = MaNguoiDung FROM dbo.NhanVien WHERE MaNhanVien = @MaNV OR MaNguoiDung = @MaNV;
            IF @MaND IS NULL SET @MaND = @MaNV;

            DECLARE @CountPN INT = 0;
            DECLARE @CountHD INT = 0;
            DECLARE @CountKK INT = 0;

            SELECT @CountPN = COUNT(1) FROM dbo.PhieuNhapKho WHERE MaNhanVien = @MaND OR MaNhanVien = @MaNV;
            SELECT @CountHD = COUNT(1) FROM dbo.HoaDon WHERE MaNhanVien = @MaND OR MaNhanVien = @MaNV;
            SELECT @CountKK = COUNT(1) FROM dbo.PhieuKiemKe WHERE MaNhanVienKho = @MaND OR MaNhanVienKho = @MaNV;

            SELECT (@CountPN + @CountHD + @CountKK) AS TongPhatSinh;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sqlCheck, conn);
        cmd.Parameters.Add(new SqlParameter("@MaNV", SqlDbType.VarChar, 50) { Value = maNV.Trim() });

        var result = cmd.ExecuteScalar();
        if (result != null && int.TryParse(result.ToString(), out var count))
        {
            return count == 0;
        }

        return false;
    }

    public bool XoaNhanVien(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV) || !KiemTraCoTheXoa(maNV))
        {
            return false;
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            const string sqlDelete = @"
                DECLARE @MaND VARCHAR(50);
                SELECT @MaND = MaNguoiDung FROM dbo.NhanVien WHERE MaNhanVien = @MaNV OR MaNguoiDung = @MaNV;
                IF @MaND IS NULL SET @MaND = @MaNV;

                DELETE FROM dbo.NhanVien WHERE MaNguoiDung = @MaND OR MaNhanVien = @MaNV;
                DELETE FROM dbo.NguoiDung WHERE MaNguoiDung = @MaND;";

            using var cmd = new SqlCommand(sqlDelete, conn, tx);
            cmd.Parameters.Add(new SqlParameter("@MaNV", SqlDbType.VarChar, 50) { Value = maNV.Trim() });
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
