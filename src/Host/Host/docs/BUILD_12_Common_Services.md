# Common Services - CurrentUser, Serializer, Event Publisher

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** B??c 11 (Repository Pattern) ?ã hoàn thành

Tài li?u này h??ng d?n xây d?ng các Core Services n?n t?ng: CurrentUser, Serializer, và Event Publisher.

---

## 1. Overview

**Làm gì:** Xây d?ng các core services ???c s? d?ng xuyên su?t application.

**T?i sao c?n:**
- **CurrentUser Service:** L?y thông tin user hi?n t?i t? JWT token trong m?i handler/service
- **Serializer Service:** Serialize/deserialize objects cho caching, logging, messaging
- **Event Publisher:** Publish domain events ?? trigger các event handlers (decoupling)

**Trong b??c này chúng ta s?:**
- ? T?o `ICurrentUser` và `ICurrentUserInitializer` interfaces
- ? Implement `CurrentUser` service v?i ClaimsPrincipal
- ? T?o `CurrentUserMiddleware` ?? auto-set current user
- ? T?o `ISerializerService` interface
- ? Implement `NewtonSoftService` (JSON serialization)
- ? T?o `IEventPublisher` interface
- ? Implement `EventPublisher` v?i MediatR integration
- ? Register services và middleware

**Real-world example:**
```csharp
// Trong handler - L?y current user
public class CreateProductHandler : IRequestHandler<CreateProductRequest, Guid>
{
 private readonly ICurrentUser _currentUser;
    private readonly IEventPublisher _eventPublisher;

    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken ct)
 {
     // Auto có thông tin user hi?n t?i
  var userId = _currentUser.GetUserId();
    var userEmail = _currentUser.GetUserEmail();
    
      var product = Product.Create(request.Name, request.Price);
     
     // Publish domain event
        await _eventPublisher.PublishAsync(new ProductCreatedEvent(product));
        
        return product.Id;
    }
}
```

---

## 2. Add Required Packages

### B??c 2.1: Add Newtonsoft.Json Package

**File:** `src/Infrastructure/Infrastructure/Infrastructure.csproj`

```xml
<ItemGroup>
    <!-- JSON Serialization -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

**Gi?i thích:**
- `Newtonsoft.Json`: JSON serializer/deserializer (mature và feature-rich h?n System.Text.Json)

**?? L?u ý:** MediatR ?ã có t? Application layer, không c?n add l?i.

---

## 3. CurrentUser Service

### B??c 3.1: ICurrentUser Interface

**Làm gì:** T?o interface ?? l?y thông tin user hi?n t?i t? JWT token.

**T?i sao:** Handlers/Services c?n bi?t user nào ?ang th?c hi?n action (audit, authorization).

**File:** `src/Core/Application/Common/Interfaces/ICurrentUser.cs`

```csharp
using System.Security.Claims;

namespace ECO.WebApi.Application.Common.Interfaces;

/// <summary>
/// Interface ?? l?y thông tin user hi?n t?i t? JWT token
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// User name t? Identity.Name
    /// </summary>
    string? Name { get; }

    /// <summary>
  /// L?y User ID (Guid) t? NameIdentifier claim
    /// </summary>
    Guid GetUserId();

/// <summary>
    /// L?y User Email t? Email claim
    /// </summary>
    string? GetUserEmail();

    /// <summary>
    /// Check user ?ã authenticate ch?a
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Check user có role c? th? không
    /// </summary>
    bool IsInRole(string role);

 /// <summary>
  /// L?y t?t c? claims c?a user
/// </summary>
 IEnumerable<Claim>? GetUserClaims();
}
```

**Gi?i thích:**
- `Name`: Display name t? JWT claims
- `GetUserId()`: User ID (Guid) t? NameIdentifier claim
- `GetUserEmail()`: Email t? Email claim
- `IsAuthenticated()`: Check xem user ?ã login ch?a
- `IsInRole(role)`: Check user có role c? th? (Admin, Basic, etc.)
- `GetUserClaims()`: L?y all claims ?? custom logic

**T?i sao tách interface:**
- Read-only trong handlers/services
- D? mock cho unit testing
- Separation of concerns

---

### B??c 3.2: ICurrentUserInitializer Interface

**Làm gì:** Interface ?? set current user (dùng trong middleware).

**T?i sao:** Middleware c?n set user t? HttpContext, còn handlers ch? c?n ??c.

**File:** `src/Core/Application/Common/Interfaces/ICurrentUserInitializer.cs`

```csharp
using System.Security.Claims;

namespace ECO.WebApi.Application.Common.Interfaces;

