using System.ComponentModel.DataAnnotations;

namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO để lấy thông tin Cart Item (chỉ đọc)
/// </summary>
public class CartDto
{
    public int CartId { get; set; }
    public int UId { get; set; }
    public int PId { get; set; }
    public DateTime AddedAt { get; set; }
    public ProductBasicDto Product { get; set; }
}

/// <summary>
/// DTO để thêm sản phẩm vào Cart
/// </summary>

public class AddToCartDto
{
    [Required]
    public int UId { get; set; }

    [Required]
    public int PId { get; set; }
}

/// <summary>
/// DTO Cart thông tin cơ bản (không có product details)
/// </summary>
public class CartBasicDto
{
    public int CartId { get; set; }
    public int UId { get; set; }
    public int PId { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// DTO để lấy danh sách giỏ hàng của người dùng
/// </summary>
public class CartSummaryDto
{
    public int UId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalPrice { get; set; }
    public List<CartDto> Items { get; set; } = new();
}
