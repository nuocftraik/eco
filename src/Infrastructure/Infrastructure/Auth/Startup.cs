using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Infrastructure.Auth.Jwt;
using ECO.WebApi.Infrastructure.Auth.OAuth2;
using ECO.WebApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ECO.WebApi.Infrastructure.Auth;
internal static class Startup
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddCurrentUser()
            // Must add identity before adding auth!
            .AddIdentity();

        services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
        services.AddO2Authentication(config);
        return services.AddJwtAuth();

    }

    internal static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app) =>
    app.UseMiddleware<CurrentUserMiddleware>();

    private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
        services
            .AddScoped<CurrentUserMiddleware>()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());


}
