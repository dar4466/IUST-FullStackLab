using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Database-Level Validation: Ensure no two books have the exact same Title
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Title)
                .IsUnique();
        }
    }
}
