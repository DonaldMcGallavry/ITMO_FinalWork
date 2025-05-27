using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ITMO_FinalWork.Models;

[Table("RegistrationReg")]
public partial class RegistrationReg
{
    [Key]
    [Column("RecordID")]
    public int RecordID { get; set; }

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
    [RegularExpression(@"^[A-Za-zА-Яа-яёЁ\- ]+$",
        ErrorMessage = "Разрешены только буквы, дефисы и пробелы")]
    [Display(Name = "Отчество")]
    [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
    public string MiddleName { get; set; } = null!;

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [Display(Name = "Дата рождения")]
    public DateOnly BirthDate { get; set; }

    [Column("PassportID")]
    public int PassportId { get; set; }

    [Required(ErrorMessage = "Дата регистрации обязательна")]
    [Display(Name = "Дата регистрации")]
    public DateTime RegistrationDate { get; set; }

    [Required(ErrorMessage = "Адрес регистрации обязателен")]
    [Display(Name = "Адрес регистрации")]
    [StringLength(500, ErrorMessage = "Адрес не должен превышать 500 символов")]
    public string RegistrationAddress { get; set; } = null!;

    [Required]
    public string Status { get; set; } = "IsRegistrated";


    [Required(ErrorMessage = "Укажите пол")]
    [Display(Name = "Пол")]
    [StringLength(1)]
    [Unicode(false)]
    public string Gender { get; set; } = null!;

    [ForeignKey("PassportId")]
    [InverseProperty("RegistrationRegs")]
    public virtual Passport Passport { get; set; } = null!;
}
