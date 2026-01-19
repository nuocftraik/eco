# ECO.WebApi - Hướng dẫn Xây dựng Base Project

Tài liệu này hướng dẫn chi tiết từng bước xây dựng base project ECO.WebApi, giải thích **làm gì**, **tại sao**, và **cách triển khai**.

Tài liệu được chia thành nhiều file nhỏ, mỗi file tập trung vào một chức năng cụ thể để dễ đọc và tham khảo.

---

## 📚 Mục lục

### 1. [Solution và Build Configuration](BUILD_01_Solution_Setup.md)
- Tạo Solution và Project Structure
- Setup Build Configuration (Directory.Build.props, stylecop.json, .editorconfig)

### 2. [Shared Layer](BUILD_02_Shared_Layer.md)
- Setup Shared Project
- Tạo Authorization Constants (Actions, Functions, Roles, Claims, Permissions)

### 3. [Domain Layer](BUILD_03_Domain_Layer.md)
- Setup Domain Project
- Tạo Identity Entities (ApplicationUser, ApplicationRole)

### 4. [Application Layer](BUILD_04_Application_Layer.md)
- Setup Application Project
- Tạo Application Startup (MediatR, FluentValidation)

### 5. [Infrastructure Layer](BUILD_05_Infrastructure_Layer.md)
- Setup Infrastructure Project
- Tạo Infrastructure Startup (Modular Startup Pattern)

### 6. [Host Layer](BUILD_06_Host_Layer.md)
- Setup Host Project
- Tạo Program.cs (Application entry point)

### 7. [Database Initialization và Seed Data](BUILD_07_Database_Initialization.md) ⭐
- IDatabaseInitializer Interface
- DatabaseInitializer
- ApplicationDbInitializer
- ApplicationDbSeeder (Seed Actions, Functions, Roles, Admin User)
- Custom Seeders (ICustomSeeder, CustomSeederRunner)
- Ví dụ: NotificationSeeder

### 8. [Service Registration Pattern](BUILD_08_Service_Registration.md)
- Interface Markers (ITransientService, IScopedService)
- AddServices Extension Method
- Auto-register Services

### 9. [Domain Base Entities và Domain Events](BUILD_09_Domain_Base_Entities.md)
- IEntity Interface
- BaseEntity và BaseEntity<TId>
- DomainEvent Base Class
- IAggregateRoot Interface
- Entity Events (Created, Updated, Deleted)

### 10. [Repository Pattern và Specification](BUILD_10_Repository_Pattern.md)
- Repository Interfaces (IRepository, IReadRepository, IRepositoryWithEvents)
- ApplicationDbRepository Implementation
- EventAddingRepositoryDecorator (Decorator Pattern)
- Pagination và Filter Models
- Specification Builder Extensions
- Base Specifications

### 11. [Common Services](BUILD_11_Common_Services.md)
- ICurrentUser và CurrentUserMiddleware
- ISerializerService (NewtonSoftService)
- CustomException và ErrorResult
- ExceptionMiddleware
- ValidationBehavior (MediatR Pipeline)

### 12. [Infrastructure Services](BUILD_12_Infrastructure_Services.md)
- Cache Service (Local và Distributed)
- FileStorage Service
- Background Jobs (Hangfire)
- Email Service (SMTP với Razor Templates)

### 13. [Application Services](BUILD_13_Application_Services.md)
- TokenService (JWT và Refresh Token)
- UserService (CRUD và quản lý users)
- RoleService (CRUD và quản lý roles/permissions)

---

## 🎯 Thứ tự xây dựng

### Phase 1: Base Setup
1. **Solution & Build Config** → Tạo solution, setup build tools
2. **Shared Layer** → Constants, interfaces cơ bản
3. **Domain Layer** → Entities, business logic
4. **Application Layer** → DTOs, handlers, validators
5. **Infrastructure Layer** → Implementations, DbContext, services
6. **Host Layer** → Controllers, Program.cs, configurations

### Phase 2: Database & Services
7. **Database Initialization** → Migrations, seed data
8. **Service Registration** → Auto-register services
9. **Domain Base Entities** → BaseEntity, Domain Events, IAggregateRoot
10. **Repository Pattern** → Repositories, Specifications, Decorator

### Phase 3: Common & Infrastructure Services
11. **Common Services** → CurrentUser, SerializerService, ExceptionMiddleware, ValidationBehavior
12. **Infrastructure Services** → Caching, FileStorage, BackgroundJobs, Email
13. **Application Services** → UserService, RoleService, TokenService

---

## ⚠️ Điểm quan trọng

- **Thứ tự dependencies:** Shared → Domain → Application → Infrastructure → Host
- **Database initialization:** Phải sau khi Build app
- **Seed data thứ tự:** Actions/Functions → Roles → Admin User → Custom Seeders
- **Service registration:** Dùng marker interfaces để auto-register

---

## 💡 Lợi ích

- **Clean Architecture:** Tách biệt concerns, dễ test và maintain
- **Auto-registration:** Không cần đăng ký services thủ công
- **Idempotent seeding:** Có thể chạy nhiều lần an toàn
- **Modular:** Mỗi module tự quản lý services của mình

---

*Mỗi file hướng dẫn từng bước cụ thể với code examples đầy đủ, giải thích **làm gì**, **tại sao**, và **cách triển khai**.*
