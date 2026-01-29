# User Management Service - User CRUD Operations

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** Bước 15 (JWT Authentication) đã hoàn thành

Tài liệu này hướng dẫn xây dựng User Management Service - Quản lý người dùng với đầy đủ CRUD operations.

---

## 1. Overview

**Làm gì:** Xây dựng User Management Service để quản lý người dùng (Create, Read, Update, Toggle Status, Email Confirmation).

**Tại sao cần:**
- **User Management:** CRUD operations cho user accounts
- **Self-Registration:** Cho phép users tự đăng ký tài khoản
- **Email Confirmation:** Xác thực email trước khi active account
- **Profile Management:** Users có thể update thông tin cá nhân
- **Image Upload:** Upload và quản lý avatar
- **Status Management:** Admin có thể active/deactive users

**Trong bước này chúng ta sẽ:**
- ✅ Tạo IUserService interface (partial methods)
- ✅ Tạo User DTOs (UserDetailDto, CreateUserRequest, UpdateUserRequest)
- ✅ Implement UserService với các operations:
  - Search users với pagination
  - Get user details
  - Create user (admin) & Self-register (anonymous)
  - Update user profile với image upload
  - Toggle user status (active/inactive)
  - Email confirmation
- ✅ Tạo UserController với RESTful endpoints
- ✅ FluentValidation cho tất cả requests
- ✅ Email templates cho registration confirmation

**Real-world example:**
```csharp
// Admin creates user
var createRequest = new CreateUserRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    UserName = "johndoe",
    Password = "SecurePass123!",
    ConfirmPassword = "SecurePass123!",
    PhoneNumber = "+84987654321"
};

var message = await _userService.CreateAsync(createRequest, origin);
// → User registered. Email confirmation sent to john@example.com

// User confirms email
await _userService.ConfirmEmailAsync(userId, code, cancellationToken);
// → Email confirmed successfully!

// User updates profile
var updateRequest = new UpdateUserRequest
{
    Id = userId,
    FirstName = "John",
    LastName = "Smith",
    PhoneNumber = "+84987654322",
    Image = new FileUploadRequest { ... }
};

await _userService.UpdateAsync(updateRequest, userId);
// → Profile updated with new avatar

// Admin toggles user status
await _userService.ToggleStatusAsync(new ToggleUserStatusRequest 
{ 
    UserId = userId, 
    ActivateUser = false 
}, cancellationToken);
// → User deactivated
```

---

## 2. User DTOs

### Bước 2.1: UserDetailDto

**Làm gì:** DTO để hiển thị thông tin user.

**Tại sao:** Không expose toàn bộ ApplicationUser entity, chỉ trả về fields cần thiết.

**File:** `src/Core/Application/Identity/Users/UserDetailDto.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// User detail DTO (dùng cho responses)
/// </summary>
public class UserDetailDto
{
    /// <summary>
    /// User ID (Guid)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username (unique)
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// First name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name
    /// </summary>
    public string? LastName { get; set; }

 /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Is user active (can login)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Has email been confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Phone number
/// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Avatar image URL
    /// </summary>
    public string? ImageUrl { get; set; }
}
```

**Giải thích:**
- **Id:** User ID dạng Guid
- **IsActive:** Admin có thể deactivate users (ngăn login)
- **EmailConfirmed:** Track email confirmation status
- **ImageUrl:** Relative path to avatar image

**Tại sao không expose Password:**
- Security: Never return password hashes
- Separation of concerns: DTOs chỉ chứa display data

---

### Bước 2.2: CreateUserRequest

**Làm gì:** Request DTO để tạo user mới.

**Tại sao:** Type-safe request với validation rules.

**File:** `src/Core/Application/Identity/Users/CreateUserRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// Request để tạo user mới
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// First name (required)
  /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Last name (required)
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Email address (required, unique)
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Username (required, unique, min 6 chars)
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// Password (required, min 6 chars)
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// Confirm password (must match Password)
    /// </summary>
    public string ConfirmPassword { get; set; } = default!;

    /// <summary>
    /// Phone number (optional, unique if provided)
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Validator cho CreateUserRequest
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IUserService userService)
    {
        RuleFor(u => u.Email)
    .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required.")
   .EmailAddress()
            .WithMessage("Invalid Email Address.")
   .MustAsync(async (email, _) => !await userService.ExistsWithEmailAsync(email))
            .WithMessage((_, email) => $"Email {email} is already registered.");

RuleFor(u => u.UserName)
         .Cascade(CascadeMode.Stop)
            .NotEmpty()
     .WithMessage("Username is required.")
  .MinimumLength(6)
    .WithMessage("Username must be at least 6 characters.")
.MustAsync(async (name, _) => !await userService.ExistsWithNameAsync(name))
            .WithMessage((_, name) => $"Username {name} is already taken.");

        RuleFor(u => u.PhoneNumber)
  .Cascade(CascadeMode.Stop)
    .MustAsync(async (phone, _) => !await userService.ExistsWithPhoneNumberAsync(phone!))
 .WithMessage((_, phone) => $"Phone number {phone} is already registered.")
  .Unless(u => string.IsNullOrWhiteSpace(u.PhoneNumber));

      RuleFor(p => p.FirstName)
    .Cascade(CascadeMode.Stop)
            .NotEmpty()
         .WithMessage("First name is required.");

 RuleFor(p => p.LastName)
  .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Last name is required.");

RuleFor(p => p.Password)
            .Cascade(CascadeMode.Stop)
     .NotEmpty()
     .WithMessage("Password is required.")
            .MinimumLength(6)
      .WithMessage("Password must be at least 6 characters.");

        RuleFor(p => p.ConfirmPassword)
   .Cascade(CascadeMode.Stop)
      .NotEmpty()
            .WithMessage("Confirm password is required.")
    .Equal(p => p.Password)
            .WithMessage("Password and Confirm Password must match.");
    }
}
```

