# BUILD_16A: User Service

> 📘 **Mục đích:** Xây dựng User Management Service - quản lý users, roles, permissions, email confirmation, và password operations.

> [!NOTE]
> **Part of BUILD_16 Identity Services Series**
> 
> - **BUILD_16A (This file):** User Service
> - **BUILD_16B:** Role Service
> - **BUILD_16C:** Function Service

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
    - "Partial Classes (Code Organization)"
    - "DTO Pattern"
  dependencies:
    - "BUILD_01_Solution_Setup"
    - "BUILD_03_Domain_Layer"
    - "BUILD_04_Application_Layer"
    - "BUILD_07_Database_Initialization"
    - "BUILD_15_JWT_Authentication"
  ai_instructions: |
    When working with Identity Services:
    1. UserService uses partial classes for code organization
    2. All service methods are interface-based (IUserService, IRoleService, IFunctionService)
    3. Return DTOs, never return domain entities directly
    4. Use FluentValidation for all request validation
    5. UserService handles: CRUD, Roles, Permissions, Email/Phone confirmation, Password operations
    6. RoleService handles: Role CRUD + Permission assignment
    7. FunctionService handles: Function (Permission) CRUD
    8. Services are registered as Transient via marker interface ITransientService
---
```

---

## 📋 Tổng quan

BUILD_16 xây dựng **3 core Identity services:**

```
┌─────────────────────────────────────────────┐
│         Identity Services Layer              │
├─────────────────────────────────────────────┤
│                                             │
│  ┌────────────────────────────────────┐     │
│  │      User Service                  │     │
│  │  • User CRUD                       │     │
│  │  • Role Assignment                 │     │
│  │  • Permission Queries              │     │
│  │  • Email/Phone Confirmation        │     │
│  │  • Password Management             │     │
│  └────────────────────────────────────┘     │
│                                             │
│  ┌────────────────────────────────────┐     │
│  │      Role Service                  │     │
│  │  • Role CRUD                       │     │
│  │  • Permission Assignment to Role   │     │
│  └────────────────────────────────────┘     │
│                                             │
│  ┌────────────────────────────────────┐     │
│  │    Function Service                │     │
│  │  • Function (Permission) CRUD      │     │
│  └────────────────────────────────────┘     │
│                                             │
└─────────────────────────────────────────────┘
```

**Quan hệ:**
- **User** ← has many → **Roles**
- **Role** ← has many → **Functions** (Permissions)
- **Function** ← contains → **Actions** (Read, Write, Delete, etc.)

---

## 🎯 Mục tiêu BUILD_16

**Sau BUILD_16, bạn sẽ có:**

✅ **User Management Service:**
- Create/Update/Search users
- Activate/Deactivate users
- Assign roles to users
- Query user permissions
- Email/Phone confirmation
- Password operations (forgot, reset, change)

✅ **Role Management Service:**
- Create/Update/Delete roles
- Assign permissions to roles
- Query role with permissions

✅ **Function Management Service:**
- Create/Update/Delete functions (permissions)
- Query all available functions

---

## 📂 Cấu trúc File

```
src/
├── Core/
│   └── Application/
│       └── Identity/
│           ├── Users/
│           │   ├── IUserService.cs                    # Interface
│           │   ├── UserDetailDto.cs                   # DTO
│           │   ├── CreateUserRequest.cs              # Request + Validator
│           │   ├── UpdateUserRequest.cs              # Request + Validator
│           │   ├── ToggleUserStatusRequest.cs       # Request
│           │   ├── UserRolesRequest.cs               # Request
│           │   ├── UserRoleDto.cs                    # DTO
│           │   ├── UserParameterFilter.cs            # Search filter
│           │   └── Password/
│           │       ├── ChangePasswordRequest.cs      # Request + Validator
│           │       ├── ForgotPasswordRequest.cs      # Request + Validator
│           │       └── ResetPasswordRequest.cs       # Request + Validator
│           │
│           └── Roles/
│               ├── IRoleService.cs                    # Interface
│               ├── IFunctionService.cs                # Interface
│               ├── RoleDto.cs                        # DTO
│               ├── FunctionDto.cs                    # DTO
│               ├── ActionDto.cs                      # DTO
│               ├── CreateOrUpdateRoleRequest.cs      # Request + Validator
│               ├── UpdateRolePermissionsRequest.cs   # Request + Validator
│               └── CreateOrUpdateFunctionRequest.cs  # Request + Validator
│
└── Infrastructure/
    └── Infrastructure/
        └── Identity/
            ├── UserService.cs                        # Main implementation
            ├── UserService.CreateUpdate.cs           # Partial: CRUD operations
            ├── UserService.Role.cs                   # Partial: Role management
            ├── UserService.Permission.cs             # Partial: Permission queries
            ├── UserService.Confirm.cs                # Partial: Confirmations
            ├── UserService.Password.cs               # Partial: Password operations
            ├── RoleService.cs                        # Full implementation
            └── FunctionService.cs                    # Full implementation
