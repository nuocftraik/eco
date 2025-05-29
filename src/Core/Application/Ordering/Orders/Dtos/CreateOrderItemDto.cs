

namespace ECO.WebApi.Application.Ordering.Orders;
public class CreateOrderItemDto
{   
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
}
