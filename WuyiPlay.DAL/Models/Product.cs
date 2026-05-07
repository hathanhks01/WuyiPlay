using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("products")]
public partial class Product
{
    [Key]
    [Column("pID")]
    public int PId { get; set; }

    [Column("cID")]
    public int? CId { get; set; }

    [Column("p_code")]
    [StringLength(30)]
    [Unicode(false)]
    public string PCode { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(250)]
    public string Password { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostPrice { get; set; }

    public int Status { get; set; }

    [Column("describe")]
    [StringLength(500)]
    public string? Describe { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("soldAt", TypeName = "datetime")]
    public DateTime? SoldAt { get; set; }

    [ForeignKey("CId")]
    [InverseProperty("Products")]
    public virtual Category? CIdNavigation { get; set; }

    [InverseProperty("PIdNavigation")]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [InverseProperty("PIdNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("PIdNavigation")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
