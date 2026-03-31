# User Rules – ECO Project (Clean Architecture)

> These rules apply to ALL conversations in this workspace. Always follow them when generating code, documentation, or architecture decisions for the ECO project.

---

## 🏗️ Project Architecture

**Layer dependency order (outer → inner):**
```
Host → Infrastructure → Application → Domain → Shared
```

**Directory structure:**
```
src/
├── Shared/
├── Domain/
├── Application/
├── Infrastructure/
├── Migrators.MSSQL/
└── Host/
```

**Dependency rules (STRICTLY enforced):**
- ✅ Host → Infrastructure, Application
- ✅ Infrastructure → Application, Domain
- ✅ Application → Domain, Shared
- ✅ Domain → Shared
- ✅ Shared → NOTHING
- ❌ NEVER let inner layers depend on outer layers

---

## 📐 Naming Conventions

| Type | Pattern | Examples |
|------|---------|---------|
| Interfaces | `I{Capability}Service` | `IMailService`, `IFileStorageService` |
| Implementations | `{Technology}{Capability}Service` | `SmtpMailService`, `AzureBlobStorageService` |
| Settings classes | `{Feature}Settings` (must match appsettings.json) | `JwtSettings`, `DatabaseSettings` |
| DTOs & Requests | `{Action}{Entity}{Type}` | `CreateUserRequest`, `UpdateProductRequest` |

---

## 🔧 Core Patterns

### Dependency Injection

Use marker interfaces for auto-registration:
```csharp
public interface IMyService : ITransientService { }
public interface IMyService : IScopedService { }
public interface IMyService : ISingletonService { }
```

Constructor injection (mandatory):
```csharp
public MyService(IOptions<MySettings> settings, ILogger<MyService> logger)
{
    _settings = settings.Value; // Always unwrap IOptions
    _logger = logger;
}
```

### Configuration Binding

Settings class name **MUST** match the appsettings.json section name:
```csharp
public class MyFeatureSettings { public string ApiKey { get; set; } = default!; }
// appsettings.json:
{ "MyFeatureSettings": { "ApiKey": "secret" } }
// Registration:
services.Configure<MyFeatureSettings>(config.GetSection(nameof(MyFeatureSettings)));
```

### MediatR Handler Pattern
```csharp
public class CreateProductRequest : IRequest<Guid> { public string Name { get; set; } = default!; }

public class CreateProductHandler : IRequestHandler<CreateProductRequest, Guid>
{
    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken ct)
    {
        // Business logic
        return productId;
    }
}
```

### File Organization
```
src/Application/Common/{Feature}/
├── I{Feature}Service.cs
├── {Action}{Entity}Request.cs
└── {Action}{Entity}Response.cs

src/Infrastructure/{Feature}/
├── {Tech}{Feature}Service.cs
├── {Feature}Settings.cs
└── Startup.cs
```

---

## 🤖 AI Behavior Rules

### When Generating Code

1. **Search workspace patterns first** – Look for similar existing services/handlers before creating new ones.
2. **Check `docs/BUILD_INDEX.md`** – If the feature is already documented, follow its exact patterns.
3. **Apply naming and layer conventions consistently** (see above).

### When Creating a New Feature – Step-by-Step

```
Step 1: Read docs/BUILD_INDEX.md → find similar existing feature
Step 2: Read that BUILD_XX doc → understand patterns
Step 3: Create Application layer
        → Interface (I{Feature}Service : ITransientService)
        → DTOs ({Action}{Entity}Request/Response)
Step 4: Create Infrastructure layer
        → Implementation ({Tech}{Feature}Service)
        → Settings ({Feature}Settings)
        → Startup (Add{Feature}() extension method)
Step 5: Wire into Infrastructure/Startup.cs → services.Add{Feature}(config)
Step 6: Document in docs/BUILD_XX_{Feature}.md (follow MODULE_DOCUMENTATION_TEMPLATE.md)
Step 7: Update docs/BUILD_INDEX.md → add entry in appropriate PHASE
```

### When to Ask the User

Ask before proceeding if:
- Layer dependency is unclear
- Multiple valid approaches exist
- Breaking changes are needed
- Security-sensitive operation
- Database schema changes
- API contract changes

Proceed confidently if:
- Following an established pattern from `docs/BUILD_XX_*.md`
- Creating standard CRUD
- Adding XML documentation
- Writing tests
- Refactoring within the same layer

---

## 🚫 Anti-Patterns (NEVER DO)

```csharp
// ❌ Layer violation
using ECO.WebApi.Infrastructure.Persistence; // In Application layer

// ❌ Hardcoded config
var apiKey = "hardcoded-secret";

// ❌ Missing CancellationToken
public async Task MethodAsync(Request request)

// ❌ Swallow exceptions
catch (Exception ex) { return null; }

// ❌ async void
public async void ProcessAsync()
```

## ✅ Always Do

```csharp
// ✅ Layer separation – Infrastructure implements Application interfaces

// ✅ Use IOptions<T>
public MyService(IOptions<MySettings> settings) { _settings = settings.Value; }

// ✅ Always accept CancellationToken
public async Task MethodAsync(Request request, CancellationToken ct)

// ✅ Log and re-throw
catch (Exception ex) { _logger.LogError(ex, "Context"); throw; }

// ✅ async Task
public async Task ProcessAsync(CancellationToken ct)
```

---

## 📖 Documentation Standards

When creating documentation for a new feature, follow this mandatory structure:

1. Overview (What, Why, Checklist, Real-world example)
2. Add Required Packages
3. Application Layer (Interfaces, DTOs)
4. Infrastructure Layer (Implementations, Settings, Startup)
5. Configuration (appsettings.json)
6. Usage Examples (3+ scenarios)
7. Best Practices
8. Testing
9. Troubleshooting
10. Summary

**Code rules in docs:**
- ✅ FULL CODE – no placeholders like `// ...existing code...`
- ✅ Self-contained – rebuild solution from docs alone
- ✅ XML documentation on public APIs
- ✅ Comments explain WHY, not WHAT
- ✅ Namespace = `{ProjectName}.{Layer}.{Feature}`

---

## 🎯 Key Principles

1. **Documentation-Driven** – Read `docs/BUILD_INDEX.md` first
2. **Pattern Consistency** – Copy patterns from `docs/BUILD_XX_*.md`
3. **Layer Sacred** – Never violate dependency rules
4. **Self-Contained** – Code in docs must compile standalone
5. **WHY over WHAT** – Comments explain reasoning
6. **Full Code** – No placeholders in documentation

---

## 📚 Quick Reference Table

| Need | Location |
|------|----------|
| Roadmap & module list | `docs/BUILD_INDEX.md` |
| Doc writing template | `docs/MODULE_DOCUMENTATION_TEMPLATE.md` |
| Naming conventions | This file |
| Layer structure | `docs/BUILD_INDEX.md` → PHASE 1 |
| DI patterns | `docs/BUILD_INDEX.md` → Bước 8, 10 |
| Repository pattern | `docs/BUILD_INDEX.md` → Bước 11 |
| Authentication | `docs/BUILD_INDEX.md` → PHASE 4 |
| Infrastructure services | `docs/BUILD_INDEX.md` → PHASE 5 |
| Feature examples | `docs/BUILD_XX_*.md` (30+ modules) |
