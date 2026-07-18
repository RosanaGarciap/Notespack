using Microsoft.EntityFrameworkCore;
using System;
using NOTESPACK.Models; // 👈 This line fixes error CS0246

namespace Notespack.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Main Tables
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Event> Events { get; set; } // 👈 This will compile successfully now
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                username = "campus_admin",
                Email = "admin@byui.edu",
                Password = "SecurePassword123",
                EnrollmentDate = DateTime.Today
            },
            new User { 
                Id = 2,
                username = "Juan_Q",
                Email = "JQVallenilla@gmail.com",
                Password = "Main123-\",",
                EnrollmentDate = DateTime.Today 
            },
            new User { 
                Id = 3,
                username = "Carolina2023",
                Email = "JQVallenilla@gmail.com",
                Password = "Main123-\",",
                EnrollmentDate = DateTime.Today 
            }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Travel"},
            new Category { Id = 2, Name = "Meeting"},
            new Category { Id = 3, Name = "Lunch"},
            new Category { Id = 4, Name = "Break"},
            new Category { Id = 5, Name = "Course"}
        );
    }
}