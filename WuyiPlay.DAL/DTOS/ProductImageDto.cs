using System.ComponentModel.DataAnnotations;

namespace WuyiPlay_DAL.DTOS;
/// <summary>
/// DTO để lấy thông tin ProductImage (chỉ đọc)
/// </summary>
public class ProductImageDto
{
    public int ImgId { get; set; }
    public int PId { get; set; }
    public string ImageUrl { get; set; }
    public int IsThumbnail { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO để tạo ProductImage mới
/// </summary>
public class CreateProductImageDto
{
    [Required]
    public int PId { get; set; }

    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; }

    public int IsThumbnail { get; set; } = 0;
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// DTO để cập nhật ProductImage
/// </summary>
public class UpdateProductImageDto
{
    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; }

    public int IsThumbnail { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// DTO để upload hình ảnh sản phẩm (nhận file)
/// </summary>
public class UploadProductImageDto
{
    public int PId { get; set; }
    public byte[] ImageData { get; set; }      // nội dung file
    public string FileName { get; set; }        // tên file gốc
    public string ContentType { get; set; }     // "image/jpeg", "image/png"...
    public int IsThumbnail { get; set; } = 0;
    public int SortOrder { get; set; } = 0;
}


