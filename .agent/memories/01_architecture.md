# ECO.WebApi - Architecture Memory

> 🧠 **Purpose**: Core architecture knowledge for AI agents
> 📅 **Last Updated**: 2026-01-28
> 👤 **Maintained By**: vuongnv1206

---

## Clean Architecture Layers

### Layer Dependencies (STRICT)
```
Shared (no dependencies)
  ↓
Domain (depends on: Shared)
  ↓
Application (depends on: Domain, Shared)
  ↓
Infrastructure (depends on: Application, Domain, Shared)
  ↓
Host (depends on: Infrastructure, Application)
```

**CRITICAL RULE**: Dependencies only flow INWARD, never outward.

---

## Layer Responsibilities

### 1. Shared Layer
**Location**: `src/Core/Shared/`

**Contains**:
- Authorization constants (ECOAction, ECOFunction, ECORoles, ECOClaims)
- Common contracts (IEvent)
- No dependencies on other layers

**Example**:
```csharp
namespace ECO.WebApi.Shared.Authorization;

public static class ECOAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    // ...
}
```

---

### 2. Domain Layer
**Location**: `src/Core/Domain/`

**Contains**:
- Entities (ApplicationUser, Product, Order)
- Value Objects
- Domain Events
- Enums
- Domain Exceptions
- Interfaces (IAggregateRoot, IEntity)

**Rules**:
- ✅ Rich domain models with business logic
- ✅ Private setters, factory methods
- ✅ Domain events for side effects
- ❌ NO infrastructure concerns
- ❌ NO persistence logic

**Example Entity**:
```csharp
namespace ECO.WebApi.Domain.Catalog;

public class Product : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    // Private constructor for EF Core
    private Product() { }

    // Factory method
    public static Product Create(string name, decimal price, int stock)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            Stock = stock
        };

        // Domain event
        product.DomainEvents.Add(EntityCreatedEvent.WithEntity(product));
        
        return product;
    }

    // Business logic
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new InvalidOperationException("Price cannot be negative");

        Price = newPrice;
        DomainEvents.Add(EntityUpdatedEvent.WithEntity(this));
    }
}
```

---

### 3. Application Layer
**Location**: `src/Core/Application/`

**Contains**:
- Use Cases (CQRS with MediatR)
- DTOs (Request/Response models)
- Validators (FluentValidation)
- Interfaces (IUserService, IEmailService)
- Specifications (Ardalis.Specification)
- Behaviors (ValidationBehavior, LoggingBehavior)
- Exceptions (NotFoundException, ConflictException)

**Patterns**:
- ✅ **CQRS**: Commands (mutations) vs Queries (reads)
- ✅ **MediatR**: Mediator pattern for handlers
- ✅ **FluentValidation**: Automatic validation
- ✅ **Specification Pattern**: Complex queries

**Example Request/Handler**:
```csharp
// Request DTO
public class CreateProductRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Validator
public class CreateProductValidator : CustomValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .MustNotBeEmpty(RuleFor(x => x.Name), maxLength: 200);

        RuleFor(x => x.Price)
            .MustBeGreaterThanZero(RuleFor(x => x.Price));

        RuleFor(x => x.Stock)
            .MustNotBeNegative(RuleFor(x => x.Stock));
    }
}

// Handler
public class CreateProductHandler : IRequestHandler<CreateProductRequest, Guid>
{
    private readonly IRepository<Product> _repository;

    public CreateProductHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken ct)
    {
        // Create domain entity
        var product = Product.Create(request.Name, request.Price, request.Stock);
        
        // Persist
        await _repository.AddAsync(product, ct);
        
        return product.Id;
    }
}
```

---

### 4. Infrastructure Layer
**Location**: `src/Infrastructure/Infrastructure/`

**Contains**:
- DbContext, Repositories
- Identity (Authentication, Authorization)
- Caching (Redis, In-Memory)
- Mailing (SMTP)
- Background Jobs (Hangfire)
- File Storage (Local, Azure Blob, Google Drive)
- Logging (Serilog)
- External API integrations

**Patterns**:
- ✅ **Repository Pattern**: Data access abstraction
- ✅ **Decorator Pattern**: EventAddingRepositoryDecorator
- ✅ **Modular Startup**: Each module has own Startup.cs

**Example Repository**:
```csharp
public class ApplicationDbRepository<T> : RepositoryBase<T>, IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicationDbRepository(ApplicationDbContext dbContext) 
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    // Implementation...
}
```

---

### 5. Host Layer
**Location**: `src/Host/Host/`

**Contains**:
- Controllers (API endpoints)
- Program.cs (startup)
- Middleware pipeline
- Configuration files (Configurations/*.json)

**Structure**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Load configurations
builder.AddConfigurations();
builder.RegisterSerilog();

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseExceptionMiddleware();  // FIRST
app.UseRouting();
app.UseCurrentUserMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Design Patterns Used

### 1. Repository Pattern
**Why**: Abstract data access, support testing
**Where**: Infrastructure layer

### 2. Specification Pattern
**Why**: Complex queries, reusable filters
**Where**: Application layer (interfaces), Infrastructure (implementation)

### 3. CQRS (MediatR)
**Why**: Separate reads/writes, pipeline behaviors
**Where**: Application layer

### 4. Decorator Pattern
**Why**: Add cross-cutting concerns (events, logging)
**Where**: Infrastructure (EventAddingRepositoryDecorator)

### 5. Marker Interfaces
**Why**: Auto service registration
**Where**: Application (ITransientService, IScopedService)

### 6. Factory Method
**Why**: Controlled entity creation
**Where**: Domain entities

---

## Dependency Injection

### Service Lifetimes

**Transient**:
- Created every time requested
- Use for: Validators, Calculators, lightweight services
- Example: `IEmailService`, `ISerializerService`

**Scoped**:
- Created once per HTTP request
- Use for: DbContext, Repositories, CurrentUser
- Example: `IRepository<T>`, `ICurrentUser`

**Singleton**:
- Created once for application lifetime
- Use for: Configuration, Caching, Logging
- Example: `IMemoryCache`, `IConfiguration`

### Auto-Registration
```csharp
// Using marker interfaces
public interface IEmailService : ITransientService { }
public class EmailService : IEmailService { }

// Auto-registered as:
services.AddTransient<IEmailService, EmailService>();
```

---

## Critical Architecture Rules

### ❌ NEVER DO:
1. Reference Infrastructure from Application
2. Reference Host from any layer except Program.cs
3. Put business logic in DbContext
4. Return entities from API - use DTOs
5. Skip validation for requests
6. Hardcode connection strings
7. Expose sensitive data in error messages

### ✅ ALWAYS DO:
1. Follow dependency flow (inward only)
2. Use DTOs for API contracts
3. Validate all inputs with FluentValidation
4. Use private setters on entities
5. Factory methods for entity creation
6. Domain events for side effects
7. Repository only for aggregate roots
8. XML documentation on public APIs

---

## Quick Reference

### Adding New Entity
1. Create in Domain layer (with IAggregateRoot)
2. Add DbSet in ApplicationDbContext
3. Create EF Core configuration
4. Create migration
5. Create DTOs in Application
6. Create CRUD handlers
7. Add validators
8. Add controller endpoints

### Adding New Feature
1. Plan domain model
2. Create entities (Domain)
3. Create interfaces (Application)
4. Create DTOs and validators (Application)
5. Create handlers (Application)
6. Implement services (Infrastructure)
7. Add controllers (Host)
8. Create migration
9. Test

---

**Last Updated**: 2026-01-28
**Maintained By**: vuongnv1206
