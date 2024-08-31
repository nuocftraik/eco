using ECO.WebApi.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace ECO.WebApi.Infrastructure.Common;
internal static class Startup
{
    internal static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped);


    // Method extend to register service dựa trên loại interface và lifetime được chỉ định.
    internal static IServiceCollection AddServices(this IServiceCollection services, Type interfaceType, ServiceLifetime lifetime)
    {
        // Tìm tất cả các kiểu trong AppDomain hiện tại và lọc các kiểu phù hợp với interfaceType.
        var interfaceTypes =
            AppDomain.CurrentDomain.GetAssemblies() // Lấy tất cả các assembly trong AppDomain hiện tại.
                .SelectMany(s => s.GetTypes()) // Lấy tất cả các types từ mỗi assembly.
                .Where(t => interfaceType.IsAssignableFrom(t) // Kiểm tra xem type này có implement interfaceType không.
                            && t.IsClass && !t.IsAbstract) // Chỉ lấy các class cụ thể (không phải abstract).
                .Select(t => new
                {
                    Service = t.GetInterfaces().FirstOrDefault(), // Lấy interface đầu tiên mà class này implement.
                    Implementation = t // Giữ lại kiểu class này làm implementation.
                })
                .Where(t => t.Service is not null // Chỉ giữ lại các kiểu có interface không null.
                            && interfaceType.IsAssignableFrom(t.Service)); // Kiểm tra lại xem interface này có thỏa mãn interfaceType.

        // Lặp qua tất cả các kiểu được tìm thấy và đăng ký chúng vào IServiceCollection.
        foreach (var type in interfaceTypes)
        {
            services.AddService(type.Service!, type.Implementation, lifetime); // Đăng ký dịch vụ với vòng đời được chỉ định.
        }

        return services; // Trả về IServiceCollection sau khi đăng ký các dịch vụ.
    }

    internal static IServiceCollection AddService(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            _ => throw new ArgumentException("Invalid lifeTime", nameof(lifetime))
        };
}