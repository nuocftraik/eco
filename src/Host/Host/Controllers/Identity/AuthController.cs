using ECO.WebApi.Application.Identity.O2Auth;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Identity;
public class AuthController : BaseApiController
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("google")]
    [AllowAnonymous]
    [OpenApiOperation("Request token using google provider")]
    public async Task<IActionResult> GoogleLogin([FromBody] string token){
        var response = await _authenticationService.GoogleSignIn(token, GetIpAddress()!);

        return Ok(response);
    }

    private string? GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"]
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
}