using System.ComponentModel.DataAnnotations;

namespace IdeaApp.Models;

//create role model
public class CreateRoleViewModel {
    //role name
    [Required]
    [Display(Name = "Role")]
    public string RoleName {get; set;}
}