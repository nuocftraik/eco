# Permission-Based Authorization

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** BUILD_16C (Function Service) đã hoàn thành

Tài liệu này hướng dẫn xây dựng Permission-Based Authorization system - bảo vệ API endpoints dựa trên permissions lưu trong database.

---

## 1. Overview

**Làm gì:** Xây dựng hệ thống Authorization dựa trên Permissions với Permission table approach - validate permissions từ database mỗi khi user gọi API.

**Tại sao cần:**
- **Security:** Bảo vệ API endpoints - chỉ users có permission mới access được
- **Flexible:** Permissions lưu trong database - có thể thay đổi mà không cần rebuild code
- **Granular Control:** Fine-grained permissions (View, Create, Update, Delete per Function)
- **Auditable:** Rõ ràng user nào có quyền gì trong database
- **Database-Driven:** Admin có thể assign/revoke permissions qua UI mà không cần deploy
- **Centralized:** Tất cả permission logic ở một chỗ, dễ maintain

**Trong bước này chúng ta sẽ:**
- ✅ Validate database design (Permission table approach)
- ✅ Tạo Permission constants (ECOAction, ECOFunction)
- ✅ Implement `UserService.GetPermissionsAsync` và `HasPermissionAsync`
- ✅ Tạo `[MustHavePermission]` attribute
- ✅ Implement `PermissionRequirement`
- ✅ Implement `PermissionAuthorizationHandler` (queries Permission table)
- ✅ Implement `PermissionPolicyProvider` (dynamic policy creation)
- ✅ Register permission services
- ✅ Tạo controller endpoints để FE lấy permissions
- ✅ Protect API endpoints với `[MustHavePermission]`

**Real-world example:**
```csharp
// FE: Login và lấy permissions
POST /api/tokens → Get JWT token
GET /api/users/me/permissions → ["Users.View", "Users.Create", "Products.View"]

// FE: Render UI dựa trên permissions
if (permissions.includes("Users.Create")) {
  showCreateButton();  // UI check (UX only)
}

// FE: Call protected API với JWT
POST /api/users
Authorization: Bearer eyJhbGci...
// FE KHÔNG gửi permission string!

// BE: Tự động validate
[MustHavePermission(ECOAction.Create, ECOFunction.User)]
public async Task<IActionResult> CreateUser(...)
{
  // 1. Extract userId from JWT
  // 2. Query Permission table
  // 3. Check if user has "Users.Create"
  // 4. If YES → execute, if NO → 403 Forbidden
}
```

---

## 2. Database Design Validation

### Bước 2.1: Review Permission Tables Schema

**Làm gì:** Validate database schema cho Permission system đã đúng chưa.

**Tại sao:** User không chắc database design đúng - cần validate và explain.

**Database Schema (from BUILD_03):**

```sql
-- Functions (Permission modules: Users, Products, Orders...)
CREATE TABLE Functions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL
);

-- Actions (Permission types: View, Create, Update, Delete...)
CREATE TABLE Actions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL
);

-- ActionInFunction (Many-to-Many: Function có những Actions nào)
CREATE TABLE ActionInFunction (
    ActionId UNIQUEIDENTIFIER NOT NULL,
    FunctionId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (ActionId, FunctionId),
    FOREIGN KEY (ActionId) REFERENCES Actions(Id) ON DELETE CASCADE,
    FOREIGN KEY (FunctionId) REFERENCES Functions(Id) ON DELETE CASCADE
);

-- Permissions (Role có quyền gì: RoleId → Function + Action)
CREATE TABLE Permissions (
    RoleId NVARCHAR(450) NOT NULL,
    FunctionId UNIQUEIDENTIFIER NOT NULL,
    ActionId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (RoleId, FunctionId, ActionId),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
    FOREIGN KEY (FunctionId) REFERENCES Functions(Id) ON DELETE CASCADE,
    FOREIGN KEY (ActionId) REFERENCES Actions(Id) ON DELETE CASCADE
);
```

**Giải thích:**

**✅ Database Design: ĐÚNG VÀ TỐT!**

**1. Function Table:**
- Chứa các modules/features trong system (Users, Products, Orders)
- Flexible - add new functions mà không cần code change

**2. Action Table:**
- Chứa các loại permissions (View, Create, Update, Delete)
- Reusable - mỗi Function có thể có các Actions khác nhau

