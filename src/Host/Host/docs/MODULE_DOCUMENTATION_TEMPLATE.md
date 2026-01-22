# Template cho Module Documentation

> ?? **M?c ?ích:** Template này giúp vi?t documentation nh?t quán cho m?i module/feature trong solution.

---

## ?? C?u trúc chu?n cho m?i Module Doc

### **Header Section**
```markdown
# [Module Name] - [Short Description]

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** [B??c tr??c ?ó ph?i hoàn thành]

Tài li?u này h??ng d?n xây d?ng [Module Name] - [Purpose].

---
```

### **Section 1: Overview (T?ng quan)**
```markdown
## 1. Overview

**Làm gì:** [Mô t? ng?n g?n module này làm gì]

**T?i sao:** 
- [Lý do 1]
- [Lý do 2]
- [Lý do 3]

**Thành ph?n chính:**
- ? [Component 1]
- ? [Component 2]
- ? [Component 3]

---
```

### **Section 2: Dependencies (Ph? thu?c)**
```markdown
## 2. Dependencies và Packages

**Project Dependencies:**
```xml
<ItemGroup>
  <ProjectReference Include="path/to/Project.csproj" />
</ItemGroup>
```

**NuGet Packages:**
```xml
<ItemGroup>
  <PackageReference Include="PackageName" Version="x.x.x" />
</ItemGroup>
```

**T?i sao c?n packages này:**
- `PackageName`: [M?c ?ích s? d?ng]

---
```

### **Section 3: Step-by-Step Implementation**
```markdown
## 3. Implementation Steps

### B??c 3.1: [First Step Name]

**Làm gì:** [Mô t? ng?n g?n]

**T?i sao:** [Gi?i thích lý do]

**File:** `path/to/File.cs`

```csharp
// Code example v?i comments gi?i thích
public class Example
{
// Comment gi?i thích
    public void Method()
    {
  // Implementation
    }
}
```

**Gi?i thích code:**
- Line 1-3: [Gi?i thích]
- Line 4-6: [Gi?i thích]

**L?u ý:**
- ?? [L?u ý quan tr?ng 1]
- ?? [L?u ý thông th??ng]

---

### B??c 3.2: [Second Step Name]

[L?p l?i c?u trúc t??ng t?]

---
```

### **Section 4: Testing và Verification**
```markdown
## 4. Testing và Verification

### 4.1. Unit Tests (n?u có)

```csharp
[Fact]
public void TestMethodName()
{
    // Arrange
    // Act
    // Assert
}
```

### 4.2. Manual Testing

**Test Case 1: [Scenario Name]**
```bash
# Command ho?c API call
curl -X POST http://localhost:5000/api/endpoint \
  -H "Content-Type: application/json" \
  -d '{"key": "value"}'
```

**Expected Result:**
```json
{
  "success": true,
  "data": {...}
}
```

---
```

### **Section 5: Common Issues và Troubleshooting**
```markdown
## 5. Common Issues

### Issue 1: [Error Message]

**Tri?u ch?ng:**
```
[Error stack trace ho?c log]
```

**Nguyên nhân:**
[Gi?i thích nguyên nhân]

**Gi?i pháp:**
```csharp
// Fix code
```

---
```

### **Section 6: Best Practices**
```markdown
## 6. Best Practices

### ? Do's (Nên làm)
- ? [Practice 1]
- ? [Practice 2]

### ? Don'ts (Không nên làm)
- ? [Anti-pattern 1]
- ? [Anti-pattern 2]

### ?? Tips
- ?? [Tip 1]
- ?? [Tip 2]

---
```

### **Section 7: Related Resources**
```markdown
## 7. Related Resources

**Internal Docs:**
- [Related Doc 1](link)
- [Related Doc 2](link)

**External Resources:**
- [Official Documentation](link)
- [Tutorial](link)
- [GitHub Repository](link)

---
```

### **Footer Section**
```markdown
---

**Ti?p theo:** [Next Module](BUILD_XX_Next_Module.md)

**Quay l?i:** [M?c l?c](BUILD_INDEX.md)
```

---

## ?? Ví d? c? th?: BUILD_14_Authentication_Module.md

### **Áp d?ng template:**

```markdown
# Authentication Module - JWT và OAuth2

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** B??c 13 (Application Services) hoàn thành

Tài li?u này h??ng d?n xây d?ng Authentication Module v?i JWT và OAuth2 (Google, Facebook).

---

## 1. Overview

**Làm gì:** Xây d?ng h? th?ng authentication v?i JWT tokens và OAuth2 providers.

**T?i sao:** 
- B?o m?t API endpoints
- H? tr? multiple authentication methods
- Stateless authentication v?i JWT
- Social login ?? UX t?t h?n

**Thành ph?n chính:**
- ? JWT Token Service
- ? Refresh Token mechanism
- ? Google OAuth2
- ? Facebook OAuth2
- ? Permission-based Authorization

---

## 2. Dependencies và Packages

**NuGet Packages:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="8.0.10" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.Facebook" Version="8.0.10" />
</ItemGroup>
```

