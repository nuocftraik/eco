# ECO.WebApi - Hướng dẫn Xây dựng Solution từ đầu

> 📘 **Mục đích:** Tài liệu này hướng dẫn **từng bước chi tiết** để xây dựng một Clean Architecture solution từ đầu.  
> Mỗi bước giải thích **làm gì**, **tại sao**, **thứ tự thực hiện**, và **code cụ thể**.

---

## 📋 Tổng quan

ECO.WebApi được xây dựng theo **Clean Architecture** với 5 layers:

```
┌─────────────────────────────────────────────────────────┐
│                     Host Layer                          │
│   ASP.NET Core API, Controllers, Program.cs             │
└────────────────────┬────────────────────────────────────┘
               ↓ depends on
┌────────────────────┴────────────────────────────────────┐
│                   Infrastructure Layer                  │
│ EF Core, Identity, Caching, Mailing, External Services  │
└────────────────────┬────────────────────────────────────┘
                ↓ depends on
┌────────────────────┴────────────────────────────────────┐
│           Application Layer                             │
│         Use Cases, DTOs, Interfaces, Validators         │
└────────────────────┬────────────────────────────────────┘
                ↓ depends on
┌────────────────────┴────────────────────────────────────┐
│               Domain Layer                              │
│        Entities, Value Objects, Domain Events, Enums    │
└────────────────────┬────────────────────────────────────┘
                ↓ depends on
┌────────────────────┴────────────────────────────────────┐
│                Shared Layer                             │
│          Common Contracts, Authorization Constants      │
└─────────────────────────────────────────────────────────┘
```

**Nguyên tắc dependency:**
- Shared: Không phụ thuộc vào layer nào
- Domain: Chỉ phụ thuộc Shared
- Application: Phụ thuộc Domain + Shared
- Infrastructure: Phụ thuộc Application + Domain
- Host: Phụ thuộc Infrastructure + Application

---

## 🎯 Lộ trình xây dựng (Build Roadmap)

### **PHASE 1: Foundation Setup** (Nền tảng)
Xây dựng cấu trúc cơ bản, build configuration, và layers trống.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 1 | [BUILD_01](BUILD_01_Solution_Setup.md) | Solution setup, build config | .NET 8 SDK |
| 2 | [BUILD_02](BUILD_02_Shared_Layer.md) | Shared layer (Authorization constants) | Bước 1 |
| 3 | [BUILD_03](BUILD_03_Domain_Layer.md) | Domain layer (Identity entities) | Bước 2 |
| 4 | [BUILD_04](BUILD_04_Application_Layer.md) | Application layer (MediatR, FluentValidation) | Bước 3 |
| 5 | [BUILD_05](BUILD_05_Infrastructure_Layer.md) | Infrastructure layer (DbContext, modular startup) | Bước 4 |
| 6 | [BUILD_06](BUILD_06_Host_Layer.md) | Host layer (Program.cs, Controllers) | Bước 5 |

**Kết quả Phase 1:** Solution build thành công, có thể chạy API (nhưng chưa có database).

---

### **PHASE 2: Database & Core Patterns** (Database + Core Logic)
Setup database, migrations, domain patterns, và repository.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 7 | [BUILD_07](BUILD_07_Database_Initialization.md) | Database setup, migrations, seeding | Phase 1 |
| 8 | [BUILD_08](BUILD_08_Service_Registration.md) | Auto service registration pattern | Bước 7 |
| 9 | [BUILD_09](BUILD_09_Domain_Base_Entities.md) | Base entities, Domain Events | Bước 8 |
| 10 | [BUILD_10](BUILD_10_Service_Registration.md) | Service registration pattern | Bước 9 |
| 11 | [BUILD_11](BUILD_11_Repository_Pattern.md) | Repository pattern, Specifications | Bước 10 |

**Kết quả Phase 2:** Database hoạt động, domain patterns complete, repository ready.

---

### **PHASE 3: Core Services** (Dịch vụ cơ bản)
Xây dựng các services nền tảng cho toàn hệ thống.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 12 | [BUILD_12](BUILD_12_Common_Services.md) | CurrentUser, Serializer, Event Publisher | Bước 11 |
| 13 | [BUILD_13](BUILD_13_Exceptions_Middleware.md) | Exception handling, Error responses | Bước 12 |
| 14 | [BUILD_14](BUILD_14_Validation_Behavior.md) | FluentValidation, MediatR Behaviors | Bước 13 |

