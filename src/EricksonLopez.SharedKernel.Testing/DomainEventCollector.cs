// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel.Testing;

/// <summary>
/// Represents an in-memory test spy for capturing, querying, and asserting on domain events emitted by aggregate roots.
/// </summary>
public sealed class DomainEventCollector
{
    private readonly List<IDomainEvent> _collectedEvents = [];

    /// <summary>
    /// Gets all domain events recorded by this collector in emission order.
    /// </summary>
    public IReadOnlyList<IDomainEvent> CollectedEvents => _collectedEvents.AsReadOnly();

    /// <summary>
    /// Drains and records all pending domain events from the specified aggregate root.
    /// </summary>
    /// <typeparam name="TId">The aggregate identifier type.</typeparam>
    /// <param name="aggregate">The aggregate root from which to collect events.</param>
    /// <returns>The current <see cref="DomainEventCollector"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="aggregate"/> is <see langword="null"/></exception>
    public DomainEventCollector CollectFrom<TId>(AggregateRoot<TId> aggregate)
        where TId : notnull, IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var events = aggregate.DrainDomainEvents();
        _collectedEvents.AddRange(events);
        return this;
    }

    /// <summary>
    /// Enumerates all collected domain events matching the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type to filter.</typeparam>
    /// <returns>An enumerable collection of matching domain events.</returns>
    public IEnumerable<TEvent> OfType<TEvent>()
        where TEvent : IDomainEvent
    {
        return _collectedEvents.OfType<TEvent>();
    }

    /// <summary>
    /// Asserts that at least one domain event matching the specified type and optional predicate was collected.
    /// </summary>
    /// <typeparam name="TEvent">The expected domain event type.</typeparam>
    /// <param name="predicate">An optional predicate to filter matching domain events.</param>
    /// <returns>The first matching domain event instance.</returns>
    /// <exception cref="InvalidOperationException">No matching domain event was recorded</exception>
    public TEvent ExpectEvent<TEvent>(Func<TEvent, bool>? predicate = null)
        where TEvent : IDomainEvent
    {
        var match = predicate is null
            ? _collectedEvents.OfType<TEvent>().FirstOrDefault()
            : _collectedEvents.OfType<TEvent>().FirstOrDefault(predicate);

        if (match is null)
        {
            throw new InvalidOperationException(
                $"Expected domain event of type '{typeof(TEvent).Name}', but none was recorded matching the criteria.");
        }

        return match;
    }

    /// <summary>
    /// Removes all previously collected domain events from this collector.
    /// </summary>
    public void Reset()
    {
        _collectedEvents.Clear();
    }
}
