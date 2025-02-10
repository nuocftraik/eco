
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Enum;
using ECO.WebApi.Domain.Identity;


namespace ECO.WebApi.Domain.Ordering;
public class Order : AuditableEntity, IAggregateRoot
{
    public OrderStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public double Total { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerPhoneNumber { get; private set; }
    public string? CustomerAddress { get; private set; }
    public string? CustomerEmail { get; private set; }

    public virtual List<OrderItem> OrderItems { get; private set; }

    public Order(PaymentMethod paymentMethod, double total, string customerName, string customerPhoneNumber, string? customerAddress, string? customerEmail)
    {
        PaymentMethod = paymentMethod;
        Total = total;
        CustomerName = customerName;
        CustomerPhoneNumber = customerPhoneNumber;
        CustomerAddress = customerAddress;
        CustomerEmail = customerEmail;
    }
    public Order SetOrderCancelled()
    {
        Status = OrderStatus.Cancelled;
        return this;
    }


    public Order SetOrderCompleted()
    {
        Status = OrderStatus.Completed;
        return this;
    }

    public Order AddOrderItem(Guid variantId, double price, int quantity = 1)
    {
        var existingOrderForVariant = OrderItems.SingleOrDefault(o => o.VariantId == variantId);

        if (existingOrderForVariant != null)
        {

            existingOrderForVariant.AddQuanity(quantity);
        }
        else
        {
            var orderItem = new OrderItem(variantId, quantity, price);
            OrderItems.Add(orderItem);
        }

        return this;
    }
    public double GetTotal()
    {
        var tru = true ? true : false;
        return OrderItems.Sum(o => o.Quantity * o.Price);

    }
}

