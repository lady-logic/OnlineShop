using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Common;
using System.Diagnostics.Metrics;

namespace ProductCatalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background service that processes domain events from the outbox table.
/// </summary>
public class OutboxProcessor(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger = logger;

    // Meter initialisieren
    private readonly Meter _meter = new("ProductCatalog.DomainEvents");
    private readonly Counter<long> _processedEventsCounter =
        new Meter("ProductCatalog.DomainEvents").CreateCounter<long>(
            "domain_events_processed",
            description: "Number of domain events processed");

    private readonly Counter<long> _failedEventsCounter =
        new Meter("ProductCatalog.DomainEvents").CreateCounter<long>(
            "domain_events_failed",
            description: "Number of domain events failed processing");

    private readonly Histogram<double> _eventProcessingTime =
        new Meter("ProductCatalog.DomainEvents").CreateHistogram<double>(
            "domain_event_processing_seconds",
            unit: "s",
            description: "Time taken to process domain events");

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
        var messageBroker = scope.ServiceProvider.GetRequiredService<IMessageBroker>();

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

        _logger.LogInformation("Processing outbox messages {@MessageDetails}", new
        {
            Count = messages.Count,
            FirstMessageId = messages.FirstOrDefault()?.Id,
            OldestMessageTime = messages.Min(m => m.OccurredOnUtc)
        });

        foreach (var message in messages)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Event deserialisieren
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Could not find type {Type}", message.Type);
                    message.Error = $"Type not found: {message.Type}";
                    message.ProcessedOnUtc = DateTime.UtcNow;

                    _failedEventsCounter.Add(1, new KeyValuePair<string, object?>("reason", "type_not_found"));
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                if (domainEvent == null)
                {
                    _logger.LogWarning("Could not deserialize event {Type}", message.Type);
                    message.Error = "Deserialization failed";
                    message.ProcessedOnUtc = DateTime.UtcNow;

                    _failedEventsCounter.Add(1, new KeyValuePair<string, object?>("reason", "deserialization_failed"));
                    continue;
                }

                // Als verarbeitet markieren
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;

                await dbContext.SaveChangesAsync(cancellationToken);

                // Event publishen
                await mediator.Publish(domainEvent, cancellationToken);
                await messageBroker.PublishAsync(domainEvent);

                await transaction.CommitAsync(cancellationToken);

                stopwatch.Stop();
                _eventProcessingTime.Record(stopwatch.Elapsed.TotalSeconds,
                    new KeyValuePair<string, object?>("event_type", message.Type));

                _processedEventsCounter.Add(1,
                    new KeyValuePair<string, object?>("event_type", message.Type));

                _logger.LogInformation(
                    "Successfully processed outbox message {Id} of type {Type}",
                    message.Id,
                    message.Type);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                message.Error = ex.Message;

                _failedEventsCounter.Add(1,
                    new KeyValuePair<string, object?>("event_type", message.Type ?? "unknown"));

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}