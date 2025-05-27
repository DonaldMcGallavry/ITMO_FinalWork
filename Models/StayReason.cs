using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Models;

public partial class StayReason
{
    [Key]
    [Column("ReasonID")]
    public int ReasonID { get; set; }

    [Required(ErrorMessage = "Цель визита обязательна")]
    [Display(Name = "Цель визита")]
    [StringLength(15)]
    public string ReasonType { get; set; } = null!;
    
    [Required(ErrorMessage = "Тип документа обязателен")]
    [Display(Name = "Тип документа")]
    [StringLength(25)]
    public string DocumentType { get; set; } = null!;

    [InverseProperty("Reason")]
    public virtual ICollection<MigrationReg> MigrationRegs { get; set; } = new List<MigrationReg>();
}
