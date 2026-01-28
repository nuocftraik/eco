# ECO.WebApi - API Patterns Memory

> 🧠 **Purpose**: RESTful API design patterns and conventions
> 📅 **Last Updated**: 2026-01-28
> 👤 **Maintained By**: vuongnv1206

---

## API Controller Structure

### Base Controller
```csharp
namespace ECO.WebApi.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected readonly IMediator _mediator;

    public BaseApiController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
```

### Standard CRUD Controller
```csharp
namespace ECO.WebApi.Host.Controllers.Catalog;

/// <summary>
/// Products management endpoints
/// </summary>
[Route("api/catalog/products")]
public class ProductsController : BaseApiController
{
    public ProductsController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [MustHavePermission(ECOAction.View, ECOFunction.Products)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdRequest { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Search products with filters and pagination
    /// </summary>
    [HttpPost("search")]
    [MustHavePermission(ECOAction.Search, ECOFunction.Products)]
    [ProducesResponseType(typeof(PaginatedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAsync(SearchProductsRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [MustHavePermission(ECOAction.Create, ECOFunction.Products)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CreateProductRequest request)
    {
        var productId = await _mediator.Send(request);
        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { id = productId },
            productId);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [MustHavePermission(ECOAction.Update, ECOFunction.Products)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        if (id != request.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(request);
        return NoContent();
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [MustHavePermission(ECOAction.Delete, ECOFunction.Products)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _mediator.Send(new DeleteProductRequest { Id = id });
        return NoContent();
    }
}
```

---

## HTTP Method & Status Code Patterns

### GET Requests
```csharp
✅ GET /api/catalog/products/{id}
   → 200 OK (ProductDto)
   → 404 Not Found

✅ GET /api/catalog/products
   → 200 OK (List<ProductDto>)

✅ POST /api/catalog/products/search
   → 200 OK (PaginatedResult<ProductDto>)

❌ GET /api/catalog/products/search?keyword=abc&categoryId=123...
   // Don't use query string for complex filters
```

### POST Requests
```csharp
✅ POST /api/catalog/products
   Body: CreateProductRequest
   → 201 Created (new resource ID)
   → Location header: /api/catalog/products/{id}
   → 400 Bad Request (validation errors)

❌ POST /api/catalog/products/create
   // "create" is redundant
```

### PUT Requests
```csharp
✅ PUT /api/catalog/products/{id}
   Body: UpdateProductRequest (full entity)
   → 204 No Content
   → 404 Not Found
   → 400 Bad Request (ID mismatch or validation)

❌ POST /api/catalog/products/{id}/update
   // Use PUT, not POST for updates
```

### PATCH Requests
```csharp
✅ PATCH /api/catalog/products/{id}/price
   Body: { "price": 99.99 }
   → 204 No Content
   → 404 Not Found

// Only for partial updates, specific fields
```

### DELETE Requests
```csharp
✅ DELETE /api/catalog/products/{id}
   → 204 No Content
   → 404 Not Found

❌ POST /api/catalog/products/{id}/delete
   // Use DELETE verb
```

---

## Routing Patterns

### Resource Naming
```
✅ /api/catalog/products          (plural, lowercase)
✅ /api/catalog/categories
✅ /api/ordering/orders

❌ /api/Product                    (singular, PascalCase)
❌ /api/catalog/product-list      (kebab-case with redundant suffix)
```

### Nested Resources
```
✅ /api/catalog/categories/{categoryId}/products
   // Get products in a category

✅ /api/ordering/orders/{orderId}/items
   // Get items in an order

❌ /api/catalog/products/by-category/{categoryId}
   // Use nested resource instead
```

### Actions on Resources
```
✅ POST /api/catalog/products/{id}/activate
   // Specific business action

✅ POST /api/ordering/orders/{id}/cancel
   // Specific business action

❌ GET /api/catalog/products/activate/{id}
   // Actions should be POST
```

---

## Request/Response Patterns

### Search/List Requests
```csharp
public class SearchProductsRequest : IRequest<PaginatedResult<ProductDto>>
{
    // Filters
    public string? Keyword { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }

    // Pagination (from PaginationFilter base)
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? OrderBy { get; set; }
    public bool Descending { get; set; }
}

// Response
public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

### Create Requests
```csharp
public class CreateProductRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
}

// Response: Guid (new entity ID)
```

### Update Requests
```csharp
public class UpdateProductRequest : IRequest<Guid>
{
    public Guid Id { get; set; }  // Must match route parameter
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
}

// Response: 204 No Content (or Guid)
```

### Delete Requests
```csharp
public class DeleteProductRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
}

