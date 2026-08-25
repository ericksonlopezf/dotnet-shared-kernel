// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Represents an abstract base class for aggregate roots that define transactional consistency boundaries in Domain-Driven Design.
/// </summary>
/// <remarks>
/// Aggregate roots encapsulate domain invariants and maintain an internal list of recorded domain events
/// that are dispatched atomically via <see cref="DrainDomainEvents"/>.
/// </remarks>
/// <typeparam name="TId">The strongly-typed identifier type of the aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull, IEquatable<TId>
{
    private List<IDomainEvent>? _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Records a domain event raised by this aggregate root.
    /// </summary>
    /// <param name="domainEvent">The domain event to record.</param>
    /// <exception cref="ArgumentNullException"><paramref name="domainEvent"/> is <see langword="null"/></exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        (_domainEvents ??= []).Add(domainEvent);
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
        if (_domainEvents is null || _domainEvents.Count == 0)
            return Array.Empty<IDomainEvent>();

        var events = _domainEvents.AsReadOnly();

        _domainEvents = null;

        return events;
    }
}
