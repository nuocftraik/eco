# Exception Handling & Middleware

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** B??c 12 (Common Services) ?ã hoàn thành

Tài li?u này h??ng d?n xây d?ng Exception Handling System v?i custom exceptions và global exception middleware.

---

## 1. Overview

**Làm gì:** Xây d?ng h? th?ng x? lý exceptions toàn c?c v?i custom exceptions và error responses.

**T?i sao c?n:**
- **Centralized Error Handling:** X? lý t?t c? exceptions ? m?t n?i
- **Consistent Error Format:** Error response format chu?n cho toàn API
- **Better User Experience:** Error messages rõ ràng, d? hi?u
- **Logging:** T? ??ng log errors v?i context (UserId, ErrorId, StackTrace)
- **HTTP Status Codes:** Tr? ?úng status code cho t?ng lo?i error

**Trong b??c này chúng ta s?:**
- ? T?o `ErrorResult` model (error response format)
- ? T?o `CustomException` base class
- ? T?o các derived exceptions (NotFoundException, UnauthorizedException, etc.)
- ? Implement `ExceptionMiddleware` (global exception handler)
- ? Register middleware pipeline

**Real-world example:**
```csharp
// Trong handler
public async Task<ProductDto> Handle(GetProductRequest request, CancellationToken ct)
{
    var product = await _repository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id), ct)
      ?? throw new NotFoundException($"Product with ID {request.Id} was not found.");
    
    return product.Adapt<ProductDto>();
}

// Exception t? ??ng ???c catch b?i ExceptionMiddleware
// Response: { "statusCode": 404, "exception": "Product with ID ... was not found.", "errorId": "..." }
```

---

## 2. Add Required Packages

**File:** `src/Infrastructure/Infrastructure/Infrastructure.csproj`

Packages ?ã có t? b??c tr??c:
- `Serilog` - Logging
- `Newtonsoft.Json` - JSON serialization

**No new packages needed for this step.**

---

## 3. T?o ErrorResult Model

### B??c 3.1: ErrorResult Class

**Làm gì:** T?o model ?? format error responses.

**File:** `src/Infrastructure/Infrastructure/Middleware/ErrorResult.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Middleware;

/// <summary>
/// Model ?? tr? v? error response cho client
/// </summary>
public class ErrorResult
{
    /// <summary>
    /// Danh sách error messages
    /// </summary>
public List<string> Messages { get; set; } = new();

    /// <summary>
    /// Source c?a exception (class và method)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Exception message
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Unique error ID ?? tracking
    /// </summary>
    public string? ErrorId { get; set; }

    /// <summary>
    /// Support message ?? user liên h? support team
    /// </summary>
    public string? SupportMessage { get; set; }

    /// <summary>
  /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
}
```

**Gi?i thích:**
- `Messages`: List các error messages (dùng cho validation errors)
- `Source`: Class và method gây ra exception
- `Exception`: Exception message chính
- `ErrorId`: Unique ID ?? tracking và debugging
- `SupportMessage`: H??ng d?n user liên h? support
- `StatusCode`: HTTP status code (400, 404, 500, etc.)

**Example response:**
```json
{
  "messages": [],
  "source": "ECO.WebApi.Application.Catalog.Products.GetProductRequestHandler.Handle",
  "exception": "Product with ID 123 was not found.",
  "errorId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "supportMessage": "Provide the ErrorId a1b2c3d4-e5f6-7890-abcd-ef1234567890 to the support team for further analysis.",
  "statusCode": 404
}
```

---

## 4. T?o Custom Exceptions

### B??c 4.1: CustomException Base Class

**Làm gì:** T?o base exception class v?i HttpStatusCode và ErrorMessages.

**File:** `src/Core/Application/Common/Exceptions/CustomException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

/// <summary>
/// Base exception class cho t?t c? custom exceptions
/// </summary>
public class CustomException : Exception
{
  /// <summary>
    /// Danh sách error messages
    /// </summary>
    public List<string>? ErrorMessages { get; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Constructor v?i message, errors, và status code
    /// </summary>
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

**Gi?i thích:**
- K? th?a `Exception` ?? có `Message`, `StackTrace`, etc.
- `ErrorMessages`: List errors (cho validation)
- `StatusCode`: HTTP status code t??ng ?ng
- Default status code: 500 (InternalServerError)

---

### B??c 4.2: NotFoundException

**Làm gì:** Exception khi không tìm th?y entity.

**File:** `src/Core/Application/Common/Exceptions/NotFoundException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

