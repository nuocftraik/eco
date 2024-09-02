

namespace ECO.WebApi.Domain.Common.Contracts;
public interface IEntity
{
    List<DomainEvent> DomainEvents { get; }
}

public interface IEntity<TId> : IEntity
{
    TId Id { get; }
}


//IEntity định nghĩa rằng mọi (entity) trong hệ thống đều phải có một list(DomainEvents)
//Nếu phát triển một hệ thống phức tạp, ví dụ như hệ thống thương mại điện tử,
//nơi mỗi khi đơn hàng được tạo, cập nhật, hoặc hủy bỏ, bạn muốn các thành phần khác trong hệ thống(như hệ thống thông báo, hệ thống tồn kho) biết về các thay đổi này,
//DomainEvents giúp bạn quản lý các sự kiện này.


//IEntity<TId> mở rộng từ IEntity và thêm một thuộc tính Id, đại diện cho định danh duy nhất của thực thể. TId là kiểu dữ liệu tổng quát (generic type), cho phép Id có thể là bất kỳ kiểu dữ liệu nào (ví dụ: Guid, int, string, v.v.).