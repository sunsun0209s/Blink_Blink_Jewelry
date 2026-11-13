using BlinkBlink.Application.Common;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Domain.Exceptions;
using Xunit;

namespace BlinkBlink.UnitTests.Common;

/// <summary>
/// Kiểm thử đơn vị cho mô hình ngoại lệ, mã lỗi và kết quả chuẩn hóa dùng trong thiết kế Log & Error thích ứng.
/// </summary>
public class AdaptiveLogAndErrorTests
{
    [Fact]
    public void AppResult_SuccessVaFailure_TraVeDungTrangThaiVaMaLoi()
    {
        var ok = AppResult<string>.Success("HD_ONLINE_001", "Thành công");
        var fail = AppResult<string>.Failure(ErrorCodes.OutOfStock, "Hết hàng");

        Assert.True(ok.IsSuccess);
        Assert.Null(ok.ErrorCode);
        Assert.Equal("HD_ONLINE_001", ok.Data);

        Assert.False(fail.IsSuccess);
        Assert.Equal(ErrorCodes.OutOfStock, fail.ErrorCode);
        Assert.Equal("Hết hàng", fail.Message);
        Assert.Null(fail.Data);
    }

    [Fact]
    public void DomainExceptions_GiuNguyenMaLoiVaThuocTinhChuyenBiet()
    {
        var locked = new ForbiddenDomainException("Tài khoản bị khóa");
        var notFound = new NotFoundDomainException("Không thấy sản phẩm", ErrorCodes.ProductNotFound);
        var conflict = new ConflictDomainException("Trùng tên đăng nhập", ErrorCodes.DuplicateUsername);
        var dbErr = new DataAccessException(ErrorCodes.DatabaseUnavailable, "Mất kết nối SQL", 53, isConnectionFailure: true, isTransient: true);

        Assert.Equal(ErrorCodes.AccountLocked, locked.ErrorCode);
        Assert.Equal(ErrorCodes.ProductNotFound, notFound.ErrorCode);
        Assert.Equal(ErrorCodes.DuplicateUsername, conflict.ErrorCode);
        Assert.Equal(ErrorCodes.DatabaseUnavailable, dbErr.ErrorCode);
        Assert.Equal(53, dbErr.SqlErrorNumber);
        Assert.True(dbErr.IsConnectionFailure);
        Assert.True(dbErr.IsTransient);
    }

    [Fact]
    public void NullAppLogger_KhongNemNgoaiLeKhiGhiMoiMucDo()
    {
        var logger = NullAppLogger.Instance;
        logger.LogDebug("Test", "Debug message");
        logger.LogInfo("Test", "Info message");
        logger.LogWarning("Test", "Warn message", ErrorCodes.InvalidInput);
        logger.LogError("Test", "Error message", ErrorCodes.DatabaseQueryError);
        logger.LogCritical("Test", "Critical message", ErrorCodes.UnhandledSystemError);

        Assert.Equal("Test", logger.DefaultContext.Channel);
    }
}

