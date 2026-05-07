using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("orders")]
public partial class Order
{
    [Key]
    [Column("oID")]
    public int OId { get; set; }

    [Column("uID")]
    public int UId { get; set; }

    [Column("pID")]
    public int PId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Column("payment_method")]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [Column("createdAt", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("PId")]
    [InverseProperty("Orders")]
    public virtual Product PIdNavigation { get; set; } = null!;

    [ForeignKey("UId")]
    [InverseProperty("Orders")]
    public virtual User UIdNavigation { get; set; } = null!;
}