```

---

## 1. User Service - DTOs

### Bước 1.1: UserDetailDto

**Làm gì:** DTO để return user information.

**File:** `src/Core/Application/Identity/Users/UserDetailDto.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

public class UserDetailDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
}
```

**Giải thích:**
- DTO trả về thông tin user (KHÔNG return entity trực tiếp)
- `IsActive` - user có bị deactivate không
- `EmailConfirmed` - email đã xác nhận chưa
- `Guid Id` - user ID (ApplicationUser.Id is string, but exposed as Guid in DTO)

---

### Bước 1.2: UserRoleDto

**Làm gì:** DTO để return user roles.

**File:** `src/Core/Application/Identity/Users/UserRoleDto.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

public class UserRoleDto
{
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    public string? Description { get; set; }
    public bool Enabled { get; set; }
}
```

**Giải thích:**
- Return danh sách roles của user
- `Enabled` - role có được assign cho user này không

---

### Bước 1.3: CreateUserRequest

**Làm gì:** Request để tạo user mới.

**File:** `src/Core/Application/Identity/Users/CreateUserRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Users;

public class CreateUserRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public string? PhoneNumber { get; set; }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IUserService userService)
    {
        RuleFor(u => u.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage("Invalid Email Address.")
            .MustAsync(async (email, _) => !await userService.ExistsWithEmailAsync(email))
                .WithMessage((_, email) => $"Email {email} is already registered.");

        RuleFor(u => u.UserName).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(6)
            .MustAsync(async (name, _) => !await userService.ExistsWithNameAsync(name))
                .WithMessage((_, name) => $"Username {name} is already taken.");

        RuleFor(u => u.PhoneNumber).Cascade(CascadeMode.Stop)
            .MustAsync(async (phone, _) => !await userService.ExistsWithPhoneNumberAsync(phone!))
                .WithMessage((_, phone) => $"Phone number {phone} is already registered.")
                .Unless(u => string.IsNullOrWhiteSpace(u.PhoneNumber));

        RuleFor(p => p.FirstName).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(p => p.LastName).Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(p => p.Password).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(p => p.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Equal(p => p.Password);
    }
}
```

**Giải thích:**

**Validation Rules:**
1. **Email:** Valid format + must be unique (call `ExistsWithEmailAsync`)
2. **UserName:** Min 6 chars + must be unique (call `ExistsWithNameAsync`)
3. **PhoneNumber:** Must be unique IF provided (optional field)
4. **Password:** Min 6 chars
5. **ConfirmPassword:** Must equal Password

**Async Validation:**
- `MustAsync` - async validation rule
- Call `IUserService` methods to check uniqueness
- Best practice: Inject service into validator constructor

---

### Bước 1.4: UpdateUserRequest

**Làm gì:** Request để update thông tin user.

**File:** `src/Core/Application/Identity/Users/UpdateUserRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Users;

public class UpdateUserRequest
{
    public string Id { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public FileUploadRequest? Image { get; set; }
    public bool DeleteCurrentImage { get; set; } = false;
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(IUserService userService)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage("Invalid Email Address.")
            .MustAsync(async (user, email, _) => !await userService.ExistsWithEmailAsync(email, user.Id))
                .WithMessage((_, email) => $"Email {email} is already registered.");

        RuleFor(u => u.PhoneNumber).Cascade(CascadeMode.Stop)
            .MustAsync(async (user, phone, _) => !await userService.ExistsWithPhoneNumberAsync(phone!, user.Id))
                .WithMessage((_, phone) => $"Phone number {phone} is already registered.")
                .Unless(u => string.IsNullOrWhiteSpace(u.PhoneNumber));
    }
}
```

**Giải thích:**

**Khác CreateUserRequest:**
- Có `Id` - identify user cần update
- KHÔNG có Password/ConfirmPassword  
- Có `Image` upload
- Có `DeleteCurrentImage` flag

**Validation:**
- Email/PhoneNumber uniqueness check **except current user** → `ExistsWithEmailAsync(email, user.Id)`
- MaximumLength validation for names

---

### Bước 1.5: ToggleUserStatusRequest

**Làm gì:** Request để activate/deactivate user.

**File:** `src/Core/Application/Identity/Users/ToggleUserStatusRequest.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

public class ToggleUserStatusRequest
{
    public bool ActivateUser { get; set; }
    public string? UserId { get; set; }
}
```

**Giải thích:**
- Simple request - không cần validator
- `ActivateUser = true` → activate user
- `ActivateUser = false` → deactivate user

---

### Bước 1.6: UserRolesRequest

**Làm gì:** Request để assign roles cho user.

**File:** `src/Core/Application/Identity/Users/UserRolesRequest.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

public class UserRolesRequest
{
    public List<UserRoleDto> UserRoles { get; set; } = new();
}
```

**Giải thích:**
- List of UserRoleDto với `Enabled` flag
- `Enabled = true` → assign role
- `Enabled = false` → remove role

---

### Bước 1.7: UserParameterFilter

**Làm gì:** Filter cho search/list users (pagination).

**File:** `src/Core/Application/Identity/Users/UserParameterFilter.cs`

```csharp
using ECO.WebApi.Application.Common.Models;

namespace ECO.WebApi.Application.Identity.Users;

public class UserParameterFilter : PaginationFilter
{
    public bool? IsActive { get; set; }
}
```

**Giải thích:**
- Extends `PaginationFilter` (PageNumber, PageSize, SearchString)
- Add `IsActive` filter - nullable (null = all, true = active only, false = inactive only)

---

## 2. Password Management DTOs

### Bước 2.1: ChangePasswordRequest

**Làm gì:** Request để user đổi password (khi đã login).

**File:** `src/Core/Application/Identity/Users/Password/ChangePasswordRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Users.Password;

public class ChangePasswordRequest
{
    public string Password { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmNewPassword { get; set; } = default!;
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(p => p.Password)
            .NotEmpty();

        RuleFor(p => p.NewPassword)
            .NotEmpty();

        RuleFor(p => p.ConfirmNewPassword)
            .Equal(p => p.NewPassword)
                .WithMessage("Passwords do not match.");
    }
}
```

**Giải thích:**
- User phải biết password cũ (`Password`)
- New password must match confirmation
- Dùng khi user muốn đổi password (trong profile settings)

---

### Bước 2.2: ForgotPasswordRequest

**Làm gì:** Request để gửi email reset password.

**File:** `src/Core/Application/Identity/Users/Password/ForgotPasswordRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Users.Password;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator() =>
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage("Invalid Email Address.");
}
```

**Giải thích:**
- User quên password → nhập email
- System gửi email với reset token
- Expression-bodied constructor for simple validator

---

### Bước 2.3: ResetPasswordRequest

**Làm gì:** Request để reset password với token.

**File:** `src/Core/Application/Identity/Users/Password/ResetPasswordRequest.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users.Password;

