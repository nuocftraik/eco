

using ECO.WebApi.Domain.Ordering;

namespace ECO.WebApi.Application.Ordering.Orders;
public class OrderBySearchSpec : EntitiesByPaginationFilterSpec<Order>
{
    public OrderBySearchSpec(PaginationFilter filter) : base(filter)
    {
    }
}
