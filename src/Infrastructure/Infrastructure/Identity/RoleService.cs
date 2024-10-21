using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Office2010.Excel;
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Application.Identity.Roles;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace ECO.WebApi.Infrastructure.Identity;
internal class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IEventPublisher _events;

    public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db, ICurrentUser currentUser, IEventPublisher events)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
        _currentUser = currentUser;
        _events = events;
    }

    public async Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken)
    {
        return (await _roleManager.Roles.ToListAsync(cancellationToken))
              .Adapt<List<RoleDto>>();
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken)
    {
        return await _roleManager.Roles.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string roleName, string? excludeId)
    {
        return await _roleManager.FindByNameAsync(roleName)
              is ApplicationRole existingRole
              && existingRole.Id != excludeId;
    }

    public async Task<RoleDto> GetByIdAsync(string id)
    {
        return await _db.Roles.SingleOrDefaultAsync(x => x.Id == id) is { } role
          ? role.Adapt<RoleDto>()
          : throw new NotFoundException("Role Not Found");
    }
    public async Task<RolePermissionDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken)
    {
        var model = new RolePermissionDto();
        var allPermissions = new List<PermissionDto>();

        // Lấy tất cả các permission từ ECOPermissions
        var permissions = ECOPermissions.All; // Lấy tất cả quyền từ ECOPermissions.All
        foreach (var permission in permissions)
        {
            allPermissions.Add(new PermissionDto
            {
                Value = ECOPermission.NameFor(permission.Action, permission.Resource),
                Type = "Permission",
                DisplayName = permission.Description
            });
        }

        // Tìm role theo roleId
        var role = await _roleManager.FindByIdAsync(roleId) ?? throw new Exception("Role not found");

        model.RoleId = roleId;

        // Lấy tất cả các claim của role
        var claims = await _roleManager.GetClaimsAsync(role);

        var roleClaimValues = claims.Select(a => a.Value).ToList();

        // Đánh dấu các quyền đã được gán cho role
        foreach (var permission in allPermissions)
        {
            permission.Selected = roleClaimValues.Contains(permission.Value);
        }

        model.Permissions = allPermissions;
        return model;
    }

    public async Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            // Create a new role.
            var role = new ApplicationRole(request.Name, request.Description);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new InternalServerException("Register role failed");
            }

            return string.Format("Role {0} Created.", request.Name);
        }
        else
        {
            // Update an existing role.
            var role = await _roleManager.FindByIdAsync(request.Id);

            _ = role ?? throw new NotFoundException("Role Not Found");

            if (ECORoles.IsDefault(role.Name!))
            {
                throw new ConflictException(string.Format("Not allowed to modify {0} Role.", role.Name));
            }

            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();
            role.Description = request.Description;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new InternalServerException("Update role failed");
            }
            return string.Format("Role {0} Updated.", role.Name);
        }
    }

    public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        _ = role ?? throw new NotFoundException("Role Not Found");
        if (role.Name == ECORoles.Admin)
        {
            throw new ConflictException("Not allowed to modify Permissions for this Role.");
        }


        var currentClaims = await _roleManager.GetClaimsAsync(role);

        // Remove permissions that were previously selected
        foreach (var claim in currentClaims.Where(c => !request.Permissions.Any(p => p == c.Value)))
        {
            var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
            if (!removeResult.Succeeded)
            {
                throw new InternalServerException("Update permissions failed.");
            }
        }

        // Add all permissions that were not previously selected
        foreach (string permission in request.Permissions.Where(c => !currentClaims.Any(p => p.Value == c)))
        {
            if (!string.IsNullOrEmpty(permission))
            {
                _db.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ECOClaims.Permission,
                    ClaimValue = permission,
                    CreatedBy = _currentUser.GetUserId().ToString()
                });
                await _db.SaveChangesAsync(cancellationToken);
            }
        }


        return "Permissions Updated.";
    }

    public async Task<string> DeleteAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        _ = role ?? throw new NotFoundException("Role Not Found");

        if (ECORoles.IsDefault(role.Name!))
        {
            throw new ConflictException(string.Format("Not allowed to delete {0} Role.", role.Name));
        }

        if ((await _userManager.GetUsersInRoleAsync(role.Name!)).Count > 0)
        {
            throw new ConflictException(string.Format("Not allowed to delete {0} Role as it is being used.", role.Name));
        }

        await _roleManager.DeleteAsync(role);


        return string.Format("Role {0} Deleted.", role.Name);
    }






}
