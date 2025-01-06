
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace ECO.WebApi.Infrastructure.FileStorage;
internal static class Startup
{
    internal static IApplicationBuilder UseFileStorage(this IApplicationBuilder app)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

        // Check if the directory exists, and if not, create it.
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        // Continue using the static file provider
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new PhysicalFileProvider(filePath),
            RequestPath = new PathString("/Files")
        });

        return app;
    }
}
