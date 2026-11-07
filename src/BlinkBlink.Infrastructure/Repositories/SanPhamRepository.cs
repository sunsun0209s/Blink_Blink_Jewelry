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
/// Triển khai Repository cho thực thể SanPhamTrangSuc bằng ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật và XML sơ đồ phân hệ sản phẩm.
/// </summary>
public class SanPhamRepository : ISanPhamRepository
{
    private readonly DatabaseHelper _dbHelper;

    public SanPhamRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public SanPhamRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public void LuuSanPham(SanPhamTrangSuc sp)
    {
        if (sp == null)
        {
            throw new ArgumentNullException(nameof(sp), "Sản phẩm không được để trống.");
        }

        const string sql = @"
            IF EXISTS (SELECT 1 FROM dbo.SanPhamTrangSuc WHERE MaSanPham = @MaSanPham)
            BEGIN
                UPDATE dbo.SanPhamTrangSuc
                SET TenSanPham = @TenSanPham,
                    MaDanhMuc = @MaDanhMuc,
                    ChatLieu = @ChatLieu,
                    Size = @Size,
                    GiaNhap = @GiaNhap,
                    GiaBan = @GiaBan,
                    TrangThai = @TrangThai,
                    HinhAnh = @HinhAnh,
                    PhanTramGiamGia = @PhanTramGiamGia,
                    MaNhom = @MaNhom
                WHERE MaSanPham = @MaSanPham;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.SanPhamTrangSuc (MaSanPham, TenSanPham, MaDanhMuc, ChatLieu, Size, GiaNhap, GiaBan, TrangThai, HinhAnh, PhanTramGiamGia, MaNhom)
                VALUES (@MaSanPham, @TenSanPham, @MaDanhMuc, @ChatLieu, @Size, @GiaNhap, @GiaBan, @TrangThai, @HinhAnh, @PhanTramGiamGia, @MaNhom);
            END";

        var parameters = new[]
        {
            new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = sp.MaSanPham },
            new SqlParameter("@TenSanPham", SqlDbType.NVarChar, 200) { Value = sp.TenSanPham },
            new SqlParameter("@MaDanhMuc", SqlDbType.VarChar, 50) { Value = sp.MaDanhMuc },
            new SqlParameter("@ChatLieu", SqlDbType.NVarChar, 100) { Value = sp.ChatLieu },
            new SqlParameter("@Size", SqlDbType.NVarChar, 20) { Value = (object?)sp.Size ?? DBNull.Value },
            new SqlParameter("@GiaNhap", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = sp.GiaNhap },
            new SqlParameter("@GiaBan", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = sp.GiaBan },
            new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50) { Value = sp.TrangThai },
            new SqlParameter("@HinhAnh", SqlDbType.NVarChar, 500) { Value = (object?)sp.HinhAnh ?? DBNull.Value },
            new SqlParameter("@PhanTramGiamGia", SqlDbType.Int) { Value = sp.PhanTramGiamGia },
            new SqlParameter("@MaNhom", SqlDbType.NVarChar, 50) { Value = string.IsNullOrWhiteSpace(sp.MaNhom) ? "0" : sp.MaNhom }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    public SanPhamTrangSuc? TimTheoId(string maSanPham)
    {
        if (string.IsNullOrWhiteSpace(maSanPham))
        {
            return null;
        }

        const string sql = @"
            SELECT s.MaSanPham, s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai, s.HinhAnh, s.PhanTramGiamGia, s.MaNhom,
                   d.TenDanhMuc, d.MoTa AS MoTaDanhMuc,
                   (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk WHERE nk.MaSanPham = s.MaSanPham), 0)
                    - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon WHERE chd.MaSanPham = s.MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'), 0)) AS SoLuongTonKho
            FROM dbo.SanPhamTrangSuc s
            LEFT JOIN dbo.DanhMuc d ON s.MaDanhMuc = d.MaDanhMuc
            WHERE s.MaSanPham = @MaSanPham;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaSanPham", SqlDbType.VarChar, 50) { Value = maSanPham.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapReaderToSanPham(reader);
        }

        return null;
    }

    public List<SanPhamTrangSuc> LayTatCa()
    {
        var list = new List<SanPhamTrangSuc>();
        const string sql = @"
            SELECT s.MaSanPham, s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai, s.HinhAnh, s.PhanTramGiamGia, s.MaNhom,
                   d.TenDanhMuc, d.MoTa AS MoTaDanhMuc,
                   (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk WHERE nk.MaSanPham = s.MaSanPham), 0)
                    - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon WHERE chd.MaSanPham = s.MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'), 0)) AS SoLuongTonKho
            FROM dbo.SanPhamTrangSuc s
            LEFT JOIN dbo.DanhMuc d ON s.MaDanhMuc = d.MaDanhMuc
            ORDER BY s.MaSanPham ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(MapReaderToSanPham(reader));
        }

        return list;
    }

    public List<SanPhamTrangSuc> LayTheoNhom(string maNhom)
    {
        var list = new List<SanPhamTrangSuc>();
        if (string.IsNullOrWhiteSpace(maNhom)) return list;

        const string sql = @"
            SELECT s.MaSanPham, s.TenSanPham, s.MaDanhMuc, s.ChatLieu, s.Size, s.GiaNhap, s.GiaBan, s.TrangThai, s.HinhAnh, s.PhanTramGiamGia, s.MaNhom,
                   d.TenDanhMuc, d.MoTa AS MoTaDanhMuc,
                   (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk WHERE nk.MaSanPham = s.MaSanPham), 0)
                    - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon WHERE chd.MaSanPham = s.MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'), 0)) AS SoLuongTonKho
            FROM dbo.SanPhamTrangSuc s
            LEFT JOIN dbo.DanhMuc d ON s.MaDanhMuc = d.MaDanhMuc
            WHERE s.MaNhom = @MaNhom
            ORDER BY s.MaSanPham ASC;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@MaNhom", SqlDbType.NVarChar, 50) { Value = maNhom.Trim() });
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(MapReaderToSanPham(reader));
        }

        return list;
    }

    public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTheoNhom(maNhom));
    }

    public Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken cancellationToken = default)
    {
        LuuSanPham(sp);
        return Task.CompletedTask;
    }

    public Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimTheoId(maSanPham));
    }

    public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LayTatCa());
    }

    private static SanPhamTrangSuc MapReaderToSanPham(SqlDataReader reader)
    {
        var hinhAnh = string.Empty;
        try
        {
            var ordHinhAnh = reader.GetOrdinal("HinhAnh");
            if (!reader.IsDBNull(ordHinhAnh))
            {
                hinhAnh = reader.GetString(ordHinhAnh);
            }
        }
        catch (IndexOutOfRangeException)
        {
            // Tránh lỗi nếu truy vấn không select cột HinhAnh
        }

        var phanTramGiamGia = 0;
        try
        {
            var ordDiscount = reader.GetOrdinal("PhanTramGiamGia");
            if (!reader.IsDBNull(ordDiscount))
            {
                phanTramGiamGia = reader.GetInt32(ordDiscount);
            }
        }
        catch (IndexOutOfRangeException)
        {
            // Tránh lỗi nếu truy vấn không select cột PhanTramGiamGia
        }

        var sp = new SanPhamTrangSuc(
            maSanPham: reader.GetString(reader.GetOrdinal("MaSanPham")),
            tenSanPham: reader.GetString(reader.GetOrdinal("TenSanPham")),
            maDanhMuc: reader.GetString(reader.GetOrdinal("MaDanhMuc")),
            chatLieu: reader.GetString(reader.GetOrdinal("ChatLieu")),
            size: reader.IsDBNull(reader.GetOrdinal("Size")) ? string.Empty : reader.GetString(reader.GetOrdinal("Size")),
            giaNhap: reader.GetDecimal(reader.GetOrdinal("GiaNhap")),
            giaBan: reader.GetDecimal(reader.GetOrdinal("GiaBan")),
            trangThai: reader.GetString(reader.GetOrdinal("TrangThai")),
            hinhAnh: hinhAnh,
            phanTramGiamGia: phanTramGiamGia
        );

        var maNhom = "0";
        try
        {
            var ordNhom = reader.GetOrdinal("MaNhom");
            if (!reader.IsDBNull(ordNhom))
            {
                maNhom = reader.GetString(ordNhom);
            }
        }
        catch (IndexOutOfRangeException)
        {
            // Tránh lỗi nếu truy vấn không select cột MaNhom
        }
        sp.MaNhom = string.IsNullOrWhiteSpace(maNhom) ? "0" : maNhom;

        var soLuongTon = 0;
        try
        {
            var ordTon = reader.GetOrdinal("SoLuongTonKho");
            if (!reader.IsDBNull(ordTon))
            {
                soLuongTon = Convert.ToInt32(reader.GetValue(ordTon));
            }
        }
        catch (IndexOutOfRangeException)
        {
            // Tránh lỗi nếu truy vấn không select cột SoLuongTonKho
        }

        // Nếu sản phẩm chưa có phiếu nhập nhưng đang ở trạng thái sẵn sàng bán, ghi nhận tồn 1 chiếc trưng bày
        if (soLuongTon <= 0 && string.Equals(sp.TrangThai, "SanSang", StringComparison.OrdinalIgnoreCase))
        {
            soLuongTon = Math.Max(1, soLuongTon);
        }
        else
        {
            soLuongTon = Math.Max(0, soLuongTon);
        }
        sp.SoLuongTon = soLuongTon;

        if (!reader.IsDBNull(reader.GetOrdinal("TenDanhMuc")))
        {
            sp.DanhMuc = new DanhMuc(
                maDanhMuc: sp.MaDanhMuc,
                tenDanhMuc: reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                moTa: reader.IsDBNull(reader.GetOrdinal("MoTaDanhMuc")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaDanhMuc"))
            );
        }

        return sp;
    }
}
