using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("users")]
[Index("Phone", Name = "UQ__users__5C7E359E7B705225", IsUnique = true)]
[Index("Email", Name = "UQ__users__A9D105342501D4C6", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("uID")]
    public int UId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(250)]
    public string HashPassword { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string Phone { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Balance { get; set; }

    [Column("is_deleted")]
    public int IsDeleted { get; set; }

    public int Role { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedAt", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("UIdNavigation")]
    public virtual ICollection<BalanceAuditLog> BalanceAuditLogs { get; set; } = new List<BalanceAuditLog>();

    [InverseProperty("UIdNavigation")]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [InverseProperty("UIdNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
