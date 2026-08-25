// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Provides equality comparison for domain events based strictly on their unique identifier (<see cref="DomainEvent.Id"/>).
/// </summary>
public sealed class DomainEventIdentityEqualityComparer : IEqualityComparer<IDomainEvent>, IEqualityComparer<DomainEvent>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="DomainEventIdentityEqualityComparer"/>.
    /// </summary>
    public static DomainEventIdentityEqualityComparer Instance { get; } = new();

    /// <inheritdoc/>
    public bool Equals(IDomainEvent? x, IDomainEvent? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Id.Equals(y.Id);
    }

    /// <inheritdoc/>
    public int GetHashCode(IDomainEvent obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj.Id.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(DomainEvent? x, DomainEvent? y) => Equals((IDomainEvent?)x, (IDomainEvent?)y);

    /// <inheritdoc/>
    public int GetHashCode(DomainEvent obj) => GetHashCode((IDomainEvent)obj);
}
