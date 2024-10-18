using ECO.WebApi.Domain.Identity;

namespace ECO.WebApi.Application.Identity.Tokens;
public interface ITokenService : ITransientService
{
    Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);

    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    
    Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, string ipAddress);
}
