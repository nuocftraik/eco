
using ECO.WebApi.Application.Identity.Roles;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Identity;
public class RoleController : BaseApiController
{
    private readonly IRoleService _roleService;
    private readonly IFunctionService _funtionService;
    public RoleController(IRoleService roleService,IFunctionService functionService)
    {
        _roleService = roleService;
        _funtionService = functionService;
    }

    [HttpGet]
    [OpenApiOperation("Get a list of all roles.", "")]
    public Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken)
    {
        return _roleService.GetListAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    [OpenApiOperation("Get role details.", "")]
    public Task<RoleDto> GetByIdAsync(string id)
    {
        return _roleService.GetByIdAsync(id);
    }

    [HttpGet("{id}/permissions")]
    [OpenApiOperation("Get role details with its permissions.", "")]
    public Task<List<FunctionDto>> GetByIdWithPermissionsAsync(string id, CancellationToken cancellationToken)
    {
        return _roleService.GetByIdWithPermissionsAsync(id, cancellationToken);
    }

    [HttpPut("{id}/permissions")]
    [OpenApiOperation("Update a role's permissions.", "")]
    public async Task<ActionResult> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RoleId)
        {
            return BadRequest();
        }


        var result = await _roleService.UpdatePermissionsAsync(request, cancellationToken);
        return Ok(new { message = result });
    }

    [HttpPost("create/update")]
    [OpenApiOperation("Create or update a role.", "")]
    public async Task<ActionResult> RegisterRoleAsync(CreateOrUpdateRoleRequest request)
    {
        var result = await _roleService.CreateOrUpdateAsync(request);

        // Trả về thông điệp dưới dạng đối tượng JSON
        return Ok(new { message = result });
    }

    [HttpDelete("{id}")]
    [OpenApiOperation("Delete a role.", "")]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        var result = await _roleService.DeleteAsync(id);
        return Ok(new { message = result });
    }

    //write controller for get function list
    [HttpGet("functions")]
    [OpenApiOperation("Get a list of all functions.", "")]
    public Task<List<FunctionDto>> GetFunctionListAsync(CancellationToken cancellationToken)
    {
        return _funtionService.GetListAsync(cancellationToken);
    }

    //write controller for get function by id
    [HttpGet("function/{id}")]
    [OpenApiOperation("Get function details.", "")]
    public Task<FunctionDto> GetFunctionByIdAsync(Guid id)
    {
        return _funtionService.GetByIdAsync(id);
    }

    //write controller for create function 
    [HttpPost("function/create/update")]
    [OpenApiOperation("Create or update a function.", "")]
    public async Task<ActionResult> CreateUpdateFunctionAsync(CreateOrUpdateFunctionRequest request)
    {
        var result = await _funtionService.CreateOrUpdateAsync(request);

        return Ok(new { message = result });
    }

    //write controller for delete function
    [HttpDelete("function/{id}")]
    [OpenApiOperation("Delete a function.", "")]
    public async Task<ActionResult> DeleteFunctionAsync(Guid id)
    {
        var result = await _funtionService.DeleteAsync(id);
        return Ok(new { message = result });
    }



}
