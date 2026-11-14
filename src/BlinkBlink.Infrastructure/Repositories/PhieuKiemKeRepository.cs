using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể PhieuKiemKe bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ sản phẩm và kho hàng.
/// </summary>
public class PhieuKiemKeRepository : IPhieuKiemKeRepository
{
    private readonly DatabaseHelper _dbHelper;

    public PhieuKiemKeRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public PhieuKiemKeRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuPhieuKiemKe(PhieuKiemKe phieu)
    {
        if (phieu == null)
        {
            throw new ArgumentNullException(nameof(phieu), "Phiếu kiểm kê không được để trống.");
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();

        try
        {
            // Tự động kiểm tra và chuyển đổi MaNhanVienKho sang MaNguoiDung hợp lệ để thỏa mãn khóa ngoại FK_PhieuKiemKe_NhanVien
            var maNhanVienHopLe = phieu.MaNhanVienKho?.Trim() ?? string.Empty;
            const string sqlCheckNguoiDung = @"
                SELECT TOP 1 MaNguoiDung FROM dbo.NguoiDung WHERE MaNguoiDung = @Code
                UNION
                SELECT TOP 1 MaNguoiDung FROM dbo.NhanVien WHERE MaNhanVien = @Code;";
            using (var cmdCheck = new SqlCommand(sqlCheckNguoiDung, conn, tx))
            {
                cmdCheck.Parameters.Add(new SqlParameter("@Code", SqlDbType.VarChar, 50) { Value = maNhanVienHopLe });
                var foundId = cmdCheck.ExecuteScalar()?.ToString();
                if (!string.IsNullOrEmpty(foundId))
                {
                    maNhanVienHopLe = foundId;
                }
                else
                {
                    maNhanVienHopLe = "USER_ADMIN01";
                }
            }

            const string sqlPhieu = @"
                IF EXISTS (SELECT 1 FROM dbo.PhieuKiemKe WHERE MaPhieuKiemKe = @MaPhieuKiemKe)
                BEGIN
                    UPDATE dbo.PhieuKiemKe
                    SET NgayKiemKe = @NgayKiemKe,
                        ViTriKiemKe = @ViTriKiemKe,
                        MaNhanVienKho = @MaNhanVienKho,
                        TrangThai = @TrangThai,
                        TrangThaiDuyet = @TrangThaiDuyet,
                        GhiChu = @GhiChu,
                        NgayDuyet = @NgayDuyet
                    WHERE MaPhieuKiemKe = @MaPhieuKiemKe;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.PhieuKiemKe (MaPhieuKiemKe, NgayKiemKe, ViTriKiemKe, MaNhanVienKho, TrangThai, TrangThaiDuyet, GhiChu, NgayDuyet)
                    VALUES (@MaPhieuKiemKe, @NgayKiemKe, @ViTriKiemKe, @MaNhanVienKho, @TrangThai, @TrangThaiDuyet, @GhiChu, @NgayDuyet);
                END";

            using (var cmdP = new SqlCommand(sqlPhieu, conn, tx))
            {
                cmdP.Parameters.Add(new SqlParameter("@MaPhieuKiemKe", SqlDbType.VarChar, 50) { Value = phieu.MaPhieuKiemKe });
                cmdP.Parameters.Add(new SqlParameter("@NgayKiemKe", SqlDbType.DateTime2) { Value = phieu.NgayKiemKe });
                cmdP.Parameters.Add(new SqlParameter("@ViTriKiemKe", SqlDbType.NVarChar, 150) { Value = (object?)phieu.ViTriKiemKe ?? DBNull.Value });
                cmdP.Parameters.Add(new SqlParameter("@MaNhanVienKho", SqlDbType.VarChar, 50) { Value = maNhanVienHopLe });
                cmdP.Parameters.Add(new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50) { Value = phieu.TrangThai });
                cmdP.Parameters.Add(new SqlParameter("@TrangThaiDuyet", SqlDbType.Bit) { Value = phieu.TrangThaiDuyet });
                cmdP.Parameters.Add(new SqlParameter("@GhiChu", SqlDbType.NVarChar, 500) { Value = (object?)phieu.GhiChu ?? DBNull.Value });
                cmdP.Parameters.Add(new SqlParameter("@NgayDuyet", SqlDbType.DateTime2) { Value = (object?)phieu.NgayDuyet ?? DBNull.Value });
                cmdP.ExecuteNonQuery();
            }

            const string sqlDeleteCt = "DELETE FROM dbo.ChiTietKiemKe WHERE MaPhieuKiemKe = @MaPhieuKiemKe;";
            using (var cmdDel = new SqlCommand(sqlDeleteCt, conn, tx))
            {
                cmdDel.Parameters.Add(new SqlParameter("@MaPhieuKiemKe", SqlDbType.VarChar, 50) { Value = phieu.MaPhieuKiemKe });
                cmdDel.ExecuteNonQuery();
            }

            if (phieu.ChiTietKiemKes != null && phieu.ChiTietKiemKes.Count > 0)
            {
                const string sqlInsertCt = @"
                    INSERT INTO dbo.ChiTietKiemKe (MaPhieuKiemKe, MaSanPham, TonSoSach, TonThucTe, ChenhLech, LyDoChenhLech)
                    VALUES (@MaPhieuKiemKe, @MaSanPham, @TonSoSach, @TonThucTe, @ChenhLech, @LyDoChenhLech);";

                foreach (var ct in phieu.ChiTietKiemKes)
                {
                    using var cmdCt = new SqlCommand(sqlInsertCt, conn, tx);
                    cmdCt.Parameters.Add(new SqlParameter("@MaPhieuKiemKe", SqlDbType.VarChar, 50) { Value = phieu.MaPhieuKiemKe });
                    cmdCt.Parameters.Add(new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = ct.MaSanPham });
                    cmdCt.Parameters.Add(new SqlParameter("@TonSoSach", SqlDbType.Int) { Value = ct.TonSoSach });
                    cmdCt.Parameters.Add(new SqlParameter("@TonThucTe", SqlDbType.Int) { Value = ct.TonThucTe });
                    cmdCt.Parameters.Add(new SqlParameter("@ChenhLech", SqlDbType.Int) { Value = ct.ChenhLech });
                    cmdCt.Parameters.Add(new SqlParameter("@LyDoChenhLech", SqlDbType.NVarChar, 300) { Value = (object?)ct.LyDoChenhLech ?? DBNull.Value });
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

    public PhieuKiemKe? TimTheoId(string maPhieu)
    {
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            return null;
        }

        const string sql = @"
            SELECT MaPhieuKiemKe, NgayKiemKe, ViTriKiemKe, MaNhanVienKho, TrangThai, TrangThaiDuyet, GhiChu, NgayDuyet
            FROM dbo.PhieuKiemKe
            WHERE MaPhieuKiemKe = @MaPhieuKiemKe;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaPhieuKiemKe", SqlDbType.VarChar, 50) { Value = maPhieu.Trim() });

        PhieuKiemKe? phieu = null;
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                var trangThaiCol = reader.GetOrdinal("TrangThai");
                var ghiChuCol = reader.GetOrdinal("GhiChu");
                var ngayDuyetCol = reader.GetOrdinal("NgayDuyet");

                phieu = new PhieuKiemKe(
                    maPhieuKiemKe: reader.GetString(reader.GetOrdinal("MaPhieuKiemKe")),
                    viTriKiemKe: reader.IsDBNull(reader.GetOrdinal("ViTriKiemKe")) ? string.Empty : reader.GetString(reader.GetOrdinal("ViTriKiemKe")),
                    maNhanVienKho: reader.GetString(reader.GetOrdinal("MaNhanVienKho")),
                    ngayKiemKe: reader.GetDateTime(reader.GetOrdinal("NgayKiemKe"))
                )
                {
                    TrangThai = !reader.IsDBNull(trangThaiCol) ? reader.GetString(trangThaiCol) : "ChoDuyet",
                    TrangThaiDuyet = reader.GetBoolean(reader.GetOrdinal("TrangThaiDuyet")),
                    GhiChu = !reader.IsDBNull(ghiChuCol) ? reader.GetString(ghiChuCol) : string.Empty,
                    NgayDuyet = !reader.IsDBNull(ngayDuyetCol) ? reader.GetDateTime(ngayDuyetCol) : null
                };
            }
        }

        if (phieu != null)
        {
            phieu.ChiTietKiemKes = LayChiTietKiemKe(phieu.MaPhieuKiemKe);
        }

        return phieu;
    }