**Giải thích:**

**Validation Rules:**
- **Email:** Required, valid email format, unique trong database
- **UserName:** Required, min 6 chars, unique
- **PhoneNumber:** Optional, nhưng nếu có thì phải unique
- **Password:** Required, min 6 chars
- **ConfirmPassword:** Must match Password

**Async Validation:**
- `ExistsWithEmailAsync()`: Check email đã tồn tại chưa
- `ExistsWithNameAsync()`: Check username đã tồn tại chưa
- `ExistsWithPhoneNumberAsync()`: Check phone number đã tồn tại chưa

**Tại sao Cascade(CascadeMode.Stop):**
- Stop validation chain nếu rule đầu tiên fail
- Ví dụ: Nếu Email empty, không cần check uniqueness nữa

---

### Bước 2.3: UpdateUserRequest

**Làm gì:** Request DTO để update user profile.

**Tại sao:** Cho phép users update thông tin cá nhân và avatar.

**File:** `src/Core/Application/Identity/Users/UpdateUserRequest.cs`

```csharp
using FluentValidation;

namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// Request để update user profile
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// User ID (required)
    /// </summary>
  public string Id { get; set; } = default!;

    /// <summary>
    /// First name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
  /// Email address (unique)
  /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Avatar image upload
    /// </summary>
 public FileUploadRequest? Image { get; set; }

  /// <summary>
    /// Delete current avatar image
    /// </summary>
    public bool DeleteCurrentImage { get; set; } = false;
}

/// <summary>
/// Validator cho UpdateUserRequest
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(IUserService userService)
    {
   RuleFor(p => p.Id)
.NotEmpty()
  .WithMessage("User ID is required.");

RuleFor(p => p.FirstName)
            .NotEmpty()
     .WithMessage("First name is required.")
            .MaximumLength(75)
   .WithMessage("First name must not exceed 75 characters.");

   RuleFor(p => p.LastName)
     .NotEmpty()
            .WithMessage("Last name is required.")
     .MaximumLength(75)
       .WithMessage("Last name must not exceed 75 characters.");

        RuleFor(p => p.Email)
      .NotEmpty()
            .WithMessage("Email is required.")
    .EmailAddress()
    .WithMessage("Invalid Email Address.")
.MustAsync(async (user, email, _) => !await userService.ExistsWithEmailAsync(email, user.Id))
.WithMessage((_, email) => $"Email {email} is already registered.");

        RuleFor(p => p.Image);

        RuleFor(u => u.PhoneNumber)
      .Cascade(CascadeMode.Stop)
      .MustAsync(async (user, phone, _) => !await userService.ExistsWithPhoneNumberAsync(phone!, user.Id))
            .WithMessage((_, phone) => $"Phone number {phone} is already registered.")
   .Unless(u => string.IsNullOrWhiteSpace(u.PhoneNumber));
    }
}
```

**Giải thích:**

**Validation với exceptId:**
- `ExistsWithEmailAsync(email, user.Id)`: Check email unique nhưng EXCLUDE current user
- `ExistsWithPhoneNumberAsync(phone, user.Id)`: Tương tự cho phone

**Image Upload:**
- **Image:** FileUploadRequest (from BUILD_20 File Storage)
- **DeleteCurrentImage:** Flag để xóa avatar hiện tại

**Tại sao cần exceptId:**
- User update profile nhưng giữ nguyên email/phone
- Không bị lỗi "email already registered" khi email là của chính họ

---

### Bước 2.4: ToggleUserStatusRequest

**Làm gì:** Request để toggle user active status.

**Tại sao:** Admin có thể activate/deactivate users.

**File:** `src/Core/Application/Identity/Users/ToggleUserStatusRequest.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// Request để toggle user active status
/// </summary>
public class ToggleUserStatusRequest
{
    /// <summary>
 /// User ID to toggle
    /// </summary>
  public string UserId { get; set; } = default!;

    /// <summary>
    /// True = activate, False = deactivate
    /// </summary>
    public bool ActivateUser { get; set; }
}
```

**Giải thích:**
- **ActivateUser:** `true` = activate, `false` = deactivate
- Admin không thể deactivate chính mình

---

### Bước 2.5: UserParameterFilter

**Làm gì:** Filter cho user search với pagination.

**Tại sao:** Support search và pagination trong user list.

**File:** `src/Core/Application/Identity/Users/UserParameterFilter.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// Filter cho user search với pagination
/// </summary>
public class UserParameterFilter : PaginationFilter
{
    /// <summary>
    /// Search keyword (search in name, email, username)
    /// </summary>
    public string? Keyword { get; set; }

  /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by email confirmed status
    /// </summary>
    public bool? EmailConfirmed { get; set; }
}
```

