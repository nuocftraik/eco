# Database Initialization và Seed Data

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn chi tiết về Database Initialization và Seed Data - một trong những phần quan trọng nhất của base project.

---

## Bước 8.1: Tạo Interface IDatabaseInitializer

**Làm gì:** Tạo interface để abstract database initialization.

**Tại sao:** 
- Dễ test
- Có thể swap implementation
- Follow Dependency Inversion Principle

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/IDatabaseInitializer.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
}
```

**Tác dụng:** Interface này cho phép chúng ta dễ dàng test và thay đổi implementation nếu cần.

---

## Bước 8.2: Tạo DatabaseInitializer

**Làm gì:** Implementation chính, tạo scope và gọi ApplicationDbInitializer.

**Tại sao tạo scope:**
- `ApplicationDbInitializer` cần scoped services (DbContext)
- Phải tạo scope riêng vì được gọi từ root service provider

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/DatabaseInitializer.cs`

```csharp
using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly ApplicationDbContext _context;
    
    public DatabaseInitializer(
        ApplicationDbContext context, 
        IServiceProvider serviceProvider, 
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        // Tạo scope mới để lấy scoped services
        using var scope = _serviceProvider.CreateScope();

        // Gọi ApplicationDbInitializer trong scope mới
        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(cancellationToken);
    }
}
```

**Lưu ý:** Phải tạo scope vì `ApplicationDbInitializer` cần `ApplicationDbContext` (scoped).

**Tại sao cần scope:**
- Root service provider không thể resolve scoped services trực tiếp
- DbContext là scoped service, cần scope riêng để quản lý lifecycle

---

## Bước 8.3: Tạo ApplicationDbInitializer

**Làm gì:** Class chịu trách nhiệm apply migrations và seed data.

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/ApplicationDbInitializer.cs`

```csharp
using ECO.WebApi.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ApplicationDbInitializer> _logger;
    private readonly ApplicationDbSeeder _dbSeeder;

    public ApplicationDbInitializer(
        ApplicationDbContext dbContext, 
        ILogger<ApplicationDbInitializer> logger, 
        ApplicationDbSeeder dbSeeder)
    {
        _dbContext = dbContext;
        _logger = logger;
        _dbSeeder = dbSeeder;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // 1. Kiểm tra có migrations không
        if (_dbContext.Database.GetMigrations().Any())
        {
            // 2. Apply pending migrations
            if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                _logger.LogInformation("Applying Migrations for system");
                await _dbContext.Database.MigrateAsync(cancellationToken);
            }

            // 3. Kiểm tra connection và seed data
            if (await _dbContext.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogInformation("Connection to system's Database Succeeded.");
                await _dbSeeder.SeedDatabaseAsync(_dbContext, cancellationToken);
            }
        }
    }
}
```

**Thứ tự thực hiện:**
1. Check migrations exist - Tránh lỗi nếu chưa có migrations
2. Apply pending migrations - Tự động apply migrations chưa được apply
3. Check connection - Đảm bảo database có thể kết nối
4. Seed data - Chạy seed data nếu connection thành công

**Tại sao check migrations trước:** Tránh lỗi nếu chưa có migrations (ví dụ: project mới).

**Tác dụng:**
- Tự động apply migrations khi app start
- Không cần chạy migration commands thủ công
- An toàn: chỉ apply nếu có migrations

---

## Bước 8.4: Tạo ApplicationDbSeeder

**Làm gì:** Class seed dữ liệu cơ bản: Actions, Functions, Roles, Admin User.

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/ApplicationDbSeeder.cs`

```csharp
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

    public ApplicationDbSeeder(
        RoleManager<ApplicationRole> roleManager, 
        UserManager<ApplicationUser> userManager, 
        CustomSeederRunner seederRunner, 
        ILogger<ApplicationDbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        // Thứ tự seed quan trọng!
        await SeedActionsAndFunctionsAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }
}
```

