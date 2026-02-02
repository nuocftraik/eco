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

### **PHASE 5: Infrastructure Services** (Dịch vụ hạ tầng)
Xây dựng các services hỗ trợ (caching, storage, jobs, email).

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 19 | [BUILD_19](BUILD_19_Caching_Services.md) | Local cache, Distributed cache (Redis) | Phase 4 |
| 20 | [BUILD_20](BUILD_20_File_Storage.md) | Local file storage, File upload/download | Bước 19 |
| 21 | [BUILD_21](BUILD_21_Email_Service.md) | SMTP email, Email templates (Razor) | Bước 20 |
| 22 | [BUILD_22](BUILD_22_Blob_Storage.md) | Azure Blob Storage, AWS S3 | Bước 21 |
| 23 | [BUILD_23](BUILD_23_Background_Jobs.md) | Hangfire background jobs | Bước 22 |
| 24 | [BUILD_24](BUILD_24_Logging.md) | Serilog, Seq, Elasticsearch | Bước 23 |

**Kết quả Phase 5:** Infrastructure services đầy đủ (Caching, Storage, Email, Jobs, Logging).

---

### **PHASE 6: Advanced Features** (Tính năng nâng cao)
Xây dựng các modules nghiệp vụ và features nâng cao.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 22 | [BUILD_22](BUILD_22_Auditing.md) | Audit trails, Change tracking | Phase 5 |
| 23 | [BUILD_23](BUILD_23_Export_Services.md) | Excel export, Report generation | Bước 22 |
| 24 | [BUILD_24](BUILD_24_Catalog_Module.md) | Products, Categories CRUD | Bước 23 |
| 25 | [BUILD_25](BUILD_25_Notifications.md) | SignalR notifications, Real-time updates | Bước 24 |
| 26 | [BUILD_26](BUILD_26_Blob_Storage.md) | Azure Blob Storage, AWS S3 | Bước 25 |
| 27 | [BUILD_27](BUILD_27_Background_Jobs.md) | Hangfire background jobs | Bước 26 |
| 28 | [BUILD_28](BUILD_28_Payment_Integration.md) | VNPay payment gateway | Bước 27 |

**Kết quả Phase 6:** Advanced features complete (Auditing, Export, Catalog, Notifications, Blob Storage, Background Jobs, Payment).

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
1. Tạo `IEvent` interface, `DomainEvent` base class
2. Tạo `IEntity`, `BaseEntity`, `AuditableEntity`
3. Tạo `IAggregateRoot` marker interface
4. Tạo domain events: `EntityCreatedEvent`, `EntityUpdatedEvent`, `EntityDeletedEvent`
5. Tạo `ISoftDelete`, `IAuditableEntity` interfaces

**Kết quả:** Base entities với domain events support.

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

### **PHASE 5: INFRASTRUCTURE SERVICES**

#### **Bước 19: Caching Services** ⭐⭐
**File:** [BUILD_19_Caching_Services.md](BUILD_19_Caching_Services.md)

**Nội dung:**
1. `ICacheService` interface
2. `LocalCacheService` (IMemoryCache)
3. `DistributedCacheService` (Redis/SQL Server)
4. `CacheSettings` configuration
5. Cache patterns (Cache-Aside, Write-Through)

**Kết quả:** Caching services hoàn chỉnh (Local + Distributed).

---

#### **Bước 20: File Storage** ⭐⭐
**File:** [BUILD_20_File_Storage.md](BUILD_20_File_Storage.md)

**Nội dung:**
1. `IFileStorageService` interface
2. `LocalFileStorageService` implementation
3. File upload/download/delete
4. File validation (size, extension)
5. `FileUploadRequest` DTO

**Kết quả:** Local file storage hoàn chỉnh.

---

#### **Bước 21: Email Service** ⭐⭐⭐
**File:** [BUILD_21_Email_Service.md](BUILD_21_Email_Service.md)

**Nội dung:**
1. `IMailService` interface
2. `SmtpMailService` implementation (MailKit)
3. `IEmailTemplateService` interface
4. `EmailTemplateService` implementation (Razor templates)
5. Email templates (WelcomeEmail.cshtml, ResetPasswordEmail.cshtml)
6. `SMTPEmailSettings` configuration

**Kết quả:** Email service hoàn chỉnh với Razor templates.

---

#### **Bước 22: Blob Storage** ⭐⭐
**File:** [BUILD_22_Blob_Storage.md](BUILD_22_Blob_Storage.md)