/// <summary>
/// Interface ?? initialize current user (dùng trong middleware)
/// </summary>
public interface ICurrentUserInitializer
{
    /// <summary>
    /// Set current user t? ClaimsPrincipal (t? JWT token)
    /// </summary>
    void SetCurrentUser(ClaimsPrincipal user);

    /// <summary>
    /// Set current user ID manually (cho background jobs/system operations)
    /// </summary>
    void SetCurrentUserId(string userId);
}
```

**Gi?i thích:**
- `SetCurrentUser()`: Set t? HttpContext.User (có JWT token)
- `SetCurrentUserId()`: Set manually cho background jobs (không có HTTP context)

**T?i sao tách 2 interfaces:**
- `ICurrentUser`: Read-only cho handlers/services
- `ICurrentUserInitializer`: Write-only cho middleware
- Better encapsulation

---

### B??c 3.3: ClaimsPrincipal Extension Methods

**Làm gì:** Extension methods ?? l?y claims t? ClaimsPrincipal d? dàng h?n.

**T?i sao:** Code g?n h?n, reusable, type-safe.

**File:** `src/Core/Shared/Authorization/ClaimsPrincipalExtensions.cs`

```csharp
using ECO.WebApi.Shared.Authorization;

namespace System.Security.Claims;

/// <summary>
/// Extension methods cho ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// L?y Email t? ClaimTypes.Email
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
      => principal.FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// L?y Full Name t? ECOClaims.Fullname
    /// </summary>
    public static string? GetFullName(this ClaimsPrincipal principal)
      => principal?.FindFirst(ECOClaims.Fullname)?.Value;

    /// <summary>
    /// L?y First Name t? ClaimTypes.Name
    /// </summary>
    public static string? GetFirstName(this ClaimsPrincipal principal)
        => principal?.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// L?y Surname t? ClaimTypes.Surname
    /// </summary>
    public static string? GetSurname(this ClaimsPrincipal principal)
=> principal?.FindFirst(ClaimTypes.Surname)?.Value;

    /// <summary>
    /// L?y Phone Number t? ClaimTypes.MobilePhone
    /// </summary>
    public static string? GetPhoneNumber(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.MobilePhone);

    /// <summary>
    /// L?y User ID t? ClaimTypes.NameIdentifier
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
       => principal.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// L?y Image URL t? ECOClaims.ImageUrl
    /// </summary>
    public static string? GetImageUrl(this ClaimsPrincipal principal)
       => principal.FindFirstValue(ECOClaims.ImageUrl);

    /// <summary>
    /// L?y Token Expiration t? ECOClaims.Expiration
    /// </summary>
    public static DateTimeOffset GetExpiration(this ClaimsPrincipal principal) =>
    DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(
            principal.FindFirstValue(ECOClaims.Expiration)));

    /// <summary>
  /// Helper method ?? tìm claim value
    /// </summary>
    private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType) =>
        principal is null
  ? throw new ArgumentNullException(nameof(principal))
            : principal.FindFirst(claimType)?.Value;
}
```

**Gi?i thích:**
- Extension methods ?? code g?n h?n: `user.GetUserId()` thay vì `user.FindFirst(ClaimTypes.NameIdentifier)?.Value`
- Support custom claims: `Fullname`, `ImageUrl`, `Expiration`
- Null-safe v?i `?` operator
- Private `FindFirstValue()` helper ?? avoid repetition

**L?i ích:**
- ? Code g?n, d? ??c
- ? Type-safe
- ? Reusable
- ? D? maintain

---

### B??c 3.4: CurrentUser Implementation

**Làm gì:** Implement CurrentUser service k?t h?p ICurrentUser và ICurrentUserInitializer.

**T?i sao:** M?t class implement c? 2 interfaces, scoped per request.

**File:** `src/Infrastructure/Infrastructure/Auth/CurrentUser.cs`

```csharp
using System.Security.Claims;
using ECO.WebApi.Application.Common.Interfaces;

namespace ECO.WebApi.Infrastructure.Auth;

/// <summary>
/// Implementation c?a ICurrentUser và ICurrentUserInitializer
/// Scoped per request - m?i HTTP request có instance riêng
/// </summary>
public class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private ClaimsPrincipal? _user;
    private Guid _userId = Guid.Empty;

    /// <summary>
    /// User name t? Identity.Name
    /// </summary>
    public string? Name => _user?.Identity?.Name;

    /// <summary>
    /// L?y User ID t? NameIdentifier claim
    /// </summary>
    public Guid GetUserId() =>
        IsAuthenticated()
