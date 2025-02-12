namespace ECO.WebApi.Application.Ordering.Baskets;
public class CreateCartDto
{
    public Guid? AnonymousId { get; set; }
    public List<CreateCartItemDto> CartItems { get; set; }
}
