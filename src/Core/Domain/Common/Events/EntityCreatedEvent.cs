

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

