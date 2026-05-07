using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("categories")]
public partial class Category
{
    [Key]
    [Column("cID")]
    public int CId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("CIdNavigation")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