**3. ActionInFunction Table:**
- Many-to-many relationship
- Example: Function "Users" có Actions ["View", "Create", "Update", "Delete"]
- Example: Function "Reports" chỉ có Actions ["View", "Export"]

**4. Permission Table:**
- **Composite Primary Key:** (RoleId, FunctionId, ActionId)
- Represents: "Role X có permission để perform Action Y trên Function Z"
- Example: Role "Admin" has permission "Users.Create"

**Query để lấy permissions của user:**
```sql
SELECT CONCAT(f.Name, '.', a.Name) AS Permission
FROM Permissions p
INNER JOIN AspNetUserRoles ur ON p.RoleId = ur.RoleId
INNER JOIN Functions f ON p.FunctionId = f.Id
INNER JOIN Actions a ON p.ActionId = a.Id
WHERE ur.UserId = @userId;

-- Result: ["Users.View", "Users.Create", "Products.View", ...]
```

**So sánh với Claims-based approach:**

| Aspect | Permission Table (Your Approach) | Claims-Based |
|--------|----------------------------------|--------------|
| Storage | Dedicated tables | AspNetRoleClaims |
| Flexibility | ✅ Very flexible | ⚠️ Less flexible |
| Query | SQL JOINs | LINQ on Claims |
| Admin UI | ✅ Easier to build | ⚠️ More complex |
| Reporting | ✅ Easy SQL queries | ⚠️ Harder |

**Kết luận:** Your approach is EXCELLENT for enterprise apps!

---

## 3. Permission Constants

### Bước 3.1: ECOAction Constants

**Làm gì:** Tạo constants cho các Action types (View, Create, Update, Delete...).

**Tại sao:** Avoid magic strings, IntelliSense support, type-safe.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs`

```csharp
namespace ECO.WebApi.Shared.Authorization;

/// <summary>
/// Standard action types for permissions
/// </summary>
public static class ECOAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Import = nameof(Import);
    public const string Export = nameof(Export);
}
```

**Giải thích:**

**Why Constants:**
- `nameof(View)` → "View" string
- Compile-time checking (typo → error)
- Refactoring support (rename → auto update)

**Usage:**
```csharp
[MustHavePermission(ECOAction.Create, ECOFunction.User)]
// Better than: [MustHavePermission("Create", "User")]
```

---

### Bước 3.2: ECOFunction Constants

**Làm gì:** Tạo constants cho các Function modules (Users, Products, Orders...).

**Tại sao:** Consistent naming, type-safe, avoid typos.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs` (same file)

```csharp
/// <summary>
/// System functions (modules/features)
/// Add more as system grows
/// </summary>
public static class ECOFunction
{
    public const string Dashboard = nameof(Dashboard);
    public const string User = nameof(User);
    public const string Role = nameof(Role);
    public const string Product = nameof(Product);
    public const string Category = nameof(Category);
    public const string Order = nameof(Order);
    // Add more functions as needed...
}
```

**Giải thích:**

**Function = Module:**
- `User` → User management module
- `Product` → Product management module
- Each function can have multiple actions

**Example Permission Strings:**
- `ECOAction.View + ECOFunction.User` → "Users.View"
- `ECOAction.Create + ECOFunction.Product` → "Products.Create"

---

### Bước 3.3: ECOPermission Helper

**Làm gì:** Tạo helper record để generate permission strings.

**Tại sao:** Centralize permission string format logic.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs` (same file)

```csharp
/// <summary>
/// Permission record with helper methods
/// </summary>
public record ECOPermission(string action, string function)
{
    /// <summary>
    /// Get permission name: "{function}.{action}"
    /// </summary>
    public string Name => NameFor(action, function);
    
    /// <summary>
    /// Generate permission string: "Permissions.{function}.{action}"
    /// Used by [MustHavePermission] attribute
    /// </summary>
    public static string NameFor(string action, string function) => 
        $"Permissions.{function}.{action}";
}
```

**Giải thích:**

**Permission Format:**
```
"Permissions.{FunctionName}.{ActionName}"
→ "Permissions.User.Create"
→ "Permissions.Product.View"
```

**Why "Permissions." prefix:**
- Distinguish from other policy types
- `PermissionPolicyProvider` checks if policy starts with "Permissions."

**Usage:**
```csharp
var permission = ECOPermission.NameFor(ECOAction.Create, ECOFunction.User);
// → "Permissions.User.Create"
```

---

### Bước 3.4: ECOClaims Constants

**Làm gì:** Tạo constants cho JWT claims.

**Tại sao:** Consistent claim names trong JWT tokens.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs` (same file)