? Guid.Parse(_user?.GetUserId() ?? Guid.Empty.ToString())
: _userId;

    /// <summary>
    /// L?y User Email t? Email claim
    /// </summary>
    public string? GetUserEmail() =>
      IsAuthenticated()
 ? _user!.GetEmail()
  : string.Empty;

    /// <summary>
    /// Check user ?ã authenticate ch?a
    /// </summary>
  public bool IsAuthenticated() =>
        _user?.Identity?.IsAuthenticated is true;

    /// <summary>
    /// Check user có role không
    /// </summary>
    public bool IsInRole(string role) =>
     _user?.IsInRole(role) is true;

    /// <summary>
    /// L?y t?t c? claims
    /// </summary>
    public IEnumerable<Claim>? GetUserClaims() =>
 _user?.Claims;

    /// <summary>
    /// Set current user t? ClaimsPrincipal
    /// Ch? ???c g?i m?t l?n per request (t? middleware)
    /// </summary>
    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (_user != null)
        {
 throw new Exception("Method reserved for in-scope initialization");
        }

 _user = user;
    }

    /// <summary>
  /// Set current user ID manually (cho background jobs)
    /// </summary>
    public void SetCurrentUserId(string userId)
 {
        if (_userId != Guid.Empty)
  {
            throw new Exception("Method reserved for in-scope initialization");
        }

        if (!string.IsNullOrEmpty(userId))
        {
        _userId = Guid.Parse(userId);
        }
    }
}
```

**Gi?i thích:**

**Private fields:**
- `_user`: ClaimsPrincipal t? JWT token (HTTP requests)
- `_userId`: User ID manual (background jobs không có HTTP context)

**Thread-safety:**
- Service là `Scoped` ? m?i request có instance riêng
- Check `_user != null` ?? prevent double initialization
- Throw exception n?u g?i `SetCurrentUser()` nhi?u l?n

**Fallback logic:**
- N?u authenticated ? l?y t? claims
- N?u không ? return empty/default values (background jobs)

**T?i sao c?n _userId riêng:**
- Background jobs (Hangfire) không có HTTP context
- V?n c?n track user th?c hi?n job
- Set manual qua `SetCurrentUserId()`

---

### B??c 3.5: CurrentUserMiddleware

**Làm gì:** Middleware ?? t? ??ng set current user t? HttpContext.User.

**T?i sao:** M?i request ??u c?n user context, middleware t? ??ng set thay vì manual.

**File:** `src/Infrastructure/Infrastructure/Auth/CurrentUserMiddleware.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECO.WebApi.Infrastructure.Auth;

/// <summary>
/// Middleware ?? set current user t? HttpContext.User
/// Ph?i ??t SAU UseAuthentication() trong pipeline
/// </summary>
public class CurrentUserMiddleware : IMiddleware
{
    private readonly ICurrentUserInitializer _currentUserInitializer;

    public CurrentUserMiddleware(ICurrentUserInitializer currentUserInitializer) =>
        _currentUserInitializer = currentUserInitializer;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Set current user t? HttpContext.User (?ã authenticate b?i JWT middleware)
        _currentUserInitializer.SetCurrentUser(context.User);
   
        // Continue pipeline
    await next(context);
    }
}
```

**Gi?i thích:**
- `IMiddleware` interface ? ASP.NET Core middleware pattern
- `SetCurrentUser(context.User)` ? Set ClaimsPrincipal t? authenticated user
- `await next(context)` ? Continue pipeline

**Th? t? middleware (QUAN TR?NG):**
```
1. UseRouting()
2. UseAuthentication()           ? JWT middleware populate context.User
3. UseCurrentUserMiddleware()    ? Set ICurrentUser t? context.User
4. UseAuthorization()
5. MapControllers()
```

**?? L?u ý:** Middleware này ph?i ??t SAU `UseAuthentication()` ?? có `context.User`.

---

### B??c 3.6: Register CurrentUser Service

**Làm gì:** Register CurrentUser và middleware vào DI container.

**T?i sao:** ASP.NET Core c?n bi?t cách t?o và inject services.

**File:** `src/Infrastructure/Infrastructure/Auth/Startup.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Auth;

internal static class Startup
{
    /// <summary>
    /// Register CurrentUser services
    /// </summary>
    internal static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        // Register middleware as Scoped (per request)
      services.AddScoped<CurrentUserMiddleware>();
        
