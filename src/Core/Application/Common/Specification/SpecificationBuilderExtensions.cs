using Ardalis.Specification;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Models;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace ECO.WebApi.Application.Common.Specification;
public static class SpecificationBuilderExtensions
{
    /// <summary>
    /// Extension method để apply tất cả search và filter từ BaseFilter vào specification.
    /// Gọi tuần tự: SearchByKeyword -> AdvancedSearch -> AdvancedFilter
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Specification builder</param>
    /// <param name="filter">BaseFilter chứa Keyword, AdvancedSearch, AdvancedFilter</param>
    /// <returns>ISpecificationBuilder để tiếp tục chain methods</returns>
    public static ISpecificationBuilder<T> SearchBy<T>(this ISpecificationBuilder<T> query, BaseFilter filter) =>
        query
            .SearchByKeyword(filter.Keyword)      // Search đơn giản với keyword trong tất cả fields
            .AdvancedSearch(filter.AdvancedSearch) // Search nâng cao với fields cụ thể
            .AdvancedFilter(filter.AdvancedFilter); // Filter với operators và logic

    /// <summary>
    /// Extension method để apply pagination và ordering vào specification.
    /// Tính toán Skip và Take dựa trên PageNumber và PageSize.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Specification builder</param>
    /// <param name="filter">PaginationFilter chứa PageNumber, PageSize, OrderBy</param>
    /// <returns>ISpecificationBuilder để tiếp tục chain methods</returns>
    public static ISpecificationBuilder<T> PaginateBy<T>(this ISpecificationBuilder<T> query, PaginationFilter filter)
    {
        // Validate và set default cho PageNumber nếu <= 0
        if (filter.PageNumber <= 0)
        {
            filter.PageNumber = 1; // Mặc định page đầu tiên
        }

        // Validate và set default cho PageSize nếu <= 0
        if (filter.PageSize <= 0)
        {
            filter.PageSize = 10; // Mặc định 10 items per page
        }

        // Tính toán số records cần skip: (PageNumber - 1) * PageSize
        // Ví dụ: Page 2, PageSize 10 => Skip 10 records
        if (filter.PageNumber > 1)
        {
            query = query.Skip((filter.PageNumber - 1) * filter.PageSize);
        }

        // Apply Take để limit số records và OrderBy để sắp xếp
        return query
            .Take(filter.PageSize)              // Lấy đúng số records theo PageSize
            .OrderBy(filter.OrderBy);           // Sắp xếp theo OrderBy fields
    }

