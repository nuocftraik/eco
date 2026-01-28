# JWT Authentication - Token Service & Middleware

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** PHASE 3 (Core Services - BUILD_12, BUILD_13, BUILD_14) đã hoàn thành

Tài liệu này hướng dẫn xây dựng JWT Authentication System với Token Service, Refresh Tokens, và JWT middleware configuration.

---

## 1. Overview

**Làm gì:** Xây dựng hệ thống JWT Authentication hoàn chỉnh với access tokens, refresh tokens, và token validation.

**Tại sao cần:**
- **Stateless Authentication:** JWT tokens không cần lưu trữ server-side sessions
- **Scalability:** Dễ dàng scale horizontally vì không có session state
- **Security:** Claims-based authentication với digital signature
- **Refresh Token Support:** Renew access tokens without re-login
- **Cross-Platform:** JWT tokens hoạt động trên mọi platform (web, mobile, desktop)
- **Microservices Ready:** Tokens có thể share giữa các services

**Trong bước này chúng ta sẽ:**
- ✅ Setup JWT configuration (JwtSettings)
- ✅ Tạo `ITokenService` interface
- ✅ Implement `TokenService` (generate access/refresh tokens)
- ✅ Tạo DTOs (TokenRequest, TokenResponse, RefreshTokenRequest)
- ✅ Implement validators (TokenRequestValidator)
- ✅ Setup JWT authentication middleware
- ✅ Tạo Tokens controller (login, refresh)
- ✅ Testing với Swagger

**Real-world example:**
```csharp
// Login request
POST /api/tokens
{
  "email": "admin@root.com",
  "password": "123Pa$$word!"
}

// Response - Access token + Refresh token
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "CfDJ8K...",
  "refreshTokenExpiryTime": "2024-02-01T00:00:00Z"
}

// Use token trong API calls
GET /api/users/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

// Refresh token when expired
POST /api/tokens/refresh
{
  "token": "expired-token",
  "refreshToken": "CfDJ8K..."
}
```

---

## 2. Add Required Packages

### Bước 2.1: Add JWT Authentication Packages

**File:** `src/Infrastructure/Infrastructure/Infrastructure.csproj`

```xml
<ItemGroup>
    <!-- JWT Authentication -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
</ItemGroup>
```

**Giải thích packages:**
- `Microsoft.AspNetCore.Authentication.JwtBearer`: JWT authentication middleware cho ASP.NET Core
- `System.IdentityModel.Tokens.Jwt`: JWT token generation và validation

**⚠️ Lưu ý:**
- Version phải match với .NET 8
- Identity packages đã có từ BUILD_03 (Domain layer)

---

## 3. JWT Configuration

### Bước 3.1: JwtSettings Model

**Làm gì:** Tạo model để map JWT configuration từ appsettings.json.

**Tại sao:** Centralized configuration, dễ thay đổi settings mà không cần rebuild.

**File:** `src/Infrastructure/Infrastructure/Auth/Jwt/JwtSettings.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Auth.Jwt;

/// <summary>
/// JWT configuration settings
/// Maps từ appsettings.json section "JwtSettings"
/// </summary>
public class JwtSettings
{
    /// <summary>
  /// Secret key để sign JWT tokens (phải >= 32 characters)
    /// </summary>
    public string Key { get; set; } = default!;

    /// <summary>
    /// Token expiration time in minutes (default: 60)
    /// </summary>
    public int TokenExpirationInMinutes { get; set; }

    /// <summary>
    /// Refresh token expiration time in days (default: 7)
    /// </summary>
    public int RefreshTokenExpirationInDays { get; set; }
}
```

**Giải thích:**
- `Key`: Secret key để sign tokens - phải đủ mạnh (>= 32 chars)
- `TokenExpirationInMinutes`: Access token lifetime (ngắn - 60 phút)
- `RefreshTokenExpirationInDays`: Refresh token lifetime (dài - 7 ngày)

**Tại sao design này:**
- Access token ngắn → giảm risk nếu bị compromise
- Refresh token dài → user experience tốt hơn (không cần re-login thường xuyên)

---

### Bước 3.2: Configuration File

**Làm gì:** Tạo configuration file cho JWT settings.

**File:** `src/Host/Host/Configurations/security.json`

