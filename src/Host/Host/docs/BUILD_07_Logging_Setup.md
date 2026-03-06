# BUILD_07 - Logging Setup (Serilog)

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** BUILD_06 (Host Layer) đã hoàn thành  
> ⏱️ **Thời gian:** Khoảng 15 phút

Tài liệu này hướng dẫn setup Serilog - structured logging framework cho .NET application.

---

## 1. Overview

**Làm gì:** Setup Serilog structured logging cho application.

**Tại sao cần:**
- **Troubleshooting:** Debug issues trong production
- **Monitoring:** Track application health
- **Auditing:** Record user actions
- **Structured logs:** Query logs như database

**Trong bước này chúng ta sẽ:**
- ✅ Add Serilog packages
- ✅ Tạo StaticLogger (bootstrap logging)
- ✅ Configure logger.json
- ✅ Setup multiple sinks (Console, File)
- ✅ Integrate vào Program.cs

---

## 2. Add Serilog Packages

**File:** `src/Infrastructure/Infrastructure.csproj`

```xml
<ItemGroup>
  <!-- Serilog Core -->
  <PackageReference Include="Serilog" Version="3.1.1" />
  <PackageReference Include="Serilog.Extensions.Hosting" Version="5.0.1" />
  <PackageReference Include="Serilog.Settings.Configuration" Version="3.4.0" />
  
  <!-- Sinks -->
  <PackageReference Include="Serilog.Sinks.Console" Version="4.1.0" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  <PackageReference Include="Serilog.Sinks.Async" Version="1.5.0" />
  
  <!-- Enrichers -->
  <PackageReference Include="Serilog.Enrichers.Environment" Version="2.2.0" />
  <PackageReference Include="Serilog.Enrichers.Process" Version="2.0.2" />
  <PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
  <PackageReference Include="Serilog.Exceptions" Version="8.4.0" />
</ItemGroup>
```

**File:** `src/Host/Host.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Serilog.AspNetCore" Version="6.1.0" />
</ItemGroup>
```

---

## 3. Tạo StaticLogger

```powershell
New-Item -ItemType Directory -Path "src\Infrastructure\Logging" -Force
```

**File:** `src/Infrastructure/Logging/StaticLogger.cs`

```csharp
using Serilog;
using Serilog.Events;

namespace {ProjectName}.Infrastructure.Logging;

public static class StaticLogger
{
    public static void EnsureInitialized()
  {
     if (Log.Logger is not Serilog.Core.Logger)
{
      Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
       .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
   .Enrich.FromLogContext()
     .WriteTo.Console()
    .CreateBootstrapLogger();
        }
    }
}
```

---

## 4. Logger Configuration

**File:** `src/Host/Configurations/logger.json`

```json
{
  "Serilog": {
    "Using": [
 "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Async"
 ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
   "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
  "Args": {
        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
   },
      {
        "Name": "Async",
     "Args": {
 "configure": [
    {
    "Name": "File",
              "Args": {
  "path": "Logs/log-.txt",
        "rollingInterval": "Day",
          "rollOnFileSizeLimit": true,
 "fileSizeLimitBytes": 10485760,
      "retainedFileCountLimit": 7,
     "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    }
      }
          ]
   }
      }
    ],
    "Enrich": [
  "FromLogContext",
      "WithMachineName",
      "WithThreadId",
      "WithExceptionDetails"
    ],
    "Properties": {
  "Application": "{ProjectName}"
    }
}
}
```

**Giải thích cấu hình:**

**MinimumLevel:**
- `Default: Information` - Log level mặc định
- `Override` - Override cho specific namespaces (giảm noise từ Microsoft logs)

**WriteTo:**
- **Console:** Hiển thị logs trong console (development)
- **File (Async):** Write logs to file với rolling (production)
  - `rollingInterval: Day` - Mỗi ngày một file mới
  - `fileSizeLimitBytes: 10MB` - Max file size
  - `retainedFileCountLimit: 7` - Keep 7 ngày logs

**Enrich:**
- `FromLogContext` - Add contextual properties
- `WithMachineName` - Add machine name
- `WithThreadId` - Add thread ID
- `WithExceptionDetails` - Add exception details

---

**Update:** `src/Host/Configurations/Startup.cs`

```csharp
namespace {ProjectName}.Host.Configurations;

internal static class Startup
{
    internal static WebApplicationBuilder AddConfigurations(
        this WebApplicationBuilder builder)
    {
        const string configurationsDirectory = "Configurations";
        var env = builder.Environment;
   
        builder.Configuration
         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
  .AddJsonFile($"{configurationsDirectory}/database.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"{configurationsDirectory}/database.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
      .AddJsonFile($"{configurationsDirectory}/logger.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"{configurationsDirectory}/logger.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables();
     
        return builder;
    }
}
```

---

## 5. Register Serilog Extension

**File:** `src/Infrastructure/Logging/Extensions.cs`

```csharp
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace {ProjectName}.Infrastructure.Logging;

public static class Extensions
{
    public static WebApplicationBuilder RegisterSerilog(this WebApplicationBuilder builder)
 {
        builder.Logging.ClearProviders();
        
        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
    .ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
         .Enrich.FromLogContext()
         .Enrich.WithProperty("Application", "{ProjectName}")
      .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
 });

        return builder;
    }
}
```

---

