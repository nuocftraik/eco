

namespace ECO.WebApi.Domain.Identity;
public class Permission
{
    public string RoleId { get; set; }
    public string FunctionId { get; set; }
    public string ActionId { get; set; }

    public Permission(string roleId, string functionId, string actionId)
    {
        RoleId = roleId;
        FunctionId = functionId;
        ActionId = actionId;
    }
}
