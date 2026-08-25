// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel.Testing;

/// <summary>
/// Provides testing extension methods for asserting and collecting domain events from <see cref="AggregateRoot{TId}"/> instances.
/// </summary>
public static class AggregateRootTestExtensions
{
    /// <summary>
    /// Drains all pending domain events from the aggregate root into a newly created <see cref="DomainEventCollector"/>.
    /// </summary>
    /// <typeparam name="TId">The aggregate identifier type.</typeparam>
    /// <param name="aggregate">The aggregate root instance from which to collect events.</param>
    /// <returns>A new <see cref="DomainEventCollector"/> populated with the drained events.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="aggregate"/> is <see langword="null"/></exception>
    public static DomainEventCollector CollectEvents<TId>(this AggregateRoot<TId> aggregate)
        where TId : notnull, IEquatable<TId>
    {
        // Stryker disable once Statement: collector.CollectFrom also validates and throws ArgumentNullException for null aggregate
        ArgumentNullException.ThrowIfNull(aggregate);

        var collector = new DomainEventCollector();
        collector.CollectFrom(aggregate);
        return collector;
    }
}