     // Register CurrentUser as Scoped - m?i request m?t instance
     // C? 2 interfaces ??u resolve v? cùng instance
services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentUserInitializer, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Use CurrentUser middleware
    /// </summary>
    internal static IApplicationBuilder UseCurrentUserMiddleware(this IApplicationBuilder app) =>
      app.UseMiddleware<CurrentUserMiddleware>();
}
```

**Gi?i thích:**
- `Scoped` lifetime ? m?i HTTP request có instance riêng, dispose sau khi request done
- `ICurrentUser` và `ICurrentUserInitializer` ? cùng resolve v? m?t instance `CurrentUser`
- Extension methods ?? code g?n

**T?i sao Scoped:**
- ? M?i request có user riêng (thread-safe)
- ? Dispose t? ??ng sau request
- ? Performance t?t h?n Transient

---

## 4. Serializer Service

### B??c 4.1: ISerializerService Interface

**Làm gì:** Interface ?? serialize/deserialize objects thành JSON.

**T?i sao:** Caching, logging, messaging ??u c?n serialize objects. Interface ?? d? thay ??i implementation.

**File:** `src/Core/Application/Common/Interfaces/ISerializerService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Interfaces;

/// <summary>
/// Interface ?? serialize/deserialize objects
/// Dùng cho caching, logging, messaging, etc.
/// </summary>
public interface ISerializerService : ITransientService
{
    /// <summary>
    /// Serialize object thành JSON string
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// Serialize object thành JSON string v?i type c? th?
/// </summary>
    string Serialize<T>(T obj, Type type);

    /// <summary>
    /// Deserialize JSON string thành object
    /// </summary>
    T Deserialize<T>(string text);
}
```

**Gi?i thích:**
- `Transient` lifetime ? t?o instance m?i m?i l?n inject (lightweight)
- Generic methods ? support any type
- 2 overloads cho `Serialize()` ?? flexible

**Use cases:**
- **Caching:** Serialize objects tr??c khi cache vào Redis
- **Logging:** Serialize request/response ?? log
- **Messaging:** Serialize events/commands ?? send qua queue
- **Database:** Serialize complex objects vào JSON column

---

### B??c 4.2: NewtonSoftService Implementation

**Làm gì:** Implement serializer s? d?ng Newtonsoft.Json.

**T?i sao:** Newtonsoft.Json mature h?n, feature-rich h?n System.Text.Json. Support nhi?u scenarios ph?c t?p.

**File:** `src/Infrastructure/Infrastructure/Common/Services/NewtonSoftService.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ECO.WebApi.Infrastructure.Common.Services;

/// <summary>
/// JSON serializer implementation s? d?ng Newtonsoft.Json
/// </summary>
public class NewtonSoftService : ISerializerService
{
    /// <summary>
    /// Deserialize JSON string thành object
  /// </summary>
    public T Deserialize<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text)!;
    }

    /// <summary>
    /// Serialize object thành JSON string v?i custom settings
/// </summary>
    public string Serialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
          // CamelCase property names (firstName thay vì FirstName)
         ContractResolver = new CamelCasePropertyNamesContractResolver(),
            
 // Ignore null values (không serialize properties null)
            NullValueHandling = NullValueHandling.Ignore,
            
      // Enum as string (thay vì number)
    Converters = new List<JsonConverter>
            {
       new StringEnumConverter { CamelCaseText = true }
 }
        });
    }

 /// <summary>
    /// Serialize object thành JSON string v?i type c? th?
    /// </summary>
    public string Serialize<T>(T obj, Type type)
    {
        return JsonConvert.SerializeObject(obj, type, new JsonSerializerSettings());
    }
}
```

**Gi?i thích JsonSerializerSettings:**

**CamelCasePropertyNamesContractResolver:**
- Property names ? camelCase: `firstName` thay vì `FirstName`
- Chu?n JSON API

**NullValueHandling.Ignore:**
- Không serialize properties null
- Gi?m response size
- Cleaner JSON

**StringEnumConverter:**
- Enum as string: `"active"` thay vì `1`
- D? ??c, d? debug
- API-friendly

**Example:**
```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ProductStatus Status { get; set; }  // Enum
    public string? Description { get; set; }   // Nullable
}

// Input
var product = new Product 
{ 
    Id = Guid.NewGuid(), 
    Name = "iPhone", 
    Status = ProductStatus.Active,
    Description = null 
};

// Serialize
var json = _serializer.Serialize(product);

// Output
{"id":"...","name":"iPhone","status":"active"}
// (description b? b? vì null, status là "active" thay vì 1)
```

**L?i ích:**
- ? API-friendly format
- ? Smaller response size
- ? Human-readable
- ? Easy debugging

---

### B??c 4.3: Register Serializer Service

**Làm gì:** Register serializer service vào DI container.

**File:** `src/Infrastructure/Infrastructure/Common/Startup.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Infrastructure.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Common;