**Kết quả Phase 3:** Core services hoạt động (CurrentUser, Serializer, Exception handling, Validation).

---

### **PHASE 4: Authentication & Authorization** (Bảo mật)
Xây dựng hệ thống authentication và authorization.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 15 | [BUILD_15](BUILD_15_JWT_Authentication.md) | JWT tokens, Token service | Phase 3 |
| 16A | [BUILD_16A](BUILD_16A_User_Service.md) | User management service | Bước 15 |
| 16B | [BUILD_16B](BUILD_16B_Role_Service.md) | Role management service | Bước 16A |
| 16C | [BUILD_16C](BUILD_16C_Function_Service.md) | Function (Permission) management service | Bước 16B |
| 16D | [BUILD_16D](BUILD_16D_Identity_Controllers.md) | Identity Controllers (User, Role, Token, Personal) | Bước 16C |
| 17 | [BUILD_17](BUILD_17_Permission_Authorization.md) | Permission-based authorization | Bước 16D |
| 18 | [BUILD_18](BUILD_18_OAuth2_Integration.md) | Google/Facebook OAuth2 login | Bước 17 |

**Kết quả Phase 4:** Authentication & Authorization hoàn chỉnh (JWT, Permissions, OAuth2).


---

### **PHASE 5: Foundation Patterns** (Patterns nền tảng)
Setup soft delete và auditing - nền tảng cho toàn hệ thống.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 19 | [BUILD_19](BUILD_19_Soft_Delete.md) | Soft Delete, Global Query Filters | Phase 4 |
| 20 | [BUILD_20](BUILD_20_Auditing.md) | Audit trails, Change tracking | Bước 19 |

**Kết quả Phase 5:** Soft delete và audit trail hoàn chỉnh - nền tảng cho data integrity và compliance.

---

### **PHASE 6: Infrastructure Services** (Dịch vụ hạ tầng)
Xây dựng các services hỗ trợ (caching, storage, jobs, email).

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 21 | [BUILD_21](BUILD_21_Caching_Services.md) | Local cache, Distributed cache (Redis) | Phase 5 |
| 22 | [BUILD_22](BUILD_22_File_Storage.md) | Local file storage, File upload/download | Bước 21 |
| 23 | [BUILD_23](BUILD_23_Email_Service.md) | SMTP email, Email templates (Razor) | Bước 22 |
| 24 | [BUILD_24](BUILD_24_Blob_Storage.md) | Azure Blob Storage, AWS S3 | Bước 23 |
| 25 | [BUILD_25](BUILD_25_Background_Jobs.md) | Hangfire background jobs | Bước 24 |
| 26 | [BUILD_26](BUILD_26_Logging.md) | Serilog, Seq, Elasticsearch | Bước 25 |

**Kết quả Phase 6:** Infrastructure services đầy đủ (Caching, Storage, Email, Jobs, Logging).

---

### **PHASE 7: Business Modules** (Modules nghiệp vụ)
Xây dựng các modules nghiệp vụ và tính năng nâng cao.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 27 | [BUILD_27](BUILD_27_Export_Services.md) | Excel export, Report generation | Phase 6 |
| 28 | [BUILD_28](BUILD_28_Catalog_Module.md) | Products, Categories CRUD | Bước 27 |
| 29 | [BUILD_29](BUILD_29_Notifications.md) | SignalR notifications, Real-time updates | Bước 28 |
| 30 | [BUILD_30](BUILD_30_Payment_Integration.md) | VNPay payment gateway | Bước 29 |

**Kết quả Phase 7:** Business modules complete (Export, Catalog, Notifications, Payment).

---

## 📚 Chi tiết các bước

### **PHASE 1: FOUNDATION SETUP**

#### **Bước 1: Solution và Build Configuration** ⭐
**File:** [BUILD_01_Solution_Setup.md](BUILD_01_Solution_Setup.md)

