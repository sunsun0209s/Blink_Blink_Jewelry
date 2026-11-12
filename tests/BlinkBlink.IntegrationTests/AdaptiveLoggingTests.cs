using System;
using System.IO;
using BlinkBlink.Admin.Services;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Infrastructure.Logging;
using Xunit;

namespace BlinkBlink.IntegrationTests;

/// <summary>
/// Kiểm thử tích hợp cơ chế ghi nhật ký thích ứng theo môi trường và bộ xử lý lỗi WinForms.
/// </summary>
public class AdaptiveLoggingTests
{
    [Fact]
    public void AdaptiveFileLogger_LocMucDoTheoMoiTruong_VaGhiDungDinhDangUtf8()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "BlinkBlinkLogTests", Guid.NewGuid().ToString("N"));
        try
        {
            // Môi trường Production: bỏ qua Debug, giữ Information trở lên
            var prodLogger = new AdaptiveFileLogger(
                new AppLogContext { Channel = "Web", EnvironmentName = "Production", DataMode = "SqlServer", CorrelationId = "REQ-PROD" },
                preferredDirectory: tempDir,
                filePrefix: "prod");

            prodLogger.LogDebug("TestSource", "Thông điệp Debug không nên xuất hiện ở Production");
            prodLogger.LogWarning("TestSource", "Cảnh báo tiếng Việt có dấu", ErrorCodes.OutOfStock);

            var prodFile = Path.Combine(tempDir, $"prod-{DateTime.Now:yyyyMMdd}.log");
            Assert.True(File.Exists(prodFile));
            var prodLines = File.ReadAllLines(prodFile);
            Assert.Single(prodLines);
            Assert.Contains("Cảnh báo tiếng Việt có dấu", prodLines[0]);
            Assert.Contains(ErrorCodes.OutOfStock, prodLines[0]);

            // Môi trường Development: ghi nhận cả Debug và StackTrace đầy đủ
            var devLogger = new AdaptiveFileLogger(
                new AppLogContext { Channel = "WinForms", EnvironmentName = "Development", DataMode = "InMemory", CorrelationId = "REQ-DEV" },
                preferredDirectory: tempDir,
                filePrefix: "dev");

            devLogger.LogDebug("TestSource", "Chi tiết chẩn đoán Dev");
            devLogger.LogError("TestSource", "Lỗi kiểm thử", ErrorCodes.DatabaseQueryError, new InvalidOperationException("Chi tiết lỗi"));

            var devFile = Path.Combine(tempDir, $"dev-{DateTime.Now:yyyyMMdd}.log");
            var devLines = File.ReadAllLines(devFile);
            Assert.Equal(2, devLines.Length);
            Assert.Contains("Chi tiết chẩn đoán Dev", devLines[0]);
            Assert.Contains("InvalidOperationException", devLines[1]);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void AdaptiveFileLogger_MaskConnectionString_CheMatKhauAnToan()
    {
        var raw = "Server=localhost;Database=BlinkBlinkJewelryDb;User Id=sa;Password=MySecret#123;TrustServerCertificate=True;";
        var masked = AdaptiveFileLogger.MaskConnectionString(raw);

        Assert.DoesNotContain("MySecret#123", masked);
        Assert.Contains("Password=***", masked);
    }

    [Fact]
    public void AdminErrorHandler_HandleKhongHienHopThoai_TraVeMaTruyVet8KyTu()
    {
        var traceId = AdminErrorHandler.Handle(
            new BusinessRuleException("Không đủ tồn kho", ErrorCodes.OutOfStock),
            nameof(AdaptiveLoggingTests),
            showDialog: false);

        Assert.False(string.IsNullOrWhiteSpace(traceId));
        Assert.Equal(8, traceId.Length);
    }
}

