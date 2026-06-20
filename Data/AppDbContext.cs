using Api_Loggin.Models;
using Microsoft.EntityFrameworkCore;

namespace Api_Loggin.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).HasDefaultValue("User");
            });
        }
    }
}
