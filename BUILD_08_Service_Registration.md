# Service Registration Pattern

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn về Service Registration Pattern - tự động đăng ký services mà không cần đăng ký từng cái một.

---

## Bước 9.1: Tạo Interface Markers

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

**Cách sử dụng:**
- Service class implement cả business interface VÀ marker interface
- Ví dụ: `IProductService` (business) + `ITransientService` (marker)

---

## Bước 9.2: Tạo AddServices Extension Method

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
- Interface đầu tiên (không phải marker) sẽ được dùng làm service type

**Tại sao scan AppDomain:**
- Tự động discover services từ tất cả assemblies
- Không cần chỉ định assembly cụ thể
- Dễ mở rộng: thêm service mới → tự động được đăng ký

---

## Bước 9.3: Sử dụng trong Infrastructure Startup

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

**Thứ tự:**
- Nên gọi `AddServices()` cuối cùng trong `AddInfrastructure()`
- Đảm bảo tất cả modules khác đã được đăng ký trước

---

## Ví dụ sử dụng

### Tạo Service mới

**File:** `src/Infrastructure/Infrastructure/Services/ProductService.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;

namespace ECO.WebApi.Infrastructure.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync();
}

public class ProductService : IProductService, ITransientService
{
    public async Task<List<ProductDto>> GetProductsAsync()
    {
        // Implementation
    }
}
```

**Kết quả:** Service tự động được đăng ký với lifetime `Transient`.

**Không cần:**
```csharp
// KHÔNG CẦN làm thế này nữa!
services.AddTransient<IProductService, ProductService>();
```

---

## Tóm tắt

### Cách hoạt động:

1. **Marker Interfaces** → Đánh dấu service lifetime
2. **AddServices()** → Scan và đăng ký tự động
3. **Service Registration** → Tự động trong Infrastructure Startup

### Điểm quan trọng:

- **Service class** phải implement cả business interface VÀ marker interface
- **Marker interface** không có methods, chỉ dùng để đánh dấu
- **Auto-discovery** từ tất cả assemblies trong AppDomain

### Lợi ích:

- **Không cần đăng ký thủ công** → Tiết kiệm thời gian
- **Dễ mở rộng** → Thêm service mới → tự động được đăng ký
- **Consistency** → Tất cả services đều được đăng ký theo cùng một pattern
- **Dễ maintain** → Không cần update Startup khi thêm service mới

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)
