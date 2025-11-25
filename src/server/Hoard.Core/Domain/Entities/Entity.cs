namespace Hoard.Core.Domain.Entities;

public abstract class Entity<TId> : IEntity<TId>
    where TId : notnull
{
    public TId Id { get; set; } = default!;
}