using ECO.WebApi.Application.Ordering.Baskets;
using ECO.WebApi.Domain.Basket;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Ordering;
[AllowAnonymous]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    
    [HttpGet("get")]
    [OpenApiOperation("Get cart by userId.", "")]

    public async Task<CartDto> GetAsync(string? anonymousUserId)
    {
        return await _cartService.GetAsync(anonymousUserId);
    }

    //write controller for update cart
    [HttpPost("update")]
    public async Task UpdateAsync(CreateCartDto cart)
    {
        await _cartService.UpdateCartAsync(cart);
    }

    //write controller for create cart
    [HttpPost("create")]
    public async Task CreateAsync(CreateCartDto cart)
    {
        await _cartService.CreateCartAsync(cart);
    }

}
