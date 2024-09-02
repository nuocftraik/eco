

using ECO.WebApi.Shared.Events;

namespace ECO.WebApi.Domain.Common.Contracts;
public abstract class DomainEvent : IEvent
{
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}
