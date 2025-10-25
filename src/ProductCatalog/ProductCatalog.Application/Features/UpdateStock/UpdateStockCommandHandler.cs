using MediatR;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Repositories;

namespace ProductCatalog.Application.Features.UpdateStock;

/// <summary>
/// Handler for the UpdateStockCommand that updates a product's stock quantity.
/// </summary>
public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand>
{
    private readonly IProductRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStockCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    public UpdateStockCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the update stock command by updating the product's stock quantity.
    /// </summary>
    /// <param name="request">The update stock command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");
        }

        product.UpdateStock(request.NewStock);

        await _repository.UpdateAsync(product, cancellationToken);
    }
}