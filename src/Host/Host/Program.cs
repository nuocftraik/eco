using ECO.WebApi.Infrastructure;
using ECO.WebApi.Host.Configurations;
using ECO.WebApi.Infrastructure.Common;
using Serilog;
using ECO.WebApi.Infrastructure.Logging;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.AddConfigurations().RegisterSerilog();
    builder.Services.AddControllers();
    builder.Services.AddInfrastructure(builder.Configuration);
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    await app.Services.InitializeDatabasesAsync();

    app.UseInfrastructure();
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
