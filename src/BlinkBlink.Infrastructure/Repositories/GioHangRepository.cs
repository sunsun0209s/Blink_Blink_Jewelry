using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Cart;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể GioHang bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bán hàng.
/// </summary>
public class GioHangRepository : IGioHangRepository
{
    private readonly DatabaseHelper _dbHelper;

    public GioHangRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public GioHangRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public GioHang? LayGioHang(string maKH)
    {
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return null;
        }

        const string sql = @"
            SELECT g.MaGioHang, g.MaKhachHang, g.SoLuong
            FROM dbo.GioHang g
            LEFT JOIN dbo.KhachHang kh ON g.MaKhachHang = kh.MaNguoiDung
            WHERE g.MaKhachHang = @MaKhachHang OR kh.MaKhachHang = @MaKhachHang;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = maKH.Trim() });

        GioHang? gh = null;
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                gh = new GioHang(
                    maGioHang: reader.GetString(reader.GetOrdinal("MaGioHang")),
                    maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang"))
                )
                {
                    SoLuong = reader.GetInt32(reader.GetOrdinal("SoLuong"))
                };
            }
        }

        if (gh != null)
        {
            gh.DanhSachSanPham = LayChiTietGioHang(gh.MaGioHang);
        }

        return gh;
    }

    public void CapNhatGioHang(GioHang gh)
    {
        if (gh == null)
        {
            throw new ArgumentNullException(nameof(gh), "Giỏ hàng không được để trống.");
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();

        try
        {
            const string sqlGioHang = @"
                DECLARE @ActualMaGioHang VARCHAR(50);
                SELECT TOP 1 @ActualMaGioHang = MaGioHang FROM dbo.GioHang WHERE MaGioHang = @MaGioHang OR MaKhachHang = @MaKhachHang;

                IF @ActualMaGioHang IS NOT NULL
                BEGIN
                    UPDATE dbo.GioHang
                    SET SoLuong = @SoLuong
                    WHERE MaGioHang = @ActualMaGioHang;
                    SELECT @ActualMaGioHang;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.GioHang (MaGioHang, MaKhachHang, SoLuong)
                    VALUES (@MaGioHang, @MaKhachHang, @SoLuong);
                    SELECT @MaGioHang;
                END";

            string resolvedMaGioHang = gh.MaGioHang;
            using (var cmdGh = new SqlCommand(sqlGioHang, conn, tx))
            {
                cmdGh.Parameters.Add(new SqlParameter("@MaGioHang", SqlDbType.VarChar, 50) { Value = gh.MaGioHang });
                cmdGh.Parameters.Add(new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = gh.MaKhachHang });
                cmdGh.Parameters.Add(new SqlParameter("@SoLuong", SqlDbType.Int) { Value = gh.SoLuong });
                var res = cmdGh.ExecuteScalar();
                if (res != null && res != DBNull.Value)
                {
                    resolvedMaGioHang = res.ToString()!;
                }
            }

            const string sqlDeleteCt = "DELETE FROM dbo.ChiTietGioHang WHERE MaGioHang = @MaGioHang;";
            using (var cmdDel = new SqlCommand(sqlDeleteCt, conn, tx))
            {
                cmdDel.Parameters.Add(new SqlParameter("@MaGioHang", SqlDbType.VarChar, 50) { Value = resolvedMaGioHang });
                cmdDel.ExecuteNonQuery();
            }

            if (gh.DanhSachSanPham != null && gh.DanhSachSanPham.Count > 0)
            {
                const string sqlInsertCt = @"
                    INSERT INTO dbo.ChiTietGioHang (MaGioHang, MaSanPham, SoLuongSanPham)
                    VALUES (@MaGioHang, @MaSanPham, @SoLuongSanPham);";

                foreach (var ct in gh.DanhSachSanPham)
                {
                    using var cmdCt = new SqlCommand(sqlInsertCt, conn, tx);
                    cmdCt.Parameters.Add(new SqlParameter("@MaGioHang", SqlDbType.VarChar, 50) { Value = resolvedMaGioHang });
                    cmdCt.Parameters.Add(new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = ct.MaSanPham });
                    cmdCt.Parameters.Add(new SqlParameter("@SoLuongSanPham", SqlDbType.Int) { Value = ct.SoLuongSanPham });
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

    public Task<GioHang?> LayGioHangAsync(string maKH, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayGioHang(maKH));
    }

    public Task CapNhatGioHangAsync(GioHang gh, CancellationToken cancellationToken = default)
    {
        CapNhatGioHang(gh);
        return Task.CompletedTask;
    }

    public Task LuuGioHangAsync(GioHang gh, CancellationToken cancellationToken = default)
    {
        CapNhatGioHang(gh);
        return Task.CompletedTask;
    }

    private List<ChiTietGioHang> LayChiTietGioHang(string maGioHang)
    {
        var list = new List<ChiTietGioHang>();
        const string sql = @"
            SELECT c.MaGioHang, c.MaSanPham, c.SoLuongSanPham,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai
            FROM dbo.ChiTietGioHang c
            LEFT JOIN dbo.SanPhamTrangSuc s ON c.MaSanPham = s.MaSanPham
            WHERE c.MaGioHang = @MaGioHang;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaGioHang", SqlDbType.VarChar, 50) { Value = maGioHang });

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var ct = new ChiTietGioHang(
                maGioHang: reader.GetString(reader.GetOrdinal("MaGioHang")),
                maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
                soLuongSanPham: reader.GetInt32(reader.GetOrdinal("SoLuongSanPham"))
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
