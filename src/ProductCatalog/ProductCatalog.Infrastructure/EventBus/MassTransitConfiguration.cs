using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Common;

namespace ProductCatalog.Infrastructure.EventBus;

public static class MassTransitConfiguration
{
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