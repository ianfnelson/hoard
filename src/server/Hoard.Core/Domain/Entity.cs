namespace Hoard.Core.Domain;

public abstract class Entity<TId> : IEntity<TId>
    where TId : notnull
{
    public TId Id { get; set; } = default!;
}