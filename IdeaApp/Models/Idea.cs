using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaApp.Models;

public class Idea
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string Content {get; set;}
    public DateOnly DateWritten {get; set;}
    public string UserId {get; set;}
    public int VoteCount {get; set;} = 0;
    public string? Voters {get; set;}

    //Navigation Property
    public AppUser Author {get; set;} //An idea comes from 1 user

}
