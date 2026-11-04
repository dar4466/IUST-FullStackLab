using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;
namespace MyMvcApp.Data
{
public class AppDbContext : DbContext
{
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
{
}
// This property represents the table in your database
public DbSet<Book> Books { get; set; }
}
}