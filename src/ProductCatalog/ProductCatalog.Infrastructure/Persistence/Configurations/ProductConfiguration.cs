using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides configuration for the <see cref="Product"/> entity.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Configures the <see cref="Product"/> entity type.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Tabellenname
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Stock)
            .IsRequired();

        // Price als Value Object konfigurieren (für SQLite)
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(pr => pr.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)");

            priceBuilder.Property(pr => pr.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3);
        });

        // Domain Events nicht speichern
        builder.Ignore(p => p.DomainEvents);
    }
}