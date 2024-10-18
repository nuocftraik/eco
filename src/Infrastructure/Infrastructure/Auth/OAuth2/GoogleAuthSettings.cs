namespace ECO.WebApi.Infrastructure.Auth.OAuth2;

public class GoogleAuthSettings
{
    public const string SectionName = "Authentication:Google";
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
}