/// <summary>
/// Exception khi không tìm th?y entity
/// HTTP Status Code: 404 Not Found
/// </summary>
public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message, null, HttpStatusCode.NotFound)
    {
    }
}
```

**Usage:**
```csharp
var product = await _repository.FirstOrDefaultAsync(spec, ct)
    ?? throw new NotFoundException($"Product with ID {request.Id} was not found.");
```

---

### B??c 4.3: UnauthorizedException

**Làm gì:** Exception khi user ch?a authenticate.

**File:** `src/Core/Application/Common/Exceptions/UnauthorizedException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

/// <summary>
/// Exception khi user ch?a authenticate
/// HTTP Status Code: 401 Unauthorized
/// </summary>
public class UnauthorizedException : CustomException
{
    public UnauthorizedException(string message)
       : base(message, null, HttpStatusCode.Unauthorized)
    {
    }
}
```

**Usage:**
```csharp
if (!_currentUser.IsAuthenticated())
    throw new UnauthorizedException("You must be logged in to access this resource.");
```

---

### B??c 4.4: ForbiddenException

**Làm gì:** Exception khi user không có permission.

**File:** `src/Core/Application/Common/Exceptions/ForbiddenException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

/// <summary>
/// Exception khi user không có permission
/// HTTP Status Code: 403 Forbidden
/// </summary>
public class ForbiddenException : CustomException
{
    public ForbiddenException(string message)
        : base(message, null, HttpStatusCode.Forbidden)
    {
    }
}
```

**Usage:**
```csharp
if (!_currentUser.IsInRole("Admin"))
    throw new ForbiddenException("You do not have permission to access this resource.");
```

---

### B??c 4.5: ConflictException

**Làm gì:** Exception khi có conflict (duplicate, etc.).

**File:** `src/Core/Application/Common/Exceptions/ConflictException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

/// <summary>
/// Exception khi có conflict (duplicate entity, etc.)
/// HTTP Status Code: 409 Conflict
/// </summary>
public class ConflictException : CustomException
{
    public ConflictException(string message)
        : base(message, null, HttpStatusCode.Conflict)
 {
    }
}
```

**Usage:**
```csharp
if (await _roleManager.RoleExistsAsync(request.Name))
    throw new ConflictException($"Role {request.Name} already exists.");
```

---

### B??c 4.6: InternalServerException

**Làm gì:** Exception cho internal server errors.

**File:** `src/Core/Application/Common/Exceptions/InternalServerException.cs`

```csharp
using System.Net;

namespace ECO.WebApi.Application.Common.Exceptions;

/// <summary>
/// Exception cho internal server errors
/// HTTP Status Code: 500 Internal Server Error
/// </summary>
public class InternalServerException : CustomException
{
    public InternalServerException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.InternalServerError)
    {
    }
}
```

**Usage:**
```csharp
var result = await _roleManager.CreateAsync(role);
if (!result.Succeeded)
    throw new InternalServerException("Register role failed", result.Errors.Select(e => e.Description).ToList());
```

---

## 5. Implement ExceptionMiddleware

### B??c 5.1: ExceptionMiddleware Class

**Làm gì:** Middleware ?? catch t?t c? exceptions và tr? v? error responses.

**File:** `src/Infrastructure/Infrastructure/Middleware/ExceptionMiddleware.cs`

```csharp
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Net;

namespace ECO.WebApi.Infrastructure.Middleware;

/// <summary>
/// Middleware ?? catch và handle t?t c? exceptions
/// </summary>
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
          // 1. L?y user context
            string email = _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous";
 var userId = _currentUser.GetUserId();
         
      // 2. Push context vào Serilog
            if (userId != Guid.Empty)
                LogContext.PushProperty("UserId", userId);
            LogContext.PushProperty("UserEmail", email);

       // 3. Generate unique error ID
        string errorId = Guid.NewGuid().ToString();
