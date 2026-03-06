# BUILD_08 - Database Initialization

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** BUILD_07 (Logging Setup) đã hoàn thành  
> ⏱️ **Thời gian:** Khoảng 25 phút

Tài liệu này hướng dẫn setup database initialization, migrations, và seeding data.

---

## 1. Overview

**Làm gì:** Setup auto-migration và seed initial data khi application khởi động.

**Tại sao cần:**
- **Auto-migration:** Tự động apply migrations khi deploy
- **Initial data:** Seed Actions, Functions, Roles, Admin User
- **Idempotent:** Chạy nhiều lần không gây duplicate
- **Zero-config:** Không cần chạy commands thủ công

**Trong bước này chúng ta sẽ:**
- ✅ Setup Migrators.MSSQL project
- ✅ Tạo MigrationDbContextFactory (Design-time)
- ✅ Tạo Custom Seeder Pattern
- ✅ Tạo ApplicationDbSeeder
- ✅ Tạo DatabaseInitializer
- ✅ Run first migration

---

## 2. Setup Migrators Project

**File:** `src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>{ProjectName}.Migrators.MSSQL</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
<ProjectReference Include="..\..\Infrastructure\Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
   <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

---

**File:** `src/Migrators/Migrators.MSSQL/MigrationDbContextFactory.cs`

```csharp
using {ProjectName}.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace {ProjectName}.Migrators.MSSQL;

public class MigrationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
         .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host"))
      .AddJsonFile("Configurations/database.json", optional: false)
            .Build();

        var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
   x => x.MigrationsAssembly("Migrators.MSSQL"));

        return new ApplicationDbContext(optionsBuilder.Options);
  }
}
```

**Giải thích:**
- EF Core tìm `IDesignTimeDbContextFactory` khi chạy migrations
- Load connection string từ Host/database.json
- Chỉ định migrations assembly là `Migrators.MSSQL`

---

**Update:** `src/Host/Host.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Migrators\Migrators.MSSQL\Migrators.MSSQL.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
    <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

---

## 3. Tạo Custom Seeder Pattern
### Bước 3.1: ICustomSeeder Interface
**File:** `src/Infrastructure/Persistence/Initialization/ICustomSeeder.cs`

```csharp
namespace {ProjectName}.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
```

**Mục đích:** Cho phép các module khác tự seed data của mình (optional).

