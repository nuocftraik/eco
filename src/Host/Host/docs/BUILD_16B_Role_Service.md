# BUILD_16B: Role Service

> 📘 **Mục đích:** Xây dựng Role Management Service - quản lý roles và assign permissions cho roles.

> [!NOTE]
> **Part of BUILD_16 Identity Services Series**
> 
> - BUILD_16A: User Service
> - **BUILD_16B (This file):** Role Service  
> - BUILD_16C: Function Service

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
  dependencies:
    - "BUILD_01_Solution_Setup"
    - "BUILD_03_Domain_Layer" 
    - "BUILD_04_Application_Layer"
    - "BUILD_07_Database_Initialization"
    - "BUILD_16A_User_Service"
  ai_instructions: |
    When working with Role Service:
    1. RoleService manages ASP.NET Core Identity Roles
    2. Roles can have multiple Permissions (stored as Claims)
    3. Role name must be unique
    4. Return DTOs, not entities
    5. CreateOrUpdate pattern (single method for both)
---
```

---

## 📋 Tổng quan

**Role Service** quản lý roles trong hệ thống và gán permissions cho roles.

**Quan hệ:**
```
Role (Admin, Manager, User...)
  ↓ has many
Functions (UserManagement, ProductManagement...)
  ↓ each has
Actions (Read, Write, Delete...)
```

**Example:**
```
Role: "Product Manager"
├─ Function: "Products"
│   ├─ Action: Read ✓
│   ├─ Action: Write ✓
│   └─ Action: Delete ✓
└─ Function: "Categories"
    ├─ Action: Read ✓
    └─ Action: Write ✓
```

---

## 1. Role DTOs

### Bước 1.1: RoleDto

**Làm gì:** DTO để return role information.

**File:** `src/Core/Application/Identity/Roles/RoleDto.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Roles;

public class RoleDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}
```

**Giải thích:**
- `Id` - Role ID (string in ASP.NET Core Identity)
- `Name` - Role name (Admin, Manager, etc.)
- `Description` - Optional description
- `Permissions` - List of permission strings (optional - loaded when needed)

---

### Bước 1.2: FunctionDto

**Làm gì:** DTO để return function (permission module) với actions.

**File:** `src/Core/Application/Identity/Roles/FunctionDto.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Roles;

public class FunctionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<ActionDto> ActionDtos { get; set; } = new();
}
```

**Giải thích:**
- Function = Permission module (UserManagement, ProductManagement, etc.)
- Each function has multiple actions

---

### Bước 1.3: ActionDto

**Làm gì:** DTO để return action (permission type).

**File:** `src/Core/Application/Identity/Roles/ActionDto.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Roles;

public class ActionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool Selected { get; set; }
}
```

**Giải thích:**
- Action = Permission type (Read, Write, Delete, etc.)
- `Selected` - is this action enabled for current role?

---

## 2. Role Requests

### Bước 2.1: CreateOrUpdateRoleRequest

**Làm gì:** Request để create hoặc update role.

**File:** `src/Core/Application/Identity/Roles/CreateOrUpdateRoleRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Roles;

public class CreateOrUpdateRoleRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateOrUpdateRoleRequestValidator : AbstractValidator<CreateOrUpdateRoleRequest>
{
    public CreateOrUpdateRoleRequestValidator(IRoleService roleService) =>
        RuleFor(r => r.Name)
            .NotEmpty()
            .MustAsync(async (role, name, _) => !await roleService.ExistsAsync(name, role.Id))
                .WithMessage("Similar Role already exists.");
}
```

**Giải thích:**

**Create vs Update:**
- `Id == null` → Create new role
- `Id != null` → Update existing role

**Validation:**
- Role name must be unique
- Check uniqueness except for current role (when updating)
- Expression-bodied constructor

---

### Bước 2.2: UpdateRolePermissionsRequest

**Làm gì:** Request để update permissions cho role.

**File:** `src/Core/Application/Identity/Roles/UpdateRolePermissionsRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Roles;

public class UpdateRolePermissionsRequest
{
    public string RoleId { get; set; } = default!;
    public List<PermissionRequest> Permissions { get; set; } = default!;
}

public class PermissionRequest
{
    public Guid FunctionId { get; set; } = default!;
    public Guid ActionId { get; set; } = default!;
}

public class UpdateRolePermissionsRequestValidator : AbstractValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsRequestValidator()
    {
        RuleFor(r => r.RoleId)
            .NotEmpty();
    }
}
```

**Giải thích:**

**Permission Structure:**
- Each permission = Function + Action combination
- Example: `{FunctionId: "UserManagement", ActionId: "Write"}` → permission "Permissions.UserManagement.Write"

**Request:**
- Send list of all enabled permissions
- Service will replace all current permissions

---

## 3. IRoleService Interface

### Bước 3.1: Interface Definition

**Làm gì:** Define contract cho Role Service.

**File:** `src/Core/Application/Identity/Roles/IRoleService.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Roles;

public interface IRoleService : ITransientService
{
    // Role CRUD
    Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken);
    Task<int> GetCountAsync(CancellationToken cancellationToken);
    Task<RoleDto> GetByIdAsync(string id);
    Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request);
    Task<string> DeleteAsync(string id);
    
    // Validation
    Task<bool> ExistsAsync(string roleName, string? excludeId);
    
    // Permissions
    Task<List<FunctionDto>> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken);
    Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
}
```

**Giải thích:**

**Method Groups:**

**1. Role CRUD (5 methods):**
- Standard CRUD operations
- CreateOrUpdate pattern (single method)

**2. Validation (1 method):**
- Check role name uniqueness

**3. Permissions (2 methods):**
- Get role with all functions/actions (selected flags set)
- Update role permissions

---

## 4. RoleService Implementation

### Bước 4.1: Implementation Highlights

**File:** `src/Infrastructure/Infrastructure/Identity/RoleService.cs`

> [!NOTE]
> **Implementation Note**
> 
> RoleService là single file (KHÔNG dùng partial classes như UserService) vì chỉ có 7 methods.

```csharp
using ECO.WebApi.Application.Identity.Roles;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ECO.WebApi.Infrastructure.Identity;

