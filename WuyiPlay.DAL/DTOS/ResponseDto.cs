namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO chuẩn cho API Response (thành công)
/// </summary>
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponseDto()
    {
    }

    public ApiResponseDto(bool success, string message, T data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}

/// <summary>
/// DTO chuẩn cho API Response (không có data)
/// </summary>
public class ApiResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponseDto()
    {
    }

    public ApiResponseDto(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

/// <summary>
/// DTO cho danh sách phân trang
/// </summary>
public class PaginatedDto<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public List<T> Items { get; set; } = new();

    public PaginatedDto()
    {
    }

    public PaginatedDto(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPrevious = pageNumber > 1;
        HasNext = pageNumber < TotalPages;
        Items = items;
    }
}

/// <summary>
/// DTO cho API Response danh sách phân trang
/// </summary>
public class PaginatedApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public PaginatedDto<T> Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public PaginatedApiResponseDto()
    {
    }

    public PaginatedApiResponseDto(bool success, string message, PaginatedDto<T> data)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}

/// <summary>
/// DTO cho Error Response
/// </summary>
public class ErrorResponseDto
{
    public bool Success { get; set; } = false;
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ErrorResponseDto()
    {
    }

    public ErrorResponseDto(string message, string errorCode = "ERROR")
    {
        Message = message;
        ErrorCode = errorCode;
    }

    public ErrorResponseDto(string message, string errorCode, Dictionary<string, string[]> errors)
    {
        Message = message;
        ErrorCode = errorCode;
        Errors = errors;
    }
}
