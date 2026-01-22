# Xây dựng Shared Layer

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn xây dựng Shared Layer - layer cơ bản nhất, không phụ thuộc vào bất kỳ layer nào.

---

## Bước 2.1: Setup Shared Project

**Làm gì:** Tạo project cơ bản nhất, không phụ thuộc gì.

**File:** `src/Core/Shared/Shared.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net8.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<RootNamespace>ECO.WebApi.Shared</RootNamespace>
		<AssemblyName>ECO.WebApi.Shared</AssemblyName>
	</PropertyGroup>
</Project>
```

**Lưu ý:** Không có package dependencies - đây là layer cơ bản nhất.

---

## Bước 2.2: Tạo Authorization Constants

**Làm gì:** Định nghĩa constants cho Actions, Functions, Roles, Claims.

**Tại sao:**
- Tránh magic strings
- Type-safe
- Dễ maintain và refactor

**File 1:** `src/Core/Shared/Authorization/ECOAction.cs`

```csharp
namespace ECO.WebApi.Shared.Authorization;

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

**File 2:** `src/Core/Shared/Authorization/ECOFunction.cs`

```csharp
namespace ECO.WebApi.Shared.Authorization;

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

**File 3:** `src/Core/Shared/Authorization/ECORoles.cs`

```csharp
using System.Collections.ObjectModel;

namespace ECO.WebApi.Shared.Authorization;

public static class ECORoles
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Basic
    });

    public static bool IsDefault(string roleName) => DefaultRoles.Any(r => r == roleName);
}
```

**File 4:** `src/Core/Shared/Authorization/ECOClaims.cs`

```csharp
namespace ECO.WebApi.Shared.Authorization;

public static class ECOClaims
{
    public const string Fullname = "fullName";
    public const string Permission = "permission";
    public const string ImageUrl = "image_url";
    public const string IpAddress = "ipAddress";
    public const string Expiration = "exp";
}
```

**File 5:** `src/Core/Shared/Authorization/ECOPermissions.cs`

```csharp
using System.Reflection;

namespace ECO.WebApi.Shared.Authorization;

public record ECOPermission(string action, string function)
{
    public string Name => NameFor(action, function);
    public static string NameFor(string action, string function) => $"Permissions.{function}.{action}";
    
    public static List<string> GeneratePermissionsForFunction(string function)
    {
        var actions = typeof(ECOAction)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && !field.IsInitOnly)
            .Select(field => field.GetValue(null)?.ToString())
            .Where(value => value != null)
            .ToList();

        return actions.Select(action => $"Permissions.{function}.{action}").ToList();
    }
}
```

**Tác dụng:**
- Sử dụng `nameof()` để tránh lỗi typo
- Dễ thêm/sửa permissions
- Generate permissions động từ Actions và Functions

---

**Tiếp theo:** [Xây dựng Domain Layer](BUILD_03_Domain_Layer.md)