LogContext.PushProperty("ErrorId", errorId);
            LogContext.PushProperty("StackTrace", exception.StackTrace);

            // 4. T?o ErrorResult
         var errorResult = new ErrorResult
      {
       Source = exception.TargetSite?.DeclaringType?.FullName,
     Exception = exception.Message.Trim(),
        ErrorId = errorId,
     SupportMessage = $"Provide the ErrorId {errorId} to the support team for further analysis."
          };

            // 5. Handle inner exception (unwrap)
            if (exception is not CustomException && exception.InnerException != null)
     {
     while (exception.InnerException != null)
             {
         exception = exception.InnerException;
  }
            }

            // 6. Handle FluentValidation exceptions
            if (exception is FluentValidation.ValidationException fluentException)
   {
         errorResult.Exception = "One or More Validations failed.";
        foreach (var error in fluentException.Errors)
    {
            errorResult.Messages.Add(error.ErrorMessage);
           }
     }

  // 7. Set status code d?a trên exception type
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

          // 8. Log error
         Log.Error($"{errorResult.Exception} Request failed with Status Code {errorResult.StatusCode} and Error Id {errorId}.");

            // 9. Write error response
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

**Gi?i thích flow:**

**1. L?y user context:**
- User email (ho?c "Anonymous" n?u ch?a login)
- User ID

**2. Push context vào Serilog:**
- UserId, UserEmail, ErrorId, StackTrace
- ?? logs có ?? context khi debug

**3. Generate unique error ID:**
- Dùng Guid ?? có unique ID
- User có th? cung c?p ErrorId cho support team

**4. T?o ErrorResult:**
- Source: Class và method gây ra exception
- Exception: Exception message
- ErrorId: Unique ID
- SupportMessage: H??ng d?n user

**5. Handle inner exception:**
- Unwrap inner exceptions
- L?y exception message g?c

**6. Handle FluentValidation:**
- Validation errors t? FluentValidation
- Add t?t c? error messages vào `Messages` list

**7. Set status code:**
- `CustomException`: L?y `StatusCode` t? exception
- `KeyNotFoundException`: 404 Not Found
- `ValidationException`: 400 Bad Request
- Default: 500 Internal Server Error

**8. Log error:**
- Log v?i Serilog
- Include ErrorId ?? tracking

**9. Write error response:**
- Set `ContentType` = "application/json"
- Set `StatusCode`
- Serialize `ErrorResult` và write vào response

---

## 6. Register Middleware

### B??c 6.1: Middleware Registration

**File:** `src/Infrastructure/Infrastructure/Middleware/Startup.cs`

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Middleware;

internal static class Startup
{
    /// <summary>
    /// Add middleware services
    /// </summary>
    internal static IServiceCollection AddExceptionMiddleware(this IServiceCollection services) =>
        services.AddScoped<ExceptionMiddleware>();

    /// <summary>
    /// Use exception middleware
    /// </summary>
    internal static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
     app.UseMiddleware<ExceptionMiddleware>();
}
```

---

### B??c 6.2: Update Infrastructure Startup

**File:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
using ECO.WebApi.Infrastructure.Middleware;
// ... other usings

namespace ECO.WebApi.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration config)
    {
      return services
     .AddPersistence()
            // ... other services
 .AddExceptionMiddleware()  // ? Add này
       .AddRouting(options => options.LowercaseUrls = true);
    }

    public static IApplicationBuilder UseInfrastructure(
        this IApplicationBuilder builder, 
        IConfiguration config)
  {
        return builder
       .UseExceptionMiddleware()  // ? PH?I ??U TIÊN
     .UseRouting()
        .UseCurrentUserMiddleware()
            .UseHttpsRedirection()
            .UseAuthentication()
       .UseAuthorization();
    }
}
```

**?? L?u ý th? t? middleware:**
```
1. UseExceptionMiddleware()  ? PH?I ??U TIÊN (?? catch t?t c? exceptions)
2. UseRouting()
3. UseCurrentUserMiddleware()
4. UseAuthentication()
5. UseAuthorization()
6. MapControllers() / MapEndpoints()
```

---

## 7. Testing

### B??c 7.1: Test NotFoundException

**Request:**
```bash
curl -X GET https://localhost:7001/api/products/00000000-0000-0000-0000-000000000001
```

**Expected Response (404):**
```json
{
  "messages": [],
  "source": "ECO.WebApi.Application.Catalog.Products.GetProductRequestHandler.Handle",
  "exception": "Product with ID 00000000-0000-0000-0000-000000000001 was not found.",
  "errorId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "supportMessage": "Provide the ErrorId a1b2c3d4-... to the support team for further analysis.",
  "statusCode": 404
}
```

