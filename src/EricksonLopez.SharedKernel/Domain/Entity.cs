namespace EricksonLopez.SharedKernel.Domain;

/// <summary>
/// Base class for all domain entities.
/// </summary>
/// <remarks>
/// <para>
/// An entity is defined by its identity (<see cref="Id"/>), not its attributes.
/// Two entities are equal if and only if they share the same Id and the same concrete type.
/// </para>
/// <para>
/// To raise domain events, use <see cref="AggregateRoot{TId}"/> instead.
/// Only Aggregate Roots can raise domain events because they represent the
/// transactional consistency boundary in DDD.
/// </para>
/// </remarks>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// The unique identifier of this entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    // ─── Equality ───────────────────────────────────────────────────────────────

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        var other = (Entity<TId>)obj;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
