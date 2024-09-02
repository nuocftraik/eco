using ECO.WebApi.Shared.Events;
using MediatR;

namespace ECO.WebApi.Application.Common.Events;
public class EventNotification<TEvent> : INotification
    where TEvent : IEvent
{
    public EventNotification(TEvent @event) => Event = @event;

    public TEvent Event { get; }
}

//Khi bạn tạo ra một EventNotification<TEvent>, @event sẽ được truyền vào constructor và được lưu trữ trong thuộc tính Event.