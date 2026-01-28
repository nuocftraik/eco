# BUILD_16C: Function Service

> 📘 **Mục đích:** Xây dựng Function Management Service - quản lý Functions (Permission modules) và Actions.

> [!NOTE]
> **Part of BUILD_16 Identity Services Series**
> 
> - BUILD_16A: User Service
> - BUILD_16B: Role Service
> - **BUILD_16C (This file):** Function Service

---

## 🤖 AI Generation Metadata

```yaml
---
ai_metadata:
  generated_by: "ai_assisted"
  reviewed_by: "vuongnv1206"
  last_updated: "2026-01-28"
  layer: "Application + Infrastructure"
  patterns_used:
    - "Service Layer Pattern"
    - "FluentValidation"
    - "DTO Pattern"
    - "Repository Pattern"
  dependencies:
    - "BUILD_01_Solution_Setup"
    - "BUILD_03_Domain_Layer"
    - "BUILD_04_Application_Layer"
    - "BUILD_07_Database_Initialization"
    - "BUILD_11_Repository_Pattern"
    - "BUILD_16A_User_Service"
    - "BUILD_16B_Role_Service"
  ai_instructions: |
    When working with Function Service:
    1. Function = Permission module (e.g., "Users", "Products", "Orders")
    2. Each Function has multiple Actions (e.g., "Read", "Write", "Delete")
    3. Functions are domain entities (not Identity framework)
    4. Use Repository pattern for data access
    5. Simple service - only 4 methods
---
```

---

## 📋 Tổng quan

**Function Service** quản lý Functions (Permission modules) trong hệ thống.

**Concept:**
- **Function** = Permission module (Users, Products, Orders, etc.)
- **Action** = Permission type (Read, Write, Delete, etc.)
- **Permission** = Function + Action combination

**Example Structure:**
```
Function: "Users"
├─ Action: "Read"    → Permission: "Permissions.Users.Read"
├─ Action: "Write"   → Permission: "Permissions.Users.Write"
└─ Action: "Delete"  → Permission: "Permissions.Users.Delete"

Function: "Products"
├─ Action: "Read"    → Permission: "Permissions.Products.Read"
└─ Action: "Write"   → Permission: "Permissions.Products.Write"
```

**Quan hệ:**
```
Function ─┐
          ├─ has many → Actions
          └─ used by → Roles (via permissions)
```

---

## 1. Function Request

### Bước 1.1: CreateOrUpdateFunctionRequest

**Làm gì:** Request để create hoặc update function.

**File:** `src/Core/Application/Identity/Roles/CreateOrUpdateFunctionRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Roles;

public class CreateOrUpdateFunctionRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = default!;
}

public class CreateOrUpdateFunctionRequestValidator : AbstractValidator<CreateOrUpdateFunctionRequest>
{
    public CreateOrUpdateFunctionRequestValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
}
```

**Giải thích:**

**CreateOrUpdate Pattern:**
- `Id == null` → Create new function
- `Id != null` → Update existing function

**Validation:**
- Name required
- Max length 200 characters
- Expression-bodied constructor

---

## 2. IFunctionService Interface

### Bước 2.1: Interface Definition

**Làm gì:** Define contract cho Function Service.

**File:** `src/Core/Application/Identity/Roles/IFunctionService.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Roles;

public interface IFunctionService : ITransientService
{
    Task<List<FunctionDto>> GetListAsync(CancellationToken cancellationToken);
    Task<FunctionDto> GetByIdAsync(Guid id);
    Task<string> CreateOrUpdateAsync(CreateOrUpdateFunctionRequest request);
    Task<string> DeleteAsync(Guid id);
}
```

**Giải thích:**

**4 Methods Total:**
1. `GetListAsync` - List all functions with actions
2. `GetByIdAsync` - Get single function by ID
3. `CreateOrUpdateAsync` - Create or update function
4. `DeleteAsync` - Delete function

