using ECO.WebApi.Application.Identity.Tokens;

namespace ECO.WebApi.Application.Identity.O2Auth;

public interface IAuthenticationService : ITransientService
{
    Task<TokenResponse> GoogleSignIn(string token, string ipAddress);
}