using ECO.WebApi.Application.Identity.O2Auth;
using ECO.WebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;
using ECO.WebApi.Infrastructure.Auth.OAuth2;
using Microsoft.Extensions.Options;
using ECO.WebApi.Application.Identity.Tokens;
using ECO.WebApi.Shared.Authorization;

namespace ECO.WebApi.Infrastructure.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GoogleAuthSettings _googleAuthConfig;
    private readonly ITokenService _tokenService;


    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IOptions<GoogleAuthSettings> googleAuthConfig,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _googleAuthConfig = googleAuthConfig.Value;
        _tokenService = tokenService;
    }

    public Task<TokenResponse> FacebookSignIn(string token, string ipAddress)
    {
        throw new NotImplementedException();
    }

    public async Task<TokenResponse> GoogleSignIn(string token, string ipAddress)
    {
        var payload = await GoogleJsonWebSignature
        .ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _googleAuthConfig.ClientId }
        });

        var emailLogin = payload.Email;

        var existingUser = await _userManager.FindByEmailAsync(emailLogin.Trim().Normalize());

        if (existingUser == null)
        {
            existingUser = new ApplicationUser
            {
                Email = emailLogin,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                UserName = payload.Email,
                EmailConfirmed = true,
                IsActive = true
            };

            await _userManager.CreateAsync(existingUser);
            await _userManager.AddToRoleAsync(existingUser, ECORoles.Basic);
        }
        // else if (existingUser.GoogleId == null){
        //     // email not linked to gg account
        // }

        var generateToken = await _tokenService.GenerateTokensAndUpdateUser(existingUser, ipAddress);

        return generateToken;
    }
}
