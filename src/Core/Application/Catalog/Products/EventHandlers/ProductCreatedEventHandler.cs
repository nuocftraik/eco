
using ECO.WebApi.Domain.Common.Events;

namespace ECO.WebApi.Application.Catalog.Products.EventHandlers;
//public class ProductCreatedEventHandler : EventNotificationHandler<EntityCreatedEvent<Product>>
//{
//    private readonly ILogger<ProductCreatedEventHandler> _logger;

//    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger) => _logger = logger;

//    public override Task Handle(EntityCreatedEvent<Product> @event, CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("{event} Triggered", @event.GetType().Name);
//        return Task.CompletedTask;
//    }
//}

public class ProductCreatedEventHandler : INotificationHandler<EventNotification<EntityCreatedEvent<Product>>>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger) => _logger = logger;

    public Task Handle(EventNotification<EntityCreatedEvent<Product>> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{event} Triggered", notification.Event.GetType().Name);
        var prod = notification.Event.Entity;
        // Logic xử lý sự kiện cụ thể
        return Task.CompletedTask;
    }
}

//Chưa thấy nhanh hơn chỗ nào
