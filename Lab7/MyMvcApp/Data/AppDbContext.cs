using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Data
{
    // Inherit from IdentityDbContext instead of DbContext
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRITICAL: Must be called first to generate Identity tables
            base.OnModelCreating(modelBuilder);

            // Preserved from your existing configuration: Ensure no two books have the exact same Title
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Title)
                .IsUnique();
        }
    }
}