
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Identity;
internal partial class UserService
{
    public async Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        _ = user ?? throw new UnauthorizedException("Authentication Failed.");

        var userRoles = await _userManager.GetRolesAsync(user);

        var permissions = await _db.Permissions.Include(x => x.Role).Include(x => x.Action).Include(x => x.Function)
        .Where(p => userRoles.Contains(p.Role.Id)) 
        .Select(p => $"{p.Function.Name}.{p.Action.Name}") 
        .Distinct()
        .ToListAsync(cancellationToken);


        return permissions.Distinct().ToList();
    }


    // Kiểm tra xem user có quyền cụ thể hay không
    public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken)
    {
        var permissions = await GetPermissionsAsync(userId, cancellationToken);  // Lấy danh sách quyền của user
        return permissions?.Contains(permission) ?? false;  // Kiểm tra quyền
    }

}
