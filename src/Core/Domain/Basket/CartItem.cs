
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Catalog;


namespace ECO.WebApi.Domain.Basket;
public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    [ForeignKey(nameof(CartId))]
    public virtual Cart Cart { get; set; }
    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; }
    [ForeignKey(nameof(VariantId))]
    public virtual Variant? Variant { get; set; }
}