internal static class Startup
{
    /// <summary>
    /// Register common services
    /// </summary>
    internal static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        // Register Serializer as Transient
        services.AddTransient<ISerializerService, NewtonSoftService>();

        return services;
  }
}
```

**Gi?i thích:**
- `Transient` lifetime ? lightweight, stateless service
- Extension method pattern ?? modular registration

---

## 5. Event Publisher Service

### B??c 5.1: IEvent Marker Interface

**Làm gì:** Marker interface cho t?t c? domain events.

**T?i sao:** ?ánh d?u class là domain event, support generic event handling.

**File:** `src/Core/Shared/Events/IEvent.cs`

```csharp
namespace ECO.WebApi.Shared.Events;

/// <summary>
/// Marker interface cho t?t c? domain events
/// Domain events represent something that happened in the domain
/// </summary>
public interface IEvent
{
}
```

**Gi?i thích:**
- Marker interface ? không có methods
- T?t c? domain events ph?i implement
- ? Shared layer ? có th? dùng ? m?i layer

**T?i sao trong Shared layer:**
- Domain events là contract
- Application và Infrastructure ??u c?n
- No dependencies

---

### B??c 5.2: EventNotification Wrapper

**Làm gì:** Wrapper class ?? wrap IEvent thành INotification (MediatR).

**T?i sao:** Domain events (`IEvent`) không ph? thu?c MediatR. Wrapper ?? publish qua MediatR.

**File:** `src/Core/Application/Common/Events/EventNotification.cs`

```csharp
using ECO.WebApi.Shared.Events;
using MediatR;

namespace ECO.WebApi.Application.Common.Events;

/// <summary>
/// Wrapper class ?? wrap IEvent thành INotification (MediatR)
/// Gi? cho Domain layer không ph? thu?c MediatR
/// </summary>
public class EventNotification<TEvent> : INotification
    where TEvent : IEvent
{
    public EventNotification(TEvent @event) => Event = @event;

  /// <summary>
    /// Domain event ???c wrap
    /// </summary>
    public TEvent Event { get; }
}
```

**Gi?i thích:**
- `INotification` ? MediatR notification interface
- Wrap `IEvent` thành `INotification` ?? publish qua MediatR
- Generic class ? support any event type

**T?i sao c?n wrapper:**
- Domain events (`IEvent`) **không ph? thu?c** MediatR ? Clean Architecture
- MediatR c?n `INotification` ?? publish ? Infrastructure concern
- Wrapper tách bi?t Domain và Infrastructure ? Separation of concerns

**Design pattern:** Adapter Pattern

---

### B??c 5.3: IEventPublisher Interface

**Làm gì:** Interface ?? publish domain events.

**T?i sao:** Application layer c?n publish events, nh?ng không bi?t implementation (MediatR).

**File:** `src/Core/Application/Common/Events/IEventPublisher.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Shared.Events;

namespace ECO.WebApi.Application.Common.Events;

/// <summary>
/// Interface ?? publish domain events
/// Implementation s? dùng MediatR ?? dispatch events ??n handlers
/// </summary>
public interface IEventPublisher : ITransientService
{
    /// <summary>
    /// Publish domain event
    /// </summary>
    Task PublishAsync(IEvent @event);
}
```

**Gi?i thích:**
- `Transient` lifetime
- Accept `IEvent` (domain abstraction)
- Async method ? await handlers

**L?i ích:**
- ? Application layer không ph? thu?c MediatR
- ? D? mock cho testing
- ? D? thay ??i implementation

---

### B??c 5.4: EventPublisher Implementation

**Làm gì:** Implement EventPublisher s? d?ng MediatR ?? dispatch events.

**T?i sao:** MediatR handle event routing và invocation. Chúng ta ch? c?n wrap events.

**File:** `src/Infrastructure/Infrastructure/Common/Events/EventPublisher.cs`

```csharp
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Common.Events;

/// <summary>
/// Implementation c?a IEventPublisher s? d?ng MediatR
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly IPublisher _mediator;

 public EventPublisher(ILogger<EventPublisher> logger, IPublisher mediator) =>
     (_logger, _mediator) = (logger, mediator);

    /// <summary>
    /// Publish domain event qua MediatR
    /// </summary>
    public Task PublishAsync(IEvent @event)
    {
        // Log event type ?? tracking
        _logger.LogInformation("Publishing Event: {EventType}", @event.GetType().Name);
    
     // Wrap event thành EventNotification và publish qua MediatR
        return _mediator.Publish(CreateEventNotification(@event));
    }

    /// <summary>