---
### Bước 3.2: CustomSeederRunner
**File:** `src/Infrastructure/Persistence/Initialization/CustomSeederRunner.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace {ProjectName}.Infrastructure.Persistence.Initialization;

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

**Giải thích:**
- Tự động tìm tất cả implementations của `ICustomSeeder`
- Chạy tuần tự từng seeder
- Custom seeders sẽ được auto-register trong DI

---

## 4. Tạo ApplicationDbSeeder
### Bước 4.1: ApplicationDbSeeder Implementation
**File:** `src/Infrastructure/Persistence/Initialization/ApplicationDbSeeder.cs`

```csharp
using System.Reflection;
using {ProjectName}.Domain.Identity;
using {ProjectName}.Infrastructure.Persistence.Context;
using {ProjectName}.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace {ProjectName}.Infrastructure.Persistence.Initialization;

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
        // Seed theo thứ tự phụ thuộc
 await SeedActionsAndFunctionsAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
   await _seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedActionsAndFunctionsAsync(ApplicationDbContext dbContext)
    {
        // 1. Seed Actions
    var actions = typeof(AppAction)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
   .Where(f => f.IsLiteral && !f.IsInitOnly)
       .Select(f => f.GetValue(null)?.ToString())
       .Where(v => v != null)
         .ToList();

        foreach (var action in actions!)
        {
   if (!await dbContext.Actions.AnyAsync(x => x.Name == action))
    {
     dbContext.Actions.Add(new Domain.Identity.Action { Name = action });
          }
   }
        await dbContext.SaveChangesAsync();

        // 2. Seed Functions
        var functions = typeof(AppFunction)
    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
         .Where(f => f.IsLiteral && !f.IsInitOnly)
   .Select(f => f.GetValue(null)?.ToString())
    .Where(v => v != null)
       .ToList();

        foreach (var functionName in functions!)
        {
     if (!await dbContext.Functions.AnyAsync(f => f.Name == functionName))
         {
  dbContext.Functions.Add(new Function { Name = functionName });
            }
      }
   await dbContext.SaveChangesAsync();

     // 3. Link Actions with Functions
        foreach (var functionName in functions!)
   {
       var function = await dbContext.Functions.FirstAsync(f => f.Name == functionName);
            foreach (var actionName in actions!)
     {
        var action = await dbContext.Actions.FirstAsync(a => a.Name == actionName);
    if (!await dbContext.ActionInFunctions.AnyAsync(
      aif => aif.FunctionId == function.Id && aif.ActionId == action.Id))
                {
    dbContext.ActionInFunctions.Add(new ActionInFunction(action.Id, function.Id));
           }
          }
 }
 await dbContext.SaveChangesAsync();
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {
        foreach (string roleName in AppRoles.DefaultRoles)
        {
            if (await _roleManager.FindByNameAsync(roleName) is not ApplicationRole role)
            {
       _logger.LogInformation("Seeding {role} Role.", roleName);
    role = new ApplicationRole(roleName, $"{roleName} Role");
                await _roleManager.CreateAsync(role);
}

       if (roleName == AppRoles.Admin)
      {
            await AssignAllPermissionsAsync(dbContext, role);
  }
      else if (roleName == AppRoles.Basic)
   {
     await AssignBasicPermissionsAsync(dbContext, role);
          }
        }
    }

    private async Task AssignAllPermissionsAsync(ApplicationDbContext dbContext, ApplicationRole role)
  {
        var functions = await dbContext.Functions.ToListAsync();
     foreach (var function in functions)
        {
  var actions = await dbContext.ActionInFunctions
            .Where(aif => aif.FunctionId == function.Id)
    .ToListAsync();

         foreach (var actionInFunction in actions)
          {
    if (!await dbContext.Permissions.AnyAsync(p =>
   p.RoleId == role.Id &&
 p.FunctionId == function.Id &&
        p.ActionId == actionInFunction.ActionId))
   {
  dbContext.Permissions.Add(new Permission(role.Id, function.Id, actionInFunction.ActionId));
  }
            }
      }
        await dbContext.SaveChangesAsync();
    }

    private async Task AssignBasicPermissionsAsync(ApplicationDbContext dbContext, ApplicationRole role)
    {
        var basicActions = new[] { AppAction.View, AppAction.Search };
        var functions = await dbContext.Functions.ToListAsync();

        foreach (var function in functions)
        {
var actions = await dbContext.ActionInFunctions
            .Include(x => x.Action)
    .Where(aif => aif.FunctionId == function.Id && basicActions.Contains(aif.Action.Name))
    .ToListAsync();

            foreach (var actionInFunction in actions)
            {
     if (!await dbContext.Permissions.AnyAsync(p =>
      p.RoleId == role.Id &&
   p.FunctionId == function.Id &&
      p.ActionId == actionInFunction.ActionId))
 {
  dbContext.Permissions.Add(new Permission(role.Id, function.Id, actionInFunction.ActionId));
   }
      }
        }
      await dbContext.SaveChangesAsync();
    }

  private async Task SeedAdminUserAsync()
    {
  const string adminEmail = "admin@gmail.com";
        const string adminPassword = "Abcd@1234";

        if (await _userManager.FindByEmailAsync(adminEmail) is not ApplicationUser adminUser)
        {
            _logger.LogInformation("Seeding default admin user.");

            adminUser = new ApplicationUser
 {
         FirstName = "System",
          LastName = "Admin",
           Email = adminEmail,
                UserName = "system.admin",
        EmailConfirmed = true,
           PhoneNumberConfirmed = true,
 IsActive = true
   };

            await _userManager.CreateAsync(adminUser, adminPassword);
        }

        if (!await _userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
    {
          await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
}
    }
}
```

**Seeding flow:**
```
Actions & Functions (từ constants)
    ↓
Roles (Admin, Basic)
    ↓
Admin User (system.admin)
    ↓
Custom Seeders (optional)
```

---

## 5. Tạo DatabaseInitializer
### Bước 5.1: DatabaseInitializer Implementation
**File:** `src/Infrastructure/Persistence/Initialization/DatabaseInitializer.cs`

```csharp
using {ProjectName}.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace {ProjectName}.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer
{
    private readonly ApplicationDbContext _dbContext;
private readonly ApplicationDbSeeder _dbSeeder;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        ApplicationDbContext dbContext,
        ApplicationDbSeeder dbSeeder,
    ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
      _dbSeeder = dbSeeder;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // 1. Check migrations exist
        if (!_dbContext.Database.GetMigrations().Any())
 {
   _logger.LogWarning("No migrations found. Skipping database initialization.");
          return;
      }

        // 2. Apply pending migrations
     var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
   _logger.LogInformation("Applying {count} pending migrations...", pendingMigrations.Count());
   await _dbContext.Database.MigrateAsync(cancellationToken);
    _logger.LogInformation("Migrations applied successfully.");
   }

        if (await _dbContext.Database.CanConnectAsync(cancellationToken))
        {
            await _dbSeeder.SeedDatabaseAsync(_dbContext, cancellationToken);
        }
    }
}
```

---

## 6. Register Services

**Update:** `src/Infrastructure/Persistence/Startup.cs`

```csharp
using {ProjectName}.Infrastructure.Persistence.Initialization;
// ... existing usings ...