internal class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IReadRepository<Function> _functionRepo;
    private readonly IStringLocalizer _localizer;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        IReadRepository<Function> functionRepo,
        IStringLocalizer<RoleService> localizer)
    {
        _roleManager = roleManager;
        _functionRepo = functionRepo;
        _localizer = localizer;
    }

    public async Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            // Create new role
            var role = new ApplicationRole
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new InternalServerException("Failed to create role.", result.GetErrors(_localizer));
            }

            return "Role Created Successfully.";
        }
        else
        {
            // Update existing role
            var role = await _roleManager.FindByIdAsync(request.Id);
            if (role == null)
            {
                throw new NotFoundException("Role Not Found.");
            }

            role.Name = request.Name;
            role.Description = request.Description;
            role.NormalizedName = request.Name.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                throw new InternalServerException("Failed to update role.", result.GetErrors(_localizer));
            }

            return "Role Updated Successfully.";
        }
    }

    public async Task<List<FunctionDto>> GetByIdWithPermissionsAsync(
        string roleId, 
        CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            throw new NotFoundException("Role Not Found.");
        }

        // Get all role claims (permissions)
        var roleClaims = await _roleManager.GetClaimsAsync(role);
        var rolePermissions = roleClaims
            .Where(c => c.Type == ECOClaims.Permission)
            .Select(c => c.Value)
            .ToList();

        // Get all functions with actions
        var functions = await _functionRepo.ListAsync(cancellationToken);

        var functionDtos = functions.Select(f => new FunctionDto
        {
            Id = f.Id,
            Name = f.Name,
            ActionDtos = f.Actions.Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Selected = rolePermissions.Contains($"Permissions.{f.Name}.{a.Name}")
            }).ToList()
        }).ToList();

        return functionDtos;
    }

    public async Task<string> UpdatePermissionsAsync(
        UpdateRolePermissionsRequest request, 
        CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        if (role == null)
        {
            throw new NotFoundException("Role Not Found.");
        }

        // Remove all current permission claims
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var permissionClaims = currentClaims.Where(c => c.Type == ECOClaims.Permission);
        foreach (var claim in permissionClaims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Add new permission claims
        var functions = await _functionRepo.ListAsync(cancellationToken);
        foreach (var permission in request.Permissions)
        {
            var function = functions.FirstOrDefault(f => f.Id == permission.FunctionId);
            var action = function?.Actions.FirstOrDefault(a => a.Id == permission.ActionId);

            if (function != null && action != null)
            {
                var permissionValue = $"Permissions.{function.Name}.{action.Name}";
                await _roleManager.AddClaimAsync(role, 
                    new Claim(ECOClaims.Permission, permissionValue));
            }
        }

        return "Role Permissions Updated Successfully.";
    }

    public async Task<bool> ExistsAsync(string roleName, string? excludeId)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        return role != null && role.Id != excludeId;
    }

    public async Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken)
    {
        return await _roleManager.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        await _roleManager.Roles.CountAsync(cancellationToken);

    public async Task<RoleDto> GetByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            throw new NotFoundException("Role Not Found.");
        }

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description
        };
    }

    public async Task<string> DeleteAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            throw new NotFoundException("Role Not Found.");
        }

        // Check if role is assigned to any users
        var usersInRole = await _roleManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            throw new ConflictException("Role is assigned to users. Cannot delete.");
        }

        await _roleManager.DeleteAsync(role);
        return "Role Deleted Successfully.";
    }
}
```

---

## 5. Key Patterns & Decisions

### Pattern 1: Permissions as Claims

**Implementation:**
- Permissions stored as Claims on Role
- Claim Type: `ECOClaims.Permission`
- Claim Value: `Permissions.{FunctionName}.{ActionName}`

**Example:**
```csharp
new Claim(ECOClaims.Permission, "Permissions.Users.Write")
new Claim(ECOClaims.Permission, "Permissions.Products.Delete")
```

**Benefits:**
- ASP.NET Core Identity built-in
- No additional tables needed
- Queryable with standard Identity methods

---

### Pattern 2: CreateOrUpdate Single Method

**Why:**
```csharp
// Single method handles both
if (string.IsNullOrEmpty(request.Id))
    // Create
else
    // Update
```

**Benefits:**
- Less code duplication
- Simpler API
- Common pattern for admin UIs

---

### Pattern 3: Check Before Delete

```csharp
var usersInRole = await _roleManager.GetUsersInRoleAsync(role.Name!);
if (usersInRole.Any())
{
    throw new ConflictException("Role is assigned to users. Cannot delete.");
}
```

**Why:**
- Data integrity
- Prevent orphaned references
- User-friendly error message

---

## 6. Tổng kết BUILD_16B

**Đã xây dựng:**

✅ **DTOs:**
- RoleDto, FunctionDto, ActionDto

✅ **Requests:**
- CreateOrUpdateRoleRequest (with validator)
- UpdateRolePermissionsRequest (with validator)
- PermissionRequest (nested)

✅ **IRoleService Interface:**
- 7 methods for role and permission management

✅ **RoleService Implementation:**
- Single file (not partial - simple enough)
- Permissions as Claims pattern
- CreateOrUpdate pattern
- Safe delete with validation

**Next:**

➡️ **BUILD_16C:** Function Service (4 methods - simplest!)
