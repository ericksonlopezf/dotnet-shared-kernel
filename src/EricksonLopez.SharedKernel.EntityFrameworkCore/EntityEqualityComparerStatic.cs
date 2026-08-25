// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.SharedKernel;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore;

/// <summary>
/// Provides factory methods and proxy unwrapping helpers for entity equality comparison in Entity Framework Core.
/// </summary>
public static class EntityEqualityComparer
{
    /// <summary>
    /// Creates a proxy-aware equality comparer for the specified strongly-typed entity identifier.
    /// </summary>
    /// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
    /// <returns>A new <see cref="EntityEqualityComparer{TId}"/> instance.</returns>
    public static EntityEqualityComparer<TId> Create<TId>()
        where TId : notnull, IEquatable<TId>
        => new();

    /// <summary>
    /// Unwraps dynamic proxy types (such as Castle DynamicProxy or EF Core lazy-loading proxies) to their base entity type.
    /// </summary>
    /// <param name="type">The runtime type of the entity or proxy instance.</param>
    /// <returns>The underlying domain entity type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static Type GetUnproxiedType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.Namespace == "Castle.Proxies" ||
            (type.BaseType is not null && type.BaseType != typeof(object) && type.Name.EndsWith("Proxy", StringComparison.Ordinal)))
        {
            return type.BaseType!;
        }

        return type;
    }
}
