# ECO.WebApi - Hướng dẫn Xây dựng Base Project

Tài liệu này hướng dẫn chi tiết từng bước xây dựng base project ECO.WebApi, giải thích **làm gì**, **tại sao**, và **cách triển khai**.

---

## Mục lục

1. [Tạo Solution và Project Structure](#1-tạo-solution-và-project-structure)
2. [Setup Build Configuration](#2-setup-build-configuration)
3. [Xây dựng Shared Layer](#3-xây-dựng-shared-layer)
4. [Xây dựng Domain Layer](#4-xây-dựng-domain-layer)
5. [Xây dựng Application Layer](#5-xây-dựng-application-layer)
6. [Xây dựng Infrastructure Layer](#6-xây-dựng-infrastructure-layer)
7. [Xây dựng Host Layer](#7-xây-dựng-host-layer)
8. [Database Initialization và Seed Data](#8-database-initialization-và-seed-data)
9. [Service Registration Pattern](#9-service-registration-pattern)

---

## 1. Tạo Solution và Project Structure

### Bước 1.1: Tạo Solution File

**Làm gì:** Tạo solution file để quản lý tất cả projects.

**Tại sao:** 
- Quản lý tập trung các projects
- Dễ build và restore packages
- IDE hỗ trợ tốt hơn

**Cách làm:**
```bash
dotnet new sln -n ECO.WebApi
```

**Kết quả:** File `ECO.WebApi.sln` được tạo.

---

### Bước 1.2: Tạo Project Structure

**Làm gì:** Tạo các projects theo Clean Architecture.

**Thứ tự tạo:**

```bash
# 1. Shared Layer (không phụ thuộc gì)
dotnet new classlib -n ECO.WebApi.Shared -o src/Core/Shared

# 2. Domain Layer (phụ thuộc Shared)
dotnet new classlib -n ECO.WebApi.Domain -o src/Core/Domain

# 3. Application Layer (phụ thuộc Domain, Shared)
dotnet new classlib -n ECO.WebApi.Application -o src/Core/Application

# 4. Infrastructure Layer (phụ thuộc Application, Domain)
dotnet new classlib -n ECO.WebApi.Infrastructure -o src/Infrastructure/Infrastructure

# 5. Host Layer (phụ thuộc Infrastructure, Application)
dotnet new webapi -n ECO.WebApi.Host -o src/Host/Host

# 6. Migrators (phụ thuộc Infrastructure, Domain)
dotnet new classlib -n Migrators.MSSQL -o src/Migrators/Migrators.MSSQL
```

**Thêm vào Solution:**
```bash
dotnet sln add src/Core/Shared/Shared.csproj
dotnet sln add src/Core/Domain/Domain.csproj
dotnet sln add src/Core/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure/Infrastructure.csproj
dotnet sln add src/Host/Host/Host.csproj
dotnet sln add src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj
```

**Lưu ý:** Thứ tự tạo quan trọng vì phải setup dependencies đúng.

---

## 2. Setup Build Configuration

### Bước 2.1: Tạo Directory.Build.props

**Làm gì:** Tạo file chứa cấu hình build chung cho tất cả projects.

**Tại sao:**
- Tránh lặp lại cấu hình
- Đảm bảo consistency
- Dễ maintain

**File:** `Directory.Build.props` (root directory)

```xml
<Project>
	<PropertyGroup>
		<AnalysisLevel>latest</AnalysisLevel>
		<AnalysisMode>All</AnalysisMode>
		<TreatWarningsAsErrors>false</TreatWarningsAsErrors>
		<CodeAnalysisTreatWarningsAsErrors>false</CodeAnalysisTreatWarningsAsErrors>
		<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference
			Include="StyleCop.Analyzers"
			Version="1.1.118"
			PrivateAssets="all"
			Condition="$(MSBuildProjectExtension) == '.csproj'"
		/>
		<PackageReference
			Include="SonarAnalyzer.CSharp"
			Version="9.7.0.75501"
			PrivateAssets="all"
			Condition="$(MSBuildProjectExtension) == '.csproj'"
		/>
	</ItemGroup>
</Project>
```

**Tác dụng:**
- Tự động áp dụng StyleCop và SonarAnalyzer cho mọi `.csproj`
- Không cần thêm vào từng project riêng

---

### Bước 2.2: Tạo Directory.Build.targets

**Làm gì:** Tạo file định nghĩa build targets chung.

**File:** `Directory.Build.targets`

```xml
<Project>
    <PropertyGroup>
        <DocumentationFile>$(OutputPath)$(AssemblyName).xml</DocumentationFile>
    </PropertyGroup>
</Project>
```

**Tác dụng:** Tự động generate XML documentation cho IntelliSense.

---

### Bước 2.3: Tạo stylecop.json

**Làm gì:** Cấu hình code style rules.

**File:** `stylecop.json` (root directory)

```json
{
  "$schema": "https://raw.githubusercontent.com/DotNetAnalyzers/StyleCopAnalyzers/master/StyleCop.Analyzers/StyleCop.Analyzers/Settings/stylecop.schema.json",
  "settings": {
    "orderingRules": {
      "systemUsingDirectivesFirst": true,
      "usingDirectivesPlacement": "outsideNamespace"
    },
    "layoutRules": {
      "newlineAtEndOfFile": "omit"
    }
  }
}
```

**Tác dụng:** Đảm bảo code style nhất quán trong toàn bộ solution.

---

## 3. Xây dựng Shared Layer

### Bước 3.1: Setup Shared Project

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

### Bước 3.2: Tạo Authorization Constants

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

## 4. Xây dựng Domain Layer

### Bước 4.1: Setup Domain Project

**Làm gì:** Tạo project chứa domain entities.

**File:** `src/Core/Domain/Domain.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net8.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<RootNamespace>ECO.WebApi.Domain</RootNamespace>
		<AssemblyName>ECO.WebApi.Domain</AssemblyName>
	</PropertyGroup>
	<ItemGroup>
		<ProjectReference Include="..\Shared\Shared.csproj" />
	</ItemGroup>
	<ItemGroup>
		<PackageReference Include="NewId" Version="4.0.1" />
		<PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.1.39" />
		<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
	</ItemGroup>
</Project>
```

**Dependencies:**
- `Shared` - Sử dụng constants và interfaces
- `NewId` - Generate unique IDs
- `Microsoft.AspNetCore.Identity` - Identity entities

---

### Bước 4.2: Tạo Identity Entities

**Làm gì:** Tạo custom Identity entities.

**File:** `src/Core/Domain/Identity/ApplicationUser.cs`

```csharp
using Microsoft.AspNetCore.Identity;

namespace ECO.WebApi.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ObjectId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
```

**File:** `src/Core/Domain/Identity/ApplicationRole.cs`

```csharp
using Microsoft.AspNetCore.Identity;

namespace ECO.WebApi.Domain.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName, string description) : base(roleName)
    {
        Description = description;
    }
    public string? Description { get; set; }
}
```

**Tại sao:** Extend Identity để thêm custom properties.

---

## 5. Xây dựng Application Layer

### Bước 5.1: Setup Application Project

**Làm gì:** Tạo project chứa application services, DTOs, handlers.

**File:** `src/Core/Application/Application.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net8.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<RootNamespace>ECO.WebApi.Application</RootNamespace>
		<AssemblyName>ECO.WebApi.Application</AssemblyName>
	</PropertyGroup>
	<ItemGroup>
		<ProjectReference Include="..\Domain\Domain.csproj" />
		<ProjectReference Include="..\Shared\Shared.csproj" />
	</ItemGroup>
	<ItemGroup>
		<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
		<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
		<PackageReference Include="Mapster" Version="7.4.0" />
		<PackageReference Include="MediatR" Version="12.4.0" />
		<PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.Localization" Version="8.0.0" />
	</ItemGroup>
</Project>
```

**Key Packages:**
- **MediatR** - CQRS pattern
- **FluentValidation** - Request validation
- **Mapster** - Object mapping (không phải AutoMapper)
- **Ardalis.Specification** - Specification pattern

---

### Bước 5.2: Tạo Application Startup

**Làm gì:** Đăng ký MediatR và FluentValidation.

**File:** `src/Core/Application/Startup.cs`

```csharp
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Application;
public static class Startup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services
            .AddValidatorsFromAssembly(assembly)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    }
}
```

**Tại sao:**
- Tự động scan và đăng ký tất cả validators
- Tự động scan và đăng ký tất cả MediatR handlers
- Không cần đăng ký từng cái một

**Cách hoạt động:**
1. `AddValidatorsFromAssembly()` - Tìm tất cả class kế thừa `AbstractValidator<T>`
2. `AddMediatR()` - Tìm tất cả class implement `IRequestHandler<TRequest, TResponse>`

---

## 6. Xây dựng Infrastructure Layer

### Bước 6.1: Setup Infrastructure Project

**Làm gì:** Tạo project chứa implementations.

**File:** `src/Infrastructure/Infrastructure/Infrastructure.csproj`

Thêm references:
```xml
<ItemGroup>
	<ProjectReference Include="..\..\Core\Application\Application.csproj" />
	<ProjectReference Include="..\..\Core\Domain\Domain.csproj" />
</ItemGroup>
```

**Thêm packages:** EF Core, Hangfire, Serilog, SignalR, MailKit, etc. (xem SETUP_GUIDE.md section 9.1)

---

### Bước 6.2: Tạo Infrastructure Startup

**Làm gì:** Tạo main startup method để đăng ký tất cả services.

**File:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
namespace ECO.WebApi.Infrastructure;
public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        MapsterSettings.Configure();
        return services
            .AddAuth(config)
            .AddGoogleDrive(config)
            .AddBackgroundJobs(config)
            .AddCaching(config)
            .AddCorsPolicy()
            .AddExceptionMiddleware()
            .AddBehaviours()
            .AddMailing(config)
            .AddNotifications(config)
            .AddPersistence()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices(); 
    }
}
```

**Tại sao dùng Modular Startup Pattern:**
- Mỗi module tự quản lý services của mình
- Dễ maintain và test
- Có thể enable/disable modules dễ dàng

**Thứ tự quan trọng:**
1. `MapsterSettings.Configure()` - Phải gọi trước để config mapping
2. `AddPersistence()` - Phải gọi trước `AddAuth()` vì Identity cần DbContext
3. `AddServices()` - Gọi cuối để đăng ký application services

---

## 7. Xây dựng Host Layer

### Bước 7.1: Setup Host Project

**Làm gì:** Tạo ASP.NET Core Web API project.

**File:** `src/Host/Host/Host.csproj`

Thêm references:
```xml
<ItemGroup>
	<ProjectReference Include="..\..\Core\Application\Application.csproj" />
	<ProjectReference Include="..\..\Infrastructure\Infrastructure\Infrastructure.csproj" />
	<ProjectReference Include="..\..\Migrators\Migrators.MSSQL\Migrators.MSSQL.csproj" />
</ItemGroup>
```

**Thêm packages:**
- `Swashbuckle.AspNetCore` - Swagger
- `FluentValidation.AspNetCore` - FluentValidation integration
- `Serilog.AspNetCore` - Logging
- `Microsoft.EntityFrameworkCore.Design` - EF Core tools

---

### Bước 7.2: Tạo Program.cs

**Làm gì:** Setup application entry point.

**File:** `src/Host/Host/Program.cs`

```csharp
using ECO.WebApi.Infrastructure;
using ECO.WebApi.Host.Configurations;
using ECO.WebApi.Application;
using Serilog;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Load configurations
    builder.AddConfigurations().RegisterSerilog();
    
    // 2. Add services
    builder.Services.AddControllers();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    
    // 3. Add Swagger
    builder.Services.AddSwaggerGen(/* ... */);

    var app = builder.Build();
    
    // 4. Initialize database
    await app.Services.InitializeDatabasesAsync();
    
    // 5. Configure middleware
    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
```

**Thứ tự quan trọng:**
1. Load configurations trước
2. Add services (Infrastructure → Application)
3. Build app
4. Initialize database (phải sau khi build)
5. Configure middleware pipeline

---

## 8. Database Initialization và Seed Data

### Bước 8.1: Tạo Interface IDatabaseInitializer

**Làm gì:** Tạo interface để abstract database initialization.

**Tại sao:** 
- Dễ test
- Có thể swap implementation
- Follow Dependency Inversion Principle

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/IDatabaseInitializer.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Persistence.Initialization;
internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
}
```

---

### Bước 8.2: Tạo DatabaseInitializer

**Làm gì:** Implementation chính, tạo scope và gọi ApplicationDbInitializer.

**Tại sao tạo scope:**
- `ApplicationDbInitializer` cần scoped services (DbContext)
- Phải tạo scope riêng vì được gọi từ root service provider

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/DatabaseInitializer.cs`

```csharp
using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly ApplicationDbContext _context;
    
    public DatabaseInitializer(
        ApplicationDbContext context, 
        IServiceProvider serviceProvider, 
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        // Tạo scope mới để lấy scoped services
        using var scope = _serviceProvider.CreateScope();

        // Gọi ApplicationDbInitializer trong scope mới
        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(cancellationToken);
    }
}
```

**Lưu ý:** Phải tạo scope vì `ApplicationDbInitializer` cần `ApplicationDbContext` (scoped).

---

### Bước 8.3: Tạo ApplicationDbInitializer

**Làm gì:** Class chịu trách nhiệm apply migrations và seed data.

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/ApplicationDbInitializer.cs`

```csharp
using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ApplicationDbInitializer> _logger;
    private readonly ApplicationDbSeeder _dbSeeder;

    public ApplicationDbInitializer(
        ApplicationDbContext dbContext, 
        ILogger<ApplicationDbInitializer> logger, 
        ApplicationDbSeeder dbSeeder)
    {
        _dbContext = dbContext;
        _logger = logger;
        _dbSeeder = dbSeeder;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // 1. Kiểm tra có migrations không
        if (_dbContext.Database.GetMigrations().Any())
        {
            // 2. Apply pending migrations
            if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                _logger.LogInformation("Applying Migrations for system");
                await _dbContext.Database.MigrateAsync(cancellationToken);
            }

            // 3. Kiểm tra connection và seed data
            if (await _dbContext.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogInformation("Connection to system's Database Succeeded.");
                await _dbSeeder.SeedDatabaseAsync(_dbContext, cancellationToken);
            }
        }
    }
}
```

**Thứ tự thực hiện:**
1. Check migrations exist
2. Apply pending migrations
3. Check connection
4. Seed data

**Tại sao check migrations trước:** Tránh lỗi nếu chưa có migrations.

---

### Bước 8.4: Tạo ApplicationDbSeeder

**Làm gì:** Class seed dữ liệu cơ bản: Actions, Functions, Roles, Admin User.

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/ApplicationDbSeeder.cs`

```csharp
using System.Reflection;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(
        RoleManager<ApplicationRole> roleManager, 
        UserManager<ApplicationUser> userManager, 
        CustomSeederRunner seederRunner, 
        ILogger<ApplicationDbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        // Thứ tự seed quan trọng!
        await SeedActionsAndFunctionsAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }
}
```

**Thứ tự seed:**
1. **Actions và Functions** - Phải seed trước vì Roles cần chúng
2. **Roles** - Phải seed trước vì Admin User cần role
3. **Admin User** - Seed user và assign role
4. **Custom Seeders** - Chạy các seeders tùy chỉnh

---

### Bước 8.5: Seed Actions và Functions

**Làm gì:** Seed Actions và Functions từ constants trong `ECOAction` và `ECOFunction`.

**Code trong ApplicationDbSeeder:**

```csharp
private async Task SeedActionsAndFunctionsAsync(ApplicationDbContext dbContext)
{
    // 1. Seed Actions từ ECOAction constants
    var actions = typeof(ECOAction)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(field => field.IsLiteral && !field.IsInitOnly) // Chỉ lấy constants
        .Select(field => field.GetValue(null)?.ToString())
        .Where(value => value != null)
        .ToList();

    foreach (var action in actions)
    {
        if (!await dbContext.Actions.AnyAsync(x => x.Name == action))
        {
            _logger.LogInformation($"Seeding action {action}.");
            dbContext.Actions.Add(new Domain.Identity.Action { Name = action });
            await dbContext.SaveChangesAsync();
        }
    }

    // 2. Seed Functions từ ECOFunction constants
    var functions = typeof(ECOFunction)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(field => field.IsLiteral && !field.IsInitOnly)
        .Select(field => field.GetValue(null)?.ToString())
        .Where(value => value != null)
        .ToList();

    foreach (var functionName in functions)
    {
        if (!await dbContext.Functions.AnyAsync(f => f.Name == functionName))
        {
            _logger.LogInformation($"Seeding function {functionName}.");
            dbContext.Functions.Add(new Function { Name = functionName });
            await dbContext.SaveChangesAsync();
        }
    }

    // 3. Seed ActionInFunction (many-to-many)
    foreach (var functionName in functions)
    {
        var function = await dbContext.Functions.SingleAsync(f => f.Name == functionName);
        foreach (var actionName in actions)
        {
            var action = await dbContext.Actions.SingleAsync(a => a.Name == actionName);
            if (!await dbContext.ActionInFunctions.AnyAsync(
                aif => aif.FunctionId == function.Id && aif.ActionId == action.Id))
            {
                _logger.LogInformation($"Seeding action {actionName} in function {functionName}.");
                dbContext.ActionInFunctions.Add(new ActionInFunction(action.Id, function.Id));
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
```

**Tại sao dùng Reflection:**
- Tự động lấy tất cả constants từ `ECOAction` và `ECOFunction`
- Không cần hard-code từng action/function
- Dễ maintain: thêm constant mới → tự động seed

**Tại sao check `AnyAsync()` trước:**
- Tránh duplicate data
- Idempotent: có thể chạy nhiều lần an toàn

---

### Bước 8.6: Seed Roles

**Làm gì:** Seed roles và assign permissions.

**Code trong ApplicationDbSeeder:**

```csharp
private async Task SeedRolesAsync(ApplicationDbContext dbContext)
{
    foreach (string roleName in ECORoles.DefaultRoles)
    {
        // 1. Tạo role nếu chưa tồn tại
        if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
            is not ApplicationRole role)
        {
            _logger.LogInformation("Seeding {role} Role for system.", roleName);
            role = new ApplicationRole(roleName, $"{roleName} Role ");
            await _roleManager.CreateAsync(role);
        }

        // 2. Assign permissions cho role
        if (roleName == ECORoles.Basic)
        {
            await AssignPermissionsToRoleAsync(dbContext, role, isBasic: true);
        }
        else if (roleName == ECORoles.Admin)
        {
            await AssignPermissionsToRoleAsync(dbContext, role, isBasic: false);
        }
    }
}

private async Task AssignPermissionsToRoleAsync(
    ApplicationDbContext dbContext, 
    ApplicationRole role, 
    bool isBasic)
{
    var currentPermissions = await dbContext.Permissions
        .Where(x => x.RoleId == role.Id)
        .ToListAsync();

    var functions = await dbContext.Functions.ToListAsync();
    
    foreach (var function in functions)
    {
        var actionsInFunction = await dbContext.ActionInFunctions
            .Include(x => x.Action)
            .Where(a => a.FunctionId == function.Id)
            .ToListAsync();
            
        foreach (var actionInFunction in actionsInFunction)
        {
            // Basic role chỉ có một số permissions cơ bản
            if (isBasic && !IsBasicPermission(actionInFunction.Action.Name, function.Name))
            {
                continue;
            }

            var permissionName = $"{function.Name}.{actionInFunction.Action.Name}";

            // Chỉ thêm nếu chưa tồn tại
            if (!currentPermissions.Any(p => 
                p.FunctionId == function.Id && p.ActionId == actionInFunction.ActionId))
            {
                _logger.LogInformation("Seeding {role} Permission '{permissionName}'.", 
                    role.Name, permissionName);
                dbContext.Permissions.Add(
                    new Permission(role.Id, function.Id, actionInFunction.ActionId));
            }
        }
    }

    await dbContext.SaveChangesAsync();
}

private bool IsBasicPermission(string actionName, string functionName)
{
    var basicPermissions = new List<string>
    {
        $"{ECOAction.View}.{ECOFunction.Dashboard}",
        $"{ECOAction.View}.{ECOFunction.Category}",
        $"{ECOAction.Search}.{ECOFunction.Category}",
        $"{ECOAction.View}.{ECOFunction.Product}",
        $"{ECOAction.Search}.{ECOFunction.Product}"
    };

    return basicPermissions.Contains($"{actionName}.{functionName}");
}
```

**Tại sao:**
- **Basic role** chỉ có quyền xem và search (read-only)
- **Admin role** có tất cả permissions
- Check `currentPermissions` để tránh duplicate
- Permission format: `{Function}.{Action}` (ví dụ: `Product.View`)

**Tác dụng:**
- Tự động assign permissions khi tạo role
- Dễ mở rộng: thêm function mới → tự động có permissions cho Admin
- Basic role chỉ có quyền cơ bản (bảo mật tốt hơn)

---

### Bước 8.7: Seed Admin User

**Làm gì:** Tạo admin user mặc định.

**Code trong ApplicationDbSeeder:**

```csharp
private async Task SeedAdminUserAsync()
{
    // 1. Kiểm tra admin user đã tồn tại chưa
    if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == "admin@gmail.com")
        is not ApplicationUser adminUser)
    {
        string adminUserName = $"System.{ECORoles.Admin}".ToLowerInvariant();
        adminUser = new ApplicationUser
        {
            FirstName = "NuocFTraiK",
            LastName = ECORoles.Admin,
            Email = "admin@gmail.com",
            UserName = adminUserName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            NormalizedEmail = "ADMIN@GMAIL.COM",
            NormalizedUserName = adminUserName.ToUpperInvariant(),
            IsActive = true
        };

        _logger.LogInformation("Seeding Default Admin User for application");
        
        // 2. Hash password
        var password = new PasswordHasher<ApplicationUser>();
        adminUser.PasswordHash = password.HashPassword(adminUser, "Abcd@1234");
        
        // 3. Tạo user
        await _userManager.CreateAsync(adminUser);
    }

    // 4. Assign Admin role nếu chưa có
    if (!await _userManager.IsInRoleAsync(adminUser, ECORoles.Admin))
    {
        _logger.LogInformation("Assigning Admin Role to Admin User");
        await _userManager.AddToRoleAsync(adminUser, ECORoles.Admin);
    }
}
```

**Thông tin Admin mặc định:**
- Email: `admin@gmail.com`
- Username: `system.admin`
- Password: `Abcd@1234` (nên đổi sau khi deploy)
- Role: `Admin`

**Tại sao:**
- Cần admin user để đăng nhập lần đầu
- EmailConfirmed = true để không cần verify email
- IsActive = true để có thể đăng nhập ngay

**Lưu ý:** Đổi password trong production!

---

### Bước 8.8: Tạo ICustomSeeder Interface

**Làm gì:** Tạo interface để cho phép custom seeders.

**Tại sao:**
- Cho phép mỗi module tự seed data của mình
- Tách biệt concerns
- Dễ mở rộng

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/ICustomSeeder.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
```

**Tác dụng:** Bất kỳ class nào implement interface này sẽ tự động được gọi khi seed data.

---

### Bước 8.9: Tạo CustomSeederRunner

**Làm gì:** Class chạy tất cả custom seeders.

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/CustomSeederRunner.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}
```

**Cách hoạt động:**
1. Constructor nhận `IServiceProvider`
2. `GetServices<ICustomSeeder>()` - Lấy tất cả implementations của `ICustomSeeder`
3. Chạy từng seeder tuần tự

**Tại sao dùng `GetServices()`:**
- Có thể có nhiều custom seeders
- Tự động discover tất cả implementations
- Không cần đăng ký từng cái một

---

### Bước 8.10: Ví dụ Custom Seeder - NotificationSeeder

**Làm gì:** Ví dụ cách tạo custom seeder.

**File:** `src/Infrastructure/Infrastructure/Notifications/NotificationSeeder.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Notifications;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ECO.WebApi.Infrastructure.Notifications;

public class NotificationSeeder : ICustomSeeder
{
    private readonly ISerializerService _serializerService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationSeeder> _logger;

    public NotificationSeeder(
        ISerializerService serializerService, 
        ApplicationDbContext db, 
        ILogger<NotificationSeeder> logger)
    {
        _serializerService = serializerService;
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // 1. Đọc file JSON chứa notification data
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string dataPath = Path.Combine(path!, "Notifications", "notificationData.json");
        
        // 2. Chỉ seed nếu chưa có data
        if (!_db.Notifications.Any())
        {
            _logger.LogInformation("Started to Seed Notifications."); 
            
            // 3. Đọc và deserialize JSON
            string notificationData = await File.ReadAllTextAsync(dataPath, cancellationToken);
            var notifications = _serializerService.Deserialize<List<Notification>>(notificationData);
            
            // 4. Lấy admin user để assign notifications
            var user = await _db.Users
                .Where(u => u.UserName == "system.admin")
                .FirstOrDefaultAsync();
                
            // 5. Assign receiver và add vào database
            foreach (var notification in notifications)
            {
                notification.ReceiverId = user.Id;
                _db.Notifications.Add(notification);
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded Notifications.");
        }
    }
}
```

**Cách sử dụng:**
1. Implement `ICustomSeeder`
2. Đăng ký trong DI (sẽ tự động nếu dùng `AddServices()`)
3. Seeder sẽ tự động chạy khi `ApplicationDbSeeder.SeedDatabaseAsync()` được gọi

**Tác dụng:**
- Mỗi module tự quản lý seed data của mình
- Có thể seed từ JSON, CSV, hoặc code
- Tự động chạy khi app start

---

### Bước 8.11: Đăng ký Services trong Persistence Startup

**Làm gì:** Đăng ký tất cả initialization services.

**File:** `src/Infrastructure/Infrastructure/Persistence/Startup.cs`

**Code:**

```csharp
internal static IServiceCollection AddPersistence(this IServiceCollection services)
{
    // ... DbContext setup ...
    
    return services
        .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
        .AddTransient<ApplicationDbInitializer>()
        .AddTransient<ApplicationDbSeeder>()
        .AddTransient<CustomSeederRunner>()
        .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient) // Đăng ký tất cả custom seeders
        // ... other services ...
        .AddRepositories();
}
```

**Giải thích:**
- `AddTransient<IDatabaseInitializer, DatabaseInitializer>()` - Đăng ký main initializer
- `AddTransient<ApplicationDbInitializer>()` - Đăng ký application initializer
- `AddTransient<ApplicationDbSeeder>()` - Đăng ký seeder
- `AddTransient<CustomSeederRunner>()` - Đăng ký runner
- `AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)` - **Tự động đăng ký tất cả custom seeders**

**Tại sao dùng `AddServices()`:**
- Tự động scan và đăng ký tất cả implementations của `ICustomSeeder`
- Không cần đăng ký từng seeder một
- Dễ mở rộng: thêm seeder mới → tự động được đăng ký

---

### Bước 8.12: Gọi InitializeDatabasesAsync trong Program.cs

**Làm gì:** Gọi database initialization khi app start.

**File:** `src/Host/Host/Program.cs`

**Code:**

```csharp
var app = builder.Build();

// Initialize database (phải sau khi Build)
await app.Services.InitializeDatabasesAsync();

app.UseInfrastructure(builder.Configuration);
app.MapEndpoints();
app.Run();
```

**Extension method:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
public static async Task InitializeDatabasesAsync(
    this IServiceProvider services, 
    CancellationToken cancellationToken = default)
{
    // Tạo scope mới để lấy scoped services
    using var scope = services.CreateScope();

    // Gọi IDatabaseInitializer
    await scope.ServiceProvider
        .GetRequiredService<IDatabaseInitializer>()
        .InitializeDatabasesAsync(cancellationToken);
}
```

**Tại sao tạo scope:**
- `IDatabaseInitializer` cần `ApplicationDbContext` (scoped)
- Root service provider không có scoped services
- Phải tạo scope riêng

**Thứ tự:**
1. Build app → Service provider sẵn sàng
2. Initialize database → Apply migrations + Seed data
3. Configure middleware → App sẵn sàng nhận requests

**Tác dụng:**
- Tự động apply migrations khi app start
- Tự động seed data nếu chưa có
- Không cần chạy migration commands thủ công (lần đầu)

---

## 9. Service Registration Pattern

### Bước 9.1: Tạo Interface Markers

**Làm gì:** Tạo interfaces để đánh dấu service lifetime.

**Tại sao:**
- Tự động đăng ký services dựa trên interface
- Không cần đăng ký từng service một
- Dễ maintain

**File 1:** `src/Core/Application/Common/Interfaces/ITransientService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Interfaces;

public interface ITransientService
{
}
```

**File 2:** `src/Core/Application/Common/Interfaces/IScopedService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Interfaces;

public interface IScopedService
{
}
```

**Tác dụng:** Marker interfaces - chỉ dùng để đánh dấu, không có methods.

---

### Bước 9.2: Tạo AddServices Extension Method

**Làm gì:** Tạo method tự động đăng ký services.

**File:** `src/Infrastructure/Infrastructure/Common/Startup.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Common;

internal static class Startup
{
    // Đăng ký tất cả services implement ITransientService và IScopedService
    internal static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped);

    // Method chính: scan và đăng ký services
    internal static IServiceCollection AddServices(
        this IServiceCollection services, 
        Type interfaceType, 
        ServiceLifetime lifetime)
    {
        // 1. Lấy tất cả assemblies trong AppDomain
        var interfaceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes()) // Lấy tất cả types
            .Where(t => interfaceType.IsAssignableFrom(t) // Implement interfaceType
                        && t.IsClass && !t.IsAbstract) // Là class, không abstract
            .Select(t => new
            {
                Service = t.GetInterfaces().FirstOrDefault(), // Interface đầu tiên
                Implementation = t // Class implementation
            })
            .Where(t => t.Service is not null 
                        && interfaceType.IsAssignableFrom(t.Service));

        // 2. Đăng ký từng service
        foreach (var type in interfaceTypes)
        {
            services.AddService(type.Service!, type.Implementation, lifetime);
        }

        return services;
    }

    // Helper: đăng ký service với lifetime cụ thể
    internal static IServiceCollection AddService(
        this IServiceCollection services, 
        Type serviceType, 
        Type implementationType, 
        ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            _ => throw new ArgumentException("Invalid lifeTime", nameof(lifetime))
        };
}
```

**Cách hoạt động:**
1. Scan tất cả assemblies trong AppDomain
2. Tìm tất cả classes implement `ITransientService` hoặc `IScopedService`
3. Lấy interface đầu tiên mà class implement (làm service type)
4. Đăng ký với lifetime tương ứng

**Ví dụ:**
```csharp
// Service class
public class ProductService : IProductService, ITransientService
{
    // ...
}

// Tự động đăng ký: services.AddTransient<IProductService, ProductService>();
```

**Tại sao:**
- Không cần đăng ký từng service trong Startup
- Chỉ cần implement marker interface
- Tự động discover và register

**Lưu ý:**
- Service class phải implement cả business interface VÀ marker interface
- Ví dụ: `IProductService` (business) + `ITransientService` (marker)

---

### Bước 9.3: Sử dụng trong Infrastructure Startup

**File:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, 
    IConfiguration config)
{
    MapsterSettings.Configure();
    return services
        // ... other modules ...
        .AddServices(); // Đăng ký tất cả services tự động
}
```

**Tác dụng:**
- Tự động đăng ký tất cả services implement `ITransientService` và `IScopedService`
- Không cần đăng ký từng service một
- Dễ mở rộng: thêm service mới → tự động được đăng ký

---

## Tóm tắt

### Thứ tự xây dựng:

1. **Solution & Build Config** → Tạo solution, setup build tools
2. **Shared Layer** → Constants, interfaces cơ bản
3. **Domain Layer** → Entities, business logic
4. **Application Layer** → DTOs, handlers, validators
5. **Infrastructure Layer** → Implementations, DbContext, services
6. **Host Layer** → Controllers, Program.cs, configurations
7. **Database Initialization** → Migrations, seed data
8. **Service Registration** → Auto-register services

### Điểm quan trọng:

- **Thứ tự dependencies:** Shared → Domain → Application → Infrastructure → Host
- **Database initialization:** Phải sau khi Build app
- **Seed data thứ tự:** Actions/Functions → Roles → Admin User → Custom Seeders
- **Service registration:** Dùng marker interfaces để auto-register

### Lợi ích:

- **Clean Architecture:** Tách biệt concerns, dễ test và maintain
- **Auto-registration:** Không cần đăng ký services thủ công
- **Idempotent seeding:** Có thể chạy nhiều lần an toàn
- **Modular:** Mỗi module tự quản lý services của mình

---

*Tài liệu này hướng dẫn từng bước cụ thể để xây dựng base project. Mỗi bước đều giải thích **làm gì**, **tại sao**, và **cách triển khai**.*