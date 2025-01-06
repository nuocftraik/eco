using ECO.WebApi.Application.Identity.O2Auth;
using ECO.WebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;
using ECO.WebApi.Infrastructure.Auth.OAuth2;
using Microsoft.Extensions.Options;
using ECO.WebApi.Application.Identity.Tokens;
using ECO.WebApi.Shared.Authorization;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using ECO.WebApi.Application.Common.Interfaces;

namespace ECO.WebApi.Infrastructure.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GoogleAuthSettings _googleAuthConfig;
    private readonly ITokenService _tokenService;
    private readonly ISerializerService _serializerService;


    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IOptions<GoogleAuthSettings> googleAuthConfig,
        ITokenService tokenService,
        ISerializerService serializerService)
    {
        _userManager = userManager;
        _googleAuthConfig = googleAuthConfig.Value;
        _tokenService = tokenService;
        _serializerService = serializerService;

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

    public async Task<TokenResponse> GoogleSignIn2(string authorizationCode, string ipAddress)
    {
        // Cấu hình GoogleAuthorizationCodeFlow
        var googleAuthorizationCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _googleAuthConfig.ClientId,
                ClientSecret = _googleAuthConfig.ClientSecret
            },
            Scopes = new[] { "https://www.googleapis.com/auth/userinfo.profile", "https://www.googleapis.com/auth/userinfo.email" }
        });

        // Trao đổi Authorization Code để lấy Token
        var tokenResponse = await googleAuthorizationCodeFlow.ExchangeCodeForTokenAsync(
            userId: "me",
            code: authorizationCode,
            redirectUri: "_googleAuthConfig.RedirectUri",
            CancellationToken.None
        );

        // Lấy thông tin người dùng từ Google API
        using var httpClient = new HttpClient();
        var userInfoResponse = await httpClient.GetStringAsync(
            $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={tokenResponse.AccessToken}"
        );

        var userInfo = _serializerService.Deserialize<ApplicationUser>(userInfoResponse);

        // Kiểm tra hoặc tạo người dùng trong hệ thống
        var emailLogin = userInfo.Email;

        var existingUser = await _userManager.FindByEmailAsync(emailLogin.Trim().Normalize());

        if (existingUser == null)
        {
            existingUser = new ApplicationUser
            {
                Email = emailLogin,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                UserName = emailLogin,
                EmailConfirmed = true,
                IsActive = true
            };

            await _userManager.CreateAsync(existingUser);
            await _userManager.AddToRoleAsync(existingUser, ECORoles.Basic);
        }

        // Tạo JWT Token
        var generateToken = await _tokenService.GenerateTokensAndUpdateUser(existingUser, ipAddress);

        return generateToken;
    }
}

