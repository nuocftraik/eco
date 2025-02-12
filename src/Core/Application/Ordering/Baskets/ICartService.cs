

using ECO.WebApi.Domain.Basket;

namespace ECO.WebApi.Application.Ordering.Baskets;
public interface ICartService : ITransientService
{   
    Task<CartDto> GetAsync(string? anonymousUserId);
    Task UpdateCartAsync(CreateCartDto cart);
    Task CreateCartAsync(CreateCartDto cart);

}
