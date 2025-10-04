using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Repositories;

/// <summary>
/// Repository interface for managing product entities.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the product if found, otherwise null.
    /// </returns>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all products in the catalog.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of all products.
    /// </returns>
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets all available products (with stock &gt; 0).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of available products.
    /// </returns>
    Task<IEnumerable<Product>> GetAvailableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new product to the catalog.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing product in the catalog.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a product from the catalog.
    /// </summary>
    /// <param name="id">The identifier of the product to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
