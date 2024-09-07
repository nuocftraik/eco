
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Mailing;
internal static class Startup
{
    internal static IServiceCollection AddMailing(this IServiceCollection services, IConfiguration config) =>
        services.Configure<SMTPEmailSettings>(config.GetSection(nameof(SMTPEmailSettings)));
}
