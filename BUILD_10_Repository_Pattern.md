# Repository Pattern và Specification

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn về Repository Pattern sử dụng Ardalis.Specification và Decorator Pattern để tự động thêm Domain Events.

---

## Bước 10.1: Tạo Repository Interfaces

**Làm gì:** Tạo interfaces cho Repository pattern.

**Tại sao:**
- Abstraction layer giữa Application và Infrastructure
- Dễ test (có thể mock)
- Sử dụng Ardalis.Specification để query linh hoạt

**File:** `src/Core/Application/Common/Persistence/IRepository.cs`

```csharp
using Ardalis.Specification;
using ECO.WebApi.Domain.Common.Contracts;

namespace ECO.WebApi.Application.Common.Persistence;

/// <summary>
/// The regular read/write repository for an aggregate root.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
{
}

/// <summary>
/// The read-only repository for an aggregate root.
/// </summary>
public interface IReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{
}

/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
public interface IRepositoryWithEvents<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
{
}
```

**Giải thích:**
- `IRepository<T>`: Read/write repository, kế thừa từ `IRepositoryBase<T>` (Ardalis.Specification)
- `IReadRepository<T>`: Read-only repository
- `IRepositoryWithEvents<T>`: Repository tự động thêm Domain Events

**Tại sao chỉ accept `IAggregateRoot`:**
- Tuân thủ DDD: Chỉ Aggregate Roots được truy cập từ bên ngoài
- Child entities chỉ được truy cập qua Aggregate Root

**Tác dụng:**
- Abstraction: Application layer không phụ thuộc vào EF Core
- Specification pattern: Query linh hoạt, dễ test
- Domain Events: Tự động phát events khi entity thay đổi

---

## Bước 10.2: Implement ApplicationDbRepository

**Làm gì:** Implement repository sử dụng EF Core và Ardalis.Specification.

**File:** `src/Infrastructure/Infrastructure/Persistence/Repository/ApplicationDbRepository.cs`

```csharp
using Ardalis.Specification.EntityFrameworkCore;
using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Domain.Common.Contracts;
using ECO.WebApi.Infrastructure.Persistence.Context;

namespace ECO.WebApi.Infrastructure.Persistence.Repository;

public class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
}
```

**Giải thích:**
- `RepositoryBase<T>`: Base class từ Ardalis.Specification, cung cấp tất cả methods cần thiết
- `ApplicationDbContext`: EF Core DbContext
- Implement cả `IReadRepository<T>` và `IRepository<T>`

**Tác dụng:**
- Sử dụng Ardalis.Specification để query
- Tự động có tất cả methods: `GetByIdAsync`, `ListAsync`, `AddAsync`, etc.

---

## Bước 10.3: Tạo EventAddingRepositoryDecorator

**Làm gì:** Decorator tự động thêm Domain Events khi Add/Update/Delete.

**Tại sao dùng Decorator Pattern:**
- Tách biệt concerns: Repository logic và Event logic
- Có thể bật/tắt events dễ dàng
- Không cần modify repository chính

**File:** `src/Infrastructure/Infrastructure/Persistence/Repository/EventAddingRepositoryDecorator.cs`

```csharp
using Ardalis.Specification;
using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Domain.Common.Contracts;
using ECO.WebApi.Domain.Common.Events;

namespace ECO.WebApi.Infrastructure.Persistence.Repository;

/// <summary>
/// The repository that implements IRepositoryWithEvents.
/// Implemented as a decorator. It only augments the Add,
/// Update and Delete calls where it adds the respective
/// EntityCreated, EntityUpdated or EntityDeleted event
/// before delegating to the decorated repository.
/// </summary>
public class EventAddingRepositoryDecorator<T> : IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    private readonly IRepository<T> _decorated;

    public EventAddingRepositoryDecorator(IRepository<T> decorated) => 
        _decorated = decorated;

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.DomainEvents.Add(EntityCreatedEvent.WithEntity(entity));
        return _decorated.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.DomainEvents.Add(EntityUpdatedEvent.WithEntity(entity));
        return _decorated.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.DomainEvents.Add(EntityDeletedEvent.WithEntity(entity));
        return _decorated.DeleteAsync(entity, cancellationToken);
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.DomainEvents.Add(EntityDeletedEvent.WithEntity(entity));
        }
        return _decorated.DeleteRangeAsync(entities, cancellationToken);
    }

    // Tất cả methods khác chỉ forward đến decorated repository
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _decorated.SaveChangesAsync(cancellationToken);
    
    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull =>
        _decorated.GetByIdAsync(id, cancellationToken);
    
    // ... các methods khác forward tương tự
}
```

