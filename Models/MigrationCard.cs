using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Models;

public partial class MigrationCard
{
    [Key]
    [Column("CardID")]

    [Required]
    public int CardID { get; set; }

    [Required(ErrorMessage = "Серия обязательна")]
    [Display(Name = "Серия")]
    [StringLength(5)]
    public string CardSeries { get; set; } = null!;

    [Required(ErrorMessage = "Номер обязателен")]
    [Display(Name = "Номер")]
    [StringLength(20)]
    public string CardNumber { get; set; } = null!;

    [Required(ErrorMessage = "Дата создания обязательна")]
    [Display(Name = "Дата создания")]
    [DataType(DataType.Date)]
    public DateTime IssueDate { get; set; }

    [Display(Name = "Срок действия")]
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

    [InverseProperty("Card")]
    public virtual ICollection<MigrationReg> MigrationRegs { get; set; } = new List<MigrationReg>();
}
