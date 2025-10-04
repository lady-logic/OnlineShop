using MediatR;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Repositories;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Features.ChangePrice;

/// <summary>
/// Handler for the ChangePriceCommand that updates a product's price.
/// </summary>
public class ChangePriceCommandHandler : IRequestHandler<ChangePriceCommand>
{
    private readonly IProductRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePriceCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    public ChangePriceCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the change price command by updating the product's price.
    /// </summary>
    /// <param name="request">The change price command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Handle(ChangePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");
        }

        var newPrice = new Price(request.NewPriceAmount, request.Currency);
        product.ChangePrice(newPrice);

        await _repository.UpdateAsync(product, cancellationToken);
    }
}