using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Persistence.Context;
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options, IOptions<DatabaseSettings> dbSettings, ICurrentUser currentUser, ISerializerService serializer, IEventPublisher events)
        : base(options, dbSettings, currentUser, serializer, events)
    {
    }


}
