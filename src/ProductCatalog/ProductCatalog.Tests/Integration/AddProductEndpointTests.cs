using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ProductCatalog.Application.Features.AddProduct;
using ProductCatalog.Tests.Fixtures;

namespace ProductCatalog.Tests.Integration;

/// <summary>
/// Integration tests for the AddProduct endpoint.
/// </summary>
public class AddProductEndpointTests : IClassFixture<ProductCatalogWebApplicationFactory>
{
    private readonly ProductCatalogWebApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddProductEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory for the product catalog.</param>
    public AddProductEndpointTests(ProductCatalogWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Tests that adding a product with valid data returns a Created (201) response and a valid product ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddProduct_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new AddProductCommand(
            "Test Product",
            "Test Description",
            99.99m,
            "EUR",
            10);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("id", out var idElement).Should().BeTrue();
        idElement.GetString().Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that adding a product with invalid data returns a BadRequest (400) response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact(DisplayName = "AddProduct WithInvalidData ShouldReturnBadRequest")]
    public async Task AddProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidCommand = new AddProductCommand(
            string.Empty, // Invalid: empty name
            "Test Description",
            -10m, // Invalid: negative price
            "INVALID", // Invalid: wrong currency length
            -5); // Invalid: negative stock

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that adding a product with missing required fields returns a BadRequest (400) response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddProduct_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidCommand = new
        {
            // Missing all required fields
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that adding a product with invalid field values returns a BadRequest (400) response.
    /// </summary>
    /// <param name="name">The product name to test.</param>
    /// <param name="description">The product description to test.</param>
    /// <param name="priceAmount">The product price amount to test.</param>
    /// <param name="currency">The product currency to test.</param>
    /// <param name="initialStock">The initial stock value to test.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Theory]
    [InlineData("", "Description", 99.99, "EUR", 10)] // Empty name
    [InlineData("Product", "", 99.99, "EUR", 10)] // Empty description
    [InlineData("Product", "Description", 0, "EUR", 10)] // Zero price
    [InlineData("Product", "Description", 99.99, "EU", 10)] // Invalid currency length
    [InlineData("Product", "Description", 99.99, "EUR", -1)] // Negative stock
    public async Task AddProduct_WithInvalidFieldValues_ShouldReturnBadRequest(
        string name, string description, decimal priceAmount, string currency, int initialStock)
    {
        // Arrange
        var command = new AddProductCommand(name, description, priceAmount, currency, initialStock);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that adding a product with valid data creates the product in the database and returns a Created (201) response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddProduct_WithValidData_ShouldCreateProductInDatabase()
    {
        // Arrange
        var command = new AddProductCommand(
            "Database Test Product",
            "Database Test Description",
            199.99m,
            "USD",
            25);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify the product was created by checking the response location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/products/");
    }
}
