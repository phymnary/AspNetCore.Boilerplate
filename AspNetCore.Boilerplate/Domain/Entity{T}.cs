using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Boilerplate.Domain;

public interface IEntity
{
    object Key();
}

public abstract class Entity<TKey> : IEntity
    where TKey : IComparable, IComparable<TKey>, IEquatable<TKey>
{
    protected Entity() { }

    protected Entity(TKey id)
    {
        Id = id;
    }

    [Key]
    public TKey Id { get; protected init; } = default!;

    public object Key()
    {
        return Id;
    }
}
