# Xây dựng Infrastructure Layer

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn xây dựng Infrastructure Layer - chứa implementations của các interfaces.

---

## Bước 6.1: Setup Infrastructure Project

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

## Bước 6.2: Tạo Infrastructure Startup

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

**Tiếp theo:** [Xây dựng Host Layer](BUILD_06_Host_Layer.md)
