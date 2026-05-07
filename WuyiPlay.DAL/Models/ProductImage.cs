using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("product_images")]
public partial class ProductImage
{
    [Key]
    [Column("imgID")]
    public int ImgId { get; set; }

    [Column("pID")]
    public int PId { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; } = null!;

    public int IsThumbnail { get; set; }

    public int SortOrder { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("PId")]
    [InverseProperty("ProductImages")]
    public virtual Product PIdNavigation { get; set; } = null!;
}
