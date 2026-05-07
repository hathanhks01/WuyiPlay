namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO để lấy thông tin Order (chỉ đọc)
/// </summary>
public class OrderDto
{
    public int OId { get; set; }
    public int UId { get; set; }
    public int PId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Thông tin tài khoản game (Chỉ trả về cho khách sau khi mua)
    public string? AccountUsername { get; set; }
    public string? AccountPassword { get; set; }
    
    // Include related data
    public UserDto User { get; set; }
    public ProductBasicDto Product { get; set; }
}

/// <summary>
/// DTO để tạo Order mới
/// </summary>
public class CreateOrderDto
{
    public int UId { get; set; }
    public int PId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Ví WuyiPlay"; // Default payment method
}

/// <summary>
/// DTO Order thông tin cơ bản (không có user/product details)
/// </summary>
public class OrderBasicDto
{
    public int OId { get; set; }
    public int UId { get; set; }
    public int PId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Thông tin tài khoản game
    public string? AccountUsername { get; set; }
    public string? AccountPassword { get; set; }
    
}

/// <summary>
/// DTO để tạo Order từ Cart (checkout)
/// </summary>
public class CheckoutDto
{
    public int UId { get; set; }
    public List<int> CartItemIds { get; set; } // List of CartId to checkout
    public string PaymentMethod { get; set; } = "Ví WuyiPlay";
}
