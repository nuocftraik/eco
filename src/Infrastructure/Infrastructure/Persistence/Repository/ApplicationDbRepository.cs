
using Ardalis.Specification.EntityFrameworkCore;
using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Domain.Common.Contracts;
using ECO.WebApi.Infrastructure.Persistence.Context;

namespace ECO.WebApi.Infrastructure.Persistence.Repository;
public class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

}
