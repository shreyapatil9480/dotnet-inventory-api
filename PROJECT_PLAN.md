# Project Plan — dotnet-inventory-api

This document outlines the full build plan, scope, and learning objectives for this project.

---

## Why This Project

SDE II interviews specifically test **architectural thinking** — not just "can you write code" but "can you structure it properly." This project provides concrete, demonstrable answers to the most common SDE II interview questions:

- "Explain SOLID principles with an example" → point to this repo's README
- "What is the Repository pattern?" → show `IProductRepository` / `SqliteProductRepository`
- "What is Dependency Injection?" → show `Program.cs` and the handler constructors
- "What is Clean Architecture?" → show the 4-layer folder structure

---

## Learning Objectives

| Objective | How It's Covered |
|---|---|
| Clean Architecture layers | Domain → Application → Infrastructure → API with unidirectional dependencies |
| CQRS pattern | `CreateProductCommand` / `GetProductByIdQuery` via MediatR |
| Repository pattern | `IProductRepository` interface in Domain, `SqliteProductRepository` in Infrastructure |
| Factory pattern | `StockMovementFactory` — encapsulates creation rules per movement type |
| Strategy pattern | `IPricingStrategy` — open for extension without modifying existing classes |
| FluentValidation | `CreateProductCommandValidator` — declarative validation rules |
| AutoMapper | DTO ↔ Domain entity mapping profiles |
| Unit testing clean code | Test `CreateProductHandler` by mocking `IProductRepository` — no DB, no HTTP |

---

## Implementation Phases

### Phase 1 — Solution Structure and Domain Layer (Days 1–2)

**Goal:** Create solution scaffold and domain models.

```bash
# Create solution
dotnet new sln -n dotnet-inventory-api

# Create projects
dotnet new classlib -n InventoryApi.Domain
dotnet new classlib -n InventoryApi.Application
dotnet new classlib -n InventoryApi.Infrastructure
dotnet new webapi   -n InventoryApi.API
dotnet new xunit    -n InventoryApi.UnitTests
dotnet new xunit    -n InventoryApi.IntegrationTests

# Add projects to solution
dotnet sln add **/*.csproj
```

**Domain models (`InventoryApi.Domain/Entities/`):**

```csharp
public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string SKU { get; private set; }
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public Category Category { get; private set; }
    public ICollection<StockMovement> StockMovements { get; private set; }

    // Domain behavior — business rules live here
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException("Price must be greater than zero.");
        Price = newPrice;
    }
}

public class StockMovement
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public MovementType Type { get; private set; }  // IN | OUT | ADJUSTMENT
    public int Quantity { get; private set; }
    public string Reference { get; private set; }   // supplier PO or reason
    public DateTime CreatedAt { get; private set; }
}

public enum MovementType { In, Out, Adjustment }
```

**Repository interfaces (`InventoryApi.Domain/Interfaces/`):**

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<int> AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IStockRepository
{
    Task<int> GetCurrentStockLevelAsync(int productId, CancellationToken ct = default);
    Task<int> AddMovementAsync(StockMovement movement, CancellationToken ct = default);
    Task<IReadOnlyList<StockMovement>> GetHistoryAsync(int productId, CancellationToken ct = default);
}
```

**Note:** The Domain layer has **zero NuGet dependencies**. It does not reference EF Core, ASP.NET, or MediatR.

---

### Phase 2 — Application Layer (Days 2–4)

**Goal:** Implement CQRS handlers with MediatR and FluentValidation.

**NuGet packages for `InventoryApi.Application`:**
```
MediatR
FluentValidation
FluentValidation.DependencyInjectionExtensions
AutoMapper
```

**Example Command and Handler:**

```csharp
// Command (what we want to do)
public record CreateProductCommand(string Name, string SKU, decimal Price, int CategoryId)
    : IRequest<int>;

// Validator (runs automatically before handler via MediatR pipeline behavior)
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SKU).NotEmpty().Matches("^[A-Z0-9\\-]+$");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

// Handler (single responsibility — creates one product)
public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public CreateProductHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = _mapper.Map<Product>(cmd);
        return await _repo.AddAsync(product, ct);
    }
}
```

**Factory Pattern — `StockMovementFactory.cs`:**

```csharp
public static class StockMovementFactory
{
    public static StockMovement CreateInbound(int productId, int quantity, string supplierRef)
    {
        if (quantity <= 0)
            throw new DomainException("Inbound quantity must be positive.");
        if (string.IsNullOrWhiteSpace(supplierRef))
            throw new DomainException("Inbound movement requires a supplier reference.");
        return new StockMovement(productId, MovementType.In, quantity, supplierRef);
    }

    public static StockMovement CreateOutbound(int productId, int quantity, int currentStock)
    {
        if (quantity <= 0)
            throw new DomainException("Outbound quantity must be positive.");
        if (quantity > currentStock)
            throw new InsufficientStockException(productId, quantity, currentStock);
        return new StockMovement(productId, MovementType.Out, -quantity, "OUTBOUND");
    }

    public static StockMovement CreateAdjustment(int productId, int delta, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Adjustment requires a reason.");
        return new StockMovement(productId, MovementType.Adjustment, delta, reason);
    }
}
```

**Strategy Pattern — `IPricingStrategy.cs`:**

```csharp
public interface IPricingStrategy
{
    decimal CalculatePrice(Product product, int quantity);
}