**Giải thích:**
- Kế thừa `PaginationFilter` (PageNumber, PageSize từ BUILD_11)
- **Keyword:** Search trong name, email, username
- **IsActive:** Filter by active status
- **EmailConfirmed:** Filter by email confirmation status

---

## 3. User Service Interface

### Bước 3.1: IUserService Interface

**Làm gì:** Define contract cho user operations.

**Tại sao:** Abstraction, dễ test, dễ swap implementations.

**File:** `src/Core/Application/Identity/Users/IUserService.cs`

```csharp
using ECO.WebApi.Application.Identity.Users.Password;

namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// Service xử lý user management operations
/// </summary>
public interface IUserService : ITransientService
{   
    #region Default Operations
    
    /// <summary>
    /// Search users với pagination và filters
    /// </summary>
    Task<PaginationResponse<UserDetailDto>> SearchAsync(
     UserParameterFilter filter, 
    CancellationToken cancellationToken);

    /// <summary>
    /// Check username đã tồn tại chưa
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name);

    /// <summary>
/// Check email đã tồn tại chưa (exclude exceptId nếu có)
    /// </summary>
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);

    /// <summary>
    /// Check phone number đã tồn tại chưa (exclude exceptId nếu có)
    /// </summary>
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);

    /// <summary>
    /// Get full name của user
    /// </summary>
    Task<string> GetFullName(Guid userId);

    /// <summary>
    /// Get list tất cả users (không pagination)
    /// </summary>
    Task<List<UserDetailDto>> GetListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get total user count
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get user details by ID
    /// </summary>
    Task<UserDetailDto> GetAsync(string userId, CancellationToken cancellationToken);

    #endregion

    #region Role Operations (sẽ implement trong BUILD_16B)
    
    /// <summary>
    /// Get user's assigned roles
    /// </summary>
 Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Assign roles to user
    /// </summary>
    Task<string> AssignRolesAsync(
        string userId, 
        UserRolesRequest request, 
        CancellationToken cancellationToken);

    #endregion

    #region Permission Operations (sẽ implement trong BUILD_16C)
    
    /// <summary>
    /// Get user's permissions
    /// </summary>
    Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Check if user has specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(
        string userId, 
        string permission, 
   CancellationToken cancellationToken = default);

    #endregion

    #region Create & Update Operations
    
    /// <summary>
    /// Toggle user active status (admin only)
    /// </summary>
    Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken);

 /// <summary>
    /// Create new user (admin hoặc self-register)
    /// </summary>
    Task<string> CreateAsync(CreateUserRequest request, string origin);

    /// <summary>
    /// Update user profile
  /// </summary>
    Task UpdateAsync(UpdateUserRequest request, string userId);

    #endregion

  #region Email Confirmation (sẽ implement chi tiết)
    
 /// <summary>
    /// Confirm email với verification code
    /// </summary>
    Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken);

    /// <summary>
    /// Confirm phone number với verification code
    /// </summary>
    Task<string> ConfirmPhoneNumberAsync(string userId, string code);

    #endregion

    #region Password Operations (sẽ implement chi tiết)
    
    /// <summary>
    /// Send forgot password email
    /// </summary>
    Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);

    /// <summary>
    /// Reset password với reset token
    /// </summary>
    Task<string> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Change password (user đã login)
  /// </summary>
    Task ChangePasswordAsync(ChangePasswordRequest request, string userId);

    #endregion
}
```

**Giải thích:**

**Default Operations:**
- Search, Get, Count, Exists checks
- Core CRUD operations

**Role Operations:**
- GetRolesAsync, AssignRolesAsync
- Sẽ implement chi tiết trong BUILD_16B

**Permission Operations:**
- GetPermissionsAsync, HasPermissionAsync
- Sẽ implement chi tiết trong BUILD_16C

**Create & Update:**
- CreateAsync, UpdateAsync, ToggleStatusAsync
- Implement trong bước này

**Email Confirmation:**
- ConfirmEmailAsync, ConfirmPhoneNumberAsync
- Implement trong bước này

**Password Operations:**
- ForgotPasswordAsync, ResetPasswordAsync, ChangePasswordAsync
- Sẽ implement trong phần riêng

**Tại sao partial interface:**
- Interface có nhiều methods (20+ methods)
- Chia nhỏ implementations thành nhiều partial classes
- Dễ maintain và navigate code

---

## 4. User Service Implementation

### Bước 4.1: UserService - Main Class

**Làm gì:** Implement core user operations.

**Tại sao:** Business logic cho user management.

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.cs`

```csharp
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using ECO.WebApi.Application.Common.Caching;
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.FileStorage;
using ECO.WebApi.Application.Common.Mailing;
using ECO.WebApi.Application.Common.Models;
using ECO.WebApi.Application.Common.Specification;
using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Auth;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Identity;

