using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Persistence.Context;
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options, IOptions<DatabaseSettings> dbSettings)
        : base(options, dbSettings)
    {
    }
}