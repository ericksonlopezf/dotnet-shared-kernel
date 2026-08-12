using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Represents an aggregate root — the consistency boundary in DDD.
/// </summary>
/// <remarks>
/// <para>
/// An Aggregate Root is an <see cref="Entity{TId}"/> that serves as the entry point
/// and consistency boundary for a cluster of related entities and value objects.
/// </para>
/// <para>
/// Only Aggregate Roots can raise <see cref="IDomainEvent"/>s, because they are the
/// transactional boundary — changes to an aggregate are persisted atomically, and
/// events are dispatched after successful persistence.
/// </para>
/// <para>
/// Domain events are stored in a lazily-allocated <see cref="List{T}"/>. No memory is
/// allocated for the collection until the first event is raised, which means that
/// read-only aggregate hydration (e.g., fetching from the database) produces
/// <b>zero bytes</b> of heap allocation for the events collection.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is intentionally <em>not</em> thread-safe.
/// An Aggregate Root is a single-threaded consistency boundary. The application layer
/// (command handlers) must ensure exclusive access before mutating an aggregate.
/// </para>
/// <para>
/// <b>Usage:</b>
/// <code>
/// public sealed class Order : AggregateRoot&lt;Guid&gt;
/// {
///     public static Order Create(Guid id, string description)
///     {
///         var order = new Order { Id = id };
///         order.RaiseDomainEvent(new OrderCreated(id));
///         return order;
///     }
/// }
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull, IEquatable<TId>
{
    private List<IDomainEvent>? _domainEvents;

    /// <summary>
    /// Gets the read-only collection of domain events that occurred within this aggregate.
    /// </summary>
    /// <remarks>
    /// Returns an empty collection when no events have been raised, with zero allocation.
    /// When events exist, returns a read-only wrapper over the internal list without copying.
    /// </remarks>
    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents?.AsReadOnly() ?? (IReadOnlyCollection<IDomainEvent>)Array.Empty<IDomainEvent>();

    /// <summary>
    /// Raises a domain event. The event will be dispatched by the infrastructure layer
    /// (e.g., after SaveChanges in the Unit of Work).
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="domainEvent"/> is null.</exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        (_domainEvents ??= new List<IDomainEvent>()).Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Should be called by the infrastructure layer
    /// after dispatching events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents?.Clear();
}

