using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể VaiTro bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ người dùng & phân quyền.
/// </summary>
public class VaiTroRepository : IVaiTroRepository
{
    private readonly DatabaseHelper _dbHelper;

    public VaiTroRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public VaiTroRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public VaiTro? TimTheoMa(string maVaiTro)
    {
        if (string.IsNullOrWhiteSpace(maVaiTro))
        {
            return null;
        }

        const string sql = "SELECT MaVaiTro, TenVaiTro, MoTa FROM dbo.VaiTro WHERE MaVaiTro = @MaVaiTro;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaVaiTro", SqlDbType.VarChar, 20) { Value = maVaiTro.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new VaiTro(
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa"))
            );
        }

        return null;
    }

    public List<VaiTro> LayTatCa()
    {
        var list = new List<VaiTro>();
        const string sql = "SELECT MaVaiTro, TenVaiTro, MoTa FROM dbo.VaiTro;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new VaiTro(
                maVaiTro: reader.GetString(reader.GetOrdinal("MaVaiTro")),
                tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa"))
            ));
        }

        return list;
    }

    public void LuuVaiTro(VaiTro vt)
    {
        if (vt == null)
        {
            throw new ArgumentNullException(nameof(vt), "Vai trò không được để trống.");
        }

        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.VaiTro WHERE MaVaiTro = @MaVaiTro)
            BEGIN
                UPDATE dbo.VaiTro
                SET TenVaiTro = @TenVaiTro,
                    MoTa = @MoTa
                WHERE MaVaiTro = @MaVaiTro;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro, MoTa)
                VALUES (@MaVaiTro, @TenVaiTro, @MoTa);
            END";

        var parameters = new[]
        {
            new SqlParameter("@MaVaiTro", SqlDbType.VarChar, 20) { Value = vt.MaVaiTro },
            new SqlParameter("@TenVaiTro", SqlDbType.NVarChar, 100) { Value = vt.TenVaiTro },
            new SqlParameter("@MoTa", SqlDbType.NVarChar, 255) { Value = (object?)vt.MoTa ?? DBNull.Value }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }
}