/// <summary>
/// Service xử lý user management operations
/// (Partial class - implementation chia thành nhiều files)
/// </summary>
internal partial class UserService : IUserService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
  private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly IJobService _jobService;
    private readonly IMailService _mailService;
    private readonly SecuritySettings _securitySettings;
    private readonly IEmailTemplateService _templateService;
    private readonly IFileStorageService _fileStorage;
    private readonly IEventPublisher _events;
    private readonly ICacheService _cache;

    public UserService(
   SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
     ApplicationDbContext db,
        IJobService jobService,
        IMailService mailService,
        IOptions<SecuritySettings> securitySettings,
        IEmailTemplateService templateService,
        IFileStorageService fileStorage,
        IEventPublisher events,
        ICacheService cache)
    {
    _signInManager = signInManager;
 _userManager = userManager;
        _roleManager = roleManager;
     _db = db;
        _jobService = jobService;
        _mailService = mailService;
        _securitySettings = securitySettings.Value;
        _templateService = templateService;
        _fileStorage = fileStorage;
        _events = events;
        _cache = cache;
    }

    #region Default Operations

    /// <summary>
/// Search users với pagination và filters
    /// </summary>
    public async Task<PaginationResponse<UserDetailDto>> SearchAsync(
  UserParameterFilter filter, 
        CancellationToken cancellationToken)
    {
        // Build specification từ filter
var spec = new EntitiesByPaginationFilterSpec<ApplicationUser>(filter);

        // Query với specification và project to DTO (efficient query)
        var users = await _userManager.Users
            .WithSpecification(spec)
        .ProjectToType<UserDetailDto>() // Mapster projection (chỉ select cần thiết)
       .ToListAsync(cancellationToken);

        // Get total count
        int count = await _userManager.Users.CountAsync(cancellationToken);

        return new PaginationResponse<UserDetailDto>(
         users, 
  count, 
    filter.PageNumber, 
 filter.PageSize);
    }

/// <summary>
    /// Check username đã tồn tại chưa
    /// </summary>
    public async Task<bool> ExistsWithNameAsync(string name)
  {
        return await _userManager.FindByNameAsync(name) is not null;
    }

    /// <summary>
    /// Check email đã tồn tại chưa (exclude exceptId nếu có)
    /// </summary>
    public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
    {
        return await _userManager.FindByEmailAsync(email.Normalize()) is ApplicationUser user 
&& user.Id != exceptId;
    }

    /// <summary>
    /// Check phone number đã tồn tại chưa (exclude exceptId nếu có)
    /// </summary>
    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
    {
  return await _userManager.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is ApplicationUser user 
            && user.Id != exceptId;
    }

    /// <summary>
    /// Get full name của user
    /// </summary>
    public async Task<string> GetFullName(Guid userId)
    {
        var user = await GetAsync(userId.ToString(), CancellationToken.None);
        return string.Join(" ", user.FirstName, user.LastName);
    }

    /// <summary>
    /// Get list tất cả users (không pagination)
    /// </summary>
    public async Task<List<UserDetailDto>> GetListAsync(CancellationToken cancellationToken) =>
        (await _userManager.Users
            .AsNoTracking()
         .ToListAsync(cancellationToken))
        .Adapt<List<UserDetailDto>>();

    /// <summary>
    /// Get total user count
    /// </summary>
    public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
     _userManager.Users.AsNoTracking().CountAsync(cancellationToken);

    /// <summary>
    /// Get user details by ID
    /// </summary>
    public async Task<UserDetailDto> GetAsync(string userId, CancellationToken cancellationToken)
    {
      var user = await _userManager.Users
   .AsNoTracking()
            .Where(u => u.Id == userId)
     .FirstOrDefaultAsync(cancellationToken);

    _ = user ?? throw new NotFoundException("User Not Found.");

        return user.Adapt<UserDetailDto>();
    }

    /// <summary>
    /// Toggle user active status (admin only)
    /// </summary>
    public async Task ToggleStatusAsync(
        ToggleUserStatusRequest request, 
        CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        _ = user ?? throw new NotFoundException("User Not Found.");

   // Không cho phép deactivate admin
    bool isAdmin = await _userManager.IsInRoleAsync(user, ECORoles.Admin);
        if (isAdmin)
      {
            throw new ConflictException("Administrators Profile's Status cannot be toggled");
        }

        user.IsActive = request.ActivateUser;

        await _userManager.UpdateAsync(user);
    }

    #endregion
}
```

**Giải thích:**

**Dependencies:**
- **UserManager:** ASP.NET Core Identity user management
- **SignInManager:** Sign in/out operations
- **RoleManager:** Role management
- **ApplicationDbContext:** Direct database access nếu cần
- **IJobService:** Background jobs (email sending)
- **IMailService:** Email service
- **IEmailTemplateService:** Email templates
- **IFileStorageService:** File upload/download
- **IEventPublisher:** Domain events
- **ICacheService:** Caching

**SearchAsync:**
- Use `EntitiesByPaginationFilterSpec` (from BUILD_11)
- `ProjectToType<UserDetailDto>()`: Mapster projection (efficient, chỉ select fields cần thiết)
- Return `PaginationResponse` với total count

**Exists Methods:**
- **ExistsWithEmailAsync:** Check email unique, exclude current user nếu có
- **ExistsWithNameAsync:** Check username unique
- **ExistsWithPhoneNumberAsync:** Check phone unique, exclude current user

**ToggleStatusAsync:**
- Admin có thể activate/deactivate users
- KHÔNG cho phép toggle admin accounts
- Security check trước khi update

**Tại sao partial class:**
- UserService có nhiều methods (20+ methods)
- Chia thành nhiều files: UserService.cs, UserService.CreateUpdate.cs, UserService.Password.cs, UserService.Role.cs, UserService.Permission.cs, UserService.Confirm.cs
- Dễ maintain và navigate

---

### Bước 4.2: UserService - Create & Update Operations

**Làm gì:** Implement create và update user operations.

**Tại sao:** Separate file cho create/update logic (partial class pattern).

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.CreateUpdate.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Mailing;
using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Domain.Common;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Shared.Authorization;

namespace ECO.WebApi.Infrastructure.Identity;

/// <summary>
/// UserService - Create & Update Operations (Partial Class)
/// </summary>
internal partial class UserService
{
    /// <summary>
    /// Create new user (admin hoặc self-register)
    /// </summary>
    public async Task<string> CreateAsync(CreateUserRequest request, string origin)
    {
        // Create ApplicationUser entity
        var user = new ApplicationUser
        {
Email = request.Email,
      FirstName = request.FirstName,
          LastName = request.LastName,
    UserName = request.UserName,
    PhoneNumber = request.PhoneNumber,
  IsActive = true
   };

        // Create user với password (ASP.NET Core Identity)
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InternalServerException(
  "Validation Errors Occurred.", 
    result.GetErrors());
        }

        // Assign "Basic" role by default
        await _userManager.AddToRoleAsync(user, ECORoles.Basic);

        var messages = new List<string> 
        { 
            $"User {user.UserName} Registered." 
        };

        // Send email confirmation nếu RequireConfirmedAccount = true
   if (_securitySettings.RequireConfirmedAccount && !string.IsNullOrEmpty(user.Email))
        {
      // Generate email verification URI
      string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);
      
            // Create email model
    RegisterUserEmailModel emailModel = new RegisterUserEmailModel()
          {
     Email = user.Email,
     UserName = user.UserName,
     Url = emailVerificationUri
       };

    // Generate email từ template
   var mailRequest = new MailRequest(
      new List<string> { user.Email },
           "Confirm Registration",
          _templateService.GenerateEmailTemplate("email-confirmation", emailModel));

       // Send email bằng background job (không block request)
            _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));

          messages.Add($"Please check {user.Email} to verify your account!");
    }

     return string.Join(Environment.NewLine, messages);
  }

    /// <summary>
    /// Update user profile (với image upload)
    /// </summary>
    public async Task UpdateAsync(UpdateUserRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("User Not Found.");

      // Handle image upload/delete
        string currentImage = user.ImageUrl ?? string.Empty;
    if (request.Image != null || request.DeleteCurrentImage)
        {
         // Upload new image
            user.ImageUrl = await _fileStorage.UploadAsync<ApplicationUser>(
        request.Image, 
    FileType.Image);

       // Delete old image nếu có
   if (request.DeleteCurrentImage && !string.IsNullOrEmpty(currentImage))
        {
    string root = Directory.GetCurrentDirectory();
        _fileStorage.Remove(Path.Combine(root, currentImage));
     }
        }

    // Update basic info
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
      user.PhoneNumber = request.PhoneNumber;

      // Update phone number nếu changed
        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (request.PhoneNumber != phoneNumber)
      {
 await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
        }

        // Update user trong database
        var result = await _userManager.UpdateAsync(user);

  // Refresh sign in (update claims)
 await _signInManager.RefreshSignInAsync(user);

        if (!result.Succeeded)
        {
            throw new InternalServerException("Update profile failed", result.GetErrors());
        }
    }
}
```

