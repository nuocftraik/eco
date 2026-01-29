# Permission-Based Authorization - Dynamic Permission System

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** Bước 16C (Function Service) đã hoàn thành

Tài liệu này hướng dẫn xây dựng Permission-Based Authorization System - Dynamic permission checks với ASP.NET Core Authorization.

---

## 1. Overview

**Làm gì:** Xây dựng Permission-Based Authorization System để protect API endpoints dựa trên dynamic permissions stored in database.

**Tại sao cần:**
- **Dynamic Authorization:** Permissions stored in database, not hardcoded
- **Fine-grained Access Control:** Check specific permissions (e.g., "Users.View", "Products.Create")
- **Declarative Security:** Use attributes `[MustHavePermission("Users", "View")]` on controllers
- **JWT-based Checks:** Permissions stored in JWT claims for fast authorization
- **Complete Permission Flow:** Function + Action + Role → JWT Claims → Authorization Handler

**Trong bước này chúng ta sẽ:**
- ✅ Tạo PermissionRequirement (IAuthorizationRequirement)
- ✅ Tạo PermissionAuthorizationHandler (check permissions from JWT claims)
- ✅ Tạo PermissionPolicyProvider (dynamic policy creation)
- ✅ Tạo MustHavePermissionAttribute (declarative attribute)
- ✅ Update TokenService để add permissions to JWT claims
- ✅ Update UserService.Permission.cs (GetPermissionsAsync, HasPermissionAsync)
- ✅ Register authorization services trong Startup
- ✅ Testing với protected endpoints

**Real-world example:**
```csharp
// Controller với permission protection
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    // Only users với "Users.View" permission có thể access
    [HttpGet]
    [MustHavePermission(ECOAction.View, ECOFunction.User)]
    public Task<List<UserDto>> GetAllAsync()
    {
        // Implementation
    }

    // Only users với "Users.Create" permission có thể access
    [HttpPost]
    [MustHavePermission(ECOAction.Create, ECOFunction.User)]
    public Task<string> CreateAsync(CreateUserRequest request)
    {
        // Implementation
    }
}

// Authorization Flow:
// 1. User logs in → TokenService generates JWT với permissions in claims
// 2. User calls API với JWT token
// 3. PermissionPolicyProvider creates policy "Permissions.Users.View"
// 4. PermissionAuthorizationHandler checks JWT claims
// 5. Has permission? → Allow request
//    No permission? → 403 Forbidden
```

---

## 2. Authorization System Architecture

### Bước 2.1: Authorization Flow Overview

**Complete Flow Diagram:**

```
┌─────────────────────────────────────────────────────────┐
│        PERMISSION-BASED AUTHORIZATION FLOW       │
└─────────────────────────────────────────────────────────┘

1. USER LOGIN
┌──────────────┐
│ POST /token  │
└──────┬───────┘
       │ { email, password }
       ▼
┌──────────────────┐
│  TokenService    │
└──────┬───────────┘
       │ 1. Validate credentials
       │ 2. Get user's roles (UserRoles table)
   │ 3. Get permissions from Permission table
       │    Query: SELECT Function.Name + '.' + Action.Name
       │           FROM Permission P
       │           JOIN Function F ON P.FunctionId = F.Id
       │        JOIN Action A ON P.ActionId = A.Id
│           WHERE P.RoleId IN (user's roles)
  │ 4. Build JWT claims
  ▼
┌──────────────────┐
│   JWT Token      │
│  with Claims:    │
│  - NameIdentifier│
│  - Email   │
│  - Fullname    │
│  - permission:   │
│    "Users.View"  │
│  - permission:   │
│    "Users.Create"│
│  - permission:   │
│    "Products.View"│
└──────┬───────────┘
       │
   ▼ (Client stores token)

2. API CALL WITH AUTHORIZATION
┌──────────────────┐
│ GET /api/users   │
│ [MustHavePermission("View", "User")]
└──────┬───────────┘
    │ Authorization: Bearer {JWT}
   ▼
┌──────────────────────────┐
│ ASP.NET Core Pipeline    │
└──────┬───────────────────┘
       │ 1. Validate JWT signature
       │ 2. Extract claims from JWT
     ▼
┌──────────────────────────┐
│ PermissionPolicyProvider │
└──────┬───────────────────┘
       │ 3. Create policy "Permissions.User.View"
    │ 4. Add PermissionRequirement("Permissions.User.View")
       ▼
┌─────────────────────────────┐
│ PermissionAuthorizationHandler│
└──────┬──────────────────────┘
       │ 5. Check JWT claims
       │    Has claim "permission" = "Users.View"?
       │    → Call UserService.HasPermissionAsync()
       │   (Optional: double-check from database)
       ▼
┌──────────────────┐
│  Authorization   │
│  Decision        │
└──────┬───────────┘
       │
       ├─► ✅ Has Permission → Allow Request (200 OK)
       │
       └─► ❌ No Permission → Deny Request (403 Forbidden)
```

---

### Bước 2.2: Key Components

**1. PermissionRequirement (IAuthorizationRequirement):**
- Represents a permission requirement
- Contains permission string (e.g., "Permissions.Users.View")

