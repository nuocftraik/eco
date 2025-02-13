

namespace ECO.WebApi.Application.Ordering.Orders.Events;
public class OrderPaidFailedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }

    public OrderPaidFailedEvent(Guid orderId, Guid paymentId)
    {
        OrderId = orderId;
        PaymentId = paymentId;
    }
}
