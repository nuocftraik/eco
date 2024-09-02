using ECO.WebApi.Shared.Events;
using MediatR;
using System.Reflection.Metadata;

namespace ECO.WebApi.Application.Common.Events;
// This is just a shorthand to make it a bit easier to create event handlers for specific events.

public interface IEventNotificationHandler<TEvent> : INotificationHandler<EventNotification<TEvent>>
    where TEvent : IEvent
{
}

public abstract class EventNotificationHandler<TEvent> : INotificationHandler<EventNotification<TEvent>>
    where TEvent : IEvent
{
    public Task Handle(EventNotification<TEvent> notification, CancellationToken cancellationToken) =>
        Handle(notification.Event, cancellationToken);

    public abstract Task Handle(TEvent @event, CancellationToken cancellationToken);

    //EventNotificationHandler<TEvent> (abstract class) cung cấp một cơ chế chuẩn để xử lý các thông báo sự kiện. Nó triển khai giao diện INotificationHandler<EventNotification<TEvent>>,
    //có nghĩa là nó định nghĩa cách thức xử lý các thông báo sự kiện.

    //Khi bạn định nghĩa phương thức Handle là abstract, bất kỳ lớp con nào kế thừa từ EventNotificationHandler<TEvent>
    //đều bắt buộc phải cung cấp một bản triển khai cụ thể của phương thức này.


    //Giả sử bạn bỏ đi phương thức abstract Handle(TEvent @event, CancellationToken cancellationToken);
    //Kết quả:
    //Không bắt buộc triển khai:
    //Lớp con không bị buộc phải implement Handle.Điều này có nghĩa là một lớp kế thừa từ EventNotificationHandler<TEvent>
    //có thể tồn tại mà không có bất kỳ logic xử lý nào cho sự kiện.

}
