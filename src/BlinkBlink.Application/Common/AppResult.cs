using System;

namespace BlinkBlink.Application.Common;

/// <summary>
/// Đối tượng kết quả chuẩn hóa cho các ca sử dụng nghiệp vụ không trả về dữ liệu kèm theo.
/// Giúp truyền tải rõ ràng mã lỗi và thông điệp thay vì chỉ trả về giá trị đúng/sai đơn thuần.
/// </summary>
public class AppResult
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string Message { get; }

    protected AppResult(bool isSuccess, string? errorCode, string message)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        Message = message ?? string.Empty;
    }

    public static AppResult Success(string message = "Thao tác thành công.")
        => new(true, null, message);

    public static AppResult Failure(string errorCode, string message)
        => new(false, errorCode, message);
}

/// <summary>
/// Đối tượng kết quả chuẩn hóa có mang theo dữ liệu trả về từ ca sử dụng nghiệp vụ.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về khi thành công</typeparam>
public sealed class AppResult<T> : AppResult
{
    public T? Data { get; }

    private AppResult(bool isSuccess, T? data, string? errorCode, string message)
        : base(isSuccess, errorCode, message)
    {
        Data = data;
    }

    public static AppResult<T> Success(T data, string message = "Thao tác thành công.")
        => new(true, data, null, message);

    public new static AppResult<T> Failure(string errorCode, string message)
        => new(false, default, errorCode, message);
}