**Giải thích:**

**CreateAsync:**
1. Create `ApplicationUser` entity từ request
2. `_userManager.CreateAsync(user, password)`: Create user với password hashing (Identity)
3. Assign "Basic" role by default
4. Nếu `RequireConfirmedAccount = true`:
   - Generate email verification URI
   - Create email model với template data
   - Send email bằng background job (Hangfire)
5. Return success messages

**UpdateAsync:**
1. Find user by ID
2. Handle image upload:
   - Upload new image nếu có
 - Delete old image nếu `DeleteCurrentImage = true`
3. Update basic info (FirstName, LastName, PhoneNumber)
4. `SetPhoneNumberAsync`: Update phone number (Identity method)
5. `RefreshSignInAsync`: Update claims trong current session
6. Return errors nếu update failed

**Tại sao background job cho email:**
- Không block HTTP request
- Retry tự động nếu email fail
- Better user experience (fast response)

**Image Upload Flow:**
```
User uploads avatar
    ↓
UploadAsync<ApplicationUser>(request.Image, FileType.Image)
    ↓
Generate unique filename (Guid)
    ↓
Save to wwwroot/Files/Images/ApplicationUser/
    ↓
Return relative path
    ↓
Update user.ImageUrl
    ↓
Delete old image if DeleteCurrentImage = true
```

---

### Bước 4.3: Email Confirmation Helper Method

**Làm gì:** Helper method để generate email verification URI.

**Tại sao:** Reusable logic cho email confirmation.

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.Confirm.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace ECO.WebApi.Infrastructure.Identity;

