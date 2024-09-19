
using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Domain.Attributes;
public class AttributeValue : BaseEntity
{
    public Guid AttributeId { get; private set; }
    public string Value { get; private set; }
    public virtual Attribute Attribute { get; private set; }
    public virtual List<VariantAttributeValue> VariantAttributeValues { get;private set; } = new();

    private AttributeValue() { }

    public AttributeValue(string value, Guid attributeId)
    {
        Value = value;
        AttributeId = attributeId;
    }

    public void Update(string value)
    {
        Value = value;
    }


}