**2. PermissionAuthorizationHandler (AuthorizationHandler):**
- Handles permission requirements
- Checks if user has required permission in JWT claims
- Calls `UserService.HasPermissionAsync()` to verify

**3. PermissionPolicyProvider (IAuthorizationPolicyProvider):**
- Dynamically creates authorization policies
- Converts permission string → AuthorizationPolicy

**4. MustHavePermissionAttribute (AuthorizeAttribute):**
- Declarative attribute for controllers/actions
- Syntax: `[MustHavePermission(ECOAction.View, ECOFunction.User)]`
- Generates policy name: "Permissions.User.View"

**5. TokenService:**
- Adds permissions to JWT claims during login
- Claims: `new Claim(ECOClaims.Permission, "Users.View")`

**6. UserService.Permission.cs:**
- `GetPermissionsAsync()`: Query permissions from database
- `HasPermissionAsync()`: Check if user has specific permission

---

## 3. Authorization Constants

### Bước 3.1: ECOAction Constants

**Làm gì:** Define available actions (operations).

**Tại sao:** Standard actions để tái sử dụng across functions.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs` (partial)

```csharp
namespace ECO.WebApi.Shared.Authorization;

/// <summary>
/// Standard actions (operations) available in the system
/// Used to build permissions: Permissions.{Function}.{Action}
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
    public const string Clean = nameof(Clean);
}
```

**Giải thích:**
- **View:** Read/display data
- **Search:** Search với filters
- **Create:** Create new entities
- **Update:** Update existing entities
- **Delete:** Delete entities
- **Import:** Import data from external sources
- **Export:** Export data to files (Excel, CSV)
- **Clean:** Clean up old data

---

### Bước 3.2: ECOFunction Constants

**Làm gì:** Define available functions (modules/features).

**Tại sao:** Standard functions để build permissions.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs` (partial)

```csharp
/// <summary>
/// Functions (modules/features) available in the system
/// Used to build permissions: Permissions.{Function}.{Action}
/// </summary>
public static class ECOFunction
{
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string User = nameof(User);
    public const string UserRole = nameof(UserRole);
    public const string Role = nameof(Role);
    public const string RoleClaim = nameof(RoleClaim);
    public const string Product = nameof(Product);
    public const string Category = nameof(Category);
}
```

**Giải thích:**
- Each constant represents a module/feature
- Used to build permission strings: `Permissions.{Function}.{Action}`
- Example: `Permissions.User.View`, `Permissions.Product.Create`

---

### Bước 3.3: ECOPermission Record

**Làm gì:** Helper record để generate permission strings.

**Tại sao:** Type-safe permission generation và helper methods.

**File:** `src/Core/Shared/Authorization/ECOPermissions.cs` (partial)

```csharp
/// <summary>
/// Permission record (Helper for permission string generation)
/// Format: "Permissions.{Function}.{Action}"
/// Example: "Permissions.User.View"
/// </summary>
public record ECOPermission(string action, string function)
{
    /// <summary>
    /// Permission name (format: Permissions.Function.Action)
  /// </summary>
    public string Name => NameFor(action, function);

    /// <summary>
    /// Generate permission name from action and function
    /// </summary>
    public static string NameFor(string action, string function) => 
        $"Permissions.{function}.{action}";

    /// <summary>
    /// Generate all permissions for a function (all actions)
    /// Example: GeneratePermissionsForFunction("User")
    /// Returns: ["Permissions.User.View", "Permissions.User.Create", ...]
    /// </summary>
    public static List<string> GeneratePermissionsForFunction(string function)
    {
        // Get all action constants using reflection
     var actions = typeof(ECOAction)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
         .Where(field => field.IsLiteral && !field.IsInitOnly) // Only constants
   .Select(field => field.GetValue(null)?.ToString())
       .Where(value => value != null)
            .ToList();

        // Generate permission strings
        return actions
            .Select(action => $"Permissions.{function}.{action}")
     .ToList();
    }

    /// <summary>
    /// Generate permissions for a function with specific actions
 /// Example: GeneratePermissionsForFunction("User", ["View", "Create"])
    /// Returns: ["Permissions.User.View", "Permissions.User.Create"]
    /// </summary>
    public static List<string> GeneratePermissionsForFunction(string function, List<string> actions)
    {
        if (actions == null || actions.Count == 0)
  throw new ArgumentException("Actions list cannot be null or empty", nameof(actions));

        return actions
      .Select(action => $"Permissions.{function}.{action}")
      .ToList();
    }
}
```

**Giải thích:**

**Permission Format:**
- Standard format: `Permissions.{Function}.{Action}`
- Example: `Permissions.User.View`, `Permissions.Product.Create`

**Helper Methods:**
- **NameFor():** Generate single permission string
- **GeneratePermissionsForFunction(function):** Generate all permissions for function
- **GeneratePermissionsForFunction(function, actions):** Generate specific permissions

