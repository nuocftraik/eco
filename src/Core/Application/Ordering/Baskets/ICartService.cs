

using ECO.WebApi.Domain.Basket;
using ECO.WebApi.Domain.Ordering;

namespace ECO.WebApi.Application.Ordering.Baskets;
public interface ICartService : ITransientService
{   
    Task<CartDto> GetAsync(string? anonymousUserId);
    Task UpdateCartAsync(CreateCartDto cart);
    Task CreateCartAsync(CreateCartDto cart);

    Task DeductOrderedItemsAsync(List<OrderItem> orderedItems, Guid? anonymousId);


}
