using ECO.WebApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure;
public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services
            .AddPersistence()
            .AddRouting(options => options.LowercaseUrls = true);
    }






    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder) =>
        builder
            .UseRequestLocalization()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization();


    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        return builder;
    }

}