**Cách hoạt động:**
1. Decorator nhận `IRepository<T>` làm dependency
2. Khi `AddAsync`, `UpdateAsync`, `DeleteAsync` được gọi:
   - Thêm Domain Event tương ứng vào `entity.DomainEvents`
   - Forward call đến decorated repository
3. Các methods khác (Get, List, etc.) chỉ forward, không thêm events

**Tại sao chỉ thêm events cho Add/Update/Delete:**
- Chỉ các operations thay đổi entity mới cần events
- Read operations không cần events

**Tác dụng:**
- Tự động phát events khi entity thay đổi
- Không cần manually thêm events trong code
- Dễ test: có thể mock `IRepository<T>` riêng

---

## Bước 10.4: Đăng ký Repositories

**Làm gì:** Đăng ký repositories trong DI container.

**File:** `src/Infrastructure/Infrastructure/Persistence/Startup.cs`

```csharp
private static IServiceCollection AddRepositories(this IServiceCollection services)
{
    // Add Repositories
    services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));
    
    // Tìm tất cả Aggregate Roots
    foreach (var aggregateRootType in
        typeof(IAggregateRoot).Assembly.GetExportedTypes()
            .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
            .ToList())
    {
        // Add ReadRepositories (alias cho IRepository)
        services.AddScoped(
            typeof(IReadRepository<>).MakeGenericType(aggregateRootType), 
            sp => sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

        // Decorate với EventAddingRepositoryDecorator và expose as IRepositoryWithEvents
        services.AddScoped(
            typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), 
            sp => Activator.CreateInstance(
                typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
                sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
            ?? throw new InvalidOperationException($"Couldn't create EventAddingRepositoryDecorator for {aggregateRootType.Name}"));
    }

    return services;
}
```

**Giải thích:**
1. Đăng ký `IRepository<T>` → `ApplicationDbRepository<T>`
2. Tìm tất cả Aggregate Roots bằng Reflection
3. Đăng ký `IReadRepository<T>` → alias cho `IRepository<T>`
4. Đăng ký `IRepositoryWithEvents<T>` → `EventAddingRepositoryDecorator<T>` (decorate `IRepository<T>`)

**Tại sao dùng Reflection:**
- Tự động discover tất cả Aggregate Roots
- Không cần đăng ký từng repository một
- Dễ mở rộng: thêm Aggregate Root mới → tự động được đăng ký

**Tác dụng:**
- Tự động đăng ký repositories cho tất cả Aggregate Roots
- Có 3 loại repository: `IRepository`, `IReadRepository`, `IRepositoryWithEvents`
- Dễ sử dụng: inject `IRepositoryWithEvents<T>` nếu cần events

---

## Bước 10.5: Tạo Pagination và Filter Models

**Làm gì:** Tạo models cho pagination, search, filter.

**File 1:** `src/Core/Application/Common/Models/BaseFilter.cs`

```csharp
namespace ECO.WebApi.Application.Common.Models;

public class BaseFilter
{
    public string? Keyword { get; set; }
    public Search? AdvancedSearch { get; set; }
    public Filter? AdvancedFilter { get; set; }
}
```

**File 2:** `src/Core/Application/Common/Models/PaginationFilter.cs`

```csharp
namespace ECO.WebApi.Application.Common.Models;

public class PaginationFilter : BaseFilter
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; } = int.MaxValue;
    public string[]? OrderBy { get; set; }
}

public static class PaginationFilterExtensions
{
    public static bool HasOrderBy(this PaginationFilter filter) =>
        filter.OrderBy?.Any() is true;
}
```

**File 3:** `src/Core/Application/Common/Models/Search.cs`

```csharp
namespace ECO.WebApi.Application.Common.Models;

public class Search
{
    public string? Keyword { get; set; }
    public string[]? Fields { get; set; }
}
```

**File 4:** `src/Core/Application/Common/Models/Filter.cs`

