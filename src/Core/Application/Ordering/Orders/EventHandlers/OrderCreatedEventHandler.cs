

using ECO.WebApi.Application.Catalog.Products.EventHandlers;
using ECO.WebApi.Application.Ordering.Baskets;
using ECO.WebApi.Domain.Basket;
using ECO.WebApi.Domain.Common.Events;
using ECO.WebApi.Domain.Ordering;

namespace ECO.WebApi.Application.Ordering.Orders.EventHandlers;
public class OrderCreatedEventHandler : INotificationHandler<EventNotification<EntityCreatedEvent<Order>>>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    private readonly ICartService _cartService;
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Variant> _variantRepository;
    private readonly ICurrentUser _currentUser;
   

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, ICartService cartService, IRepository<Cart> cartRepository, IRepository<Variant> variantRepository, ICurrentUser currentUser)
    {
        _logger = logger;
        _cartService = cartService;
        _cartRepository = cartRepository;
        _variantRepository = variantRepository;
        _currentUser = currentUser;
    }

    public async Task Handle(EventNotification<EntityCreatedEvent<Order>> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{event} Triggered", notification.Event.GetType().Name);

        var orderCreated = notification.Event.Entity;
        //Gửi email xác nhận đơn hàng.

        _logger.LogInformation("OrderCreatedEventHandler completed for OrderId: {OrderId}", orderCreated.Id);
    }
}
