namespace ProductServiceInfrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using ProductServiceDomain.Entities;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.SKU).IsRequired().HasMaxLength(100);
            e.Property(p => p.Name).IsRequired().HasMaxLength(500);
            e.Property(p => p.Price).HasColumnType("numeric(18,4)");
            e.HasIndex(p => p.SKU).IsUnique();
        });
    }
}
