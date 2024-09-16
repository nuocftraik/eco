using ECO.WebApi.Domain.Catalog;
namespace ECO.WebApi.Application.Catalog.Categories;
public class CategoryByIdSpec : Specification<Category>
{
    public CategoryByIdSpec(Guid id)
    {
        Query.Include(x => x.ProductCategories)
                .ThenInclude(x => x.Product)
             .Where(x => x.Id == id);
    }
}