**Nội dung:**
1. `IBlobStorageService` interface
2. `BlobStorageService` implementation (Azure Blob Storage)
3. Container management
4. Blob upload/download/delete
5. `BlobModel`, `BlobContainerModel` DTOs

**Kết quả:** Azure Blob Storage integration.

---

#### **Bước 23: Background Jobs** ⭐⭐⭐
**File:** [BUILD_23_Background_Jobs.md](BUILD_23_BACKGROUND_JOBS.md)

**Nội dung:**
1. `IJobService` interface
2. `HangfireService` implementation
3. Hangfire setup (SQL Server storage)
4. Job scheduling (Fire-and-forget, Delayed, Recurring)
5. `HangfireStorageSettings` configuration
6. Hangfire dashboard

**Kết quả:** Background jobs hoàn chỉnh (Hangfire).

---

#### **Bước 24: Logging** ⭐⭐
**File:** [BUILD_24_Logging.md](BUILD_24_Logging.md)

**Nội dung:**
1. Serilog setup (Console, File, Seq, Elasticsearch)
2. `LoggerSettings` configuration
3. Structured logging
4. Request logging middleware
5. Exception logging

**Kết quả:** Logging hoàn chỉnh (Serilog + Seq/Elasticsearch).

---

### **PHASE 6: ADVANCED FEATURES**

#### **Bước 22: Auditing** ⭐⭐
**File:** [BUILD_22_Auditing.md](BUILD_22_Auditing.md)

**Nội dung:**
1. `IAuditService` interface
2. `AuditService` implementation
3. `Trail` entity (audit log)
4. `AuditTrail` helper class
5. Audit interceptor (track changes)
6. `GetMyAuditLogsRequest` query

**Kết quả:** Audit trails hoàn chỉnh (track all entity changes).

---

#### **Bước 23: Export Services** ⭐⭐
**File:** [BUILD_23_Export_Services.md](BUILD_23_Export_Services.md)

**Nội dung:**
1. `IExcelWriter` interface
2. `ExcelWriter` implementation (ClosedXML)
3. Export templates
4. Dynamic column mapping
5. Export products to Excel example

**Kết quả:** Excel export hoàn chỉnh.

---

#### **Bước 24: Catalog Module** ⭐⭐⭐
**File:** [BUILD_24_Catalog_Module.md](BUILD_24_Catalog_Module.md)

**Nội dung:**
1. **Products:** CRUD operations, variants, attributes
2. **Categories:** CRUD operations, product categories
3. Product DTOs (ProductDto, ProductInListDto, VariantDto, AttributeDto)
4. Category DTOs (CategoryDto, CategoryInListDto)
5. Specifications (ProductBySearchSpec, CategoryBySearchSpec)
6. Requests (CreateProductRequest, UpdateProductRequest, SearchProductRequest)
7. Event handlers (ProductCreatedEventHandler)

**Kết quả:** Catalog module hoàn chỉnh (Products, Categories).

---

#### **Bước 25: Notifications** ⭐⭐⭐
**File:** [BUILD_25_Notifications.md](BUILD_25_Notifications.md)

**Nội dung:**
1. `INotificationService` interface
2. `NotificationService` implementation
3. `INotificationSender` interface
4. `NotificationSender` implementation (SignalR)
5. `NotificationHub` (SignalR hub)
6. Notification DTOs (NotificationDto, SendNotificationRequest)
7. `SignalRSettings` configuration

**Kết quả:** Real-time notifications hoàn chỉnh (SignalR).

---

#### **Bước 26: Blob Storage** ⭐⭐
**File:** [BUILD_26_Blob_Storage.md](BUILD_26_Blob_Storage.md)

**Nội dung:**
1. `IBlobStorageService` interface
2. `BlobStorageService` implementation (Azure Blob Storage)
3. Container management
4. Blob upload/download/delete
5. `BlobModel`, `BlobContainerModel` DTOs

**Kết quả:** Azure Blob Storage integration.

---

#### **Bước 27: Background Jobs** ⭐⭐⭐
**File:** [BUILD_27_Background_Jobs.md](BUILD_27_Background_Jobs.md)

**Nội dung:**
1. `IJobService` interface
2. `HangfireService` implementation
3. Hangfire setup (SQL Server storage)
4. Job scheduling (Fire-and-forget, Delayed, Recurring)
5. `HangfireStorageSettings` configuration
6. Hangfire dashboard

**Kết quả:** Background jobs hoàn chỉnh (Hangfire).

---

#### **Bước 28: Payment Integration** ⭐⭐⭐
**File:** [BUILD_28_Payment_Integration.md](BUILD_28_Payment_Integration.md)