/// <summary>
/// UserService - Email/Phone Confirmation Operations (Partial Class)
/// </summary>
internal partial class UserService
{
    /// <summary>
    /// Confirm email với verification code
    /// </summary>
    public async Task<string> ConfirmEmailAsync(
        string userId, 
        string code, 
      CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

    _ = user ?? throw new NotFoundException("User Not Found.");

 // Decode code từ query string
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        // Confirm email với Identity
    var result = await _userManager.ConfirmEmailAsync(user, code);

      if (result.Succeeded)
        {
      return "Email confirmed successfully!";
        }

        throw new InternalServerException("An error occurred while confirming email.");
    }

    /// <summary>
    /// Confirm phone number với verification code
    /// </summary>
    public async Task<string> ConfirmPhoneNumberAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("User Not Found.");

        // Confirm phone với Identity
        var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber!, code);

        if (result.Succeeded)
    {
    return "Phone number confirmed successfully!";
        }

     throw new InternalServerException("An error occurred while confirming phone number.");
    }

    /// <summary>
    /// Generate email verification URI (helper method)
    /// </summary>
    private async Task<string> GetEmailVerificationUriAsync(
        ApplicationUser user, 
        string origin)
    {
 // Generate email confirmation token (Identity)
        string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

     // Encode token for URL
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        // Build verification URI
        const string route = "api/users/confirm-email";
    var endpointUri = new Uri(string.Concat($"{origin}/", route));

        string verificationUri = QueryHelpers.AddQueryString(
       endpointUri.ToString(),
          new Dictionary<string, string>
       {
      ["userId"] = user.Id,
    ["code"] = code
 });

        return verificationUri;
    }
}
```

**Giải thích:**

**ConfirmEmailAsync:**
1. Find user by ID
2. Decode code từ Base64Url (query string encoding)
3. `_userManager.ConfirmEmailAsync(user, code)`: Confirm email với Identity
4. Return success message hoặc throw exception

**ConfirmPhoneNumberAsync:**
- Tương tự ConfirmEmailAsync
- Use `ChangePhoneNumberAsync` với verification code

**GetEmailVerificationUriAsync (Helper):**
1. Generate email confirmation token (Identity)
2. Encode token thành Base64Url (safe for URL)
3. Build verification URI: `https://localhost:7001/api/users/confirm-email?userId=xxx&code=yyy`
4. Return URI để gửi trong email

**Email Confirmation Flow:**
```
User registers
    ↓
GenerateEmailConfirmationTokenAsync (Identity)
    ↓
Encode token to Base64Url
    ↓
Build verification URI
    ↓
Send email với link
  ↓
User clicks link
    ↓
GET /api/users/confirm-email?userId=xxx&code=yyy
    ↓
Decode code
    ↓
ConfirmEmailAsync (Identity)
↓
Email confirmed!
```

---

## 5. Email Templates

### Bước 5.1: RegisterUserEmailModel

**Làm gì:** Model cho email registration template.

**Tại sao:** Type-safe data cho email template rendering.

**File:** `src/Core/Application/Identity/Users/RegisterUserEmailModel.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

/// <summary>
/// Model cho email registration confirmation
/// </summary>
public class RegisterUserEmailModel
{
    /// <summary>
    /// User email
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// Email verification URL
    /// </summary>
    public string Url { get; set; } = default!;
}
```

**Giải thích:**
- **Email:** Recipient email
- **UserName:** Display trong email
- **Url:** Verification link (GET /api/users/confirm-email?...)

---

### Bước 5.2: Email Confirmation Template

**Làm gì:** Razor template cho email confirmation.

**Tại sao:** Professional email với branding.

**File:** `src/Infrastructure/Infrastructure/Mailing/EmailTemplates/email-confirmation.cshtml`

```cshtml
@model ECO.WebApi.Application.Identity.Users.RegisterUserEmailModel

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Confirm Your Email</title>
    <style>
  body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
        padding: 0;
        }
  .container {
            max-width: 600px;
            margin: 50px auto;
        background-color: #ffffff;
         padding: 20px;
        border-radius: 8px;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }
        h1 {
     color: #333333;
        }
 p {
      color: #555555;
         line-height: 1.6;
        }
        .button {
   display: inline-block;
            padding: 10px 20px;
        margin: 20px 0;
  background-color: #007bff;
        color: #ffffff;
   text-decoration: none;
            border-radius: 5px;
        }
    .button:hover {
    background-color: #0056b3;
    }
        .footer {
  margin-top: 20px;
         font-size: 12px;
            color: #999999;
        text-align: center;
  }
    </style>
</head>
<body>
  <div class="container">
        <h1>Welcome to ECO.WebApi, @Model.UserName!</h1>
        <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
        <a href="@Model.Url" class="button">Confirm Email</a>
        <p>If the button doesn't work, copy and paste this link into your browser:</p>
    <p><a href="@Model.Url">@Model.Url</a></p>
        <p>If you did not create an account, please ignore this email.</p>
    <div class="footer">
            <p>&copy; 2024 ECO.WebApi. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

**Giải thích:**
- Razor template với `@model RegisterUserEmailModel`
- Professional HTML email với CSS styling
- Button link đến verification URL
- Fallback text link nếu button không hoạt động
- Footer với copyright

**Template Rendering:**
```csharp
var mailRequest = new MailRequest(
    new List<string> { user.Email },
    "Confirm Registration",
    _templateService.GenerateEmailTemplate("email-confirmation", emailModel));