**Thứ tự seed (QUAN TRỌNG):**
1. **Actions và Functions** - Phải seed trước vì Roles cần chúng để tạo permissions
2. **Roles** - Phải seed trước vì Admin User cần role
3. **Admin User** - Seed user và assign role
4. **Custom Seeders** - Chạy các seeders tùy chỉnh (có thể cần Admin User)

**Tại sao thứ tự quan trọng:**
- Roles cần Actions và Functions để tạo permissions
- Admin User cần Roles để assign
- Custom seeders có thể cần Admin User (ví dụ: NotificationSeeder)

---

## Bước 8.5: Seed Actions và Functions

**Làm gì:** Seed Actions và Functions từ constants trong `ECOAction` và `ECOFunction`.

**Code trong ApplicationDbSeeder:**

```csharp
private async Task SeedActionsAndFunctionsAsync(ApplicationDbContext dbContext)
{
    // 1. Seed Actions từ ECOAction constants
    var actions = typeof(ECOAction)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(field => field.IsLiteral && !field.IsInitOnly) // Chỉ lấy constants
        .Select(field => field.GetValue(null)?.ToString())
        .Where(value => value != null)
        .ToList();

    foreach (var action in actions)
    {
        if (!await dbContext.Actions.AnyAsync(x => x.Name == action))
        {
            _logger.LogInformation($"Seeding action {action}.");
            dbContext.Actions.Add(new Domain.Identity.Action { Name = action });
            await dbContext.SaveChangesAsync();
        }
    }

    // 2. Seed Functions từ ECOFunction constants
    var functions = typeof(ECOFunction)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(field => field.IsLiteral && !field.IsInitOnly)
        .Select(field => field.GetValue(null)?.ToString())
        .Where(value => value != null)
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

    // 3. Seed ActionInFunction (many-to-many)
    foreach (var functionName in functions)
    {
        var function = await dbContext.Functions.SingleAsync(f => f.Name == functionName);
        foreach (var actionName in actions)
        {
            var action = await dbContext.Actions.SingleAsync(a => a.Name == actionName);
            if (!await dbContext.ActionInFunctions.AnyAsync(
                aif => aif.FunctionId == function.Id && aif.ActionId == action.Id))
            {
                _logger.LogInformation($"Seeding action {actionName} in function {functionName}.");
                dbContext.ActionInFunctions.Add(new ActionInFunction(action.Id, function.Id));
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
```

**Tại sao dùng Reflection:**
- Tự động lấy tất cả constants từ `ECOAction` và `ECOFunction`
- Không cần hard-code từng action/function
- Dễ maintain: thêm constant mới → tự động seed

**Tại sao check `AnyAsync()` trước:**
- Tránh duplicate data
- Idempotent: có thể chạy nhiều lần an toàn
- Không gây lỗi nếu data đã tồn tại

**Cách hoạt động:**
1. Dùng Reflection để lấy tất cả constants từ `ECOAction` và `ECOFunction`
2. Check xem đã tồn tại trong database chưa
3. Nếu chưa có → thêm vào database
4. Tạo relationship many-to-many giữa Actions và Functions

---

## Bước 8.6: Seed Roles

**Làm gì:** Seed roles và assign permissions.

**Code trong ApplicationDbSeeder:**

