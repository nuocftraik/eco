
namespace ECO.WebApi.Application.Identity.Roles;
public class RolePermissionDto
{
    public string RoleId { get; set; }
    public List<PermissionDto> Permissions { get; set; }
}