```

---

## 6. User Controller

### Bước 6.1: UsersController Implementation

**Làm gì:** Expose user management APIs.

**Tại sao:** RESTful endpoints cho user operations.

**File:** `src/Host/Host/Controllers/Identity/UsersController.cs`

```csharp
using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Application.Identity.Users.Password;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Identity;

/// <summary>
/// User management APIs
/// </summary>
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>
    /// Get list of all users
    /// </summary>
    [HttpGet("list")]
    [OpenApiOperation("Get list of all users.", "")]
    public Task<List<UserDetailDto>> GetListAsync(CancellationToken cancellationToken)
    {
        return _userService.GetListAsync(cancellationToken);
    }

    /// <summary>
    /// Get user details by ID
    /// </summary>
    [HttpGet("{id}")]
    [OpenApiOperation("Get a user's details.", "")]
 public Task<UserDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return _userService.GetAsync(id, cancellationToken);
    }

    /// <summary>
    /// Get user's assigned roles
    /// </summary>
    [HttpGet("{id}/roles")]
    [OpenApiOperation("Get a user's roles.", "")]
public Task<List<UserRoleDto>> GetRolesAsync(string id, CancellationToken cancellationToken)
    {
        return _userService.GetRolesAsync(id, cancellationToken);
    }

 /// <summary>
  /// Assign roles to user
    /// </summary>
    [HttpPost("{id}/roles")]
    [OpenApiOperation("Update a user's assigned roles.", "")]
    public Task<string> AssignRolesAsync(
  string id, 
        UserRolesRequest request, 
 CancellationToken cancellationToken)
 {
        return _userService.AssignRolesAsync(id, request, cancellationToken);
    }

    /// <summary>
    /// Create new user (Admin only)
    /// </summary>
    [HttpPost("create")]
    [OpenApiOperation("Creates a new user.", "")]
    public Task<string> CreateAsync(CreateUserRequest request)
    {
     // TODO: Add [MustHavePermission("Users.Create")] trong BUILD_17
        return _userService.CreateAsync(request, GetOriginFromRequest());
    }

    /// <summary>
    /// Self-register (Anonymous - anyone can register)
    /// </summary>
    [HttpPost("self-register")]
    [AllowAnonymous]
    [OpenApiOperation("Anonymous user creates a user.", "")]
    public Task<string> SelfRegisterAsync(CreateUserRequest request)
    {
        // TODO: Add captcha validation để prevent spam
  // TODO: Add rate limiting
      return _userService.CreateAsync(request, GetOriginFromRequest());
    }

    /// <summary>
    /// Toggle user active status (Admin only)
    /// </summary>
    [HttpPost("{id}/toggle-status")]
    [OpenApiOperation("Toggle a user's active status.", "")]
    public async Task<ActionResult> ToggleStatusAsync(
        string id, 
        ToggleUserStatusRequest request, 
   CancellationToken cancellationToken)
  {
        if (id != request.UserId)
        {
       return BadRequest();
        }

        await _userService.ToggleStatusAsync(request, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Confirm email address (GET from email link)
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [OpenApiOperation("Confirm email address for a user.", "")]
    public Task<string> ConfirmEmailAsync(
  [FromQuery] string userId, 
        [FromQuery] string code, 
    CancellationToken cancellationToken)
    {
        return _userService.ConfirmEmailAsync(userId, code, cancellationToken);
    }

    /// <summary>
    /// Confirm phone number (GET from SMS link)
    /// </summary>
    [HttpGet("confirm-phone-number")]
    [AllowAnonymous]
    [OpenApiOperation("Confirm phone number for a user.", "")]
  public Task<string> ConfirmPhoneNumberAsync(
        [FromQuery] string userId, 
        [FromQuery] string code)
    {
        return _userService.ConfirmPhoneNumberAsync(userId, code);
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    [HttpPost("forgot-password")]
  [AllowAnonymous]
    [OpenApiOperation("Request a password reset email for a user.", "")]
    public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return _userService.ForgotPasswordAsync(request, GetOriginFromRequest());
}

    /// <summary>
    /// Reset password với reset token
    /// </summary>
    [HttpPost("reset-password")]
    [OpenApiOperation("Reset a user's password.", "")]
    public Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return _userService.ResetPasswordAsync(request);
    }

    /// <summary>
    /// Get origin URL from request (for email links)
    /// </summary>
    private string GetOriginFromRequest() => 
        $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}
```

**Giải thích:**

**GET /api/users/list:**
- Get all users (no pagination)
- Requires authentication

**GET /api/users/{id}:**
- Get user details by ID
- Requires authentication

**GET /api/users/{id}/roles:**
- Get user's assigned roles
- Sẽ implement trong BUILD_16B

**POST /api/users/{id}/roles:**
- Assign roles to user
- Sẽ implement trong BUILD_16B

**POST /api/users/create:**
- Admin creates user
- TODO: Add `[MustHavePermission("Users.Create")]` trong BUILD_17

**POST /api/users/self-register:**
- Anonymous user registration
- `[AllowAnonymous]` - không cần authentication
- TODO: Add captcha và rate limiting

**POST /api/users/{id}/toggle-status:**
- Toggle user active status
- Admin only
- Check `id == request.UserId` (route parameter match request body)

**GET /api/users/confirm-email:**
- Email confirmation endpoint
- `[AllowAnonymous]` - user chưa login
- Query parameters: userId, code

**GET /api/users/confirm-phone-number:**
- Phone confirmation endpoint
- Tương tự confirm-email

**POST /api/users/forgot-password:**
- Request password reset
- `[AllowAnonymous]`
- Sẽ implement trong phần Password Operations

**POST /api/users/reset-password:**
- Reset password với token
- Sẽ implement trong phần Password Operations

**GetOriginFromRequest():**
- Helper method để get origin URL
- Dùng để build email verification links
- Ví dụ: `https://localhost:7001`

