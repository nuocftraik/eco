# Copilot Instructions Validation

This document contains test queries to verify Copilot understands project patterns.

---

## How to Use This File

1. Open Copilot Chat in VS/VS Code
2. Ask questions from each section below
3. Verify responses match expected answers
4. If responses incorrect → Update `.github/copilot-instructions.md`

---

## Test Set 1: Architecture Understanding

### Q1: Layer Dependencies
**Question:**
```
What are the dependency rules between layers in this project?
```

**Expected Answer:**
```
- Domain: No dependencies on other layers
- Application: Depends on Domain only
- Infrastructure: Depends on Application and Domain
- Host: Depends on all layers
- Dependencies flow inward (Host → Infrastructure → Application → Domain)
```

---

### Q2: Layer Locations
**Question:**
```
Where should I create a new email service interface?
```

**Expected Answer:**
```
Application layer:
src/Core/Application/Common/Mailing/IMailService.cs

Then implement in Infrastructure:
src/Infrastructure/Infrastructure/Mailing/SmtpMailService.cs
```

---

## Test Set 2: Naming Conventions

### Q3: Interface Naming
**Question:**
```
What is the naming pattern for service interfaces? Give 3 examples.
```

**Expected Answer:**
```
Pattern: I{Capability}Service

Examples:
- IMailService (email sending capability)
- IFileStorageService (file storage capability)
- INotificationService (notification capability)

NOT:
- IEmailService (ambiguous)
- ISmtpService (too specific)
```

---

### Q4: Implementation Naming
**Question:**
```
If I have IMailService, what should the SMTP implementation be called?
```

**Expected Answer:**
```
SmtpMailService

Pattern: {Technology}{Capability}Service

This indicates:
- Technology: SMTP
- Capability: Mail
- Type: Service
```

---

### Q5: Settings Naming
**Question:**
```
How should I name a configuration class for JWT authentication?
```

**Expected Answer:**
```
JwtSettings

Rules:
- Use "Settings" suffix (not "Options" or "Config")
- Name must match appsettings.json section name
- Example:
  
  // appsettings.json
  {
    "JwtSettings": { ... }
  }
  
  // Class
  public class JwtSettings { ... }
```

---

## Test Set 3: Dependency Injection

### Q6: Marker Interfaces
**Question:**
```
How do I register a service with transient lifetime?
```

**Expected Answer:**
```
Use marker interface:

public interface IMyService : ITransientService
{
    Task DoSomethingAsync(CancellationToken ct);
}

public class MyService : IMyService
{
    // Implementation
}

No manual registration needed - auto-discovered via marker interface.
```

---

### Q7: Constructor Injection
**Question:**
```
Show me how to inject dependencies in a service constructor.
```

**Expected Answer:**
```csharp
public class MyService : IMyService
{
    private readonly MySettings _settings;
    private readonly ILogger<MyService> _logger;
    private readonly IDbContext _context;

    public MyService(
        IOptions<MySettings> settings,
        ILogger<MyService> logger,
  IDbContext context)
    {
        _settings = settings.Value;  // Unwrap IOptions
   _logger = logger;
        _context = context;
    }
}

Rules:
- Use constructor injection (not property)
- Store dependencies in readonly fields
- Unwrap IOptions<T>.Value
```

---

## Test Set 4: Configuration

### Q8: Settings Binding
**Question:**
```
How do I bind configuration settings to a class?
```

**Expected Answer:**
```csharp
// Settings class
public class MyFeatureSettings
{
    public string ApiKey { get; set; } = default!;
    public int Timeout { get; set; } = 30;
}

// appsettings.json
{
  "MyFeatureSettings": {  // Name matches class
    "ApiKey": "secret",
    "Timeout": 60
  }
}

// Startup.cs
services.Configure<MyFeatureSettings>(
    config.GetSection(nameof(MyFeatureSettings))
);

Rule: Section name MUST match class name
```

---

### Q9: Modular Startup
**Question:**
```
Show me the pattern for feature-specific startup registration.
```

**Expected Answer:**
```csharp
// File: Infrastructure/MyFeature/Startup.cs
internal static class Startup
{
    internal static IServiceCollection AddMyFeature(
        this IServiceCollection services,
        IConfiguration config)
    {
// 1. Bind configuration
        services.Configure<MyFeatureSettings>(
   config.GetSection(nameof(MyFeatureSettings)));

        // 2. Manual registrations (if needed)
        // services.AddTransient<IMyService, MyService>();

     return services;
    }
}

// File: Infrastructure/Startup.cs (Main)
public static IServiceCollection AddInfrastructure(...)
{
  services.AddMyFeature(config);
    return services;
}
```

---

## Test Set 5: Code Style

### Q10: Async Method Signature
**Question:**
```
What is the correct async method signature pattern?
```

**Expected Answer:**
```csharp
// ✅ CORRECT
public async Task<TResult> MethodAsync(
    TRequest request, 
    CancellationToken ct = default)

// ❌ WRONG: Missing CancellationToken
public async Task<TResult> MethodAsync(TRequest request)

// ❌ WRONG: async void
public async void MethodAsync()

Rule: All async methods MUST accept CancellationToken
```

---

### Q11: Error Handling
**Question:**
```
Show me the error handling pattern for services.
```

