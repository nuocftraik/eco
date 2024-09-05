
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Behaviors;
public static class Extensions
{
    public static IServiceCollection AddBehaviours(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