/// Create EventNotification&lt;TEvent&gt; t? IEvent b?ng reflection
    /// Vì runtime type, không th? dùng generic compile-time
    /// </summary>
  private static INotification CreateEventNotification(IEvent @event)
    {
    // Step 1: L?y runtime type c?a event (ví d?: ProductCreatedEvent)
  var eventType = @event.GetType();
      
        // Step 2: T?o generic type EventNotification<ProductCreatedEvent>
        var notificationType = typeof(EventNotification<>).MakeGenericType(eventType);
        
        // Step 3: Create instance: new EventNotification<ProductCreatedEvent>(event)
        var instance = Activator.CreateInstance(notificationType, @event);

        // Step 4: Cast v? INotification
   return (INotification)instance!;
    }
}
```

**Gi?i thích Reflection Magic:**

```csharp
// Input: ProductCreatedEvent (implements IEvent)
var @event = new ProductCreatedEvent(product);

// Step 1: Get runtime type
var eventType = @event.GetType(); 
// Result: typeof(ProductCreatedEvent)

// Step 2: Make generic type
var notificationType = typeof(EventNotification<>).MakeGenericType(eventType);
// Result: typeof(EventNotification<ProductCreatedEvent>)

// Step 3: Create instance with constructor parameter
var instance = Activator.CreateInstance(notificationType, @event);
// Result: new EventNotification<ProductCreatedEvent>(event)

// Step 4: Cast to INotification
return (INotification)instance;
// MediatR accepts INotification
```

**T?i sao c?n reflection:**
- `IEvent` là interface ? không bi?t concrete type compile-time
- Runtime type ? ph?i dùng reflection ?? t?o `EventNotification<T>`
- Generic type argument c?n runtime type information

**Performance consideration:**
- Reflection có overhead nh?ng acceptable
- Events không publish th??ng xuyên nh? queries
- Tradeoff ?? gi? clean architecture

---

### B??c 5.5: EventNotificationHandler Base Class

**Làm gì:** Base class ?? d? dàng t?o event handlers.

**T?i sao:** Auto unwrap EventNotification, handlers ch? c?n handle domain event.

**File:** `src/Core/Application/Common/Events/IEventNotificationHandler.cs`

```csharp
using ECO.WebApi.Shared.Events;
using MediatR;

namespace ECO.WebApi.Application.Common.Events;

/// <summary>
/// Interface cho event notification handlers (shorthand)
/// </summary>
public interface IEventNotificationHandler<TEvent> : INotificationHandler<EventNotification<TEvent>>
    where TEvent : IEvent
{
}

/// <summary>
/// Abstract base class cho event notification handlers
/// Auto unwrap EventNotification ?? handlers ch? c?n handle domain event
/// </summary>
public abstract class EventNotificationHandler<TEvent> : INotificationHandler<EventNotification<TEvent>>
    where TEvent : IEvent
{
    /// <summary>
    /// Handle EventNotification (wrapper) - auto called b?i MediatR
    /// </summary>
    public Task Handle(EventNotification<TEvent> notification, CancellationToken cancellationToken) =>
        Handle(notification.Event, cancellationToken);

/// <summary>
    /// Handle domain event (ph?i implement trong derived class)
    /// </summary>
    public abstract Task Handle(TEvent @event, CancellationToken cancellationToken);
}
```

**Gi?i thích:**

**Interface shorthand:**
- `IEventNotificationHandler<ProductCreatedEvent>` thay vì `INotificationHandler<EventNotification<ProductCreatedEvent>>`
- G?n h?n, d? ??c h?n

**Abstract class:**
- Auto unwrap `EventNotification` ? handler ch? c?n handle `TEvent`
- Abstract method ? force derived classes implement
- Template Method pattern

**Usage example:**
```csharp
// ? Không dùng base class - ph?i unwrap manually
public class ProductCreatedHandler : INotificationHandler<EventNotification<ProductCreatedEvent>>
{
    public Task Handle(EventNotification<ProductCreatedEvent> notification, ...)
    {
        var @event = notification.Event; // Unwrap manually
        // Handle event logic
    }
}

// ? Dùng base class - auto unwrap
public class ProductCreatedHandler : EventNotificationHandler<ProductCreatedEvent>
{
    public override Task Handle(ProductCreatedEvent @event, ...)
    {
        // Handle event directly - ?ã unwrap r?i
  _logger.LogInformation("Product created: {Name}", @event.Product.Name);
        return Task.CompletedTask;
    }
}
```

**L?i ích:**
- ? Code g?n h?n
- ? Ít boilerplate
- ? Focus vào business logic

---

### B??c 5.6: Register Event Publisher

**Làm gì:** Register EventPublisher vào DI container.

**File:** `src/Infrastructure/Infrastructure/Common/Startup.cs`

```csharp
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Infrastructure.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Common;

