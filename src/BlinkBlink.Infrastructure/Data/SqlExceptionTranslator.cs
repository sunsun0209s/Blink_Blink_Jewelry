using System;
using Microsoft.Data.SqlClient;
using BlinkBlink.Domain.Exceptions;

namespace BlinkBlink.Infrastructure.Data;

/// <summary>
/// Bộ chuyển đổi ngoại lệ SQL Server sang hệ thống ngoại lệ và mã lỗi chuẩn hóa của tầng Domain.
/// Giúp ẩn chi tiết kỹ thuật CSDL khỏi người dùng cuối đồng thời giữ nguyên nguyên nhân gốc để ghi log.
/// </summary>
public static class SqlExceptionTranslator
{
    /// <summary>
    /// Ánh xạ SqlException sang DomainException tương ứng theo mã lỗi SQL Server.
    /// </summary>
    /// <param name="ex">Ngoại lệ SQL Server phát sinh</param>
    /// <param name="context">Ngữ cảnh nghiệp vụ nơi xảy ra lỗi</param>
    /// <returns>Ngoại lệ Domain chuẩn hóa</returns>
    public static DomainException Translate(SqlException ex, string context = "")
    {
        ArgumentNullException.ThrowIfNull(ex);

        switch (ex.Number)
        {
            case 2601:
            case 2627:
                if (ex.Message.Contains("TenDangNhap", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConflictDomainException(
                        "Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên đăng nhập khác.",
                        ErrorCodes.DuplicateUsername,
                        ex);
                }
                return new ConflictDomainException(
                    "Dữ liệu bị trùng mã định danh hoặc khóa duy nhất trong hệ thống.",
                    ErrorCodes.DuplicateKey,
                    ex);

            case 547:
                return new BusinessRuleException(
                    "Dữ liệu đang gắn với chứng từ hoặc bản ghi liên quan nên không thể xóa hoặc thay đổi.",
                    ErrorCodes.ForeignKeyConstraint,
                    ex);

            case -2:
                return new DataAccessException(
                    ErrorCodes.DatabaseTimeout,
                    "Hệ thống cơ sở dữ liệu phản hồi chậm quá thời gian cho phép. Vui lòng thử lại sau.",
                    ex.Number,
                    isConnectionFailure: false,
                    isTransient: true,
                    innerException: ex);

            case 2:
            case 53:
            case 4060:
            case 18456:
            case 40613:
            case 10060:
            case 10061:
                return new DataAccessException(
                    ErrorCodes.DatabaseUnavailable,
                    "Không thể kết nối tới máy chủ cơ sở dữ liệu SQL Server.",
                    ex.Number,
                    isConnectionFailure: true,
                    isTransient: true,
                    innerException: ex);

            case 1205:
                return new DataAccessException(
                    ErrorCodes.DatabaseDeadlock,
                    "Xung đột giao dịch dữ liệu đồng thời. Vui lòng thực hiện lại thao tác.",
                    ex.Number,
                    isConnectionFailure: false,
                    isTransient: true,
                    innerException: ex);

            default:
                var prefix = string.IsNullOrWhiteSpace(context) ? string.Empty : $"[{context}] ";
                return new DataAccessException(
                    ErrorCodes.DatabaseQueryError,
                    $"{prefix}Đã xảy ra lỗi khi truy xuất cơ sở dữ liệu.",
                    ex.Number,
                    isConnectionFailure: false,
                    isTransient: false,
                    innerException: ex);
        }
    }
}