    /// <summary>
    /// Extension method để search đơn giản với keyword trong tất cả fields.
    /// Wrapper method gọi AdvancedSearch với Search object chỉ có Keyword.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="specificationBuilder">Specification builder</param>
    /// <param name="keyword">Keyword để search (có thể null)</param>
    /// <returns>IOrderedSpecificationBuilder để tiếp tục chain methods</returns>
    public static IOrderedSpecificationBuilder<T> SearchByKeyword<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        string? keyword) =>
        specificationBuilder.AdvancedSearch(new Search { Keyword = keyword }); // Tạo Search object và gọi AdvancedSearch

    /// <summary>
    /// Extension method để search nâng cao với keyword trong các fields cụ thể hoặc tất cả fields.
    /// Sử dụng Expression Trees để build dynamic search queries.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="specificationBuilder">Specification builder</param>
    /// <param name="search">Search object chứa Keyword và Fields (optional)</param>
    /// <returns>IOrderedSpecificationBuilder để tiếp tục chain methods</returns>
    public static IOrderedSpecificationBuilder<T> AdvancedSearch<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Search? search)
    {
        // Chỉ search nếu có keyword
        if (!string.IsNullOrEmpty(search?.Keyword))
        {
            // Nếu có chỉ định fields cụ thể
            if (search.Fields?.Any() is true)
            {
                // Search trong các fields được chỉ định (có thể là nested fields như "Category.Name")
                foreach (string field in search.Fields)
                {
                    // Tạo parameter expression: x => (x là parameter)
                    var paramExpr = Expression.Parameter(typeof(T));
                    
                    // Build property expression từ field name (support nested: "Category.Name")
                    // Ví dụ: "Category.Name" => x.Category.Name
                    MemberExpression propertyExpr = GetPropertyExpression(field, paramExpr);

                    // Thêm search criteria cho field này
                    specificationBuilder.AddSearchPropertyByKeyword(propertyExpr, paramExpr, search.Keyword);
                }
            }
            else
            {
                // Nếu không chỉ định fields, search trong TẤT CẢ fields (chỉ first level)
                foreach (var property in typeof(T).GetProperties()
                    .Where(prop => 
                        // Lấy underlying type nếu là nullable (int? => int)
                        (Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType) is { } propertyType
                        && !propertyType.IsEnum                    // Bỏ qua enum
                        && Type.GetTypeCode(propertyType) != TypeCode.Object)) // Chỉ lấy primitive types (int, string, bool, etc.)
                {
                    // Tạo parameter expression: x => (x là parameter)
                    var paramExpr = Expression.Parameter(typeof(T));
                    
                    // Build property expression: x.Property
                    var propertyExpr = Expression.Property(paramExpr, property);

                    // Thêm search criteria cho property này
                    specificationBuilder.AddSearchPropertyByKeyword(propertyExpr, paramExpr, search.Keyword);
                }
            }
        }

        // Trả về OrderedSpecificationBuilder để có thể chain thêm methods
        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    /// <summary>
    /// Private helper method để thêm search criteria cho một property cụ thể.
    /// Build Expression Tree để tạo lambda: x => x.Property.ToLower() và search với pattern.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="specificationBuilder">Specification builder</param>
    /// <param name="propertyExpr">Property expression (x.Property)</param>
    /// <param name="paramExpr">Parameter expression (x)</param>
    /// <param name="keyword">Keyword để search</param>
    /// <param name="operatorSearch">Search operator: CONTAINS, STARTSWITH, ENDSWITH</param>
    private static void AddSearchPropertyByKeyword<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression propertyExpr,
        ParameterExpression paramExpr,
        string keyword,
        string operatorSearch = FilterOperator.CONTAINS)
    {
        // Validate: propertyExpr phải là MemberExpression và Member phải là PropertyInfo
        if (propertyExpr is not MemberExpression memberExpr || memberExpr.Member is not PropertyInfo property)
        {
            throw new ArgumentException("propertyExpr must be a property expression.", nameof(propertyExpr));
        }

        // Tạo search pattern dựa trên operator:
        // STARTSWITH: "keyword%" (bắt đầu với keyword)
        // ENDSWITH: "%keyword" (kết thúc với keyword)
        // CONTAINS: "%keyword%" (chứa keyword) - mặc định
        string searchTerm = operatorSearch switch
        {
            FilterOperator.STARTSWITH => $"{keyword.ToLower()}%",  // "test%" => LIKE 'test%'
            FilterOperator.ENDSWITH => $"%{keyword.ToLower()}",     // "%test" => LIKE '%test'
            FilterOperator.CONTAINS => $"%{keyword.ToLower()}%",    // "%test%" => LIKE '%test%'
            _ => throw new ArgumentException("operatorSearch is not valid.", nameof(operatorSearch))
        };

        // Build selector expression để convert property value thành string:
        // - Nếu property là string: x => x.Property
        // - Nếu property là type khác: x => x.Property == null ? null : x.Property.ToString()
        Expression selectorExpr =
            property.PropertyType == typeof(string)
                ? propertyExpr  // String: dùng trực tiếp x.Property
                : Expression.Condition(
                    // Check null: ((object)x.Property) == null
                    Expression.Equal(
                        Expression.Convert(propertyExpr, typeof(object)), 
                        Expression.Constant(null, typeof(object))),
                    Expression.Constant(null, typeof(string)),  // Nếu null => return null
                    Expression.Call(propertyExpr, "ToString", null, null)); // Nếu không null => ToString()

        // Lấy method ToLower() từ string type
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        
        // Gọi ToLower() trên selector: selectorExpr.ToLower()
        Expression callToLowerMethod = Expression.Call(selectorExpr, toLowerMethod!);

        // Tạo lambda expression: x => x.Property.ToLower() hoặc x => (x.Property ?? "").ToLower()
        var selector = Expression.Lambda<Func<T, string>>(callToLowerMethod, paramExpr);

        // Thêm SearchExpressionInfo vào Specification.SearchCriterias
        // SearchExpressionInfo chứa: selector (lambda), searchTerm (pattern), và weight (1)
        ((List<SearchExpressionInfo<T>>)specificationBuilder.Specification.SearchCriterias)
            .Add(new SearchExpressionInfo<T>(selector, searchTerm, 1));
    }

    /// <summary>
    /// Extension method để apply advanced filter với operators và logic (AND/OR/XOR).
    /// Support nested filters và complex conditions.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="specificationBuilder">Specification builder</param>
    /// <param name="filter">Filter object chứa Field, Operator, Value hoặc Logic + Filters</param>
    /// <returns>IOrderedSpecificationBuilder để tiếp tục chain methods</returns>
    public static IOrderedSpecificationBuilder<T> AdvancedFilter<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Filter? filter)
    {
        if (filter is not null)
        {
            // Tạo parameter expression: x => (x là parameter)
            var parameter = Expression.Parameter(typeof(T));

            Expression binaryExpresioFilter;

            // Nếu có Logic (AND/OR/XOR) => filter phức tạp với nhiều conditions
            if (!string.IsNullOrEmpty(filter.Logic))
            {
                // Validate: phải có Filters khi dùng Logic
                if (filter.Filters is null) 
                    throw new CustomException("The Filters attribute is required when declaring a logic");
                
                // Build expression từ Logic và Filters (recursive)
                // Ví dụ: Logic="and", Filters=[{Field="Name", Operator="eq", Value="Test"}, ...]
                binaryExpresioFilter = CreateFilterExpression(filter.Logic, filter.Filters, parameter);
            }
            else
            {
                // Filter đơn giản: chỉ có Field, Operator, Value
                // Validate: Field và Operator phải có
                var filterValid = GetValidFilter(filter);
                
                // Build expression: x.Field Operator Value
                // Ví dụ: x.Name == "Test" hoặc x.Price > 100
                binaryExpresioFilter = CreateFilterExpression(
                    filterValid.Field!,      // Field name (có thể nested: "Category.Name")
                    filterValid.Operator!,   // Operator: eq, gt, contains, etc.
                    filterValid.Value,       // Value để compare
                    parameter);
            }

            // Thêm WhereExpressionInfo vào Specification.WhereExpressions
            // Convert binaryExpression thành lambda: x => binaryExpression
            ((List<WhereExpressionInfo<T>>)specificationBuilder.Specification.WhereExpressions)
                .Add(new WhereExpressionInfo<T>(
                    Expression.Lambda<Func<T, bool>>(binaryExpresioFilter, parameter)));
        }

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    /// <summary>
    /// Private helper method để build filter expression từ Logic và Filters (recursive).
    /// Hỗ trợ nested filters với logic AND/OR/XOR.
    /// </summary>
    /// <param name="logic">Logic operator: "and", "or", "xor"</param>
    /// <param name="filters">List các Filter objects</param>
    /// <param name="parameter">Parameter expression (x)</param>
    /// <returns>Binary expression kết hợp tất cả filters với logic</returns>
    private static Expression CreateFilterExpression(
        string logic,
        IEnumerable<Filter> filters,
        ParameterExpression parameter)
    {
        Expression filterExpression = default!; // Expression kết quả

        // Duyệt qua từng filter trong list
        foreach (var filter in filters)
        {
            Expression bExpresionFilter;

            // Nếu filter này có Logic => nested filter (recursive call)
            if (!string.IsNullOrEmpty(filter.Logic))
            {
                // Validate: phải có Filters khi dùng Logic
                if (filter.Filters is null) 
                    throw new CustomException("The Filters attribute is required when declaring a logic");
                
                // Recursive: build expression từ Logic và Filters của filter này
                // Ví dụ: Filter có Logic="or", Filters=[{Field="Name", ...}, {Field="Email", ...}]
                bExpresionFilter = CreateFilterExpression(filter.Logic, filter.Filters, parameter);
            }
            else
            {
                // Filter đơn giản: build expression từ Field, Operator, Value
                var filterValid = GetValidFilter(filter);
                bExpresionFilter = CreateFilterExpression(
                    filterValid.Field!, 
                    filterValid.Operator!, 
                    filterValid.Value, 
                    parameter);
            }

            // Kết hợp expression với logic:
            // - Filter đầu tiên: dùng trực tiếp
            // - Filter tiếp theo: kết hợp với expression hiện tại bằng logic (AND/OR/XOR)
            // Ví dụ: (x.Name == "Test") AND (x.Price > 100)
            filterExpression = filterExpression is null 
                ? bExpresionFilter  // Filter đầu tiên
                : CombineFilter(logic, filterExpression, bExpresionFilter); // Kết hợp với filter hiện tại
        }

        return filterExpression;
    }

    /// <summary>
    /// Private helper method để build filter expression từ Field, Operator, Value.
    /// Overload method: nhận field name (string) và convert thành property expression.
    /// </summary>
    /// <param name="field">Field name (có thể nested: "Category.Name")</param>
    /// <param name="filterOperator">Operator: eq, gt, contains, etc.</param>
    /// <param name="value">Value để compare (có thể là JsonElement từ JSON)</param>
    /// <param name="parameter">Parameter expression (x)</param>
    /// <returns>Binary expression: x.Field Operator Value</returns>
    private static Expression CreateFilterExpression(
        string field,
        string filterOperator,
        object? value,
        ParameterExpression parameter)
    {
        // Build property expression từ field name: "Category.Name" => x.Category.Name
        var propertyExpresion = GetPropertyExpression(field, parameter);
        
        // Convert value thành ConstantExpression với type phù hợp
        // Handle: Enum, Guid, String, DateTime, và các types khác
        var valueExpresion = GeValuetExpression(field, value, propertyExpresion.Type);
        
        // Build binary expression: propertyExpression Operator valueExpression
        return CreateFilterExpression(propertyExpresion, valueExpresion, filterOperator);
    }

    /// <summary>
    /// Private helper method để build binary expression từ member expression và constant expression.
    /// Core method: tạo các comparison expressions (==, >, Contains, etc.)
    /// </summary>
    /// <param name="memberExpression">Member expression (x.Property)</param>
    /// <param name="constantExpression">Constant expression (value)</param>
    /// <param name="filterOperator">Operator: eq, gt, contains, etc.</param>
    /// <returns>Binary expression: memberExpression Operator constantExpression</returns>
    private static Expression CreateFilterExpression(
        Expression memberExpression,
        Expression constantExpression,
        string filterOperator)
    {
        // Nếu là string type => convert cả hai về lowercase để case-insensitive comparison
        if (memberExpression.Type == typeof(string))
        {
            // constantExpression.ToLower()
            constantExpression = Expression.Call(constantExpression, "ToLower", null);
            // memberExpression.ToLower()
            memberExpression = Expression.Call(memberExpression, "ToLower", null);
        }

        // Build binary expression dựa trên operator:
        return filterOperator switch
        {
            FilterOperator.EQ => Expression.Equal(memberExpression, constantExpression),                    // x.Property == value
            FilterOperator.NEQ => Expression.NotEqual(memberExpression, constantExpression),                // x.Property != value
            FilterOperator.LT => Expression.LessThan(memberExpression, constantExpression),                 // x.Property < value
            FilterOperator.LTE => Expression.LessThanOrEqual(memberExpression, constantExpression),         // x.Property <= value
            FilterOperator.GT => Expression.GreaterThan(memberExpression, constantExpression),             // x.Property > value
            FilterOperator.GTE => Expression.GreaterThanOrEqual(memberExpression, constantExpression),      // x.Property >= value
            FilterOperator.CONTAINS => Expression.Call(memberExpression, "Contains", null, constantExpression),      // x.Property.Contains(value)
            FilterOperator.STARTSWITH => Expression.Call(memberExpression, "StartsWith", null, constantExpression),  // x.Property.StartsWith(value)
            FilterOperator.ENDSWITH => Expression.Call(memberExpression, "EndsWith", null, constantExpression),     // x.Property.EndsWith(value)
            _ => throw new CustomException("Filter Operator is not valid."),
        };
    }

    /// <summary>
    /// Private helper method để kết hợp hai expressions với logic operator.
    /// </summary>
    /// <param name="filterOperator">Logic operator: "and", "or", "xor"</param>
    /// <param name="bExpresionBase">Expression đầu tiên (đã có sẵn)</param>
    /// <param name="bExpresion">Expression thứ hai (mới thêm)</param>
    /// <returns>Binary expression kết hợp: baseExpression Operator expression</returns>
    private static Expression CombineFilter(
        string filterOperator,
        Expression bExpresionBase,
        Expression bExpresion) => filterOperator switch
        {
            FilterLogic.AND => Expression.And(bExpresionBase, bExpresion),           // base && expression
            FilterLogic.OR => Expression.Or(bExpresionBase, bExpresion),             // base || expression
            FilterLogic.XOR => Expression.ExclusiveOr(bExpresionBase, bExpresion),    // base ^ expression
            _ => throw new ArgumentException("FilterLogic is not valid."),
        };

    /// <summary>
    /// Private helper method để build property expression từ property name (support nested properties).
    /// Ví dụ: "Category.Name" => x.Category.Name
    /// </summary>
    /// <param name="propertyName">Property name (có thể nested: "Category.Name")</param>
    /// <param name="parameter">Parameter expression (x)</param>
    /// <returns>MemberExpression: x.Property hoặc x.Nested.Property</returns>
    private static MemberExpression GetPropertyExpression(
        string propertyName,
        ParameterExpression parameter)
    {
        // Bắt đầu từ parameter: x
        Expression propertyExpression = parameter;
        
        // Split property name theo dấu chấm để handle nested properties
        // Ví dụ: "Category.Name" => ["Category", "Name"]
        foreach (string member in propertyName.Split('.'))
        {
            // Build nested property: x.Category => x.Category.Name
            // Expression.PropertyOrField: tự động detect là Property hay Field
            propertyExpression = Expression.PropertyOrField(propertyExpression, member);
        }

        // Cast về MemberExpression (PropertyExpression hoặc FieldExpression đều kế thừa MemberExpression)
        return (MemberExpression)propertyExpression;
    }

    /// <summary>
    /// Private helper method để extract string từ JsonElement.
    /// Value từ JSON request thường là JsonElement, cần convert sang string.
    /// </summary>
    /// <param name="value">JsonElement object</param>
    /// <returns>String value từ JsonElement</returns>
    private static string GetStringFromJsonElement(object value)
        => ((JsonElement)value).GetString()!; // Cast về JsonElement và lấy string value

    /// <summary>
    /// Private helper method để convert value (thường là JsonElement từ JSON) thành ConstantExpression.
    /// Handle các types đặc biệt: Enum, Guid, String, DateTime, và các types khác.
    /// </summary>
    /// <param name="field">Field name (dùng cho error message)</param>
    /// <param name="value">Value từ JSON request (thường là JsonElement)</param>
    /// <param name="propertyType">Type của property cần convert</param>
    /// <returns>ConstantExpression với value đã convert sang đúng type</returns>
    private static ConstantExpression GeValuetExpression(
        string field,
        object? value,
        Type propertyType)
    {
        // Nếu value là null => return null constant
        if (value == null) 
            return Expression.Constant(null, propertyType);

        // Handle Enum: parse string thành enum value
        if (propertyType.IsEnum)
        {
            // Extract string từ JsonElement
            string? stringEnum = GetStringFromJsonElement(value);

            // Parse string thành enum (ignoreCase = true)
            if (!Enum.TryParse(propertyType, stringEnum, true, out object? valueparsed)) 
                throw new CustomException(string.Format("Value {0} is not valid for {1}", value, field));

            // Return constant với enum value
            return Expression.Constant(valueparsed, propertyType);
        }

        // Handle Guid: parse string thành Guid
        if (propertyType == typeof(Guid))
        {
            // Extract string từ JsonElement
            string? stringGuid = GetStringFromJsonElement(value);

            // Parse string thành Guid
            if (!Guid.TryParse(stringGuid, out Guid valueparsed)) 
                throw new CustomException(string.Format("Value {0} is not valid for {1}", value, field));

            // Return constant với Guid value
            return Expression.Constant(valueparsed, propertyType);
        }

        // Handle String: extract string từ JsonElement
        if (propertyType == typeof(string))
        {
            string? text = GetStringFromJsonElement(value);
            return Expression.Constant(text, propertyType);
        }

        // Handle DateTime: parse string thành DateTime
        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        {
            string? text = GetStringFromJsonElement(value);
            // ChangeType để convert string sang DateTime
            return Expression.Constant(ChangeType(text, propertyType), propertyType);
        }

        // Handle các types khác: lấy raw text từ JsonElement và convert
        // Ví dụ: int, decimal, bool, etc.
        return Expression.Constant(
            ChangeType(((JsonElement)value).GetRawText(), propertyType), 
            propertyType);
    }

    /// <summary>
    /// Public helper method để convert value sang type khác.
    /// Handle Nullable types: nếu là Nullable<T> thì convert về underlying type T.
    /// </summary>
    /// <param name="value">Value cần convert</param>
    /// <param name="conversion">Target type</param>
    /// <returns>Value đã convert sang target type</returns>
    public static dynamic? ChangeType(object value, Type conversion)
    {
        var t = conversion;

        // Nếu là Nullable<T> (ví dụ: int?, DateTime?)
        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            // Nếu value là null => return null
            if (value == null)
            {
                return null;
            }

            // Lấy underlying type: Nullable<int> => int
            t = Nullable.GetUnderlyingType(t);
        }

        // Convert value sang type t
        return Convert.ChangeType(value, t!);
    }

    /// <summary>
    /// Private helper method để validate Filter object.
    /// Đảm bảo Filter có Field và Operator (required khi không dùng Logic).
    /// </summary>
    /// <param name="filter">Filter object cần validate</param>
    /// <returns>Filter object nếu valid</returns>
    /// <exception cref="CustomException">Nếu Field hoặc Operator thiếu</exception>
    private static Filter GetValidFilter(Filter filter)
    {
        // Validate: Field phải có
        if (string.IsNullOrEmpty(filter.Field)) 
            throw new CustomException("The field attribute is required when declaring a filter");
        
        // Validate: Operator phải có
        if (string.IsNullOrEmpty(filter.Operator)) 
            throw new CustomException("The Operator attribute is required when declaring a filter");
        
        return filter;
    }

    /// <summary>
    /// Extension method để apply ordering vào specification.
    /// Support multiple fields và ascending/descending.
    /// Format: ["Name", "Price Desc", "Category.Name"] => OrderBy Name, ThenByDescending Price, ThenBy Category.Name
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="specificationBuilder">Specification builder</param>
    /// <param name="orderByFields">Array các field names để order (có thể có " Desc" để descending)</param>
    /// <returns>IOrderedSpecificationBuilder để tiếp tục chain methods</returns>
    public static IOrderedSpecificationBuilder<T> OrderBy<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        string[]? orderByFields)
    {
        if (orderByFields is not null)
        {
            // Parse orderByFields: ["Name", "Price Desc"] => Dictionary["Name" => OrderBy, "Price" => OrderByDescending]
            foreach (var field in ParseOrderBy(orderByFields))
            {
                // Tạo parameter expression: x => (x là parameter)
                var paramExpr = Expression.Parameter(typeof(T));

                // Build property expression từ field name (support nested: "Category.Name")
                Expression propertyExpr = paramExpr; // Bắt đầu từ x
                foreach (string member in field.Key.Split('.')) // Split "Category.Name" => ["Category", "Name"]
                {
                    // Build nested: x => x.Category => x.Category.Name
                    propertyExpr = Expression.PropertyOrField(propertyExpr, member);
                }

                // Tạo lambda selector: x => (object)x.Property
                // Convert về object để có thể dùng cho bất kỳ type nào
                var keySelector = Expression.Lambda<Func<T, object?>>(
                    Expression.Convert(propertyExpr, typeof(object)), // Convert property về object
                    paramExpr);

                // Thêm OrderExpressionInfo vào Specification.OrderExpressions
                // OrderExpressionInfo chứa: keySelector (lambda) và OrderType (OrderBy, OrderByDescending, ThenBy, ThenByDescending)
                ((List<OrderExpressionInfo<T>>)specificationBuilder.Specification.OrderExpressions)
                    .Add(new OrderExpressionInfo<T>(keySelector, field.Value));
            }
        }

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    /// <summary>
    /// Private helper method để parse orderByFields array thành Dictionary.
    /// Parse format: "FieldName" hoặc "FieldName Desc" => Dictionary["FieldName" => OrderType]
    /// </summary>
    /// <param name="orderByFields">Array các field names (có thể có " Desc" để descending)</param>
    /// <returns>Dictionary: Key = field name, Value = OrderTypeEnum (OrderBy, OrderByDescending, ThenBy, ThenByDescending)</returns>
    private static Dictionary<string, OrderTypeEnum> ParseOrderBy(string[] orderByFields) =>
        new(orderByFields.Select((orderByfield, index) =>
        {
            // Split field name và direction: "Price Desc" => ["Price", "Desc"]
            string[] fieldParts = orderByfield.Split(' ');
            string field = fieldParts[0]; // Field name: "Price"
            
            // Check nếu có "Desc" hoặc "Descending" => descending = true
            bool descending = fieldParts.Length > 1 && 
                fieldParts[1].StartsWith("Desc", StringComparison.OrdinalIgnoreCase);
            
            // Xác định OrderType dựa trên index và descending:
            // - Field đầu tiên (index == 0): OrderBy hoặc OrderByDescending
            // - Field tiếp theo: ThenBy hoặc ThenByDescending
            var orderBy = index == 0
                ? descending ? OrderTypeEnum.OrderByDescending  // Field đầu tiên, descending
                                : OrderTypeEnum.OrderBy         // Field đầu tiên, ascending
                : descending ? OrderTypeEnum.ThenByDescending   // Field tiếp theo, descending
                                : OrderTypeEnum.ThenBy;         // Field tiếp theo, ascending

            return new KeyValuePair<string, OrderTypeEnum>(field, orderBy);
        }));
}