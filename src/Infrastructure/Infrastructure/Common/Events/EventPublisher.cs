using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ECO.WebApi.Infrastructure.Common.Events;

public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly IPublisher _mediator;

    public EventPublisher(ILogger<EventPublisher> logger, IPublisher mediator) =>
        (_logger, _mediator) = (logger, mediator);

    public Task PublishAsync(IEvent @event)
    {
        _logger.LogInformation("Publishing Event : {event}", @event.GetType().Name);
        return _mediator.Publish(CreateEventNotification(@event));
    }

    private static INotification CreateEventNotification(IEvent @event) =>
        (INotification)Activator.CreateInstance(
            typeof(EventNotification<>).MakeGenericType(@event.GetType()), @event)!;
}



//typeof(EventNotification<>) biểu thị kiểu chung(generic type) EventNotification<>.
//MakeGenericType(@event.GetType()) biến kiểu chung đó thành một kiểu cụ thể bằng cách truyền vào loại của sự kiện thực tế(ví dụ: nếu @event là một sự kiện của loại OrderPlacedEvent, thì kết quả của MakeGenericType sẽ là EventNotification<OrderPlacedEvent>).
//Truyền @event làm tham số:

//Activator.CreateInstance sau đó được sử dụng để tạo một instance của EventNotification<OrderPlacedEvent> với tham số @event được truyền vào constructor. Kết quả của việc này là một EventNotification<TEvent> chứa sự kiện thực tế.
//Cast về INotification:

//Kết quả của Activator.CreateInstance được cast về INotification, bởi vì EventNotification<TEvent> thực tế triển khai INotification.