using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Auth.OAuth2;

internal static class Startup
{
    internal static IServiceCollection AddO2Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleAuthSettings>
            (configuration.GetSection(GoogleAuthSettings.SectionName));

        services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            var googleAuthSettings = configuration
                .GetSection(GoogleAuthSettings.SectionName)
                .Get<GoogleAuthSettings>();

            googleOptions.ClientId = googleAuthSettings.ClientId;
            googleOptions.ClientSecret = googleAuthSettings.ClientSecret;
        });

        return services;
    }
}