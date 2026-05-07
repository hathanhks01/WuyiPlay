namespace WuyiPlay_DAL.DTOS;

/// <summary>
/// DTO để lấy thông tin Category (chỉ đọc)
/// </summary>
public class CategoryDto
{
    public int CId { get; set; }
    public string Name { get; set; }
    
    // Include related products
    public List<ProductBasicDto> Products { get; set; } = new();
}

/// <summary>
/// DTO để tạo Category mới
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; }
}

/// <summary>
/// DTO để cập nhật Category
/// </summary>
public class UpdateCategoryDto
{
    public string Name { get; set; }
}

/// <summary>
/// DTO Category thông tin cơ bản (không có products)
/// </summary>
public class CategoryBasicDto
{
    public int CId { get; set; }
    public string Name { get; set; }
}
