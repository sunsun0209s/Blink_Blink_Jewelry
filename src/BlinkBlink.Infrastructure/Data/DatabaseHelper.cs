using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace BlinkBlink.Infrastructure.Data;

/// <summary>
/// Lớp tiện ích trung tâm quản lý kết nối và thực thi các thao tác cơ sở dữ liệu qua ADO.NET thuần.
/// Khớp 100% với Class Diagram kỹ thuật sau khi chuyển đổi từ Clean Architecture.
/// </summary>
public class DatabaseHelper
{
    private readonly string _connectionString;

    /// <summary>
    /// Chuỗi kết nối tới SQL Server
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Khởi tạo DatabaseHelper với chuỗi kết nối SQL Server
    /// </summary>
    /// <param name="connectionString">Chuỗi kết nối</param>
    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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
        catch
        {
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

                IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = '0')
                BEGIN
                    INSERT INTO dbo.NguoiDung (MaNguoiDung, HoTen, SoDienThoai, TenDangNhap, MatKhau, MaVaiTro, TrangThai)
                    VALUES ('0', N'Khách Hàng Vãng Lai (POS)', '', 'khach.pos', '123456789', 'KHACHHANG', 1);

                    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE MaKhachHang = 'KH_GUEST_POS' OR MaNguoiDung = '0')
                    BEGIN
                        INSERT INTO dbo.KhachHang (MaNguoiDung, MaKhachHang, NgaySinh)
                        VALUES ('0', 'KH_GUEST_POS', '2000-01-01');
                    END
                END";

            ExecuteNonQuery(migrationSql);
        }
        catch
        {
            // Bỏ qua nếu không có quyền DDL hoặc database ngoại tuyến
        }
    }

    /// <summary>
    /// Thử tự động khởi tạo cơ sở dữ liệu và nạp dữ liệu mẫu từ kịch bản SQL Server nếu máy chủ SQL đang hoạt động
    /// nhưng chưa có database BlinkBlinkJewelryDb.
    /// </summary>
    /// <param name="targetConnectionString">Chuỗi kết nối mục tiêu tới BlinkBlinkJewelryDb</param>
    /// <param name="helper">Đối tượng DatabaseHelper đã kết nối thành công sau khi khởi tạo</param>
    /// <returns>true nếu CSDL đã sẵn sàng hoạt động, false nếu không thể kết nối tới máy chủ SQL Server</returns>
    public static bool TryProvisionDatabase(string targetConnectionString, out DatabaseHelper? helper)
    {
        helper = null;
        try
        {
            var targetBuilder = new SqlConnectionStringBuilder(targetConnectionString);
            var dbName = string.IsNullOrWhiteSpace(targetBuilder.InitialCatalog) ? "BlinkBlinkJewelryDb" : targetBuilder.InitialCatalog;

            // Bước 1: Kiểm tra xem đã kết nối trực tiếp được vào database mục tiêu chưa
            var directHelper = new DatabaseHelper(targetConnectionString);
            if (directHelper.TestConnection())
            {
                // Nếu database đã tồn tại nhưng có thể chưa nạp bảng, kiểm tra bảng NguoiDung
                using (var conn = directHelper.GetConnection())
                {
                    using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'NguoiDung'", conn);
                    var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count == 0)
                    {
                        var script = LoadInitializationScript();
                        if (!string.IsNullOrWhiteSpace(script))
                        {
                            ExecuteScriptBatches(conn, script);
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
            var newDbHelper = new DatabaseHelper(targetConnectionString);
            using (var dbConn = newDbHelper.GetConnection())
            {
                using var checkTableCmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'NguoiDung'", dbConn);
                var tableCount = Convert.ToInt32(checkTableCmd.ExecuteScalar());

                // Nếu chưa có bảng NguoiDung, thực thi kịch bản SQL khởi tạo toàn bộ
                if (tableCount == 0)
                {
                    var sqlScript = LoadInitializationScript();
                    if (!string.IsNullOrWhiteSpace(sqlScript))
                    {
                        ExecuteScriptBatches(dbConn, sqlScript);
                    }
                }
            }

            // Bước 4: Kiểm tra và đảm bảo tương thích các cột mở rộng
            newDbHelper.EnsureSchemaCompatibility();

            if (newDbHelper.TestConnection())
            {
                helper = newDbHelper;
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseHelper.TryProvisionDatabase] {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Đọc nội dung kịch bản SQL khởi tạo CSDL từ file vật lý trên đĩa hoặc từ tài nguyên nhúng trong assembly.
    /// </summary>
    /// <returns>Chuỗi văn bản kịch bản SQL UTF-8</returns>
    public static string LoadInitializationScript()
    {
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
            catch { }
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
        catch { }

        return string.Empty;
    }

    /// <summary>
    /// Thực thi kịch bản SQL có chứa nhiều khối lệnh được phân tách bằng từ khóa GO.
    /// ADO.NET thuần không nhận diện lệnh GO nên phải tách batch trước khi gửi tới SQL Server.
    /// </summary>
    /// <param name="conn">Kết nối đã mở đến cơ sở dữ liệu mục tiêu</param>
    /// <param name="script">Toàn văn kịch bản SQL</param>
    public static void ExecuteScriptBatches(SqlConnection conn, string script)
    {
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
                System.Diagnostics.Debug.WriteLine($"[ExecuteScriptBatches] Lỗi tại batch: {ex.Message}");
            }
        }
    }
}

