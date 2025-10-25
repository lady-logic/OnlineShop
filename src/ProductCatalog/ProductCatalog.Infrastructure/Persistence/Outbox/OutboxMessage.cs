namespace ProductCatalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Represents a domain event stored in the outbox for guaranteed delivery.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the outbox message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type name of the domain event.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized content of the domain event.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the event was processed (null if not yet processed).
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the error message if processing failed.
    /// </summary>
    public string? Error { get; set; }
}