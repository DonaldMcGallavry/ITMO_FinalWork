using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Models;

[Table("MigrationReg")]
public partial class MigrationReg
{
    [Key]
    [Column("MigrantID")]
    public int MigrantID { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна")]
    [RegularExpression(@"^[A-Za-zА-Яа-яёЁ\- ]+$",
               ErrorMessage = "Разрешены только буквы, дефисы и пробелы")]
    [Display(Name = "Фамилия")]
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
    public string LastName { get; set; } = null!;


    [Required(ErrorMessage = "Имя обязательно")]
    [RegularExpression(@"^[A-Za-zА-Яа-яёЁ\- ]+$",
        ErrorMessage = "Разрешены только буквы, дефисы и пробелы")]
    [Display(Name = "Имя")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Отчество обязательно")]
    [RegularExpression(@"^[A-Za-zА-Яа-яёЁ\- ]*$",
            ErrorMessage = "Разрешены только буквы, дефисы и пробелы")]
    [Display(Name = "Отчество")]
    [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
    public string MiddleName { get; set; } = null!;

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateOnly BirthDate { get; set; }

    [Required(ErrorMessage = "Укажите пол")]
    [Display(Name = "Пол")]
    [StringLength(1)]
    [Unicode(false)]
    public string Gender { get; set; } = null!;

    [Column("PassportID")]
    public int PassportId { get; set; }

    [Required(ErrorMessage = "Страна рождения обязательна")]
    [Display(Name = "Страна рождения")]
    [StringLength(100, ErrorMessage = "Название страны не должно превышать 100 символов")]
    public string BirthCountry { get; set; } = null!;

    [Required(ErrorMessage = "Гражданство обязательно")]
    [Display(Name = "Гражданство")]
    [StringLength(100, ErrorMessage = "Гражданство не должно превышать 100 символов")]
    public string Citizenship { get; set; } = null!;

    [Required(ErrorMessage = "Дата въезда обязательна")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата въезда")]
    public DateTime EntryDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Дата регистрации")]
    public DateTime RegistrationDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата окончания регистрации")]
    public DateTime? RegistrationEndDate { get; set; }

    [Column("ReasonID")]
    public int? ReasonId { get; set; }

    [Required(ErrorMessage = "Адрес регистрации обязателен")]
    [Display(Name = "Адрес регистрации")]
    [StringLength(500, ErrorMessage = "Адрес не должен превышать 500 символов")]
    public string RegistrationAddress { get; set; } = null!;

    [Column("CardID")]
    public int? CardId { get; set; } 

    [StringLength(20)]
    public string Status { get; set; } = "IsRegistrated";

    [ForeignKey("CardId")]
    [InverseProperty("MigrationRegs")]
    public  MigrationCard? Card { get; set; } = null!;

    [ForeignKey("PassportId")]
    [InverseProperty("MigrationRegs")]
    public  Passport Passport { get; set; } = null!;

    [ForeignKey("ReasonId")]
    [InverseProperty("MigrationRegs")]
    public  StayReason? Reason { get; set; } = null!;
}
