# ECO.WebApi - Agent Rules

> 🤖 **Purpose**: Development rules for AI agents working on ECO.WebApi
> 📅 **Last Updated**: 2026-01-28
> 👤 **Maintained By**: vuongnv1206

---

## 🎯 CORE PRINCIPLES

### 1. Clean Architecture STRICT Adherence
**NEVER violate dependency rules:**
```
✅ Shared ← Domain ← Application ← Infrastructure ← Host
❌ Domain → Infrastructure (FORBIDDEN)
❌ Application → Host (FORBIDDEN)
```

### 2. Self-Documenting Code
**ALWAYS include:**
- XML documentation on public APIs
- Clear variable/method names
- Business logic comments (WHY, not WHAT)

### 3. Production-Ready Code
**Every generated code must:**
- Compile without warnings
- Pass StyleCop + SonarAnalyzer
- Include error handling
- Support cancellation tokens
- Use async/await properly

---

## 📝 CODE GENERATION RULES

### When generating entities:
```csharp
✅ DO:
- Inherit from AuditableEntity (if needs audit)
- Implement IAggregateRoot (for repositories)
- Private setters on properties
- Factory method (Create) instead of public constructor
- Domain events for state changes
- Business validation in methods

❌ DON'T:
- Public setters
- Parameterless public constructors
- Anemic domain models
- Database concerns in entities
```

### When generating DTOs:
```csharp
✅ DO:
- Suffix: Request/Response/Dto
- XML documentation
- Data annotations (if needed for Swagger)
- default! for required properties

❌ DON'T:
- Return entities from API
- Include navigation properties
- Complex business logic
```

### When generating handlers:
```csharp
✅ DO:
- One handler per request
- Inject only what's needed
- Use repository for data access
- Return DTOs (never entities)
- Log important operations
- Use specification pattern for queries

❌ DON'T:
- Direct DbContext access
- Multiple responsibilities
- Catch generic exceptions
```

### When generating validators:
```csharp
✅ DO:
- Inherit from CustomValidator<T>
- Use helper methods (MustNotBeEmpty, etc.)
- Async validation for database checks
- Clear error messages
- Validate business rules

❌ DON'T:
- Skip validation
- Generic error messages
- Database operations without async
```

---

## 🗂️ FILE ORGANIZATION RULES

### Folder Structure
```
Application/
├── [Feature]/
│   ├── [Entity]/
│   │   ├── Create[Entity]Request.cs      # Command
│   │   ├── Update[Entity]Request.cs      # Command
│   │   ├── Delete[Entity]Request.cs      # Command
│   │   ├── Get[Entity]ByIdRequest.cs     # Query
│   │   ├── Search[Entity]Request.cs      # Query
│   │   └── [Entity]Dto.cs               # DTO
```

### File Naming
```
✅ CreateProductRequest.cs
✅ ProductDto.cs
✅ IEmailService.cs
❌ product_request.cs
❌ ProductViewModel.cs
```

---

## 🔒 SECURITY RULES

### Authentication/Authorization
```csharp
✅ DO:
- Check authentication with [Authorize]
- Check permissions with [MustHavePermission]
- Validate user owns resource
- Log security events

❌ DON'T:
- Trust client input
- Expose sensitive data in errors
- Skip authorization checks
- Hardcode credentials
```

### Input Validation
```csharp
✅ DO:
- Validate ALL inputs
- Sanitize user input
- Use FluentValidation
- Check business rules

❌ DON'T:
- Trust any input
- Skip validation
- Use string concatenation for SQL
```

---

## 🧪 TESTING RULES

### Unit Tests
```csharp
✅ DO:
- Test business logic
- Mock dependencies
- Test edge cases
- One assertion per test
- Arrange-Act-Assert pattern

❌ DON'T:
- Test framework code
- Test private methods directly
- Multiple assertions (unless related)
```

### Integration Tests
```csharp
✅ DO:
- Test API endpoints
- Use WebApplicationFactory
- Test database operations
- Test authentication/authorization

❌ DON'T:
- Test every possible scenario (unit tests)
- Rely on specific database state
```

---

## 📊 DATABASE RULES

### Entity Configuration
```csharp
✅ DO:
- Fluent API in separate config class
- Specify max lengths
- Create indexes on FKs
- Use schemas for organization
- Configure relationships explicitly

❌ DON'T:
- Data annotations (use Fluent API)
- Cascade deletes (use Restrict)
- Missing indexes on FKs
```

### Migrations
```csharp
✅ DO:
- Descriptive migration names
- Check migration before apply
- Include rollback plan
- Test on non-production first

❌ DON'T:
- Edit migrations after applied
- Delete migrations
- Skip migration review
```

---

## 🎨 CODE STYLE RULES

### Naming Conventions
```csharp
✅ PascalCase: Classes, Methods, Properties, Constants
✅ camelCase: Parameters, local variables
✅ _camelCase: Private fields
✅ SCREAMING_CASE: Never use
```

### Code Organization
```csharp
✅ Order in class:
1. Constants
2. Fields
3. Constructors
4. Properties
5. Methods (public → private)

✅ Use regions: Never (rely on outlining)
✅ File header: XML doc only
```

---

## 🚫 ANTI-PATTERNS TO AVOID

### God Classes
```csharp
❌ public class ProductService
{
    // 50 methods doing everything
}

✅ public class CreateProductHandler { }
✅ public class UpdateProductHandler { }
✅ public class DeleteProductHandler { }
```

### Anemic Domain Models
```csharp
❌ public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    // No behavior
}

✅ public class Product
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw ...
        Price = newPrice;
    }
}
```

