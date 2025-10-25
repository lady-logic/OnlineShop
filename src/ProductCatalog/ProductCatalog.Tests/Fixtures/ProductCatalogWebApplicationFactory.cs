using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.Tests.Fixtures;

/// <summary>
/// Web application factory for integration testing.
/// </summary>
public class ProductCatalogWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Initializes the test database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Cleans up the test database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        await context.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Configures the web host builder for testing.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database context
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProductCatalogDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database
            services.AddDbContext<ProductCatalogDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });

        builder.UseEnvironment("Testing");
    }
}
