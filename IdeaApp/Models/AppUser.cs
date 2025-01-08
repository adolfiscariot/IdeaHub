using System;
using Microsoft.AspNetCore.Identity;

namespace IdeaApp.Models;

public class AppUser : IdentityUser
{
    public ICollection<Idea> Ideas {get; set;}
}
