using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProductCatalog.Application.Features.AddProduct;

/// <summary>
/// Command to add a new product to the catalog.
/// </summary>
public record AddProductCommand(
    /// <summary>
    /// Product name (max 100 characters)
    /// </summary>
    /// <example>iPhone 15 Pro</example>
    [Required]
    string Name,

    /// <summary>
    /// Product description (max 500 characters)
    /// </summary>
    /// <example>Latest Apple smartphone with titanium design and advanced camera system</example>
    [Required]
    string Description,

    /// <summary>
    /// Price amount (must be greater than 0)
    /// </summary>
    /// <example>1199.99</example>
    [Required]
    decimal PriceAmount,

    /// <summary>
    /// Currency code (3 characters)
    /// </summary>
    /// <example>EUR</example>
    [Required]
    string Currency,

    /// <summary>
    /// Initial stock quantity (must be >= 0)
    /// </summary>
    /// <example>25</example>
    [Required]
    int InitialStock) : IRequest<Guid>;
