

namespace ECO.WebApi.Application.Identity.Roles;
public class UpdateRolePermissionsRequest
{
    public string RoleId { get; set; } = default!;
    public List<PermissionRequest> Permissions { get; set; } = default!;
}

public class PermissionRequest
{
    public Guid FunctionId { get; set; } = default!;
    public Guid ActionId { get; set; } = default!;
}
public class UpdateRolePermissionsRequestValidator : AbstractValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsRequestValidator()
    {
        RuleFor(r => r.RoleId)
            .NotEmpty();
    }
}