## 6. Update Program.cs

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

 // 2. Load configurations (includes logger.json)
    builder.AddConfigurations();
    
    // 3. Register Serilog (full configuration)
    builder.RegisterSerilog();

    // 4. Add services to DI container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // 5. Add layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    
    // 6. Add Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
      {
          Title = "{ProjectName} API",
  Version = "v1"
        });
    });

    // 7. Build application
    var app = builder.Build();

    // 8. Log application built
    Log.Information("Application built successfully");

    // 9. Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
   app.UseSwagger();
        app.UseSwaggerUI();
    }

    // 10. Request logging middleware
    app.UseSerilogRequestLogging(options =>
    {
   options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

    // 11. Use Infrastructure middleware
    app.UseInfrastructure(builder.Configuration);
    
    // 12. Map endpoints
    app.MapEndpoints();

    // 13. Run application
    Log.Information("Application Starting...");
    Log.Information("Listening on: {Addresses}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception occurred during application startup");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
```

**Thêm features:**
- `UseSerilogRequestLogging()` - Log HTTP requests/responses
- `EnrichDiagnosticContext` - Add extra properties to request logs
- Better error handling với detailed error messages

**Lưu ý:** Đã remove dòng `await app.Services.InitializeDatabasesAsync()` vì phần này sẽ có ở BUILD_08.

---

## 6. Logging Best Practices

### Bước 6.1: Log Levels Usage

```csharp
// Verbose - Quá chi tiết, chỉ dùng khi debug sâu
Log.Verbose("Processing item {ItemId}", itemId);

// Debug - Thông tin development/troubleshooting
Log.Debug("Cache miss for key {CacheKey}", key);

// Information - General flow của application
Log.Information("User {UserId} logged in successfully", userId);

// Warning - Unexpected nhưng không critical
Log.Warning("Rate limit approaching for IP {IpAddress}", ipAddress);

// Error - Lỗi cần attention
Log.Error(ex, "Failed to process order {OrderId}", orderId);

// Fatal - Application không thể tiếp tục
Log.Fatal(ex, "Database connection failed. Application cannot start.");
```

---

### Bước 6.2: Structured Logging Example

```csharp
// ❌ BAD - String concatenation
Log.Information("User " + userId + " placed order " + orderId);

// ✅ GOOD - Structured logging
Log.Information("User {UserId} placed order {OrderId}", userId, orderId);

// ✅ BETTER - With object
Log.Information("Order placed: {@Order}", new
{
    UserId = userId,
    OrderId = orderId,
    Total = total,
    Items = items.Count
});
```

**Benefits:**
- Searchable: Có thể query `UserId = "123"`
- Analyzable: Aggregate và analytics
- Machine-readable: Parse và process logs

---

### Bước 6.3: Using LogContext

**File:** `src/Infrastructure/Infrastructure/Identity/UserService.cs` (example)

```csharp
using Serilog.Context;

public class UserService : IUserService
{
    public async Task<UserDto> GetByIdAsync(string userId)
{
        // Push context property
        using (LogContext.PushProperty("UserId", userId))
        {
            Log.Information("Fetching user details");
   
            var user = await _db.Users.FindAsync(userId);
      
            if (user == null)
            {
                Log.Warning("User not found");
                throw new NotFoundException("User not found");
}
      
            Log.Information("User details fetched successfully");
            return user.Adapt<UserDto>();
        }
    }
}
```

**Output log:**
```
[Information] Fetching user details | UserId: "abc123"
[Information] User details fetched successfully | UserId: "abc123"
```

---

## 7. Environment-Specific Configurations

### Bước 7.1: Development Configuration

**File:** `src/Host/Host/Configurations/logger.Development.json`

```json
{
  "Serilog": {
  "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
  },
 "WriteTo": [
      {
        "Name": "Console",
        "Args": {
"outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}  {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
  ]
  }
}
```

**Features:**
- `Default: Debug` - More verbose logging
- EF Core SQL queries visible
- Prettier console output

---

### Bước 7.2: Production Configuration

**File:** `src/Host/Host/Configurations/logger.Production.json`

```json
{
  "Serilog": {
  "MinimumLevel": {
      "Default": "Information",
      "Override": {
   "Microsoft": "Warning",
      "System": "Warning"
      }
    },
    "WriteTo": [
{
        "Name": "Async",
  "Args": {
          "configure": [
          {
              "Name": "File",
       "Args": {
                "path": "/var/log/eco-webapi/log-.txt",
              "rollingInterval": "Day",
        "rollOnFileSizeLimit": true,
                "fileSizeLimitBytes": 52428800,
     "retainedFileCountLimit": 30,
      "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    }
    }
          ]
        }
      }
 ]
        }
      }
```

**Features:**
- `Default: Information` - Less noise
- Larger files (50MB)
- Keep logs 30 days
- Production log path

---

## 8. Advanced Sinks (Optional)

### Bước 8.1: Seq (Centralized Logging)

**Add package:**
```xml
<PackageReference Include="Serilog.Sinks.Seq" Version="5.2.2" />
```

**Update logger.json:**
```json
{
  "Serilog": {
    "WriteTo": [
    {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
  "apiKey": "your-api-key"
        }
      }
 ]
  }
}
```

**Expected console:**
```
[12:00:00 INF] Server Booting Up...
[12:00:01 INF] Application built successfully
[12:00:02 INF] Application Starting...
```

**Check log file:** `src/Host/Logs/log-{date}.txt`

---

## 8. Summary

### ✅ Đã hoàn thành:

- ✅ Serilog packages
- ✅ StaticLogger (bootstrap)
- ✅ logger.json configuration
- ✅ Multiple sinks (Console, File, Async)
- ✅ Enrichers
- ✅ Request logging

### 📁 File Structure:

```
src\Infrastructure\
├── Logging\
│├── StaticLogger.cs
│   └── Extensions.cs
└── ...

src\Host\
├── Configurations\
│   └── logger.json
└── Logs\
    └── log-{date}.txt
```

---

## 9. Bước tiếp theo

**Tiếp theo:** [BUILD_08 - Database Initialization](BUILD_08_Database_Initialization.md)

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