**Usage Examples:**
```csharp
// Single permission
var permission = ECOPermission.NameFor(ECOAction.View, ECOFunction.User);
// → "Permissions.User.View"

// All permissions for User function
var allUserPermissions = ECOPermission.GeneratePermissionsForFunction(ECOFunction.User);
// → ["Permissions.User.View", "Permissions.User.Create", "Permissions.User.Update", ...]

// Specific permissions for Product function
var productPermissions = ECOPermission.GeneratePermissionsForFunction(
    ECOFunction.Product, 
    new List<string> { ECOAction.View, ECOAction.Create });
// → ["Permissions.Product.View", "Permissions.Product.Create"]
```

---

### Bước 3.4: ECOClaims Constants

**Làm gì:** Define JWT claim names.

**Tại sao:** Consistent claim names across application.

**File:** `src/Core/Shared/Authorization/ECOClaims.cs`

```csharp
namespace ECO.WebApi.Shared.Authorization;

/// <summary>
/// JWT claim names
/// </summary>
public static class ECOClaims
{
    /// <summary>
    /// Full name claim (FirstName + LastName)
    /// </summary>
    public const string Fullname = "fullName";

  /// <summary>
    /// Permission claim (multiple claims with this name)
    /// Format: "Permissions.Function.Action"
    /// Example: "Permissions.User.View"
    /// </summary>
    public const string Permission = "permission";

    /// <summary>
    /// Image URL claim (avatar)
    /// </summary>
    public const string ImageUrl = "image_url";

    /// <summary>
    /// IP Address claim
/// </summary>
    public const string IpAddress = "ipAddress";

    /// <summary>
    /// Expiration claim (standard JWT claim)
    /// </summary>
    public const string Expiration = "exp";
}
```

**Giải thích:**
- **Permission:** Multiple claims với cùng tên (one claim per permission)
- Standard JWT claims: NameIdentifier, Email, Name, Surname
- Custom claims: Fullname, Permission, ImageUrl, IpAddress

---

## 4. Permission Authorization Components

### Bước 4.1: PermissionRequirement

**Làm gì:** Authorization requirement for permission checks.

**Tại sao:** Represents a permission requirement in authorization pipeline.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/PermissionRequirement.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Permission requirement (implements IAuthorizationRequirement)
/// Represents a permission that must be checked
/// </summary>
internal class PermissionRequirement : IAuthorizationRequirement
{
/// <summary>
    /// Permission string (format: "Permissions.Function.Action")
    /// Example: "Permissions.User.View"
    /// </summary>
    public string Permission { get; private set; }

    public PermissionRequirement(string permission)
  {
     Permission = permission;
    }
}
```

**Giải thích:**
- Implements `IAuthorizationRequirement` (ASP.NET Core Authorization)
- Stores permission string to be checked
- Used by `PermissionAuthorizationHandler`

---

### Bước 4.2: PermissionAuthorizationHandler

**Làm gì:** Authorization handler to check permissions.

**Tại sao:** Evaluates permission requirements against user's claims.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/PermissionAuthorizationHandler.cs`

```csharp
using System.Security.Claims;
using ECO.WebApi.Application.Identity.Users;
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Authorization handler for permission requirements
/// Checks if user has required permission in JWT claims
/// </summary>
internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserService _userService;

    public PermissionAuthorizationHandler(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Handle permission requirement
    /// Checks if user has required permission
    /// </summary>
    protected override async Task HandleRequirementAsync(
     AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
     // Get user ID from JWT claims
        if (context.User?.GetUserId() is { } userId &&
        // Check if user has permission (from JWT claims or database)
         await _userService.HasPermissionAsync(userId, requirement.Permission))
        {
 // User has permission → Succeed
    context.Succeed(requirement);
        }

        // If not succeeded → Authorization fails (403 Forbidden)
    }
}
```

**Giải thích:**

