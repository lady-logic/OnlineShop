# Online Shop - Domain Driven Design Learning Project

Ein wachsendes Lernprojekt zur Implementierung von **Domain Driven Design (DDD)** und **Clean Architecture** mit .NET 8, entwickelt durch Event Storming und Vertical Slice Architecture.

## Projektübersicht

Dieses Projekt ist ein **kontinuierlich wachsendes Lernprojekt**, das einen vollständigen Online-Shop implementiert. Aktuell ist der **Product Catalog** Bounded Context implementiert, weitere Contexts folgen schrittweise.

### Geplante Bounded Contexts
- ✅ **Product Catalog** - Produktverwaltung (Aktuell implementiert)
- 🔄 **Shopping Basket** - Warenkorbfunktionalität (Geplant)
- 🔄 **Checkout Process** - Bestellabwicklung (Geplant)
- 🔄 **Payment** - Zahlungsabwicklung (Geplant)
- 🔄 **Fulfillment** - Versand und Lieferung (Geplant)

## Event Storming Basis

Das Projekt basiert auf einem Event Storming Workshop, bei dem die folgenden Domain Events für den Product Catalog identifiziert wurden:

- `ProductAdded` - Neues Produkt hinzugefügt
- `ProductDetailsUpdated` - Produktdetails aktualisiert (Geplant)
- `PriceChanged` - Preis geändert (Geplant)
- `StockUpdated` - Lagerbestand aktualisiert (Geplant)
- `ProductBecameUnavailable` - Produkt nicht mehr verfügbar (Geplant)
- `StockValidationRequested` - Lagerbestand-Validierung angefordert (Geplant)
- `StockValidationFailed` - Lagerbestand-Validierung fehlgeschlagen (Geplant)

## Architektur

### Clean Architecture + DDD
- **Domain Layer**: Entities, Value Objects, Domain Events, Domain Exceptions
- **Application Layer**: Use Cases, Commands, Queries, Handlers (CQRS)
- **Infrastructure Layer**: Database, Repository Implementations, Event Processing
- **API Layer**: HTTP Endpoints, Dependency Injection Setup

### Implementierte DDD Patterns
- **Aggregate Root**: Product als Aggregate mit invarianten Schutz
- **Value Objects**: Price als immutable Value Object
- **Domain Events**: Event-basierte Kommunikation zwischen Aggregates
- **Factory Method Pattern**: Kontrollierte Entity-Erstellung mit Validation
- **Repository Pattern**: Abstraktion der Datenzugriffsschicht
- **Transactional Outbox Pattern**: Garantierte Event-Auslieferung mit Transaktionssicherheit

### Vertical Slice Architecture
Jedes Feature ist als eigenständiger "Slice" implementiert:
```
Features/AddProduct/
├── AddProductCommand.cs
├── AddProductCommandHandler.cs
├── AddProductCommandValidator.cs
└── AddProductEndpoint.cs
```

## Technologie-Stack

### Backend (.NET 8)
- **Framework**: ASP.NET Core 8 Web API
- **Architecture**: Clean Architecture + DDD + Vertical Slice
- **CQRS**: MediatR für Command/Query Separation
- **Validation**: FluentValidation für Input-Validierung
- **ORM**: Entity Framework Core mit SQLite
- **Domain Events**: MediatR mit Transactional Outbox Pattern
- **Message Broker**: MassTransit mit RabbitMQ für robuste Event-Verarbeitung
- **Background Services**: .NET Hosted Services für Event Processing
- **API Documentation**: Swagger/OpenAPI

### Patterns & Practices
- **Domain-Driven Design**: Aggregates, Value Objects, Domain Events
- **CQRS**: Command/Query Responsibility Segregation
- **Event Sourcing Ready**: Domain Events als First-Class Citizens
- **Transactional Outbox**: Atomare Persistence mit garantierter Event-Auslieferung
- **Event Bus**: MassTransit für zuverlässige Bounded Context-Kommunikation
- **EF Core Interceptors**: Automatische Event-Persistierung
- **Factory Method Pattern**: Kontrollierte Object Creation
- **Repository Pattern**: Clean Data Access Abstraction

### Development Tools
- **Code Quality**: StyleCop für Code-Standards
- **CI/CD**: GitHub Actions
- **Code Analysis**: SonarCloud Integration (Geplant)
- **Testing**: xUnit, FluentAssertions, Moq

### Future Integrations
- **Service Discovery**: .NET Aspire (Geplant)
- **Caching**: Redis für Performance (Geplant)
- **Monitoring**: Observability mit Aspire (Geplant)

## Quick Start

### Voraussetzungen
- .NET 8 SDK
- Git
- RabbitMQ Server (lokal oder via Docker)

### Installation
```bash
# Repository klonen
git clone https://github.com/[username]/online-shop-ddd.git
cd online-shop-ddd

# Dependencies installieren
dotnet restore

# Datenbank erstellen
cd src/ProductCatalog/ProductCatalog.Api
dotnet ef database update --project ../ProductCatalog.Infrastructure

# API starten
dotnet run

# RabbitMQ starten (via Docker)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### API testen
1. Öffne `https://localhost:7xxx/swagger`
2. Teste den AddProduct Endpoint:
```json
{
  "name": "iPhone 15",
  "description": "Latest Apple smartphone",
  "priceAmount": 999.99,
  "currency": "EUR",
  "initialStock": 50
}
```

## Projektstruktur

```
src/ProductCatalog/
├── ProductCatalog.Api/              # HTTP Endpoints, Program.cs
├── ProductCatalog.Application/      # Use Cases, Commands, Queries
├── ProductCatalog.Domain/           # Entities, Value Objects, Events, Exceptions
│   ├── Common/                      # AggregateRoot, IDomainEvent
│   ├── Entities/                    # Product Aggregate
│   ├── Events/                      # Domain Events
│   ├── Exceptions/                  # Domain-specific Exceptions
│   └── ValueObjects/                # Price Value Object
├── ProductCatalog.Infrastructure/   # Database, Repositories, Event Processing
│   ├── Persistence/
│   │   ├── Configurations/          # EF Core Entity Configurations
│   │   ├── Interceptors/            # Domain Event Interceptor
│   │   └── Outbox/                  # Outbox Pattern Implementation
│   └── Repositories/                # Repository Implementations
└── ProductCatalog.Tests/            # Unit & Integration Tests
```

## Tests

```bash
# Alle Tests ausführen
dotnet test

# Tests mit Coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Lernziele

Dieses Projekt dient dem Erlernen von:
- ✅ Domain Driven Design (DDD) Prinzipien
- ✅ Clean Architecture Implementierung
- ✅ CQRS Pattern mit MediatR
- ✅ Event Storming als Design-Methode
- ✅ Vertical Slice Architecture
- ✅ Transactional Outbox Pattern
- ✅ Domain Events Processing
- ✅ Factory Method Pattern
- ✅ EF Core Interceptors
- ✅ Event-Driven Architecture mit MassTransit/RabbitMQ
- 🔄 Microservices Communication (geplant)
- 🔄 .NET Aspire für Service Orchestration (geplant)


## Status

**Aktueller Stand**: Product Catalog Bounded Context mit Event Bus-Integration
**Nächste Schritte**: Shopping Basket Bounded Context implementieren

