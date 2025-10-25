using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Common.Behaviors;

namespace ProductCatalog.Application;

/// <summary>
/// Extension methods for configuring application layer dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Neue Syntax für MediatR 12.0+
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Validation
        services.AddValidatorsFromAssembly(assembly);

        // Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
