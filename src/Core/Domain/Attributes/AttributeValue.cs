
using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Domain.Attributes;
public class AttributeValue : BaseEntity
{
    public Guid AttributeId { get; set; }
    public string Value { get; set; }
    public virtual Attribute Attribute { get; set; }
    public virtual List<VariationAttributeValue> VariationAttributeValues { get; set; } = new();
}
