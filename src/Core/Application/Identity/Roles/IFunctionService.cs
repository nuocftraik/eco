

namespace ECO.WebApi.Application.Identity.Roles;
public interface IFunctionService : ITransientService
{
    Task<List<FunctionDto>> GetListAsync(CancellationToken cancellationToken);

    Task<FunctionDto> GetByIdAsync(Guid id);

    Task<string> CreateOrUpdateAsync(CreateOrUpdateFunctionRequest request);

    Task<string> DeleteAsync(Guid id);
}
