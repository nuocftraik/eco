# ECO.WebApi - Coding Standards Memory

> 🧠 **Purpose**: Coding conventions and standards for AI agents
> 📅 **Last Updated**: 2026-01-28
> 👤 **Maintained By**: vuongnv1206

---

## Code Analysis Tools

### Required Analyzers
```xml
<!-- Directory.Build.props -->
<ItemGroup>
  <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.507">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  <PackageReference Include="SonarAnalyzer.CSharp" Version="9.12.0.78982">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

---

## Naming Conventions

### Namespaces
**Pattern**: `ECO.WebApi.{Layer}.{Feature}`

```csharp
// ✅ CORRECT
namespace ECO.WebApi.Application.Catalog.Products;
namespace ECO.WebApi.Infrastructure.Persistence;
namespace ECO.WebApi.Domain.Ordering;

// ❌ WRONG
namespace ECO.Products;
namespace MyApp.Services;
```

### Classes

**Entities**: PascalCase, singular
```csharp
✅ public class Product : AuditableEntity, IAggregateRoot { }
✅ public class Order : AuditableEntity, IAggregateRoot { }
❌ public class product { }
❌ public class Products { }
```

**Services**: `I{Name}Service` (interface), `{Name}Service` (implementation)
```csharp
✅ public interface IEmailService : ITransientService { }
✅ public class EmailService : IEmailService { }
❌ public class EmailSvc { }
```

**DTOs**: `{Action}{Entity}{Request|Response|Dto}`
```csharp
✅ public class CreateProductRequest : IRequest<Guid> { }
✅ public class ProductDto { }
✅ public class UpdateUserRequest : IRequest<Guid> { }
❌ public class ProductViewModel { }
❌ public class CreateProduct { }
```

**Validators**: `{RequestName}Validator`
```csharp
✅ public class CreateProductRequestValidator : CustomValidator<CreateProductRequest> { }
✅ public class UpdateUserRequestValidator : CustomValidator<UpdateUserRequest> { }
```

**Handlers**: `{RequestName}Handler`
```csharp
✅ public class CreateProductHandler : IRequestHandler<CreateProductRequest, Guid> { }
✅ public class GetProductByIdHandler : IRequestHandler<GetProductByIdRequest, ProductDto> { }
```

---

### Methods

**Commands (mutations)**: Verb + Object
```csharp
✅ public Task<Guid> CreateAsync(...)
✅ public Task UpdateAsync(...)
✅ public Task DeleteAsync(...)
✅ public void ReduceStock(int quantity)
❌ public Task Create(...) // Missing Async suffix
❌ public Task HandleCreate(...) // Don't use "Handle"
```

**Queries (reads)**: Get + Object
```csharp
✅ public Task<ProductDto> GetByIdAsync(Guid id)
✅ public Task<List<ProductDto>> GetAllAsync()
✅ public Task<PaginatedResult<ProductDto>> SearchAsync(...)
```

**Async methods**: ALWAYS suffix with `Async`
```csharp
✅ public async Task<Product> GetByIdAsync(Guid id)
❌ public async Task<Product> GetById(Guid id)
```

---

### Properties

**Public properties**: PascalCase
```csharp
✅ public string Name { get; private set; }
✅ public decimal Price { get; private set; }
❌ public string name { get; set; }
❌ public string _name { get; set; }
```

**Private fields**: camelCase with underscore
```csharp
✅ private readonly IRepository<Product> _repository;
✅ private readonly ILogger<ProductService> _logger;
❌ private IRepository<Product> repository;
❌ private IRepository<Product> Repository;
```

---

### Constants

**Public constants**: PascalCase
```csharp
✅ public const string Admin = nameof(Admin);
✅ public const int MaxPageSize = 100;
❌ public const string ADMIN = "Admin";
```

---

## Code Structure

### Entity Structure
```csharp
namespace ECO.WebApi.Domain.Catalog;

/// <summary>
/// Represents a product in the catalog
/// </summary>
public class Product : AuditableEntity, IAggregateRoot
{
    // Properties (public with private setters)
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoryId { get; private set; }
    
    // Navigation properties
    public Category Category { get; private set; } = default!;
    
    // Private constructor for EF Core
    private Product() 
    { 
    }
    