---

### B??c 7.2: Test ValidationException

**Request:**
```bash
curl -X POST https://localhost:7001/api/products \
  -H "Content-Type: application/json" \
-d '{
    "name": "",
    "price": -100
  }'
```

**Expected Response (400):**
```json
{
  "messages": [
    "Product name is required.",
    "Price must be greater than 0."
  ],
"source": null,
  "exception": "One or More Validations failed.",
  "errorId": "b2c3d4e5-f6g7-8901-bcde-f12345678901",
  "supportMessage": "Provide the ErrorId b2c3d4e5-... to the support team for further analysis.",
  "statusCode": 400
}
```

---

### B??c 7.3: Test UnauthorizedException

**Request (without token):**
```bash
curl -X GET https://localhost:7001/api/users/me
```

**Expected Response (401):**
```json
{
  "messages": [],
  "source": null,
  "exception": "You must be logged in to access this resource.",
  "errorId": "c3d4e5f6-g7h8-9012-cdef-123456789012",
  "supportMessage": "Provide the ErrorId c3d4e5f6-... to the support team for further analysis.",
  "statusCode": 401
}
```

---

### B??c 7.4: Test ConflictException

**Request:**
```bash
curl -X POST https://localhost:7001/api/roles \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Admin"
  }'
```

**Expected Response (409):**
```json
{
  "messages": [],
  "source": "ECO.WebApi.Infrastructure.Identity.RoleService.CreateOrUpdateAsync",
  "exception": "Role Admin already exists.",
  "errorId": "d4e5f6g7-h8i9-0123-defg-234567890123",
  "supportMessage": "Provide the ErrorId d4e5f6g7-... to the support team for further analysis.",
  "statusCode": 409
}
```

---

## 8. Common Issues

### Issue 1: "Response has already started"

**Tri?u ch?ng:**
```
Can't write error response. Response has already started.
```

**Nguyên nhân:** 
Response ?ã ???c g?i m?t ph?n (headers ho?c body) tr??c khi exception x?y ra.

**Gi?i pháp:**
- ??m b?o exception x?y ra TR??C khi `await next()` g?i response
- Ho?c dùng `response.HasStarted` ?? check (?ã có trong code)

---

### Issue 2: Inner exceptions không ???c log

**Tri?u ch?ng:**
Exception message không rõ ràng, thi?u details.

**Nguyên nhân:**
Inner exception không ???c unwrap.

**Gi?i pháp:**
Code ?ã handle unwrap inner exceptions:
```csharp
if (exception is not CustomException && exception.InnerException != null)
{
    while (exception.InnerException != null)
    {
        exception = exception.InnerException;
  }
}
```

---

### Issue 3: ValidationException không có messages

**Tri?u ch?ng:**
Validation errors không hi?n trong response.

**Nguyên nhân:**
FluentValidation exceptions không ???c handle ?úng.

**Gi?i pháp:**
Code ?ã handle FluentValidation:
```csharp
if (exception is FluentValidation.ValidationException fluentException)
{
    errorResult.Exception = "One or More Validations failed.";
    foreach (var error in fluentException.Errors)
    {
   errorResult.Messages.Add(error.ErrorMessage);
    }
}
```

---

## 9. Best Practices

### ? Do's (Nên làm)

**1. Throw specific exceptions:**
```csharp
// ? ?úng - Specific exception
throw new NotFoundException($"Product {id} not found.");

// ? Sai - Generic exception
throw new Exception("Not found");
```

**2. Include context in message:**
```csharp
// ? ?úng - Include ID/name
throw new NotFoundException($"Product with ID {request.Id} was not found.");

// ? Sai - Generic message
throw new NotFoundException("Product not found");
```

**3. Use proper status codes:**
```csharp
// ? ?úng
NotFoundException ? 404
UnauthorizedException ? 401
ForbiddenException ? 403
ConflictException ? 409

// ? Sai - Dùng sai status code
throw new CustomException("Not found", null, HttpStatusCode.OK); // 200
```

---

### ? Don'ts (Không nên làm)

