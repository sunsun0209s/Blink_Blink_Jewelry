using System;
using System.Data;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;

namespace BlinkBlink.Infrastructure.Repositories;

/// <summary>
/// Triển khai kho dữ liệu Người dùng thông qua ADO.NET thuần.
/// Bản vẽ kỹ thuật: Phân hệ quản lý người dùng & phân quyền.xml - NguoiDungRepository
/// </summary>
public class NguoiDungRepository : INguoiDungRepository
{
    private readonly DatabaseHelper _dbHelper;

    public NguoiDungRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
    }

    public NguoiDungRepository(string connectionString) : this(new DatabaseHelper(connectionString))
    {
    }

    public NguoiDung? TimTheoTenDangNhap(string tenDN)
    {
        if (string.IsNullOrWhiteSpace(tenDN))
        {
            return null;
        }

        const string sql = @"
            SELECT u.MaNguoiDung, u.HoTen, u.SoDienThoai, u.TenDangNhap, u.MatKhau, u.MaVaiTro, u.TrangThai,
                   v.TenVaiTro, v.MoTa AS MoTaVaiTro,
                   nv.MaNhanVien, nv.LuongCoBan,
                   kh.MaKhachHang, kh.NgaySinh
            FROM dbo.NguoiDung u
            LEFT JOIN dbo.VaiTro v ON u.MaVaiTro = v.MaVaiTro
            LEFT JOIN dbo.NhanVien nv ON u.MaNguoiDung = nv.MaNguoiDung
            LEFT JOIN dbo.KhachHang kh ON u.MaNguoiDung = kh.MaNguoiDung
            WHERE u.TenDangNhap = @TenDangNhap;";

        using var conn = _dbHelper.GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@TenDangNhap", SqlDbType.VarChar, 50) { Value = tenDN.Trim() });

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var maNguoiDung = reader.GetString(reader.GetOrdinal("MaNguoiDung"));
            var hoTen = reader.GetString(reader.GetOrdinal("HoTen"));
            var sdt = reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? string.Empty : reader.GetString(reader.GetOrdinal("SoDienThoai"));
            var tenDangNhap = reader.GetString(reader.GetOrdinal("TenDangNhap"));
            var matKhau = reader.GetString(reader.GetOrdinal("MatKhau"));
            var maVaiTro = reader.GetString(reader.GetOrdinal("MaVaiTro"));
            var trangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai"));

            VaiTro? vaiTro = null;
            if (!reader.IsDBNull(reader.GetOrdinal("TenVaiTro")))
            {
                vaiTro = new VaiTro(
                    maVaiTro: maVaiTro,
                    tenVaiTro: reader.GetString(reader.GetOrdinal("TenVaiTro")),
                    moTa: reader.IsDBNull(reader.GetOrdinal("MoTaVaiTro")) ? string.Empty : reader.GetString(reader.GetOrdinal("MoTaVaiTro"))
                );
            }

            // Nếu người dùng là nhân viên
            if (!reader.IsDBNull(reader.GetOrdinal("MaNhanVien")))
            {
                var nv = new NhanVien(
                    maNguoiDung: maNguoiDung,
                    hoTen: hoTen,
                    soDienThoai: sdt,
                    tenDangNhap: tenDangNhap,
                    matKhau: matKhau,
                    maVaiTro: maVaiTro,
                    maNhanVien: reader.GetString(reader.GetOrdinal("MaNhanVien")),
                    luongCoBan: reader.GetDecimal(reader.GetOrdinal("LuongCoBan")),
                    trangThai: trangThai
                )
                {
                    VaiTro = vaiTro
                };
                return nv;
            }

            // Nếu người dùng là khách hàng
            if (!reader.IsDBNull(reader.GetOrdinal("MaKhachHang")))
            {
                var kh = new KhachHang(
                    maNguoiDung: maNguoiDung,
                    hoTen: hoTen,
                    soDienThoai: sdt,
                    tenDangNhap: tenDangNhap,
                    matKhau: matKhau,
                    maVaiTro: maVaiTro,
                    maKhachHang: reader.GetString(reader.GetOrdinal("MaKhachHang")),
                    ngaySinh: reader.GetDateTime(reader.GetOrdinal("NgaySinh")),
                    trangThai: trangThai
                )
                {
                    VaiTro = vaiTro
                };
                return kh;
            }

            // Người dùng cơ sở
            var genericUser = new NguoiDungThongThuong(maNguoiDung, hoTen, sdt, tenDangNhap, matKhau, maVaiTro, trangThai)
            {
                VaiTro = vaiTro
            };
            return genericUser;
        }

        return null;
    }

    public void CapNhat(NguoiDung nd)
    {
        if (nd == null)
        {
            return;
        }

        const string sql = @"
            UPDATE dbo.NguoiDung
            SET HoTen = @HoTen,
                SoDienThoai = @SoDienThoai,
                MatKhau = @MatKhau,
                MaVaiTro = @MaVaiTro,
                TrangThai = @TrangThai
            WHERE MaNguoiDung = @MaNguoiDung;";

        var parameters = new[]
        {
            new SqlParameter("@MaNguoiDung", SqlDbType.VarChar, 50) { Value = nd.MaNguoiDung },
            new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = nd.HoTen },
            new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 20) { Value = (object?)nd.SoDienThoai ?? DBNull.Value },
            new SqlParameter("@MatKhau", SqlDbType.NVarChar, 255) { Value = nd.MatKhau },
            new SqlParameter("@MaVaiTro", SqlDbType.VarChar, 20) { Value = nd.MaVaiTro },
            new SqlParameter("@TrangThai", SqlDbType.Bit) { Value = nd.TrangThai }
        };

        _dbHelper.ExecuteNonQuery(sql, parameters);
    }

    private sealed class NguoiDungThongThuong : NguoiDung
    {
        public NguoiDungThongThuong(string maNguoiDung, string hoTen, string soDienThoai, string tenDangNhap, string matKhau, string maVaiTro, bool trangThai)
            : base(maNguoiDung, hoTen, soDienThoai, tenDangNhap, matKhau, maVaiTro, trangThai)
        {
        }
    }
}
