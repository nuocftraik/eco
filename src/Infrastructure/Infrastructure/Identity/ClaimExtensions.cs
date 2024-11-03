
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ECO.WebApi.Application.Identity.Roles;
public static class ClaimExtensions
{
    public static void GetPermissions(this List<PermissionDto> allPermissions, Type policy)
    {
        FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (FieldInfo fi in fields)
        {
            var attribute = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
            string displayName = fi.GetValue(null).ToString();
            var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attributes.Length > 0)
            {
                var description = (DescriptionAttribute)attribute[0];
                displayName = description.Description;
            }
            var permissionValue = fi.GetValue(null) as ECOPermission;
            if (permissionValue != null)
            {
                allPermissions.Add(new PermissionDto
                {
                    Value = permissionValue.Name,  // Sử dụng Name của ECOPermission để lưu giá trị
                    Type = "Permission",
                    DisplayName = displayName
                });
            }
        }
    }
    public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
        {
            await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }
    }
}