internal static class Startup
{
    internal static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        // Serializer
        services.AddTransient<ISerializerService, NewtonSoftService>();
        
      // Event Publisher
        services.AddTransient<IEventPublisher, EventPublisher>();

   return services;
    }
}
```

**Gi?i thích:**
- `Transient` lifetime ? stateless service
- MediatR auto-scan và register event handlers

---

## 6. Update Infrastructure Startup

### B??c 6.1: Consolidate All Services

**Làm gì:** Update Infrastructure Startup ?? register t?t c? services.

**T?i sao:** Centralized registration, d? maintain.

**File:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
using ECO.WebApi.Infrastructure.Auth;
using ECO.WebApi.Infrastructure.Common;
using ECO.WebApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure;

public static class Startup
{
    /// <summary>
    /// Add all infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        return services
            // Persistence (DbContext, Repositories)
        .AddPersistence()
         
          // CurrentUser service
            .AddCurrentUser()
            
            // Common services (Serializer, EventPublisher)
            .AddCommonServices()
            
    // Routing
          .AddRouting(options => options.LowercaseUrls = true);
    }

    /// <summary>
    /// Use infrastructure middleware
    /// </summary>
    public static IApplicationBuilder UseInfrastructure(
   this IApplicationBuilder builder,
        IConfiguration config)
    {
        return builder
      .UseRouting()
       
            // CurrentUser middleware - SAU UseRouting, TR??C UseAuthentication
         .UseCurrentUserMiddleware()
 
  .UseHttpsRedirection();
    }
}
```

**?? L?u ý th? t? middleware:**
```
1. UseRouting()
2. UseCurrentUserMiddleware()  ? Set current user
3. UseAuthentication()          ? Will add in BUILD_15
4. UseAuthorization()           ? Will add in BUILD_17
5. MapControllers()
```

**Gi?i thích:**
- Fluent interface pattern (.AddX().AddY())
- Modular registration
- Clear middleware order

---

## 7. Testing

### B??c 7.1: Test CurrentUser Service

**Create test handler:**

**File:** `src/Core/Application/Identity/Users/GetMyProfileRequest.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using MediatR;

namespace ECO.WebApi.Application.Identity.Users;

public class GetMyProfileRequest : IRequest<UserDetailDto> { }

public class GetMyProfileHandler : IRequestHandler<GetMyProfileRequest, UserDetailDto>
{
  private readonly ICurrentUser _currentUser;
    private readonly IUserService _userService;

  public GetMyProfileHandler(ICurrentUser currentUser, IUserService userService)
    {
        _currentUser = currentUser;
        _userService = userService;
    }

    public async Task<UserDetailDto> Handle(GetMyProfileRequest request, CancellationToken ct)
    {
        // L?y current user info t? JWT token
 var userId = _currentUser.GetUserId();
   var email = _currentUser.GetUserEmail();
        var isAuthenticated = _currentUser.IsAuthenticated();

        // Get user from database
        var user = await _userService.GetAsync(userId.ToString(), ct);
     
      return user;
    }
}
```

**Test v?i curl:**
```bash
# Step 1: Login ?? l?y token
curl -X POST https://localhost:7001/api/tokens \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@root.com",
    "password": "123Pa$$word!"
  }'

# Step 2: Get token from response, then call API
curl -X GET https://localhost:7001/api/users/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Expected response:**
```json
{
  "id": "xxx-xxx-xxx",
  "firstName": "Admin",
  "lastName": "Root",
  "email": "admin@root.com"
}
```

---

### B??c 7.2: Test Serializer Service

**Create test:**
```csharp
public class SerializerTest
{
    private readonly ISerializerService _serializer;

    public void Test()
    {
        var product = new Product
        {
        Id = Guid.NewGuid(),
            Name = "Test Product",
      Price = 100,
          Status = ProductStatus.Active,
            Description = null
        };

        // Serialize
     var json = _serializer.Serialize(product);
   Console.WriteLine(json);
        // Output: {"id":"...","name":"Test Product","price":100,"status":"active"}

        // Deserialize
        var deserialized = _serializer.Deserialize<Product>(json);
        Assert.Equal(product.Id, deserialized.Id);
    Assert.Equal(product.Name, deserialized.Name);
    }
}
```

---

### B??c 7.3: Test Event Publisher

**Create domain event:**
```csharp
// File: src/Core/Domain/Catalog/Events/ProductCreatedEvent.cs
using ECO.WebApi.Shared.Events;

namespace ECO.WebApi.Domain.Catalog.Events;

public class ProductCreatedEvent : IEvent
{
    public Product Product { get; }