```csharp
namespace ECO.WebApi.Application.Common.Models;

public class Filter
{
    public string? Logic { get; set; }
    public string? Field { get; set; }
    public string? Operator { get; set; }
    public object? Value { get; set; }
    public List<Filter>? Filters { get; set; }
}
```

**Tác dụng:**
- `BaseFilter`: Base class cho tất cả filters
- `PaginationFilter`: Pagination + search + filter
- `Search`: Advanced search với keyword và fields
- `Filter`: Advanced filter với logic (AND/OR), operators (EQ, GT, etc.)

---

## Bước 10.6: Tạo Specification Builder Extensions

**Làm gì:** Tạo extension methods để dễ dàng build specifications.

**File:** `src/Core/Application/Common/Specification/SpecificationBuilderExtensions.cs`

**Các extension methods (đầy đủ code + chú thích ngắn):**

```csharp
using Ardalis.Specification;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Models;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace ECO.WebApi.Application.Common.Specification;

public static class SpecificationBuilderExtensions
{
    // Ghép keyword search + advanced search + advanced filter
    public static ISpecificationBuilder<T> SearchBy<T>(this ISpecificationBuilder<T> query, BaseFilter filter) =>
        query.SearchByKeyword(filter.Keyword)
             .AdvancedSearch(filter.AdvancedSearch)
             .AdvancedFilter(filter.AdvancedFilter);

    // Chuẩn hóa page/pageSize rồi Skip/Take + OrderBy
    public static ISpecificationBuilder<T> PaginateBy<T>(this ISpecificationBuilder<T> query, PaginationFilter filter)
    {
        if (filter.PageNumber <= 0) filter.PageNumber = 1;
        if (filter.PageSize <= 0) filter.PageSize = 10;

        if (filter.PageNumber > 1)
        {
            query = query.Skip((filter.PageNumber - 1) * filter.PageSize);
        }

        return query.Take(filter.PageSize).OrderBy(filter.OrderBy);
    }

    // Keyword search (wrapper của AdvancedSearch)
    public static IOrderedSpecificationBuilder<T> SearchByKeyword<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        string? keyword) =>
        specificationBuilder.AdvancedSearch(new Search { Keyword = keyword });

    // Search nhiều field (support nested: "Category.Name")
    public static IOrderedSpecificationBuilder<T> AdvancedSearch<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Search? search)
    {
        if (!string.IsNullOrEmpty(search?.Keyword))
        {
            if (search.Fields?.Any() is true)
            {
                foreach (string field in search.Fields)
                {
                    var paramExpr = Expression.Parameter(typeof(T));
                    MemberExpression propertyExpr = GetPropertyExpression(field, paramExpr);
                    specificationBuilder.AddSearchPropertyByKeyword(propertyExpr, paramExpr, search.Keyword);
                }
            }
            else
            {
                foreach (var property in typeof(T).GetProperties()
                    .Where(prop => (Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType) is { } propertyType
                        && !propertyType.IsEnum
                        && Type.GetTypeCode(propertyType) != TypeCode.Object))
                {
                    var paramExpr = Expression.Parameter(typeof(T));
                    var propertyExpr = Expression.Property(paramExpr, property);
                    specificationBuilder.AddSearchPropertyByKeyword(propertyExpr, paramExpr, search.Keyword);
                }
            }
        }

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    // Build search expression Contains/StartsWith/EndsWith
    private static void AddSearchPropertyByKeyword<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression propertyExpr,
        ParameterExpression paramExpr,
        string keyword,
        string operatorSearch = FilterOperator.CONTAINS)
    {
        if (propertyExpr is not MemberExpression memberExpr || memberExpr.Member is not PropertyInfo property)
        {
            throw new ArgumentException("propertyExpr must be a property expression.", nameof(propertyExpr));
        }

        string searchTerm = operatorSearch switch
        {
            FilterOperator.STARTSWITH => $"{keyword.ToLower()}%",
            FilterOperator.ENDSWITH => $"%{keyword.ToLower()}",
            FilterOperator.CONTAINS => $"%{keyword.ToLower()}%",
            _ => throw new ArgumentException("operatorSearch is not valid.", nameof(operatorSearch))
        };

        // string: x => x.Prop; non-string: x => x.Prop?.ToString()
        Expression selectorExpr =
            property.PropertyType == typeof(string)
                ? propertyExpr
                : Expression.Condition(
                    Expression.Equal(Expression.Convert(propertyExpr, typeof(object)), Expression.Constant(null, typeof(object))),
                    Expression.Constant(null, typeof(string)),
                    Expression.Call(propertyExpr, "ToString", null, null));

        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        Expression callToLowerMethod = Expression.Call(selectorExpr, toLowerMethod!);
        var selector = Expression.Lambda<Func<T, string>>(callToLowerMethod, paramExpr);

        ((List<SearchExpressionInfo<T>>)specificationBuilder.Specification.SearchCriterias)
            .Add(new SearchExpressionInfo<T>(selector, searchTerm, 1));
    }

    // Advanced filter (logic AND/OR/XOR + operators eq, gt, contains, ...)
    public static IOrderedSpecificationBuilder<T> AdvancedFilter<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Filter? filter)
    {
        if (filter is not null)
        {
            var parameter = Expression.Parameter(typeof(T));
            Expression binaryExpresioFilter;

            if (!string.IsNullOrEmpty(filter.Logic))
            {
                if (filter.Filters is null) throw new CustomException("The Filters attribute is required when declaring a logic");
                binaryExpresioFilter = CreateFilterExpression(filter.Logic, filter.Filters, parameter);
            }
            else
            {
                var filterValid = GetValidFilter(filter);
                binaryExpresioFilter = CreateFilterExpression(filterValid.Field!, filterValid.Operator!, filterValid.Value, parameter);
            }

            ((List<WhereExpressionInfo<T>>)specificationBuilder.Specification.WhereExpressions)
                .Add(new WhereExpressionInfo<T>(Expression.Lambda<Func<T, bool>>(binaryExpresioFilter, parameter)));
        }

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    // Build expression theo operator
    private static Expression CreateFilterExpression(
        Expression memberExpression,
        Expression constantExpression,
        string filterOperator)
    {
        if (memberExpression.Type == typeof(string))
        {
            constantExpression = Expression.Call(constantExpression, "ToLower", null);
            memberExpression = Expression.Call(memberExpression, "ToLower", null);
        }

        return filterOperator switch
        {
            FilterOperator.EQ => Expression.Equal(memberExpression, constantExpression),
            FilterOperator.NEQ => Expression.NotEqual(memberExpression, constantExpression),
            FilterOperator.LT => Expression.LessThan(memberExpression, constantExpression),
            FilterOperator.LTE => Expression.LessThanOrEqual(memberExpression, constantExpression),
            FilterOperator.GT => Expression.GreaterThan(memberExpression, constantExpression),
            FilterOperator.GTE => Expression.GreaterThanOrEqual(memberExpression, constantExpression),
            FilterOperator.CONTAINS => Expression.Call(memberExpression, "Contains", null, constantExpression),
            FilterOperator.STARTSWITH => Expression.Call(memberExpression, "StartsWith", null, constantExpression),
            FilterOperator.ENDSWITH => Expression.Call(memberExpression, "EndsWith", null, constantExpression),
            _ => throw new CustomException("Filter Operator is not valid."),
        };
    }

    // OrderBy/ThenBy parse từ mảng string
    public static IOrderedSpecificationBuilder<T> OrderBy<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        string[]? orderByFields)
    {
        if (orderByFields is not null)
        {
            foreach (var field in ParseOrderBy(orderByFields))
            {
                var paramExpr = Expression.Parameter(typeof(T));

                Expression propertyExpr = paramExpr;
                foreach (string member in field.Key.Split('.'))
                {
                    propertyExpr = Expression.PropertyOrField(propertyExpr, member);
                }

                var keySelector = Expression.Lambda<Func<T, object?>>(
                    Expression.Convert(propertyExpr, typeof(object)),
                    paramExpr);

                ((List<OrderExpressionInfo<T>>)specificationBuilder.Specification.OrderExpressions)
                    .Add(new OrderExpressionInfo<T>(keySelector, field.Value));
            }
        }

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    // Helpers: parse orderBy, parse filter value (enum/guid/datetime), build nested property
    private static Dictionary<string, OrderTypeEnum> ParseOrderBy(string[] orderByFields) =>
        new(orderByFields.Select((orderByfield, index) =>
        {
            string[] fieldParts = orderByfield.Split(' ');
            string field = fieldParts[0];
            bool descending = fieldParts.Length > 1 && fieldParts[1].StartsWith("Desc", StringComparison.OrdinalIgnoreCase);
            var orderBy = index == 0
                ? descending ? OrderTypeEnum.OrderByDescending : OrderTypeEnum.OrderBy
                : descending ? OrderTypeEnum.ThenByDescending : OrderTypeEnum.ThenBy;

            return new KeyValuePair<string, OrderTypeEnum>(field, orderBy);
        }));

    private static MemberExpression GetPropertyExpression(string propertyName, ParameterExpression parameter)
    {
        Expression propertyExpression = parameter;
        foreach (string member in propertyName.Split('.'))
        {
            propertyExpression = Expression.PropertyOrField(propertyExpression, member);
        }
        return (MemberExpression)propertyExpression;
    }

    private static ConstantExpression GeValuetExpression(string field, object? value, Type propertyType)
    {
        if (value == null) return Expression.Constant(null, propertyType);
        if (propertyType.IsEnum)
        {
            string? stringEnum = ((JsonElement)value).GetString();
            if (!Enum.TryParse(propertyType, stringEnum, true, out object? valueparsed))
                throw new CustomException(string.Format("Value {0} is not valid for {1}", value, field));
            return Expression.Constant(valueparsed, propertyType);
        }

        if (propertyType == typeof(Guid))
        {
            string? stringGuid = ((JsonElement)value).GetString();
            if (!Guid.TryParse(stringGuid, out Guid valueparsed))
                throw new CustomException(string.Format("Value {0} is not valid for {1}", value, field));
            return Expression.Constant(valueparsed, propertyType);
        }

        if (propertyType == typeof(string))
        {
            string? text = ((JsonElement)value).GetString();
            return Expression.Constant(text, propertyType);
        }

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        {
            string? text = ((JsonElement)value).GetString();
            return Expression.Constant(ChangeType(text, propertyType), propertyType);
        }

        return Expression.Constant(ChangeType(((JsonElement)value).GetRawText(), propertyType), propertyType);
    }

    public static dynamic? ChangeType(object value, Type conversion)
    {
        var t = conversion;
        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null) return null;
            t = Nullable.GetUnderlyingType(t);
        }
        return Convert.ChangeType(value, t!);
    }

    private static Filter GetValidFilter(Filter filter)
    {
        if (string.IsNullOrEmpty(filter.Field)) throw new CustomException("The field attribute is required when declaring a filter");
        if (string.IsNullOrEmpty(filter.Operator)) throw new CustomException("The Operator attribute is required when declaring a filter");
        return filter;
    }
}
```

