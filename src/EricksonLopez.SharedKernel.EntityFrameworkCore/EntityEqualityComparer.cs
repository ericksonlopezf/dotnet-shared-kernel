// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.SharedKernel;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore;



/// <summary>
/// Provides proxy-aware equality comparison for <see cref="Entity{TId}"/> instances in Entity Framework Core,
/// unwrapping dynamic proxies (such as Castle or EF Core change-tracking proxies) to their underlying domain entity types.
/// </summary>
/// <remarks>
/// Per ADR-015, proxy-transparent equality belongs strictly to the Infrastructure layer to preserve domain purity
/// and prevent ORM coupling in the Tier 0 SharedKernel domain primitives.
/// </remarks>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public sealed class EntityEqualityComparer<TId> : IEqualityComparer<Entity<TId>>
    where TId : notnull, IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityEqualityComparer{TId}"/> class.
    /// </summary>
    public EntityEqualityComparer()
    {
    }

    /// <inheritdoc/>
    public bool Equals(Entity<TId>? x, Entity<TId>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        var xType = EntityEqualityComparer.GetUnproxiedType(x.GetType());
        var yType = EntityEqualityComparer.GetUnproxiedType(y.GetType());

        return xType == yType && EqualityComparer<TId>.Default.Equals(x.Id, y.Id);
    }

    /// <inheritdoc/>
    public int GetHashCode(Entity<TId> obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        var unproxiedType = EntityEqualityComparer.GetUnproxiedType(obj.GetType());
        return HashCode.Combine(unproxiedType, obj.Id);
    }
}
