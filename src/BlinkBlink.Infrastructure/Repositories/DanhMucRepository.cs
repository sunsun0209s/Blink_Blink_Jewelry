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

    public bool LuuDanhMuc(DanhMuc dm)
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
                    MoTa = @MoTa,
                    IsHidden = @IsHidden
                WHERE MaDanhMuc = @MaDanhMuc;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.DanhMuc (MaDanhMuc, TenDanhMuc, MoTa, IsHidden)
                VALUES (@MaDanhMuc, @TenDanhMuc, @MoTa, @IsHidden);
            END";

        var parameters = new[]
        {
            new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = dm.MaDanhMuc },
            new SqlParameter("@TenDanhMuc", SqlDbType.NVarChar, 150) { Value = dm.TenDanhMuc },
            new SqlParameter("@MoTa", SqlDbType.NVarChar, 500) { Value = (object?)dm.MoTa ?? DBNull.Value },
            new SqlParameter("@IsHidden", SqlDbType.Bit) { Value = dm.IsHidden }
        };

        return _dbHelper.ExecuteNonQuery(sql, parameters) > 0;
    }

    public DanhMuc? TimTheoId(string maDanhMuc)
    {
        if (string.IsNullOrWhiteSpace(maDanhMuc))
        {
            return null;
        }

        const string sql = "SELECT MaDanhMuc, TenDanhMuc, MoTa, ISNULL(IsHidden, 0) AS IsHidden FROM dbo.DanhMuc WHERE MaDanhMuc = @MaDanhMuc;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = maDanhMuc.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new DanhMuc(
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa")),
                isHidden: reader.GetBoolean(reader.GetOrdinal("IsHidden"))
            );
        }

        return null;
    }

    public List<DanhMuc> LayTatCa()
    {
        var list = new List<DanhMuc>();
        const string sql = "SELECT MaDanhMuc, TenDanhMuc, MoTa, ISNULL(IsHidden, 0) AS IsHidden FROM dbo.DanhMuc ORDER BY MaDanhMuc ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new DanhMuc(
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTa")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTa")),
                isHidden: reader.GetBoolean(reader.GetOrdinal("IsHidden"))
            ));
        }

        return list;
    }

    public bool XoaDanhMuc(string maDanhMuc)
    {
        if (string.IsNullOrWhiteSpace(maDanhMuc)) return false;
        var cleanMa = maDanhMuc.Trim();

        // Không cho phép xóa danh mục mặc định 'Chưa phân loại' (DM00)
        if (string.Equals(cleanMa, "DM00", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Không thể xóa danh mục mặc định 'Chưa phân loại' (DM00).");
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            // 1. Đảm bảo danh mục DM00 đã tồn tại để nhận các sản phẩm được chuyển giao
            const string sqlEnsureDm00 = @"
                IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE MaDanhMuc = 'DM00')
                BEGIN
                    INSERT INTO dbo.DanhMuc (MaDanhMuc, TenDanhMuc, MoTa)
                    VALUES ('DM00', N'Chưa phân loại', N'Danh mục mặc định cho sản phẩm chưa phân loại hoặc danh mục cũ bị xóa');
                END";
            using (var cmdEnsure = new SqlCommand(sqlEnsureDm00, conn, tx))
            {
                cmdEnsure.ExecuteNonQuery();
            }

            // 2. Chuyển toàn bộ sản phẩm thuộc danh mục bị xóa sang 'DM00'
            const string sqlUpdateSp = "UPDATE dbo.SanPhamTrangSuc SET MaDanhMuc = 'DM00' WHERE MaDanhMuc = @MaDanhMuc;";
            using (var cmdUpdate = new SqlCommand(sqlUpdateSp, conn, tx))
            {
                cmdUpdate.Parameters.Add(new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = cleanMa });
                cmdUpdate.ExecuteNonQuery();
            }

            // 3. Xóa danh mục khỏi bảng DanhMuc
            int rowsAffected;
            const string sqlDel = "DELETE FROM dbo.DanhMuc WHERE MaDanhMuc = @MaDanhMuc;";
            using (var cmdDel = new SqlCommand(sqlDel, conn, tx))
            {
                cmdDel.Parameters.Add(new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = cleanMa });
                rowsAffected = cmdDel.ExecuteNonQuery();
            }

            if (rowsAffected <= 0)
            {
                tx.Rollback();
                return false;
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public Task<bool> LuuDanhMucAsync(DanhMuc dm, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LuuDanhMuc(dm));
    }

    public Task<DanhMuc?> TimTheoIdAsync(string maDanhMuc, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoId(maDanhMuc));
    }

    public Task<List<DanhMuc>> LayTatCaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTatCa());
    }

    public Task<bool> XoaDanhMucAsync(string maDanhMuc, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(XoaDanhMuc(maDanhMuc));
    }
}
