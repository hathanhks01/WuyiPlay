using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WuyiPlay_DAL.Models;

[Table("balance_audit_log")]
public partial class BalanceAuditLog
{
    [Key]
    [Column("logID")]
    public int LogId { get; set; }

    [Column("uID")]
    public int UId { get; set; }

    [Column("change_amount", TypeName = "decimal(18, 2)")]
    public decimal ChangeAmount { get; set; }

    [Column("balance_before", TypeName = "decimal(18, 2)")]
    public decimal BalanceBefore { get; set; }

    [Column("balance_after", TypeName = "decimal(18, 2)")]
    public decimal BalanceAfter { get; set; }

    [Column("reason")]
    [StringLength(100)]
    public string Reason { get; set; } = null!;

    [Column("ref_id")]
    public int? RefId { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UId")]
    [InverseProperty("BalanceAuditLogs")]
    public virtual User UIdNavigation { get; set; } = null!;
}
