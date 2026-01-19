# Xây dựng Host Layer

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn xây dựng Host Layer - ASP.NET Core Web API entry point.

---

## Bước 7.1: Setup Host Project

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

## Bước 7.2: Tạo Program.cs

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

**Tiếp theo:** [Database Initialization và Seed Data](BUILD_07_Database_Initialization.md)