**Nội dung:**
1. Tạo solution file (`ECO.WebApi.sln`)
2. Tạo 6 projects theo thứ tự dependency
3. Setup `Directory.Build.props` (StyleCop, SonarAnalyzer)
4. Setup `Directory.Build.targets` (XML documentation)
5. Tạo `stylecop.json` (code style rules)
6. Tạo `.editorconfig` (editor formatting)

**Kết quả:** Solution structure hoàn chỉnh, build configuration áp dụng cho tất cả projects.

---

#### **Bước 2: Shared Layer** ⭐
**File:** [BUILD_02_Shared_Layer.md](BUILD_02_Shared_Layer.md)

**Nội dung:**
1. Setup `Shared.csproj` (no dependencies)
2. Tạo `ECOAction` constants (View, Create, Update, Delete...)
3. Tạo `ECOFunction` constants (Dashboard, User, Role...)
4. Tạo `ECORoles` constants (Admin, Basic)
5. Tạo `ECOClaims` constants (Fullname, Permission...)
6. Tạo `ECOPermission` record (generate permissions động)

**Kết quả:** Authorization constants sẵn sàng để dùng trong các layers khác.

---

#### **Bước 3: Domain Layer** ⭐
**File:** [BUILD_03_Domain_Layer.md](BUILD_03_Domain_Layer.md)

**Nội dung:**
1. Setup `Domain.csproj` (phụ thuộc Shared)
2. Add packages: `Microsoft.AspNetCore.Identity`, `NewId`
3. Tạo `ApplicationUser` entity (kế thừa IdentityUser)
4. Tạo `ApplicationRole` entity (kế thừa IdentityRole)
5. Tạo `ApplicationRoleClaim` entity
6. Tạo Custom Identity entities (Action, Function, Permission)
7. Tạo domain entities (Product, Category, Order...)

**Kết quả:** Domain entities hoàn chỉnh, không phụ thuộc infrastructure.

---

#### **Bước 4: Application Layer** ⭐
**File:** [BUILD_04_Application_Layer.md](BUILD_04_Application_Layer.md)

**Nội dung:**
1. Setup `Application.csproj` (phụ thuộc Domain + Shared)
2. Add packages: `MediatR`, `FluentValidation`, `Mapster`, `Ardalis.Specification`
3. Tạo `Startup.cs` (register MediatR, FluentValidation)
4. Tạo Common interfaces (ICurrentUser, ISerializerService, IRepository...)
5. Tạo Common models (BaseFilter, PaginationFilter, Search, Filter)
6. Setup GlobalUsings

**Kết quả:** Application layer foundation với core interfaces và models.

---

#### **Bước 5: Infrastructure Layer** ⭐
**File:** [BUILD_05_Infrastructure_Layer.md](BUILD_05_Infrastructure_Layer.md)

**Nội dung:**
1. Setup `Infrastructure.csproj` (phụ thuộc Application + Domain)
2. Add packages: `EF Core`, `Hangfire`, `Serilog`, `MailKit`...
3. Tạo `ApplicationDbContext` (kế thừa `BaseDbContext`)
4. Tạo modular `Startup.cs` pattern
5. Setup Persistence module (DbContext, Repository)

**Kết quả:** Infrastructure layer foundation với DbContext và modular startup.

---

#### **Bước 6: Host Layer** ⭐
**File:** [BUILD_06_Host_Layer.md](BUILD_06_Host_Layer.md)

**Nội dung:**
1. Setup `Host.csproj` (phụ thuộc Infrastructure + Application)
2. Add packages: `Swashbuckle`, `FluentValidation.AspNetCore`
3. Tạo `Program.cs` (configure middleware pipeline)
4. Tạo `BaseApiController`
5. Setup Swagger documentation
6. Tạo configuration files structure

**Kết quả:** API hoàn chỉnh, có thể chạy (chưa có database).

---

### **PHASE 2: DATABASE & CORE PATTERNS**

#### **Bước 7: Database Initialization và Seed Data** ⭐⭐⭐
**File:** [BUILD_07_Database_Initialization.md](BUILD_07_Database_Initialization.md)

**Nội dung quan trọng - thứ tự thực hiện:**

**7.1. Tạo Interfaces:**
```csharp
IDatabaseInitializer
ICustomSeeder
```

