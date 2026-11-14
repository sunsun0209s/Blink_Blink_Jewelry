using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Infrastructure.Logging;

namespace BlinkBlink.Infrastructure.Data;

/// <summary>
/// Lớp tiện ích trung tâm quản lý kết nối và thực thi các thao tác cơ sở dữ liệu qua ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật sau khi chuyển đổi từ Clean Architecture.
/// </summary>
public class DatabaseHelper
{
    private readonly string _connectionString;
    private readonly IAppLogger _logger;

    /// <summary>
    /// Chuỗi kết nối tới SQL Server
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Khởi tạo DatabaseHelper với chuỗi kết nối SQL Server và bộ ghi nhật ký tùy chọn
    /// </summary>
    /// <param name="connectionString">Chuỗi kết nối</param>
    /// <param name="logger">Bộ ghi nhật ký thích ứng</param>
    public DatabaseHelper(string connectionString, IAppLogger? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? NullAppLogger.Instance;
    }

    /// <summary>
    /// Mở và trả về một kết nối SqlConnection mới
    /// </summary>
    /// <returns>Đối tượng SqlConnection đã mở</returns>
    public SqlConnection GetConnection()
    {
        var connection = new SqlConnection(_connectionString);
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
        return connection;
    }

    /// <summary>
    /// Kiểm tra xem có kết nối được tới cơ sở dữ liệu SQL Server hay không
    /// </summary>
    /// <returns>true nếu kết nối thành công, false nếu thất bại</returns>
    public bool TestConnection()
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn.State == ConnectionState.Open;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                nameof(DatabaseHelper),
                $"Kiểm tra kết nối thất bại tới {AdaptiveFileLogger.MaskConnectionString(_connectionString)}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Thực thi lệnh SQL không trả về dữ liệu (INSERT, UPDATE, DELETE)
    /// </summary>
    public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters != null && parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Thực thi lệnh SQL trả về một giá trị duy nhất (COUNT, SUM, MAX,...)
    /// </summary>
    public object? ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters != null && parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? null : result;
    }

    /// <summary>
    /// Thực thi câu lệnh truy vấn và trả về bảng dữ liệu DataTable trong bộ nhớ
    /// </summary>
    public DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        if (parameters != null && parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }
        using var adapter = new SqlDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    /// <summary>
    /// Tự động kiểm tra và cập nhật các cột/dữ liệu cần thiết cho CSDL hiện có,
    /// đảm bảo tương thích ngược và không gây lỗi khi chạy trên CSDL cũ chưa được nạp lại script mới.
    /// </summary>
    public void EnsureSchemaCompatibility()
    {
        try
        {
            const string migrationSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ChiTietHoaDon') AND name = 'PhanTramGiamGia')
                BEGIN
                    ALTER TABLE dbo.ChiTietHoaDon ADD PhanTramGiamGia INT NOT NULL CONSTRAINT DF_ChiTietHoaDon_PhanTramGiamGia DEFAULT 0;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuBaoHanh') AND name = 'ChiPhiUocTinh')
                BEGIN
                    ALTER TABLE dbo.PhieuBaoHanh ADD ChiPhiUocTinh DECIMAL(18,2) NOT NULL CONSTRAINT DF_PhieuBaoHanh_ChiPhiUocTinh DEFAULT 0;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuBaoHanh') AND name = 'ChiPhiThucTe')
                BEGIN
                    ALTER TABLE dbo.PhieuBaoHanh ADD ChiPhiThucTe DECIMAL(18,2) NOT NULL CONSTRAINT DF_PhieuBaoHanh_ChiPhiThucTe DEFAULT 0;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuBaoHanh') AND name = 'TinhTrangTiepNhan')
                BEGIN
                    ALTER TABLE dbo.PhieuBaoHanh ADD TinhTrangTiepNhan NVARCHAR(300) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuBaoHanh') AND name = 'GhiChu')
                BEGIN
                    ALTER TABLE dbo.PhieuBaoHanh ADD GhiChu NVARCHAR(500) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuBaoHanh') AND name = 'SoDienThoai')
                BEGIN
                    ALTER TABLE dbo.PhieuBaoHanh ADD SoDienThoai VARCHAR(20) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE MaVaiTro = 'TECHNICIAN')
                BEGIN
                    INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro, MoTa)
                    VALUES ('TECHNICIAN', N'Nhân viên kỹ thuật', N'Tiếp nhận sửa chữa, gia công và quyết toán chi phí bảo hành trang sức');
                END

                IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE TenDangNhap = 'kythuat')
                BEGIN
                    INSERT INTO dbo.NguoiDung (MaNguoiDung, HoTen, SoDienThoai, TenDangNhap, MatKhau, MaVaiTro, TrangThai)
                    VALUES ('USER_NV003', N'Đỗ Văn Kỹ Thuật', '0944556677', 'kythuat', '123456789', 'TECHNICIAN', 1);

                    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE MaNguoiDung = 'USER_NV003')
                    BEGIN
                        INSERT INTO dbo.NhanVien (MaNguoiDung, MaNhanVien, LuongCoBan)
                        VALUES ('USER_NV003', 'NV003', 10000000.00);
                    END
                END

                IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = '0')
                BEGIN
                    INSERT INTO dbo.NguoiDung (MaNguoiDung, HoTen, SoDienThoai, TenDangNhap, MatKhau, MaVaiTro, TrangThai)
                    VALUES ('0', N'Khách Hàng Vãng Lai (POS)', '', 'khach.pos', '123456789', 'KHACHHANG', 1);

                    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE MaKhachHang = 'KH_GUEST_POS' OR MaNguoiDung = '0')
                    BEGIN
                        INSERT INTO dbo.KhachHang (MaNguoiDung, MaKhachHang, NgaySinh)
                        VALUES ('0', 'KH_GUEST_POS', '2000-01-01');
                    END
                END

                -- Tạo bảng NhomSanPham và cột MaNhom cho SanPhamTrangSuc
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NhomSanPham')
                BEGIN
                    CREATE TABLE dbo.NhomSanPham (
                        MaNhom NVARCHAR(50) PRIMARY KEY,
                        TenNhom NVARCHAR(255) NOT NULL,
                        MoTa NVARCHAR(500) NULL
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.SanPhamTrangSuc') AND name = 'MaNhom')
                BEGIN
                    ALTER TABLE dbo.SanPhamTrangSuc ADD MaNhom NVARCHAR(50) NOT NULL CONSTRAINT DF_SanPhamTrangSuc_MaNhom DEFAULT '0';
                END

                -- Cột TyLeLoiNhuan cho SanPhamTrangSuc (% lợi nhuận)
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.SanPhamTrangSuc') AND name = 'TyLeLoiNhuan')
                BEGIN
                    ALTER TABLE dbo.SanPhamTrangSuc ADD TyLeLoiNhuan DECIMAL(5, 2) NOT NULL CONSTRAINT DF_SanPhamTrangSuc_TyLeLoiNhuan DEFAULT 0;
                    -- Tự động tính tỷ lệ lợi nhuận cho sản phẩm đã có sẵn giá nhập và giá bán
                    UPDATE dbo.SanPhamTrangSuc
                    SET TyLeLoiNhuan = ROUND(((GiaBan / GiaNhap) - 1.0) * 100.0, 2)
                    WHERE GiaNhap > 0 AND GiaBan > GiaNhap;
                END

                -- Chuẩn hóa cột TrangThai cho PhieuNhapKho (ChoDuyet, DaDuyet, DaHuy)
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuNhapKho') AND name = 'TrangThai' AND system_type_id = 104)
                BEGIN
                    DECLARE @dfName NVARCHAR(200);
                    SELECT @dfName = d.name 
                    FROM sys.default_constraints d 
                    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
                    WHERE d.parent_object_id = OBJECT_ID(N'dbo.PhieuNhapKho') AND c.name = 'TrangThai';

                    IF @dfName IS NOT NULL
                        EXEC('ALTER TABLE dbo.PhieuNhapKho DROP CONSTRAINT ' + @dfName);

                    ALTER TABLE dbo.PhieuNhapKho ALTER COLUMN TrangThai NVARCHAR(50) NOT NULL;
                    UPDATE dbo.PhieuNhapKho SET TrangThai = CASE WHEN TrangThai = '1' THEN 'DaDuyet' ELSE 'DaHuy' END WHERE TrangThai IN ('0', '1');
                    ALTER TABLE dbo.PhieuNhapKho ADD CONSTRAINT DF_PhieuNhapKho_TrangThai DEFAULT 'ChoDuyet' FOR TrangThai;
                END
                ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuNhapKho') AND name = 'TrangThai')
                BEGIN
                    ALTER TABLE dbo.PhieuNhapKho ADD TrangThai NVARCHAR(50) NOT NULL CONSTRAINT DF_PhieuNhapKho_TrangThai DEFAULT 'ChoDuyet';
                END

                -- Cột TrangThai, GhiChu và NgayDuyet cho PhieuKiemKe
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuKiemKe') AND name = 'TrangThai')
                BEGIN
                    ALTER TABLE dbo.PhieuKiemKe ADD TrangThai NVARCHAR(50) NOT NULL CONSTRAINT DF_PhieuKiemKe_TrangThai DEFAULT 'ChoDuyet';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuKiemKe') AND name = 'GhiChu')
                BEGIN
                    ALTER TABLE dbo.PhieuKiemKe ADD GhiChu NVARCHAR(500) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PhieuKiemKe') AND name = 'NgayDuyet')
                BEGIN
                    ALTER TABLE dbo.PhieuKiemKe ADD NgayDuyet DATETIME2 NULL;
                END

                -- Bổ sung danh mục mặc định 'Chưa phân loại' (DM00) nếu chưa tồn tại
                IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE MaDanhMuc = 'DM00')
                BEGIN
                    INSERT INTO dbo.DanhMuc (MaDanhMuc, TenDanhMuc, MoTa)
                    VALUES ('DM00', N'Chưa phân loại', N'Danh mục mặc định cho sản phẩm chưa phân loại hoặc danh mục cũ bị xóa');
                END

                -- Tự động khôi phục trạng thái SanSang cho các sản phẩm còn tồn kho > 0 nhưng bị đánh dấu nhầm là DaBan hoặc DangBaoHanh
                UPDATE dbo.SanPhamTrangSuc
                SET TrangThai = 'SanSang'
                WHERE (TrangThai = 'DaBan' OR TrangThai = 'DangBaoHanh') AND (
                    (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk INNER JOIN dbo.PhieuNhapKho pn ON nk.MaPhieuNhap = pn.MaPhieuNhap WHERE nk.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham AND (pn.TrangThai = 'DaDuyet' OR pn.TrangThai = '1')), 0)
                     - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon WHERE chd.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'), 0)) > 0
                );

                -- Tự động khôi phục trạng thái DaBan cho các sản phẩm hết hàng nhưng bị đánh dấu DangBaoHanh
                UPDATE dbo.SanPhamTrangSuc
                SET TrangThai = 'DaBan'
                WHERE TrangThai = 'DangBaoHanh' AND (
                    (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk INNER JOIN dbo.PhieuNhapKho pn ON nk.MaPhieuNhap = pn.MaPhieuNhap WHERE nk.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham AND (pn.TrangThai = 'DaDuyet' OR pn.TrangThai = '1')), 0)
                     - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon WHERE chd.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'), 0)) <= 0
                );

                -- Tự động hoàn nguyên Giá Nhập và Giá Bán về 0 đối với các sản phẩm có toàn bộ phiếu nhập bị hủy và tồn kho = 0
                UPDATE dbo.SanPhamTrangSuc
                SET GiaNhap = 0, GiaBan = 0, TrangThai = 'HetHang'
                WHERE MaSanPham IN (
                    SELECT DISTINCT ct.MaSanPham
                    FROM dbo.ChiTietNhapKho ct
                    INNER JOIN dbo.PhieuNhapKho pn ON ct.MaPhieuNhap = pn.MaPhieuNhap
                    WHERE pn.TrangThai = 'DaHuy'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM dbo.ChiTietNhapKho ct2
                    INNER JOIN dbo.PhieuNhapKho pn2 ON ct2.MaPhieuNhap = pn2.MaPhieuNhap
                    WHERE ct2.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham
                      AND (pn2.TrangThai = 'DaDuyet' OR pn2.TrangThai = '1')
                )
                AND (
                    (ISNULL((SELECT SUM(nk.SoLuong) FROM dbo.ChiTietNhapKho nk INNER JOIN dbo.PhieuNhapKho pn ON nk.MaPhieuNhap = pn.MaPhieuNhap WHERE nk.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham AND (pn.TrangThai = 'DaDuyet' OR pn.TrangThai = '1')), 0)
                     - ISNULL((SELECT SUM(chd.SoLuong) FROM dbo.ChiTietHoaDon chd INNER JOIN dbo.HoaDon hd ON chd.SoHoaDon = hd.MaHoaDon WHERE chd.MaSanPham = dbo.SanPhamTrangSuc.MaSanPham AND hd.TrangThaiDonHang != 'DaHuy'), 0)) <= 0
                );";

            ExecuteNonQuery(migrationSql);
            _logger.LogInfo(nameof(DatabaseHelper), "Kiểm tra tương thích lược đồ cơ sở dữ liệu hoàn tất.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogWarning(
                nameof(DatabaseHelper),
                "Không thể chạy nâng cấp lược đồ tự động (có thể thiếu quyền DDL hoặc bảng chưa tồn tại).",
                ErrorCodes.DatabaseQueryError,
                sqlEx);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                nameof(DatabaseHelper),
                "Bỏ qua bước kiểm tra tương thích lược đồ do lỗi môi trường.",
                ErrorCodes.DatabaseUnavailable,
                ex);
        }
    }

    /// <summary>
    /// Thử tự động khởi tạo cơ sở dữ liệu và nạp dữ liệu mẫu từ kịch bản SQL Server nếu máy chủ SQL đang hoạt động
    /// nhưng chưa có database BlinkBlinkJewelryDb.
    /// </summary>
    public static bool TryProvisionDatabase(string targetConnectionString, out DatabaseHelper? helper)
        => TryProvisionDatabase(targetConnectionString, out helper, null);

    /// <summary>
    /// Thử tự động khởi tạo cơ sở dữ liệu kèm bộ ghi nhật ký chẩn đoán.
    /// </summary>
    public static bool TryProvisionDatabase(string targetConnectionString, out DatabaseHelper? helper, IAppLogger? logger)
    {
        var activeLogger = logger ?? NullAppLogger.Instance;
        helper = null;
        try
        {
            var targetBuilder = new SqlConnectionStringBuilder(targetConnectionString);
            var dbName = string.IsNullOrWhiteSpace(targetBuilder.InitialCatalog) ? "BlinkBlinkJewelryDb" : targetBuilder.InitialCatalog;

            // Bước 1: Kiểm tra xem đã kết nối trực tiếp được vào database mục tiêu chưa
            var directHelper = new DatabaseHelper(targetConnectionString, activeLogger);
            if (directHelper.TestConnection())
            {
                // Nếu database đã tồn tại nhưng có thể chưa nạp bảng, kiểm tra bảng NguoiDung
                using (var conn = directHelper.GetConnection())
                {
                    using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'NguoiDung'", conn);
                    var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count == 0)
                    {
                        var script = LoadInitializationScript(activeLogger);
                        if (!string.IsNullOrWhiteSpace(script))
                        {
                            ExecuteScriptBatches(conn, script, activeLogger);
                        }
                    }
                }

                directHelper.EnsureSchemaCompatibility();
                helper = directHelper;
                return true;
            }

            // Bước 2: Thử kết nối vào CSDL hệ thống master của máy chủ này để kiểm tra máy chủ có chạy không
            var masterBuilder = new SqlConnectionStringBuilder(targetConnectionString)
            {
                InitialCatalog = "master",
                ConnectTimeout = 3
            };

            using (var masterConn = new SqlConnection(masterBuilder.ConnectionString))
            {
                masterConn.Open();

                // Tạo database nếu chưa tồn tại
                using var createDbCmd = new SqlCommand($"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{dbName}') BEGIN CREATE DATABASE [{dbName}]; END", masterConn);
                createDbCmd.ExecuteNonQuery();
            }

            // Bước 3: Kết nối vào database mới tạo và kiểm tra xem bảng chính (NguoiDung) đã có chưa
            var newDbHelper = new DatabaseHelper(targetConnectionString, activeLogger);
            using (var dbConn = newDbHelper.GetConnection())
            {
                using var checkTableCmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'NguoiDung'", dbConn);
                var tableCount = Convert.ToInt32(checkTableCmd.ExecuteScalar());

                // Nếu chưa có bảng NguoiDung, thực thi kịch bản SQL khởi tạo toàn bộ
                if (tableCount == 0)
                {
                    var sqlScript = LoadInitializationScript(activeLogger);
                    if (!string.IsNullOrWhiteSpace(sqlScript))
                    {
                        ExecuteScriptBatches(dbConn, sqlScript, activeLogger);
                    }
                }
            }

            // Bước 4: Kiểm tra và đảm bảo tương thích các cột mở rộng
            newDbHelper.EnsureSchemaCompatibility();

            if (newDbHelper.TestConnection())
            {
                activeLogger.LogInfo(nameof(DatabaseHelper), $"Đã tự động khởi tạo cơ sở dữ liệu thành công tại {AdaptiveFileLogger.MaskConnectionString(targetConnectionString)}.");
                helper = newDbHelper;
                return true;
            }
        }
        catch (Exception ex)
        {
            activeLogger.LogWarning(
                nameof(DatabaseHelper),
                $"Không thể tự động khởi tạo CSDL với chuỗi {AdaptiveFileLogger.MaskConnectionString(targetConnectionString)}.",
                ErrorCodes.DatabaseUnavailable,
                ex);
        }

        return false;
    }

    /// <summary>
    /// Đọc nội dung kịch bản SQL khởi tạo CSDL từ file vật lý trên đĩa hoặc từ tài nguyên nhúng trong assembly.
    /// </summary>
    /// <returns>Chuỗi văn bản kịch bản SQL UTF-8</returns>
    public static string LoadInitializationScript()
        => LoadInitializationScript(null);

    /// <summary>
    /// Đọc nội dung kịch bản SQL khởi tạo CSDL kèm ghi nhật ký chẩn đoán.
    /// </summary>
    public static string LoadInitializationScript(IAppLogger? logger)
    {
        var activeLogger = logger ?? NullAppLogger.Instance;
        // 1. Tìm kiếm file vật lý tại các thư mục tương đối của solution
        var searchPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "BlinkBlinkJewelry.sql"),
            Path.Combine(AppContext.BaseDirectory, "database", "BlinkBlinkJewelry.sql"),
            Path.Combine(Directory.GetCurrentDirectory(), "BlinkBlinkJewelry.sql"),
            Path.Combine(Directory.GetCurrentDirectory(), "database", "BlinkBlinkJewelry.sql"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "BlinkBlinkJewelry.sql"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "BlinkBlinkJewelry.sql"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "BlinkBlinkJewelry.sql"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "database", "BlinkBlinkJewelry.sql")
        };

        foreach (var path in searchPaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                activeLogger.LogDebug(nameof(DatabaseHelper), $"Không đọc được kịch bản SQL tại '{path}': {ex.Message}");
            }
        }

        // 2. Dự phòng: Đọc từ tài nguyên nhúng (EmbeddedResource) đã được biên dịch thẳng vào DLL
        try
        {
            var assembly = typeof(DatabaseHelper).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            var targetName = Array.Find(resourceNames, n => n.EndsWith("BlinkBlinkJewelry.sql", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(targetName))
            {
                using var stream = assembly.GetManifestResourceStream(targetName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            activeLogger.LogWarning(nameof(DatabaseHelper), "Lỗi khi đọc tài nguyên nhúng BlinkBlinkJewelry.sql.", ErrorCodes.DatabaseQueryError, ex);
        }

        return string.Empty;
    }

    /// <summary>
    /// Thực thi kịch bản SQL có chứa nhiều khối lệnh được phân tách bằng từ khóa GO.
    /// ADO.NET thuần không nhận diện lệnh GO nên phải tách batch trước khi gửi tới SQL Server.
    /// </summary>
    /// <param name="conn">Kết nối đã mở đến cơ sở dữ liệu mục tiêu</param>
    /// <param name="script">Toàn văn kịch bản SQL</param>
    /// <param name="logger">Bộ ghi nhật ký tùy chọn</param>
    public static void ExecuteScriptBatches(SqlConnection conn, string script, IAppLogger? logger = null)
    {
        var activeLogger = logger ?? NullAppLogger.Instance;
        if (string.IsNullOrWhiteSpace(script) || conn == null || conn.State != ConnectionState.Open) return;

        var batches = Regex.Split(
            script,
            @"^\s*GO\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        foreach (var rawBatch in batches)
        {
            var batch = rawBatch.Trim();
            if (string.IsNullOrWhiteSpace(batch)) continue;

            // Bỏ qua các khối lệnh chuyển đổi ngữ cảnh USE master hoặc lệnh tạo CSDL vì CSDL mục tiêu đã được tạo sẵn
            if (batch.Equals("USE master;", StringComparison.OrdinalIgnoreCase) ||
                batch.StartsWith("USE master", StringComparison.OrdinalIgnoreCase) ||
                batch.Contains("CREATE DATABASE BlinkBlinkJewelryDb", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                using var cmd = new SqlCommand(batch, conn);
                cmd.CommandTimeout = 120; // Đặt thời gian chờ phù hợp để hoàn tất các bảng lớn
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                activeLogger.LogWarning(nameof(DatabaseHelper), $"Lỗi thực thi batch SQL khởi tạo: {ex.Message}", ErrorCodes.DatabaseQueryError, ex);
            }
        }
    }
}

