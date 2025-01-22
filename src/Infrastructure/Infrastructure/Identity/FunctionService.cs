
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Identity.Roles;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Persistence.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Identity;
public class FunctionService : IFunctionService
{   
    private readonly ApplicationDbContext _db;
    public FunctionService(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<string> CreateOrUpdateAsync(CreateOrUpdateFunctionRequest request)
    {
        // if request.Id is null, create a new function
        if (request.Id == null)
        {
            var function = new Function { Name = request.Name };
            foreach (var actionId in request.ActionIds)
            {
                function.AddAction(actionId);
            }
            _db.Functions.Add(function);
            await _db.SaveChangesAsync();
            return function.Id.ToString();
        }
        else
        {
            var func = _db.Functions.FirstOrDefault(x => x.Id == request.Id) ?? throw new NotFoundException("Function not found");
            func.UpdateActions(request.ActionIds);
            await _db.SaveChangesAsync();
            return func.Id.ToString();
        }


    }

    public Task<string> DeleteAsync(Guid id)
    {
         var func = _db.Functions.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException("Function not found");
        _db.Functions.Remove(func);
        return Task.FromResult(id.ToString());
    }

    public async Task<FunctionDto> GetByIdAsync(Guid id)
    {
        var func = await _db.Functions.Include(x => x.ActionInFunctions).ThenInclude(x => x.Action).FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Function not found");
        return func.Adapt<FunctionDto>();
    }

    public async Task<List<FunctionDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var functions = await _db.Functions.Include(x => x.ActionInFunctions).ThenInclude(x => x.Action).ToListAsync();
        return functions.Adapt<List<FunctionDto>>();
    }
}