**7.2. Tạo Implementations (theo thứ tự dependency):**
```csharp
DatabaseInitializer (implement IDatabaseInitializer)
ApplicationDbInitializer (kế thừa DatabaseInitializer)
ApplicationDbSeeder
  ├── Seed Actions
  ├── Seed Functions
  ├── Seed Roles
  └── Seed Admin User
CustomSeederRunner
NotificationSeeder (implement ICustomSeeder)
```

**7.3. Register và Run:**
```csharp
services.AddScoped<IDatabaseInitializer, ApplicationDbInitializer>();
await app.Services...InitializeDatabasesAsync();
```

**Kết quả:** Database tự động migrate và seed Actions → Functions → Roles → Admin User.

---

#### **Bước 8: Service Registration Pattern** ⭐
**File:** [BUILD_08_Service_Registration.md](BUILD_08_Service_Registration.md)

**Nội dung:**
1. Tạo marker interfaces: `ITransientService`, `IScopedService`, `ISingletonService`
2. Tạo `AddServices()` extension method
3. Auto-register services bằng reflection
4. Apply cho Application và Infrastructure layers

**Kết quả:** Services tự động register, không cần thủ công.

---

#### **Bước 9: Domain Base Entities và Events** ⭐
**File:** [BUILD_09_Domain_Base_Entities.md](BUILD_09_Domain_Base_Entities.md)

**Nội dung:**
1. **IEvent interface** - Domain event marker
2. **DomainEvent base class** - With TriggeredOn timestamp
3. **IEntity interface** - Base entity contract với DomainEvents collection
4. **IAuditableEntity interface** - Created/Modified tracking 
5. **BaseEntity** - Sequential GUID generation, DomainEvents
6. **AuditableEntity** - Implement IAuditableEntity (Created/Modified only)
7. **IAggregateRoot** - Marker for aggregate roots
8. **Entity Lifecycle Events** - Created, Updated, Deleted events


---

#### **Bước 10: Service Registration Pattern** ⭐
**File:** [BUILD_10_Service_Registration.md](BUILD_10_Service_Registration.md)

**Nội dung:**
1. Marker interfaces (ITransientService, IScopedService, ISingletonService)
2. Auto-registration với reflection
3. Convention-based service discovery

**Kết quả:** Service registration pattern hoàn chỉnh.

---

#### **Bước 11: Repository Pattern và Specifications** ⭐⭐⭐
**File:** [BUILD_11_Repository_Pattern.md](BUILD_11_Repository_Pattern.md)

**Nội dung:**
1. Tạo Search/Filter models (Search, Filter, BaseFilter, PaginationFilter)
2. Tạo `IRepository<T>`, `IReadRepository<T>`, `IRepositoryWithEvents<T>`
3. Implement `ApplicationDbRepository<T>`
4. Tạo `EventAddingRepositoryDecorator<T>` (decorator pattern)
5. Tạo `SpecificationBuilderExtensions` (full code trong BUILD_11_Specification.md)
6. Tạo base specifications: `EntitiesByBaseFilterSpec`, `EntitiesByPaginationFilterSpec`

**Kết quả:** Repository pattern hoàn chỉnh với specification support và domain events.

---

### **PHASE 3: CORE SERVICES**

#### **Bước 12: Common Services** ⭐
**File:** [BUILD_12_Common_Services.md](BUILD_12_Common_Services.md)

**Nội dung:**
1. **CurrentUser:** `ICurrentUser`, `ICurrentUserInitializer`, `CurrentUser`, `CurrentUserMiddleware`
2. **Serializer:** `ISerializerService`, `NewtonSoftService`
3. **Event Publisher:** `IEventPublisher`, `EventPublisher` (MediatR integration)

**Kết quả:** Core services foundation (CurrentUser, Serializer, Events).

---

#### **Bước 13: Exception Handling & Middleware** ⭐
**File:** [BUILD_13_Exceptions_Middleware.md](BUILD_13_Exceptions_Middleware.md)

**Nội dung:**
1. Exception hierarchy: `CustomException`, `NotFoundException`, `UnauthorizedException`, `ForbiddenException`, `ConflictException`, `InternalServerException`
2. `ErrorResult` model
3. `ExceptionMiddleware` (global exception handler)
4. Register middleware pipeline

**Kết quả:** Exception handling hoàn chỉnh với proper HTTP status codes.

