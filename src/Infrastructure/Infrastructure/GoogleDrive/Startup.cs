
using ECO.WebApi.Application.GoogleDrive;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.GoogleDrive;
internal static class Startup
{
    internal static IServiceCollection AddGoogleDrive(this IServiceCollection services, IConfiguration configuration)
    {
        // Load Google Drive settings from configuration
        services.Configure<GoogleDriveSettings>(configuration.GetSection("DriveSettings"));

        // Configure DriveService with OAuth2
        services.AddSingleton<DriveService>(provider =>
        {
            var driveSettings = provider.GetRequiredService<IOptions<GoogleDriveSettings>>().Value;

            // Create UserCredential here using the settings
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = driveSettings.ClientId,
                    ClientSecret = driveSettings.ClientSecret
                },
                new[] { DriveService.Scope.Drive },
                "user",
                CancellationToken.None,
                new FileDataStore("Drive.Auth.Store")).Result;

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v3",
            });
        });

        return services; 
    }
}