internal static IServiceCollection AddPersistence(this IServiceCollection services)
{
    // ... existing code ...

    // Database initialization
    services.AddScoped<CustomSeederRunner>();
    services.AddScoped<ApplicationDbSeeder>();
 services.AddScoped<DatabaseInitializer>();

    return services;
}
```

**Thứ tự registration:**
1. `CustomSeederRunner` - Chạy custom seeders
2. `ApplicationDbSeeder` - Seeding chính (depends on CustomSeederRunner)
3. `DatabaseInitializer` - Orchestrator (depends on ApplicationDbSeeder)

---

**Update:** `src/Infrastructure/Startup.cs`

```csharp
using {ProjectName}.Infrastructure.Persistence.Initialization;
// ... existing usings ...

public static class Startup
{
  // ... existing methods ...

    /// <summary>
    /// Initialize databases (apply migrations + seed data)
    /// </summary>
    public static async Task InitializeDatabasesAsync(
        this IServiceProvider services,
      CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync(cancellationToken);
    }
}
```

---

## 7. Create First Migration

```powershell
cd src\Host
dotnet ef migrations add InitialCreate `
  --project ..\Migrators\Migrators.MSSQL\Migrators.MSSQL.csproj `
  --context ApplicationDbContext
```

---

## 8. Update Program.cs

**File:** `src/Host/Program.cs`

```csharp
using {ProjectName}.Application;
using {ProjectName}.Host.Configurations;
using {ProjectName}.Infrastructure;
using {ProjectName}.Infrastructure.Logging;
using Serilog;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations();
    builder.RegisterSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
Title = "{ProjectName} API",
         Version = "v1"
        });
    });

    var app = builder.Build();

    Log.Information("Application built successfully");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

  app.UseSerilogRequestLogging(options =>
    {
  options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    // ⭐ Initialize database
    await app.Services.InitializeDatabasesAsync();

    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();

    Log.Information("Application Starting...");
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
```

---

## 9. Verify

```powershell
dotnet run --project src\Host\Host.csproj
```

**Expected logs:**
```
[INF] Server Booting Up...
[INF] Application built successfully
[INF] Applying 1 pending migrations...
[INF] Migrations applied successfully.
[INF] Seeding Admin Role.
[INF] Seeding Basic Role.
[INF] Seeding default admin user.
[INF] Application Starting...
```

---

## 10. Summary

### ✅ Đã hoàn thành:

- ✅ Migrators.MSSQL project
- ✅ MigrationDbContextFactory
- ✅ ICustomSeeder pattern
- ✅ ApplicationDbSeeder
- ✅ DatabaseInitializer
- ✅ First migration
- ✅ Auto-migration on startup

**Data seeded:**
- Actions & Functions (từ constants)
- Roles (Admin, Basic)
- Admin User: `system.admin / Abcd@1234`
- Permissions

---

## 11. Bước tiếp theo

**Tiếp theo:** [BUILD_09 - Domain Base Entities](BUILD_09_Domain_Base_Entities.md)

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
