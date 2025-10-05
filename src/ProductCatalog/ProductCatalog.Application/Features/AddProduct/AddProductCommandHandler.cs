using MediatR;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AddProductCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    public AddProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the add product command by creating a new product and persisting it.
    /// </summary>
    /// <param name="request">The add product command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the created product.</returns>
    public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var price = new Price(request.PriceAmount, request.Currency);
        var product = Product.Create(
            request.Name,
            request.Description,
            price,
            request.InitialStock);

        await _repository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
