# ECO.WebApi - Hướng dẫn Xây dựng Solution từ đầu

> 📘 **Mục đích:** Tài liệu này hướng dẫn **từng bước chi tiết** để xây dựng một Clean Architecture solution từ đầu.  
> Mỗi bước giải thích **làm gì**, **tại sao**, **thứ tự thực hiện**, và **code cụ thể**.

---

## 📋 Tổng quan

ECO.WebApi được xây dựng theo **Clean Architecture** với 5 layers:

```
┌─────────────────────────────────────────────────────────┐
│   Host Layer    │
│  ASP.NET Core API, Controllers, Program.cs              │
└────────────────────┬──────────────────────────────────┘
            ↓ depends on
┌────────────────────┴──────────────────────────────────┐
│              Infrastructure Layer       │
│  EF Core, Identity, Caching, Mailing, External Services│
└────────────────────┬──────────────────────────────────┘
           ↓ depends on
┌────────────────────┴──────────────────────────────────┐
│           Application Layer            │
│  Use Cases, DTOs, Interfaces, Validators    │
└────────────────────┬──────────────────────────────────┘
      ↓ depends on
┌────────────────────┴──────────────────────────────────┐
│      Domain Layer     │
│  Entities, Value Objects, Domain Events, Enums│
└────────────────────┬──────────────────────────────────┘
          ↓ depends on
┌────────────────────┴──────────────────────────────────┐
│      Shared Layer             │
│  Common Contracts, Authorization Constants             │
└────────────────────────────────────────────────────────┘
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

### **PHASE 2: Core Domain & Patterns** (Core Logic)
Xây dựng domain entities, base patterns, và repository.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 7 | [BUILD_09](BUILD_09_Domain_Base_Entities.md) | Base entities, Domain Events | Phase 1 hoàn thành |
| 8 | [BUILD_10](BUILD_10_Repository_Pattern.md) | Repository pattern, Specifications | Bước 7 |

**Kết quả Phase 2:** Domain layer hoàn chỉnh với base patterns.

---

### **PHASE 3: Database & Initialization** (Database Setup)
Setup database, migrations, và seed data.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 9 | [BUILD_07](BUILD_07_Database_Initialization.md) | Database initialization, seeding | Bước 8 |

**Kết quả Phase 3:** Database tự động migrate và seed khi chạy application.

---

### **PHASE 4: Service Layer** (Business Services)
Xây dựng common services, infrastructure services, và application services.

| Bước | Tài liệu | Nội dung | Prerequisites |
|------|----------|----------|---------------|
| 10 | [BUILD_08](BUILD_08_Service_Registration.md) | Auto service registration | Bước 9 |
| 11 | [BUILD_11](BUILD_11_Common_Services.md) | CurrentUser, Serializer, Exceptions, Validation | Bước 10 |
| 12 | [BUILD_12](BUILD_12_Infrastructure_Services.md) | Caching, FileStorage, BackgroundJobs, Email | Bước 11 |
| 13 | [BUILD_13](BUILD_13_Application_Services.md) | Token, User, Role services | Bước 12 |

**Kết quả Phase 4:** Hệ thống hoàn chỉnh với authentication, authorization, và business services.

---

## 📚 Chi tiết các bước

### **PHASE 1: Foundation Setup**

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
6. Tạo các domain entities khác (Product, Category, Order...)

**Kết quả:** Domain entities hoàn chỉnh, không phụ thuộc infrastructure.

---

#### **Bước 4: Application Layer** ⭐
**File:** [BUILD_04_Application_Layer.md](BUILD_04_Application_Layer.md)

**Nội dung:**
1. Setup `Application.csproj` (phụ thuộc Domain + Shared)
2. Add packages: `MediatR`, `FluentValidation`, `Mapster`, `Ardalis.Specification`
3. Tạo `Startup.cs` (register MediatR, FluentValidation)
4. Tạo DTOs (UserDto, RoleDto, ProductDto...)
5. Tạo Interfaces (IUserService, IRoleService...)
6. Tạo Specifications (ProductByIdSpec, UserByEmailSpec...)

**Kết quả:** Application layer với use cases, DTOs, và contracts.

---

#### **Bước 5: Infrastructure Layer** ⭐
**File:** [BUILD_05_Infrastructure_Layer.md](BUILD_05_Infrastructure_Layer.md)

**Nội dung:**
1. Setup `Infrastructure.csproj` (phụ thuộc Application + Domain)
2. Add packages: `EF Core`, `Hangfire`, `Serilog`, `MailKit`...
3. Tạo `ApplicationDbContext` (kế thừa `BaseDbContext`)
4. Tạo modular `Startup.cs` pattern
5. Setup các modules: Auth, Persistence, Caching, Mailing...

**Kết quả:** Infrastructure layer với tất cả implementations.

---

#### **Bước 6: Host Layer** ⭐
**File:** [BUILD_06_Host_Layer.md](BUILD_06_Host_Layer.md)

**Nội dung:**
1. Setup `Host.csproj` (phụ thuộc Infrastructure + Application)
2. Add packages: `Swashbuckle`, `FluentValidation.AspNetCore`
3. Tạo `Program.cs` (configure middleware pipeline)
4. Tạo `BaseApiController`
5. Tạo Controllers (UserController, RoleController...)
6. Setup Swagger documentation

**Kết quả:** API hoàn chỉnh, có thể chạy và test qua Swagger.

---

### **PHASE 2: Core Domain & Patterns**

#### **Bước 7: Domain Base Entities và Events** ⭐
**File:** [BUILD_09_Domain_Base_Entities.md](BUILD_09_Domain_Base_Entities.md)

**Nội dung:**
1. Tạo `IEntity` interface
2. Tạo `BaseEntity` và `BaseEntity<TId>`
3. Tạo `DomainEvent` base class
4. Tạo `IAggregateRoot` interface
5. Tạo domain events: `EntityCreatedEvent`, `EntityUpdatedEvent`, `EntityDeletedEvent`
6. Tạo `ISoftDelete`, `IAuditableEntity` interfaces
7. Tạo `AuditableEntity` base class

**Kết quả:** Base entities với domain events support.

---

#### **Bước 8: Repository Pattern và Specifications** ⭐
**File:** [BUILD_10_Repository_Pattern.md](BUILD_10_Repository_Pattern.md)

**Nội dung:**
1. Tạo `IRepository<T>`, `IReadRepository<T>` interfaces
2. Tạo `ApplicationDbRepository<T>` implementation
3. Tạo `EventAddingRepositoryDecorator<T>` (decorator pattern)
4. Tạo `PaginationFilter`, `PaginationResponse` models
5. Tạo `SpecificationBuilderExtensions`
6. Tạo base specifications: `EntitiesByPaginationFilterSpec`, `EntitiesByBaseFilterSpec`

**Kết quả:** Repository pattern hoàn chỉnh với specification support.

---

### **PHASE 3: Database & Initialization**

#### **Bước 9: Database Initialization và Seed Data** ⭐⭐⭐
**File:** [BUILD_07_Database_Initialization.md](BUILD_07_Database_Initialization.md)

**Nội dung quan trọng - thứ tự thực hiện:**

**9.1. Tạo Interfaces:**
```csharp
// Thứ tự 1: Interface cơ bản
IDatabaseInitializer
ICustomSeeder
```

**9.2. Tạo Implementations (theo thứ tự dependency):**
```csharp
// Thứ tự 2: Base initializer
DatabaseInitializer (implement IDatabaseInitializer)