```csharp
/// <summary>
/// JWT claim types
/// </summary>
public static class ECOClaims
{
    public const string Permission = "Permission";
    public const string Fullname = "fullName";
    public const string IpAddress = "ipAddress";
    public const string ImageUrl = "image_url";
    public const string Expiration = "exp";
}
```

**Giải thích:**

**Custom Claims:**
- `Permission` - used by PermissionPolicyProvider to identify permission policies
- `Fullname` - user's full name
- `IpAddress` - request IP address (security)
- `ImageUrl` - user avatar URL

---

## 4. UserService - Permission Methods

### Bước 4.1: GetPermissionsAsync Implementation

**Làm gì:** Implement method để lấy tất cả permissions của user (aggregated từ tất cả roles).

**Tại sao:** FE cần list permissions để render UI, BE cần check permissions.

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.Permission.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Identity;

internal partial class UserService
{
    /// <summary>
    /// Get all permissions for a user (aggregated from all their roles)
    /// FE calls this to get permission list for UI rendering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission strings: ["Users.View", "Users.Create", ...]</returns>
    public async Task<List<string>> GetPermissionsAsync(
        string userId, 
        CancellationToken cancellationToken)
    {
        // 1. Find user
        var user = await _userManager.FindByIdAsync(userId);
        _ = user ?? throw new UnauthorizedException("Authentication Failed.");

        // 2. Get user's roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // 3. Query Permission table with JOINs
        // SQL: SELECT DISTINCT Function.Name + '.' + Action.Name
        //      FROM Permissions
        //      INNER JOIN Functions ON Permissions.FunctionId = Functions.Id
        //      INNER JOIN Actions ON Permissions.ActionId = Actions.Id
        //      WHERE Permissions.RoleId IN (user's roles)
        var permissions = await _db.Permissions
            .Include(x => x.Role)
            .Include(x => x.Action)
            .Include(x => x.Function)
            .Where(p => userRoles.Contains(p.Role.Id))
            .Select(p => $"{p.Function.Name}.{p.Action.Name}")
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
```

**Giải thích:**

**Flow:**
1. **Find user:** Validate user exists
2. **Get roles:** Get role IDs user belongs to
3. **Query Permission table:**
   - JOIN với Function và Action tables
   - Filter by user's role IDs
   - Format: `"{Function.Name}.{Action.Name}"`
   - Distinct để remove duplicates

**Permission Format:**
```
Function.Name + '.' + Action.Name
→ "Users.View"
→ "Users.Create"
→ "Products.Update"
```

**Example Result:**
```csharp
[
  "Users.View",
  "Users.Create",
  "Users.Update",
  "Products.View",
  "Products.Create"
]
```

**Note:** Format KHÔNG có "Permissions." prefix - chỉ có "Function.Action"

---

### Bước 4.2: HasPermissionAsync Implementation

**Làm gì:** Implement method để check xem user có permission cụ thể không.

**Tại sao:** `PermissionAuthorizationHandler` gọi method này để validate permission.

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.Permission.cs` (same file)

```csharp
/// <summary>
/// Check if user has a specific permission
/// Used by PermissionAuthorizationHandler to validate API access
/// </summary>
/// <param name="userId">User ID</param>
/// <param name="permission">Permission string (e.g., "Users.Create")</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>True if user has permission, false otherwise</returns>
public async Task<bool> HasPermissionAsync(
    string userId, 
    string permission, 
    CancellationToken cancellationToken = default)
{
    // Get all permissions for user
    var permissions = await GetPermissionsAsync(userId, cancellationToken);
    
    // Check if permission exists in list
    return permissions?.Contains(permission) ?? false;
}
```

**Giải thích:**

**Simple Check:**
1. Call `GetPermissionsAsync` để lấy tất cả permissions
2. Check if permission string exists in list
3. Return true/false

**Permission String Format:**
- Input: `"Users.Create"` (WITHOUT "Permissions." prefix)
- `GetPermissionsAsync` returns: `["Users.View", "Users.Create"]`
- Contains check → true/false

**Used By:**
- `PermissionAuthorizationHandler` - validate mỗi khi user call API

**Performance Note:**
- Should add caching (see Best Practices section later)

---

## 5. Authorization Infrastructure

### Bước 5.1: MustHavePermissionAttribute

**Làm gì:** Tạo attribute để mark controllers/actions cần permission cụ thể.

**Tại sao:** Declarative authorization - rõ ràng endpoint nào cần quyền gì.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/MustHavePermissionAttribute.cs`

```csharp
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Attribute to mark controllers/actions requiring specific permission
/// Usage: [MustHavePermission(ECOAction.Create, ECOFunction.User)]
/// </summary>
public class MustHavePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Constructor
    /// Sets Policy name = "Permissions.{function}.{action}"
    /// </summary>
    /// <param name="action">Action type (View, Create, Update, Delete...)</param>
    /// <param name="function">Function name (User, Product, Order...)</param>
    public MustHavePermissionAttribute(string action, string function)
    {
        // Set Policy = "Permissions.User.Create"
        Policy = ECOPermission.NameFor(action, function);
    }
}
```

**Giải thích:**

**Inherits AuthorizeAttribute:**
- ASP.NET Core built-in attribute
- Setting `Policy` property triggers policy-based authorization

**Policy Name Format:**
```csharp
ECOPermission.NameFor("Create", "User")
→ "Permissions.User.Create"
```

**Usage Example:**
```csharp
[HttpPost]
[MustHavePermission(ECOAction.Create, ECOFunction.User)]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // Only accessible if user has "Permissions.User.Create"
}
```

**Flow:**
1. Request arrives
2. ASP.NET sees `[Authorize(Policy = "Permissions.User.Create")]`
3. Asks `IAuthorizationPolicyProvider` for policy named "Permissions.User.Create"
4. `PermissionPolicyProvider` creates `PermissionRequirement`
5. `PermissionAuthorizationHandler` validates requirement

---

### Bước 5.2: PermissionRequirement

**Làm gì:** Tạo requirement class cho permission authorization.

**Tại sao:** ASP.NET Core authorization system cần `IAuthorizationRequirement`.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/PermissionRequirement.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Represents a permission requirement that must be satisfied
/// Contains permission string to validate
/// </summary>
internal class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Permission string (e.g., "Permissions.User.Create")
    /// </summary>
    public string Permission { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="permission">Permission string to validate</param>
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
```