**Tóm tắt nhanh:**
- `SearchBy` = keyword + advancedSearch + advancedFilter.
- `PaginateBy` = chuẩn hóa page + skip/take + orderBy.
- `AdvancedSearch` = search đa field (nested được), tự động chọn primitive fields nếu không truyền `Fields`.
- `AdvancedFilter` = build biểu thức LINQ động với logic AND/OR/XOR và operators đầy đủ.
- `OrderBy` = parse mảng string thành OrderBy/ThenBy (hỗ trợ nested field).

**Cách sử dụng:**
```csharp
public class ProductBySearchSpec : Specification<Product>
{
    public ProductBySearchSpec(SearchProductRequest request)
    {
        Query
            .SearchBy(request)  // Search + Filter
            .PaginateBy(request);  // Pagination + OrderBy
    }
}
```

---

## Bước 10.7: Tạo Base Specifications

**Làm gì:** Tạo base specifications để reuse.

**File 1:** `src/Core/Application/Common/Specification/EntitiesByBaseFilterSpec.cs`

```csharp
using Ardalis.Specification;
using ECO.WebApi.Application.Common.Models;

namespace ECO.WebApi.Application.Common.Specification;

public class EntitiesByBaseFilterSpec<T> : Specification<T>
{
    public EntitiesByBaseFilterSpec(BaseFilter filter) =>
        Query.SearchBy(filter);
}

public class EntitiesByBaseFilterSpec<T, TResult> : Specification<T, TResult>
{
    public EntitiesByBaseFilterSpec(BaseFilter filter) =>
        Query.SearchBy(filter);
}
```