public class ResetPasswordRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
}
```

**Giải thích:**
- User click link trong email → nhập password mới
- Token verify request hợp lệ
- Không cần validator (validation trong service)

---

## 3. IUserService Interface

### Bước 3.1: Interface Definition

**Làm gì:** Define contract cho User Service.

**File:** `src/Core/Application/Identity/Users/IUserService.cs`

```csharp
using ECO.WebApi.Application.Identity.Users.Password;

namespace ECO.WebApi.Application.Identity.Users;

public interface IUserService : ITransientService
{   
    // User CRUD & Queries
    Task<PaginationResponse<UserDetailDto>> SearchAsync(UserParameterFilter filter, CancellationToken cancellationToken);
    Task<List<UserDetailDto>> GetListAsync(CancellationToken cancellationToken);
    Task<int> GetCountAsync(CancellationToken cancellationToken);
    Task<UserDetailDto> GetAsync(string userId, CancellationToken cancellationToken);
    Task<string> GetFullName(Guid userId);
    
    // Existence Checks
    Task<bool> ExistsWithNameAsync(string name);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);
    
    // Create & Update
    Task<string> CreateAsync(CreateUserRequest request, string origin);
    Task UpdateAsync(UpdateUserRequest request, string userId);
    Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken);
    
    // Role Management
    Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken);
    Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken);
    
    // Permission Queries
    Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
    
    // Email/Phone Confirmation
    Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken);
    Task<string> ConfirmPhoneNumberAsync(string userId, string code);
    
    // Password Operations
    Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
    Task<string> ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request, string userId);
}
```

**Giải thích:**

**Method Groups:**

**1. User CRUD & Queries (5 methods):**
- `SearchAsync` - Paginated search with filter
- `GetListAsync` - Get all users
- `GetCountAsync` - Total user count
- `GetAsync` - Get single user by ID
- `GetFullName` - Helper to get user's full name

**2. Existence Checks (3 methods):**
- Used by validators for uniqueness checks
- `exceptId` parameter - check uniqueness except for specific user (for Update)

**3. Create & Update (3 methods):**
- `CreateAsync` - returns user ID (string)
- `origin` parameter - for email link generation
- `UpdateAsync` - void return
- `ToggleStatusAsync` - activate/deactivate

**4. Role Management (2 methods):**
- `GetRolesAsync` - user's current roles
- `AssignRolesAsync` - bulk assign/remove roles

**5. Permission Queries (2 methods):**
- `GetPermissionsAsync` - all permissions for user (aggregated from roles)
- `HasPermissionAsync` - check single permission (for authorization)

**6. Confirmations (2 methods):**
- Email and phone number confirmation
- Return success message

**7. Password Operations (3 methods):**
- Complete password flow: forgot → reset, or change (when logged in)

---

## 4. UserService Implementation

### Bước 4.1: Partial Class Structure

**Tại sao Partial Classes?**

UserService có 20+ methods → chia ra partial classes theo chức năng cho dễ maintain.

**Structure:**

```
UserService.cs                  # Main class, constructor, shared methods
UserService.CreateUpdate.cs     # CRUD operations
UserService.Role.cs             # Role management
UserService.Permission.cs       # Permission queries
UserService.Confirm.cs          # Email/Phone confirmation
UserService.Password.cs         # Password operations
```

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.cs` (Main)