```json
{
  "JwtSettings": {
 "Key": "S0M3RAN0MS3CR3T!1!MAG1C!1!",
    "TokenExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

**⚠️ QUAN TRỌNG - Security Best Practices:**

**Development:**
```json
{
  "JwtSettings": {
    "Key": "your-super-secret-development-key-minimum-32-characters-long",
    "TokenExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

**Production (User Secrets / Environment Variables):**
```bash
# Đừng commit vào source control!
# Dùng User Secrets:
dotnet user-secrets set "JwtSettings:Key" "production-key-super-secure-minimum-32-characters"

# Hoặc Environment Variables:
export JwtSettings__Key="production-key-super-secure-minimum-32-characters"
```

**Tại sao:**
- Development: Key có thể commit (chỉ để dev)
- Production: Key PHẢI secure, không commit vào Git
- Environment Variables hoặc Azure Key Vault cho production

---

### Bước 3.3: Load Configuration

**Làm gì:** Load security.json vào Program.cs.

**File:** `src/Host/Host/Program.cs`

```csharp
// Add configuration files
builder.Configuration
.AddJsonFile("Configurations/database.json", optional: false, reloadOnChange: true)
    .AddJsonFile("Configurations/security.json", optional: false, reloadOnChange: true) // ← Add này
    .AddEnvironmentVariables();
```

**Giải thích:**
- `optional: false` → File bắt buộc phải có
- `reloadOnChange: true` → Auto reload nếu file thay đổi
- Environment Variables override JSON config (production)

---

## 4. Token Service Interface

### Bước 4.1: ITokenService Interface

**Làm gì:** Tạo interface cho token operations.

**Tại sao:** Abstraction layer, dễ mock cho testing, có thể swap implementations.

**File:** `src/Core/Application/Common/Interfaces/ITokenService.cs`

```csharp
using ECO.WebApi.Application.Identity.Tokens;

namespace ECO.WebApi.Application.Common.Interfaces;

/// <summary>
/// Service để generate và validate JWT tokens
/// </summary>
public interface ITokenService : ITransientService
{
 /// <summary>
    /// Generate access token và refresh token cho user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    /// <param name="permissions">User permissions (optional)</param>
    /// <returns>Token response với access token và refresh token</returns>
    Task<TokenResponse> GetTokenAsync(
        string userId,
      string email,
        IList<string> roles,
     IList<string>? permissions = null);

    /// <summary>
    /// Refresh access token bằng refresh token
  /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New token response</returns>
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
```

**Giải thích:**

**GetTokenAsync:**
- Generate access token + refresh token
- Claims: UserId, Email, Roles, Permissions
- Dùng khi login thành công

**RefreshTokenAsync:**
- Validate refresh token
- Generate new access token + refresh token
- Dùng khi access token expired

**Tại sao Transient:**
- Lightweight service, không maintain state
- Có thể create instance mới mỗi request
- Better performance

---

## 5. Token DTOs

### Bước 5.1: TokenRequest DTO

**Làm gì:** Request DTO cho login.

**File:** `src/Core/Application/Identity/Tokens/TokenRequest.cs`

```csharp
using FluentValidation;
using MediatR;

namespace ECO.WebApi.Application.Identity.Tokens;

/// <summary>
/// Request DTO để login và lấy tokens
/// </summary>
public class TokenRequest : IRequest<TokenResponse>
{
    /// <summary>
    /// User email
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; } = default!;
}

/// <summary>
/// Validator cho TokenRequest
/// </summary>
public class TokenRequestValidator : AbstractValidator<TokenRequest>
{
    public TokenRequestValidator()
    {
        RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required.");
    }
}
```

**Giải thích:**
- Simple request: Email + Password
- Validator tự động chạy bởi ValidationBehavior (BUILD_14)
- Return `TokenResponse` nếu login thành công

---

### Bước 5.2: TokenResponse DTO

**Làm gì:** Response DTO chứa tokens.

**File:** `src/Core/Application/Identity/Tokens/TokenResponse.cs`

```csharp
namespace ECO.WebApi.Application.Identity.Tokens;

/// <summary>
/// Response DTO chứa access token và refresh token
/// </summary>
public class TokenResponse
{
    /// <summary>
 /// JWT access token (dùng cho Authorization header)
    /// </summary>
    public string Token { get; set; } = default!;

    /// <summary>
    /// Refresh token (dùng để renew access token)
    /// </summary>
    public string RefreshToken { get; set; } = default!;

    /// <summary>
    /// Refresh token expiry time (UTC)
    /// </summary>
  public DateTime RefreshTokenExpiryTime { get; set; }
}
```

**Giải thích:**
- `Token`: Access token - dùng trong Authorization header
- `RefreshToken`: Refresh token - dùng để renew khi access token expired
- `RefreshTokenExpiryTime`: Client biết khi nào cần refresh

---

### Bước 5.3: RefreshTokenRequest DTO

**Làm gì:** Request DTO để refresh token.

**File:** `src/Core/Application/Identity/Tokens/RefreshTokenRequest.cs`

```csharp
using FluentValidation;
using MediatR;

namespace ECO.WebApi.Application.Identity.Tokens;

/// <summary>
/// Request DTO để refresh access token
/// </summary>
public class RefreshTokenRequest : IRequest<TokenResponse>
{
    /// <summary>
    /// Expired access token
    /// </summary>
    public string Token { get; set; } = default!;

    /// <summary>
    /// Valid refresh token
    /// </summary>
    public string RefreshToken { get; set; } = default!;
}

/// <summary>
/// Validator cho RefreshTokenRequest
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
   RuleFor(x => x.Token)
    .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.RefreshToken)
    .NotEmpty().WithMessage("Refresh token is required.");
 }
}
```

**Giải thích:**
- Client gửi cả access token (expired) và refresh token
- Server validate refresh token
- Generate new tokens nếu refresh token valid

---

## 6. Token Service Implementation

### Bước 6.1: TokenService Class

**Làm gì:** Implement TokenService với JWT generation và validation.

**Tại sao:** Core logic để generate secure JWT tokens với claims.

**File:** `src/Infrastructure/Infrastructure/Auth/Jwt/TokenService.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Application.Identity.Tokens;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECO.WebApi.Infrastructure.Auth.Jwt;

/// <summary>
/// Implementation của ITokenService
/// Generate và validate JWT tokens
/// </summary>
public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public TokenService(
     UserManager<ApplicationUser> userManager,
  IOptions<JwtSettings> jwtSettings)
    {
    _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Generate access token và refresh token
    /// </summary>
    public async Task<TokenResponse> GetTokenAsync(
      string userId,
        string email,
        IList<string> roles,
   IList<string>? permissions = null)
    {
   // 1. Lấy user từ database
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        // 2. Generate access token
        var token = GenerateJwt(user, roles, permissions);

        // 3. Generate refresh token
     var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        // 4. Save refresh token vào database
      user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
        await _userManager.UpdateAsync(user);

        // 5. Return token response
        return new TokenResponse
        {
     Token = token,
        RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiryTime
};
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // 1. Validate access token (allow expired)
   var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
 var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? throw new UnauthorizedException("Invalid token.");

        // 2. Lấy user từ database
 var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

  // 3. Validate refresh token
        if (user.RefreshToken != request.RefreshToken)
            throw new UnauthorizedException("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired.");

        // 4. Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(user);

        return await GetTokenAsync(user.Id.ToString(), user.Email!, roles, permissions);
    }

    /// <summary>
    /// Generate JWT access token
    /// </summary>
    private string GenerateJwt(ApplicationUser user, IList<string> roles, IList<string>? permissions)
    {
    // 1. Create claims
        var claims = new List<Claim>
        {
 new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
        new(ClaimTypes.Name, user.FirstName ?? string.Empty),
   new(ClaimTypes.Surname, user.LastName ?? string.Empty),
  new(ECOClaims.Fullname, $"{user.FirstName} {user.LastName}"),
   new(ECOClaims.ImageUrl, user.ImageUrl ?? string.Empty),
   new(ECOClaims.Expiration, DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes).ToUnixTimeSeconds().ToString())
        };

        // 2. Add roles vào claims
        foreach (var role in roles)
 {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 3. Add permissions vào claims
      if (permissions != null)
 {
          foreach (var permission in permissions)
       {
       claims.Add(new Claim(ECOClaims.Permission, permission));
       }
 }

        // 4. Create signing credentials
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        // 5. Create JWT token
  var jwtSecurityToken = new JwtSecurityToken(
    claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
            signingCredentials: signingCredentials);

    // 6. Return token string
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    /// <summary>
    /// Generate random refresh token
    /// </summary>
 private static string GenerateRefreshToken()
    {
     var randomNumber = new byte[32];
    using var rng = RandomNumberGenerator.Create();
  rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Get principal từ expired token
    /// </summary>
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
       ValidateIssuer = false,
            ValidateAudience = false,
    ValidateLifetime = false // Allow expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
       !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
 {
            throw new UnauthorizedException("Invalid token.");
    }

        return principal;
    }

    /// <summary>
    /// Get user permissions từ roles
    /// </summary>
    private async Task<List<string>> GetPermissionsAsync(ApplicationUser user)
    {
      var permissions = new List<string>();
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await _userManager.Users
  .SelectMany(u => u.UserRoles)
         .Where(ur => ur.Role.Name == roleName)
            .Select(ur => ur.Role)
          .FirstOrDefaultAsync();

   if (role?.RoleClaims != null)
        {
         permissions.AddRange(
       role.RoleClaims
                 .Where(rc => rc.ClaimType == ECOClaims.Permission)
            .Select(rc => rc.ClaimValue!)
     );
            }
        }

        return permissions.Distinct().ToList();
    }
}
```

**Giải thích chi tiết:**

**GetTokenAsync Flow:**
1. Lấy user từ database
2. Generate JWT với claims (UserId, Email, Roles, Permissions)
3. Generate random refresh token
4. Save refresh token + expiry vào database
5. Return TokenResponse

**JWT Claims:**
- `NameIdentifier`: User ID (Guid)
- `Email`: User email
- `Name`: First name
- `Surname`: Last name
- `Fullname`: Full name (custom claim)
- `Role`: User roles (multiple claims)
- `Permission`: User permissions (multiple claims)

**RefreshTokenAsync Flow:**
1. Validate access token (allow expired)
2. Extract UserId từ token
3. Validate refresh token trong database
4. Check expiry time
5. Generate new tokens

**Security:**
- Refresh token: Random 32 bytes, Base64 encoded
- JWT signed với HmacSha256
- Refresh token stored trong database (not in JWT)

**Tại sao design này:**
- Access token stateless (no database lookup)
- Refresh token stateful (database validation)
- Short-lived access token (security)
- Long-lived refresh token (UX)

---

## 7. Token Request Handler

### Bước 7.1: GetTokenHandler

**Làm gì:** Handler để xử lý login request.

**File:** `src/Core/Application/Identity/Tokens/GetTokenHandler.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECO.WebApi.Application.Identity.Tokens;

/// <summary>
/// Handler để login và generate tokens
/// </summary>
public class GetTokenHandler : IRequestHandler<TokenRequest, TokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public GetTokenHandler(
     UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
      _userManager = userManager;
  _signInManager = signInManager;
     _tokenService = tokenService;
    }

    public async Task<TokenResponse> Handle(TokenRequest request, CancellationToken cancellationToken)
    {
        // 1. Tìm user theo email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
    {
            throw new UnauthorizedException("Invalid email or password.");
        }

        // 2. Check user active
        if (!user.IsActive)
        {
     throw new UnauthorizedException("User is not active. Please contact administrator.");
    }

        // 3. Validate password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        
      if (result.IsLockedOut)
   {
        throw new UnauthorizedException("User account is locked out.");
   }

        if (!result.Succeeded)
        {
       throw new UnauthorizedException("Invalid email or password.");
        }

        // 4. Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        
        return await _tokenService.GetTokenAsync(
            user.Id.ToString(),
 user.Email!,
            roles);
    }
}
```

**Giải thích:**

**Security checks:**
1. User tồn tại?
2. User active?
3. Password đúng?
4. Account bị lock?

**Error Messages:**
- Generic message "Invalid email or password" (không expose thông tin)
- Specific message cho locked/inactive (để user biết)

**Lockout:**
- `lockoutOnFailure: true` → lock account sau N failed attempts
- Security feature của ASP.NET Identity

---

### Bước 7.2: RefreshTokenHandler

**Làm gì:** Handler để refresh token.

**File:** `src/Core/Application/Identity/Tokens/RefreshTokenHandler.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using MediatR;

namespace ECO.WebApi.Application.Identity.Tokens;

/// <summary>
/// Handler để refresh access token
/// </summary>
public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, TokenResponse>
{
    private readonly ITokenService _tokenService;

public RefreshTokenHandler(ITokenService tokenService)
    {
      _tokenService = tokenService;
    }

    public async Task<TokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
  {
      // Delegate to TokenService
      return await _tokenService.RefreshTokenAsync(request);
    }
}
```

**Giải thích:**
- Simple handler, delegate logic vào TokenService
- TokenService handle validation và generation

---

## 8. JWT Middleware Configuration

### Bước 8.1: Register TokenService

**Làm gì:** Register TokenService vào DI container.

**File:** `src/Infrastructure/Infrastructure/Auth/Startup.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Infrastructure.Auth.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Auth;

internal static class Startup
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        return services
            // Bind JwtSettings từ config
       .Configure<JwtSettings>(config.GetSection(nameof(JwtSettings)))
            
         // Register CurrentUser
        .AddCurrentUser()
       
 // Register TokenService
            .AddTransient<ITokenService, TokenService>();
    }

    internal static IApplicationBuilder UseCurrentUserMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<CurrentUserMiddleware>();
}
```

**Giải thích:**
- `Configure<JwtSettings>()`: Bind config vào JwtSettings
- TokenService registered as Transient
- CurrentUser middleware từ BUILD_12

---

### Bước 8.2: JWT Authentication Middleware

**Làm gì:** Configure JWT authentication middleware.

**File:** `src/Infrastructure/Infrastructure/Auth/Startup.cs` (continue)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECO.WebApi.Infrastructure.Auth;

internal static class Startup
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<JwtSettings>(config.GetSection(nameof(JwtSettings)))
  .AddCurrentUser()
            .AddTransient<ITokenService, TokenService>()
            
    // Add JWT Authentication
    .AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
      .AddJwtBearer(options =>
     {
           var jwtSettings = config.GetSection(nameof(JwtSettings)).Get<JwtSettings>()!;

         options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set true in production
     options.TokenValidationParameters = new TokenValidationParameters
                {
          ValidateIssuerSigningKey = true,
  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
ValidateIssuer = false,
ValidateAudience = false,
    ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // No tolerance for expiration
      };
   })
        .Services;
    }
}
```

**Giải thích:**

**Authentication Configuration:**
- `DefaultAuthenticateScheme`: JWT Bearer
- `DefaultChallengeScheme`: Return 401 nếu unauthorized

**Token Validation Parameters:**
- `ValidateIssuerSigningKey = true`: Validate signature
- `IssuerSigningKey`: Key để verify signature
- `ValidateIssuer = false`: Không validate issuer (single app)
- `ValidateAudience = false`: Không validate audience
- `ValidateLifetime = true`: Check expiration
- `ClockSkew = TimeSpan.Zero`: No time tolerance

**Security Notes:**
- `RequireHttpsMetadata = false`: Chỉ để development
- Production: Set `true` và enforce HTTPS

---

### Bước 8.3: Update Infrastructure Startup

**Làm gì:** Register Auth services trong Infrastructure Startup.

**File:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
using ECO.WebApi.Infrastructure.Auth;
using ECO.WebApi.Infrastructure.Common;
using ECO.WebApi.Infrastructure.Middleware;
using ECO.WebApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
      return services
    .AddPersistence()
     .AddAuth(config) // ← Add Auth services
  .AddCommonServices()
            .AddExceptionMiddleware()
          .AddRouting(options => options.LowercaseUrls = true);
    }

    public static IApplicationBuilder UseInfrastructure(
   this IApplicationBuilder builder,
      IConfiguration config)
    {
        return builder
            .UseExceptionMiddleware()
         .UseRouting()
     .UseAuthentication() // ← Add này
     .UseCurrentUserMiddleware()
   .UseAuthorization() // ← Add này (sẽ implement trong BUILD_17)
     .UseHttpsRedirection();
    }
}
```

