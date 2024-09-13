

using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;
public class Product : AuditableEntity, IAggregateRoot
{
    //Basic Info
    public ProductType ProductType { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    //Media
    public string? MainImage { get; set; }
    public ProductStatus Status { get; set; }
    public int ViewCount { get; set; }

    //Navigation
    public List<Variant> Variants { get; set; } = new();
    public Product ClearMainImagePath()
    {
        MainImage = string.Empty;
        return this;
    }
}
