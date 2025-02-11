

namespace ECO.WebApi.Application.Ordering.Orders;
public class OrderByIdSpec : Specification<Domain.Ordering.Order>
{
    public OrderByIdSpec(Guid orderId)
    {
        Query.Where(o => o.Id == orderId)
            .Include(o => o.OrderItems)
                .ThenInclude(orderId => orderId.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Attributes)
                            .ThenInclude(a => a.AttributeValues)
            ;
    }
}
