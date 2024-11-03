using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Auth.OAuth2;

internal static class Startup
{
    internal static IServiceCollection AddO2Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleAuthSettings>
            (configuration.GetSection(GoogleAuthSettings.SectionName));

        services.Configure<FacebookAuthSettings>
            (configuration.GetSection(FacebookAuthSettings.SectionName));

        services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            var googleAuthSettings = configuration
                .GetSection(GoogleAuthSettings.SectionName)
                .Get<GoogleAuthSettings>();

            googleOptions.ClientId = googleAuthSettings.ClientId;
            googleOptions.ClientSecret = googleAuthSettings.ClientSecret;
        })
        .AddFacebook(fo => {
            var facebookSetting = configuration
                .GetSection(FacebookAuthSettings.SectionName)
                .Get<FacebookAuthSettings>();

            fo.AppId = facebookSetting.AppId;
            fo.AppSecret = facebookSetting.AppSecret;
        });

        return services;
    }
}