### Service Locator
```csharp
❌ var service = ServiceLocator.Get<IService>();

✅ Constructor injection
```

### Primitive Obsession
```csharp
❌ public decimal Amount { get; set; }
❌ public string Currency { get; set; }

✅ public Money Amount { get; set; }
```

---

## 🔄 WORKFLOW RULES

### Before Committing Code
1. ✅ Run `dotnet build` - no warnings
2. ✅ Run `dotnet test` - all pass
3. ✅ Check StyleCop warnings
4. ✅ Review generated code
5. ✅ Update documentation if needed

### When Adding New Feature
1. ✅ Plan domain model first
2. ✅ Create entities (Domain)
3. ✅ Create interfaces (Application)
4. ✅ Create DTOs + validators (Application)  
5. ✅ Create handlers (Application)
6. ✅ Implement services (Infrastructure)
7. ✅ Add controllers (Host)
8. ✅ Create migration
9. ✅ Test thoroughly

### When Modifying Existing Code
1. ✅ Understand current implementation
2. ✅ Check for breaking changes
3. ✅ Update tests
4. ✅ Update documentation
5. ✅ Migration if schema changed

---

## 🎯 AGENT-SPECIFIC RULES

### When asked to generate CRUD:
```
1. Generate entity (Domain)
2. Generate DTOs (Application)
3. Generate all CRUD requests (Application)
4. Generate validators (Application)
5. Generate handlers (Application)
6. Generate controller (Host)
7. Generate EF Core configuration (Infrastructure)
8. Suggest migration command
```

### When asked to review code:
```
1. Check Clean Architecture compliance
2. Check naming conventions
3. Check for anti-patterns
4. Check error handling
5. Check validation
6. Check XML documentation
7. Check async/await usage
8. Suggest improvements
```

### When asked to refactor:
```
1. Explain current issues
2. Propose solution
3. Show before/after
4. Highlight breaking changes
5. Update related code
6. Update tests
7. Update documentation
```

---

## ⚡ PERFORMANCE RULES

### Database Queries
```csharp
✅ DO:
- Use AsNoTracking for reads
- Use Select projections
- Paginate large results
- Create appropriate indexes
- Use async methods

❌ DON'T:
- Load entire tables
- Use Include for DTOs
- N+1 queries
- Synchronous database calls
```

### API Responses
```csharp
✅ DO:
- Return only needed data
- Use pagination
- Cache when appropriate
- Compress responses (middleware)

❌ DON'T:
- Return entire entities
- Expose internal IDs
- Include sensitive data
```

---

## 📚 DOCUMENTATION RULES

### ⭐ Self-Contained Principle (CRITICAL)

**Definition:** Documentation MUST contain COMPLETE, EXACT code that compiles and runs without reference to actual codebase.

**Rules:**
```
✅ MUST DO:
1. Copy EXACT code from actual implementation
   - Include ALL using statements
   - Include ALL interfaces implemented
   - Include ALL methods (public, private, protected)
   - Include ALL properties with exact initializers
   - Match actual code 100% - no simplification

2. Verify code completeness:
   - Can reader copy-paste and compile immediately?
   - Are all dependencies shown?
   - Are all members present?
   - Is business logic complete?

3. Explain WHY for every part:
   - Why this interface? (e.g., IValidatableObject)
   - Why this method? (e.g., Validate())
   - Why this initialization? (e.g., string.Empty vs default!)
   - Business reasons, not just technical

❌ NEVER DO:
- Simplify code for "readability" - show REAL code
- Skip "implementation details" - ALL details matter
- Use placeholders (// ..., etc.)
- Assume "obvious" parts can be omitted
- Show different code than actual implementation
```

**Example - WRONG (Incomplete):**
```csharp
// ❌ BAD: Missing interface, method, using
public class JwtSettings
{
    public string Key { get; set; } = default!;
    public int TokenExpirationInMinutes { get; set; }
}
```

**Example - CORRECT (Self-Contained):**
```csharp
// ✅ GOOD: Complete, matches actual implementation
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

**Verification Checklist:**
Before documenting any code, verify:
- [ ] Viewed actual implementation file
- [ ] Copied ALL using statements
- [ ] Copied ALL interface implementations
- [ ] Copied ALL members (fields, properties, methods)
- [ ] Matched exact initializers (string.Empty vs default!)
- [ ] Included ALL method bodies
- [ ] Explained WHY for each part
- [ ] Code compiles when copy-pasted alone

**When in doubt:** Copy from actual code file, don't simplify!

---

### XML Documentation
```csharp
✅ Required for:
- Public classes
- Public methods
- Public properties
- Interfaces

✅ Include:
- <summary> - what it does
- <param> - parameter description
- <returns> - return value
- <exception> - when exceptions thrown
```

### Code Comments
```csharp
✅ DO comment:
- WHY (business logic)
- Complex algorithms
- Workarounds
- TODO with ticket number

❌ DON'T comment:
- WHAT (obvious from code)
- Outdated info
- Commented-out code
```

---

## 🎓 LEARNING RESOURCES

When generating code, refer to:
- `.agent/memories/01_architecture.md` - Architecture patterns
- `.agent/memories/02_coding_standards.md` - Coding conventions
- `.agent/memories/03_database_patterns.md` - Database patterns
- `docs/BUILD_INDEX.md` - Full documentation
- `docs/MODULE_DOCUMENTATION_TEMPLATE.md` - Doc template

---

**Last Updated**: 2026-01-28
**Maintained By**: vuongnv1206
