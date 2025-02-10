using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Common.Exceptions;

namespace ECO.WebApi.Domain.Ordering;
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }
    public double Price { get; private set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; private set; }
    [ForeignKey(nameof(VariantId))]
    public virtual Variant Variant { get; private set; }

    public OrderItem(Guid variantId, int quantity, double price)
    {
        VariantId = variantId;
        Quantity = quantity;
        Price = price;
    }

    public void AddQuanity(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException("Invalid unit");
        }

        Quantity += quantity;
    }
}
