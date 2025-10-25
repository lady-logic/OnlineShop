using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCatalog.Domain.Common;
using ProductCatalog.Infrastructure.Persistence.Outbox;

namespace ProductCatalog.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that saves domain events to the outbox table for guaranteed delivery.
/// </summary>
public class DomainEventInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await SaveDomainEventsToOutbox(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SaveDomainEventsToOutbox(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Saves all domain events from aggregate roots tracked by the context to the outbox table.
    /// </summary>
    /// <param name="context">The current <see cref="DbContext"/> instance.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    private static Task SaveDomainEventsToOutbox(
        DbContext? context,
        CancellationToken cancellationToken = default)
    {
        if (context == null)
        {
            return Task.CompletedTask;
        }

        // Alle Aggregates mit Domain Events finden
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Alle Domain Events sammeln
        var domainEvents = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        // Events aus Aggregates entfernen
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        // Events in Outbox-Tabelle speichern
        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
            Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredOnUtc = DateTime.UtcNow
        }).ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);

        return Task.CompletedTask;
    }
}