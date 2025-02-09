using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdeaApp.Models;

public class AppUser : IdentityUser
{
    public string FirstName {get; set;}

    public string LastName {get; set;}

    public ICollection<Idea> Ideas {get; set;}
}
