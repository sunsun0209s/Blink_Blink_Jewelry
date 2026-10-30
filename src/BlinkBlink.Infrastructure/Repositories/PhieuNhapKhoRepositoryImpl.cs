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
/// Triển khai Repository cho thực thể PhieuNhapKho bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ sản phẩm và kho hàng.
/// </summary>
public class PhieuNhapKhoRepositoryImpl : IPhieuNhapKhoRepository
{
    private readonly DatabaseHelper _dbHelper;

    public PhieuNhapKhoRepositoryImpl(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public PhieuNhapKhoRepositoryImpl(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuPhieuNhap(PhieuNhapKho phieu)
    {
        if (phieu == null)
        {
            throw new ArgumentNullException(nameof(phieu), "Phiếu nhập kho không được để trống.");
        }

        using var conn = _dbHelper.GetConnection();
        using var tx = conn.BeginTransaction();

        try
        {
            const string sqlPhieu = @"
                IF EXISTS (SELECT 1 FROM dbo.PhieuNhapKho WHERE MaPhieuNhap = @MaPhieuNhap)
                BEGIN
                    UPDATE dbo.PhieuNhapKho
                    SET NgayNhap = @NgayNhap,
                        NhaCungCap = @NhaCungCap,
                        MaNhanVien = @MaNhanVien,
                        TongSoSanPham = @TongSoSanPham,
                        TongGiaTri = @TongGiaTri
                    WHERE MaPhieuNhap = @MaPhieuNhap;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.PhieuNhapKho (MaPhieuNhap, NgayNhap, NhaCungCap, MaNhanVien, TongSoSanPham, TongGiaTri)
                    VALUES (@MaPhieuNhap, @NgayNhap, @NhaCungCap, @MaNhanVien, @TongSoSanPham, @TongGiaTri);
                END";

            using (var cmdP = new SqlCommand(sqlPhieu, conn, tx))
            {
                cmdP.Parameters.Add(new SqlParameter("@MaPhieuNhap", SqlDbType.VarChar, 50) { Value = phieu.MaPhieuNhap });
                cmdP.Parameters.Add(new SqlParameter("@NgayNhap", SqlDbType.DateTime2) { Value = phieu.NgayNhap });
                cmdP.Parameters.Add(new SqlParameter("@NhaCungCap", SqlDbType.NVarChar, 200) { Value = phieu.NhaCungCap });
                cmdP.Parameters.Add(new SqlParameter("@MaNhanVien", SqlDbType.VarChar, 50) { Value = phieu.MaNhanVien });
                cmdP.Parameters.Add(new SqlParameter("@TongSoSanPham", SqlDbType.Int) { Value = phieu.TongSoSanPham });
                cmdP.Parameters.Add(new SqlParameter("@TongGiaTri", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = phieu.TongGiaTri });
                cmdP.ExecuteNonQuery();
            }

            const string sqlDeleteCt = "DELETE FROM dbo.ChiTietNhapKho WHERE MaPhieuNhap = @MaPhieuNhap;";
            using (var cmdDel = new SqlCommand(sqlDeleteCt, conn, tx))
            {
                cmdDel.Parameters.Add(new SqlParameter("@MaPhieuNhap", SqlDbType.VarChar, 50) { Value = phieu.MaPhieuNhap });
                cmdDel.ExecuteNonQuery();
            }

            if (phieu.ChiTietNhapKhos != null && phieu.ChiTietNhapKhos.Count > 0)
            {
                const string sqlInsertCt = @"
                    INSERT INTO dbo.ChiTietNhapKho (MaPhieuNhap, MaSanPham, SoLuong, GiaVonNhap, SoChungTu)
                    VALUES (@MaPhieuNhap, @MaSanPham, @SoLuong, @GiaVonNhap, @SoChungTu);";

                const string sqlUpdateGiaBinhQuan = @"
                    -- Tính số lượng tồn kho thực tế trước đợt nhập này
                    DECLARE @TonKho INT = ISNULL((
                        SELECT ISNULL(SUM(ct.SoLuong), 0)
                        FROM dbo.ChiTietNhapKho ct
                        WHERE ct.MaSanPham = @MaSanPham
                    ), 0) - ISNULL((
                        SELECT ISNULL(SUM(chd.SoLuong), 0)
                        FROM dbo.ChiTietHoaDon chd
                        INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon
                        WHERE chd.MaSanPham = @MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'
                    ), 0);

                    IF (@TonKho < 0) SET @TonKho = 0;

                    DECLARE @GiaHienTai DECIMAL(18,2) = ISNULL((SELECT GiaNhap FROM dbo.SanPhamTrangSuc WHERE MaSanPham = @MaSanPham), 0);
                    DECLARE @GiaMoi DECIMAL(18,2);

                    -- Công thức bình quân: (Số lượng tồn * Giá nhập hiện tại + Số lượng nhập mới * Giá nhập mới) / (Số lượng tồn + Số lượng nhập mới)
                    IF (@TonKho > 0 AND (@TonKho + @SoLuong) > 0)
                    BEGIN
                        SET @GiaMoi = ((CAST(@TonKho AS DECIMAL(18,2)) * @GiaHienTai) + (CAST(@SoLuong AS DECIMAL(18,2)) * @GiaVonNhap)) / CAST((@TonKho + @SoLuong) AS DECIMAL(18,2));
                    END
                    ELSE
                    BEGIN
                        SET @GiaMoi = @GiaVonNhap;
                    END

                    UPDATE dbo.SanPhamTrangSuc
                    SET TrangThai = 'SanSang',
                        GiaNhap = @GiaMoi
                    WHERE MaSanPham = @MaSanPham;";

                foreach (var ct in phieu.ChiTietNhapKhos)
                {
                    // 1. Cập nhật trạng thái và giá nhập bình quân gia quyền vào sản phẩm trước khi thêm dòng chi tiết
                    if (ct.SoLuong > 0)
                    {
                        using var cmdSp = new SqlCommand(sqlUpdateGiaBinhQuan, conn, tx);
                        cmdSp.Parameters.Add(new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = ct.MaSanPham });
                        cmdSp.Parameters.Add(new SqlParameter("@SoLuong", SqlDbType.Int) { Value = ct.SoLuong });
                        cmdSp.Parameters.Add(new SqlParameter("@GiaVonNhap", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = ct.GiaVonNhap });
                        cmdSp.ExecuteNonQuery();
                    }

                    // 2. Lưu dòng chi tiết phiếu nhập kho
                    using var cmdCt = new SqlCommand(sqlInsertCt, conn, tx);
                    cmdCt.Parameters.Add(new SqlParameter("@MaPhieuNhap", SqlDbType.VarChar, 50) { Value = phieu.MaPhieuNhap });
                    cmdCt.Parameters.Add(new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = ct.MaSanPham });
                    cmdCt.Parameters.Add(new SqlParameter("@SoLuong", SqlDbType.Int) { Value = ct.SoLuong });
                    cmdCt.Parameters.Add(new SqlParameter("@GiaVonNhap", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = ct.GiaVonNhap });
                    cmdCt.Parameters.Add(new SqlParameter("@SoChungTu", SqlDbType.NVarChar, 100) { Value = (object?)ct.SoChungTu ?? DBNull.Value });
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

    public List<PhieuNhapKho> LayTatCa()
    {
        var list = new List<PhieuNhapKho>();
        const string sql = @"
            SELECT MaPhieuNhap, NgayNhap, NhaCungCap, MaNhanVien, TongSoSanPham, TongGiaTri
            FROM dbo.PhieuNhapKho
            ORDER BY NgayNhap DESC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var p = new PhieuNhapKho(
                maPhieuNhap: reader.GetString(reader.GetOrdinal("MaPhieuNhap")),
                nhaCungCap: reader.GetString(reader.GetOrdinal("NhaCungCap")),
                maNhanVien: reader.GetString(reader.GetOrdinal("MaNhanVien")),
                ngayNhap: reader.GetDateTime(reader.GetOrdinal("NgayNhap"))
            )
            {
                TongSoSanPham = reader.GetInt32(reader.GetOrdinal("TongSoSanPham")),
                TongGiaTri = reader.GetDecimal(reader.GetOrdinal("TongGiaTri"))
            };
            list.Add(p);
        }

        foreach (var p in list)
        {
            p.ChiTietNhapKhos = LayChiTietNhapKho(p.MaPhieuNhap);
        }

        return list;
    }

    public Task LuuPhieuNhapAsync(PhieuNhapKho phieu, CancellationToken cancellationToken = default)
    {
        LuuPhieuNhap(phieu);
        return Task.CompletedTask;
    }

    public Task<List<PhieuNhapKho>> LayTatCaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTatCa());
    }

    public Task<PhieuNhapKho?> TimTheoIdAsync(string maPhieu, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maPhieu))
        {
            return Task.FromResult<PhieuNhapKho?>(null);
        }