    // Factory method
    public static Product Create(string name, string description, decimal price, int stock, Guid categoryId)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        // Create
        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            CategoryId = categoryId
        };

        // Domain event
        product.DomainEvents.Add(EntityCreatedEvent.WithEntity(product));
        
        return product;
    }
    
    // Business methods
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new InvalidOperationException("Price cannot be negative");

        Price = newPrice;
        DomainEvents.Add(EntityUpdatedEvent.WithEntity(this));
    }
    
    public void ReduceStock(int quantity)
    {
        if (Stock < quantity)
            throw new InvalidOperationException("Insufficient stock");

        Stock -= quantity;
        DomainEvents.Add(EntityUpdatedEvent.WithEntity(this));
    }
    
    public void AddStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        Stock += quantity;
        DomainEvents.Add(EntityUpdatedEvent.WithEntity(this));
    }
}
```

---

### Request/Handler Structure
```csharp
// Request DTO
namespace ECO.WebApi.Application.Catalog.Products;

/// <summary>
/// Request to create a new product
/// </summary>
public class CreateProductRequest : IRequest<Guid>
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = default!;
    
    /// <summary>
    /// Product description
    /// </summary>
    public string Description { get; set; } = default!;
    
    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Initial stock quantity
    /// </summary>
    public int Stock { get; set; }
    
    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }
}

// Validator
/// <summary>
/// Validator for CreateProductRequest
/// </summary>
public class CreateProductRequestValidator : CustomValidator<CreateProductRequest>
{
    private readonly IRepository<Category> _categoryRepository;

    public CreateProductRequestValidator(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .MustNotBeEmpty(RuleFor(x => x.Name), maxLength: 200);

        RuleFor(x => x.Description)
            .MustNotBeEmpty(RuleFor(x => x.Description), maxLength: 2000);

        RuleFor(x => x.Price)
            .MustBeGreaterThanZero(RuleFor(x => x.Price));

        RuleFor(x => x.Stock)
            .MustNotBeNegative(RuleFor(x => x.Stock));

        RuleFor(x => x.CategoryId)
            .MustNotBeEmpty(RuleFor(x => x.CategoryId))
            .MustAsync(CategoryMustExist)
            .WithMessage("Category with ID {PropertyValue} does not exist.");
    }

    private async Task<bool> CategoryMustExist(Guid id, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        return category != null;
    }
}

// Handler
/// <summary>
/// Handler for creating a new product
/// </summary>
public class CreateProductHandler : IRequestHandler<CreateProductRequest, Guid>
{
    private readonly IRepository<Product> _repository;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        IRepository<Product> repository,
        ILogger<CreateProductHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product {ProductName}", request.Name);

        // Create entity
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.Stock,
            request.CategoryId);

        // Persist
        await _repository.AddAsync(product, cancellationToken);

        _logger.LogInformation("Product {ProductId} created successfully", product.Id);

        return product.Id;
    }
}
```

---

## XML Documentation

### REQUIRED for:
- ✅ Public classes
- ✅ Public methods
- ✅ Public properties
- ✅ Interfaces
- ✅ DTOs

### Format
```csharp
/// <summary>
/// Short description (one sentence)
/// </summary>
/// <param name="paramName">Description</param>
/// <returns>Description of return value</returns>
/// <exception cref="ExceptionType">When this exception is thrown</exception>
public async Task<ProductDto> GetByIdAsync(Guid id)
{
    // Implementation
}
```

---

## Code Quality Rules

### ✅ DO:
1. **Use `var` for obvious types**
   ```csharp
   ✅ var product = new Product();
   ✅ var products = await _repository.ListAsync();
   ❌ Product product = new Product();
   ```

2. **Use expression-bodied members for simple methods**
   ```csharp
   ✅ public string GetFullName() => $"{FirstName} {LastName}";
   ✅ public bool IsActive => Status == UserStatus.Active;
   ```

3. **Use null-conditional operators**
   ```csharp
   ✅ var email = user?.Email;
   ✅ var count = products?.Count ?? 0;
   ```

4. **Use pattern matching**
   ```csharp
   ✅ if (product is null) return NotFound();
   ✅ if (exception is NotFoundException notFound) { }
   ```

5. **Use `default!` for required properties**
   ```csharp
   ✅ public string Name { get; set; } = default!;
   ```

6. **Use `const` for true constants**
   ```csharp
   ✅ public const int MaxPageSize = 100;
   ✅ public const string Admin = nameof(Admin);
   ```

7. **Use `readonly` for fields set in constructor**
   ```csharp
   ✅ private readonly IRepository<Product> _repository;
   ```

### ❌ DON'T:
1. **Magic strings**
   ```csharp
   ❌ if (role == "Admin")
   ✅ if (role == ECORoles.Admin)
   ```

2. **Magic numbers**
   ```csharp
   ❌ if (age > 18)
   ✅ if (age > MinimumAge)
   ```

3. **Catch generic exceptions**
   ```csharp
   ❌ catch (Exception ex) { }
   ✅ catch (NotFoundException ex) { }
   ```

4. **Return null for collections**
   ```csharp
   ❌ return null;
   ✅ return new List<Product>();
   ✅ return Enumerable.Empty<Product>();
   ```

5. **Use `async void`** (except event handlers)
   ```csharp
   ❌ public async void DoSomething()
   ✅ public async Task DoSomethingAsync()
   ```

---

## File Organization

### File per class
**Each class in its own file**
```
✅ Product.cs (contains only Product class)
✅ ProductDto.cs (contains only ProductDto class)
❌ Models.cs (contains multiple classes)
```

### Folder structure
```
Application/
├── Catalog/
│   ├── Products/
│   │   ├── CreateProductRequest.cs
│   │   ├── UpdateProductRequest.cs
│   │   ├── DeleteProductRequest.cs
│   │   ├── GetProductByIdRequest.cs
│   │   ├── SearchProductsRequest.cs
│   │   └── ProductDto.cs
│   └── Categories/
│       └── ...
```

---

## Error Handling

### Use custom exceptions
```csharp
✅ throw new NotFoundException($"Product with ID {id} not found");
✅ throw new ConflictException($"Product name '{name}' already exists");
✅ throw new ForbiddenException("You do not have permission");
❌ throw new Exception("Error occurred");
```

### Exception messages
```csharp
✅ "Product with ID {guid} not found"
✅ "User {email} is not authorized"
❌ "Error"
❌ "Something went wrong"
```

---

## Async/Await

### Always use async/await for I/O
```csharp
✅ public async Task<Product> GetByIdAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}