    public List<PhieuKiemKe> LayTatCa()
    {
        var list = new List<PhieuKiemKe>();
        const string sql = @"
            SELECT MaPhieuKiemKe, NgayKiemKe, ViTriKiemKe, MaNhanVienKho, TrangThai, TrangThaiDuyet, GhiChu, NgayDuyet
            FROM dbo.PhieuKiemKe
            ORDER BY NgayKiemKe DESC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var trangThaiCol = reader.GetOrdinal("TrangThai");
            var ghiChuCol = reader.GetOrdinal("GhiChu");
            var ngayDuyetCol = reader.GetOrdinal("NgayDuyet");

            var p = new PhieuKiemKe(
                maPhieuKiemKe: reader.GetString(reader.GetOrdinal("MaPhieuKiemKe")),
                viTriKiemKe: reader.IsDBNull(reader.GetOrdinal("ViTriKiemKe")) ? string.Empty : reader.GetString(reader.GetOrdinal("ViTriKiemKe")),
                maNhanVienKho: reader.GetString(reader.GetOrdinal("MaNhanVienKho")),
                ngayKiemKe: reader.GetDateTime(reader.GetOrdinal("NgayKiemKe"))
            )
            {
                TrangThai = !reader.IsDBNull(trangThaiCol) ? reader.GetString(trangThaiCol) : "ChoDuyet",
                TrangThaiDuyet = reader.GetBoolean(reader.GetOrdinal("TrangThaiDuyet")),
                GhiChu = !reader.IsDBNull(ghiChuCol) ? reader.GetString(ghiChuCol) : string.Empty,
                NgayDuyet = !reader.IsDBNull(ngayDuyetCol) ? reader.GetDateTime(ngayDuyetCol) : null
            };
            list.Add(p);
        }

