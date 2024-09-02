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

//Sử dụng khi tất cả các thực thể trong hệ thống đều sử dụng Guid làm Id.
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() => Id = NewId.Next().ToGuid();
}


//Không giống như interface, abstract class cho phép triển khai sẵn
//một số logic cơ bản,vd như việc quản lý list DomainEvents
//hoặc tự động tạo Id. Nghĩa là các lớp con có thể kế thừa và sử dụng trực tiếp các logic này mà không cần phải tự triển khai lại.
//Đồng thời ngăn cản việc khởi tạo trực tiếp các lớp cơ sở không đầy đủ thông tin hoặc logic để tồn tại độc lập.