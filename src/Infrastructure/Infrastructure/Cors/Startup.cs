using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ECO.WebApi.Infrastructure.Cors;
internal static class Startup
{
    private const string CorsPolicy = nameof(CorsPolicy);

    internal static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        return services.AddCors(opt =>
            opt.AddPolicy(CorsPolicy, policy =>
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("https://localhost:5001")));
    }

    internal static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app) =>
        app.UseCors(CorsPolicy);
}