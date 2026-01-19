# Infrastructure Services - Caching, FileStorage, BackgroundJobs, Email

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn về các Infrastructure Services: Caching, FileStorage, BackgroundJobs, và Email.

---

## Bước 12.1: Implement Cache Service

**Làm gì:** Tạo cache service với Local và Distributed cache.

**File:** `src/Core/Application/Common/Caching/ICacheService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Caching;

public interface ICacheService
{
    T? Get<T>(string key);
    Task<T?> GetAsync<T>(string key, CancellationToken token = default);
    void Refresh(string key);
    Task RefreshAsync(string key, CancellationToken token = default);
    void Remove(string key);
    Task RemoveAsync(string key, CancellationToken token = default);
    void Set<T>(string key, T value, TimeSpan? slidingExpiration = null);
    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
}
```

**Implementations:**
- `LocalCacheService`: Sử dụng `IMemoryCache` (in-memory)
- `DistributedCacheService`: Sử dụng `IDistributedCache` (Redis hoặc DistributedMemoryCache)

**Đăng ký:**
```csharp
// Trong Infrastructure/Startup.cs
if (settings.UseDistributedCache)
{
    if (settings.PreferRedis)
    {
        services.AddStackExchangeRedisCache(options => { ... });
    }
    else
    {
        services.AddDistributedMemoryCache();
    }
    services.AddTransient<ICacheService, DistributedCacheService>();
}
else
{
    services.AddTransient<ICacheService, LocalCacheService>();
}
```

**Tác dụng:**
- Local cache: Nhanh, nhưng chỉ trong một instance
- Distributed cache: Có thể share giữa nhiều instances (Redis)

---

## Bước 12.2: Implement FileStorage Service

**Làm gì:** Service để upload và remove files local.

**File:** `src/Core/Application/Common/FileStorage/IFileStorageService.cs`

```csharp
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Common;

namespace ECO.WebApi.Application.Common.FileStorage;

public interface IFileStorageService : ITransientService
{
    Task<string> UploadAsync<T>(FileUploadRequest? request, FileType supportedFileType, CancellationToken cancellationToken = default)
        where T : class;
    void Remove(string? path);
}
```

**Implementation:** `LocalFileStorageService`
- Upload file từ base64 string
- Lưu vào folder `Files/Images/{EntityName}` hoặc `Files/Others/{EntityName}`
- Remove file từ path

**Tác dụng:**
- Upload images/files cho entities
- Tự động tạo folder theo entity type
- Handle duplicate file names

---

## Bước 12.3: Implement Background Jobs với Hangfire

**Làm gì:** Setup Hangfire cho background jobs.

**Đăng ký:**
```csharp
// Trong Infrastructure/Startup.cs
services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

services.AddHangfireServer();

// Trong Program.cs
app.UseHangfireDashboard();
```

**Tác dụng:**
- Chạy background jobs (email, reports, etc.)
- Hangfire Dashboard để monitor jobs
- Persistent storage (SQL Server)

---

## Bước 12.4: Implement Email Service

**Làm gì:** Service để gửi email qua SMTP.

**File:** `src/Core/Application/Common/Mailing/IMailService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Mailing;

public interface IMailService : ITransientService
{
    Task SendAsync(MailRequest request, CancellationToken cancellationToken = default);
}
```

**Implementation:** `SmtpMailService`
- Sử dụng MailKit để gửi email
- Support: To, Cc, Bcc, ReplyTo, Attachments
- Config qua `SMTPEmailSettings`

**Email Template Service:**
```csharp
public interface IEmailTemplateService : ITransientService
{
    string GenerateEmailTemplate<T>(string templateName, T mailTemplateModel);
}
```

**Implementation:** `EmailTemplateService`
- Sử dụng RazorEngineCore để render templates
- Templates nằm trong folder `Email Templates/`
- Support Razor syntax

**Tác dụng:**
- Gửi email với templates
- Support attachments, multiple recipients
- Razor templates để dynamic content

---

## Tóm tắt

### Các services:

1. **Cache Service** → Local (MemoryCache) hoặc Distributed (Redis)
2. **FileStorage Service** → Upload/remove files local
3. **Background Jobs** → Hangfire cho async tasks
4. **Email Service** → SMTP với Razor templates

### Điểm quan trọng:

- **Cache strategy** → Chọn Local hoặc Distributed dựa trên requirements
- **File storage** → Có thể mở rộng để support Azure Blob Storage
- **Hangfire** → Cần SQL Server để lưu jobs
- **Email templates** → Sử dụng Razor syntax

---

**Tiếp theo:** [Application Services](BUILD_13_Application_Services.md)