---

#### **Bước 14: Validation Behavior** ⭐
**File:** [BUILD_14_Validation_Behavior.md](BUILD_14_Validation_Behavior.md)

**Nội dung:**
1. FluentValidation setup
2. `ValidationBehavior<TRequest, TResponse>` (MediatR pipeline behavior)
3. Validation examples (CreateUserRequestValidator, UpdateProductRequestValidator)
4. Auto-register validators

**Kết quả:** Automatic validation cho tất cả MediatR requests.

---

### **PHASE 4: AUTHENTICATION & AUTHORIZATION**

#### **Bước 15: JWT Authentication** ⭐⭐⭐
**File:** [BUILD_15_JWT_Authentication.md](BUILD_15_JWT_Authentication.md)

**Nội dung:**
1. `JwtSettings` configuration
2. `ITokenService` interface
3. `TokenService` implementation (generate tokens, refresh tokens)
4. JWT authentication middleware setup
5. `TokenRequest`, `TokenResponse`, `RefreshTokenRequest` DTOs

**Kết quả:** JWT authentication hoàn chỉnh với refresh token support.

---

#### **Bước 16: Identity Services** ⭐⭐⭐

**Bước 16 bao gồm 4 phần:**

##### **Bước 16A: User Service (Complete)** ⭐⭐⭐
**File:** [BUILD_16A_User_Service.md](BUILD_16A_User_Service.md)

**Nội dung:**
1. **UserService:** Complete user management với TẤT CẢ operations
2. User DTOs (UserDetailDto, CreateUserRequest, UpdateUserRequest)
3. User CRUD operations (Search, Get, Create, Update, Toggle Status)
4. Email/Phone confirmation
5. Password operations (Forgot, Reset, Change) - Note: Chi tiết implementation trong file
6. Permission operations (GetPermissions, HasPermission với caching) - Note: Chi tiết implementation trong file
7. Email templates (registration + password reset)
8. FluentValidation cho tất cả requests

**Kết quả:** User management service HOÀN CHỈNH với tất cả operations.

---

##### **Bước 16B: Role Service** ⭐⭐⭐
**File:** [BUILD_16B_Role_Service.md](BUILD_16B_Role_Service.md)

**Nội dung:**
1. **RoleService:** CRUD roles, manage permissions
2. Role DTOs (RoleDto, CreateOrUpdateRoleRequest, UpdateRolePermissionsRequest)
3. Role specifications (RoleByNameSpec, RoleByIdSpec)
4. Permission management (Get, Update permissions for roles)

**Kết quả:** Role management service hoàn chỉnh.

---

##### **Bước 16C: Function Service** ⭐⭐⭐
**File:** [BUILD_16C_Function_Service.md](BUILD_16C_Function_Service.md)

**Nội dung:**
1. **FunctionService:** CRUD functions (Permission modules)
2. Function DTOs (FunctionDto, CreateOrUpdateFunctionRequest)
3. Function specifications (FunctionByIdSpec, FunctionByNameSpec)
4. Action management (Get actions for functions)

**Kết quả:** Function management service hoàn chỉnh.

---

##### **Bước 16D: Identity Controllers** ⭐⭐⭐
**File:** [BUILD_16D_Identity_Controllers.md](BUILD_16D_Identity_Controllers.md)

**Nội dung:**
1. **TokensController:** Login, Refresh token endpoints
2. **UsersController:** User management REST APIs
3. **RoleController:** Role & Function management APIs
4. **PersonalController:** Current user profile APIs
5. **ClaimsPrincipalExtensions:** Helper methods (GetUserId, GetEmail)
6. Swagger documentation với OpenAPI attributes

**Kết quả:** Identity Controllers hoàn chỉnh với REST APIs.

---

#### **Bước 17: Permission-based Authorization** ⭐⭐⭐
**File:** [BUILD_17_Permission_Authorization.md](BUILD_17_Permission_Authorization.md)

**Nội dung:**
1. `PermissionRequirement` (IAuthorizationRequirement)
2. `PermissionAuthorizationHandler` (check permissions from claims)
3. `PermissionPolicyProvider` (dynamic policy creation)
4. `MustHavePermissionAttribute` ([MustHavePermission("Users.View")])
5. Permission seeding in ApplicationDbSeeder

