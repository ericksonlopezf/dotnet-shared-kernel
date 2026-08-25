// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Defines the contract for dispatching domain events collected from domain entities.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a collection of domain events in emission order asynchronously.
    /// </summary>
    /// <param name="domainEvents">The read-only collection of domain events to dispatch.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a collection of domain events in emission order synchronously.
    /// </summary>
    /// <remarks>
    /// Default implementation throws <see cref="NotSupportedException"/> to prevent sync-over-async threadpool starvation and deadlocks. Implementations requiring synchronous dispatch must provide a dedicated synchronous execution path.
    /// </remarks>
    /// <param name="domainEvents">The read-only collection of domain events to dispatch.</param>
    void Dispatch(IReadOnlyList<IDomainEvent> domainEvents)
    {
        throw new NotSupportedException(
            "Synchronous dispatch is not supported by default to prevent sync-over-async threadpool starvation and deadlocks. " +
            "Implement Dispatch explicitly or use DispatchAsync.");
    }
}

