using System;
using System.Data;
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
}
