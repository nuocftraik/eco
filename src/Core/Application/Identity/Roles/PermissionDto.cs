

namespace ECO.WebApi.Application.Identity.Roles;
public class PermissionDto
{
    public string RoleId { get; set; }
    public string FunctionId { get; set; }
    public string ActionId { get; set; }
    public string Value { get; set; }
    public bool Selected { get; set; }
}
