using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("cart")]
[Index("UId", "PId", Name = "UQ_cart_user_product", IsUnique = true)]
public partial class Cart
{
    [Key]
    [Column("cartID")]
    public int CartId { get; set; }

    [Column("uID")]
    public int UId { get; set; }

    [Column("pID")]
    public int PId { get; set; }

    [Column("addedAt", TypeName = "datetime")]
    public DateTime AddedAt { get; set; }

    [ForeignKey("PId")]
    [InverseProperty("Carts")]
    public virtual Product PIdNavigation { get; set; } = null!;

    [ForeignKey("UId")]
    [InverseProperty("Carts")]
    public virtual User UIdNavigation { get; set; } = null!;
}
