using Microsoft.EntityFrameworkCore;
using ORMapperSample.Models;

namespace ORMapperSample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.CategoryId).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // シードデータ
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "電子機器" },
            new Category { Id = 2, Name = "書籍" },
            new Category { Id = 3, Name = "日用品" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "サンプル商品1",
                Description = "これはサンプル商品1の説明です",
                Price = 1000,
                Quantity = 10,
                CategoryId = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = 2,
                Name = "サンプル商品2",
                Description = "これはサンプル商品2の説明です",
                Price = 2000,
                Quantity = 5,
                CategoryId = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Product && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var product = (Product)entry.Entity;
            product.UpdatedAt = DateTime.Now;

            if (entry.State == EntityState.Added)
            {
                product.CreatedAt = DateTime.Now;
            }
        }
    }
}