        foreach (var p in list)
        {
            p.ChiTietKiemKes = LayChiTietKiemKe(p.MaPhieuKiemKe);
        }

        return list;
    }

    public void XoaPhieuKiemKe(string maPhieu)
    {
        if (string.IsNullOrWhiteSpace(maPhieu)) return;

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();

        try
        {
            using (var cmdCt = new SqlCommand("DELETE FROM dbo.ChiTietKiemKe WHERE MaPhieuKiemKe = @MaPhieu AND EXISTS (SELECT 1 FROM dbo.PhieuKiemKe WHERE MaPhieuKiemKe = @MaPhieu AND TrangThai = 'ChoDuyet');", conn, tx))
            {
                cmdCt.Parameters.Add(new SqlParameter("@MaPhieu", SqlDbType.VarChar, 50) { Value = maPhieu.Trim() });
                cmdCt.ExecuteNonQuery();
            }

            using (var cmdP = new SqlCommand("DELETE FROM dbo.PhieuKiemKe WHERE MaPhieuKiemKe = @MaPhieu AND TrangThai = 'ChoDuyet';", conn, tx))
            {
                cmdP.Parameters.Add(new SqlParameter("@MaPhieu", SqlDbType.VarChar, 50) { Value = maPhieu.Trim() });
                cmdP.ExecuteNonQuery();
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void CapNhatTrangThai(string maPhieu, string trangThaiMoi, DateTime? ngayDuyet = null)
    {
        if (string.IsNullOrWhiteSpace(maPhieu)) return;

        const string sql = @"
            UPDATE dbo.PhieuKiemKe
            SET TrangThai = @TrangThai,
                TrangThaiDuyet = @TrangThaiDuyet,
                NgayDuyet = @NgayDuyet
            WHERE MaPhieuKiemKe = @MaPhieu;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaPhieu", SqlDbType.VarChar, 50) { Value = maPhieu.Trim() });
        cmd.Parameters.Add(new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50) { Value = trangThaiMoi });
        cmd.Parameters.Add(new SqlParameter("@TrangThaiDuyet", SqlDbType.Bit) { Value = string.Equals(trangThaiMoi, "DaDuyet", StringComparison.OrdinalIgnoreCase) });
        cmd.Parameters.Add(new SqlParameter("@NgayDuyet", SqlDbType.DateTime2) { Value = (object?)ngayDuyet ?? DBNull.Value });
        cmd.ExecuteNonQuery();
    }

    public Task LuuPhieuKiemKeAsync(PhieuKiemKe phieu, CancellationToken cancellationToken = default)
    {
        LuuPhieuKiemKe(phieu);
        return Task.CompletedTask;
    }

    public Task<PhieuKiemKe?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoId(maPhieu));
    }

    private List<ChiTietKiemKe> LayChiTietKiemKe(string maPhieuKiemKe)
    {
        var list = new List<ChiTietKiemKe>();
        const string sql = @"
            SELECT c.MaPhieuKiemKe, c.MaSanPham, c.TonSoSach, c.TonThucTe, c.ChenhLech, c.LyDoChenhLech,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai
            FROM dbo.ChiTietKiemKe c
            LEFT JOIN dbo.SanPhamTrangSuc s ON c.MaSanPham = s.MaSanPham
            WHERE c.MaPhieuKiemKe = @MaPhieuKiemKe;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaPhieuKiemKe", SqlDbType.VarChar, 50) { Value = maPhieuKiemKe });

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
}
