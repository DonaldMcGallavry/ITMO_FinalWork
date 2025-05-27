using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Models;

[Index("Series", "Number", Name = "IX_Passports_SeriesNumber")]
[Index("Series", "Number", Name = "UQ_Passport_SeriesNumber", IsUnique = false)]
public partial class Passport
{
    [Key]
    [Column("PassportID")]
    public int PassportID { get; set; }

    [Required(ErrorMessage = "Серия паспорта обязательна")]
    [RegularExpression(@"^[a-zA-Z0-9]*$",
    ErrorMessage = "Серия паспорта может содержать только буквы и цифры")]
    [Display(Name = "Серия паспорта")]
    public string Series { get; set; } = null!;

    [Required(ErrorMessage = "Номер паспорта обязателен")]
    [RegularExpression(@"^[a-zA-Z0-9]*$",
    ErrorMessage = "Номер паспорта может содержать только буквы и цифры")]
    [Display(Name = "Номер паспорта")]
    public string Number { get; set; } = null!;

    [Required(ErrorMessage = "Дата выдачи обязательна")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата выдачи")]
    public DateTime IssueDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата окончания действия")]
    public DateTime? ExpiryDate { get; set; }

    [InverseProperty("Passport")]
    public virtual ICollection<MigrationReg> MigrationRegs { get; set; } = new List<MigrationReg>();

    [InverseProperty("Passport")]
    public virtual ICollection<RegistrationReg> RegistrationRegs { get; set; } = new List<RegistrationReg>();
}
