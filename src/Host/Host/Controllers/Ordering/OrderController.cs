using ECO.WebApi.Application.Ordering.Orders;


namespace ECO.WebApi.Host.Controllers.Ordering;

public class OrderController : BaseApiController
{
    //write controller for CreateOrderRequest
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var order = await Mediator.Send(request);
        return Ok(order);
    }
}
