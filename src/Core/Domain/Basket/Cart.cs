
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Domain.Ordering;

namespace ECO.WebApi.Domain.Basket;
public class Cart : AuditableEntity, IAggregateRoot
{

    public virtual List<CartItem> CartItems { get; set; } = new();

    public void AddVariant(Guid variantId, int count = 1)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Product count should be 1 or more!");
        }

        var item = CartItems.FirstOrDefault(x => x.VariantId == variantId);
        if (item == null)
        {
            CartItems.Add(new CartItem(variantId, count));
        }
        else
        {
            item.Quantity += count;
        }
    }

    public void RemoveVariant(Guid variantId, int? count = null)
    {
        if (count is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Product count should be null, 1 or more!");
        }

        var item = CartItems.FirstOrDefault(x => x.VariantId == variantId);
        if (item == null)
        {
            return;
        }

        if (count == null || item.Quantity <= count)
        {
            CartItems.Remove(item);
            return;
        }

        item.Quantity -= count.Value;
    }

    public int GetVariantCount(Guid variantId)
    {
        var item = CartItems.FirstOrDefault(x => x.VariantId == variantId);
        return item?.Quantity ?? 0;
    }

    public void Clear()
    {
        CartItems.Clear();
    }

    public void Merge(Cart cart)
    {
        foreach (var item in cart.CartItems)
        {
            AddVariant(item.VariantId, item.Quantity);
        }
    }


    // Trừ số lượng sản phẩm đã đặt, nếu về 0 thì xóa khỏi giỏ hàng
    public void DeductOrderedItems(List<OrderItem> orderedItems)
    {
        foreach (var orderedItem in orderedItems)
        {
            var cartItem = CartItems.FirstOrDefault(i => i.VariantId == orderedItem.VariantId);
            if (cartItem != null)
            {
                cartItem.Quantity -= orderedItem.Quantity;

                if (cartItem.Quantity <= 0)
                {
                    CartItems.Remove(cartItem);
                }
            }
        }
    }















}