**HandleRequirementAsync Flow:**
1. Get user ID from JWT claims (`context.User.GetUserId()`)
2. Call `UserService.HasPermissionAsync()` to check permission
3. If user has permission → `context.Succeed(requirement)`
4. If not → Authorization fails (handler doesn't call Succeed)

**Why call UserService.HasPermissionAsync():**
- Permissions are stored in JWT claims (fast check)
- Optional: Double-check from database (for revoked permissions)
- Flexible: Can implement caching strategy

**Authorization Result:**
- **Succeed:** User has permission → Request allowed (200 OK)
- **Not Succeed:** User doesn't have permission → 403 Forbidden

---

### Bước 4.3: PermissionPolicyProvider

**Làm gì:** Dynamic policy provider for permission-based policies.

**Tại sao:** Creates authorization policies on-the-fly based on permission strings.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/PermissionPolicyProvider.cs`

```csharp
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// Permission policy provider (dynamic policy creation)
/// Creates authorization policies based on permission strings
/// </summary>
internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    /// <summary>
    /// Fallback policy provider (for non-permission policies)
    /// </summary>
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
     FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <summary>
    /// Get default policy (not used for permissions)
    /// </summary>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
        FallbackPolicyProvider.GetDefaultPolicyAsync();

  /// <summary>
    /// Get policy by name
    /// If policy name starts with "Permissions", create permission policy
    /// Otherwise, use fallback provider
    /// </summary>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if policy is a permission policy
        if (policyName.StartsWith(ECOClaims.Permission, StringComparison.OrdinalIgnoreCase))
        {
     // Create permission policy dynamically
            var policy = new AuthorizationPolicyBuilder();
      policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

      // Use fallback for non-permission policies
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    /// <summary>
/// Get fallback policy (not used for permissions)
    /// </summary>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
        Task.FromResult<AuthorizationPolicy?>(null);
}
```

**Giải thích:**

**GetPolicyAsync Flow:**
1. Check if policy name starts with "Permissions" (e.g., "Permissions.User.View")
2. If yes → Create `AuthorizationPolicy` với `PermissionRequirement`
3. If no → Use fallback provider (for other policies như Roles)

**Why Dynamic Policy Creation:**
- Không cần register từng permission policy
- Policies được tạo on-the-fly based on permission string
- Scalable: Support unlimited permissions

**Example:**
```csharp
// Attribute on controller:
[MustHavePermission(ECOAction.View, ECOFunction.User)]
// → Policy name: "Permissions.User.View"

// PermissionPolicyProvider creates:
// AuthorizationPolicy with PermissionRequirement("Permissions.User.View")

// PermissionAuthorizationHandler checks:
// Does user have permission "Permissions.User.View"?
```

---

### Bước 4.4: MustHavePermissionAttribute

**Làm gì:** Declarative attribute for permission-based authorization.

**Tại sao:** Easy-to-use attribute for controllers/actions.

**File:** `src/Infrastructure/Infrastructure/Auth/Permissions/MustHavePermissionAttribute.cs`

```csharp
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;

/// <summary>
/// MustHavePermission attribute (declarative authorization)
/// Usage: [MustHavePermission(ECOAction.View, ECOFunction.User)]
/// Generates policy: "Permissions.User.View"
/// </summary>
public class MustHavePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Constructor with action and function parameters
    /// </summary>
    /// <param name="action">Action (e.g., ECOAction.View)</param>
    /// <param name="function">Function (e.g., ECOFunction.User)</param>
    public MustHavePermissionAttribute(string action, string function)
    {
        // Generate policy name: "Permissions.{Function}.{Action}"
        Policy = ECOPermission.NameFor(action, function);
    }
}
```

**Giải thích:**

**How It Works:**
1. Attribute sets `Policy` property (từ `AuthorizeAttribute`)
2. Policy name format: `Permissions.{Function}.{Action}`
3. ASP.NET Core Authorization pipeline calls `PermissionPolicyProvider.GetPolicyAsync(policyName)`
4. Policy provider creates policy với `PermissionRequirement`
5. `PermissionAuthorizationHandler` evaluates requirement

**Usage Examples:**
```csharp
// Controller-level permission
[ApiController]
[Route("api/users")]
[MustHavePermission(ECOAction.View, ECOFunction.User)] // All actions require Users.View
public class UsersController : ControllerBase
{
    // ...
}

// Action-level permission
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [MustHavePermission(ECOAction.View, ECOFunction.User)]
    public Task<List<UserDto>> GetAllAsync()
    {
      // Only users với "Permissions.User.View" permission
    }

    [HttpPost]
    [MustHavePermission(ECOAction.Create, ECOFunction.User)]
 public Task<string> CreateAsync(CreateUserRequest request)
    {
        // Only users với "Permissions.User.Create" permission
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ECOAction.Delete, ECOFunction.User)]
    public Task DeleteAsync(string id)
    {
        // Only users với "Permissions.User.Delete" permission
    }
}
```

---

## 5. UserService - Permission Operations

### Bước 5.1: UserService.Permission.cs (Partial Class)

**Làm gì:** Implement permission query operations.

**Tại sao:** Get user's permissions from database và check permissions.

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.Permission.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Identity;

/// <summary>
/// UserService - Permission Operations (Partial Class)
/// </summary>
internal partial class UserService
{
    /// <summary>
    /// Get user's permissions from database
 /// Returns list of permission strings (Format: "Function.Action")
    /// Note: Stored in Permission table as (RoleId, FunctionId, ActionId)
    /// </summary>
    public async Task<List<string>> GetPermissionsAsync(
        string userId, 
        CancellationToken cancellationToken)
    {
        // Find user
  var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
    throw new UnauthorizedException("Authentication Failed.");
        }

        // Get user's roles (from UserRoles table - Identity)
        var userRoles = await _userManager.GetRolesAsync(user);

        // Query permissions from Permission table
        // JOIN: Permission → Role → Function → Action
        var permissions = await _db.Permissions
            .Include(p => p.Role)
            .Include(p => p.Function)
       .Include(p => p.Action)
      .Where(p => userRoles.Contains(p.Role.Name!)) // Filter by user's roles
       .Select(p => $"{p.Function.Name}.{p.Action.Name}") // Format: "Function.Action"
      .Distinct()
       .ToListAsync(cancellationToken);

        return permissions;
    }

    /// <summary>
    /// Check if user has specific permission
    /// Used by PermissionAuthorizationHandler
    /// </summary>
    public async Task<bool> HasPermissionAsync(
  string userId, 
        string permission, 
        CancellationToken cancellationToken = default)
    {
        // Get user's permissions
 var permissions = await GetPermissionsAsync(userId, cancellationToken);

   // Check if permission exists in list
     // Permission format: "Permissions.Function.Action" (from JWT claims)
        // OR "Function.Action" (from database)
        // So we need to normalize comparison
        var normalizedPermission = permission
   .Replace("Permissions.", "", StringComparison.OrdinalIgnoreCase);

        return permissions?.Contains(normalizedPermission) ?? false;
    }
}
```

