namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO để lấy thông tin User (chỉ đọc)
/// </summary>
public class UserDto
{
    public int UId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public decimal Balance { get; set; }
    public int Role { get; set; }
    public int IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO để tạo User mới
/// </summary>
public class CreateUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int Role { get; set; } = 2; // Default: Customer
    public decimal InitialBalance { get; set; } = 0;
}

/// <summary>
/// DTO để cập nhật thông tin User
/// </summary>
public class UpdateUserDto
{
    public string Email { get; set; }
    public string Phone { get; set; }
    public int Role { get; set; }
}

/// <summary>
/// DTO để cập nhật mật khẩu
/// </summary>
public class ChangePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

/// <summary>
/// DTO để cập nhật Balance
/// </summary>
public class UpdateBalanceDto
{
    public decimal Amount { get; set; }
    public string Reason { get; set; }
    public int? RefId { get; set; } // Reference ID (e.g., order ID) - optional
}
