namespace ECO.WebApi.Infrastructure.Auth.OAuth2;

public class FacebookAuthSettings
{
    public const string SectionName = "Authentication:Facebook";
    public string AppId { get; set; } = default!;
    public string AppSecret { get; set; } = default!;
}