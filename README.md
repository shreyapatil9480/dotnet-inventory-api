# dotnet-inventory-api

A **Clean Architecture Inventory Management API** built with **ASP.NET Core (.NET 8)**, demonstrating **SOLID principles**, **design patterns**, **CQRS with MediatR**, and the **Repository pattern**. This project is explicitly designed to showcase the architectural knowledge tested in Software Engineer II interviews.

---

## What This Project Demonstrates

| Skill Area | Implementation |
|---|---|
| **Clean Architecture** | Four decoupled layers: Domain, Application, Infrastructure, API |
| **SOLID Principles** | All 5 principles applied and documented with code references |
| **CQRS via MediatR** | Commands and Queries separated — no business logic in controllers |
| **Repository Pattern** | `IProductRepository` / `IStockRepository` decouple business logic from EF Core |
| **Dependency Injection** | All interfaces registered in `Program.cs`, injected via constructor |
| **Factory Pattern** | `StockMovementFactory` creates movement records per type (IN/OUT/ADJUSTMENT) |
| **Strategy Pattern** | `IPricingStrategy` — extensible pricing without modifying existing code |
| **FluentValidation** | Declarative validation rules decoupled from controllers |
| **xUnit + Moq** | Application layer unit tests with mocked repositories |
| **GitHub Actions CI** | Build and test on every push |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│         (Controllers, Middleware, Program.cs DI setup)      │
│         Depends on: Application                             │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│   (MediatR Handlers, FluentValidation, AutoMapper, DTOs)   │
│   Zero framework dependencies — pure C# business logic      │
│   Depends on: Domain (interfaces only)                      │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                      │
│       (EF Core DbContext, Repository implementations,       │
│        SQLite, Migrations)                                  │
│        Implements: Domain interfaces                        │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│    (Entity classes, Repository interfaces, Domain events,  │
│     Domain exceptions)                                     │
│     Zero dependencies on any external framework            │
└─────────────────────────────────────────────────────────────┘
```

**Key rule:** Dependencies always point inward. The Domain layer knows nothing about EF Core, ASP.NET Core, or any other framework.

---

## SOLID Principles

This section is intentionally detailed — interviewers ask about this directly.

### S — Single Responsibility
Each MediatR handler class does exactly one thing:
- `CreateProductHandler` — creates a product
- `UpdateStockHandler` — records a stock movement
- `GetProductByIdHandler` — retrieves a single product

No handler mixes concerns. Controllers only route requests — they contain zero business logic.

### O — Open/Closed
`IPricingStrategy` is open for extension (add `PremiumPricing` class) but closed for modification (existing `StandardPricing` and `DiscountedPricing` are never touched):

```csharp
public interface IPricingStrategy
{
    decimal CalculatePrice(Product product, int quantity);
}

public class StandardPricing   : IPricingStrategy { ... }  // existing
public class DiscountedPricing : IPricingStrategy { ... }  // existing
public class PremiumPricing    : IPricingStrategy { ... }  // add new — no existing code changed
```

### L — Liskov Substitution
Any `IPricingStrategy` implementation can be substituted anywhere an `IPricingStrategy` is expected without changing behavior. Any `IProductRepository` implementation (SQLite, in-memory, future PostgreSQL) works anywhere `IProductRepository` is used.

### I — Interface Segregation
Repository interfaces are split by entity:
- `IProductRepository` — product CRUD only
- `IStockRepository` — stock movement operations only
- `ICategoryRepository` — category operations only

Controllers that only query products don't get stock movement methods injected.

### D — Dependency Inversion
```csharp
// Controller depends on IProductRepository (abstraction), NOT SqliteProductRepository (detail)
public class ProductsController : ControllerBase
{
    private readonly ISender _mediator;  // depends on MediatR abstraction
    public ProductsController(ISender mediator) => _mediator = mediator;
}