```csharp
private async Task SeedRolesAsync(ApplicationDbContext dbContext)
{
    foreach (string roleName in ECORoles.DefaultRoles)
    {
        // 1. Tạo role nếu chưa tồn tại
        if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
            is not ApplicationRole role)
        {
            _logger.LogInformation("Seeding {role} Role for system.", roleName);
            role = new ApplicationRole(roleName, $"{roleName} Role ");
            await _roleManager.CreateAsync(role);
        }

        // 2. Assign permissions cho role
        if (roleName == ECORoles.Basic)
        {
            await AssignPermissionsToRoleAsync(dbContext, role, isBasic: true);
        }
        else if (roleName == ECORoles.Admin)
        {
            await AssignPermissionsToRoleAsync(dbContext, role, isBasic: false);
        }
    }
}

private async Task AssignPermissionsToRoleAsync(
    ApplicationDbContext dbContext, 
    ApplicationRole role, 
    bool isBasic)
{
    var currentPermissions = await dbContext.Permissions
        .Where(x => x.RoleId == role.Id)
        .ToListAsync();

    var functions = await dbContext.Functions.ToListAsync();
    
    foreach (var function in functions)
    {
        var actionsInFunction = await dbContext.ActionInFunctions
            .Include(x => x.Action)
            .Where(a => a.FunctionId == function.Id)
            .ToListAsync();
            
        foreach (var actionInFunction in actionsInFunction)
        {
            // Basic role chỉ có một số permissions cơ bản
            if (isBasic && !IsBasicPermission(actionInFunction.Action.Name, function.Name))
            {
                continue;
            }

            var permissionName = $"{function.Name}.{actionInFunction.Action.Name}";

            // Chỉ thêm nếu chưa tồn tại
            if (!currentPermissions.Any(p => 
                p.FunctionId == function.Id && p.ActionId == actionInFunction.ActionId))
            {
                _logger.LogInformation("Seeding {role} Permission '{permissionName}'.", 
                    role.Name, permissionName);
                dbContext.Permissions.Add(
                    new Permission(role.Id, function.Id, actionInFunction.ActionId));
            }
        }
    }

    await dbContext.SaveChangesAsync();
}

private bool IsBasicPermission(string actionName, string functionName)
{
    var basicPermissions = new List<string>
    {
        $"{ECOAction.View}.{ECOFunction.Dashboard}",
        $"{ECOAction.View}.{ECOFunction.Category}",
        $"{ECOAction.Search}.{ECOFunction.Category}",
        $"{ECOAction.View}.{ECOFunction.Product}",
        $"{ECOAction.Search}.{ECOFunction.Product}"
    };

    return basicPermissions.Contains($"{actionName}.{functionName}");
}
```

**Tại sao:**
- **Basic role** chỉ có quyền xem và search (read-only) - bảo mật tốt hơn
- **Admin role** có tất cả permissions - full access
- Check `currentPermissions` để tránh duplicate
- Permission format: `{Function}.{Action}` (ví dụ: `Product.View`)

**Tác dụng:**
- Tự động assign permissions khi tạo role
- Dễ mở rộng: thêm function mới → tự động có permissions cho Admin
- Basic role chỉ có quyền cơ bản (bảo mật tốt hơn)

**Cách hoạt động:**
1. Lặp qua tất cả roles trong `ECORoles.DefaultRoles`
2. Tạo role nếu chưa tồn tại
3. Assign permissions:
   - Basic: chỉ một số permissions cơ bản (View, Search)
   - Admin: tất cả permissions (View, Search, Create, Update, Delete, etc.)

---

## Bước 8.7: Seed Admin User

**Làm gì:** Tạo admin user mặc định.

**Code trong ApplicationDbSeeder:**

```csharp
private async Task SeedAdminUserAsync()
{
    // 1. Kiểm tra admin user đã tồn tại chưa
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
        
        // 2. Hash password
        var password = new PasswordHasher<ApplicationUser>();
        adminUser.PasswordHash = password.HashPassword(adminUser, "Abcd@1234");
        
        // 3. Tạo user
        await _userManager.CreateAsync(adminUser);
    }

    // 4. Assign Admin role nếu chưa có
    if (!await _userManager.IsInRoleAsync(adminUser, ECORoles.Admin))
    {
        _logger.LogInformation("Assigning Admin Role to Admin User");
        await _userManager.AddToRoleAsync(adminUser, ECORoles.Admin);
    }
}
```

**Thông tin Admin mặc định:**
- Email: `admin@gmail.com`
- Username: `system.admin`
- Password: `Abcd@1234` ⚠️ **Nên đổi sau khi deploy**
- Role: `Admin`

**Tại sao:**
- Cần admin user để đăng nhập lần đầu
- `EmailConfirmed = true` để không cần verify email
- `IsActive = true` để có thể đăng nhập ngay
- `NormalizedEmail` và `NormalizedUserName` để search case-insensitive

**Lưu ý:** ⚠️ **Đổi password trong production!**

