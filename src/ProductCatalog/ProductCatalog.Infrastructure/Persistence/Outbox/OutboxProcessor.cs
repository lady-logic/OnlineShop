using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Common;

namespace ProductCatalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background service that processes domain events from the outbox table.
/// </summary>
public class OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger, IMessageBroker messageBroker) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger = logger;
    private readonly IMessageBroker _messageBroker = messageBroker;

    /// <summary>
    /// Executes the background service logic for processing outbox messages.
    /// </summary>
    /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            // Warte 10 Sekunden bis zum nächsten Durchlauf
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _logger.LogInformation("Outbox Processor stopped");
    }

    /// <summary>
    /// Processes unhandled outbox messages and publishes their events.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Hole unverarbeitete Messages
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Event deserialisieren
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Could not find type {Type}", message.Type);
                    message.Error = $"Type not found: {message.Type}";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                if (domainEvent == null)
                {
                    _logger.LogWarning("Could not deserialize event {Type}", message.Type);
                    message.Error = "Deserialization failed";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                // Als verarbeitet markieren
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;

                await dbContext.SaveChangesAsync(cancellationToken);

                // Event publishen
                await mediator.Publish(domainEvent, cancellationToken);
                await _messageBroker.PublishAsync(domainEvent);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully processed outbox message {Id} of type {Type}",
                    message.Id,
                    message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                message.Error = ex.Message;

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}