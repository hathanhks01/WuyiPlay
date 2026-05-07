using System.ComponentModel.DataAnnotations;

namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO để lấy thông tin BalanceAuditLog (chỉ đọc)
/// </summary>
public class BalanceAuditLogDto
{
    public int LogId { get; set; }
    public int UId { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Reason { get; set; }
    public string RefId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Include related data
    public UserDto User { get; set; }
}

/// <summary>
/// DTO để tạo BalanceAuditLog
/// </summary>
public class CreateBalanceAuditLogDto
{
    [Required]
    public int UId { get; set; }

    public decimal ChangeAmount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    [Required]
    [StringLength(100)]
    public string Reason { get; set; }

    public int? RefId { get; set; }  // FIX: INT? thay vì string
}

/// <summary>
/// DTO BalanceAuditLog thông tin cơ bản (không có user details)
/// </summary>
public class BalanceAuditLogBasicDto
{
    public int LogId { get; set; }
    public int UId { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Reason { get; set; }
    public int? RefId { get; set; }  // FIX: INT? thay vì string
    public DateTime CreatedAt { get; set; }
}
/// <summary>
/// DTO để lọc lịch sử giao dịch
/// </summary>
public class BalanceAuditFilterDto
{
    public int UId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