**Nội dung:**
1. `IPaymentService` interface
2. `IVnPay` interface
3. `PaymentService` implementation (VNPay)
4. Payment models (PaymentRequest, PaymentResponse, PaymentResult, TransactionStatus)
5. VNPay helpers (Encoder, PaymentHelper, NetworkHelper)
6. Payment callback handling

**Kết quả:** VNPay payment integration hoàn chỉnh.

---

## 🔧 Công cụ và Packages chính

### **Domain Layer**
- `Microsoft.AspNetCore.Identity` (v2.1.39)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (v8.0.0)
- `NewId` (v4.0.1) - Distributed ID generation

### **Application Layer**
- `MediatR` (v12.4.0) - CQRS pattern
- `FluentValidation.DependencyInjectionExtensions` (v11.9.2)
- `Mapster` (v7.4.0) - Object mapping
- `Ardalis.Specification` (v8.0.0) - Specification pattern

### **Infrastructure Layer**
- `Microsoft.EntityFrameworkCore.SqlServer` (v8.0.0)
- `Hangfire` (v1.7.34) - Background jobs
- `Serilog` suite - Structured logging
- `MailKit` (v3.6.0) - Email sending
- `StackExchange.Redis` - Distributed caching
- `Azure.Storage.Blobs` (v12.21.2) - Blob storage
- `Microsoft.AspNetCore.SignalR` (v8.0.0) - Real-time notifications

### **Host Layer**
- `Swashbuckle.AspNetCore` (v6.4.0) - Swagger/OpenAPI
- `FluentValidation.AspNetCore` (v11.3.0)

---

## ⚠️ Điểm quan trọng cần nhớ

### **1. Thứ tự tạo Projects (QUAN TRỌNG!)**
```bash
# Phải tạo theo thứ tự này vì dependencies
1. Shared (không phụ thuộc gì)
2. Domain (phụ thuộc Shared)
3. Application (phụ thuộc Domain + Shared)
4. Infrastructure (phụ thuộc Application + Domain)
5. Host (phụ thuộc Infrastructure + Application)
6. Migrators.MSSQL (phụ thuộc Infrastructure + Domain)
```

### **2. Thứ tự Seed Data (QUAN TRỌNG!)**
```
1. Actions (Create, Update, Delete, View...)
2. Functions (User, Role, Product...)
3. ActionInFunctions (mapping table)
4. Roles (Admin, Basic)
5. Admin User
6. RoleClaims (permissions for roles)
7. Custom Seeders (NotificationSeeder...)
```

### **3. Database Migration Commands**
```bash
# Phải chạy từ thư mục Host
cd src/Host/Host/

# Tạo migration
dotnet ef migrations add MigrationName --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj

# Apply migration
dotnet ef database update --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj
```

### **4. Configuration Files Location**
Tất cả JSON configs nằm trong `src/Host/Host/Configurations/`:
- `database.json` - Database connection strings
- `cache.json` - Redis configuration
- `mail.json` - SMTP settings
- `hangfire.json` - Background jobs settings
- `security.json` - JWT settings
- `signalr.json` - SignalR settings

### **5. Sử dụng Mapster thay vì AutoMapper**
```csharp
// ✅ Đúng - Mapster
var dto = entity.Adapt<EntityDto>();

// ❌ Sai - Không dùng AutoMapper
var dto = _mapper.Map<EntityDto>(entity);
```

---

## 🎓 Best Practices

### **1. Domain Layer**
- ✅ Entities có methods, không chỉ properties
- ✅ Sử dụng Domain Events cho side effects
- ✅ Value Objects cho complex concepts
- ❌ Không reference Infrastructure

### **2. Application Layer**
- ✅ DTOs chỉ chứa data
- ✅ Handlers delegate cho domain services
- ✅ Validation trong FluentValidation validators
- ❌ Không có business logic trong DTOs

### **3. Infrastructure Layer**
- ✅ Implementations đơn giản
- ✅ Repository pattern cho data access
- ✅ Caching ở infrastructure layer
- ❌ Không chứa business logic

### **4. Host Layer**
- ✅ Controllers thin, chỉ route requests
- ✅ Configuration files tách biệt
- ✅ Middleware pipeline rõ ràng
- ❌ Không có business logic trong controllers

---

## 🚀 Quick Start Checklist

Sau khi hoàn thành tất cả bước, check list này để verify:

