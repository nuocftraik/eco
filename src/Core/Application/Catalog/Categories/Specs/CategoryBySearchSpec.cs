using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Application.Catalog.Categories;
public class CategoryBySearchSpec : EntitiesByPaginationFilterSpec<Category>
{
    public CategoryBySearchSpec(SearchCategoryRequest request)
         : base(request)
    {
        Query.Include(x => x.ProductCategories)
             .ThenInclude(x => x.Product)
             .Where(c => c.IsActive == request.IsActive)
             .OrderBy(c => c.CreatedOn, !request.HasOrderBy());
        if (request.IsActive.HasValue)
        {
            Query.Where(c => c.IsActive == request.IsActive.Value);
        }
        if (request.CreatedFilter != DateRangeFilter.All)
        {
            ApplyDateRangeFilter(request.CreatedFilter, "CreatedOn");
        }

        if (request.ModifiedFilter != DateRangeFilter.All)
        {
            ApplyDateRangeFilter(request.ModifiedFilter, "LastModifiedOn");
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
