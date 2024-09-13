

namespace ECO.WebApi.Domain.Catalog;

public class Tag : BaseEntity, IAggregateRoot
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public virtual List<ProductTag> ProductTags { get; set; } = new();
}
