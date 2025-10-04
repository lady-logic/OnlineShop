using ProductCatalog.Domain.Common;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Domain.Entities;

/// <summary>
/// Represents a product in the catalog with pricing and stock management capabilities.
/// </summary>
public class Product : AggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Product"/> class.
    /// </summary>
    private Product()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Product"/> class with specified values.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="name">The product name.</param>
    /// <param name="description">The product description.</param>
    /// <param name="price">The product price.</param>
    /// <param name="stock">The initial stock quantity.</param>
    private Product(Guid id, string name, string description, Price price, int stock)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
    }

    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of the product.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the price of the product.
    /// </summary>
    public Price Price { get; private set; } = Price.Zero;

    /// <summary>
    /// Gets the current stock quantity of the product.
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the product is available (has stock).
    /// </summary>
    public bool IsAvailable => Stock > 0;

    /// <summary>
    /// Creates a new product with the specified details.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="description">The product description.</param>
    /// <param name="price">The product price.</param>
    /// <param name="initialStock">The initial stock quantity.</param>
    /// <returns>A new product instance.</returns>
    public static Product Create(string name, string description, Price price, int initialStock)
    {
        // EntityTypeConfiguration nachlesen -> Konfiguration
        // SequentialGuid nachlesen
        // Validation ergänzen
        // Warum private Konstruktoren? Factory Method Pattern?
        var product = new Product(Guid.NewGuid(), name, description, price, initialStock);
        product.AddDomainEvent(new ProductAdded(product.Id, name, description, price.Amount, price.Currency, initialStock));
        return product;
    }

    /// <summary>
    /// Changes the price of the product and raises a price changed event.
    /// </summary>
    /// <param name="newPrice">The new price for the product.</param>
    public void ChangePrice(Price newPrice)
    {
        var oldPrice = Price;
        Price = newPrice;
        // Validation ergänzen
        //if()
        AddDomainEvent(new PriceChanged(Id, oldPrice.Amount, newPrice.Amount, newPrice.Currency));
    }

    /// <summary>
    /// Updates the stock quantity and raises appropriate events.
    /// </summary>
    /// <param name="newStock">The new stock quantity.</param>
    public void UpdateStock(int newStock)
    {
        var oldStock = Stock;
        Stock = newStock;
        AddDomainEvent(new StockUpdated(Id, oldStock, newStock));

        if (oldStock > 0 && newStock == 0)
        {
            AddDomainEvent(new ProductBecameUnavailable(Id));
        }
    }
}