**File 2:** `src/Core/Application/Common/Specification/EntitiesByPaginationFilterSpec.cs`

```csharp
using ECO.WebApi.Application.Common.Models;

namespace ECO.WebApi.Application.Common.Specification;

public class EntitiesByPaginationFilterSpec<T> : EntitiesByBaseFilterSpec<T>
{
    public EntitiesByPaginationFilterSpec(PaginationFilter filter)
        : base(filter) =>
        Query.PaginateBy(filter);
}

public class EntitiesByPaginationFilterSpec<T, TResult> : EntitiesByBaseFilterSpec<T, TResult>
{
    public EntitiesByPaginationFilterSpec(PaginationFilter filter)
        : base(filter) =>
        Query.PaginateBy(filter);
}
```

**Tác dụng:**
- `EntitiesByBaseFilterSpec`: Base spec với search + filter
- `EntitiesByPaginationFilterSpec`: Base spec với search + filter + pagination

**Cách sử dụng:**
```csharp
public class ProductBySearchSpec : EntitiesByPaginationFilterSpec<Product>
{
    public ProductBySearchSpec(SearchProductRequest request)
         : base(request)
    {
        // Thêm custom query logic
        Query.Include(x => x.ProductCategories)
             .ThenInclude(x => x.Category);
             
        if (request.ProductType.HasValue)
        {
            Query.Where(c => c.ProductType == request.ProductType.Value);
        }
    }
}
```