```csharp
using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ECO.WebApi.Infrastructure.Identity;

/// <summary>
/// User management service
/// Organized using partial classes for maintainability
/// </summary>
internal partial class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStringLocalizer _localizer;
    private readonly IJobService _jobService;
    private readonly IMailService _mailService;
    private readonly IFileStorageService _fileStorage;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IStringLocalizer<UserService> localizer,
        IJobService jobService,
        IMailService mailService,
        IFileStorageService fileStorage)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _localizer = localizer;
        _jobService = jobService;
        _mailService = mailService;
        _fileStorage = fileStorage;
    }

    // Shared helper methods
    private async Task<ApplicationUser> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user ?? throw new NotFoundException("User Not Found.");
    }
}
```

**Dependencies:**
- `UserManager` - ASP.NET Core Identity user management
- `RoleManager` - ASP.NET Core Identity role management
- `SignInManager` - Sign-in operations (password verification)
- `IStringLocalizer` - Localized messages
- `IJobService` - Background jobs (email sending)
- `IMailService` - Email service
- `IFileStorageService` - Upload/manage user avatars

---

### Bước 4.2: Key Implementation Highlights

> [!NOTE]
> **Clean Architecture Approach**
> 
> Implementation details ở BUILD_16A này là **simplified highlights** chứ không phải full actual code.
> Mục đích: Hiểu flow và patterns, không phải copy-paste toàn bộ implementation.

