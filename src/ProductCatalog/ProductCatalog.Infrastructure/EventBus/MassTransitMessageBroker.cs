using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Common;

namespace ProductCatalog.Infrastructure.EventBus;

public class MassTransitMessageBroker : IMessageBroker
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitMessageBroker> _logger;

    public MassTransitMessageBroker(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitMessageBroker> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string routingKey = "") where T : class
    {
        try
        {
            await _publishEndpoint.Publish(message);
            _logger.LogInformation("Published message of type {Type} via MassTransit", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message of type {Type} via MassTransit", typeof(T).Name);
            throw;
        }
    }
}