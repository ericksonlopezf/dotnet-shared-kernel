using System;
using System.Collections.Generic;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Represents a domain entity.
/// </summary>
/// <remarks>
/// <para>
/// An entity is defined by its identity (<see cref="Id"/>), not its attributes.
/// Two entities are equal if and only if they share the same <see cref="Id"/> and
/// the same concrete type.
/// </para>
/// <para>
/// To raise domain events, use <see cref="AggregateRoot{TId}"/> instead.
/// Only Aggregate Roots can raise domain events because they represent the
/// transactional consistency boundary in DDD.
/// </para>
/// </remarks>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets the unique identifier of this entity.
    /// </summary>
    /// <remarks>
    /// The identifier is set once during object initialization (<c>init</c>-only) and cannot
    /// be changed afterwards, preserving the DDD invariant that entity identity is immutable.
    /// </remarks>
    public TId Id { get; protected init; } = default!;

    // ─── Equality ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines if this entity is transient (has not been assigned a persistent identity).
    /// </summary>
    public bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default!);

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <remarks>
    /// Two entities are considered equal if they have the same concrete type and the same
    /// non-transient identifier. Transient entities (default Id) are never equal to any other
    /// entity, even if they share the same default Id value.
    /// </remarks>
    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (IsTransient())
            return false;

        if (other.IsTransient())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    /// <summary>
    /// Calculates the hash code for this entity.
    /// </summary>
    public override int GetHashCode()
    {
        if (IsTransient())
        {
            // If the entity is transient, we use base.GetHashCode() to ensure the hash code
            // is stable and unique for this specific instance in memory.
            return base.GetHashCode();
        }

        return HashCode.Combine(GetType(), EqualityComparer<TId>.Default.GetHashCode(Id));
    }

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}

