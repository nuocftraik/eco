

namespace ECO.WebApi.Application.Ordering.Baskets;
public class CreateCartItemDto
{
    public Guid VariantId { get; set; }
    public double Price { get; set; }
    public int Quantity { get; set; }
}
