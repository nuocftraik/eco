
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ECO.WebApi.Application.Identity.Roles;
public static class ClaimExtensions
{
    public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
        {
            await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }
    }
}
