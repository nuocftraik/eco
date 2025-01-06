

namespace ECO.WebApi.Application.Identity.Tokens;

public record TokenResponse(string accessToken, string refreshToken, DateTime RefreshTokenExpiryTime);
