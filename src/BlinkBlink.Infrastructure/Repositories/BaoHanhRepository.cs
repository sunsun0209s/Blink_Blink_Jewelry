using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể PhieuBaoHanh bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ bảo hành.
/// </summary>
public class BaoHanhRepository : IBaoHanhRepository
{
    private readonly DatabaseHelper _dbHelper;

    public BaoHanhRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public BaoHanhRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuPhieu(PhieuBaoHanh pbh)
    {
        if (pbh == null)
        {
            throw new ArgumentNullException(nameof(pbh), "Phiếu bảo hành không được để trống.");
        }

        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.PhieuBaoHanh WHERE SoPhieuBH = @SoPhieuBH)
            BEGIN
                UPDATE dbo.PhieuBaoHanh
                SET MaKhachHang = @MaKhachHang,
                    MaSanPham = @MaSanPham,
                    LyDoBaoHanh = @LyDoBaoHanh,
                    ChiPhiUocTinh = @ChiPhiUocTinh,
                    ChiPhiThucTe = @ChiPhiThucTe,
                    ChiPhiBaoHanh = @ChiPhiBaoHanh,
                    TrangThaiXuLy = @TrangThaiXuLy,
                    NgayTiepNhan = @NgayTiepNhan,
                    NgayHenTra = @NgayHenTra
                WHERE SoPhieuBH = @SoPhieuBH;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.PhieuBaoHanh (SoPhieuBH, MaKhachHang, MaSanPham, LyDoBaoHanh, ChiPhiUocTinh, ChiPhiThucTe, ChiPhiBaoHanh, TrangThaiXuLy, NgayTiepNhan, NgayHenTra)
                VALUES (@SoPhieuBH, @MaKhachHang, @MaSanPham, @LyDoBaoHanh, @ChiPhiUocTinh, @ChiPhiThucTe, @ChiPhiBaoHanh, @TrangThaiXuLy, @NgayTiepNhan, @NgayHenTra);
            END";

