// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore;

/// <summary>
/// Represents an Entity Framework Core <see cref="SaveChangesInterceptor"/> that collects and dispatches domain events
/// from tracked <see cref="IHasDomainEvents"/> aggregate roots and entities before saving changes, clearing event buffers
/// only after the database transaction successfully commits.
/// </summary>
public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher? _dispatcher;
    private readonly DomainEventDispatchTiming _timing;
    private readonly Action<Exception, IReadOnlyList<IDomainEvent>>? _onDispatchError;
    private readonly ConditionalWeakTable<DbContext, List<IHasDomainEvents>> _pendingEntities = new();
    private readonly ConditionalWeakTable<DbContext, List<(IHasDomainEvents Entity, IReadOnlyList<IDomainEvent> Events)>> _beforeCommitDrainedEvents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventsInterceptor"/> class without a dispatcher.
    /// </summary>
    public DomainEventsInterceptor()
        : this(null, DomainEventDispatchTiming.AfterCommit, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventsInterceptor"/> class with the optional domain event dispatcher.
    /// </summary>
    /// <param name="dispatcher">The optional domain event dispatcher used to dispatch drained events.</param>
    public DomainEventsInterceptor(IDomainEventDispatcher? dispatcher)
        : this(dispatcher, DomainEventDispatchTiming.AfterCommit, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventsInterceptor"/> class with the domain event dispatcher,
    /// dispatch timing, and error handling callback.
    /// </summary>
    /// <param name="dispatcher">The optional domain event dispatcher used to dispatch drained events.</param>
    /// <param name="timing">The dispatch timing relative to the database commit.</param>
    /// <param name="onDispatchError">Optional callback invoked when dispatching fails in post-commit mode.</param>
    public DomainEventsInterceptor(
        IDomainEventDispatcher? dispatcher,
        DomainEventDispatchTiming timing,
        Action<Exception, IReadOnlyList<IDomainEvent>>? onDispatchError = null)
    {
        _dispatcher = dispatcher;
        _timing = timing;
        _onDispatchError = onDispatchError;
    }

    /// <summary>
    /// Intercepts synchronous <see cref="DbContext.SaveChanges()"/> calls to collect pending domain events from tracked entities.
    /// In <see cref="DomainEventDispatchTiming.BeforeCommit"/> mode, events are dispatched immediately before saving.
    /// In <see cref="DomainEventDispatchTiming.AfterCommit"/> mode, dispatching is deferred to <see cref="SavedChanges"/>.
    /// </summary>
    /// <param name="eventData">The contextual information for the SaveChanges operation.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The interception result value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="eventData"/> is <see langword="null"/></exception>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            var entities = GetEntitiesWithDomainEvents(eventData.Context);
            // Stryker disable once Equality : Allocation optimization bypassing draining logic when no tracked entities implement IHasDomainEvents
            if (entities.Count > 0)
            {
                if (_timing == DomainEventDispatchTiming.BeforeCommit)
                {
                    var drainedRecords = new List<(IHasDomainEvents Entity, IReadOnlyList<IDomainEvent> Events)>();
                    var allEvents = new List<IDomainEvent>();
                    foreach (var entity in entities)
                    {
                        var events = entity.DrainDomainEvents();
                        // Stryker disable once Equality : Optimization avoiding tracking entities with zero domain events
                        if (events.Count > 0)
                        {
                            drainedRecords.Add((entity, events));
                            allEvents.AddRange(events);
                        }
                    }

                    // Stryker disable once Equality : Optimization avoiding tracking contexts with no drained events
                    if (drainedRecords.Count > 0)
                    {
                        lock (_beforeCommitDrainedEvents)
                        {
                            _beforeCommitDrainedEvents.AddOrUpdate(eventData.Context, drainedRecords);
                        }
                    }

                    if (allEvents.Count > 0 && _dispatcher is not null)
                    {
                        try
                        {
                            _dispatcher.Dispatch(allEvents);
                        }
                        catch
                        {
                            RestoreDrainedEvents(drainedRecords);
                            // Stryker disable once all : Cleanup of temporary tracking state on failure
                            lock (_beforeCommitDrainedEvents)
                            {
                                _beforeCommitDrainedEvents.Remove(eventData.Context);
                            }
                            throw;
                        }
                    }
                }
                else
                {
                    lock (_pendingEntities)
                    {
                        _pendingEntities.AddOrUpdate(eventData.Context, entities);
                    }
                }
            }
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts asynchronous <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> calls to collect pending domain events from tracked entities.
    /// In <see cref="DomainEventDispatchTiming.BeforeCommit"/> mode, events are dispatched immediately before saving.
    /// In <see cref="DomainEventDispatchTiming.AfterCommit"/> mode, dispatching is deferred to <see cref="SavedChangesAsync"/>.
    /// </summary>
    /// <param name="eventData">The contextual information for the SaveChanges operation.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the interception result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="eventData"/> is <see langword="null"/></exception>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            var entities = GetEntitiesWithDomainEvents(eventData.Context);
            // Stryker disable once Equality : Allocation optimization bypassing draining logic when no tracked entities implement IHasDomainEvents
            if (entities.Count > 0)
            {
                if (_timing == DomainEventDispatchTiming.BeforeCommit)
                {
                    var drainedRecords = new List<(IHasDomainEvents Entity, IReadOnlyList<IDomainEvent> Events)>();
                    var allEvents = new List<IDomainEvent>();
                    foreach (var entity in entities)
                    {
                        var events = entity.DrainDomainEvents();
                        // Stryker disable once Equality : Optimization avoiding tracking entities with zero domain events
                        if (events.Count > 0)
                        {
                            drainedRecords.Add((entity, events));
                            allEvents.AddRange(events);
                        }
                    }

                    // Stryker disable once Equality : Optimization avoiding tracking contexts with no drained events
                    if (drainedRecords.Count > 0)
                    {
                        lock (_beforeCommitDrainedEvents)
                        {
                            _beforeCommitDrainedEvents.AddOrUpdate(eventData.Context, drainedRecords);
                        }
                    }

                    if (allEvents.Count > 0 && _dispatcher is not null)
                    {
                        try
                        {
                            // Stryker disable once Boolean : Library best practice ConfigureAwait(false) produces equivalent test outcome
                            await _dispatcher.DispatchAsync(allEvents, cancellationToken).ConfigureAwait(false);
                        }
                        catch
                        {
                            RestoreDrainedEvents(drainedRecords);
                            // Stryker disable once all : Cleanup of temporary tracking state on failure
                            lock (_beforeCommitDrainedEvents)
                            {
                                _beforeCommitDrainedEvents.Remove(eventData.Context);
                            }
                            throw;
                        }
                    }
                }
                else
                {
                    lock (_pendingEntities)
                    {
                        _pendingEntities.AddOrUpdate(eventData.Context, entities);
                    }
                }
            }
        }

        // Stryker disable once Boolean : Library best practice ConfigureAwait(false) produces equivalent test outcome
        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        // Stryker disable once all : Memory cleanup of drained events after commit completion
        if (eventData.Context is not null)
        {
            lock (_beforeCommitDrainedEvents)
            {
                _beforeCommitDrainedEvents.Remove(eventData.Context);
            }
        }

        // Stryker disable once all : In BeforeCommit mode, _pendingEntities is not populated, so TryGetValue is false and no dispatch occurs
        if (eventData.Context is not null && _timing == DomainEventDispatchTiming.AfterCommit)
        {
            List<IHasDomainEvents>? entities = null;
            lock (_pendingEntities)
            {
                if (_pendingEntities.TryGetValue(eventData.Context, out var list))
                {
                    entities = list;
                    // Stryker disable once all : Memory cleanup of pending entities after retrieval
                    _pendingEntities.Remove(eventData.Context);
                }
            }

            // Stryker disable once Equality : Optimization avoiding event extraction when entity list is empty
            if (entities is not null && entities.Count > 0)
            {
                var events = ExtractEvents(entities);
                if (events.Count > 0 && _dispatcher is not null)
                {
                    try
                    {
                        _dispatcher.Dispatch(events);
                    }
                    catch (Exception ex)
                    {
                        if (_onDispatchError is not null)
                        {
                            _onDispatchError(ex, events);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        return base.SavedChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        // Stryker disable once all : Memory cleanup of drained events after commit completion
        if (eventData.Context is not null)
        {
            lock (_beforeCommitDrainedEvents)
            {
                _beforeCommitDrainedEvents.Remove(eventData.Context);
            }
        }

        // Stryker disable once all : In BeforeCommit mode, _pendingEntities is not populated, so TryGetValue is false and no dispatch occurs
        if (eventData.Context is not null && _timing == DomainEventDispatchTiming.AfterCommit)
        {
            List<IHasDomainEvents>? entities = null;
            lock (_pendingEntities)
            {
                if (_pendingEntities.TryGetValue(eventData.Context, out var list))
                {
                    entities = list;
                    // Stryker disable once all : Memory cleanup of pending entities after retrieval
                    _pendingEntities.Remove(eventData.Context);
                }
            }

            // Stryker disable once Equality : Optimization avoiding event extraction when entity list is empty
            if (entities is not null && entities.Count > 0)
            {
                var events = ExtractEvents(entities);
                if (events.Count > 0 && _dispatcher is not null)
                {
                    try
                    {
                        // Stryker disable once Boolean : Library best practice ConfigureAwait(false) produces equivalent test outcome
                        await _dispatcher.DispatchAsync(events, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (_onDispatchError is not null)
                        {
                            _onDispatchError(ex, events);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        // Stryker disable once Boolean : Library best practice ConfigureAwait(false) produces equivalent test outcome
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            lock (_beforeCommitDrainedEvents)
            {
                if (_beforeCommitDrainedEvents.TryGetValue(eventData.Context, out var records))
                {
                    RestoreDrainedEvents(records);
                    // Stryker disable once all : Memory cleanup of restored tracking records
                    _beforeCommitDrainedEvents.Remove(eventData.Context);
                }
            }

            lock (_pendingEntities)
            {
                // Stryker disable once all : Memory cleanup of pending entities on save failure
                _pendingEntities.Remove(eventData.Context);
            }
        }

        // Stryker disable once all : EF Core base SaveChangesFailed method is a virtual no-op
        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc/>
    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            lock (_beforeCommitDrainedEvents)
            {
                if (_beforeCommitDrainedEvents.TryGetValue(eventData.Context, out var records))
                {
                    RestoreDrainedEvents(records);
                    // Stryker disable once all : Memory cleanup of restored tracking records
                    _beforeCommitDrainedEvents.Remove(eventData.Context);
                }
            }

            lock (_pendingEntities)
            {
                // Stryker disable once all : Memory cleanup of pending entities on save failure
                _pendingEntities.Remove(eventData.Context);
            }
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    /// <summary>
    /// Collects all pending domain events from tracked entities within the specified database context without clearing them.
    /// </summary>
    /// <param name="context">The database context containing tracked entities.</param>
    /// <returns>A read-only collection of all pending domain events in emission order, or an empty collection if no events were recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/></exception>
    public static IReadOnlyList<IDomainEvent> CollectEvents(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var entities = GetEntitiesWithDomainEvents(context);
        // Stryker disable once all : Fast zero-allocation return when context has no entities with domain events
        if (entities.Count == 0)
        {
            return Array.Empty<IDomainEvent>();
        }

        var allEvents = new List<IDomainEvent>();
        foreach (var entity in entities)
        {
            var events = entity.DomainEvents;
            // Stryker disable once Equality : Optimization skipping empty event snapshots
            if (events.Count > 0)
            {
                allEvents.AddRange(events);
            }
        }

        return allEvents.Count == 0 ? Array.Empty<IDomainEvent>() : allEvents;
    }

    /// <summary>
    /// Clears all pending domain events from tracked entities within the specified database context.
    /// </summary>
    /// <param name="context">The database context containing tracked entities.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/></exception>
    public static void ClearEvents(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var entities = GetEntitiesWithDomainEvents(context);
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
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

        var entities = GetEntitiesWithDomainEvents(context);
        var allEvents = new List<IDomainEvent>();

        foreach (var entity in entities)
        {
            var events = entity.DrainDomainEvents();
            // Stryker disable once Equality : Optimization skipping empty event snapshots
            if (events.Count > 0)
            {
                allEvents.AddRange(events);
            }
        }

        return allEvents.Count == 0 ? Array.Empty<IDomainEvent>() : allEvents;
    }

    private static List<IHasDomainEvents> GetEntitiesWithDomainEvents(DbContext context)
    {
        return context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasDomainEvents)
            .Select(e => (IHasDomainEvents)e.Entity)
            .ToList();
    }

    private static IReadOnlyList<IDomainEvent> ExtractEvents(List<IHasDomainEvents> entities)
    {
        // Stryker disable once all : Fast zero-allocation return when entity list is empty
        if (entities.Count == 0)
        {
            return Array.Empty<IDomainEvent>();
        }

        var allEvents = new List<IDomainEvent>();
        foreach (var entity in entities)
        {
            var events = entity.DrainDomainEvents();
            // Stryker disable once Equality : Optimization skipping empty event snapshots
            if (events.Count > 0)
            {
                allEvents.AddRange(events);
            }
        }

        // Stryker disable once all : Return cached empty array rather than newly allocated empty list
        return allEvents.Count == 0 ? Array.Empty<IDomainEvent>() : allEvents;
    }

    private static void RestoreDrainedEvents(List<(IHasDomainEvents Entity, IReadOnlyList<IDomainEvent> Events)> records)
    {
        foreach (var (entity, events) in records)
        {
            entity.RequeueDomainEvents(events);
        }
    }
}



