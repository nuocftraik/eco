using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Domain.Common.Contracts;
using ECO.WebApi.Infrastructure.Common;
using ECO.WebApi.Infrastructure.Persistence.ConnectionString;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Infrastructure.Persistence.Initialization;
using ECO.WebApi.Infrastructure.Persistence.Repository;
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
            .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            .AddTransient<ApplicationDbInitializer>()
            .AddTransient<ApplicationDbSeeder>()
            .AddTransient<CustomSeederRunner>()
            //.AddTransient(typeof(ICustomSeeder))
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()

            .AddRepositories();
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


    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {

        foreach (var aggregateRootType in
            typeof(IAggregateRoot).Assembly.GetExportedTypes()
                .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
                .ToList())
        {
            // Add ReadRepositories.
            services.AddScoped(typeof(IReadRepository<>).MakeGenericType(aggregateRootType), sp =>
                sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

            // Decorate the repositories with EventAddingRepositoryDecorators and expose them as IRepositoryWithEvents.
            services.AddScoped(typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), sp =>
                Activator.CreateInstance(
                    typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
                    sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
                ?? throw new InvalidOperationException($"Couldn't create EventAddingRepositoryDecorator for aggregateRootType {aggregateRootType.Name}"));
        }

        return services;
    }

}