---

## 7. Testing User Service

### Bước 7.1: Test Self-Register API

**API Call:**
```bash
curl -X POST https://localhost:7001/api/users/self-register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "userName": "johndoe",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "phoneNumber": "+84987654321"
  }'
```

**Expected Response:**
```text
User johndoe Registered.
Please check john.doe@example.com to verify your account!
```

---

### Bước 7.2: Test Email Confirmation

**Step 1: Check email inbox (hoặc MailHog/Papercut)**

**Email Content:**
```html
Subject: Confirm Registration

Welcome to ECO.WebApi, johndoe!

Thank you for registering. Please confirm your email address by clicking the button below:

[Confirm Email Button]

Link: https://localhost:7001/api/users/confirm-email?userId=xxx&code=yyy
```

**Step 2: Click confirmation link**

**GET Request:**
```bash
curl -X GET "https://localhost:7001/api/users/confirm-email?userId=xxx&code=yyy"
```

**Expected Response:**
```text
Email confirmed successfully!
```

---

### Bước 7.3: Test Get User Details

**API Call:**
```bash
curl -X GET https://localhost:7001/api/users/{userId} \
  -H "Authorization: Bearer {accessToken}"
```

**Expected Response:**
```json
{
  "id": "3fa85f64-5717-4eb2-b25f-58616aa2ffcc",
  "userName": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "isActive": true,
  "emailConfirmed": true,
  "phoneNumber": "+84987654321",
  "imageUrl": null
}
```

---

### Bước 7.4: Test Error Cases

**Case 1: Duplicate Email**
```bash
curl -X POST https://localhost:7001/api/users/self-register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "userName": "janedoe",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!"
  }'
```

**Response:**
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "Email": ["Email john.doe@example.com is already registered."]
  }
}
```

---

**Case 2: Duplicate Username**
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "UserName": ["Username johndoe is already taken."]
  }
}
```

---

**Case 3: Password Mismatch**
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "ConfirmPassword": ["Password and Confirm Password must match."]
  }
}
```

---

## 8. Summary

### ✅ Đã hoàn thành trong bước này:

**User DTOs:**
- ✅ UserDetailDto (display user info)
- ✅ CreateUserRequest với FluentValidation
- ✅ UpdateUserRequest với image upload
- ✅ ToggleUserStatusRequest
- ✅ UserParameterFilter (search với pagination)

**User Service Interface:**
- ✅ IUserService với partial methods
  - Default operations (Search, Get, Exists checks)
  - Create & Update operations
  - Email confirmation
  - Toggle status

**User Service Implementation:**
- ✅ UserService.cs (main class với dependencies)
- ✅ UserService.CreateUpdate.cs (Create & Update operations)
- ✅ UserService.Confirm.cs (Email confirmation)

**Email Templates:**
- ✅ RegisterUserEmailModel
- ✅ email-confirmation.cshtml (Razor template)

**Controllers:**
- ✅ UsersController với RESTful endpoints

### 📊 User Registration Flow:

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ 1. POST /api/users/self-register
       ▼
┌──────────────┐
│ UserService  │
└──────┬───────┘
       │ 2. Create user + Assign role
       │ 3. Send confirmation email
    ▼
┌─────────────┐
│   Email│
└──────┬──────┘
       │ 4. User clicks link
       │ 5. GET /api/users/confirm-email
       ▼
┌─────────────┐
│   Success   │
└─────────────┘
```

### 📁 File Structure:

```
src/
├── Core/
│   └── Application/
│       └── Identity/
│      └── Users/
│       ├── IUserService.cs
│    ├── UserDetailDto.cs
│           ├── CreateUserRequest.cs
│               ├── UpdateUserRequest.cs
│   ├── ToggleUserStatusRequest.cs
│     ├── UserParameterFilter.cs
│ └── RegisterUserEmailModel.cs
├── Infrastructure/
│   └── Infrastructure/
│       ├── Identity/
│       │   ├── UserService.cs
│       │   ├── UserService.CreateUpdate.cs
│       │   └── UserService.Confirm.cs
│       └── Mailing/
│       └── EmailTemplates/
│               └── email-confirmation.cshtml
└── Host/
    └── Host/
        └── Controllers/
└── Identity/
         └── UsersController.cs
```

---

## 9. Next Steps

**Tiếp theo:** [BUILD_16B - Role Service](BUILD_16B_Role_Service.md)

Trong bước tiếp theo, chúng ta sẽ xây dựng Role Management Service:
1. ✅ Role CRUD operations
2. ✅ Assign permissions to roles
3. ✅ Role DTOs và validation
4. ✅ RoleController với RESTful endpoints
5. ✅ Update UserService.Role.cs (assign roles to users)

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
