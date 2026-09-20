// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Represents an abstract base class for domain entities whose equality is determined by runtime type and identity.
/// </summary>
/// <remarks>
/// Entities compare equality based on exact type matching and the equality of their <see cref="Id"/> property.
/// </remarks>
/// <typeparam name="TId">The strongly-typed identifier type of the entity.</typeparam>
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is equal to the default value of <typeparamref name="TId"/></exception>
    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
        {
            throw new ArgumentException(
                "Entity identity cannot be default.",
                nameof(id));
        }

        if (id is string strId)
        {
            if (string.IsNullOrWhiteSpace(strId))
            {
                throw new ArgumentException(
                    "Entity identity cannot be empty or whitespace.",
                    nameof(id));
            }

            if (strId.Contains('\0'))
            {
                throw new ArgumentException(
                    "Entity identity cannot contain null characters.",
                    nameof(id));
            }
        }

        Id = id;
    }

    /// <summary>
    /// Gets the type used for entity equality comparison, enabling support for derived types and dynamic proxies.
    /// </summary>
    /// <returns>The <see cref="Type"/> to compare for entity equality.</returns>
    protected virtual Type GetEqualityType() => GetType();

    /// <inheritdoc />
    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetEqualityType() != other.GetEqualityType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(GetEqualityType(), Id);

    /// <summary>
    /// Determines whether two entity instances are equal based on their runtime type and identity.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><see langword="true"/> if both entities are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(
        Entity<TId>? left,
        Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines whether two entity instances are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><see langword="true"/> if the entities are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(
        Entity<TId>? left,
        Entity<TId>? right)
        => !(left == right);
}
