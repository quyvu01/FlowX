using Microsoft.EntityFrameworkCore;
using Service2.Models;

namespace Service2.Contexts;

public sealed class Service2DbContext(DbContextOptions<Service2DbContext> options) : DbContext(options)
{
    public DbSet<Province> Provinces { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Province>(a => a.HasKey(u => u.Id));
        base.OnModelCreating(modelBuilder);
    }
}