// Thứ tự 3: Application initializer
ApplicationDbInitializer (kế thừa DatabaseInitializer)

// Thứ tự 4: Seeder chính
ApplicationDbSeeder
├── Seed Actions (Create, Update, Delete...)
  ├── Seed Functions (User, Role, Product...)
  ├── Seed Roles (Admin, Basic)
  └── Seed Admin User

// Thứ tự 5: Custom seeder runner
CustomSeederRunner (chạy tất cả ICustomSeeder)

// Thứ tự 6: Custom seeders (optional)
NotificationSeeder (implement ICustomSeeder)
```

**9.3. Register và Run:**
```csharp
// Trong Program.cs
services.AddScoped<IDatabaseInitializer, ApplicationDbInitializer>();
services.AddScoped<ApplicationDbSeeder>();
services.AddScoped<CustomSeederRunner>();

// Run sau khi build app
await app.Services.CreateScope().ServiceProvider
    .GetRequiredService<IDatabaseInitializer>()
    .InitializeDatabasesAsync(cancellationToken);
```

**Kết quả:** Database tự động migrate và seed Actions → Functions → Roles → Admin User.

---

### **PHASE 4: Service Layer**

#### **Bước 10: Service Registration Pattern** ⭐
**File:** [BUILD_08_Service_Registration.md](BUILD_08_Service_Registration.md)

**Nội dung:**
1. Tạo marker interfaces: `ITransientService`, `IScopedService`, `ISingletonService`
2. Tạo `AddServices()` extension method
3. Auto-register services bằng reflection
4. Apply cho Application và Infrastructure layers

**Kết quả:** Services tự động register, không cần thủ công.

---

#### **Bước 11: Common Services** ⭐
**File:** [BUILD_11_Common_Services.md](BUILD_11_Common_Services.md)

**Nội dung:**
1. **CurrentUser:** `ICurrentUser` interface + `CurrentUser` + `CurrentUserMiddleware`
2. **Serializer:** `ISerializerService` + `NewtonSoftService`
3. **Exceptions:** `CustomException` hierarchy + `ErrorResult` + `ExceptionMiddleware`
4. **Validation:** `ValidationBehavior` (MediatR pipeline)

**Kết quả:** Common services sẵn sàng cho toàn application.

---

#### **Bước 12: Infrastructure Services** ⭐
**File:** [BUILD_12_Infrastructure_Services.md](BUILD_12_Infrastructure_Services.md)

**Nội dung:**
1. **Caching:** `LocalCacheService` + `DistributedCacheService`
2. **FileStorage:** `LocalFileStorageService`
3. **BlobStorage:** `BlobStorageService` (Azure/AWS)
4. **GoogleDrive:** `GoogleDriveService`
5. **BackgroundJobs:** `HangfireService`
6. **Email:** `SmtpMailService` + `EmailTemplateService` (Razor templates)
7. **Logging:** Serilog configuration

**Kết quả:** Infrastructure services đầy đủ cho production.

---

#### **Bước 13: Application Services** ⭐
**File:** [BUILD_13_Application_Services.md](BUILD_13_Application_Services.md)

**Nội dung:**
1. **TokenService:** JWT token generation + refresh token
2. **UserService:** CRUD users, assign roles, change password
3. **RoleService:** CRUD roles, manage permissions
4. **FunctionService:** CRUD functions
5. **AuthenticationService:** Login, OAuth2 (Google, Facebook)

**Kết quả:** Authentication & Authorization hoàn chỉnh.

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
- `StackExchangeRedis` - Distributed caching
- `Azure.Storage.Blobs` (v12.21.2) - Blob storage
- `Google.Apis.Drive.v3` (v1.68.0.3574) - Google Drive integration

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
dotnet ef migrations add InitialCreate --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj

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

---

## 📖 Tài liệu tham khảo

- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ardalis Specification Pattern](https://github.com/ardalis/Specification)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Mapster Documentation](https://github.com/MapsterMapper/Mapster)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Hangfire Documentation](https://docs.hangfire.io/)

---

## 📝 Notes

- **Version:** .NET 8.0
- **Architecture:** Clean Architecture / Onion Architecture
- **Patterns:** Repository, Specification, CQRS, Decorator, Modular Startup
- **Database:** SQL Server (có thể switch sang PostgreSQL/MySQL)
- **Last Updated:** 2024

---

**🎯 Mục tiêu cuối cùng:** Sau khi hoàn thành tất cả bước, bạn sẽ có một production-ready API với:
- ✅ Clean Architecture
- ✅ Authentication & Authorization
- ✅ Background Jobs
- ✅ Caching
- ✅ Email Service
- ✅ File Storage
- ✅ Logging
- ✅ API Documentation (Swagger)
- ✅ Database Migrations & Seeding

---

*Bắt đầu với [Bước 1: Solution Setup](BUILD_01_Solution_Setup.md)*
