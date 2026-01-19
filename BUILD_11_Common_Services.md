# Common Services - CurrentUser, SerializerService, ExceptionMiddleware, ValidationBehavior

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn về các Common Services cơ bản: CurrentUser, SerializerService, ExceptionMiddleware, và ValidationBehavior.

---

## Bước 11.1: Tạo ICurrentUser và ICurrentUserInitializer

**Làm gì:** Tạo interfaces để lấy thông tin user hiện tại.

**File:** `src/Core/Application/Common/Interfaces/ICurrentUser.cs`

```csharp
using System.Security.Claims;

namespace ECO.WebApi.Application.Common.Interfaces;

public interface ICurrentUser
{
    string? Name { get; }
    Guid GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
    bool IsInRole(string role);
    IEnumerable<Claim>? GetUserClaims();
}
```

**File:** `src/Core/Application/Common/Interfaces/ICurrentUserInitializer.cs`

```csharp
using System.Security.Claims;

namespace ECO.WebApi.Application.Common.Interfaces;

public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user);
    void SetCurrentUserId(string userId);
}
```

**Tác dụng:**
- `ICurrentUser`: Lấy thông tin user hiện tại (Id, Email, Roles, Claims)
- `ICurrentUserInitializer`: Set current user (dùng trong middleware)

---

## Bước 11.2: Implement CurrentUser

**Làm gì:** Implement CurrentUser với ClaimsPrincipal.

**File:** `src/Infrastructure/Infrastructure/Auth/CurrentUser.cs`

```csharp
using System.Security.Claims;
using ECO.WebApi.Application.Common.Interfaces;

namespace ECO.WebApi.Infrastructure.Auth;

public class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private ClaimsPrincipal? _user;
    private Guid _userId = Guid.Empty;

    public string? Name => _user?.Identity?.Name;

    public Guid GetUserId() =>
        IsAuthenticated()
            ? Guid.Parse(_user?.GetUserId() ?? Guid.Empty.ToString())
            : _userId;

    public string? GetUserEmail() =>
        IsAuthenticated()
            ? _user!.GetEmail()
            : string.Empty;

    public bool IsAuthenticated() =>
        _user?.Identity?.IsAuthenticated is true;

    public bool IsInRole(string role) =>
        _user?.IsInRole(role) is true;

    public IEnumerable<Claim>? GetUserClaims() =>
        _user?.Claims;

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        if (_user != null)
        {
            throw new Exception("Method reserved for in-scope initialization");
        }
        _user = user;
    }

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

**Tác dụng:**
- Lưu `ClaimsPrincipal` từ JWT token
- Cung cấp methods để lấy thông tin user
- Thread-safe: mỗi request có instance riêng (scoped)

---

## Bước 11.3: Tạo CurrentUserMiddleware

**Làm gì:** Middleware để set current user từ HttpContext.

**File:** `src/Infrastructure/Infrastructure/Auth/CurrentUserMiddleware.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECO.WebApi.Infrastructure.Auth;

public class CurrentUserMiddleware : IMiddleware
{
    private readonly ICurrentUserInitializer _currentUserInitializer;

    public CurrentUserMiddleware(ICurrentUserInitializer currentUserInitializer) =>
        _currentUserInitializer = currentUserInitializer;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _currentUserInitializer.SetCurrentUser(context.User);
        await next(context);
    }
}
```

**Tác dụng:**
- Set current user từ `HttpContext.User` (từ JWT authentication)
- Phải đặt **trước** authentication middleware
- Mỗi request tự động set current user

**Thứ tự middleware:**
```csharp
app.UseCurrentUser();  // Phải trước UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
```

---

## Bước 11.4: Tạo ISerializerService

**Làm gì:** Interface để serialize/deserialize objects.

**File:** `src/Core/Application/Common/Interfaces/ISerializerService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Interfaces;

public interface ISerializerService : ITransientService
{
    string Serialize<T>(T obj);
    string Serialize<T>(T obj, Type type);
    T Deserialize<T>(string text);
}
```

**Tác dụng:**
- Serialize objects thành JSON string
- Deserialize JSON string thành objects
- Dùng cho caching, logging, etc.

---

## Bước 11.5: Implement NewtonSoftService

**Làm gì:** Implement serializer sử dụng Newtonsoft.Json.

**File:** `src/Infrastructure/Infrastructure/Common/Services/NewtonSoftService.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ECO.WebApi.Infrastructure.Common.Services;

public class NewtonSoftService : ISerializerService
{
    public T Deserialize<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text);
    }

    public string Serialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter() { CamelCaseText = true }
            }
        });
    }

    public string Serialize<T>(T obj, Type type)
    {
        return JsonConvert.SerializeObject(obj, type, new());
    }
}
```

**Tác dụng:**
- CamelCase property names
- Ignore null values
- Enum as string (camelCase)

---

## Bước 11.6: Tạo CustomException và ErrorResult

**Làm gì:** Tạo custom exceptions và error result model.

**File 1:** `src/Core/Application/Common/Exceptions/CustomException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

public class CustomException : Exception
{
    public List<string>? ErrorMessages { get; }
    public HttpStatusCode StatusCode { get; }

    public CustomException(
        string message, 
        List<string>? errors = default, 
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        ErrorMessages = errors;
        StatusCode = statusCode;
    }
}
```

**File 2:** `src/Core/Application/Common/Exceptions/ErrorResult.cs`

```csharp
namespace ECO.WebApi.Application.Common.Exceptions;

