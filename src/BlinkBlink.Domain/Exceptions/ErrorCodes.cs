namespace BlinkBlink.Domain.Exceptions;

/// <summary>
/// Tập hằng số mã lỗi chuẩn hóa dùng chung xuyên suốt toàn bộ hệ thống Clean Architecture.
/// Giúp liên kết đồng nhất giữa ngoại lệ nghiệp vụ, phản hồi HTTP API, giao diện WinForms và file nhật ký.
/// </summary>
public static class ErrorCodes
{
    // Nhóm lỗi kiểm tra dữ liệu đầu vào (Validation)
    public const string InvalidInput = "VAL_INVALID_INPUT";
    public const string InvalidPriceOrQuantity = "VAL_INVALID_PRICE";

    // Nhóm lỗi xác thực và phân quyền (Auth)
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AccountLocked = "AUTH_ACCOUNT_LOCKED";

    // Nhóm lỗi quy tắc nghiệp vụ (Business Rule)
    public const string CartEmpty = "BUS_CART_EMPTY";
    public const string OutOfStock = "BUS_OUT_OF_STOCK";
    public const string InvalidStateTransition = "BUS_INVALID_STATE_TRANSITION";
    public const string NegativeStockNotAllowed = "BUS_NEGATIVE_STOCK_NOT_ALLOWED";
    public const string ForeignKeyConstraint = "BUS_FOREIGN_KEY_CONSTRAINT";

    // Nhóm lỗi không tìm thấy dữ liệu (Not Found)
    public const string ResourceNotFound = "NOT_FOUND_RESOURCE";
    public const string ProductNotFound = "NOT_FOUND_PRODUCT";
    public const string CustomerNotFound = "NOT_FOUND_CUSTOMER";
    public const string OrderNotFound = "NOT_FOUND_ORDER";
    public const string WarrantyNotFound = "NOT_FOUND_WARRANTY";

    // Nhóm lỗi xung đột / trùng lặp dữ liệu (Conflict)
    public const string DuplicateKey = "CONF_DUPLICATE_KEY";
    public const string DuplicateUsername = "CONF_DUPLICATE_USERNAME";

    // Nhóm lỗi hạ tầng cơ sở dữ liệu (Database)
    public const string DatabaseUnavailable = "DB_UNAVAILABLE";
    public const string DatabaseTimeout = "DB_TIMEOUT";
    public const string DatabaseDeadlock = "DB_DEADLOCK";
    public const string DatabaseQueryError = "DB_QUERY_ERROR";

    // Nhóm lỗi hệ thống chưa xác định (System)
    public const string UnhandledSystemError = "SYS_UNHANDLED_ERROR";
}