**⚠️ THỨ TỰ MIDDLEWARE QUAN TRỌNG:**
```
1. UseExceptionMiddleware() - Catch all exceptions
2. UseRouting() - Route matching
3. UseAuthentication() - Populate User principal
4. UseCurrentUserMiddleware() - Set ICurrentUser
5. UseAuthorization() - Check permissions (BUILD_17)
6. MapControllers() - Execute endpoints
```

**Tại sao thứ tự này:**
- Authentication trước Authorization
- CurrentUser sau Authentication (cần User principal)

---

## 9. Tokens Controller

### Bước 9.1: TokensController

**Làm gì:** Controller để expose token endpoints.

**File:** `src/Host/Host/Controllers/Identity/TokensController.cs`

```csharp
using ECO.WebApi.Application.Identity.Tokens;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.WebApi.Host.Controllers.Identity;

/// <summary>
/// Controller để authenticate và generate tokens
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Không cần authentication
public class TokensController : ControllerBase
{
  private readonly ISender _mediator;

    public TokensController(ISender mediator)
    {
_mediator = mediator;
    }

    /// <summary>
    /// Login và lấy access token + refresh token
    /// </summary>
    /// <param name="request">Login request (email + password)</param>
    /// <returns>Token response</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TokenResponse>> GetTokenAsync(TokenRequest request)
    {
        var response = await _mediator.Send(request);
     return Ok(response);
    }

/// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New token response</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TokenResponse>> RefreshAsync(RefreshTokenRequest request)
    {
        var response = await _mediator.Send(request);
  return Ok(response);
    }
}
```

