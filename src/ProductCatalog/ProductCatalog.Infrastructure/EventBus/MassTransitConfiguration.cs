using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Common;

namespace ProductCatalog.Infrastructure.EventBus;

/// <summary>
/// Provides extension methods for configuring MassTransit and RabbitMQ event bus services.
/// </summary>
public static class MassTransitConfiguration
{
    /// <summary>
    /// Adds MassTransit and RabbitMQ event bus services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the event bus services to.</param>
    /// <param name="configuration">The application configuration containing RabbitMQ settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEventBusServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                // RabbitMQ-Konfiguration
                var host = configuration["RabbitMQ:HostName"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Automatische Endpunkt-Konfiguration für Consumer
                cfg.ConfigureEndpoints(context);
            });
        });

        // Registriere das IMessageBroker-Interface mit der MassTransit-Implementierung
        services.AddSingleton<IMessageBroker, MassTransitMessageBroker>();

        return services;
    }
}