**Giải thích:**

**IAuthorizationRequirement:**
- Marker interface từ ASP.NET Core
- No methods - just a marker
- Represents "something that must be satisfied"

**Contains:**
- `Permission` string - e.g., "Permissions.User.Create"

**Used By:**
- `PermissionPolicyProvider` - creates this requirement
- `PermissionAuthorizationHandler` - evaluates this requirement

**Example:**
```csharp
var requirement = new PermissionRequirement("Permissions.User.Create");
// Handler will check if user has "Users.Create" permission
```

---

### Bước 5.3: PermissionAuthorizationHandler

**Làm gì:** Implement handler để validate permission requirement - queries Permission table.

**Tại sao:** Core logic để check xem user có permission không.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/PermissionAuthorizationHandler.cs`

```csharp
using ECO.WebApi.Application.Identity.Users;
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Handler that validates if user has required permission
/// Queries Permission table via UserService
/// Called automatically by ASP.NET Core on every request to protected endpoint
/// </summary>
internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserService _userService;

    public PermissionAuthorizationHandler(IUserService userService) =>
        _userService = userService;

    /// <summary>
    /// Handle permission requirement validation
    /// </summary>
    /// <param name="context">Authorization context</param>
    /// <param name="requirement">Permission requirement to validate</param>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 1. Extract userId from JWT claims
        // context.User = ClaimsPrincipal from JWT token
        // GetUserId() extension method extracts "sub" claim
        if (context.User?.GetUserId() is { } userId &&
            await _userService.HasPermissionAsync(userId, requirement.Permission))
        {
            // 2. User has permission → Succeed
            context.Succeed(requirement);
        }
        
        // 3. If not succeeded → requirement fails → 403 Forbidden
    }
}
```

**Giải thích:**

**AuthorizationHandler<PermissionRequirement>:**
- Base class từ ASP.NET Core
- Override `HandleRequirementAsync` để implement validation logic

**Validation Flow:**
1. **Extract userId:**
   - `context.User` = ClaimsPrincipal from JWT
   - `GetUserId()` extracts "sub" claim → userId

2. **Check Permission:**
   - Call `UserService.HasPermissionAsync(userId, "Permissions.User.Create")`
   - UserService queries Permission table
   - Returns true/false

3. **Result:**
   - `context.Succeed(requirement)` → authorized → 200 OK
   - Not succeeded → 403 Forbidden

**Example Flow:**
```
Request: POST /api/users
Header: Authorization: Bearer <JWT>