        var parameters = new[]
        {
            new SqlParameter("@SoPhieuBH", SqlDbType.VarChar, 50) { Value = pbh.SoPhieuBH },
            new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = pbh.MaKhachHang },
            new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = pbh.MaSanPham },
            new SqlParameter("@LyDoBaoHanh", SqlDbType.NVarChar, 500) { Value = pbh.LyDoBaoHanh },
            new SqlParameter("@ChiPhiUocTinh", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = pbh.ChiPhiUocTinh },
            new SqlParameter("@ChiPhiThucTe", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = pbh.ChiPhiThucTe },
            new SqlParameter("@ChiPhiBaoHanh", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = pbh.ChiPhiBaoHanh },
            new SqlParameter("@TrangThaiXuLy", SqlDbType.NVarChar, 50) { Value = pbh.TrangThaiXuLy },
            new SqlParameter("@NgayTiepNhan", SqlDbType.DateTime2) { Value = pbh.NgayTiepNhan },
            new SqlParameter("@NgayHenTra", SqlDbType.DateTime2) { Value = (object?)pbh.NgayHenTra ?? DBNull.Value }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    public PhieuBaoHanh? TimTheoSoPhieu(string soPhieu)
    {
        if (string.IsNullOrWhiteSpace(soPhieu))
        {
            return null;
        }

        const string sql = @"
            SELECT b.SoPhieuBH, b.MaKhachHang, b.MaSanPham, b.LyDoBaoHanh, b.ChiPhiBaoHanh, b.TrangThaiXuLy, b.NgayTiepNhan, b.NgayHenTra,
                   u.HoTen AS TenKhachHang, u.SoDienThoai AS SdtKhachHang, kh.MaKhachHang AS MaDinhDanhKhachHang, kh.NgaySinh AS NgaySinhKhachHang,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai AS TrangThaiSanPham
            FROM dbo.PhieuBaoHanh b
            LEFT JOIN dbo.NguoiDung u ON b.MaKhachHang = u.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON u.MaNguoiDung = kh.MaNguoiDung
            LEFT JOIN dbo.SanPhamTrangSuc s ON b.MaSanPham = s.MaSanPham
            WHERE b.SoPhieuBH = @SoPhieuBH;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SoPhieuBH", SqlDbType.VarChar, 50) { Value = soPhieu.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapReaderToPhieuBaoHanh(reader);
        }

        return null;
    }

    public List<PhieuBaoHanh> LayTheoKhachHang(string maKH)
    {
        var list = new List<PhieuBaoHanh>();
        if (string.IsNullOrWhiteSpace(maKH))
        {
            return list;
        }

        const string sql = @"
            SELECT b.SoPhieuBH, b.MaKhachHang, b.MaSanPham, b.LyDoBaoHanh, b.ChiPhiBaoHanh, b.TrangThaiXuLy, b.NgayTiepNhan, b.NgayHenTra,
                   u.HoTen AS TenKhachHang, u.SoDienThoai AS SdtKhachHang, kh.MaKhachHang AS MaDinhDanhKhachHang, kh.NgaySinh AS NgaySinhKhachHang,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai AS TrangThaiSanPham
            FROM dbo.PhieuBaoHanh b
            LEFT JOIN dbo.NguoiDung u ON b.MaKhachHang = u.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON u.MaNguoiDung = kh.MaNguoiDung
            LEFT JOIN dbo.SanPhamTrangSuc s ON b.MaSanPham = s.MaSanPham
            WHERE b.MaKhachHang = @MaKhachHang OR kh.MaKhachHang = @MaKhachHang
            ORDER BY b.NgayTiepNhan DESC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaKhachHang", SqlDbType.VarChar, 50) { Value = maKH.Trim() });

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToPhieuBaoHanh(reader));
        }

        return list;
    }

    public List<PhieuBaoHanh> LayDanhSachPhieu()
    {
        var list = new List<PhieuBaoHanh>();

        const string sql = @"
            SELECT b.SoPhieuBH, b.MaKhachHang, b.MaSanPham, b.LyDoBaoHanh, b.ChiPhiBaoHanh, b.TrangThaiXuLy, b.NgayTiepNhan, b.NgayHenTra,
                   u.HoTen AS TenKhachHang, u.SoDienThoai AS SdtKhachHang, kh.MaKhachHang AS MaDinhDanhKhachHang, kh.NgaySinh AS NgaySinhKhachHang,
                   s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai AS TrangThaiSanPham
            FROM dbo.PhieuBaoHanh b
            LEFT JOIN dbo.NguoiDung u ON b.MaKhachHang = u.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON u.MaNguoiDung = kh.MaNguoiDung
            LEFT JOIN dbo.SanPhamTrangSuc s ON b.MaSanPham = s.MaSanPham
            ORDER BY b.NgayTiepNhan DESC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToPhieuBaoHanh(reader));
        }

        return list;
    }

    public void CapNhatPhieu(PhieuBaoHanh pbh)
    {
        if (pbh == null)
        {
            return;
        }

        const string sql = @"
            UPDATE dbo.PhieuBaoHanh
            SET ChiPhiUocTinh = @ChiPhiUocTinh,
                ChiPhiThucTe = @ChiPhiThucTe,
                ChiPhiBaoHanh = @ChiPhiBaoHanh,
                TrangThaiXuLy = @TrangThaiXuLy,
                NgayHenTra = @NgayHenTra
            WHERE SoPhieuBH = @SoPhieuBH;";

        var parameters = new[]
        {
            new SqlParameter("@SoPhieuBH", SqlDbType.VarChar, 50) { Value = pbh.SoPhieuBH },
            new SqlParameter("@ChiPhiUocTinh", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = pbh.ChiPhiUocTinh },
            new SqlParameter("@ChiPhiThucTe", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = pbh.ChiPhiThucTe },
            new SqlParameter("@ChiPhiBaoHanh", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = pbh.ChiPhiBaoHanh },
            new SqlParameter("@TrangThaiXuLy", SqlDbType.NVarChar, 50) { Value = pbh.TrangThaiXuLy },
            new SqlParameter("@NgayHenTra", SqlDbType.DateTime2) { Value = (object?)pbh.NgayHenTra ?? DBNull.Value }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    public Task LuuPhieuAsync(PhieuBaoHanh pbh, CancellationToken cancellationToken = default)
    {
        LuuPhieu(pbh);
        return Task.CompletedTask;
    }

    public Task<PhieuBaoHanh?> TimTheoSoPhieuAsync(string soPhieu, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoSoPhieu(soPhieu));
    }

    public Task<List<PhieuBaoHanh>> LayTheoKhachHangAsync(string maKH, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTheoKhachHang(maKH));
    }

    public Task<List<PhieuBaoHanh>> LayDanhSachPhieuAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayDanhSachPhieu());
    }

    public Task CapNhatPhieuAsync(PhieuBaoHanh pbh, CancellationToken cancellationToken = default)
    {
        CapNhatPhieu(pbh);
        return Task.CompletedTask;
    }

    private static PhieuBaoHanh MapReaderToPhieuBaoHanh(SqlDataReader reader)
    {
        decimal chiPhiUocTinh = 0m;
        decimal chiPhiThucTe = 0m;

        try
        {
            var ordUocTinh = reader.GetOrdinal("ChiPhiUocTinh");
            if (!reader.IsDBNull(ordUocTinh)) chiPhiUocTinh = reader.GetDecimal(ordUocTinh);
        }
        catch { }

        try
        {
            var ordThucTe = reader.GetOrdinal("ChiPhiThucTe");
            if (!reader.IsDBNull(ordThucTe)) chiPhiThucTe = reader.GetDecimal(ordThucTe);
        }
        catch { }

        decimal chiPhiBaoHanh = 0m;
        try
        {
            chiPhiBaoHanh = reader.GetDecimal(reader.GetOrdinal("ChiPhiBaoHanh"));
        }
        catch { }

        if (chiPhiUocTinh == 0 && chiPhiBaoHanh > 0) chiPhiUocTinh = chiPhiBaoHanh;

        var pbh = new PhieuBaoHanh(
            soPhieuBH: reader.GetString(reader.GetOrdinal("SoPhieuBH")),
            maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang")),
            maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
            lyDoBaoHanh: reader.GetString(reader.GetOrdinal("LyDoBaoHanh")),
            chiPhiUocTinh: chiPhiUocTinh,
            trangThaiXuLy: reader.GetString(reader.GetOrdinal("TrangThaiXuLy")),
            ngayTiepNhan: reader.GetDateTime(reader.GetOrdinal("NgayTiepNhan")),
            ngayHenTra: reader.IsDBNull(reader.GetOrdinal("NgayHenTra")) ? null : reader.GetDateTime(reader.GetOrdinal("NgayHenTra")),
            chiPhiThucTe: chiPhiThucTe
        );

        if (!reader.IsDBNull(reader.GetOrdinal("TenSanPham")))
        {
            pbh.SanPham = new SanPhamTrangSuc(
                maSanPham: pbh.MaSanPham,
                tenSanPham: reader.GetString(reader.GetOrdinal("TenSanPham")),
                maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
                chatLieu: reader.GetString(reader.GetOrdinal("ChatLieu")),
                size: reader.IsDBNull(reader.GetOrdinal("Size")) ? string.Empty : reader.GetString(reader.GetOrdinal("Size")),
                giaNhap: reader.GetDecimal(reader.GetOrdinal("GiaNhap")),
                giaBan: reader.GetDecimal(reader.GetOrdinal("GiaBan")),
                trangThai: reader.GetString(reader.GetOrdinal("TrangThaiSanPham"))
            );
        }

        return pbh;
    }
}
