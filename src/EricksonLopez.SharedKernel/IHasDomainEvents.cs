// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Defines the contract for domain objects capable of recording and releasing domain events.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets a read-only view of pending domain events recorded by this instance without clearing them.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all pending domain events recorded by this instance without returning them.
    /// </summary>
    void ClearDomainEvents() => DrainDomainEvents();

    /// <summary>
    /// Transfers and clears all pending domain events recorded by this instance.
    /// </summary>
    /// <returns>
    /// A read-only collection of pending domain events in emission order, or an empty collection if no events were recorded.
    /// </returns>
    IReadOnlyList<IDomainEvent> DrainDomainEvents();

    /// <summary>
    /// Re-queues previously drained domain events back into this instance, typically during transactional rollback.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Default implementation is a no-op for backward compatibility. Concrete aggregates should restore the events.
    /// </para>
    /// <para>
    /// <strong>⚠️ WARNING: The default implementation is a no-op.</strong>
    /// Types that implement <see cref="IHasDomainEvents"/> directly without extending
    /// <see cref="AggregateRoot{TId}"/> MUST override this method. Failing to do so will cause
    /// domain events to be silently discarded during transactional rollback, leading to data loss.
    /// </para>
    /// </remarks>
    /// <param name="events">The domain events to restore.</param>
    void RequeueDomainEvents(IEnumerable<IDomainEvent> events) { }
}

