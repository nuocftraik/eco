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
    public async Task<IActionResult> GoogleLogin([FromBody] OAuthRequest request)
    {
        var response = await _authenticationService.GoogleSignIn(request.IdToken, GetIpAddress()!);

        return Ok(response);
    }

    [HttpPost("google2")]
    [AllowAnonymous]
    [OpenApiOperation("Request token using google provider")]
    public async Task<IActionResult> GoogleLogin2([FromBody] string authorizedCode)
    {
        var response = await _authenticationService.GoogleSignIn2(authorizedCode, GetIpAddress()!);

        return Ok(response);
    }


    [HttpPost("facebook")]
    [AllowAnonymous]
    [OpenApiOperation("Request token using facebook provider")]
    public async Task<IActionResult> FacebookLogin([FromBody] OAuthRequest request)
    {
        var response = await _authenticationService.FacebookSignIn(request.IdToken, GetIpAddress()!);

        return Ok(response);
    }

    private string? GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"]
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";

}

public class OAuthRequest {
    public string IdToken { get; set; } = default!;
}
