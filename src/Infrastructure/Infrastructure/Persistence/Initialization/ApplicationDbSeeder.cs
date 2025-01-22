

using System.Reflection;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;
internal class ApplicationDbSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedActionsAndFunctionsAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }


    private async Task SeedActionsAndFunctionsAsync(ApplicationDbContext dbContext)
    {
        // Seed actions
        var actions = typeof(ECOAction)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && !field.IsInitOnly) // Chỉ lấy các hằng số
            .Select(field => field.GetValue(null)?.ToString())
            .Where(value => value != null) // Loại bỏ các giá trị null
            .ToList();

        foreach (var action in actions)
        {
            if (!await dbContext.Actions.AnyAsync(x => x.Name == action))
            {
                _logger.LogInformation($"Seeding action {action}.");
                dbContext.Actions.Add(new Domain.Identity.Action
                {
                    Name = action
                });
                await dbContext.SaveChangesAsync();
            }
        }

        var functions = typeof(ECOFunction)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && !field.IsInitOnly) // Chỉ lấy các hằng số
            .Select(field => field.GetValue(null)?.ToString())
            .Where(value => value != null) // Loại bỏ các giá trị null
            .ToList();

        foreach (var functionName in functions)
        {
            if (!await dbContext.Functions.AnyAsync(f => f.Name == functionName))
            {
                _logger.LogInformation($"Seeding function {functionName}.");
                dbContext.Functions.Add(new Function { Name = functionName });
                await dbContext.SaveChangesAsync();
            }
        }

        // Seed actions in functions
        foreach (var functionName in functions)
        {
            var function = await dbContext.Functions.SingleAsync(f => f.Name == functionName);
            foreach (var actionName in actions)
            {
                var action = await dbContext.Actions.SingleAsync(a => a.Name == actionName);
                if (!await dbContext.ActionInFunctions.AnyAsync(aif => aif.FunctionId == function.Id && aif.ActionId == action.Id))
                {
                    _logger.LogInformation($"Seeding action {actionName} in function {functionName}.");
                    dbContext.ActionInFunctions.Add(new ActionInFunction(action.Id,function.Id));
                    await dbContext.SaveChangesAsync();
                }
            }
        }

    }
    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {
        foreach (string roleName in ECORoles.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                is not ApplicationRole role)
            {
                // Create the role
                _logger.LogInformation("Seeding {role} Role for system.", roleName);
                role = new ApplicationRole(roleName, $"{roleName} Role ");
                await _roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == ECORoles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, role ,true);
            }
            else if (roleName == ECORoles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, role ,false);


            }
        }
    }
    private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, ApplicationRole role, bool isBasic)
    {
        var currentPermissions = await dbContext.Permissions.Where(x => x.RoleId == role.Id).ToListAsync();

        var functions = await dbContext.Functions.ToListAsync(); // Lấy tất cả các Function
        foreach (var function in functions)
        {
            var actionsInFunction = await dbContext.ActionInFunctions.Include(x => x.Action).Where(a => a.FunctionId == function.Id).ToListAsync();
            foreach (var actionInFunction in actionsInFunction)
            {
                // Kiểm tra nếu vai trò là Basic và hành động không phải là quyền cơ bản thì bỏ qua
                if (isBasic && !IsBasicPermission(actionInFunction.Action.Name, function.Name))
                {
                    continue;
                }

                var permissionName = $"{function.Name}.{actionInFunction.Action.Name}";

                // Kiểm tra nếu permission đã tồn tại
                if (!currentPermissions.Any(p => p.FunctionId == function.Id && p.ActionId == actionInFunction.ActionId))
                {
                    _logger.LogInformation("Seeding {role} Permission '{permissionName}'.", role.Name, permissionName);

                    // Thêm Permission mới vào database
                    dbContext.Permissions.Add(new Permission(role.Id,function.Id,actionInFunction.ActionId));
                }
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private bool IsBasicPermission(string actionName, string functionName)
    {
        // Định nghĩa các quyền cơ bản
        var basicPermissions = new List<string>
    {   
        $"{ECOAction.View}.{ECOFunction.Dashboard}",
        $"{ECOAction.View}.{ECOFunction.Category}",
        $"{ECOAction.Search}.{ECOFunction.Category}",
        $"{ECOAction.View}.{ECOFunction.Product}",
        $"{ECOAction.Search}.{ECOFunction.Product}"
        // Thêm các quyền cơ bản khác nếu cần
    };

        return basicPermissions.Contains($"{actionName}.{functionName}");
    }


    private async Task SeedAdminUserAsync()
    {

        if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == "admin@gmail.com")
     is not ApplicationUser adminUser)
        {
            string adminUserName = $"System.{ECORoles.Admin}".ToLowerInvariant();
            adminUser = new ApplicationUser
            {
                FirstName = "NuocFTraiK",
                LastName = ECORoles.Admin,
                Email = "admin@gmail.com",
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = "ADMIN@GMAIL.COM",
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                IsActive = true
            };

            _logger.LogInformation("Seeding Default Admin User for application");
            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, "Abcd@1234");
            await _userManager.CreateAsync(adminUser);
        }

        // Assign role to user
        if (!await _userManager.IsInRoleAsync(adminUser, ECORoles.Admin))
        {
            _logger.LogInformation("Assigning Admin Role to Admin {}");
            await _userManager.AddToRoleAsync(adminUser, ECORoles.Admin);
        }
    }
}