public class ErrorResult
{
    public string? Source { get; set; }
    public string Exception { get; set; } = string.Empty;
    public string ErrorId { get; set; } = string.Empty;
    public string SupportMessage { get; set; } = string.Empty;
    public List<string> Messages { get; set; } = new();
    public int StatusCode { get; set; }
}
```

**Tác dụng:**
- `CustomException`: Custom exception với StatusCode và ErrorMessages
- `ErrorResult`: Model để trả về lỗi cho client

---

## Bước 11.7: Tạo ExceptionMiddleware

**Làm gì:** Middleware để handle exceptions và trả về error response.

**File:** `src/Infrastructure/Infrastructure/Middleware/ExceptionMiddleware.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace ECO.WebApi.Infrastructure.Middleware;

internal class ExceptionMiddleware : IMiddleware
{
    private readonly ICurrentUser _currentUser;
    private readonly ISerializerService _jsonSerializer;

    public ExceptionMiddleware(
        ICurrentUser currentUser,
        ISerializerService jsonSerializer)
    {
        _currentUser = currentUser;
        _jsonSerializer = jsonSerializer;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            // Log error với context
            string email = _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous";
            var userId = _currentUser.GetUserId();
            if (userId != Guid.Empty)
                LogContext.PushProperty("UserId", userId);
            LogContext.PushProperty("UserEmail", email);

            string errorId = Guid.NewGuid().ToString();
            LogContext.PushProperty("ErrorId", errorId);
            LogContext.PushProperty("StackTrace", exception.StackTrace);

            var errorResult = new ErrorResult
            {
                Source = exception.TargetSite?.DeclaringType?.FullName,
                Exception = exception.Message.Trim(),
                ErrorId = errorId,
                SupportMessage = $"Provide the ErrorId {errorId} to the support team for further analysis."
            };

            // Handle inner exception
            if (exception is not CustomException && exception.InnerException != null)
            {
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
            }

            // Handle FluentValidation exceptions
            if (exception is FluentValidation.ValidationException fluentException)
            {
                errorResult.Exception = "One or More Validations failed.";
                foreach (var error in fluentException.Errors)
                {
                    errorResult.Messages.Add(error.ErrorMessage);
                }
            }

            // Set status code based on exception type
            switch (exception)
            {
                case CustomException e:
                    errorResult.StatusCode = (int)e.StatusCode;
                    if (e.ErrorMessages is not null)
                    {
                        errorResult.Messages = e.ErrorMessages;
                    }
                    break;

                case KeyNotFoundException:
                    errorResult.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case FluentValidation.ValidationException:
                    errorResult.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            Log.Error($"{errorResult.Exception} Request failed with Status Code {errorResult.StatusCode} and Error Id {errorId}.");

            // Write error response
            var response = context.Response;
            if (!response.HasStarted)
            {
                response.ContentType = "application/json";
                response.StatusCode = errorResult.StatusCode;
                await response.WriteAsync(_jsonSerializer.Serialize(errorResult));
            }
            else
            {
                Log.Warning("Can't write error response. Response has already started.");
            }
        }
    }
}
```

**Tác dụng:**
- Catch tất cả exceptions
- Log với context (UserId, Email, ErrorId)
- Trả về error response với format chuẩn
- Handle các loại exceptions khác nhau (CustomException, ValidationException, etc.)

**Thứ tự middleware:**
```csharp
app.UseExceptionMiddleware();  // Phải đầu tiên
app.UseCurrentUser();
app.UseAuthentication();
```

---

## Bước 11.8: Tạo ValidationBehavior

**Làm gì:** MediatR pipeline behavior để tự động validate requests.

**File:** `src/Infrastructure/Infrastructure/Behaviors/ValidationBehavior.cs`

```csharp
using FluentValidation;
using MediatR;

namespace ECO.WebApi.Infrastructure.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

**Tác dụng:**
- Tự động validate requests trước khi handler chạy
- Nếu validation fail → throw `ValidationException`
- Không cần validate manually trong handlers

**Đăng ký trong Application Startup:**
```csharp
services.AddMediatR(cfg =>
{
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

---

## Tóm tắt

### Thứ tự thực hiện:

1. **ICurrentUser Interfaces** → ICurrentUser, ICurrentUserInitializer
2. **CurrentUser Implementation** → Implement với ClaimsPrincipal
3. **CurrentUserMiddleware** → Set current user từ HttpContext
4. **ISerializerService** → Interface để serialize/deserialize
5. **NewtonSoftService** → Implement với Newtonsoft.Json
6. **CustomException & ErrorResult** → Custom exceptions và error model
7. **ExceptionMiddleware** → Handle exceptions và trả về error response
8. **ValidationBehavior** → MediatR pipeline behavior để validate requests

### Điểm quan trọng:

- **CurrentUserMiddleware** → Phải đặt trước UseAuthentication
- **ExceptionMiddleware** → Phải đặt đầu tiên trong pipeline
- **ValidationBehavior** → Tự động validate tất cả requests
- **Scoped services** → CurrentUser là scoped (mỗi request một instance)

### Lợi ích:

- **Centralized error handling** → Tất cả exceptions được handle ở một nơi
- **Automatic validation** → Không cần validate manually
- **User context** → Dễ dàng lấy thông tin user hiện tại
- **Consistent error format** → Error response format chuẩn

---

**Tiếp theo:** [Infrastructure Services](BUILD_12_Infrastructure_Services.md)