Simple CRUD - no complex logic.

---

## 3. FunctionService Implementation

### Bước 3.1: Implementation

**File:** `src/Infrastructure/Infrastructure/Identity/FunctionService.cs`

```csharp
using ECO.WebApi.Application.Identity.Roles;
using ECO.WebApi.Domain.Identity;

namespace ECO.WebApi.Infrastructure.Identity;

internal class FunctionService : IFunctionService
{
    private readonly IRepository<Function> _repository;

    public FunctionService(IRepository<Function> repository)
    {
        _repository = repository;
    }

    public async Task<List<FunctionDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var functions = await _repository.ListAsync(cancellationToken);
        
        return functions.Select(f => new FunctionDto
        {
            Id = f.Id,
            Name = f.Name,
            ActionDtos = f.Actions.Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Selected = false // Default - will be set by RoleService
            }).ToList()
        }).ToList();
    }

    public async Task<FunctionDto> GetByIdAsync(Guid id)
    {
        var function = await _repository.GetByIdAsync(id);
        if (function == null)
        {
            throw new NotFoundException("Function Not Found.");
        }

        return new FunctionDto
        {
            Id = function.Id,
            Name = function.Name,
            ActionDtos = function.Actions.Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Selected = false
            }).ToList()
        };
    }

    public async Task<string> CreateOrUpdateAsync(CreateOrUpdateFunctionRequest request)
    {
        if (request.Id == null || request.Id == Guid.Empty)
        {
            // Create new function
            var function = new Function(request.Name);
            await _repository.AddAsync(function);
            return "Function Created Successfully.";
        }
        else
        {
            // Update existing function
            var function = await _repository.GetByIdAsync(request.Id.Value);
            if (function == null)
            {
                throw new NotFoundException("Function Not Found.");
            }

            function.Update(request.Name);
            await _repository.UpdateAsync(function);
            return "Function Updated Successfully.";
        }
    }

    public async Task<string> DeleteAsync(Guid id)
    {
        var function = await _repository.GetByIdAsync(id);
        if (function == null)
        {
            throw new NotFoundException("Function Not Found.");
        }

        await _repository.DeleteAsync(function);
        return "Function Deleted Successfully.";
    }
}
```

**Giải thích:**

**Dependencies:**
- `IRepository<Function>` - Generic repository for data access
- NO UserManager, RoleManager (Functions are domain entities, not Identity)

**Implementation:**
- Simple CRUD operations
- Use Repository pattern
- Map to DTOs
- Standard error handling

---

## 4. Function Domain Entity

### Bước 4.1: Function Entity (Reference)

**File:** `src/Core/Domain/Identity/Function.cs` (from BUILD_03)

```csharp
namespace ECO.WebApi.Domain.Identity;

public class Function : BaseEntity
{
    public string Name { get; private set; }
    public ICollection<Action> Actions { get; private set; }

    private Function() { } // EF Core

    public Function(string name)
    {
        Name = name;
        Actions = new List<Action>();
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void AddAction(string actionName)
    {
        var action = new Action(actionName, this.Id);
        Actions.Add(action);
    }
}
```

**Giải thích:**

**Domain Entity:**
- Encapsulated properties (private setters)
- Constructor for creation
- Methods for mutations (Update, AddAction)
- Navigation property to Actions

**Not Identity Framework:**
- Custom entity (not from Microsoft.AspNetCore.Identity)
- Stored in regular database table (not AspNetRoles)

---

### Bước 4.2: Action Entity (Reference)

**File:** `src/Core/Domain/Identity/Action.cs` (from BUILD_03)

```csharp
namespace ECO.WebApi.Domain.Identity;

public class Action : BaseEntity
{
    public string Name { get; private set; }
    public Guid FunctionId { get; private set; }
    public Function Function { get; private set; }

    private Action() { } // EF Core

    public Action(string name, Guid functionId)
    {
        Name = name;
        FunctionId = functionId;
    }

    public void Update(string name)
    {
        Name = name;
    }
}
```

