
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Catalog;


namespace ECO.WebApi.Domain.Basket;
public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }

    [ForeignKey(nameof(CartId))]
    public virtual Cart Cart { get; set; }

    [ForeignKey(nameof(VariantId))]
    public virtual Variant Variant { get; set; }

    public CartItem(Guid variantId, int quantity = 1)
    {
        VariantId = variantId;
        Quantity = quantity;
    }
}