**Kết quả:** Permission-based authorization hoàn chỉnh.

---

#### **Bước 18: OAuth2 Integration** ⭐⭐
**File:** [BUILD_18_OAuth2_Integration.md](BUILD_18_OAuth2_Integration.md)

**Nội dung:**
1. Google OAuth2 setup (`GoogleAuthSettings`, configuration)
2. Facebook OAuth2 setup (`FacebookAuthSettings`, configuration)
3. `IAuthenticationService` interface
4. `AuthenticationService` implementation (login với Google/Facebook)
5. OAuth2 middleware configuration

**Kết quả:** Social login hoàn chỉnh (Google, Facebook).

---

### **PHASE 5: FOUNDATION PATTERNS**

#### **Bước 19: Soft Delete** ⭐⭐
**File:** [BUILD_19_Soft_Delete.md](BUILD_19_Soft_Delete.md)

**Nội dung:**
1. **ISoftDelete Interface:** Marker interface với DeletedOn, DeletedBy properties
2. **Update AuditableEntity:** Implement ISoftDelete (thêm DeletedOn, DeletedBy)
3. **Global Query Filter:** Tự động exclude deleted entities (`WHERE DeletedOn IS NULL`)
4. **AppendGlobalQueryFilter:** Extension method apply filter cho interfaces
5. **SaveChangesAsync Enhancement:** Convert `EntityState.Deleted → EntityState.Modified`
6. **Restore Methods:** Restore deleted entities
7. **Soft Delete Specifications:** Query deleted entities (OnlyDeletedSpec, IncludeDeletedSpec)
8. **API Endpoints:** Restore, permanent delete, get deleted

**Changes to existing code:**
```diff
// AuditableEntity.cs
- public abstract class AuditableEntity<T> : BaseEntity<T>, IAuditableEntity
+ public abstract class AuditableEntity<T> : BaseEntity<T>, IAuditableEntity, ISoftDelete
+ {
+     public DateTime? DeletedOn { get; set; }
+     public Guid? DeletedBy { get; set; }
+ }
```

**Key Features:**
- ✅ Global query filters tự động apply
- ✅ Soft delete thay vì physical delete
- ✅ Restore functionality
- ✅ Permanent delete option (with caution)
- ✅ Integration với BUILD_20 Auditing (tracks delete events)

**Kết quả:** Soft delete pattern complete - xóa mềm thay vì xóa vĩnh viễn.

---

#### **Bước 20: Auditing** ⭐⭐
**File:** [BUILD_20_Auditing.md](BUILD_20_Auditing.md)

**Nội dung:**
1. **Trail Entity:** Lưu audit logs trong database
2. **TrailType Enum:** Type-safe audit types (Create, Update, Delete)
3. **AuditTrail Helper:** Build audit trails từ EntityEntry
4. **Audit Interceptor:** Tự động capture changes trong SaveChangesAsync
5. **Soft Delete Detection:** Detect khi DeletedOn changed from null → value
6. **IAuditService:** Query audit logs
7. **GetMyAuditLogsRequest:** Current user audit logs
8. **PersonalController:** Expose audit logs via API

**Dependencies:**
- ✅ IAuditableEntity (từ BUILD_09) - Track Created/Modified
- ✅ ISoftDelete (từ BUILD_19) - Track soft delete events
- ✅ BaseDbContext (từ BUILD_05)
- ✅ ISerializerService (từ BUILD_12)

**Audit Integration:**
```csharp
// Audit automatically tracks soft delete
if (property.IsModified && 
    entry.Entity is ISoftDelete && 
    propertyName == nameof(ISoftDelete.DeletedOn) &&
    property.OriginalValue == null && 
    property.CurrentValue != null)
{
    trailEntry.TrailType = TrailType.Delete; // ✅ Log as Delete
}
```

**Kết quả:** Audit trail system complete - track tất cả thay đổi including soft delete.

---

### **PHASE 6: INFRASTRUCTURE SERVICES**

#### **Bước 21: Caching Services** ⭐⭐
**File:** [BUILD_21_Caching_Services.md](BUILD_21_Caching_Services.md)