---

## Payload mẫu (Search + Filter + Pagination + OrderBy)

**Request body** (ví dụ tìm sản phẩm):
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "orderBy": ["Name asc", "CreatedOn desc"],
  "keyword": "iphone",
  "advancedSearch": {
    "fields": ["Name", "Code", "Description"],
    "keyword": "pro"
  },
  "advancedFilter": {
    "logic": "and",
    "filters": [
      { "field": "Price", "operator": "gte", "value": 1000 },
      { "field": "Price", "operator": "lte", "value": 3000 },
      { "field": "Status", "operator": "eq", "value": "Active" },
      { "logic": "or", "filters": [
          { "field": "Category.Name", "operator": "contains", "value": "phone" },
          { "field": "Category.Name", "operator": "contains", "value": "tablet" }
        ]
      }
    ]
  }
}
```

**Luồng xử lý:**
1. Controller nhận `SearchProductRequest` (kế thừa `PaginationFilter`).
2. Handler tạo `ProductBySearchSpec` → `Query.SearchBy(request)` + `Query.PaginateBy(request)` và thêm `Include/Where` riêng.
3. Repository `ListAsync(spec)` + `CountAsync(spec)` sinh SQL đã filter/paging/order.
4. Decorator không thêm event (read only), events chỉ cho Add/Update/Delete.

**Lưu ý:**
- `orderBy`: phần tử 1 = `OrderBy`, các phần tử sau = `ThenBy`. Hỗ trợ nested field bằng dấu `.`.
- `advancedFilter`: hỗ trợ `logic` (`and/or/xor`) và `operator` (`eq, neq, gt, gte, lt, lte, contains, startswith, endswith`).
- Enum/Guid/DateTime đều được parse an toàn (nếu sai -> `CustomException`).
- Keyword search mặc định dùng `contains`, bạn có thể đổi operator trong `AddSearchPropertyByKeyword` nếu cần.

---

## Tóm tắt

### Thứ tự thực hiện:

1. **Repository Interfaces** → IRepository, IReadRepository, IRepositoryWithEvents
2. **ApplicationDbRepository** → Implement repository với EF Core
3. **EventAddingRepositoryDecorator** → Decorator tự động thêm Domain Events
4. **Register Repositories** → Đăng ký trong DI container
5. **Pagination/Filter Models** → Models cho search, filter, pagination
6. **Specification Extensions** → Extension methods để build specifications
7. **Base Specifications** → Base specs để reuse

### Điểm quan trọng:

- **Aggregate Root only** → Repositories chỉ accept IAggregateRoot
- **Decorator Pattern** → Tách biệt Repository logic và Event logic
- **Ardalis.Specification** → Query linh hoạt, dễ test
- **Auto-registration** → Tự động đăng ký repositories cho tất cả Aggregate Roots

### Lợi ích:

- **Abstraction** → Application layer không phụ thuộc EF Core
- **Domain Events** → Tự động phát events khi entity thay đổi
- **Flexible Querying** → Specification pattern cho query phức tạp
- **Reusable** → Base specifications dễ reuse

---

**Tiếp theo:** [Common Services](BUILD_11_Common_Services.md)
                                                                                                        