❌ public Task<Product> GetByIdAsync(Guid id)
{
    return _repository.GetByIdAsync(id); // Missing await
}
```

### Use CancellationToken
```csharp
✅ public async Task<Product> GetByIdAsync(Guid id, CancellationToken cancellationToken)
{
    return await _repository.GetByIdAsync(id, cancellationToken);
}
```

---

## Logging

### Use structured logging
```csharp
✅ _logger.LogInformation("Creating product {ProductName} with price {Price}", name, price);
❌ _logger.LogInformation($"Creating product {name} with price {price}");
```

### Log levels
- **Verbose**: Tracing information
- **Debug**: Development debugging
- **Information**: General flow
- **Warning**: Unexpected but handled
- **Error**: Errors need attention
- **Fatal**: Application cannot continue

---

## 📖 Self-Contained Documentation Standards

### CRITICAL Rule for ALL Documentation

**When documenting any code in BUILD_XX files:**

1. **View Actual Implementation First**
   ```bash
   # ALWAYS check actual code before documenting
   view_file src/Infrastructure/Infrastructure/Path/To/File.cs
   ```

2. **Copy EXACT Code**
   - ALL using statements
   - ALL interfaces implemented
   - ALL members (fields, properties, methods)
   - EXACT initializers (string.Empty vs default! vs null)
   - ALL method bodies (no // ... placeholders)

3. **Verify Completeness**
   - Can copy-paste and compile immediately?
   - All dependencies visible?
   - All business logic included?
   - Nothing simplified or omitted?

### Example Comparison

**❌ INCOMPLETE Documentation:**
```csharp
public class JwtSettings
{
    public string Key { get; set; }
    public int TokenExpirationInMinutes { get; set; }
}
```
**Issues:** Missing `IValidatableObject`, missing `Validate()`, missing `using`, wrong initializer

**✅ COMPLETE Documentation:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ECO.WebApi.Infrastructure.Auth.Jwt;

public class JwtSettings : IValidatableObject
{
    public string Key { get; set; } = string.Empty;
    public int TokenExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Key))
        {
            yield return new ValidationResult(
                "No Key defined in JwtSettings config", 
                new[] { nameof(Key) });
        }
    }
}
```
**Perfect:** Matches actual implementation 100%

### When Suggesting Code Improvements

**If actual code can be improved:**

1. **Document AS-IS first** (exact current implementation)
2. **Then add "Code Improvements" section:**
   ```markdown
   ### 💡 Potential Code Improvements
   
   Current implementation works, but consider:
   
   **Current:**
   ```csharp
   // Exact current code
   ```
   
   **Suggested Improvement:**
   ```csharp
   // Better version with explanation WHY
   ```
   
   **Benefits:**
   - Reason 1
   - Reason 2
   ```

3. **Never replace actual code with "better" code in main docs**
   - Main docs = what's ACTUALLY in codebase
   - Improvements = separate optional section

### Flexibility Principle

**Agent can suggest improvements when:**
- Code quality can be enhanced
- Security can be improved  
- Performance can be optimized
- Patterns can be applied better

**But ALWAYS:**
- Document actual code first
- Separate improvements clearly
- Explain WHY improvement is better
- Let developer decide

---

**Last Updated**: 2026-01-28
**Maintained By**: vuongnv1206