// Handler depends on IProductRepository (abstraction)
public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repo;  // abstraction injected
    public CreateProductHandler(IProductRepository repo) => _repo = repo;
}
```

### SOLID at a Glance

| Principle | Code Location | Description |
|---|---|---|
| **S** — Single Responsibility | [`Application/Handlers/`](src/InventoryApi.Application/) | Each handler class has one job |
| **O** — Open/Closed | [`Application/Pricing/IPricingStrategy.cs`](src/InventoryApi.Application/Pricing/IPricingStrategy.cs) | New pricing = new class, no existing code changed |
| **L** — Liskov Substitution | [`Infrastructure/Repositories/`](src/InventoryApi.Infrastructure/Repositories/) | Any repository implementation substitutes the interface safely |
| **I** — Interface Segregation | [`Domain/Interfaces/`](src/InventoryApi.Domain/Interfaces/) | Separate interfaces per entity, no fat interfaces |
| **D** — Dependency Inversion | [`API/Controllers/`](src/InventoryApi.API/Controllers/) | All controllers depend on abstractions |

---

## Design Patterns

### Repository Pattern
```
IProductRepository (Domain)  ←  SqliteProductRepository (Infrastructure)
```
Unit tests mock `IProductRepository` — they never touch `DbContext` or SQLite.

### CQRS via MediatR
```
CreateProductCommand → CreateProductHandler → returns ProductId
GetProductByIdQuery  → GetProductByIdHandler → returns ProductDto
UpdateStockCommand   → UpdateStockHandler    → returns StockMovementId
```

### Factory Pattern
`StockMovementFactory` encapsulates the rules for creating stock movement records:
- **IN** movements must have a positive quantity and a supplier reference
- **OUT** movements must not exceed current stock level
- **ADJUSTMENT** movements can be positive or negative with an adjustment reason

### Strategy Pattern
`IPricingStrategy` — injected at runtime based on product category:
- `StandardPricing` — list price × quantity
- `DiscountedPricing` — applies bulk discount above a threshold quantity

---

## API Endpoints

### Products
| Method | Route | Handler |
|---|---|---|
| `POST` | `/api/products` | `CreateProductCommand` |
| `GET` | `/api/products` | `GetAllProductsQuery` |
| `GET` | `/api/products/{id}` | `GetProductByIdQuery` |
| `PUT` | `/api/products/{id}` | `UpdateProductCommand` |
| `DELETE` | `/api/products/{id}` | `DeleteProductCommand` |

### Stock
| Method | Route | Handler |
|---|---|---|
| `POST` | `/api/products/{id}/stock` | `UpdateStockCommand` |
| `GET` | `/api/products/{id}/stock` | `GetStockLevelQuery` |
| `GET` | `/api/products/{id}/stock/history` | `GetStockHistoryQuery` |

### Categories
| Method | Route | Handler |
|---|---|---|
| `POST` | `/api/categories` | `CreateCategoryCommand` |
| `GET` | `/api/categories` | `GetAllCategoriesQuery` |

---

## Project Structure

```
dotnet-inventory-api/
├── src/
│   ├── InventoryApi.Domain/         # Entities, interfaces, domain exceptions
│   ├── InventoryApi.Application/    # MediatR commands/queries/handlers, FluentValidation
│   ├── InventoryApi.Infrastructure/ # EF Core, SQLite, repository implementations
│   └── InventoryApi.API/            # Controllers, middleware, Program.cs
├── tests/
│   ├── InventoryApi.UnitTests/      # xUnit + Moq — Application layer tests
│   └── InventoryApi.IntegrationTests/ # WebApplicationFactory + in-memory SQLite
├── .github/
│   └── workflows/
│       └── ci.yml
└── README.md
```

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET | 8.0 | Runtime |
| ASP.NET Core | 8.0 | Web API framework |
| Entity Framework Core | 8.x | ORM |
| SQLite | — | Database |
| MediatR | 12.x | CQRS / mediator pattern |
| FluentValidation | 11.x | Declarative request validation |
| AutoMapper | 12.x | DTO ↔ Domain mapping |
| xUnit | 2.x | Tests |
| Moq | 4.x | Mocking |
| FluentAssertions | 6.x | Readable test assertions |
| GitHub Actions | — | CI/CD |

---

## Getting Started

```bash
git clone https://github.com/shreyapatil9480/dotnet-inventory-api.git
cd dotnet-inventory-api/src/InventoryApi.API
dotnet restore
dotnet ef database update
dotnet run
```

Swagger UI at `https://localhost:5001/swagger`

```bash
# Run tests
dotnet test
```

---

## Background

This project was built as part of a portfolio to demonstrate **software architecture and design patterns** knowledge, targeting a **Software Engineer / SDE II** role. See [PROJECT_PLAN.md](PROJECT_PLAN.md) for the full build plan and SOLID principle documentation.