**UserService.CreateUpdate.cs** (CRUD Operations)

```csharp
namespace ECO.WebApi.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<string> CreateAsync(CreateUserRequest request, string origin)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InternalServerException("Validation Errors Occurred.", result.GetErrors(_localizer));
        }

        // Send email confirmation (background job)
        await SendVerificationEmailAsync(user, origin);

        return user.Id;
    }

    public async Task UpdateAsync(UpdateUserRequest request, string userId)
    {
        var user = await GetUserAsync(userId);

        // Update properties
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        // Handle image upload
        if (request.Image != null)
        {
            user.ImageUrl = await _fileStorage.UploadAsync<ApplicationUser>(request.Image, FileType.Image);
        }

        if (request.DeleteCurrentImage)
        {
            await _fileStorage.RemoveAsync(user.ImageUrl);
            user.ImageUrl = null;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InternalServerException("Update failed.", result.GetErrors(_localizer));
        }
    }

    public async Task<PaginationResponse<UserDetailDto>> SearchAsync(
        UserParameterFilter filter, 
        CancellationToken cancellationToken)
    {
        var users = _userManager.Users;

        // Apply filters
        if (filter.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == filter.IsActive);
        }

        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            users = users.Where(u =>
                u.FirstName.Contains(filter.SearchString) ||
                u.LastName.Contains(filter.SearchString) ||
                u.Email.Contains(filter.SearchString));
        }

        // Map to DTO
        return await users
            .Select(u => new UserDetailDto
            {
                Id = Guid.Parse(u.Id),
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                PhoneNumber = u.PhoneNumber,
                ImageUrl = u.ImageUrl
            })
            .ToPaginatedListAsync(filter.PageNumber, filter.PageSize, cancellationToken);
    }
}
```

**UserService.Role.cs** (Role Management)

```csharp
namespace ECO.WebApi.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(userId);
        var userRoleNames = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.ToListAsync(cancellationToken);

        return allRoles.Select(role => new UserRoleDto
        {
            RoleId = role.Id,
            RoleName = role.Name!,
            Description = role.Description,
            Enabled = userRoleNames.Contains(role.Name!)
        }).ToList();
    }

    public async Task<string> AssignRolesAsync(
        string userId, 
        UserRolesRequest request, 
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(userId);
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove all current roles
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Add selected roles
        var selectedRoles = request.UserRoles
            .Where(x => x.Enabled)
            .Select(x => x.RoleName)
            .ToList();

        await _userManager.AddToRolesAsync(user, selectedRoles);

        return "User Roles Updated Successfully.";
    }
}
```

**UserService.Permission.cs** (Permission Queries)

```csharp
namespace ECO.WebApi.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(userId);
        var userRoles = await _userManager.GetRolesAsync(user);

        var permissions = new List<string>();

        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                permissions.AddRange(
                    roleClaims
                        .Where(c => c.Type == ECOClaims.Permission)
                        .Select(c => c.Value));
            }
        }

        return permissions.Distinct().ToList();
    }

    public async Task<bool> HasPermissionAsync(
        string userId, 
        string permission, 
        CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permission);
    }
}
```

**UserService.Password.cs** (Password Operations)

