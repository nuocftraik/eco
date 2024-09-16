

namespace ECO.WebApi.Application.Catalog.Categories;
public class ProductByIdWithCategorySpec : Specification<Product>
{
    public ProductByIdWithCategorySpec(Guid productId)
    {
        Query.Where(x => x.Id == productId)
            .Include(x => x.ProductCategories);
    }
}
