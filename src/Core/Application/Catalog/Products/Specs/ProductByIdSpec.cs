

namespace ECO.WebApi.Application.Catalog.Products;
public class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(Guid productId)
    {
        Query.Where(x => x.Id == productId)
            .Include(x => x.Attributes).ThenInclude(x => x.AttributeValues)
            .Include(x => x.ProductCategories).ThenInclude(x => x.Category)
            .Include(x => x.Variants).ThenInclude(x => x.VariantAttributeValues).ThenInclude(x => x.AttributeValue)
            .Include(x => x.ProductTags).ThenInclude(x => x.Tag)
            ;
    }
}
