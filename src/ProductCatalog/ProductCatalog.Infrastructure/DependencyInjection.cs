using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Common;
using ProductCatalog.Domain.Repositories;
using ProductCatalog.Infrastructure.EventBus;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Persistence.Interceptors;
using ProductCatalog.Infrastructure.Persistence.Outbox;
using ProductCatalog.Infrastructure.Persistence.Repositories;

namespace ProductCatalog.Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure layer dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Interceptor als Service registrieren
        services.AddScoped<DomainEventInterceptor>();

        // DbContext mit Interceptor
        services.AddDbContext<ProductCatalogDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseSqlite(connectionString, sqliteOptions =>
            {
                // Command-Timeout auf 30 Sekunden setzen
                sqliteOptions.CommandTimeout(30);
            });

            // Logging hinzufügen 
            options.LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableSensitiveDataLogging();

            // Interceptor hinzufügen
            options.AddInterceptors(
                serviceProvider.GetRequiredService<DomainEventInterceptor>());
        });

        // Outbox Processor als Background Service registrieren
        services.AddHostedService<OutboxProcessor>();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();

        // MassTransit hinzufügen
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:HostName"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // IMessageBroker mit der MassTransit-Implementierung registrieren
        services.AddScoped<IMessageBroker, MassTransitMessageBroker>();

        return services;
    }
}
