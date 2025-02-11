

using ECO.WebApi.Application.Catalog.Products.EventHandlers;
using ECO.WebApi.Domain.Common.Events;
using ECO.WebApi.Domain.Ordering;

namespace ECO.WebApi.Application.Ordering.Orders.EventHandlers;
public class OrderCreatedEventHandler : INotificationHandler<EventNotification<EntityCreatedEvent<Order>>>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EventNotification<EntityCreatedEvent<Order>> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{event} Triggered", notification.Event.GetType().Name);
        return Task.CompletedTask;
    }
}
