using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly ApplicationDbContext _context;
    public DatabaseInitializer(ApplicationDbContext context, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        // First create a new scope
        using var scope = _serviceProvider.CreateScope();

        // Then run the initialization in the new scope
        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(cancellationToken);
    }
}

//Mục đích: DatabaseInitializer là lớp thực thi IDatabaseInitializer và chịu trách nhiệm thực hiện
//quá trình khởi tạo cơ sở dữ liệu.Nó cung cấp một cách để khởi tạo cơ sở dữ liệu trong một phạm vi (scope)