---

## Bước 8.8: Tạo ICustomSeeder Interface

**Làm gì:** Tạo interface để cho phép custom seeders.

**Tại sao:**
- Cho phép mỗi module tự seed data của mình
- Tách biệt concerns
- Dễ mở rộng

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/ICustomSeeder.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
```

**Tác dụng:** Bất kỳ class nào implement interface này sẽ tự động được gọi khi seed data.

**Cách sử dụng:**
1. Tạo class implement `ICustomSeeder`
2. Đăng ký trong DI (tự động nếu dùng `AddServices()`)
3. Seeder sẽ tự động chạy khi `ApplicationDbSeeder.SeedDatabaseAsync()` được gọi

---

## Bước 8.9: Tạo CustomSeederRunner

**Làm gì:** Class chạy tất cả custom seeders.

**File:** `src/Infrastructure/Infrastructure/Persistence/Initialization/CustomSeederRunner.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}
```

**Cách hoạt động:**
1. Constructor nhận `IServiceProvider`
2. `GetServices<ICustomSeeder>()` - Lấy tất cả implementations của `ICustomSeeder`
3. Chạy từng seeder tuần tự

**Tại sao dùng `GetServices()`:**
- Có thể có nhiều custom seeders
- Tự động discover tất cả implementations
- Không cần đăng ký từng cái một

**Tại sao chạy tuần tự:**
- Đảm bảo thứ tự (nếu cần)
- Dễ debug nếu có lỗi
- Tránh conflict nếu seeders phụ thuộc nhau

---

## Bước 8.10: Ví dụ Custom Seeder - NotificationSeeder

**Làm gì:** Ví dụ cách tạo custom seeder.

**File:** `src/Infrastructure/Infrastructure/Notifications/NotificationSeeder.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Notifications;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ECO.WebApi.Infrastructure.Notifications;

public class NotificationSeeder : ICustomSeeder
{
    private readonly ISerializerService _serializerService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationSeeder> _logger;

    public NotificationSeeder(
        ISerializerService serializerService, 
        ApplicationDbContext db, 
        ILogger<NotificationSeeder> logger)
    {
        _serializerService = serializerService;
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // 1. Đọc file JSON chứa notification data
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string dataPath = Path.Combine(path!, "Notifications", "notificationData.json");
        
        // 2. Chỉ seed nếu chưa có data
        if (!_db.Notifications.Any())
        {
            _logger.LogInformation("Started to Seed Notifications."); 
            
            // 3. Đọc và deserialize JSON
            string notificationData = await File.ReadAllTextAsync(dataPath, cancellationToken);
            var notifications = _serializerService.Deserialize<List<Notification>>(notificationData);
            
            // 4. Lấy admin user để assign notifications
            var user = await _db.Users
                .Where(u => u.UserName == "system.admin")
                .FirstOrDefaultAsync();
                
            // 5. Assign receiver và add vào database
            foreach (var notification in notifications)
            {
                notification.ReceiverId = user.Id;
                _db.Notifications.Add(notification);
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded Notifications.");
        }
    }
}
```

**Cách sử dụng:**
1. Implement `ICustomSeeder`
2. Đăng ký trong DI (sẽ tự động nếu dùng `AddServices()`)
3. Seeder sẽ tự động chạy khi `ApplicationDbSeeder.SeedDatabaseAsync()` được gọi

**Tác dụng:**
- Mỗi module tự quản lý seed data của mình
- Có thể seed từ JSON, CSV, hoặc code
- Tự động chạy khi app start

**Lưu ý:**
- Check `Any()` trước để tránh duplicate
- Có thể inject bất kỳ service nào cần thiết
- File JSON nằm trong folder `Notifications/` của Infrastructure project

---

## Bước 8.11: Đăng ký Services trong Persistence Startup

**Làm gì:** Đăng ký tất cả initialization services.

**File:** `src/Infrastructure/Infrastructure/Persistence/Startup.cs`

**Code:**

```csharp
internal static IServiceCollection AddPersistence(this IServiceCollection services)
{
    // ... DbContext setup ...
    
    return services
        .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
        .AddTransient<ApplicationDbInitializer>()
        .AddTransient<ApplicationDbSeeder>()
        .AddTransient<CustomSeederRunner>()
        .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient) // Đăng ký tất cả custom seeders
        // ... other services ...
        .AddRepositories();
}
```

**Giải thích:**
- `AddTransient<IDatabaseInitializer, DatabaseInitializer>()` - Đăng ký main initializer
- `AddTransient<ApplicationDbInitializer>()` - Đăng ký application initializer
- `AddTransient<ApplicationDbSeeder>()` - Đăng ký seeder
- `AddTransient<CustomSeederRunner>()` - Đăng ký runner
- `AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)` - **Tự động đăng ký tất cả custom seeders**

**Tại sao dùng `AddServices()`:**
- Tự động scan và đăng ký tất cả implementations của `ICustomSeeder`
- Không cần đăng ký từng seeder một
- Dễ mở rộng: thêm seeder mới → tự động được đăng ký

**Tại sao dùng Transient:**
- Mỗi lần chạy initialization tạo instance mới
- Đảm bảo không có state còn lại từ lần chạy trước

---

## Bước 8.12: Gọi InitializeDatabasesAsync trong Program.cs

**Làm gì:** Gọi database initialization khi app start.

**File:** `src/Host/Host/Program.cs`

**Code:**

```csharp
var app = builder.Build();