**Nội dung:**
1. `ICacheService` interface
2. `LocalCacheService` (IMemoryCache)
3. `DistributedCacheService` (Redis/SQL Server)
4. `CacheSettings` configuration
5. Cache patterns (Cache-Aside, Write-Through)

**Kết quả:** Caching services hoàn chỉnh (Local + Distributed).

---

#### **Bước 22: File Storage** ⭐⭐
**File:** [BUILD_22_File_Storage.md](BUILD_22_File_Storage.md)

**Nội dung:**
1. `IFileStorageService` interface
2. `LocalFileStorageService` implementation
3. File upload/download/delete
4. File validation (size, extension)
5. `FileUploadRequest` DTO

**Kết quả:** Local file storage hoàn chỉnh.

---

#### **Bước 23: Email Service** ⭐⭐⭐
**File:** [BUILD_23_Email_Service.md](BUILD_23_Email_Service.md)

**Nội dung:**
1. `IMailService` interface
2. `SmtpMailService` implementation (MailKit)
3. `IEmailTemplateService` interface
4. `EmailTemplateService` implementation (Razor templates)
5. Email templates (WelcomeEmail.cshtml, ResetPasswordEmail.cshtml)
6. `SMTPEmailSettings` configuration

**Kết quả:** Email service hoàn chỉnh với Razor templates.

---

#### **Bước 24: Blob Storage** ⭐⭐
**File:** [BUILD_24_Blob_Storage.md](BUILD_24_Blob_Storage.md)

**Nội dung:**
1. `IBlobStorageService` interface
2. `BlobStorageService` implementation (Azure Blob Storage)
3. Container management
4. Blob upload/download/delete
5. `BlobModel`, `BlobContainerModel` DTOs

**Kết quả:** Azure Blob Storage integration.

---

#### **Bước 25: Background Jobs** ⭐⭐⭐
**File:** [BUILD_25_Background_Jobs.md](BUILD_25_Background_Jobs.md)

**Nội dung:**
1. `IJobService` interface
2. `HangfireService` implementation
3. Hangfire setup (SQL Server storage)
4. Job scheduling (Fire-and-forget, Delayed, Recurring)
5. `HangfireStorageSettings` configuration
6. Hangfire dashboard

**Kết quả:** Background jobs hoàn chỉnh (Hangfire).

---

#### **Bước 26: Logging** ⭐⭐
**File:** [BUILD_26_Logging.md](BUILD_26_Logging.md)

**Nội dung:**
1. Serilog setup (Console, File, Seq, Elasticsearch)
2. `LoggerSettings` configuration
3. Structured logging
4. Request logging middleware
5. Exception logging

**Kết quả:** Logging hoàn chỉnh (Serilog + Seq/Elasticsearch).

---

### **PHASE 7: BUSINESS MODULES**

#### **Bước 27: Export Services** ⭐⭐
**File:** [BUILD_27_Export_Services.md](BUILD_27_Export_Services.md)

**Nội dung:**
1. `IExcelService` interface
2. Excel export với ClosedXML
3. Export audit logs, products, users
4. Dynamic column mapping
5. Template-based export

**Kết quả:** Excel export service hoàn chỉnh.

---

#### **Bước 28: Catalog Module** ⭐⭐⭐
**File:** [BUILD_28_Catalog_Module.md](BUILD_28_Catalog_Module.md)

**Nội dung:**
1. Product entity và CRUD operations
2. Category entity và hierarchical structure
3. Product-Category relationships
4. Search và filtering
5. Specifications cho complex queries

**Kết quả:** Complete catalog module với products và categories.

---

#### **Bước 29: Notifications** ⭐⭐
**File:** [BUILD_29_Notifications.md](BUILD_29_Notifications.md)

**Nội dung:**
1. SignalR setup
2. Real-time notification hub
3. Notification entity và persistence
4. Push notifications to clients
5. Notification center UI integration

**Kết quả:** Real-time notifications với SignalR.

---

#### **Bước 30: Payment Integration** ⭐⭐
**File:** [BUILD_30_Payment_Integration.md](BUILD_30_Payment_Integration.md)

**Nội dung:**
1. VNPay payment gateway integration
2. Payment request và callback handling
3. Payment verification và security
4. Order payment flow
5. Payment status tracking

**Kết quả:** VNPay payment gateway hoàn chỉnh.