**Giải thích:**

**Login Endpoint:**
- `POST /api/tokens`
- Public endpoint (AllowAnonymous)
- Return access token + refresh token

**Refresh Endpoint:**
- `POST /api/tokens/refresh`
- Public endpoint
- Renew access token

**Response Codes:**
- 200: Success
- 401: Unauthorized (invalid credentials/token)
- 400: Validation error (từ ValidationBehavior)

---

## 10. Testing

### Bước 10.1: Test Login - Success

**Request:**
```bash
curl -X POST https://localhost:7001/api/tokens \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@root.com",
    "password": "123Pa$$word!"
  }'
```

**Expected Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
  "refreshToken": "CfDJ8Kzw...",
  "refreshTokenExpiryTime": "2024-02-01T00:00:00Z"
}
```

---

### Bước 10.2: Test Login - Invalid Credentials

**Request:**
```bash
curl -X POST https://localhost:7001/api/tokens \
  -H "Content-Type: application/json" \
  -d '{
    "email": "wrong@email.com",
    "password": "wrongpassword"
}'
```

**Expected Response (401):**
```json
{
  "statusCode": 401,
  "exception": "Invalid email or password.",
  "errorId": "a1b2c3d4-...",
  "supportMessage": "Provide the ErrorId..."
}
```

---

### Bước 10.3: Test Use Token

**Request:**
```bash
# Decode token ở jwt.io để see claims
# Use token trong Authorization header
curl -X GET https://localhost:7001/api/users/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Expected Response (200):**
```json
{
  "id": "a1b2c3d4-...",
  "email": "admin@root.com",
  "firstName": "Admin",
  "lastName": "Root"
}
```