**1. Catch exceptions trong handlers:**
```csharp
// ? Sai - Catch trong handler
public async Task<ProductDto> Handle(...)
{
    try
    {
        var product = await _repository.FirstOrDefaultAsync(spec);
        if (product == null) return null; // Sai!
    }
    catch (Exception ex)
    {
        // Log và swallow exception - Sai!
        return null;
  }
}

// ? ?úng - Throw exception, ?? middleware handle
public async Task<ProductDto> Handle(...)
{
    var product = await _repository.FirstOrDefaultAsync(spec)
        ?? throw new NotFoundException($"Product {id} not found.");
    
    return product.Adapt<ProductDto>();
}
```

**2. Return null thay vì throw exception:**
```csharp
// ? Sai
public async Task<ProductDto> GetProduct(Guid id)
{
    var product = await _repository.GetByIdAsync(id);
    if (product == null) return null; // Client không bi?t l?i gì
}

// ? ?úng
public async Task<ProductDto> GetProduct(Guid id)
{
    var product = await _repository.GetByIdAsync(id)
        ?? throw new NotFoundException($"Product {id} not found.");
    
    return product.Adapt<ProductDto>();
}
```

**3. Expose sensitive information:**
```csharp
// ? Sai - Expose connection string
throw new Exception($"Database connection failed: {connectionString}");

// ? ?úng - Generic message
throw new InternalServerException("Database connection failed");
```

---

### ?? Tips

**1. Use ErrorId for debugging:**
- User báo l?i ? Cung c?p ErrorId
- Support team search logs theo ErrorId
- Có ?? context ?? debug (UserId, StackTrace, etc.)

**2. Localization:**
Có th? extend ?? support multiple languages:
```csharp
// Future enhancement
public class NotFoundException : CustomException
{
    public NotFoundException(string messageKey, params object[] args)
        : base(_localizer[messageKey, args], null, HttpStatusCode.NotFound)
    {
    }
}
```

**3. Custom error codes:**
Có th? thêm error codes ngoài HTTP status codes:
```csharp
public class ErrorResult
{
    public string? ErrorCode { get; set; } // "PRODUCT_NOT_FOUND", "INVALID_PAYMENT"
    // ... existing properties
}
```

---

## 10. Summary

### ? ?ã hoàn thành trong b??c này:

**Exception Models:**
- ? `ErrorResult` model (error response format)
- ? `CustomException` base class
- ? `NotFoundException` (404)
- ? `UnauthorizedException` (401)
- ? `ForbiddenException` (403)
- ? `ConflictException` (409)
- ? `InternalServerException` (500)

**Middleware:**
- ? `ExceptionMiddleware` implementation
- ? Global exception handling
- ? Logging v?i context (UserId, ErrorId, StackTrace)
- ? Proper HTTP status codes

**Features:**
- ? Centralized error handling
- ? Consistent error format
- ? Support FluentValidation errors
- ? Unwrap inner exceptions
- ? Unique ErrorId for tracking

### ?? Key Concepts:

**CustomException:**
- Base class cho t?t c? custom exceptions
- Có `StatusCode` và `ErrorMessages`
- K? th?a t? `Exception`

**ExceptionMiddleware:**
- Catch t?t c? exceptions
- Convert thành `ErrorResult`
- Log v?i full context
- Tr? v? JSON response

**Error Flow:**
```
Handler throws exception
    ?
ExceptionMiddleware catches
    ?
Create ErrorResult (with ErrorId)
    ?
Log with context
    ?
Return JSON response (with status code)
```

### ?? File Structure:

```
src/Core/Application/Common/Exceptions/
??? CustomException.cs
??? NotFoundException.cs
??? UnauthorizedException.cs
??? ForbiddenException.cs
??? ConflictException.cs
??? InternalServerException.cs

src/Infrastructure/Infrastructure/Middleware/
??? ErrorResult.cs
??? ExceptionMiddleware.cs
??? Startup.cs
```

---

## 11. Next Steps

**Ti?p theo:** [BUILD_14 - Validation Behavior](BUILD_14_Validation_Behavior.md)

Trong b??c ti?p theo, chúng ta s?:
1. ? Setup FluentValidation
2. ? T?o `ValidationBehavior` (MediatR pipeline)
3. ? Validator examples
4. ? Auto-register validators

---

**Quay l?i:** [M?c l?c](BUILD_INDEX.md)
