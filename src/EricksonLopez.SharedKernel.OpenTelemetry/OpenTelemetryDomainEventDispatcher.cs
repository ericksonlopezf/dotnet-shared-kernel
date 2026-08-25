// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly IDomainEventDispatcher _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenTelemetryDomainEventDispatcher"/> class wrapping the specified dispatcher.
    /// </summary>
    /// <param name="inner">The underlying domain event dispatcher to wrap.</param>
    /// <exception cref="ArgumentNullException"><paramref name="inner"/> is <see langword="null"/></exception>
    public OpenTelemetryDomainEventDispatcher(IDomainEventDispatcher inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc/>
    public async ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        if (domainEvents.Count == 0)
            return;

        using var batchActivity = SharedKernelInstrumentation.ActivitySource.StartActivity(
            "DomainEvents.DispatchBatch",
            ActivityKind.Internal);

        batchActivity?.SetTag("domain_events.batch_size", domainEvents.Count);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            foreach (var domainEvent in domainEvents)
            {
                var eventType = domainEvent.GetType().Name;
                using var eventActivity = SharedKernelInstrumentation.ActivitySource.StartActivity(
                    $"DomainEvent {eventType}",
                    ActivityKind.Internal);

                eventActivity?.SetTag(SharedKernelInstrumentation.Attributes.EventId, domainEvent.Id.ToString());
                eventActivity?.SetTag(SharedKernelInstrumentation.Attributes.EventType, eventType);
                eventActivity?.SetTag(SharedKernelInstrumentation.Attributes.OccurredAt, domainEvent.OccurredAt.ToString("O"));
                eventActivity?.SetStatus(ActivityStatusCode.Ok);

                SharedKernelInstrumentation.DispatchedEventsCounter.Add(
                    1,
                    new KeyValuePair<string, object?>("event_type", eventType));
            }


            // Stryker disable once Boolean: ConfigureAwait(false) is standard library practice to avoid capturing synchronization context
            await _inner.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false);

            batchActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            batchActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            batchActivity?.AddException(ex);
            throw;
        }
        finally
        {
            // Stryker disable once Statement: stopwatch.Stop() freezes local timer before reading Elapsed; local stopwatch is not reused
            stopwatch.Stop();
            SharedKernelInstrumentation.DispatchDurationHistogram.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("batch_size", domainEvents.Count));
        }
    }
}
