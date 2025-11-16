using MediatR;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Repositories;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Features.AddProduct;

/// <summary>
/// Handler for the AddProductCommand that creates a new product in the catalog.
/// </summary>
public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly IProductRepository _repository;
    private readonly ILogger<AddProductCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddProductCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    public AddProductCommandHandler(IProductRepository repository, ILogger<AddProductCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the add product command by creating a new product and persisting it.
    /// </summary>
    /// <param name="request">The add product command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the created product.</returns>
    public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new product: {ProductName}", request.Name);

        try
        {
            var price = new Price(request.PriceAmount, request.Currency);
            var product = Product.Create(
                request.Name,
                request.Description,
                price,
                request.InitialStock);

            await _repository.AddAsync(product, cancellationToken);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            return product.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", request.Name);
            throw;
        }
    }
}