1. JWT Middleware validates token → sets context.User
2. [MustHavePermission] triggers authorization
3. Handler called:
   - userId = "user-id-guid" (from JWT)
   - requirement.Permission = "Permissions.User.Create"
4. Call UserService.HasPermissionAsync("user-id", "Permissions.User.Create")
5. SQL Query:
   SELECT COUNT(*)
   FROM Permissions
   WHERE UserId = 'user-id'
   AND Function.Name = 'User'
   AND Action.Name = 'Create'
6. If found → Succeed → execute controller
   If not → 403 Forbidden
```

---

### Bước 5.4: PermissionPolicyProvider

**Làm gì:** Implement policy provider để dynamically create authorization policies.

**Tại sao:** Permissions lưu trong database - cannot pre-register all policies at startup.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/PermissionPolicyProvider.cs`

```csharp
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Dynamically creates authorization policies for permissions
/// ASP.NET Core asks this provider for policies by name
/// </summary>
internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    /// <summary>
    /// Fallback to default provider for non-permission policies
    /// </summary>
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <summary>
    /// Get default policy (authenticated user)
    /// </summary>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        FallbackPolicyProvider.GetDefaultPolicyAsync();

    /// <summary>
    /// Get policy by name
    /// If policy name starts with "Permissions." → create PermissionRequirement
    /// Otherwise → fallback to default provider
    /// </summary>
    /// <param name="policyName">Policy name (e.g., "Permissions.User.Create")</param>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if permission policy
        if (policyName.StartsWith(ECOClaims.Permission, StringComparison.OrdinalIgnoreCase))
        {
            // Create policy with PermissionRequirement
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        // Fallback for other policies
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    /// <summary>
    /// Get fallback policy (optional)
    /// </summary>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        Task.FromResult<AuthorizationPolicy?>(null);
}
```

**Giải thích:**

**Why Policy Provider:**
- Permissions are **dynamic** (stored in database)
- Cannot register all policies at startup
- Policy provider creates policies **on-demand**

**Flow:**
1. **Request arrives** with `[MustHavePermission]`
2. **ASP.NET asks PolicyProvider** for policy named "Permissions.User.Create"
3. **PolicyProvider checks:**
   - If starts with "Permissions." → create `PermissionRequirement`
   - Otherwise → fallback to default provider
4. **Returns AuthorizationPolicy** with requirement
5. **PermissionAuthorizationHandler validates** requirement

**Example:**
```csharp
GetPolicyAsync("Permissions.User.Create")
→ Creates: AuthorizationPolicy {
    Requirements = [ PermissionRequirement("Permissions.User.Create") ]
  }
→ PermissionAuthorizationHandler validates this requirement
```

---

### Bước 5.5: Register Permission Services

**Làm gì:** Register permission authorization services vào DI container.

**Tại sao:** ASP.NET Core cần services để run authorization.

**File:** `src/Infrastructure/Infrastructure/Auth/Startup.cs`

```csharp
using ECO.WebApi.Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Auth;

internal static class Startup
{
    internal static IServiceCollection AddAuth(
        this IServiceCollection services, 
        IConfiguration config)
    {
        services
            .AddCurrentUser()
            .AddPermissions()  // ← Permission system
            .AddIdentity();

        services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
        return services.AddJwtAuth();
    }

    /// <summary>
    /// Register permission authorization services
    /// </summary>
    private static IServiceCollection AddPermissions(this IServiceCollection services) =>
       services
           .AddSingleton<IAuthorizationPolicy Provider, PermissionPolicyProvider>()
           .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
}
```

**Giải thích:**

**Service Lifetimes:**
- `PermissionPolicyProvider` - **Singleton**
  - Stateless - just creates policies
  - No dependencies on scoped services
  
