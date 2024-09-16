using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Domain.Catalog;
[PrimaryKey(nameof(ProductId), nameof(CategoryId))]
public class ProductCategory
{
    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
    public virtual Product Product { get; private set; }
    public virtual Category Category { get; private set; }

    private ProductCategory() { }

    public ProductCategory(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }
}

