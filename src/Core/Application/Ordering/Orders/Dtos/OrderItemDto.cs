

using ECO.WebApi.Application.Catalog.Products;

namespace ECO.WebApi.Application.Ordering.Orders;
public class OrderItemDto
{
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }
    public double Price { get; private set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductImage { get; set; }
    public List<AttributeDto> Attributes { get; set; }

}
