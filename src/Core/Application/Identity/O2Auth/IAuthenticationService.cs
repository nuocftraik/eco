using ECO.WebApi.Application.Identity.Tokens;

namespace ECO.WebApi.Application.Identity.O2Auth;

public interface IAuthenticationService : ITransientService
{   
    //signing in with google using idToken
    Task<TokenResponse> GoogleSignIn(string idToken, string ipAddress);
    //authorize google flow
    Task<TokenResponse> GoogleSignIn2(string authorizedCode,string ipAddress);

    Task<TokenResponse> FacebookSignIn(string idToken, string ipAddress);
}