**Giải thích:**

**GetPermissionsAsync Flow:**
1. Find user by ID
2. Get user's roles (`_userManager.GetRolesAsync()`)
3. Query Permission table:
   - JOIN với Role, Function, Action
   - WHERE Role.Name IN (user's roles)
   - SELECT Function.Name + '.' + Action.Name
4. Return distinct permissions

**HasPermissionAsync:**
- Called by `PermissionAuthorizationHandler`
- Checks if user has specific permission
- Normalizes permission string (remove "Permissions." prefix if present)

**Permission Format:**
- **In Database:** `"Users.View"` (Function.Action)
- **In JWT Claims:** `"Permissions.Users.View"` (with prefix)
- **Comparison:** Normalize to `"Users.View"` format

**Why Include Relations:**
- Eager loading: Load Role, Function, Action in single query
- Avoid N+1 query problem
- Better performance

---

## 6. TokenService - Add Permissions to JWT

### Bước 6.1: Update TokenService.GetClaims()

**Làm gì:** Add permissions to JWT claims during login.

**Tại sao:** Permissions stored in JWT for fast authorization checks.

**File:** `src/Infrastructure/Infrastructure/Identity/TokenService.cs` (partial - update existing method)

```csharp
using ECO.WebApi.Application.Identity.Tokens;
using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Auth.Jwt;
using ECO.WebApi.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Shared.Authorization;

namespace ECO.WebApi.Infrastructure.Identity;

internal class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly SecuritySettings _securitySettings;
    private readonly JwtSettings _jwtSettings;

    public TokenService(
        UserManager<ApplicationUser> userManager,
    IUserService userService,
  IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings)
    {
        _userManager = userManager;
    _userService = userService;
     _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
  }

    // ... existing methods ...

    /// <summary>
    /// Generate JWT token with claims
    /// </summary>
    private string GenerateJwt(ApplicationUser user, string ipAddress) =>
        GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user, ipAddress));

    /// <summary>
    /// Get claims for JWT token
    /// Includes permissions from database
    /// </summary>
  private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user, string ipAddress)
    {
      // Standard claims
        var claims = new List<Claim>
  {
            new(ClaimTypes.NameIdentifier, user.Id),
   new(ClaimTypes.Email, user.Email!),
      new(ECOClaims.Fullname, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
          new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ECOClaims.IpAddress, ipAddress),
            new(ECOClaims.ImageUrl, user.ImageUrl ?? string.Empty),
      new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
        };

        // Add permissions to claims
        // Query permissions from database
        var permissions = await _userService.GetPermissionsAsync(user.Id, CancellationToken.None);

        // Add each permission as a separate claim
        // Multiple claims with same name (ECOClaims.Permission)
        foreach (var permission in permissions)
        {
          // Add with "Permissions." prefix for consistency
      claims.Add(new Claim(ECOClaims.Permission, $"Permissions.{permission}"));
        }

        return claims;
    }

    /// <summary>
    /// Generate encrypted JWT token
    /// </summary>
    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
     claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
     signingCredentials: signingCredentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    // ... other existing methods ...
}
```

**Giải thích:**

**GetClaimsAsync Changes:**
- Changed from synchronous `GetClaims()` to async `GetClaimsAsync()`
- Query permissions from database: `_userService.GetPermissionsAsync()`
- Add each permission as separate claim
- Format: `new Claim(ECOClaims.Permission, "Permissions.Function.Action")`

**Multiple Claims với Same Name:**
- JWT supports multiple claims với cùng tên
- Example JWT payload:
```json
{
  "nameid": "user-id",
  "email": "user@example.com",
  "permission": "Permissions.Users.View",
  "permission": "Permissions.Users.Create",
  "permission": "Permissions.Products.View"
}
```

**Why Add Permissions to JWT:**
- **Fast Authorization:** No database query per request
- **Stateless:** All info in JWT token
- **Scalable:** No session storage needed

**⚠️ Important Note:**
- Need to update `GenerateJwt()` to call async `GetClaimsAsync()`
- Update all method signatures to async if needed

---

## 7. Register Authorization Services

### Bước 7.1: Auth Startup Configuration

**Làm gì:** Register authorization services trong dependency injection.

**Tại sao:** Configure ASP.NET Core Authorization với custom components.

**File:** `src/Infrastructure/Infrastructure/Auth/Startup.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Infrastructure.Auth.Jwt;
using ECO.WebApi.Infrastructure.Auth.OAuth2;
using ECO.WebApi.Infrastructure.Auth.Permissions;
using ECO.WebApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Auth;

internal static class Startup
{
    /// <summary>
    /// Add authentication and authorization services
    /// </summary>
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddCurrentUser()
            .AddPermissions() // ← Add permission services
            // Must add identity before adding auth!
            .AddIdentity();

      services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
        services.AddO2Authentication(config);
        return services.AddJwtAuth();
    }

    /// <summary>
    /// Use current user middleware
    /// </summary>
    internal static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app) =>
      app.UseMiddleware<CurrentUserMiddleware>();

    /// <summary>
    /// Add current user services
    /// </summary>
    private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
        services
   .AddScoped<CurrentUserMiddleware>()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

    /// <summary>
    /// Add permission-based authorization services
    /// </summary>
    private static IServiceCollection AddPermissions(this IServiceCollection services) =>
        services
  // Register PermissionPolicyProvider as Singleton
            // Singleton: Policy provider doesn't have state, safe to share
 .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
   // Register PermissionAuthorizationHandler as Scoped
            // Scoped: Handler needs UserService (scoped), so handler must be scoped too
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
}
```

**Giải thích:**

**AddPermissions() Method:**
- **PermissionPolicyProvider:** Registered as Singleton
  - Policy provider doesn't have state
  - Safe to share across requests
  - Better performance

- **PermissionAuthorizationHandler:** Registered as Scoped
  - Handler depends on `IUserService` (scoped service)
  - Must match service lifetime
  - New instance per request

**Service Lifetimes:**
```
Singleton← PermissionPolicyProvider
    │
    ├─ Same instance for all requests
    └─ No state, thread-safe

Scoped  ← PermissionAuthorizationHandler
    │
    ├─ New instance per request
    ├─ Can depend on other scoped services (UserService)
    └─ Disposed at end of request

Transient
    │
    ├─ New instance every time injected
    └─ Short-lived services
```

**Registration Order:**
1. `AddCurrentUser()` - Register current user services
2. `AddPermissions()` - Register authorization services
3. `AddIdentity()` - Register ASP.NET Core Identity
4. `AddJwtAuth()` - Register JWT authentication

---

## 8. Testing Permission Authorization

### Bước 8.1: Test Setup - Create Test User with Permissions

**Step 1: Create Manager Role (already done in BUILD_16B)**
```csharp
// Manager role đã được tạo trong RoleService tests
```

**Step 2: Assign Permissions to Manager Role**

**API Call:**
```bash
curl -X PUT https://localhost:7001/api/role/{managerRoleId}/permissions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {adminToken}" \
  -d '{
    "roleId": "{managerRoleId}",
    "permissions": [
    {
        "functionId": "{usersFunction}",
        "actionId": "{viewAction}"
      },
      {
     "functionId": "{usersFunction}",
 "actionId": "{createAction}"
 },
      {
        "functionId": "{productsFunction}",
        "actionId": "{viewAction}"
      }
    ]
  }'
```

**Expected Response:**
```json
{
  "message": "Permissions Updated."
}
```

**Step 3: Assign Manager Role to Test User**

**API Call:**
```bash
curl -X POST https://localhost:7001/api/users/{userId}/roles \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {adminToken}" \
  -d '{
    "userRoles": [
      {
        "roleId": "{managerRoleId}",
        "roleName": "Manager",
     "enabled": true
      }
    ]
  }'
```

---

### Bước 8.2: Test Login and JWT Claims

**API Call:**
```bash
curl -X POST https://localhost:7001/api/tokens \
  -H "Content-Type: application/json" \
  -d '{
  "email": "manager@example.com",
    "password": "SecurePass123!"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "refreshTokenExpiryTime": "2024-02-29T12:00:00Z"
}
```

**Verify JWT Claims (Decode JWT on jwt.io):**
```json
{
  "nameid": "user-id",
  "email": "manager@example.com",
  "fullName": "Manager User",
  "permission": "Permissions.User.View",
  "permission": "Permissions.User.Create",
  "permission": "Permissions.Product.View",
  "exp": 1706529600,
  "iss": "ECO.WebApi",
  "aud": "ECO.WebApi"
}
```

✅ **Verify:** JWT contains multiple `permission` claims

---

### Bước 8.3: Test Protected Endpoint - Success Case

**Scenario:** Manager user calls `GET /api/users` (requires "Users.View" permission)

**API Call:**
```bash
curl -X GET https://localhost:7001/api/users \
  -H "Authorization: Bearer {managerToken}"
```

**Expected Response:**
```json
[
  {
    "id": "user-1",
    "userName": "admin",
    "firstName": "Admin",
    "lastName": "User",
    "email": "admin@example.com",
    "isActive": true
  },
  {
    "id": "user-2",
    "userName": "manager",
    "firstName": "Manager",
    "lastName": "User",
    "email": "manager@example.com",
    "isActive": true
  }
]
```

**✅ Success:** Manager has "Users.View" permission → Request allowed

---

### Bước 8.4: Test Protected Endpoint - Forbidden Case

**Scenario:** Manager user calls `DELETE /api/users/{id}` (requires "Users.Delete" permission)

**API Call:**
```bash
curl -X DELETE https://localhost:7001/api/users/{userId} \
  -H "Authorization: Bearer {managerToken}"
```

**Expected Response:**
```json
{
  "statusCode": 403,
  "message": "You do not have permission to access this resource."
}
```

**❌ Forbidden:** Manager doesn't have "Users.Delete" permission → 403 Forbidden

---

### Bước 8.5: Test Without Authentication

**Scenario:** Anonymous user calls protected endpoint

**API Call:**
```bash
curl -X GET https://localhost:7001/api/users
# No Authorization header
```

**Expected Response:**
```json
{
  "statusCode": 401,
  "message": "Unauthorized. Please authenticate."
}
```

**❌ Unauthorized:** No JWT token → 401 Unauthorized

---

## 9. Example: Protected Controller

### Bước 9.1: UsersController with Permission Protection

**File:** `src/Host/Host/Controllers/Identity/UsersController.cs` (update existing)

```csharp
using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Infrastructure.Auth.Permissions;
using ECO.WebApi.Shared.Authorization;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Identity;

/// <summary>
/// User management APIs (with permission protection)
/// </summary>
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
      _userService = userService;
    }

    /// <summary>
    /// Get list of all users
    /// Requires: Users.View permission
    /// </summary>
    [HttpGet("list")]
    [MustHavePermission(ECOAction.View, ECOFunction.User)]
    [OpenApiOperation("Get list of all users.", "")]
    public Task<List<UserDetailDto>> GetListAsync(CancellationToken cancellationToken)
    {
return _userService.GetListAsync(cancellationToken);
    }

    /// <summary>
    /// Get user details by ID
    /// Requires: Users.View permission
    /// </summary>
    [HttpGet("{id}")]
    [MustHavePermission(ECOAction.View, ECOFunction.User)]
    [OpenApiOperation("Get a user's details.", "")]
 public Task<UserDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return _userService.GetAsync(id, cancellationToken);
    }

    /// <summary>
    /// Create new user (Admin only)
    /// Requires: Users.Create permission
    /// </summary>
    [HttpPost("create")]
    [MustHavePermission(ECOAction.Create, ECOFunction.User)]
    [OpenApiOperation("Creates a new user.", "")]
 public Task<string> CreateAsync(CreateUserRequest request)
  {
        return _userService.CreateAsync(request, GetOriginFromRequest());
    }

    /// <summary>
    /// Update user profile
    /// Requires: Users.Update permission
 /// </summary>
    [HttpPut("{id}")]
    [MustHavePermission(ECOAction.Update, ECOFunction.User)]
    [OpenApiOperation("Update user profile.", "")]
    public async Task<ActionResult> UpdateAsync(string id, UpdateUserRequest request)
    {
    if (id != request.Id)
        {
            return BadRequest();
 }

        await _userService.UpdateAsync(request, id);
      return Ok();
    }

/// <summary>
    /// Delete user
    /// Requires: Users.Delete permission
    /// </summary>
    [HttpDelete("{id}")]
    [MustHavePermission(ECOAction.Delete, ECOFunction.User)]
    [OpenApiOperation("Delete a user.", "")]
public async Task<ActionResult> DeleteAsync(string id)
    {
        // Implementation (not in UserService yet)
        return NoContent();
    }

    /// <summary>
    /// Self-register (Anonymous - no permission required)
    /// </summary>
    [HttpPost("self-register")]
    [AllowAnonymous]
    [OpenApiOperation("Anonymous user creates a user.", "")]
    public Task<string> SelfRegisterAsync(CreateUserRequest request)
    {
        return _userService.CreateAsync(request, GetOriginFromRequest());
    }

  private string GetOriginFromRequest() =>
        $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}
```

**Giải thích:**

**Permission Attributes:**
- `[MustHavePermission(ECOAction.View, ECOFunction.User)]`
  - Generates policy: "Permissions.User.View"
  - Only users với "Users.View" permission can access

**AllowAnonymous:**
- `/self-register` endpoint không cần authentication
- Anyone can register

**Authorization Flow:**
```
Request → JWT Authentication → Permission Check → Controller Action
    │    │              │
    │ │                    └─ PermissionAuthorizationHandler
    │         │      checks JWT claims
    │    │
    │              └─ JwtBearerHandler validates JWT
    │
    └─ Authorization: Bearer {token}
```

---

## 10. Summary

### ✅ Đã hoàn thành trong bước này:

**Authorization Components:**
- ✅ PermissionRequirement (IAuthorizationRequirement)
- ✅ PermissionAuthorizationHandler (check permissions)
- ✅ PermissionPolicyProvider (dynamic policy creation)
- ✅ MustHavePermissionAttribute (declarative attribute)

**Permission Constants:**
- ✅ ECOAction (View, Create, Update, Delete, etc.)
- ✅ ECOFunction (User, Role, Product, etc.)
- ✅ ECOPermission (helper record)
- ✅ ECOClaims (Permission claim name)

**UserService - Permission Operations:**
- ✅ GetPermissionsAsync (query from database)
- ✅ HasPermissionAsync (check specific permission)

**TokenService - JWT Claims:**
- ✅ Add permissions to JWT claims during login
- ✅ Multiple permission claims in JWT

**Startup Configuration:**
- ✅ Register authorization services
- ✅ PermissionPolicyProvider (Singleton)
- ✅ PermissionAuthorizationHandler (Scoped)

**Testing:**
- ✅ Protected endpoints với MustHavePermission
- ✅ Success case (has permission)
- ✅ Forbidden case (no permission)
- ✅ Unauthorized case (no authentication)

### 📊 Complete Authorization Flow:

```
┌─────────────────────────────────────────────────┐
│   COMPLETE PERMISSION AUTHORIZATION FLOW        │
└─────────────────────────────────────────────────┘

1. USER REGISTRATION & ROLE ASSIGNMENT
   User creates account → Admin assigns Manager role
→ Manager role has permissions: Users.View, Users.Create

2. LOGIN & JWT GENERATION
   POST /tokens
   → TokenService.GetTokenAsync()
   → UserService.GetPermissionsAsync()
   Query: SELECT Function.Name + '.' + Action.Name
   FROM Permission P
        WHERE P.RoleId IN (user's roles)
   → Add permissions to JWT claims
   → Return JWT token

3. API CALL WITH JWT
 GET /api/users
   Authorization: Bearer {JWT}
   [MustHavePermission(ECOAction.View, ECOFunction.User)]
   → JWT middleware validates token
   → Extract claims from JWT

4. AUTHORIZATION CHECK
   → PermissionPolicyProvider.GetPolicyAsync("Permissions.User.View")
   → Create policy với PermissionRequirement
   → PermissionAuthorizationHandler.HandleRequirementAsync()
 → Check JWT claims: Has "permission" = "Permissions.User.View"?
   → UserService.HasPermissionAsync() (optional double-check)

5. RESULT
   ✅ Has Permission → 200 OK với data
   ❌ No Permission → 403 Forbidden
   ❌ No Auth → 401 Unauthorized
```

### 📌 Key Concepts:

**Permission Format:**
- **Database:** `"Users.View"` (Function.Action)
- **JWT Claims:** `"Permissions.Users.View"` (with prefix)
- **Attribute:** `[MustHavePermission(ECOAction.View, ECOFunction.User)]`
- **Policy:** `"Permissions.User.View"`

**Components Interaction:**
1. **MustHavePermissionAttribute:** Sets policy name
2. **PermissionPolicyProvider:** Creates policy with PermissionRequirement
3. **PermissionAuthorizationHandler:** Evaluates requirement against JWT claims
4. **UserService:** Queries permissions from database (for JWT generation)
5. **TokenService:** Adds permissions to JWT claims

**Benefits:**
- ✅ Dynamic authorization (no hardcoded permissions)
- ✅ Fast checks (permissions in JWT claims)
- ✅ Fine-grained access control (per-function, per-action)
- ✅ Declarative security (attributes on controllers)
- ✅ Scalable (support unlimited permissions)

**Security Considerations:**
- Permissions loaded from database on login
- Stored in JWT for fast authorization
- If permission changed, user must re-login
- Optional: Implement permission cache invalidation

### 📁 Complete File Structure:

```
src/
├── Core/
│   ├── Shared/
│   │   └── Authorization/
│   │    ├── ECOPermissions.cs (ECOAction, ECOFunction, ECOPermission)
│   │       ├── ECOClaims.cs
│   │       └── ECORoles.cs
│   ├── Domain/
│   │   └── Identity/
│   │       ├── Permission.cs (entity)
│   │       ├── Function.cs
│   │       └── Action.cs
│   └── Application/
│  └── Identity/
│           ├── Users/
│      │   └── IUserService.cs (GetPermissionsAsync, HasPermissionAsync)
│           └── Tokens/
│    └── ITokenService.cs
├── Infrastructure/
│   └── Infrastructure/
│       ├── Auth/
│   │ ├── Startup.cs (AddPermissions)
│       │   └── Permissions/
│       │   ├── PermissionRequirement.cs
│       │       ├── PermissionAuthorizationHandler.cs
│       │       ├── PermissionPolicyProvider.cs
│       │       └── MustHavePermissionAttribute.cs
│       └── Identity/
│        ├── TokenService.cs (GetClaimsAsync - add permissions)
│           └── UserService.Permission.cs (GetPermissionsAsync, HasPermissionAsync)
└── Host/
    └── Host/
        └── Controllers/
         └── Identity/
            └── UsersController.cs (with [MustHavePermission] attributes)
```

---

## 11. Next Steps

**Tiếp theo:** [BUILD_18 - OAuth2 Integration](BUILD_18_OAuth2_Integration.md)

Trong bước tiếp theo, chúng ta sẽ implement OAuth2 authentication:
1. ✅ Google OAuth2 setup
2. ✅ Facebook OAuth2 setup
3. ✅ IAuthenticationService interface
4. ✅ AuthenticationService implementation
5. ✅ OAuth2 middleware configuration
6. ✅ Social login flows

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
