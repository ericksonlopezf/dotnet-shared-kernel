// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore;

using System.Linq;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Represents an Entity Framework Core <see cref="SaveChangesInterceptor"/> that collects, drains, and dispatches domain events
/// from tracked <see cref="IHasDomainEvents"/> aggregate roots and entities before saving changes.
/// </summary>
public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher? _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventsInterceptor"/> class with the optional domain event dispatcher.
    /// </summary>
    /// <param name="dispatcher">The optional domain event dispatcher used to dispatch drained events.</param>
    public DomainEventsInterceptor(IDomainEventDispatcher? dispatcher = null)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Intercepts synchronous <see cref="DbContext.SaveChanges()"/> calls to drain and dispatch pending domain events from tracked entities.
    /// </summary>
    /// <param name="eventData">The contextual information for the SaveChanges operation.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The interception result value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="eventData"/> is <see langword="null"/></exception>
    /// <remarks>
    /// <para>
    /// <b>WARNING — Deadlock Risk:</b> When a dispatcher is registered, this synchronous
    /// path blocks the current thread on <c>.GetAwaiter().GetResult()</c> over an
    /// asynchronous dispatch operation (<see cref="EricksonLopez.SharedKernel.IDomainEventDispatcher.DispatchAsync"/>).
    /// </para>
    /// <para>
    /// In environments with an active <see cref="System.Threading.SynchronizationContext"/>
    /// (legacy ASP.NET, Windows Forms, WPF), the blocked thread holds the context lock
    /// while the async continuation attempts to resume on the same context — causing
    /// a classic deadlock.
    /// </para>
    /// <para>
    /// <b>Recommendation:</b> Always prefer <see cref="SavingChangesAsync"/> in async
    /// EF Core pipelines (<see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>).
    /// Only rely on this synchronous path in environments confirmed to have no
    /// <see cref="System.Threading.SynchronizationContext"/> (e.g., .NET thread-pool
    /// threads, console applications, Kestrel-based ASP.NET Core with default async pipeline).
    /// </para>
    /// <para>
    /// See ADR-031 for the formal policy decision on the sync dispatcher path.
    /// </para>
    /// </remarks>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            var events = CollectAndDrainEvents(eventData.Context);
            if (events.Count > 0 && _dispatcher is not null)
            {
                _dispatcher.DispatchAsync(events).AsTask().GetAwaiter().GetResult();
            }
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts asynchronous <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> calls to drain and dispatch pending domain events from tracked entities.
    /// </summary>
    /// <param name="eventData">The contextual information for the SaveChanges operation.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the interception result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="eventData"/> is <see langword="null"/></exception>
    /// <remarks>
    /// This is the <b>preferred interception path</b>. Dispatch is fully awaited — no
    /// <see cref="System.Threading.SynchronizationContext"/> deadlock risk.
    /// Use <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>
    /// in all async-capable EF Core pipelines.
    /// </remarks>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            var events = CollectAndDrainEvents(eventData.Context);
            if (events.Count > 0 && _dispatcher is not null)
            {
                await _dispatcher.DispatchAsync(events, cancellationToken);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Collects and drains all pending domain events from tracked entities within the specified database context.
    /// </summary>
    /// <param name="context">The database context containing tracked entities.</param>
    /// <returns>A read-only collection of all drained domain events in emission order, or an empty collection if no events were recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/></exception>
    public static IReadOnlyList<IDomainEvent> CollectAndDrainEvents(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var entities = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasDomainEvents)
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .ToList();

        var allEvents = new List<IDomainEvent>();
        foreach (var entity in entities)
        {
            allEvents.AddRange(entity.DrainDomainEvents());
        }

        return allEvents.Count == 0 ? Array.Empty<IDomainEvent>() : allEvents;
    }
}



