

using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Attributes;
public class Attribute : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public AttributeType AttributeType { get; private set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; private set; }
    public virtual List<AttributeValue> AttributeValues { get; private set; } = new();

    private Attribute() { }

    public Attribute(string name, AttributeType attributeType, Guid productId)
    {
        Name = name;
        AttributeType = attributeType;
        ProductId = productId;
    }

    public void Update(string name, AttributeType attributeType)
    {
        Name = name;
        AttributeType = attributeType;
    }

    public void AddValue(string value)
    {
        AttributeValues.Add(new AttributeValue(value, Id));
    }

    public void UpdateAttributeValues(List<AttributeValue> newValues)
    {
        // Xóa các giá trị không còn tồn tại
        AttributeValues.RemoveAll(av => !newValues.Any(nv => nv.Value == av.Value));
        foreach (var newValue in newValues)
        {
            var existingValue = AttributeValues.FirstOrDefault(av => av.Value == newValue.Value);
            if (existingValue != null)
            {
                existingValue.Update(newValue.Value);
            }
            else
            {
                AddValue(newValue.Value);
            }
        }
    }

}
