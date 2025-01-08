using System;
using IdeaApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdeaApp.Data;

public class IdeaappDbContextFactory : IDesignTimeDbContextFactory<IdeaappDbContext>
{
    public IdeaappDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdeaappDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost; Port=5432; Database=newideahub; User Id=mark; Password=markothengo99; Include Error Detail=true;");
        return new IdeaappDbContext(optionsBuilder.Options);
    }

}
