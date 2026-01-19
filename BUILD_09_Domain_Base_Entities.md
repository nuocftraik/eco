# Domain Base Entities và Domain Events

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn về Domain Base Entities và Domain Events - nền tảng cho tất cả entities trong hệ thống.

---

## Bước 9.1: Tạo IEntity Interface

**Làm gì:** Tạo interface cơ bản cho tất cả entities.

**Tại sao:**
- Đảm bảo mọi entity đều có DomainEvents
- Cho phép xử lý domain events thống nhất
- Hỗ trợ DDD (Domain-Driven Design)

**File:** `src/Core/Domain/Common/Contracts/IEntity.cs`

```csharp
namespace ECO.WebApi.Domain.Common.Contracts;

public interface IEntity
{
    List<DomainEvent> DomainEvents { get; }
}

public interface IEntity<TId> : IEntity
{
    TId Id { get; }
}
```

**Giải thích:**
- `IEntity`: Interface cơ bản, yêu cầu mọi entity phải có `DomainEvents`
- `IEntity<TId>`: Mở rộng với `Id` generic, cho phép dùng Guid, int, string, etc.

**Tác dụng:**
- Mọi entity đều có thể phát domain events
- Hỗ trợ event-driven architecture
- Dễ dàng track changes trong domain

---

## Bước 9.2: Tạo DomainEvent Base Class

**Làm gì:** Tạo base class cho tất cả domain events.

**File:** `src/Core/Domain/Common/Contracts/DomainEvent.cs`

```csharp
using ECO.WebApi.Shared.Events;

namespace ECO.WebApi.Domain.Common.Contracts;

public abstract class DomainEvent : IEvent
{
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}
```

**Giải thích:**
- `DomainEvent` kế thừa từ `IEvent` (trong Shared layer)
- `TriggeredOn`: Timestamp khi event được tạo
- Abstract class: Không thể khởi tạo trực tiếp

**Tác dụng:**
- Tất cả domain events đều có timestamp
- Có thể track khi nào event xảy ra
- Hỗ trợ event sourcing nếu cần

---

## Bước 9.3: Tạo BaseEntity

**Làm gì:** Tạo abstract base class cho tất cả entities.

**File:** `src/Core/Domain/Common/Contracts/BaseEntity.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;

namespace ECO.WebApi.Domain.Common.Contracts;

public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; protected set; } = default!;

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new();
}

// Sử dụng khi tất cả các thực thể trong hệ thống đều sử dụng Guid làm Id.
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() => Id = NewId.Next().ToGuid();
}
```

**Giải thích:**
- `BaseEntity<TId>`: Generic base class, cho phép dùng bất kỳ kiểu Id nào
- `BaseEntity`: Non-generic version, tự động dùng Guid và tạo Id bằng `NewId.Next().ToGuid()`
- `[NotMapped]`: DomainEvents không được lưu vào database (chỉ dùng trong memory)
- `protected set`: Chỉ cho phép set Id từ bên trong class hoặc derived classes

**Tại sao dùng `NewId` thay vì `Guid.NewGuid()`:**
- `NewId` từ MassTransit tạo sequential GUIDs (tốt hơn cho database indexing)
- Giảm fragmentation trong database
- Performance tốt hơn khi insert nhiều records

**Tác dụng:**
- Tự động tạo Id cho entities
- Quản lý DomainEvents tập trung
- Không cần implement lại logic cơ bản

**Lưu ý:**
- `DomainEvents` là `[NotMapped]` - không lưu vào database
- Events chỉ tồn tại trong memory, được publish trước khi SaveChanges

---

## Bước 9.4: Tạo IAggregateRoot Interface

**Làm gì:** Tạo marker interface cho Aggregate Roots.

**File:** `src/Core/Domain/Common/Contracts/IAggregateRoot.cs`

```csharp
namespace ECO.WebApi.Domain.Common.Contracts;

// Apply this marker interface only to aggregate root entities
// Repositories will only work with aggregate roots, not their children
public interface IAggregateRoot : IEntity
{
}
```

**Giải thích:**
- Marker interface: Không có methods, chỉ dùng để đánh dấu
- Aggregate Root: Entity chính trong một aggregate (DDD pattern)
- Repositories chỉ làm việc với Aggregate Roots

