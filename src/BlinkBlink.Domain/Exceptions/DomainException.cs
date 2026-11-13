using System;

namespace BlinkBlink.Domain.Exceptions;

/// <summary>
/// Lớp ngoại lệ cơ sở của tầng nghiệp vụ, mang mã lỗi chuẩn hóa để các tầng ngoài ánh xạ thích ứng.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Mã lỗi định danh duy nhất phục vụ tra cứu và xử lý tự động phía client.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Khởi tạo ngoại lệ nghiệp vụ với mã lỗi và thông điệp tiếng Việt thân thiện.
    /// </summary>
    /// <param name="errorCode">Mã lỗi chuẩn hóa từ ErrorCodes</param>
    /// <param name="message">Thông điệp mô tả lỗi</param>
    /// <param name="innerException">Ngoại lệ gốc nếu có</param>
    protected DomainException(string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? ErrorCodes.UnhandledSystemError : errorCode;
    }
}

/// <summary>
/// Ngoại lệ khi dữ liệu đầu vào không hợp lệ hoặc thiếu thông tin bắt buộc.
/// </summary>
public sealed class ValidationDomainException : DomainException
{
    public ValidationDomainException(string message, string errorCode = ErrorCodes.InvalidInput, Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Ngoại lệ khi vi phạm quy tắc nghiệp vụ cốt lõi (hết tồn kho, giỏ hàng rỗng, sai trạng thái chuyển đổi).
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message, string errorCode = ErrorCodes.InvalidStateTransition, Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Ngoại lệ khi không tìm thấy thực thể hoặc bản ghi được yêu cầu.
/// </summary>
public sealed class NotFoundDomainException : DomainException
{
    public NotFoundDomainException(string message, string errorCode = ErrorCodes.ResourceNotFound, Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Ngoại lệ khi thông tin xác thực đăng nhập không hợp lệ.
/// </summary>
public sealed class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException(string message, string errorCode = ErrorCodes.InvalidCredentials, Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Ngoại lệ khi tài khoản bị tạm khóa hoặc không đủ quyền truy cập nghiệp vụ.
/// </summary>
public sealed class ForbiddenDomainException : DomainException
{
    public ForbiddenDomainException(string message, string errorCode = ErrorCodes.AccountLocked, Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Ngoại lệ khi dữ liệu bị trùng khóa chính, mã định danh hoặc tên đăng nhập.
/// </summary>
public sealed class ConflictDomainException : DomainException
{
    public ConflictDomainException(string message, string errorCode = ErrorCodes.DuplicateKey, Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Ngoại lệ đại diện cho lỗi kết nối hoặc thực thi tại tầng truy xuất dữ liệu mà vẫn giữ ranh giới trừu tượng với tầng trên.
/// </summary>
public sealed class DataAccessException : DomainException
{
    /// <summary>
    /// Mã lỗi gốc từ SQL Server (nếu phát sinh từ SqlException).
    /// </summary>
    public int? SqlErrorNumber { get; }

    /// <summary>
    /// Cho biết lỗi có thuộc nhóm mất kết nối tới máy chủ cơ sở dữ liệu hay không.
    /// </summary>
    public bool IsConnectionFailure { get; }

    /// <summary>
    /// Cho biết lỗi có tính tạm thời và có thể thử lại hay không.
    /// </summary>
    public bool IsTransient { get; }

    public DataAccessException(
        string errorCode,
        string message,
        int? sqlErrorNumber = null,
        bool isConnectionFailure = false,
        bool isTransient = false,
        Exception? innerException = null)
        : base(errorCode, message, innerException)
    {
        SqlErrorNumber = sqlErrorNumber;
        IsConnectionFailure = isConnectionFailure;
        IsTransient = isTransient;
    }
}