        const string sql = @"
            SELECT MaPhieuNhap, NgayNhap, NhaCungCap, MaNhanVien, TongSoSanPham, TongGiaTri
            FROM dbo.PhieuNhapKho
            WHERE MaPhieuNhap = @MaPhieuNhap;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaPhieuNhap", SqlDbType.VarChar, 50) { Value = maPhieu.Trim() });

        PhieuNhapKho? phieu = null;
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                phieu = new PhieuNhapKho(
                    maPhieuNhap: reader.GetString(reader.GetOrdinal("MaPhieuNhap")),
                    nhaCungCap: reader.GetString(reader.GetOrdinal("NhaCungCap")),
                    maNhanVien: reader.GetString(reader.GetOrdinal("MaNhanVien")),
                    ngayNhap: reader.GetDateTime(reader.GetOrdinal("NgayNhap"))
                )
                {
                    TongSoSanPham = reader.GetInt32(reader.GetOrdinal("TongSoSanPham")),
                    TongGiaTri = reader.GetDecimal(reader.GetOrdinal("TongGiaTri"))
                };
            }
        }

        if (phieu != null)
        {
            phieu.ChiTietNhapKhos = LayChiTietNhapKho(phieu.MaPhieuNhap);
        }

        return Task.FromResult(phieu);
    }

    private List<ChiTietNhapKho> LayChiTietNhapKho(string maPhieuNhap)
    {
        var list = new List<ChiTietNhapKho>();
        const string sql = @"
            SELECT c.MaPhieuNhap, c.MaSanPham, c.SoLuong, c.GiaVonNhap, c.SoChungTu,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai
            FROM dbo.ChiTietNhapKho c
            LEFT JOIN dbo.SanPhamTrangSuc s ON c.MaSanPham = s.MaSanPham
            WHERE c.MaPhieuNhap = @MaPhieuNhap;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaPhieuNhap", SqlDbType.VarChar, 50) { Value = maPhieuNhap });

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var ct = new ChiTietNhapKho(
                maPhieuNhap: reader.GetString(reader.GetOrdinal("MaPhieuNhap")),
                maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
                soLuong: reader.GetInt32(reader.GetOrdinal("SoLuong")),
                giaVonNhap: reader.GetDecimal(reader.GetOrdinal("GiaVonNhap")),
                soChungTu: reader.IsDBNull(reader.GetOrdinal("SoChungTu")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoChungTu"))
            );

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
