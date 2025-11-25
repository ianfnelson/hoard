namespace Hoard.Core.Domain.Entities;

public interface IEntity<TId> : IEntity
    where TId : notnull
{
    TId Id { get; set; }
}


public interface IEntity;