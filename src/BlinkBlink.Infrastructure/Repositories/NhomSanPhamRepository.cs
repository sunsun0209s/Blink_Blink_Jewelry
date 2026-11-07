using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể Nhóm sản phẩm (NhomSanPham) bằng ADO.NET thuần.
/// </summary>
public class NhomSanPhamRepository : INhomSanPhamRepository
{
    private readonly DatabaseHelper _dbHelper;

    public NhomSanPhamRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public NhomSanPhamRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuNhom(NhomSanPham nhom)
    {
        if (nhom == null)
        {
            throw new ArgumentNullException(nameof(nhom), "Nhóm sản phẩm không được để trống.");
        }

        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.NhomSanPham WHERE MaNhom = @MaNhom)
            BEGIN
                UPDATE dbo.NhomSanPham
                SET TenNhom = @TenNhom,
                    MoTa = @MoTa
                WHERE MaNhom = @MaNhom;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.NhomSanPham (MaNhom, TenNhom, MoTa)
                VALUES (@MaNhom, @TenNhom, @MoTa);
            END";

        var parameters = new[]
        {
            new SqlParameter("@MaNhom", SqlDbType.NVarChar, 50) { Value = nhom.MaNhom },
            new SqlParameter("@TenNhom", SqlDbType.NVarChar, 255) { Value = nhom.TenNhom },
            new SqlParameter("@MoTa", SqlDbType.NVarChar, 500) { Value = (object?)nhom.MoTa ?? DBNull.Value }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    public void CapNhatNhom(NhomSanPham nhom)
    {
        LuuNhom(nhom);
    }

    public bool XoaNhom(string maNhom)
    {
        if (string.IsNullOrWhiteSpace(maNhom))
        {
            return false;
        }

        const string sql = "DELETE FROM dbo.NhomSanPham WHERE MaNhom = @MaNhom;";
        var parameters = new[]
        {
            new SqlParameter("@MaNhom", SqlDbType.NVarChar, 50) { Value = maNhom }
        };

        return _dbHelper.ExecuteNonQuery(sql, parameters) > 0;
    }

    public NhomSanPham? TimTheoId(string maNhom)
    {
        if (string.IsNullOrWhiteSpace(maNhom))
        {
            return null;
        }

        const string sql = "SELECT MaNhom, TenNhom, MoTa FROM dbo.NhomSanPham WHERE MaNhom = @MaNhom;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaNhom", SqlDbType.NVarChar, 50) { Value = maNhom.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new NhomSanPham(
                maNhom: reader.GetString(reader.GetOrdinal("MaNhom")),
                tenNhom: reader.GetString(reader.GetOrdinal("TenNhom")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa"))
            );
        }

        return null;
    }

    public List<NhomSanPham> LayTatCa()
    {
        var list = new List<NhomSanPham>();
        const string sql = "SELECT MaNhom, TenNhom, MoTa FROM dbo.NhomSanPham ORDER BY TenNhom ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new NhomSanPham(
                maNhom: reader.GetString(reader.GetOrdinal("MaNhom")),
                tenNhom: reader.GetString(reader.GetOrdinal("TenNhom")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa"))
            ));
        }

        return list;
    }

    public Task LuuNhomAsync(NhomSanPham nhom, CancellationToken cancellationToken = default)
    {
        LuuNhom(nhom);
        return Task.CompletedTask;
    }

    public Task CapNhatNhomAsync(NhomSanPham nhom, CancellationToken cancellationToken = default)
    {
        CapNhatNhom(nhom);
        return Task.CompletedTask;
    }

    public Task<bool> XoaNhomAsync(string maNhom, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(XoaNhom(maNhom));
    }

    public Task<NhomSanPham?> TimTheoIdAsync(string maNhom, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoId(maNhom));
    }

    public Task<List<NhomSanPham>> LayTatCaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTatCa());
    }
}
