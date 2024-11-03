

namespace ECO.WebApi.Infrastructure.GoogleDrive;
public class GoogleDriveSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public List<string> Scopes { get; set; }
    public string CredentialStore { get; set; }
}