- [ ] Solution build thành công
- [ ] Tất cả tests pass (nếu có)
- [ ] Database migrate và seed thành công
- [ ] API chạy được trên `https://localhost:7001`
- [ ] Swagger UI accessible tại `/swagger`
- [ ] Có thể login với admin user
- [ ] JWT token được generate đúng
- [ ] Permissions được check đúng
- [ ] Background jobs chạy được (Hangfire dashboard)
- [ ] Email gửi thành công (test với MailHog/Papercut)
- [ ] Cache hoạt động (Redis/In-Memory)
- [ ] File upload/download hoạt động
- [ ] Logging ghi ra Seq/Elasticsearch/File
- [ ] Real-time notifications hoạt động (SignalR)
- [ ] Payment integration hoạt động (VNPay sandbox)

---

## 📞 Troubleshooting

### **Problem 1: Migration fails**
```bash
# Solution: Ensure correct project paths
cd src/Host/Host/
dotnet ef migrations add MigrationName --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj
```

### **Problem 2: Seed data duplicates**
```csharp
// Solution: Check idempotency in seeder
if (await _db.Actions.AnyAsync()) return;
```

### **Problem 3: JWT token invalid**
```json
// Solution: Check security.json
{
  "SecuritySettings": {
    "Key": "your-super-secret-key-minimum-32-characters",
    "Issuer": "ECO.WebApi",
    "Audience": "ECO.WebApi"
  }
}
```

### **Problem 4: Redis connection failed**
```json
// Solution: Check cache.json và ensure Redis is running
{
  "CacheSettings": {
    "UseDistributedCache": true,
    "PreferRedis": true,
    "RedisURL": "localhost:6379"
  }
}
```

### **Problem 5: Hangfire dashboard not accessible**
```csharp
// Solution: Check Hangfire settings and authentication
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireCustomBasicAuthenticationFilter() }
});
```

---

## 📖 Tài liệu tham khảo

- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ardalis Specification Pattern](https://github.com/ardalis/Specification)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Mapster Documentation](https://github.com/MapsterMapper/Mapster)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [Serilog Documentation](https://serilog.net/)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)

---

## 🤖 **IX. AI-Assisted Development Infrastructure**

### **Agent Memories Location**

ECO.WebApi hỗ trợ AI-assisted development với tập hợp tài liệu kiến thức có cấu trúc:

```
.agent/
├── memories/
│   ├── 01_architecture.md          # Clean Architecture patterns
│   ├── 02_coding_standards.md      # Coding conventions
│   ├── 03_database_patterns.md     # Database design patterns
│   └── 04_api_patterns.md          # RESTful API conventions
├── skills/                          # Custom agent skills (future)
│   ├── eco_crud_generator/         # Generate CRUD operations
│   ├── eco_migration_helper/       # EF Core migrations
│   └── eco_test_generator/         # Unit test scaffolding
├── rules.md                         # Development rules
└── target_instructions.md           # Agent objectives
```

### **Memory Files Purpose**

| File | Purpose | Content |
|------|---------|---------|
| `01_architecture.md` | Core architecture | Clean Architecture layers, dependencies, design patterns |
| `02_coding_standards.md` | Coding conventions | Naming, structure, code quality rules |
| `03_database_patterns.md` | Database patterns | EF Core, migrations, seeding, queries |
| `04_api_patterns.md` | API design | RESTful conventions, routing, responses |
| `rules.md` | Development rules | Code generation, security, workflow |
| `target_instructions.md` | Agent capabilities | Objectives, tasks, response patterns |

### **Quick AI Assistant Commands**

Khi làm việc với AI agent, bạn có thể sử dụng các lệnh:

```bash
# Generate CRUD
"/generate-crud Product"
→ Tạo đầy đủ CRUD cho entity Product

# Add Feature
"/add-feature OrderManagement"
→ Plan và generate feature mới

# Review Code
"/review-code ProductService.cs"
→ Kiểm tra code theo ECO standards

# Create Migration
"/create-migration AddProductTable"
→ Generate migration command

# Explain Concept
"/explain Repository Pattern"
→ Giải thích pattern trong context ECO

# Refactor code
"/refactor ProductController.cs"
→ Đề xuất cải tiến

# Generate Tests
"/test ProductService"
→ Tạo unit tests

# Generate Documentation
"/docs Authentication"
→ Tạo tài liệu
```

### **Agent Capabilities**

AI assistant có thể giúp bạn:

1. **✅ Code Generation**: Tạo code theo ECO patterns
   - Entities với business logic
   - DTOs và validators
   - Handlers (Commands/Queries)
   - Controllers với routing chuẩn
   - EF Core configurations
   - Unit và integration tests

2. **✅ Code Review**: Kiểm tra code theo standards
   - Clean Architecture compliance
   - StyleCop + SonarAnalyzer rules
   - Naming conventions
   - Anti-patterns
   - Security vulnerabilities

3. **✅ Documentation**: Tự động generate docs
   - XML documentation
   - README files
   - API documentation
   - Architecture diagrams

4. **✅ Refactoring**: Cải thiện code
   - Extract interfaces
   - Apply design patterns
   - Performance optimization
   - Reduce duplication

### **How to Use Agent Effectively**

**1. Bắt đầu với context:**
```
Tôi đang làm việc với ECO.WebApi. 
Tôi muốn thêm feature quản lý sản phẩm.
```

**2. Agent sẽ tham khảo memories:**
- `.agent/memories/01_architecture.md` cho layer structure
- `.agent/memories/02_coding_standards.md` cho naming
- `.agent/memories/03_database_patterns.md` cho database
- `.agent/memories/04_api_patterns.md` cho API design
- `.agent/rules.md` cho development rules

**3. Agent sẽ generate code:**
- Full code, không có placeholders
- Tuân thủ ECO conventions
- Include XML documentation
- Ready to compile

**4. Bạn review và apply:**
- Copy code vào project
- Run migration nếu có
- Test endpoints
- Commit changes

### **Agent Learning Resources**

Agent luôn tham khảo:
- ✅ `.agent/memories/` - Core knowledge base
- ✅ `.agent/rules.md` - Development rules
- ✅ `.agent/target_instructions.md` - Agent objectives
- ✅ `docs/BUILD_INDEX.md` - Full documentation roadmap
- ✅ `docs/MODULE_DOCUMENTATION_TEMPLATE.md` - Doc template
- ✅ `docs/BUILD_XX_*.md` - Specific module docs

### **Benefits of AI-Assisted Development**

1. **🚀 Faster Development**
   - Generate boilerplate code instantly
   - No manual repetitive work
   - Focus on business logic

2. **✅ Consistent Quality**
   - Always follow patterns
   - No naming inconsistencies
   - Proper error handling

3. **📚 Learning Tool**
   - Explains WHY, not just WHAT
   - Teaches best practices
   - References documentation

4. **🔍 Code Review Assistant**
   - Catch issues early
   - Enforce standards
   - Suggest improvements

---

## 📝 Notes

- **Version:** .NET 8.0
- **Architecture:** Clean Architecture / Onion Architecture
- **Patterns:** Repository, Specification, CQRS, Decorator, Modular Startup, Event-Driven
- **Database:** SQL Server (có thể switch sang PostgreSQL/MySQL)
- **Last Updated:** 2024

---

## 🎯 Module Status

| Module | Status | Build Doc | Notes |
|--------|--------|-----------|-------|
| Foundation Setup | ✅ Complete | BUILD_01 - BUILD_06 | Solution structure |
| Database & Patterns | ✅ Complete | BUILD_07 - BUILD_11 | Database, Repository |
| Core Services | 🚧 In Progress | BUILD_12 - BUILD_14 | CurrentUser, Exceptions, Validation |
| Authentication & Authorization | 📝 Planned | BUILD_15 - BUILD_18 | JWT, Identity, Permissions, OAuth2 |
| Infrastructure Services | 📝 Planned | BUILD_19 - BUILD_21 | Caching, Storage, Email, Logging |
| Advanced Features | 📝 Planned | BUILD_22 - BUILD_28 | Auditing, Export, Catalog, Notifications, Blob, Jobs, Payment |

**Legend:**
- ✅ Complete - Đã hoàn thành và có docs
- 🚧 In Progress - Đang viết docs
- 📝 Planned - Chưa bắt đầu

---

**🎯 Mục tiêu cuối cùng:** Sau khi hoàn thành tất cả bước, bạn sẽ có một production-ready API với:
- ✅ Clean Architecture
- ✅ Authentication & Authorization (JWT + OAuth2)
- ✅ Caching (Local + Redis)
- ✅ File Storage (Local file system)
- ✅ Email Service (SMTP + Templates)
- ✅ Logging (Serilog + Seq/Elasticsearch)
- ✅ Audit Trails (Track entity changes)
- ✅ Excel Export (ClosedXML)
- ✅ Catalog Module (Products, Categories CRUD)
- ✅ Real-time Notifications (SignalR)
- ✅ Blob Storage (Azure Blob Storage)
- ✅ Background Jobs (Hangfire)
- ✅ Payment Integration (VNPay)
- ✅ API Documentation (Swagger)
- ✅ Database Migrations & Seeding

---

*Bắt đầu với [Bước 1: Solution Setup](BUILD_01_Solution_Setup.md)*
