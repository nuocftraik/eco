

namespace ECO.WebApi.Application.Ordering.Orders;
public class CreateOrderItemDto
{   
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }
    public double Price { get; private set; }
}
