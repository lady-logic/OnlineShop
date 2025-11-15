using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalog.Application.Features.AddProduct;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Repositories;

namespace ProductCatalog.Tests.Unit.Application;

/// <summary>
/// Unit tests for the AddProductCommandHandler.
/// </summary>
public class AddProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly AddProductCommandHandler _handler;
    private readonly Mock<ILogger<AddProductCommandHandler>> _loggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddProductCommandHandlerTests"/> class.
    /// </summary>
    public AddProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<AddProductCommandHandler>>();
        _handler = new AddProductCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that a valid <see cref="AddProductCommand"/> creates a product and returns its ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductAndReturnId()
    {
        // Arrange
        var command = new AddProductCommand(
            "Test Product",
            "Test Description",
            99.99m,
            "EUR",
            10);

        Product? capturedProduct = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), CancellationToken.None))
            .Callback<Product>(p => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedProduct.Should().NotBeNull();
        capturedProduct!.Name.Should().Be(command.Name);
        capturedProduct.Description.Should().Be(command.Description);
        capturedProduct.Price.Amount.Should().Be(command.PriceAmount);
        capturedProduct.Price.Currency.Should().Be(command.Currency);
        capturedProduct.Stock.Should().Be(command.InitialStock);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), CancellationToken.None), Times.Once);
    }

    /// <summary>
    /// Tests that a valid <see cref="AddProductCommand"/> raises a <see cref="ProductCatalog.Domain.Events.ProductAdded"/> domain event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WithValidCommand_ShouldRaiseProductAddedEvent()
    {
        // Arrange
        var command = new AddProductCommand(
            "Test Product",
            "Test Description",
            99.99m,
            "EUR",
            10);

        Product? capturedProduct = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), CancellationToken.None))
            .Callback<Product>(p => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.DomainEvents.Should().HaveCount(1);
        var domainEvent = capturedProduct.DomainEvents.First() as ProductCatalog.Domain.Events.ProductAdded;
        domainEvent.Should().NotBeNull();
        domainEvent!.Name.Should().Be(command.Name);
        domainEvent.Description.Should().Be(command.Description);
        domainEvent.Price.Should().Be(command.PriceAmount);
        domainEvent.Currency.Should().Be(command.Currency);
        domainEvent.InitialStock.Should().Be(command.InitialStock);
    }

    /// <summary>
    /// Tests that a valid <see cref="AddProductCommand"/> with different currencies creates the correct price.
    /// </summary>
    /// <param name="currency">The currency code to test.</param>
    /// <param name="amount">The price amount to test.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("USD", 149.99)]
    [InlineData("GBP", 79.99)]
    [InlineData("JPY", 15000)]
    public async Task Handle_WithDifferentCurrencies_ShouldCreateCorrectPrice(string currency, decimal amount)
    {
        // Arrange
        var command = new AddProductCommand(
            "Test Product",
            "Test Description",
            amount,
            currency,
            5);

        Product? capturedProduct = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), CancellationToken.None))
            .Callback<Product>(p => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.Price.Currency.Should().Be(currency);
        capturedProduct.Price.Amount.Should().Be(amount);
    }

    /// <summary>
    /// Tests that when the repository throws an exception during product addition,
    /// the exception is propagated by the handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new AddProductCommand(
            "Test Product",
            "Test Description",
            99.99m,
            "EUR",
            10);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