**T?i sao c?n packages này:**
- `JwtBearer`: Xác th?c JWT tokens
- `Authentication.Google`: Google OAuth2 integration
- `Authentication.Facebook`: Facebook OAuth2 integration

---

## 3. Implementation Steps

### B??c 3.1: Setup JWT Settings

**File:** `src/Infrastructure/Infrastructure/Auth/Jwt/JwtSettings.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Auth.Jwt;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int TokenExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
```

**Configuration File:** `src/Host/Host/Configurations/security.json`

```json
{
  "SecuritySettings": {
    "Key": "your-super-secret-key-at-least-32-characters-long!",
    "Issuer": "ECO.WebApi",
    "Audience": "ECO.WebApi",
    "TokenExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

---

### B??c 3.2: Configure JWT Authentication

**File:** `src/Infrastructure/Infrastructure/Auth/Jwt/Startup.cs`

```csharp
public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services, 
    IConfiguration config)
{
    var jwtSettings = config.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
    
    services
        .AddAuthentication(options =>
        {
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
         ValidateIssuer = true,
    ValidateAudience = true,
     ValidateLifetime = true,
     ValidateIssuerSigningKey = true,
       ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
              IssuerSigningKey = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
    });

    return services;
}
```

---

[... Continue v?i các b??c ti?p theo ...]

## 4. Testing

### 4.1. Test Login

```bash
curl -X POST https://localhost:7001/api/tokens \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@root.com",
    "password": "123Pa$$word!"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "xxx-xxx-xxx",
  "refreshTokenExpiryTime": "2024-01-08T00:00:00Z"
}
```

---

## 5. Common Issues

### Issue 1: "IDX10503: Signature validation failed"

**Nguyên nhân:** JWT Key không kh?p ho?c quá ng?n.

**Gi?i pháp:**
```json
{
  "SecuritySettings": {
    "Key": "must-be-at-least-32-characters-long!"
  }
}
```

---

## 6. Best Practices

### ? Do's
- ? S? d?ng HTTPS trong production
- ? Store JWT key trong Environment Variables
- ? Set appropriate token expiration times
- ? Implement refresh token rotation

### ? Don'ts
- ? Commit JWT keys vào Git
- ? S? d?ng weak keys (< 32 characters)
- ? Store sensitive data trong JWT claims
- ? Set quá dài token expiration

---

**Ti?p theo:** [Authorization Module](BUILD_15_Authorization_Module.md)
```

---

## ?? Checklist khi vi?t Module Doc

### Content Checklist
- [ ] Header v?i link quay l?i index
- [ ] Overview gi?i thích "làm gì" và "t?i sao"
- [ ] List dependencies và packages ??y ??
- [ ] Steps có th? t? logic rõ ràng
- [ ] Code examples có comments gi?i thích
- [ ] Testing instructions c? th?
- [ ] Common issues và solutions
- [ ] Best practices và anti-patterns
- [ ] Links ??n related resources

### Quality Checklist
- [ ] Code examples compile ???c
- [ ] Commands test thành công
- [ ] Không có typos
- [ ] Formatting nh?t quán
- [ ] Screenshots (n?u c?n) rõ ràng
- [ ] Cross-references ?úng

### Style Checklist
- [ ] S? d?ng emojis phù h?p (? ? ?? ?? ?? ??)
- [ ] Code blocks có syntax highlighting
- [ ] Sections có separators (---)
- [ ] Lists có indentation ?úng
- [ ] Headers có hierarchy rõ ràng (##, ###, ####)

---

## ?? Naming Convention

### File Names
```
BUILD_[Number]_[Module_Name].md

Examples:
- BUILD_14_Authentication.md
- BUILD_15_Authorization.md
- BUILD_16_Caching_Strategy.md
- BUILD_17_Email_Service.md
```

### Section Numbers
```
1. Overview
2. Dependencies
3. Implementation Steps
   3.1. First Step
   3.2. Second Step
4. Testing
5. Common Issues
6. Best Practices
7. Related Resources
```

---

## ?? Formatting Guidelines

### Code Blocks
```markdown
```csharp
// C# code with syntax highlighting
public class Example { }
\```

```bash
# Bash commands
dotnet run
\```

```json
// JSON configuration
{ "key": "value" }
\```
```

### Tables
```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Value 1  | Value 2  | Value 3  |
```

### Callouts
```markdown
> ?? **Warning:** Critical information  
> ?? **Note:** Helpful information  
> ?? **Tip:** Pro tip  
> ?? **Prerequisites:** Required steps
```

---

## ?? Cross-Referencing

### Internal Links
```markdown
[Link Text](BUILD_01_Solution_Setup.md)
[Specific Section](BUILD_01_Solution_Setup.md#21-create-directorybuilds-props)
```

### External Links
```markdown
[External Resource](https://example.com)
```

---

**S? d?ng template này ?? vi?t documentation nh?t quán và ch?t l??ng cao!**
