using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Domain.Catalog;
[PrimaryKey(nameof(ProductId), nameof(CategoryId))]
public class ProductCategory
{
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }
    public virtual Product Product { get; set; }
    public virtual Category Category { get; set; }
}