---

### Bước 10.4: Test Refresh Token

**Request:**
```bash
curl -X POST https://localhost:7001/api/tokens/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "token": "expired-access-token",
    "refreshToken": "CfDJ8Kzw..."
  }'
```

**Expected Response (200):**
```json
{
  "token": "new-access-token",
  "refreshToken": "new-refresh-token",
  "refreshTokenExpiryTime": "2024-02-08T00:00:00Z"
}
```

---

### Bước 10.5: Test Invalid Refresh Token

**Request:**
```bash
curl -X POST https://localhost:7001/api/tokens/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "token": "expired-token",
    "refreshToken": "invalid-refresh-token"
  }'
```

**Expected Response (401):**
```json
{
  "statusCode": 401,
  "exception": "Invalid refresh token.",
  "errorId": "b2c3d4e5-...",
  "supportMessage": "Provide the ErrorId..."
}
```

---

## 11. Swagger Configuration

### Bước 11.1: Add JWT to Swagger

**Làm gì:** Configure Swagger để support JWT authentication.

**File:** `src/Host/Host/Program.cs`

```csharp
// Configure Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECO.WebApi",
        Version = "v1",
        Description = "Clean Architecture API with JWT Authentication"
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
      BearerFormat = "JWT",
        In = ParameterLocation.Header,
      Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
 {
     Reference = new OpenApiReference
   {
                 Type = ReferenceType.SecurityScheme,
               Id = "Bearer"
        }
    },
            Array.Empty<string>()
        }
    });
});
```

