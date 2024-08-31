using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;
internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    public ApplicationDbInitializer(ApplicationDbContext dbContext, ILogger<ApplicationDbInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
       
    }
}