- `PermissionAuthorizationHandler` - **Scoped**
  - Uses `IUserService` (scoped)
  - Per-request lifetime

**Registration Order:**
1. `CurrentUser` - extract user from JWT
2. `Permissions` - authorization
3. `Identity` - UserManager, RoleManager
4. `JwtAuth` - JWT middleware

---

## 6. Controller Endpoints

### Bước 6.1: Get My Permissions Endpoint

**Làm gì:** Tạo endpoint để FE lấy permissions của current user.

**Tại sao:** FE cần permission list để render UI (show/hide buttons based on permissions).

**File:** `src/Host/Host/Controllers/Identity/UserController.cs`

```csharp
using ECO.WebApi.Application.Identity.Users;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Identity;

[ApiController]
[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>
    /// Get current user's permissions
    /// FE calls this after login to get permission list for UI
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission strings</returns>
    [HttpGet("me/permissions")]
    [OpenApiOperation("Get current user's permissions.", "")]
    public async Task<List<string>> GetMyPermissionsAsync(CancellationToken cancellationToken)
    {
        // Get current user ID from JWT
        var userId = User.GetUserId();
        
        // Query Permission table via UserService
        return await _userService.GetPermissionsAsync(userId, cancellationToken);
    }

    // ... other endpoints
}
```

**Giải thích:**

**Endpoint:** `GET /api/users/me/permissions`

**FE Flow:**
```javascript
// After login, FE calls this
const response = await axios.get('/api/users/me/permissions', {
  headers: {
    Authorization: `Bearer ${jwtToken}`
  }
});

// Response: ["Users.View", "Users.Create", "Products.View", ...]
store.commit(' SET_PERMISSIONS', response.data);
```

**BE Processing:**
1. JWT middleware validates token → sets `User` (ClaimsPrincipal)
2. `User.GetUserId()` extracts userId from "sub" claim
3. Call `UserService.GetPermissionsAsync(userId)`
4. UserService queries Permission table
5. Return list of strings

**Response Example:**
```json
[
  "Users.View",
  "Users.Create",
  "Users.Update",
  "Products.View",
  "Products.Create",
  "Orders.View"
]
```

---

### Bước 6.2: Protected Endpoint Example

**Làm gì:** Example protect API endpoint với `[MustHavePermission]`.

**Tại sao:** Show how to use attribute trên controllers.

**File:** `src/Host/Host/Controllers/Identity/UserController.cs` (same file)

```csharp
/// <summary>
/// Create new user
/// Protected by Create permission
/// </summary>
[HttpPost]
[MustHavePermission(ECOAction.Create, ECOFunction.User)]
[OpenApiOperation("Creates a new user.", "")]
public async Task<IActionResult> CreateAsync(CreateUserRequest request)
{
    //  Authorization happens BEFORE this line
    // If user doesn't have "User.Create" permission → 403 Forbidden
    
    var userId = await _userService.CreateAsync(request, GetOriginFromRequest());
    return Ok(new { userId });
}

/// <summary>
/// Get users list
/// Protected by View permission
/// </summary>
[HttpGet]
[MustHavePermission(ECOAction.View, ECOFunction.User)]
[OpenApiOperation("Search users with pagination.", "")]
public async Task<PaginationResponse<UserDetailDto>> SearchAsync(
    [FromQuery] UserParameterFilter filter,
    CancellationToken cancellationToken)
{
    // Only executed if user has "User.View" permission
    return await _userService.SearchAsync(filter, cancellationToken);
}

/// <summary>
/// Update user
/// Protected by Update permission
/// </summary>
[HttpPut("{id}")]
[MustHavePermission(ECOAction.Update, ECOFunction.User)]
[OpenApiOperation("Update a user.", "")]
public async Task<IActionResult> UpdateAsync(string id, UpdateUserRequest request)
{
    if (id != request.Id)
    {
        return BadRequest("ID mismatch");
    }
    
    var currentUserId = User.GetUserId();
    await _userService.UpdateAsync(request, currentUserId);
    return Ok();
}

private string GetOriginFromRequest() =>
    $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
```

**Giải thích:**

**Each Endpoint Protected:**
- `POST /api/users` → Requires "User.Create"
- `GET /api/users` → Requires "User.View"
- `PUT /api/users/{id}` → Requires "User.Update"

