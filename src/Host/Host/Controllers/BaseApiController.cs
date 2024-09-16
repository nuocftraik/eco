using MediatR;

namespace ECO.WebApi.Host.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    private ISender _mediator = null!;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
