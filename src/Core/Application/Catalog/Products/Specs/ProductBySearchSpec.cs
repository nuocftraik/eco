
using ECO.WebApi.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Application.Catalog.Products;
public class ProductBySearchSpec : EntitiesByPaginationFilterSpec<Product>
{
    public ProductBySearchSpec(SearchProductRequest request)
         : base(request)
    {
        Query.Include(x => x.ProductCategories)
             .ThenInclude(x => x.Category)
             .OrderBy(c => c.CreatedOn, !request.HasOrderBy());
        if (request.CreatedFilter != DateRangeFilter.All)
        {
            ApplyDateRangeFilter(request.CreatedFilter, "CreatedOn");
        }

        if (request.ModifiedFilter != DateRangeFilter.All)
        {
            ApplyDateRangeFilter(request.ModifiedFilter, "LastModifiedOn");
        }

        if (request.ProductType.HasValue)
        {
            Query.Where(c => c.ProductType == request.ProductType.Value);
        }
    }

    private void ApplyDateRangeFilter(DateRangeFilter dateRange, string dateField)
    {
        DateTime now = DateTime.UtcNow;

        switch (dateRange)
        {
            case DateRangeFilter.Last24Hours:
                Query.Where(c => EF.Property<DateTime>(c, dateField) >= now.AddHours(-24));
                break;
            case DateRangeFilter.Last7Days:
                Query.Where(c => EF.Property<DateTime>(c, dateField) >= now.AddDays(-7));
                break;
            case DateRangeFilter.Last30Days:
                Query.Where(c => EF.Property<DateTime>(c, dateField) >= now.AddDays(-30));
                break;
            default:
                break; // No filter for DateRange.All
        }
    }
}