// Initialize database (phải sau khi Build)
await app.Services.InitializeDatabasesAsync();

app.UseInfrastructure(builder.Configuration);
app.MapEndpoints();
app.Run();
```

**Extension method:** `src/Infrastructure/Infrastructure/Startup.cs`

```csharp
public static async Task InitializeDatabasesAsync(
    this IServiceProvider services, 
    CancellationToken cancellationToken = default)
{
    // Tạo scope mới để lấy scoped services
    using var scope = services.CreateScope();

    // Gọi IDatabaseInitializer
    await scope.ServiceProvider
        .GetRequiredService<IDatabaseInitializer>()
        .InitializeDatabasesAsync(cancellationToken);
}
```

**Tại sao tạo scope:**
- `IDatabaseInitializer` cần `ApplicationDbContext` (scoped)
- Root service provider không có scoped services
- Phải tạo scope riêng

**Thứ tự:**
1. Build app → Service provider sẵn sàng
2. Initialize database → Apply migrations + Seed data
3. Configure middleware → App sẵn sàng nhận requests

**Tác dụng:**
- Tự động apply migrations khi app start
- Tự động seed data nếu chưa có
- Không cần chạy migration commands thủ công (lần đầu)

**Lưu ý:**
- Phải gọi sau `builder.Build()` vì cần service provider
- Nên đặt trước middleware pipeline để đảm bảo database sẵn sàng

---

## Tóm tắt

### Thứ tự thực hiện:

1. **IDatabaseInitializer** → Interface để abstract
2. **DatabaseInitializer** → Tạo scope và gọi ApplicationDbInitializer
3. **ApplicationDbInitializer** → Apply migrations và seed data
4. **ApplicationDbSeeder** → Seed Actions, Functions, Roles, Admin User
5. **Custom Seeders** → Chạy các seeders tùy chỉnh

### Điểm quan trọng:

- **Thứ tự seed:** Actions/Functions → Roles → Admin User → Custom Seeders
- **Idempotent:** Có thể chạy nhiều lần an toàn (check `AnyAsync()` trước)
- **Scope management:** Phải tạo scope riêng cho scoped services
- **Auto-registration:** Custom seeders tự động được đăng ký

### Lợi ích:

- **Tự động hóa:** Không cần chạy migration/seed thủ công
- **Idempotent:** An toàn khi chạy nhiều lần
- **Modular:** Mỗi module tự quản lý seed data
- **Dễ mở rộng:** Thêm seeder mới rất dễ

---

**Tiếp theo:** [Service Registration Pattern](BUILD_08_Service_Registration.md)
