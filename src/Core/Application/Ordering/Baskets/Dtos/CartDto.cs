

namespace ECO.WebApi.Application.Ordering.Baskets;
public class CartDto : IHasAnonymousId
{
    public Guid? AnonymousId { get; set; }
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public List<CartItemDto> CartItems { get; set; }
    public double TotalPrice { get; set; }

}
