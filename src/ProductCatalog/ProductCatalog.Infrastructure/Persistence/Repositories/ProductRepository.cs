using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Repositories;

namespace ProductCatalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework implementation of the product repository.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductCatalogDbContext _context;
    private readonly ILogger<ProductRepository> _logger;
    private const int MaxRetries = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ProductRepository(ProductCatalogDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The product if found, otherwise null.</returns>
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (retryCount <= MaxRetries)
        {
            try
            {
                return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount > MaxRetries ||
                    !(ex.Message.Contains("database is locked") || ex is SqliteException))
                {
                    _logger.LogError(ex, "Failed to get product by ID after {Retries} attempts", retryCount);
                    throw;
                }

                _logger.LogWarning(ex, "Database error in GetByIdAsync, retrying {RetryCount}/{MaxRetries}...",
                                retryCount, MaxRetries);

                await Task.Delay(200 * retryCount, cancellationToken);
            }
        }

        return null; // Dieser Punkt sollte nie erreicht werden, aber der Compiler erfordert einen Return-Wert
    }

    /// <summary>
    /// Gets all products in the catalog.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of all products.</returns>
    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (retryCount <= MaxRetries)
        {
            try
            {
                return await _context.Products.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount > MaxRetries ||
                    !(ex.Message.Contains("database is locked") || ex is SqliteException))
                {
                    _logger.LogError(ex, "Failed to get all products after {Retries} attempts", retryCount);
                    throw;
                }

                _logger.LogWarning(ex, "Database error in GetAllAsync, retrying {RetryCount}/{MaxRetries}...",
                                retryCount, MaxRetries);

                await Task.Delay(200 * retryCount, cancellationToken);
            }
        }

        return Enumerable.Empty<Product>(); // Dieser Punkt sollte nie erreicht werden
    }

    /// <summary>
    /// Gets all available products (with stock > 0).
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of available products.</returns>
    public async Task<IEnumerable<Product>> GetAvailableAsync(CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (retryCount <= MaxRetries)
        {
            try
            {
                return await _context.Products
                    .Where(p => p.Stock > 0)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount > MaxRetries ||
                    !(ex.Message.Contains("database is locked") || ex is SqliteException))
                {
                    _logger.LogError(ex, "Failed to get available products after {Retries} attempts", retryCount);
                    throw;
                }

                _logger.LogWarning(ex, "Database error in GetAvailableAsync, retrying {RetryCount}/{MaxRetries}...",
                                retryCount, MaxRetries);

                await Task.Delay(200 * retryCount, cancellationToken);
            }
        }

        return Enumerable.Empty<Product>(); // Dieser Punkt sollte nie erreicht werden
    }


    /// <summary>
    /// Adds a new product to the catalog.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        int retryCount = 0;
        const int maxRetries = 3;

        while (retryCount <= maxRetries)
        {
            try
            {
                await _context.Products.AddAsync(product, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return; // Erfolgreich - beenden
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount > maxRetries ||
                    !(ex.Message.Contains("database is locked") || ex is Microsoft.Data.Sqlite.SqliteException))
                {
                    // Bei maxRetries erreicht oder nicht behandelbaren Fehlern
                    _logger.LogError(ex, "Failed to add product after {Retries} attempts", retryCount);
                    throw; 
                }

                _logger.LogWarning(ex, "Database error, retrying {RetryCount}/{MaxRetries}...",
                                  retryCount, maxRetries);

                await Task.Delay(200 * retryCount, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Updates an existing product in the catalog.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (retryCount <= MaxRetries)
        {
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount > MaxRetries ||
                    !(ex.Message.Contains("database is locked") || ex is SqliteException))
                {
                    _logger.LogError(ex, "Failed to update product after {Retries} attempts", retryCount);
                    throw;
                }

                _logger.LogWarning(ex, "Database error in UpdateAsync, retrying {RetryCount}/{MaxRetries}...",
                                retryCount, MaxRetries);

                await Task.Delay(200 * retryCount, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Deletes a product from the catalog.
    /// </summary>
    /// <param name="id">The identifier of the product to delete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (retryCount <= MaxRetries)
        {
            try
            {
                var product = await GetByIdAsync(id, cancellationToken);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return;
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount > MaxRetries ||
                    !(ex.Message.Contains("database is locked") || ex is SqliteException))
                {
                    _logger.LogError(ex, "Failed to delete product after {Retries} attempts", retryCount);
                    throw;
                }

                _logger.LogWarning(ex, "Database error in DeleteAsync, retrying {RetryCount}/{MaxRetries}...",
                                retryCount, MaxRetries);

                await Task.Delay(200 * retryCount, cancellationToken);
            }
        }
    }
}
