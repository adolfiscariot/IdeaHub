using System;
using System.ComponentModel.DataAnnotations;

namespace IdeaApp.Models;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email {get; set;}

    [Required]
    [DataType(DataType.Password)]
    public string Password {get; set;}

    [DataType(DataType.Password)]
    [Display(Name ="ConfirmPassword")]
    [Compare("Password", ErrorMessage ="Password & Confirmation Password don't match")]
    public string ConfirmPassword {get; set;}
}
