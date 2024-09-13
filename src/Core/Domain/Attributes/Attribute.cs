

using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Attributes;
public class Attribute : BaseEntity, IAggregateRoot
{
    public string Name { get; set; }
    public AttributeType AttributeType { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }
    public virtual List<AttributeValue> AttributeValues { get; set; } = new();

}
