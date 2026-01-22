# Application Services - UserService, RoleService, TokenService

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn về các Application Services: UserService, RoleService, và TokenService.

---

## Bước 13.1: Implement TokenService

**Làm gì:** Service để generate và refresh JWT tokens.

**File:** `src/Core/Application/Identity/Tokens/ITokenService.cs`

```csharp
using ECO.WebApi.Domain.Identity;

namespace ECO.WebApi.Application.Identity.Tokens;

public interface ITokenService : ITransientService
{
    Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, string ipAddress);
}
```

**Implementation:** `TokenService`
- Generate JWT token với claims (UserId, Email, Roles, etc.)
- Generate refresh token (random string)
- Validate và refresh tokens
- Update user với refresh token và expiry time

**Tác dụng:**
- JWT authentication
- Refresh token để renew access token
- Claims-based authorization

---

## Bước 13.2: Implement UserService

**Làm gì:** Service để quản lý users.

**File:** `src/Core/Application/Identity/Users/IUserService.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Users;

public interface IUserService : ITransientService
{
    // CRUD operations
    Task<PaginationResponse<UserDto>> SearchAsync(SearchUsersRequest request, CancellationToken cancellationToken);
    Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken);
    Task<string> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<string> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken);
    Task<string> DeleteAsync(string userId, CancellationToken cancellationToken);
    
    // Other operations
    Task<bool> ExistsWithNameAsync(string name);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);
}
```

**Implementation:** `UserService`
- Sử dụng `UserManager<ApplicationUser>` để quản lý users
- Mapster để map giữa DTOs và entities
- Validation và error handling

**Tác dụng:**
- CRUD operations cho users
- Search và filter users
- Check duplicate (name, email, phone)

---

## Bước 13.3: Implement RoleService

**Làm gì:** Service để quản lý roles và permissions.

**File:** `src/Core/Application/Identity/Roles/IRoleService.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Roles;

public interface IRoleService : ITransientService
{
    Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken);
    Task<RoleDto> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<string> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task<string> UpdateAsync(UpdateRoleRequest request, CancellationToken cancellationToken);
    Task<string> DeleteAsync(string id, CancellationToken cancellationToken);
    Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken);
    Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
}
```

**Implementation:** `RoleService`
- Sử dụng `RoleManager<ApplicationRole>` để quản lý roles
- Quản lý permissions (Actions, Functions)
- Map roles với permissions

**Tác dụng:**
- CRUD operations cho roles
- Assign permissions cho roles
- Get roles với permissions

---

## Tóm tắt

### Các services:

1. **TokenService** → Generate và refresh JWT tokens
2. **UserService** → CRUD và quản lý users
3. **RoleService** → CRUD và quản lý roles/permissions

### Điểm quan trọng:

- **TokenService** → JWT với refresh token mechanism
- **UserService** → Sử dụng UserManager từ Identity
- **RoleService** → Quản lý permissions với Actions/Functions
- **Mapster** → Mapping giữa DTOs và entities

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
