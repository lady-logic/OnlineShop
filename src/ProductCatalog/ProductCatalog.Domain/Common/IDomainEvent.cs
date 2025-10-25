using MediatR;

namespace ProductCatalog.Domain.Common;

/// <summary>
/// Represents a domain event that occurred within the domain model.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    Guid Id => Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when the domain event occurred.
    /// </summary>
    DateTime OccurredOn => DateTime.UtcNow;
}