**Giải thích:**
- Add "Authorize" button trong Swagger UI
- User paste token vào để test protected endpoints
- Swagger tự động add "Authorization: Bearer {token}" header

**Usage trong Swagger:**
1. Click "Authorize" button
2. Paste token (không cần "Bearer " prefix)
3. Click "Authorize"
4. Test protected endpoints

---

## 12. Security Best Practices

### ✅ Do's (Nên làm)

**1. Secure Secret Key:**
```csharp
// ✅ Production - Environment Variable
export JwtSettings__Key="super-secure-production-key-minimum-32-characters-long"

// ✅ Azure - Key Vault
services.AddAzureKeyVault();

// ❌ Development only - trong code
const string Key = "dev-key"; // KHÔNG BAO GIỜ commit vào production
```

**2. Short-lived Access Tokens:**
```json
// ✅ Đúng - Short expiration
{
  "TokenExpirationInMinutes": 60
}

// ❌ Sai - Quá dài
{
  "TokenExpirationInMinutes": 43200 // 30 days
}
```

**3. HTTPS Only in Production:**
```csharp
// ✅ Production
options.RequireHttpsMetadata = true;

// Development only
options.RequireHttpsMetadata = false;
```

**4. Validate Token Lifetime:**
```csharp
// ✅ Đúng
ValidateLifetime = true,
ClockSkew = TimeSpan.Zero

// ❌ Sai
ValidateLifetime = false // Tokens never expire!
```

