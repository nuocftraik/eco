

namespace ECO.WebApi.Application.Ordering.Orders.Events;
public class OrderPaidSuccessEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }

    public OrderPaidSuccessEvent(Guid orderId, Guid paymentId)
    {
        OrderId = orderId;
        PaymentId = paymentId;
    }
}