**Giải thích:**
- Action belongs to Function (many-to-one)
- Encapsulated domain entity

---

##  5. Key Patterns & Decisions

### Pattern 1: Domain Entities (Not Identity Framework)

**Why separate entities:**
```
ASP.NET Core Identity:
├─ ApplicationUser (Framework)
└─ ApplicationRole (Framework)

Custom Domain:
├─ Function (Custom)
└─ Action (Custom)
```

**Benefits:**
- Control over schema
- Domain logic in entities
- Repository pattern works
- Easier testing

---

### Pattern 2: Repository Pattern

**Usage:**
```csharp
private readonly IRepository<Function> _repository;

// Simple CRUD
await _repository.AddAsync(function);
await _repository.UpdateAsync(function);
await _repository.DeleteAsync(function);
await _repository.GetByIdAsync(id);
await _repository.ListAsync(cancellationToken);
```

**Benefits:**
- Abstraction over data access
- Easy to test (mock repository)
- Consistent API

---

### Pattern 3: Encapsulated Entities

**Domain-Driven Design:**
```csharp
// ❌ BAD
function.Name = "NewName"; // Direct property access

// ✅ GOOD
function.Update("NewName"); // Method expresses intent
```

**Benefits:**
- Business rules in entity
- Clear intent
- Easier to maintain invariants

---

## 6. How Functions/Actions Are Used

### Usage Flow:

**1. Admin creates Functions & Actions:**
```
POST /api/functions
{
    "name": "Users"
}

// Then add Actions to Function (separate API or seeding)
```

**2. Admin assigns Permissions to Roles:**
```
POST /api/roles/{roleId}/permissions
{
    "permissions": [
        { "functionId": "...", "actionId": "..." }  // Users.Read
        { "functionId": "...", "actionId": "..." }  // Users.Write
    ]
}
```

**3. System converts to permission string:**
```csharp
$"Permissions.{function.Name}.{action.Name}"
// → "Permissions.Users.Read"
// → "Permissions.Users.Write"
```

**4. Permission stored as Claim on Role:**
```csharp
await _roleManager.AddClaimAsync(role, 
    new Claim(ECOClaims.Permission, "Permissions.Users.Write"));
```

**5. User gets permissions from their roles:**
```csharp
// User → Roles → Claims (Permissions)
var permissions = await _userService.GetPermissionsAsync(userId);
// Returns: ["Permissions.Users.Read", "Permissions.Users.Write", ...]
```

**6. Authorization checks permission:**
```csharp
[MustHavePermission("Permissions.Users.Write")]
public async Task<IActionResult> UpdateUser(...)
```

---

## 7. Tổng kết BUILD_16C

**Đã xây dựng:**

✅ **Request:**
- CreateOrUpdateFunctionRequest (with validator)

✅ **IFunctionService Interface:**
- 4 simple CRUD methods

✅ **FunctionService Implementation:**
- Repository pattern
- Domain entity mapping
- Standard error handling

✅ **Domain Understanding:**
- Function & Action entities
- Link to permission system
- Not part of Identity framework

---

## 8. Tổng kết toàn bộ BUILD_16 Series

**BUILD_16A: User Service** (~900 lines)
- 20 methods
- Partial classes organization
- Complex business logic

**BUILD_16B: Role Service** (~350 lines)
- 7 methods
- Permission assignment
- Claims-based permissions

**BUILD_16C: Function Service** (~250 lines)
- 4 methods
- Domain entities
- Repository pattern

**Total:** ~1500 lines of comprehensive Identity Services documentation! ✅

**Next in BUILD_INDEX:**

➡️ **BUILD_17:** Permission Authorization (middleware, policy-based authorization)  
➡️ **BUILD_18:** OAuth2 Integration (Google/Facebook login)
