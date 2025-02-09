using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace IdeaApp.Models;

public class RegisterViewModel
{
    [Required]
    [DataType(DataType.Text)]
    public string FirstName {get; set;}

    [Required]
    [DataType(DataType.Text)]
    public string LastName {get; set;}

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