**Authorization Flow (Automatic):**
```
1. FE Request: POST /api/users
   Authorization: Bearer <JWT>

2. JWT Middleware: Validates token → sets context.User

3. [MustHavePermission] Attribute: 
   Sets Policy = "Permissions.User.Create"

4. PermissionPolicyProvider:
   Creates PermissionRequirement("Permissions.User.Create")

5. PermissionAuthorizationHandler:
   - Extract userId from JWT
   - Call UserService.HasPermissionAsync(userId, "Permissions.User.Create")
   - Query Permission table
   - If found → Succeed
   - If not → 403 Forbidden

6a. Authorized → Execute controller method
6b. Not authorized → Return 403 Forbidden
```

---

## 7. Frontend-Backend Integration

### Bước 7.1: FE-BE Communication Flow

**Làm gì:** Explain complete flow từ login đến API call.

**Tại sao:** User không biết FE truyền về BE như thế nào - cần clarify.

**Complete Flow:**

```
┌─────────────────────────────────────────────────────────┐
│              FE-BE PERMISSION FLOW                      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  STEP 1: Login                                          │
│  FE → BE: POST /api/tokens                              │
│           { email, password }                           │
│  BE → FE: { accessToken: "eyJhbG...",                   │
│             refreshToken: "..." }                       │
│                                                         │
│  JWT Contains:                                          │
│  { sub: "user-id",                                      │
│    email: "admin@example.com",                          │
│    exp: 1234567890 }                                    │
│  ❌ JWT KHÔNG chứa permissions (quá dài)!               │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  STEP 2: Get Permissions                                │
│  FE → BE: GET /api/users/me/permissions                 │
│           Authorization: Bearer <JWT>                   │
│  BE Processing:                                         │
│    1. Extract userId from JWT                           │
│    2. Query Permission table:                           │
│       SELECT f.Name + '.' + a.Name                      │
│       FROM Permissions p                                │
│       WHERE p.RoleId IN (user.Roles)                    │
│  BE → FE: ["Users.View", "Users.Create", ...]           │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  STEP 3: FE Stores Permissions (UI Rendering)           │
│  store.commit('SET_PERMISSIONS', permissions);          │
│                                                         │
│  if (permissions.includes("Users.Create")) {            │
│    showCreateButton = true;  // UI check (UX only)      │
│  }                                                      │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  STEP 4: FE Calls Protected API                         │
│  FE → BE: POST /api/users                               │
│           Authorization: Bearer <JWT>                   │
│           { userData }                                  │
│  ❌ FE KHÔNG gửi: Permission string!                   │
│  ✅ FE CHỈ gửi: JWT token!                             │
│                                                         │
│  BE Authorization (Automatic):                          │
│    1. [MustHavePermission] attribute                    │
│    2. Extract userId from JWT                           │
│    3. Query Permission table                            │
│    4. Check if user has "Users.Create"                  │
│    5. If YES → execute, if NO → 403                     │
│                                                         │
│  BE → FE: { userId: "new-id" } (200 OK)                 │
│       or { error } (403 Forbidden)                      │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Giải thích:**

**Key Points:**
1. **JWT KHÔNG chứa permissions** - chỉ có userId, email, roles
2. **FE gets permissions** qua separate API call
3. **FE CHỈ gửi JWT token** - KHÔNG gửi permission strings
4. **BE tự động validate** mỗi request qua Authorization Handler

**FE Example (Vue):**
```vue
<template>
  <div>
    <!-- UI check (UX only) -->
    <button v-if="canCreateUser" @click="createUser">
      Create User
    </button>
  </div>
</template>

<script>
export default {
  computed: {
    permissions() {
      return this.$store.state.auth.permissions;
    },
    canCreateUser() {
      // Check locally for UI
      return this.permissions.includes('Users.Create');
    }
  },
  methods: {
    async createUser() {
      // Send request with JWT (NO permission string!)
      const response = await axios.post('/api/users', userData, {
        headers: {
          Authorization: `Bearer ${this.$store.state.auth.token}`
          // ❌ NOT: 'X-Permission': 'Users.Create'
        }
      });
    }
  }
}
</script>
```

---

## 8. Best Practices

### Bước 8.1: Pattern - Always Validate on Backend

**Làm gì:** Best practice - LUÔN validate permission trên backend.

**Tại sao:** FE có thể bypass - BE là source of truth.

```csharp
// ❌ BAD - No protection
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // Anyone can call this - NO validation!
    await _userService.CreateAsync(request, origin);
}

