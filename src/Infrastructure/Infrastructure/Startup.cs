using ECO.WebApi.Infrastructure.Auth;
using ECO.WebApi.Infrastructure.Behaviors;
using ECO.WebApi.Infrastructure.Common;
using ECO.WebApi.Infrastructure.Cors;
using ECO.WebApi.Infrastructure.FileStorage;
using ECO.WebApi.Infrastructure.Middleware;
using ECO.WebApi.Infrastructure.Persistence;
using ECO.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure;
public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddAuth(config)
            .AddCorsPolicy(config)
            .AddExceptionMiddleware()
            .AddBehaviours()
            .AddPersistence()
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices(); 
    }




    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(cancellationToken);
    }


    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder) =>
        builder
             //.UseRequestLocalization()
             //.UseStaticFiles()
            .UseCurrentUser()
            .UseFileStorage()
            .UseExceptionMiddleware()
            .UseRouting()
            .UseCorsPolicy()
            .UseHttpsRedirection()
            .UseAuthentication()
            .UseAuthorization();


    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers();
            //.RequireAuthorization();

        return builder;
    }

}
