using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products.Dtos;

public class CreateAttributeDto
{
    public string Name { get; set; }
    public AttributeType AttributeType { get; set; }
    public List<string> Values { get; set; }
}
