// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Represents an abstract base class for aggregate roots that define transactional consistency boundaries in Domain-Driven Design.
/// </summary>
/// <remarks>
/// Aggregate roots encapsulate domain invariants and maintain an internal list of recorded domain events
/// that are dispatched atomically via <see cref="DrainDomainEvents"/>.
/// <para>
/// <b>Note on Thread-Safety:</b> This class is explicitly <b>not thread-safe</b>. Domain aggregates define a transactional consistency boundary
/// and are designed to be mutated by a single logical request/thread at a time. Concurrent calls to <see cref="RaiseDomainEvent"/> will corrupt internal state.
/// </para>
/// </remarks>
/// <typeparam name="TId">The strongly-typed identifier type of the aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull, IEquatable<TId>
{

    private List<IDomainEvent>? _domainEvents;
    private ReadOnlyDomainEventList? _cachedSnapshot;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Records a domain event raised by this aggregate root in a thread-safe manner.
    /// </summary>
    /// <param name="domainEvent">The domain event to record.</param>
    /// <exception cref="ArgumentNullException"><paramref name="domainEvent"/> is <see langword="null"/></exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        (_domainEvents ??= []).Add(domainEvent);
        _cachedSnapshot = null;
    }

    /// <summary>
    /// Gets a read-only view of pending domain events recorded by this aggregate root without clearing them.
    /// Returns an immutable snapshot list to prevent concurrent modification exceptions during enumeration.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents
    {
        get
        {
            if (_domainEvents is null || _domainEvents.Count == 0)
            {
                return ReadOnlyDomainEventList.Empty;
            }

            return _cachedSnapshot ??= new ReadOnlyDomainEventList(_domainEvents.ToArray());
        }
    }

    /// <summary>
    /// Gets the count of pending domain events recorded by this aggregate root without allocating a snapshot list.
    /// </summary>
    public int PendingDomainEventsCount
    {
        get
        {
            return _domainEvents?.Count ?? 0;
        }
    }

    /// <summary>
    /// Clears all pending domain events recorded by this aggregate root.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents = null;
        _cachedSnapshot = null;
    }

    /// <summary>
    /// Transfers and clears all pending domain events recorded by this aggregate root.
    /// </summary>
    /// <remarks>
    /// Detaches the internal event collection and resets the buffer, ensuring domain events are dispatched only once.
    /// </remarks>
    /// <returns>
    /// A read-only collection of pending domain events in emission order, or an empty collection if no events were recorded.
    /// </returns>
    public IReadOnlyList<IDomainEvent> DrainDomainEvents()
    {
        // Atomically detach the internal event buffer to guarantee single-consumer detachment under concurrency.
        var local = Interlocked.Exchange(ref _domainEvents, null);
        if (local is null || local.Count == 0)
        {
            return ReadOnlyDomainEventList.Empty;
        }

        var snapshot = Interlocked.Exchange(ref _cachedSnapshot, null);
        return snapshot ?? new ReadOnlyDomainEventList(local.ToArray());
    }

    /// <summary>
    /// Re-queues previously drained domain events back into this aggregate root, typically used during transactional rollback.
    /// </summary>
    /// <param name="events">The domain events to restore.</param>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="events"/> contains one or more <see langword="null"/> elements.</exception>
    public void RequeueDomainEvents(IEnumerable<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var incomingList = events switch
        {
            IReadOnlyList<IDomainEvent> roList => roList,
            _ => new List<IDomainEvent>(events)
        };

        var incomingSet = new HashSet<EricksonLopez.Events.Identifiers.EventId>();

        // REM-001: Build existing event ID set ONCE (O(m)) before the incoming loop to avoid
        // an O(n*m) inner scan. Total complexity is now O(n + m) instead of O(n*m).
        var existingIds = _domainEvents is { Count: > 0 }
            ? new HashSet<EricksonLopez.Events.Identifiers.EventId>(_domainEvents.Select(e => e.Id))
            : null;

        for (int i = 0; i < incomingList.Count; i++)
        {
            var item = incomingList[i];
            if (item is null)
            {
                throw new ArgumentException("The domain event collection cannot contain null elements.", nameof(events));
            }

            if (!incomingSet.Add(item.Id))
            {
                throw new ArgumentException($"The domain event collection contains duplicate events (Id: {item.Id}).", nameof(events));
            }

            if (existingIds is not null && existingIds.Contains(item.Id))
            {
                throw new ArgumentException($"The domain event with Id {item.Id} already exists in the aggregate.", nameof(events));
            }
        }

        if (incomingList.Count == 0)
        {
            return;
        }

        if (_domainEvents is null || _domainEvents.Count == 0)
        {
            _domainEvents = new List<IDomainEvent>(incomingList);
        }
        else
        {
            var combined = new List<IDomainEvent>(incomingList.Count + _domainEvents.Count);
            for (int i = 0; i < incomingList.Count; i++)
            {
                combined.Add(incomingList[i]);
            }
            combined.AddRange(_domainEvents);
            _domainEvents = combined;
        }

        _cachedSnapshot = null;
    }

    private sealed class ReadOnlyDomainEventList : IReadOnlyList<IDomainEvent>
    {
        internal static readonly ReadOnlyDomainEventList Empty = new([]);
        private readonly IDomainEvent[] _items;

        public ReadOnlyDomainEventList(IDomainEvent[] items)
        {
            _items = items;
        }

        public IDomainEvent this[int index] => _items[index];

        public int Count => _items.Length;

        public IEnumerator<IDomainEvent> GetEnumerator() => ((IEnumerable<IDomainEvent>)_items).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}
