// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;

namespace EricksonLopez.SharedKernel.OpenTelemetry;

/// <summary>
/// Decorates an <see cref="IDomainEventDispatcher"/> with OpenTelemetry distributed tracing spans and metrics collection.
/// </summary>
public sealed class OpenTelemetryDomainEventDispatcher : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, string> _eventTypeNameCache = new();
    private readonly IDomainEventDispatcher _inner;
    private readonly ActivitySource _activitySource;
    private readonly Counter<long> _dispatchedEventsCounter;
    private readonly Histogram<double> _dispatchDurationHistogram;

    private static string GetEventTypeName(IDomainEvent domainEvent)
        => _eventTypeNameCache.GetOrAdd(domainEvent.GetType(), static t => t.Name);

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenTelemetryDomainEventDispatcher"/> class wrapping the specified dispatcher.
    /// </summary>
    /// <param name="inner">The underlying domain event dispatcher to wrap.</param>
    /// <exception cref="ArgumentNullException"><paramref name="inner"/> is <see langword="null"/></exception>
    public OpenTelemetryDomainEventDispatcher(IDomainEventDispatcher inner)
        : this(inner, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenTelemetryDomainEventDispatcher"/> class with optional custom activity source and meter.
    /// </summary>
    /// <param name="inner">The underlying domain event dispatcher to wrap.</param>
    /// <param name="activitySource">Optional custom activity source; falls back to <see cref="SharedKernelInstrumentation.ActivitySource"/> if <see langword="null"/>.</param>
    /// <param name="meter">Optional custom meter; falls back to <see cref="SharedKernelInstrumentation.Meter"/> if <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="inner"/> is <see langword="null"/></exception>
    public OpenTelemetryDomainEventDispatcher(
        IDomainEventDispatcher inner,
        ActivitySource? activitySource = null,
        Meter? meter = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _activitySource = activitySource ?? SharedKernelInstrumentation.ActivitySource;

        if (meter is not null)
        {
            _dispatchedEventsCounter = meter.CreateCounter<long>(
                "domain_events.dispatched",
                unit: "{events}",
                description: "Total count of domain events dispatched");
            _dispatchDurationHistogram = meter.CreateHistogram<double>(
                "domain_events.dispatch_duration",
                unit: "ms",
                description: "Duration of domain event dispatch in milliseconds");
        }
        else
        {
            _dispatchedEventsCounter = SharedKernelInstrumentation.DispatchedEventsCounter;
            _dispatchDurationHistogram = SharedKernelInstrumentation.DispatchDurationHistogram;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        for (var i = 0; i < domainEvents.Count; i++)
        {
            if (domainEvents[i] is null)
            {
                throw new ArgumentException($"Domain event at index {i} cannot be null.", nameof(domainEvents));
            }
        }

        if (domainEvents.Count == 0)
            return;

        using var batchActivity = _activitySource.StartActivity(
            "DomainEvents.DispatchBatch",
            ActivityKind.Internal);

        batchActivity?.SetTag("domain_events.batch_size", domainEvents.Count);

        List<Activity?>? eventActivities = null;
        if (_activitySource.HasListeners())
        {
            eventActivities = new List<Activity?>(domainEvents.Count);
            foreach (var domainEvent in domainEvents)
            {
                Activity.Current = batchActivity;
                var eventType = GetEventTypeName(domainEvent);
                // Stryker disable once NullCoalescing : Activity.Current is explicitly set to batchActivity above, making fallback to default context functionally equivalent in .NET BCL
                var activity = _activitySource.StartActivity(
                    $"DomainEvent {eventType}",
                    ActivityKind.Internal,
                    batchActivity?.Context ?? default);

                if (activity is not null)
                {
                    activity.SetTag(SharedKernelInstrumentation.Attributes.EventId, domainEvent.Id.ToString());
                    activity.SetTag(SharedKernelInstrumentation.Attributes.EventType, eventType);
                    activity.SetTag(SharedKernelInstrumentation.Attributes.OccurredAt, domainEvent.OccurredAt.ToString("O"));
                }

                eventActivities.Add(activity);
            }
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            Activity.Current = batchActivity;
            // Stryker disable once Boolean: ConfigureAwait(false) is standard library practice to avoid capturing synchronization context
            await _inner.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false);

            foreach (var domainEvent in domainEvents)
            {
                var eventType = GetEventTypeName(domainEvent);
                _dispatchedEventsCounter.Add(
                    1,
                    new KeyValuePair<string, object?>("event_type", eventType));
            }

            if (eventActivities is not null)
            {
                foreach (var activity in eventActivities)
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
            }

            batchActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            if (eventActivities is not null)
            {
                foreach (var activity in eventActivities)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                }
            }

            batchActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            batchActivity?.AddException(ex);
            throw;
        }
        finally
        {
            if (eventActivities is not null)
            {
                foreach (var activity in eventActivities)
                {
                    activity?.Dispose();
                }
            }

            // Stryker disable once Statement: stopwatch.Stop() freezes local timer before reading Elapsed; local stopwatch is not reused
            stopwatch.Stop();
            _dispatchDurationHistogram.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("batch_size", domainEvents.Count));
        }
    }

    /// <inheritdoc/>
    public void Dispatch(IReadOnlyList<IDomainEvent> domainEvents)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        for (var i = 0; i < domainEvents.Count; i++)
        {
            if (domainEvents[i] is null)
            {
                throw new ArgumentException($"Domain event at index {i} cannot be null.", nameof(domainEvents));
            }
        }

        if (domainEvents.Count == 0)
            return;

        using var batchActivity = _activitySource.StartActivity(
            "DomainEvents.DispatchBatch",
            ActivityKind.Internal);

        batchActivity?.SetTag("domain_events.batch_size", domainEvents.Count);

        List<Activity?>? eventActivities = null;
        if (_activitySource.HasListeners())
        {
            eventActivities = new List<Activity?>(domainEvents.Count);
            foreach (var domainEvent in domainEvents)
            {
                Activity.Current = batchActivity;
                var eventType = GetEventTypeName(domainEvent);
                // Stryker disable once NullCoalescing : Activity.Current is explicitly set to batchActivity above, making fallback to default context functionally equivalent in .NET BCL
                var activity = _activitySource.StartActivity(
                    $"DomainEvent {eventType}",
                    ActivityKind.Internal,
                    batchActivity?.Context ?? default);

                if (activity is not null)
                {
                    activity.SetTag(SharedKernelInstrumentation.Attributes.EventId, domainEvent.Id.ToString());
                    activity.SetTag(SharedKernelInstrumentation.Attributes.EventType, eventType);
                    activity.SetTag(SharedKernelInstrumentation.Attributes.OccurredAt, domainEvent.OccurredAt.ToString("O"));
                }

                eventActivities.Add(activity);
            }
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            Activity.Current = batchActivity;
            _inner.Dispatch(domainEvents);

            foreach (var domainEvent in domainEvents)
            {
                var eventType = GetEventTypeName(domainEvent);
                _dispatchedEventsCounter.Add(
                    1,
                    new KeyValuePair<string, object?>("event_type", eventType));
            }

            if (eventActivities is not null)
            {
                foreach (var activity in eventActivities)
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
            }

            batchActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            if (eventActivities is not null)
            {
                foreach (var activity in eventActivities)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                }
            }

            batchActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            batchActivity?.AddException(ex);
            throw;
        }
        finally
        {
            if (eventActivities is not null)
            {
                foreach (var activity in eventActivities)
                {
                    activity?.Dispose();
                }
            }

            // Stryker disable once Statement: stopwatch.Stop() freezes local timer before reading Elapsed; local stopwatch is not reused
            stopwatch.Stop();
            _dispatchDurationHistogram.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("batch_size", domainEvents.Count));
        }
    }
}