---

### ❌ Don'ts (Không nên làm)

**1. Commit Secret Keys:**
```json
// ❌ SAI - Commit vào Git
{
  "JwtSettings": {
    "Key": "production-secret-key"
  }
}

// ✅ ĐÚNG - User Secrets / Environment Variables
dotnet user-secrets set "JwtSettings:Key" "secret"
```

**2. Store Sensitive Data in JWT:**
```csharp
// ❌ Sai - Password, credit card trong JWT
claims.Add(new Claim("password", user.Password));

// ✅ Đúng - Chỉ non-sensitive claims
claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
```

**3. Long Access Token Lifetime:**
```json
// ❌ Sai
{
"TokenExpirationInMinutes": 10080 // 7 days
}

// ✅ Đúng
{
  "TokenExpirationInMinutes": 60 // 1 hour
}
```

**4. No Refresh Token Rotation:**
```csharp
// ❌ Sai - Reuse old refresh token
return oldRefreshToken;

// ✅ Đúng - Generate new refresh token
return GenerateRefreshToken();
```

---

### 💡 Security Tips

**1. Token Storage:**
- **Mobile/Desktop:** Secure storage (Keychain, Keystore)
- **Web:** HttpOnly cookie (không access từ JavaScript)
- **SPA:** Memory only, refresh token trong HttpOnly cookie

**2. Token Revocation:**
```csharp
// Implement token blacklist
public async Task RevokeTokenAsync(string token)
{
    // Add to blacklist (Redis)
    await _cache.SetAsync($"revoked:{token}", true, TimeSpan.FromHours(1));
}
```

**3. Audit Logging:**
```csharp
// Log token generation
_logger.LogInformation("Token generated for user {UserId}", userId);

// Log failed login attempts
_logger.LogWarning("Failed login attempt for {Email}", email);
```

**4. Rate Limiting:**
```csharp
// Limit login attempts
[RateLimit(requests: 5, window: "1m")]
public async Task<TokenResponse> GetTokenAsync(TokenRequest request)
```

---

## 13. Common Issues & Solutions

### Issue 1: "IDX10503: Signature validation failed"

**Triệu chứng:**
```
Microsoft.IdentityModel.Tokens.SecurityTokenSignatureKeyNotFoundException: 
IDX10503: Signature validation failed. Keys tried: '[PII is hidden]'.
```

**Nguyên nhân:**
- Secret key không match giữa generate và validate
- Key bị thay đổi sau khi generate token

**Giải pháp:**
```csharp
// Đảm bảo dùng CÙNG KEY
// Generation
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

// Validation
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
```

---

### Issue 2: "Token expired"

**Triệu chứng:**
```
401 Unauthorized
"The token expired at '2024-01-15T10:00:00Z'"
```

**Giải pháp:**
```csharp
// Client side - Implement token refresh
if (response.status === 401) {
    // Try refresh token
    const newToken = await refreshToken();
    // Retry original request
}
```

---

### Issue 3: "ClockSkew issue"

**Triệu chứng:**
Token valid nhưng vẫn bị reject vì time difference.

