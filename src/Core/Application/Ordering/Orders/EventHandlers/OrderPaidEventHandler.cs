
using ECO.WebApi.Application.Ordering.Orders.Events;
using ECO.WebApi.Domain.Ordering;
using MediatR;

namespace ECO.WebApi.Application.Ordering.Orders.EventHandlers;
public class OrderPaidEventHandler : INotificationHandler<EventNotification<OrderPaidSuccessEvent>>,
                                     INotificationHandler<EventNotification<OrderPaidFailedEvent>>
{
    private readonly ILogger<OrderPaidEventHandler> _logger;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Variant> _variantRepository;
    public OrderPaidEventHandler(ILogger<OrderPaidEventHandler> logger, IRepository<Order> orderRepository, IRepository<Variant> variantRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _variantRepository = variantRepository;
    }
    public async Task Handle(EventNotification<OrderPaidSuccessEvent> notification, CancellationToken cancellationToken)
    {

        _logger.LogInformation("OrderPaidEvent triggered for OrderId: {OrderId}", notification.Event.OrderId);

        var order = await _orderRepository.GetByIdAsync(notification.Event.OrderId);
        if (order == null)
        {
            _logger.LogError("Order not found: {OrderId}", notification.Event.OrderId);
            return;
        }

        foreach (var item in order.OrderItems)
        {
            var variant = await _variantRepository.GetByIdAsync(item.VariantId) ?? throw new NotFoundException($"Variant with ID {item.VariantId} was not found.");
            variant.DecreaseStock(item.Quantity);
            await _variantRepository.UpdateAsync(variant);

        }



        // Cập nhật trạng thái đơn hàng sau khi thanh toán thành công
        order.SetOrderCompleted();
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} marked as PAID.", notification.Event.OrderId);


    }

    public async Task Handle(EventNotification<OrderPaidFailedEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderPaidEvent triggered for OrderId: {OrderId}", notification.Event.OrderId);

        var order = await _orderRepository.GetByIdAsync(notification.Event.OrderId);
        if (order == null)
        {
            _logger.LogError("Order not found: {OrderId}", notification.Event.OrderId);
            return;
        }

        // Cập nhật trạng thái đơn hàng sau khi thanh toán thành công
        order.SetOrderCancelled();
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} marked as CANCELLED.", notification.Event.OrderId);
    }
}
