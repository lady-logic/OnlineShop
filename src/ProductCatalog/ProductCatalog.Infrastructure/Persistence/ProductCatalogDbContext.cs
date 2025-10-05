using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Persistence.Configurations;
using ProductCatalog.Infrastructure.Persistence.Outbox;

namespace ProductCatalog.Infrastructure.Persistence;

/// <summary>
/// Database context for the Product Catalog, providing access to product entities.
/// </summary>
public class ProductCatalogDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductCatalogDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets the outbox messages DbSet.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