**Tại sao cần Aggregate Root:**
- Trong DDD, chỉ Aggregate Root được truy cập từ bên ngoài
- Child entities chỉ được truy cập thông qua Aggregate Root
- Đảm bảo consistency và encapsulation

**Ví dụ:**
```csharp
// Aggregate Root
public class Order : BaseEntity, IAggregateRoot
{
    public List<OrderItem> Items { get; set; } // Child entities
}

// Child entity (không implement IAggregateRoot)
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
}
```

**Tác dụng:**
- Repositories chỉ accept `IAggregateRoot`
- Đảm bảo chỉ truy cập entities qua Aggregate Root
- Tuân thủ DDD principles

---

## Bước 9.5: Tạo Entity Events

**Làm gì:** Tạo các events cho Entity Created, Updated, Deleted.

**File 1:** `src/Core/Domain/Common/Events/EntityCreatedEvent.cs`

```csharp
using ECO.WebApi.Domain.Common.Contracts;

namespace ECO.WebApi.Domain.Common.Events;

public static class EntityCreatedEvent
{
    public static EntityCreatedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

public class EntityCreatedEvent<TEntity> : DomainEvent
{
    public TEntity Entity { get; }
    internal EntityCreatedEvent(TEntity entity) => Entity = entity;
}
```

**File 2:** `src/Core/Domain/Common/Events/EntityUpdatedEvent.cs`

```csharp
using ECO.WebApi.Domain.Common.Contracts;

namespace ECO.WebApi.Domain.Common.Events;

public static class EntityUpdatedEvent
{
    public static EntityUpdatedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

public class EntityUpdatedEvent<TEntity> : DomainEvent
    where TEntity : IEntity
{
    internal EntityUpdatedEvent(TEntity entity) => Entity = entity;
    public TEntity Entity { get; }
}
```

**File 3:** `src/Core/Domain/Common/Events/EntityDeletedEvent.cs`

```csharp
using ECO.WebApi.Domain.Common.Contracts;

namespace ECO.WebApi.Domain.Common.Events;

public static class EntityDeletedEvent
{
    public static EntityDeletedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

public class EntityDeletedEvent<TEntity> : DomainEvent
    where TEntity : IEntity
{
    internal EntityDeletedEvent(TEntity entity) => Entity = entity;
    public TEntity Entity { get; }
}
```

**Giải thích:**
- Static factory methods (`WithEntity`): Tạo event instance dễ dàng
- `internal` constructor: Chỉ cho phép tạo event từ bên trong assembly
- Generic `<TEntity>`: Type-safe, biết chính xác entity type

**Tại sao dùng static factory methods:**
- Dễ sử dụng: `EntityCreatedEvent.WithEntity(product)`
- Type inference: Compiler tự động infer type
- Consistent API

**Tác dụng:**
- Tự động phát events khi entity thay đổi
- Hỗ trợ event-driven architecture
- Có thể handle events để gửi notifications, update caches, etc.

**Cách sử dụng:**
```csharp
// Trong Repository (sẽ được thêm tự động bởi decorator)
var product = new Product { Name = "Test" };
product.DomainEvents.Add(EntityCreatedEvent.WithEntity(product));
```

---

## Tóm tắt

### Thứ tự thực hiện:

1. **IEntity Interface** → Interface cơ bản với DomainEvents
2. **DomainEvent Base Class** → Base class cho tất cả domain events
3. **BaseEntity** → Abstract base class cho entities
4. **IAggregateRoot** → Marker interface cho Aggregate Roots
5. **Entity Events** → Created, Updated, Deleted events

### Điểm quan trọng:

- **DomainEvents không lưu vào database** → Chỉ dùng trong memory
- **Aggregate Root pattern** → Chỉ Aggregate Roots được truy cập từ bên ngoài
- **NewId cho Guid** → Sequential GUIDs tốt hơn cho database
- **Events được publish** → Trước khi SaveChanges

### Lợi ích:

- **Event-driven architecture** → Dễ mở rộng và maintain
- **DDD compliance** → Tuân thủ Domain-Driven Design principles
- **Type safety** → Generic types đảm bảo type safety
- **Automatic Id generation** → Không cần tự tạo Id

---

**Tiếp theo:** [Repository Pattern](BUILD_10_Repository_Pattern.md)
