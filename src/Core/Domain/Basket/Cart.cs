
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Identity;

namespace ECO.WebApi.Domain.Basket;
public class Cart : AuditableEntity, IAggregateRoot
{

    public string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; }
    public decimal TotalPrice => CartItems.Sum(item => item.Price * item.Quantity);

    public virtual ICollection<CartItem> CartItems { get; set; }
}