```csharp
namespace ECO.WebApi.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.EmailConfirmed)
        {
            // Don't reveal user existence - security
            throw new InternalServerException("An Error has occurred!");
        }

        // Generate reset token
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = $"{origin}/account/reset-password?email={user.Email}&code={Uri.EscapeDataString(code)}";

        // Send email (background job)
        var mailRequest = new MailRequest
        {
            To = user.Email,
            Subject = "Reset Password",
            Body = $"Please reset your password by clicking: {resetUrl}"
        };
        _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));

        return "Password Reset Email sent successfully.";
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            throw new InternalServerException("An Error has occurred!");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token!, request.Password!);
        if (!result.Succeeded)
        {
            throw new InternalServerException("An Error has occurred!");
        }

        return "Password Reset Successful.";
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, string userId)
    {
        var user = await GetUserAsync(userId);

        var result = await _userManager.ChangePasswordAsync(
            user, 
            request.Password, 
            request.NewPassword);

        if (!result.Succeeded)
        {
            throw new InternalServerException("Change Password Failed.", result.GetErrors(_localizer));
        }
    }
}
```

---

## 5. Key Patterns & Design Decisions

### Pattern 1: Partial Classes for Code Organization

**Why:**
- 20+ methods → hard to navigate single large file
- Group related functionality
- Easier code review

**Structure:**
```
UserService.cs          ← Constructor, dependencies, shared helpers
  ├─ CreateUpdate.cs    ← CRUD
  ├─ Role.cs            ← Role management
  ├─ Permission.cs      ← Permissions
  ├─ Confirm.cs         ← Email/Phone
  └─ Password.cs        ← Password ops
```

---

### Pattern 2: Service Layer (Not CQRS)

**Why not Handlers?**
- Identity operations are infrastructure concerns
- ASP.NET Core Identity UserManager/RoleManager are already service-like
- Wrapping in CQRS commands adds unnecessary complexity

**When to use CQRS:**
- Domain logic operations ✅
- Business workflows ✅
- Complex queries ✅

**When Service Layer is OK:**
- Infrastructure wrappers (Identity, Email, File Storage)
- Simple CRUD on framework entities
- Technical operations (password reset, email confirmation)

---

### Pattern 3: Return DTOs, Not Entities

```csharp
// ✅ GOOD
public async Task<UserDetailDto> GetAsync(string userId, ...)
{
    var user = await GetUserAsync(userId);
    return new UserDetailDto { ... }; // Map to DTO
}

// ❌ BAD
public async Task<ApplicationUser> GetAsync(string userId, ...)
{
    return await GetUserAsync(userId); // Return entity!
}
```

**Why DTOs:**
- Control what data is exposed
- Decouple API from domain
- Easier to version API

---

### Pattern 4: Background Jobs for Email

```csharp
_jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));
```

**Why:**
- Don't make user wait for email sending
- Improve response time
- Better UX

---

### Pattern 5: Generic Error Messages (Security)

```csharp
// Don't reveal if email exists
if (user == null || !user.EmailConfirmed)
{
    throw new InternalServerException("An Error has occurred!");
}
```

**Why:**
- Prevent user enumeration attacks
- Don't leak system information
- Security best practice

---

## 6. Tổng kết BUILD_16A

**Đã xây dựng:**

✅ **DTOs:**
- UserDetailDto, UserRoleDto
- CreateUserRequest, UpdateUserRequest
- ToggleUserStatusRequest, UserRolesRequest
- ChangePasswordRequest, ForgotPasswordRequest, ResetPasswordRequest
- UserParameterFilter

✅ **Validators:**
- FluentValidation for all requests
- Async validation for uniqueness checks
- Inject IUserService into validators

✅ **IUserService Interface:**
- 20 methods grouped by functionality
- Clear contracts

✅ **UserService Implementation:**
- Partial classes for organization
- Clean separation of concerns
- Background jobs for emails
- Security best practices

**Next Steps:**

➡️ **BUILD_16B:** Role Service (simpler - 7 methods)  
➡️ **BUILD_16C:** Function Service (simplest - 4 methods)

