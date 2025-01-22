using System.Linq;
using System.Reflection;
using System.Threading;
using DocumentFormat.OpenXml.Office2010.Excel;
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Application.Common.Exceptions;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Application.Identity.Roles;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Shared.Authorization;
using Google.Apis.Drive.v3.Data;
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
        // Lấy tất cả các quyền từ database
        var allPermissions = await _db.Permissions.Include(x => x.Function).Include(x => x.Action)
            .Select(p => new PermissionDto
            {
                Value = ECOPermission.NameFor(p.Function.Name, p.Action.Name),
                RoleId = p.RoleId,
                ActionId = p.ActionId.ToString(),
                FunctionId = p.FunctionId.ToString(),
            })
            .ToListAsync(cancellationToken);

        // Tìm role theo roleId
        var role = await _roleManager.FindByIdAsync(roleId) ?? throw new Exception("Role not found");

        model.RoleId = roleId;

        var rolePermissions = await GetPermissionsByRole(roleId, cancellationToken);

        // Đánh dấu các quyền đã được gán cho role
        foreach (var permission in allPermissions)
        {
            permission.Selected = rolePermissions.Contains(permission.Value);
        }

        model.Permissions = allPermissions;
        return model;
    }

    private async Task<List<string>> GetPermissionsByRole(string roleId, CancellationToken cancellationToken)
    {
        return await _db.Permissions.Include(x => x.Function).Include(x => x.Action)
            .Where(p => p.RoleId == roleId)
            .Select(p => ECOPermission.NameFor(p.Function.Name, p.Action.Name))
            .ToListAsync(cancellationToken);
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
        var currentPermissions = await _db.Permissions.Where(p => p.RoleId == role.Id).ToListAsync(cancellationToken);

        _db.Permissions.RemoveRange(currentPermissions);
        await _db.SaveChangesAsync(cancellationToken);

        // Thêm các quyền mới từ request.Permissions
        foreach (var permissionRequest in request.Permissions)
        {
            if (!string.IsNullOrEmpty(permissionRequest.FunctionId.ToString()) && !string.IsNullOrEmpty(permissionRequest.ActionId.ToString()))
            {
                // Thêm quyền mới vào bảng Permission
                _db.Permissions.Add(new Domain.Identity.Permission(role.Id, permissionRequest.FunctionId, permissionRequest.ActionId));
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




