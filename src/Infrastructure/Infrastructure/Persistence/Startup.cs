using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Infrastructure.Common;
using ECO.WebApi.Infrastructure.Persistence.ConnectionString;
using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
//using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ECO.WebApi.Infrastructure.Persistence;
internal static class Startup
{
    private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

    internal static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddOptions<DatabaseSettings>()
            .BindConfiguration(nameof(DatabaseSettings))
            .PostConfigure(databaseSettings =>
            {
                _logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddDbContext<ApplicationDbContext>((p, m) =>
            {
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            })

            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>();
    }

    internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString) => dbProvider.ToLowerInvariant() switch
    {
      
        DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString, e =>
                             e.MigrationsAssembly("Migrators.MSSQL")),
        //DbProviderKeys.Npgsql => builder.UseNpgsql(connectionString, e =>
        //                      e.MigrationsAssembly("Migrators.PostgreSQL")),
        //DbProviderKeys.MySql => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), e =>
        //                     e.MigrationsAssembly("Migrators.MySQL")
        //                      .SchemaBehavior(MySqlSchemaBehavior.Ignore)),
    };


}