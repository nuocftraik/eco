
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Enum;
using ECO.WebApi.Domain.Identity;


namespace ECO.WebApi.Domain.Ordering;
public class Order : AuditableEntity, IAggregateRoot
{
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public double ShippingFee { get; set; }
    public double Tax { get; set; }
    public double Total { get; set; }
    public double SubTotal { get; set; }
    public double Discount { get; set; }
    public double GrandTotal { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string CustomerAddress { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid? CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public virtual ApplicationUser Customer { get; set; }
    public virtual List<OrderItem> OrderItems { get; set; }
}
