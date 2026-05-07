namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO để lấy thông tin Product (chỉ đọc)
/// </summary>
public class ProductDto
{
    public int PId { get; set; }
    public int CId { get; set; }
    public string PCode { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SoldAt { get; set; }

    // Include related data
    public CategoryBasicDto Category { get; set; }
    public List<ProductImageDto> ProductImages { get; set; } = new();
}

/// <summary>
/// DTO để tạo Product mới
/// </summary>
public class CreateProductDto
{
    public int CId { get; set; }

    /// <summary>Tên đăng nhập game (thông tin bí mật — chỉ hiển thị sau khi mua)</summary>
    public string UserName { get; set; }

    /// <summary>Mật khẩu game (thông tin bí mật — chỉ hiển thị sau khi mua)</summary>
    public string Password { get; set; }

    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int Status { get; set; } = 1;

    /// <summary>
    /// Mã sản phẩm tuỳ chỉnh — nếu để trống, hệ thống tự sinh WP-XXXXXX
    /// </summary>
    public string? PCode { get; set; }
}

/// <summary>
/// DTO để cập nhật Product
/// </summary>
public class UpdateProductDto
{
    public int CId { get; set; }

    /// <summary>
    /// Mã hiển thị sản phẩm — sẽ cập nhật PCode trong DB.
    /// </summary>
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int Status { get; set; }
}

/// <summary>
/// DTO Product thông tin cơ bản (dùng trong danh sách, giỏ hàng, đơn hàng)
/// </summary>
public class ProductBasicDto
{
    public int PId { get; set; }
    public int CId { get; set; }

    /// <summary>Mã sản phẩm hiển thị — VD: WP-000001</summary>
    public string PCode { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductImageDto> ProductImages { get; set; } = new();
    public CategoryBasicDto Category { get; set; }
}
