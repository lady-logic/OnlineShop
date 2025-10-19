namespace ProductCatalog.Domain.Common;

/// <summary>
/// Defines a contract for publishing messages to a message broker.
/// </summary>
public interface IMessageBroker
{
    /// <summary>
    /// Publishes a message asynchronously to the message broker.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="message">The message instance to publish.</param>
    /// <param name="routingKey">The routing key to use for the message. Optional.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(T message, string routingKey = "")
        where T : class;
}