**Expected Answer:**
```csharp
public async Task ProcessAsync(Request request, CancellationToken ct)
{
    try
    {
        await _service.ExecuteAsync(request, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, 
        "Failed {Operation} for {@Request}", 
            nameof(ProcessAsync), 
     request);
        throw;  // Re-throw for caller
    }
}

Rules:
- Log with structured context
- Always re-throw (don't swallow)
- Use nameof() for operation name
```

---

### Q12: DTO Immutability
**Question:**
```
How should I design a DTO/request class?
```

**Expected Answer:**
```csharp
// ✅ CORRECT: Immutable DTO
public class CreateUserRequest
{
    public CreateUserRequest(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; }      // No setter
    public string Password { get; }   // No setter
}

// ❌ WRONG: Mutable
public class CreateUserRequest
{
    public string Email { get; set; }  // Can be modified
    public string Password { get; set; }
}

Rule: DTOs should be immutable (readonly properties)
```

---

## Test Set 6: MediatR/CQRS

### Q13: Request Pattern
**Question:**
```
Show me how to create a MediatR command request.
```

**Expected Answer:**
```csharp
// Pattern: {Action}{Entity}Request
public class CreateProductRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// For queries:
public class GetProductsQuery : IRequest<List<ProductDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

---

### Q14: Handler Pattern
**Question:**
```
Show me the MediatR handler template.
```

**Expected Answer:**
```csharp
public class CreateProductHandler 
    : IRequestHandler<CreateProductRequest, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
 IApplicationDbContext context,
        ILogger<CreateProductHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateProductRequest request, 
        CancellationToken ct)
    {
_logger.LogInformation("Creating product: {Name}", request.Name);

        // Business logic
  var product = new Product { ... };
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        return product.Id;
    }
}
```

---

## Test Set 7: File Organization

### Q15: Feature Structure
**Question:**
```
How should I organize files for a new "Notification" feature?
```

**Expected Answer:**
```
Application Layer:
src/Core/Application/Common/Notifications/
├── INotificationService.cs
├── NotificationRequest.cs
└── NotificationResponse.cs

Infrastructure Layer:
src/Infrastructure/Infrastructure/Notifications/
├── SmtpNotificationService.cs (or other tech)
├── NotificationSettings.cs
└── Startup.cs

Host Layer (if needed):
src/Host/Host/
├── Controllers/NotificationsController.cs
└── Notification Templates/ (if templates needed)

Configuration:
appsettings.json → Add "NotificationSettings" section

Documentation:
docs/BUILD_XX_Notifications.md
Update docs/BUILD_INDEX.md
```

---

## Test Set 8: Documentation

### Q16: XML Documentation
**Question:**
```
Show me XML documentation for a public method.
```

**Expected Answer:**
```csharp
/// <summary>
/// Sends a notification to specified recipients
/// </summary>
/// <param name="request">Notification request with recipients and message</param>
/// <param name="ct">Cancellation token for operation cancellation</param>
/// <returns>Task representing the asynchronous operation</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when request is null
/// </exception>
public async Task SendAsync(NotificationRequest request, CancellationToken ct)
{
    // Implementation
}

Rule: All public APIs MUST have XML documentation
```

---

### Q17: BUILD Documentation
**Question:**
```
What structure should BUILD_XX documentation follow?
```

**Expected Answer:**
```markdown
Mandatory sections:

1. Overview (What, Why, Checklist, Real-world example)
2. Add Required Packages (NuGet with versions & rationale)
3. Application Layer (Interfaces, DTOs)
4. Infrastructure Layer (Settings, Services, Startup)
5. Configuration (appsettings.json structure)
6. Usage Examples (3-4 real-world scenarios)
7. Best Practices (Do's and Don'ts)
8. Testing (Unit + Integration examples)
9. Troubleshooting (Common errors + solutions)
10. Summary (Architecture diagram, key concepts)

Reference: docs/MODULE_DOCUMENTATION_TEMPLATE.md
```

---

## Test Set 9: Anti-Patterns

### Q18: What to Avoid
**Question:**
```
What are common anti-patterns to avoid?
```

**Expected Answer:**
```
❌ DON'T:
1. Violate layer dependencies (Application → Infrastructure)
2. Use async void (except event handlers)
3. Swallow exceptions without re-throwing
4. Hardcode configuration values
5. Skip XML documentation on public APIs
6. Use var when type is not obvious
7. Ignore CancellationToken in async methods
8. Use property injection (use constructor)
9. Use service locator pattern

✅ DO:
- Follow Clean Architecture layers
- Use async Task with CancellationToken
- Log and re-throw exceptions
- Use IOptions<T> for configuration
- Document all public APIs
- Be explicit with types when unclear
- Pass CancellationToken to all async calls
- Constructor injection only
```

---

## Scoring

```
Total Questions: 18

Your Score: ____ / 18

Interpretation:
- 18/18: Perfect! Copilot fully understands instructions
- 15-17: Good. Minor clarifications needed
- 12-14: Fair. Review key sections in instructions
- <12: Poor. Major updates needed to instructions

If score < 15:
1. Identify which questions failed
2. Update .github/copilot-instructions.md to clarify those patterns
3. Restart IDE
4. Re-test
```

---

## Continuous Validation

Run this test:
- ✅ After updating instructions
- ✅ When onboarding new team members (verify their Copilot works)
- ✅ Monthly (ensure Copilot still aligned with project)
- ✅ After major refactoring (patterns may have changed)

---

**Last Updated**: 2026-01-30  
**Maintained By**: ECO.WebApi Development Team
