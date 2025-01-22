
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ECO.WebApi.Infrastructure.Auth.Permissions;
public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string action, string function)
    {
        Policy = ECOPermission.NameFor(action, function);
    }
}

