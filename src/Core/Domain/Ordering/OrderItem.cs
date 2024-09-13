using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Domain.Ordering;
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid VariationId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; }
    [ForeignKey(nameof(VariationId))]
    public virtual Variant Variant { get; set; }

}
