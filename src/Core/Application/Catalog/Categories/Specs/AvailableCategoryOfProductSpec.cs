

namespace ECO.WebApi.Application.Catalog.Categories;
public class AvailableCategoryOfProductSpec : Specification<Category>
{
    public AvailableCategoryOfProductSpec(List<Guid> categoryIds)
    {
        Query.Where(c => !categoryIds.Contains(c.Id))
            .OrderBy(x => x.Name); 
    }
}