// ✅ GOOD - Backend validates
[HttpPost]
[MustHavePermission(ECOAction.Create, ECOFunction.User)]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // Protected - permission checked automatically
    await _userService.CreateAsync(request, origin);
}
```

**Giải thích:**

**Why:**
- FE can be manipulated (browser dev tools, Postman)
- FE checks are for UX ONLY (show/hide buttons)
- BE is source of truth - always validate

---

### Bước 8.2: Pattern - Cache Permissions

**Làm gì:** Cache permissions per user để improve performance.

**Tại sao:** Mỗi API call đều query Permission table → slow!

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.Permission.cs`

```csharp
using Microsoft.Extensions.Caching.Memory;

internal partial class UserService
{
    private readonly IMemoryCache _cache;

    public async Task<List<string>> GetPermissionsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"permissions_{userId}";

        // Try get from cache
        if (_cache.TryGetValue(cacheKey, out List<string> cachedPermissions))
        {
            return cachedPermissions;
        }

        // Query database
        var user = await _userManager.FindByIdAsync(userId);
        _ = user ?? throw new UnauthorizedException("Authentication Failed.");

        var userRoles = await _userManager.GetRolesAsync(user);

        var permissions = await _db.Permissions
            .Include(x => x.Role)
            .Include(x => x.Action)
            .Include(x => x.Function)
            .Where(p => userRoles.Contains(p.Role.Id))
            .Select(p => $"{p.Function.Name}.{p.Action.Name}")
            .Distinct()
            .ToListAsync(cancellationToken);

        // Cache for 10 minutes
        _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));

        return permissions;
    }
}
```

**Giải thích:**

**Caching Strategy:**
- Cache key: `"permissions_{userId}"`
- TTL: 10 minutes
- Cache per user

**Cache Invalidation:**
- When user roles change → remove cache
- When role permissions change → remove all users with that role
- Use distributed cache (Redis) for multi-server

---

## 9. Testing

### Bước 9.1: Test với Swagger

**Làm gì:** Test permission authorization với Swagger UI.

**Tại sao:** Validate system works before building FE.

**Steps:**
1. Start application
2. Open Swagger: `https://localhost:5001/swagger`
3. Login:
   - `POST /api/tokens`
   - Body: `{ "email": "admin@root.com", "password": "..." }`
   - Copy `accessToken`
4. Authorize:
   - Click "Authorize" button
   - Paste token: `Bearer <accessToken>`
5. Test protected endpoint:
   - `GET /api/users/me/permissions` → should return permission list
   - `POST /api/users` → should work if has "Users.Create"
   - `POST /api/users` without token → 401 Unauthorized
   - `POST /api/users` with token but no permission → 403 Forbidden

---

## 10. Tổng kết

**Đã xây dựng:**

✅ **Database Design Validated:**
- Permission table approach (RoleId + FunctionId + ActionId)
- SQL schema và queries
- Design comparison (Permission table vs Claims)

✅ **Permission Constants:**
- `ECOAction` - action types
- `ECOFunction` - modules
- `ECOPermission` - helper
- `ECOClaims` - JWT claims

✅ **UserService Methods:**
- `GetPermissionsAsync` - return List<string>
- `HasPermissionAsync` - check specific permission

✅ **Authorization Infrastructure:**
- `[MustHavePermission]` attribute
- `PermissionRequirement`
- `PermissionAuthorizationHandler` (queries Permission table)
- `PermissionPolicyProvider` (dynamic policies)
- Service registration

✅ **Controller Endpoints:**
- `GET /api/users/me/permissions` - FE gets permissions
- Protected endpoints với `[MustHavePermission]`

✅ **FE-BE Integration:**
- Complete 4-step flow explained
- FE GỬI: JWT token (Authorization header)
- FE KHÔNG GỬI: Permission strings
- BE validates automatically

✅ **Best Practices:**
- Backend always validates
- FE permissions for UX only
- Caching strategy

**Database Design:** ✅ ĐÚNG VÀ TUYỆT VỜI!

**Next:** BUILD_18 - OAuth2 Integration

---
