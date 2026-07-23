namespace EricksonLopez.SharedKernel.Domain;

/// <summary>
/// Base class for Aggregate Roots — the consistency boundary in DDD.
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
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Read-only view of domain events raised by this aggregate since the last dispatch.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event. The event will be dispatched by the infrastructure layer
    /// (e.g., after SaveChanges in the Unit of Work).
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="domainEvent"/> is null.</exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null) throw new ArgumentNullException(nameof(domainEvent));
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Should be called by the infrastructure layer
    /// after dispatching events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