    public ProductCreatedEvent(Product product)
 {
   Product = product;
    }
}
```

**Create event handler:**
```csharp
// File: src/Core/Application/Catalog/Products/EventHandlers/ProductCreatedEventHandler.cs
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Domain.Catalog.Events;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Application.Catalog.Products.EventHandlers;

public class ProductCreatedEventHandler : EventNotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
    _logger = logger;
    }

    public override Task Handle(ProductCreatedEvent @event, CancellationToken ct)
    {
        _logger.LogInformation("Product created: {ProductId} - {ProductName}",
 @event.Product.Id,
            @event.Product.Name);

     // TODO: Send email notification
        // TODO: Update cache
        // TODO: Send webhook
        
        return Task.CompletedTask;
    }
}
```

**Publish event trong handler:**
```csharp
public class CreateProductHandler : IRequestHandler<CreateProductRequest, Guid>
{
    private readonly IRepository<Product> _repository;
    private readonly IEventPublisher _eventPublisher;

    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken ct)
    {
        var product = Product.Create(request.Name, request.Price);
        
      await _repository.AddAsync(product, ct);
        await _repository.SaveChangesAsync(ct);
 
        // Publish event SAU KHI save
        await _eventPublisher.PublishAsync(new ProductCreatedEvent(product));
  
        return product.Id;
    }
}
```

**Expected log:**
```
info: Publishing Event: ProductCreatedEvent
info: Product created: a1b2c3d4-e5f6-... - iPhone 15
```

---

## 8. Summary

### ? ?ã hoàn thành trong b??c này:

**CurrentUser Service:**
- ? `ICurrentUser` interface (GetUserId, GetEmail, IsAuthenticated, etc.)
- ? `ICurrentUserInitializer` interface (SetCurrentUser, SetCurrentUserId)
- ? `ClaimsPrincipalExtensions` (helper methods)
- ? `CurrentUser` implementation (v?i ClaimsPrincipal)
- ? `CurrentUserMiddleware` (auto-set current user)
- ? Service registration (Scoped)

**Serializer Service:**
- ? `ISerializerService` interface (Serialize, Deserialize)
- ? `NewtonSoftService` implementation (Newtonsoft.Json)
- ? Settings: CamelCase, Ignore nulls, Enum as string
- ? Service registration (Transient)

**Event Publisher:**
- ? `IEvent` marker interface (Shared layer)
- ? `EventNotification<TEvent>` wrapper (Application layer)
- ? `IEventPublisher` interface (PublishAsync)
- ? `EventPublisher` implementation (v?i MediatR + reflection)
- ? `EventNotificationHandler<TEvent>` base class
- ? Service registration (Transient)

### ?? Key Concepts:

**CurrentUser:**
- Scoped service ? m?i request m?t instance
- Thread-safe v?i ClaimsPrincipal
- Middleware auto-set t? JWT token
- Fallback cho background jobs

**Serializer:**
- Transient service ? stateless
- JSON serialization v?i custom settings
- API-friendly format (camelCase, no nulls, enum strings)

**Event Publisher:**
- Publish domain events qua MediatR
- Decouple domain logic và side effects
- Multiple handlers cho m?t event
- Reflection ?? support runtime types

### ?? File Structure:

```
src/Core/Application/Common/
??? Interfaces/
?   ??? ICurrentUser.cs
?   ??? ICurrentUserInitializer.cs
?   ??? ISerializerService.cs
??? Events/
    ??? IEventPublisher.cs
    ??? EventNotification.cs
    ??? IEventNotificationHandler.cs

src/Core/Shared/
??? Events/
?   ??? IEvent.cs
??? Authorization/
    ??? ClaimsPrincipalExtensions.cs

src/Infrastructure/Infrastructure/
??? Auth/
?   ??? CurrentUser.cs
?   ??? CurrentUserMiddleware.cs
?   ??? Startup.cs
??? Common/
    ??? Services/
    ? ??? NewtonSoftService.cs
  ??? Events/
    ?   ??? EventPublisher.cs
    ??? Startup.cs
```

---

## 9. Next Steps

**Ti?p theo:** [BUILD_13 - Exception Handling & Middleware](BUILD_13_Exceptions_Middleware.md)

Trong b??c ti?p theo, chúng ta s?:
1. ? T?o Custom Exceptions (NotFoundException, UnauthorizedException, ForbiddenException, ConflictException, InternalServerException)
2. ? T?o ErrorResult model (error response format)
3. ? Implement ExceptionMiddleware (global exception handler)
4. ? Register middleware pipeline

---

**Quay l?i:** [M?c l?c](BUILD_INDEX.md)