public class StandardPricing : IPricingStrategy
{
    public decimal CalculatePrice(Product product, int quantity) =>
        product.Price * quantity;
}

public class DiscountedPricing : IPricingStrategy
{
    private const int BulkThreshold = 10;
    private const decimal DiscountRate = 0.90m;  // 10% off

    public decimal CalculatePrice(Product product, int quantity)
    {
        var basePrice = product.Price * quantity;
        return quantity >= BulkThreshold ? basePrice * DiscountRate : basePrice;
    }
}
```

---

### Phase 3 — Infrastructure Layer (Days 4–5)

**Goal:** EF Core DbContext, repository implementations, SQLite migrations.

**NuGet packages for `InventoryApi.Infrastructure`:**
```
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```

**`InventoryDbContext.cs`** configures the entity relationships.  
**`SqliteProductRepository.cs`** implements `IProductRepository` using EF Core.  
**`SqliteStockRepository.cs`** implements `IStockRepository` using EF Core.

Add first migration:
```bash
cd src/InventoryApi.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../InventoryApi.API
```

---

### Phase 4 — API Layer and Dependency Injection (Day 5)

**Goal:** Thin controllers that delegate to MediatR, with all DI registered in `Program.cs`.

**Controller pattern (thin — zero business logic):**
```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _mediator;
    public ProductsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
```

**`Program.cs` DI registration:**
```csharp
// Application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductHandler).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Infrastructure layer
builder.Services.AddDbContext<InventoryDbContext>(opts => opts.UseSqlite("Data Source=inventory.db"));
builder.Services.AddScoped<IProductRepository, SqliteProductRepository>();
builder.Services.AddScoped<IStockRepository, SqliteStockRepository>();

// Pricing strategies registered by name
builder.Services.AddKeyedScoped<IPricingStrategy, StandardPricing>("standard");
builder.Services.AddKeyedScoped<IPricingStrategy, DiscountedPricing>("discounted");
```

---

### Phase 5 — Unit Tests (Days 6–7)

**Goal:** Test Application layer handlers with mocked repositories (no database).

**Example unit test:**
```csharp
public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewProductId()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockMapper = new Mock<IMapper>();
        var newProduct = new Product { Id = 42, Name = "Widget A" };

        mockMapper.Setup(m => m.Map<Product>(It.IsAny<CreateProductCommand>()))
                  .Returns(newProduct);
        mockRepo.Setup(r => r.AddAsync(newProduct, It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

        var handler = new CreateProductHandler(mockRepo.Object, mockMapper.Object);
        var command = new CreateProductCommand("Widget A", "WGT-001", 9.99m, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(42);
        mockRepo.Verify(r => r.AddAsync(newProduct, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NegativePrice_ThrowsDomainException()
    {
        // Validation happens via MediatR pipeline behavior before handler runs
        // Test the validator directly
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("Widget", "WGT-001", -5.00m, 1);
        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}
```

**`StockMovementFactoryTests`** — test each factory method:
```
CreateInbound_ValidInputs_ReturnsInboundMovement
CreateInbound_ZeroQuantity_ThrowsDomainException
CreateInbound_MissingSupplierRef_ThrowsDomainException
CreateOutbound_QuantityExceedsStock_ThrowsInsufficientStockException
CreateAdjustment_NoReason_ThrowsDomainException
```

**`DiscountedPricingTests`** — test the strategy:
```
CalculatePrice_BelowThreshold_NoDiscount
CalculatePrice_AtThreshold_AppliesDiscount
CalculatePrice_AboveThreshold_AppliesDiscount
```

---

### Phase 6 — GitHub Actions CI (Day 7)

Same pattern as `dotnet-bug-tracker`. Build → unit tests → integration tests → coverage.

---

## SOLID Checklist for README

After completing the project, add this table to the README with links to specific files:

| Principle | Code Location | Description |
|---|---|---|
| **S** — Single Responsibility | `Application/Handlers/` | Each handler class has one job |
| **O** — Open/Closed | `Application/Pricing/IPricingStrategy.cs` | New pricing = new class, no existing code changed |
| **L** — Liskov Substitution | All `IRepository` implementations | Any impl substitutes the interface safely |
| **I** — Interface Segregation | `Domain/Interfaces/` | Separate interfaces per entity, no fat interfaces |
| **D** — Dependency Inversion | `API/Controllers/` | All controllers depend on abstractions |

---

## Estimated Timeline

| Phase | Time (part-time, ~2 hrs/day) |
|---|---|
| Phase 1 — Domain layer | 2 days |
| Phase 2 — Application layer | 2 days |
| Phase 3 — Infrastructure | 1 day |
| Phase 4 — API + DI | 1 day |
| Phase 5 — Unit tests | 2 days |
| Phase 6 — GitHub Actions | 1 day |
| README polish + SOLID table | 1 day |
| **Total** | **~10 days** |

---

## Resume Bullets (add after completing)

- Architected a .NET 8 Inventory Management API using Clean Architecture, implementing CQRS with MediatR, the Repository pattern, and FluentValidation across four decoupled layers (Domain, Application, Infrastructure, API).
- Applied all five SOLID principles throughout the codebase — explicitly documented in the project README with file-level code references for each principle.
- Implemented Factory and Strategy design patterns for stock movement creation and extensible pricing rules, enabling new behavior without modifying existing code (Open/Closed Principle).
- Wrote xUnit unit tests that mock repository interfaces to test Application layer handlers in complete isolation from the database, achieving 90%+ coverage on business logic.
