
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;
public class AttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public AttributeType AttributeType { get; set; }
    public List<AttributeValueDto> AttributeValues { get; set; }
}
