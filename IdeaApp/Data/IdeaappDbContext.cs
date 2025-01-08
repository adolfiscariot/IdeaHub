using System;
using IdeaApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdeaApp.Data;
using IdeaApp.Models;

public class IdeaappDbContext : IdentityDbContext<AppUser>
{
    public virtual DbSet<AppUser> Users {get; set;}
    public virtual DbSet<Idea> Ideas {get; set;}

    public IdeaappDbContext(DbContextOptions<IdeaappDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //AppUser r/ship with others
        builder.Entity<AppUser>(entity => {

            //Primary Key
            entity.HasKey(u => u.Id);

            //Relationships
            entity.HasMany(u => u.Ideas)
                .WithOne(i => i.Author)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Idea>(entity => {

            //primary key
            entity.HasKey(i => i.Id);

            //Properties
            entity.Property(i => i.Name)
                .IsRequired();

            entity.Property(i => i.Content)
                .IsRequired();

            entity.Property(i => i.DateWritten)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_DATE");

            entity.Property(i => i.VoteCount);
                
            entity.Property(i => i.Voters)
                .IsRequired(false)
                .HasColumnType("text")
                .HasDefaultValue(null);

            //Relationships
            entity.HasOne(i => i.Author)
                .WithMany(u => u.Ideas)
                .HasForeignKey(i => i.UserId);
        });
    }
}
