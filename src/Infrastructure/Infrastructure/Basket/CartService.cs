using ECO.WebApi.Application.Common.Caching;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Application.Ordering.Baskets;
using ECO.WebApi.Domain.Basket;
using ECO.WebApi.Infrastructure.Persistence.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;


namespace ECO.WebApi.Infrastructure.Basket;

public class CartService : ICartService
{
    private readonly ICacheService _cache;
    private readonly ICurrentUser _currentUser;
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Cart> _cartRepository;
    private const string CacheKeyPrefix = "cart_";

    public CartService(ICacheService cache, ICurrentUser currentUser, ApplicationDbContext context, IRepository<Cart> cartRepository)
    {
        _cache = cache;
        _currentUser = currentUser;
        _context = context;
        _cartRepository = cartRepository;
    }

    private static string GetCacheKey(Guid id)
    {
        return $"{CacheKeyPrefix}{id}";
    }

    public async Task<CartDto> GetAsync(string? anonymousUserId)
    {
        // Trường hợp 1: Người dùng hiện tại đã xác thực và có ID người dùng ẩn danh
        if (anonymousUserId != null && _currentUser.IsAuthenticated())
        {
            // Lấy giỏ hàng của người dùng hiện tại
            var userCart = await GetMyCartAsync(_currentUser.GetUserId().ToString());

            // Lấy giỏ hàng của người dùng ẩn danh
            var anonymousUserCart = await _cache.GetAsync<CreateCartDto>(GetCacheKey(Guid.Parse(anonymousUserId)));

            if (anonymousUserCart != null)
            {
                // Hợp nhất giỏ hàng của người dùng ẩn danh vào giỏ hàng của người dùng hiện tại
                userCart.Merge(anonymousUserCart.Adapt<Cart>());
                await UpdateCartAsync(userCart.Adapt<CreateCartDto>());

                // Xóa giỏ hàng của người dùng ẩn danh
                anonymousUserCart.CartItems.Clear();
                await _cache.SetAsync(GetCacheKey(Guid.Parse(anonymousUserId)), anonymousUserCart);
            }

            return await GetCartDtoAsync(userCart);
        }

        // Trường hợp 2: Người dùng chưa xác thực và có ID người dùng ẩn danh
        if (anonymousUserId != null && !_currentUser.IsAuthenticated())
        {
            var cart = await _cache.GetAsync<Cart>(GetCacheKey(Guid.Parse(anonymousUserId)));
            return await GetCartDtoAsync(cart);
        }

        // Trường hợp 3: Người dùng đã xác thực và không có ID người dùng ẩn danh
        if (_currentUser.IsAuthenticated())
        {
            var cart = await GetMyCartAsync(_currentUser.GetUserId().ToString());
            return await GetCartDtoAsync(cart);
        }

        throw new NotFoundException("Cart not found.");
    }

    private async Task<Cart> GetMyCartAsync(string customerId)
    {
        var cart = await _context.Carts.Include(x => x.CartItems).ThenInclude(x => x.Variant).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.CreatedBy == Guid.Parse(customerId)) ?? throw new NotFoundException($"Cart was not found.");
        return cart;
    }

    private async Task<CartDto> GetCartDtoAsync(Cart cart)
    {
        var cardDto = new CartDto();
        foreach (var item in cart.CartItems)
        {
            var variant = _context.Variants.Include(x => x.Product).FirstOrDefault(x => x.Id == item.VariantId)
            ?? throw new NotFoundException($"Item with ID {item.VariantId} was not found.");
            if (item.Quantity > variant.Quantity)
            {
                cart.RemoveVariant(item.VariantId, item.Quantity - variant.Quantity);
            }

            cardDto.CartItems.Add(new CartItemDto
            {
                VariantId = item.VariantId,
                ProductId = variant.ProductId,
                ProductName = variant.Product.Name,
                Price = variant.Price,
                Quantity = item.Quantity,
                ProductImage = variant.Product.MainImage,
                MainImage = variant.MainImage,
                Status = variant.Status,
                IsDefault = variant.IsDefault,
                IncludeDownload = variant.IncludeDownload,
                FileName = variant.FileName,
                ComparePrice = variant.ComparePrice,
            });
        }
        return cardDto;
    }

    public async Task UpdateCartAsync(CreateCartDto cart)
    {
        if (cart.AnonymousId.HasValue)
        {   

            await _cache.SetAsync(GetCacheKey(cart.AnonymousId.Value), cart);
        }
        else
        {   
            var myCart = await GetMyCartAsync(_currentUser.GetUserId().ToString());
            myCart.Adapt(cart);
            _context.Carts.Update(myCart);
            await _context.SaveChangesAsync();
        }
       
    }


    public async Task CreateCartAsync(CreateCartDto cart)
    {


        // Kiểm tra nếu là người dùng ẩn danh
        if (cart.AnonymousId.HasValue)
        {
            // Lưu giỏ hàng vào cache
            await _cache.SetAsync(GetCacheKey(cart.AnonymousId.Value), cart);
        }
        else
        {
            // Kiểm tra nếu người dùng đã xác thực
            if (_currentUser.IsAuthenticated())
            {
                // Lưu giỏ hàng vào cơ sở dữ liệu
                var cardToAdd = cart.Adapt<Cart>();

                await _context.Carts.AddAsync(cardToAdd);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Cannot create cart for unauthenticated user without an anonymous ID.");
            }
        }
    }
}
