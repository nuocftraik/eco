using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Shared.Events;

namespace ECO.WebApi.Application.Common.Events;
public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event);
}

//Gửi event để  được xử lý bởi các handler tương ứng