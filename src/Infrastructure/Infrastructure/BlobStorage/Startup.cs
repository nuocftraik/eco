using ECO.WebApi.Application.Common.BlobStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.BlobStorage;
internal static class Startup
{
    internal static IServiceCollection AddBlobStorage(IServiceCollection services)
    {
        return services.AddSingleton<IBlobStorageService, BlobStorageService>();

    }

}
