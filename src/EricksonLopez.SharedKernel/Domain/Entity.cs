using System;
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
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// The unique identifier of this entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    // ─── Equality ───────────────────────────────────────────────────────────────

    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}

