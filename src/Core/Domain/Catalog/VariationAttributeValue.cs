using Microsoft.EntityFrameworkCore;



namespace ECO.WebApi.Domain.Catalog;
[PrimaryKey(nameof(VariationId), nameof(AttributeValueId))]
public class VariationAttributeValue
{
    public Guid VariationId { get; set; }
    public Guid AttributeValueId { get; set; }
    public virtual Variant Variation { get; set; }
    public virtual Attributes.AttributeValue AttributeValue { get; set; }
}

