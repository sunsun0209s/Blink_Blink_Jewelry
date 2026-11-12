using System;
using BlinkBlink.Infrastructure.Data;
using Xunit;

namespace BlinkBlink.IntegrationTests;

/// <summary>
/// Kiểm thử tính năng tự động khởi tạo và nạp script cơ sở dữ liệu của DatabaseHelper.
/// </summary>
public class DatabaseHelperTests
{
    [Fact]
    public void LoadInitializationScript_NenTraVeNoiDungScriptHopLe()
    {
        // Act: Đọc nội dung kịch bản SQL (từ file vật lý hoặc tài nguyên nhúng)
        var script = DatabaseHelper.LoadInitializationScript();

        // Assert: Kịch bản phải tồn tại và chứa các bảng cùng tài khoản quản trị viên
        Assert.False(string.IsNullOrWhiteSpace(script), "Kịch bản SQL không được rỗng.");
        Assert.Contains("CREATE TABLE dbo.NguoiDung", script);
        Assert.Contains("CREATE TABLE dbo.SanPhamTrangSuc", script);
        Assert.Contains("admin", script);
        Assert.Contains("123456789", script);
    }
}