**Giải pháp:**
```csharp
// Set ClockSkew = Zero cho strict validation
ClockSkew = TimeSpan.Zero

// Hoặc allow small tolerance
ClockSkew = TimeSpan.FromMinutes(5)
```

---

### Issue 4: "User not found in token"

**Triệu chứng:**
```
ICurrentUser.GetUserId() returns empty Guid
```

**Giải pháp:**
```csharp
// Đảm bảo claim type đúng
claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

// Not:
claims.Add(new Claim("userId", userId)); // Wrong claim type
```

---

## 14. Summary

### ✅ Đã hoàn thành trong bước này:

**Core Components:**
- ✅ `JwtSettings` configuration model
- ✅ `ITokenService` interface
- ✅ `TokenService` implementation
- ✅ JWT generation với claims
- ✅ Refresh token generation
- ✅ Token validation

**DTOs:**
- ✅ `TokenRequest` (Login)
- ✅ `TokenResponse` (Tokens)
- ✅ `RefreshTokenRequest` (Refresh)
- ✅ Validators cho tất cả requests

**Handlers:**
- ✅ `GetTokenHandler` (Login)
- ✅ `RefreshTokenHandler` (Refresh)

**Infrastructure:**
- ✅ JWT authentication middleware
- ✅ Token validation configuration
- ✅ Auth services registration

**API:**
- ✅ `TokensController` với login và refresh endpoints
- ✅ Swagger JWT configuration

### 🎯 Key Concepts:

**JWT Authentication Flow:**
```
1. User Login
    ↓
2. Validate Credentials
    ↓
3. Generate Access Token (short-lived)
    ↓
4. Generate Refresh Token (long-lived)
    ↓
5. Save Refresh Token in DB
    ↓
6. Return both tokens to client
    ↓
7. Client uses Access Token in requests
    ↓
8. When Access Token expires
    ↓
9. Use Refresh Token to get new tokens
    ↓
10. Repeat from step 7
```

**Token Structure:**
```
JWT = Header.Payload.Signature

Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload (Claims):
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": ["Admin"],
  "permission": ["Users.View"],
  "exp": 1706198400
}

Signature:
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret-key
)
```

**Security Model:**
```
Access Token:
- Short-lived (60 minutes)
- Stateless (no DB lookup)
- Contains claims
- Used for API requests

Refresh Token:
- Long-lived (7 days)
- Stateful (stored in DB)
- No claims
- Used to renew Access Token
```

### 📁 File Structure:

```
src/Core/Application/
├── Common/Interfaces/
│   └── ITokenService.cs
└── Identity/Tokens/
    ├── TokenRequest.cs
    ├── TokenResponse.cs
    ├── RefreshTokenRequest.cs
    ├── GetTokenHandler.cs
    └── RefreshTokenHandler.cs

src/Infrastructure/Infrastructure/
└── Auth/
    ├── Jwt/
    │   ├── JwtSettings.cs
    │   └── TokenService.cs
    └── Startup.cs

src/Host/Host/
├── Controllers/Identity/
│   └── TokensController.cs
└── Configurations/
    └── security.json
```

### 🔑 Important Points:

1. **Security First:** Secret key >= 32 characters, HTTPS in production
2. **Token Lifetime:** Access token ngắn (60m), Refresh token dài (7d)
3. **Claims-Based:** UserId, Email, Roles, Permissions trong JWT
4. **Stateless Access:** Access token không cần DB lookup
5. **Stateful Refresh:** Refresh token validated trong DB
6. **Middleware Order:** Authentication → CurrentUser → Authorization

---

## 15. Next Steps

**Tiếp theo:** [BUILD_16 - Identity Services](BUILD_16_Identity_Services.md)

Trong bước tiếp theo, chúng ta sẽ:
1. ✅ Implement `IUserService` (CRUD users, assign roles)
2. ✅ Implement `IRoleService` (CRUD roles, manage permissions)
3. ✅ Implement `IFunctionService` (CRUD functions)
4. ✅ User DTOs (CreateUserRequest, UpdateUserRequest, UserDto)
5. ✅ Role DTOs (CreateRoleRequest, UpdateRoleRequest, RoleDto)
6. ✅ Specifications (UserByEmailSpec, RoleByNameSpec)
7. ✅ Controllers (UsersController, RolesController)
8. ✅ Testing user management endpoints

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
