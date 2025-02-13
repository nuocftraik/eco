
using ECO.WebApi.Domain.Basket;

namespace ECO.WebApi.Application.Ordering.Orders;
public class MyCartSpec : Specification<Cart>
{
    public MyCartSpec(Guid userId)
    {
        Query.Where(x => x.CreatedBy == userId).Include(x => x.CartItems)
                .ThenInclude(x => x.Variant)
                    .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.Attributes)
                            .ThenInclude(x => x.AttributeValues)
            ;
    }
}