// Response: 204 No Content
```

---

## Error Response Pattern

### Standard Error Response
```csharp
public class ErrorResult
{
    public List<string> Messages { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Exception { get; set; } = string.Empty;
    public string ErrorId { get; set; } = string.Empty;
    public string SupportMessage { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
```

### Example Error Responses
```json
// 404 Not Found
{
  "messages": ["Product with ID 123e4567-e89b-12d3-a456-426614174000 not found"],
  "statusCode": 404,
  "errorId": "abc123"
}

// 400 Bad Request (Validation)
{
  "messages": [
    "Product Name is required",
    "Price must be greater than 0"
  ],
  "statusCode": 400,
  "errorId": "def456"
}

// 401 Unauthorized
{
  "messages": ["You are not authenticated"],
  "statusCode": 401
}

// 403 Forbidden
{
  "messages": ["You do not have permission to access this resource"],
  "statusCode": 403
}

// 500 Internal Server Error
{
  "messages": ["An error occurred while processing your request"],
  "statusCode": 500,
  "errorId": "ghi789",
  "supportMessage": "Please contact support with error ID ghi789"
}
```

---

## Authorization Patterns

### Permission-based
```csharp
[MustHavePermission(ECOAction.View, ECOFunction.Products)]
public async Task<IActionResult> GetByIdAsync(Guid id)

[MustHavePermission(ECOAction.Create, ECOFunction.Products)]
public async Task<IActionResult> CreateAsync(CreateProductRequest request)
```

### Role-based
```csharp
[Authorize(Roles = ECORoles.Admin)]
public async Task<IActionResult> AdminOnlyEndpoint()
```

### Resource-based (in handler)
```csharp
public async Task<ProductDto> Handle(GetProductByIdRequest request, CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(request.Id, ct);
    
    // Check if user owns this resource
    if (product.CreatedBy != _currentUser.GetUserId() && !_currentUser.IsInRole(ECORoles.Admin))
        throw new ForbiddenException("You do not own this resource");
    
    return product.Adapt<ProductDto>();
}
```

---

## Pagination Patterns

### Request
```csharp
public class PaginationFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public int AdvancedPageNumber
    {
        get => PageNumber < 1 ? 1 : PageNumber;
        set => PageNumber = value;
    }

    public int AdvancedPageSize
    {
        get => PageSize > 100 ? 100 : PageSize < 1 ? 10 : PageSize;
        set => PageSize = value;
    }
}
```

### Response Headers (optional)
```csharp
Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
{
    currentPage = result.CurrentPage,
    totalPages = result.TotalPages,
    totalCount = result.TotalCount,
    pageSize = result.PageSize
}));
```

---

## Sorting Patterns

```csharp
// Request
public string? OrderBy { get; set; }  // "Name", "Price", "CreatedOn"
public bool Descending { get; set; } = false;

// In Specification
if (!string.IsNullOrEmpty(request.OrderBy))
{
    switch (request.OrderBy.ToLower())
    {
        case "name":
            Query.OrderBy(p => p.Name, request.Descending);
            break;
        case "price":
            Query.OrderBy(p => p.Price, request.Descending);
            break;
        case "createdon":
            Query.OrderBy(p => p.CreatedOn, request.Descending);
            break;
        default:
            Query.OrderBy(p => p.Name); // Default sort
            break;
    }
}
```

---

## File Upload Patterns

```csharp
[HttpPost("upload")]
[MustHavePermission(ECOAction.Create, ECOFunction.Products)]
public async Task<IActionResult> UploadProductImageAsync(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("No file uploaded");

    // Validate file type
    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
    if (!allowedTypes.Contains(file.ContentType))
        return BadRequest("Invalid file type");

    // Validate file size (5MB max)
    if (file.Length > 5 * 1024 * 1024)
        return BadRequest("File size exceeds 5MB");

    var result = await _mediator.Send(new UploadProductImageRequest
    {
        File = file
    });

    return Ok(result);
}
```

---

## Bulk Operations Pattern

```csharp
[HttpPost("bulk-create")]
[MustHavePermission(ECOAction.Create, ECOFunction.Products)]
public async Task<IActionResult> BulkCreateAsync(List<CreateProductRequest> requests)
{
    if (requests.Count > 100)
        return BadRequest("Maximum 100 items allowed per bulk operation");

    var result = await _mediator.Send(new BulkCreateProductsRequest
    {
        Products = requests
    });

    return Ok(result);
}
```

---

## Versioning Patterns

### URL Versioning
```csharp
[Route("api/v1/catalog/products")]
public class ProductsV1Controller : BaseApiController

[Route("api/v2/catalog/products")]
public class ProductsV2Controller : BaseApiController
```

### Header Versioning (preferred)
```csharp
[ApiVersion("1.0")]
[Route("api/catalog/products")]
public class ProductsController : BaseApiController
```

---

## Swagger/OpenAPI Documentation

```csharp
/// <summary>
/// Get product by ID
/// </summary>
/// <param name="id">Product unique identifier</param>
/// <returns>Product details</returns>
/// <response code="200">Product found</response>
/// <response code="404">Product not found</response>
[HttpGet("{id:guid}")]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetByIdAsync(Guid id)
```

---

## Best Practices

### ✅ DO:
1. Use meaningful HTTP status codes
2. Return DTOs, never entities
3. Validate inputs with FluentValidation
4. Use authorization attributes
5. Document with XML comments
6. Use ProducesResponseType
7. Handle errors globally (middleware)
8. Use async/await
9. Return 201 Created with Location header
10. Use pagination for lists

### ❌ DON'T:
1. Expose internal implementation details
2. Return different types from same endpoint
3. Use verbs in URLs (use HTTP methods)
4. Return sensitive data
5. Skip authorization
6. Use query strings for complex filters
7. Return 200 OK for everything
8. Expose database exceptions
9. Use synchronous methods
10. Return entire collections without pagination

---

**Last Updated**: 2026-01-28
**Maintained By**: vuongnv1206
