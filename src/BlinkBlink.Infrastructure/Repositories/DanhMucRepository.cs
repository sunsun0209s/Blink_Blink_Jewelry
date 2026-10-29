using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể DanhMuc bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ sản phẩm.
/// </summary>
public class DanhMucRepository : IDanhMucRepository
{
    private readonly DatabaseHelper _dbHelper;

    public DanhMucRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public DanhMucRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuDanhMuc(DanhMuc dm)
    {
        if (dm == null)
        {
            throw new ArgumentNullException(nameof(dm), "Danh mục không được để trống.");
        }

        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE MaDanhMuc = @MaDanhMuc)
            BEGIN
                UPDATE dbo.DanhMuc
                SET TenDanhMuc = @TenDanhMuc,
                    MoTa = @MoTa
                WHERE MaDanhMuc = @MaDanhMuc;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.DanhMuc (MaDanhMuc, TenDanhMuc, MoTa)
                VALUES (@MaDanhMuc, @TenDanhMuc, @MoTa);
            END";

        var parameters = new[]
        {
            new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = dm.MaDanhMuc },
            new SqlParameter("@TenDanhMuc", SqlDbType.NVarChar, 150) { Value = dm.TenDanhMuc },
            new SqlParameter("@MoTa", SqlDbType.NVarChar, 500) { Value = (object?)dm.MoTa ?? DBNull.Value }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    public DanhMuc? TimTheoId(string maDanhMuc)
    {
        if (string.IsNullOrWhiteSpace(maDanhMuc))
        {
            return null;
        }

        const string sql = "SELECT MaDanhMuc, TenDanhMuc, MoTa FROM dbo.DanhMuc WHERE MaDanhMuc = @MaDanhMuc;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = maDanhMuc.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new DanhMuc(
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa"))
            );
        }

        return null;
    }

    public List<DanhMuc> LayTatCa()
    {
        var list = new List<DanhMuc>();
        const string sql = "SELECT MaDanhMuc, TenDanhMuc, MoTa FROM dbo.DanhMuc ORDER BY MaDanhMuc ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new DanhMuc(
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa"))
            ));
        }

        return list;
    }

    public Task LuuDanhMucAsync(DanhMuc dm, CancellationToken cancellationToken = default)
    {
        LuuDanhMuc(dm);
        return Task.CompletedTask;
    }

    public Task<DanhMuc?> TimTheoIdAsync(string maDanhMuc, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoId(maDanhMuc));
    }

    public Task<List<DanhMuc>> LayTatCaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTatCa());
    }
}
