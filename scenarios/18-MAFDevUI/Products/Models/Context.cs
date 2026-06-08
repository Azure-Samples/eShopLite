using DataEntities;
using Microsoft.EntityFrameworkCore;

namespace Products.Models;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Product => Set<Product>();
    public DbSet<Location> Location => Set<Location>();
    public DbSet<Customer> Customer => Set<Customer>();
    public DbSet<Discount> Discount => Set<Discount>();
    public DbSet<ProductsByLocation> ProductsByLocation => Set<ProductsByLocation>();
    public DbSet<PurchaseHistory> PurchaseHistory => Set<PurchaseHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProductsByLocation relationships
        modelBuilder.Entity<ProductsByLocation>()
            .HasOne(pl => pl.Product)
            .WithMany()
            .HasForeignKey(pl => pl.ProductId);

        modelBuilder.Entity<ProductsByLocation>()
            .HasOne(pl => pl.Location)
            .WithMany()
            .HasForeignKey(pl => pl.LocationId);

        // Configure PurchaseHistory relationships
        modelBuilder.Entity<PurchaseHistory>()
            .HasOne(ph => ph.Customer)
            .WithMany()
            .HasForeignKey(ph => ph.CustomerId);

        modelBuilder.Entity<PurchaseHistory>()
            .HasOne(ph => ph.Product)
            .WithMany()
            .HasForeignKey(ph => ph.ProductId);

        modelBuilder.Entity<PurchaseHistory>()
            .HasOne(ph => ph.Location)
            .WithMany()
            .HasForeignKey(ph => ph.LocationId);
    }
}
