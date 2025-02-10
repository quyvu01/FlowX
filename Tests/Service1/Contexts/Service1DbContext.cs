using Microsoft.EntityFrameworkCore;
using Service1.Models;

namespace Service1.Contexts;

public sealed class Service1DbContext(DbContextOptions<Service1DbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(a => a.HasKey(u => u.Id));
        base.OnModelCreating(modelBuilder);
    }
}