using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Extensions;
using ProductCatalog.Application;
using ProductCatalog.Infrastructure;
using ProductCatalog.Infrastructure.Persistence;
using Serilog;
using Serilog.Formatting.Json;
using System.Reflection;

// Serilog Konfiguration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter()) // Strukturiertes JSON für Promtail
    .Enrich.WithProperty("Application", "ProductCatalog") // Labels hinzufügen
    .Enrich.WithProperty("Environment", "Development")
    .CreateLogger();

try
{
    Log.Information("Starting ProductCatalog API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog als Logger verwenden
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Product Catalog API",
            Version = "v1",
            Description = "Product Catalog API for Online Shop DDD Learning Project"
        });

        // Include XML comments for better documentation
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    // Layer Dependencies
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Datenbank-Migration beim Start anwenden
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }

    // Configure pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
            c.RoutePrefix = string.Empty; // Swagger UI als Startseite
        });
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    // Map endpoints
    app.MapProductEndpoints();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Partial Program class to enable integration testing.
/// </summary>
public partial class Program
{
}