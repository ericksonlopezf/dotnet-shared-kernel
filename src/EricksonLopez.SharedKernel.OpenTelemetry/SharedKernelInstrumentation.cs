// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EricksonLopez.SharedKernel.OpenTelemetry;

/// <summary>
/// Provides OpenTelemetry diagnostic activity sources, meters, and metric instruments for domain event dispatching.
/// </summary>
public static class SharedKernelInstrumentation
{
    /// <summary>
    /// Specifies the name of the OpenTelemetry <see cref="System.Diagnostics.ActivitySource"/> and <see cref="System.Diagnostics.Metrics.Meter"/>.
    /// </summary>
    public const string ActivitySourceName = "EricksonLopez.SharedKernel";

    /// <summary>
    /// Specifies the instrumentation library version.
    /// </summary>
    public const string Version = "3.0.0";

    /// <summary>
    /// Specifies the <see cref="System.Diagnostics.ActivitySource"/> used to create OpenTelemetry spans during domain event dispatch.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version);

    /// <summary>
    /// Specifies the <see cref="System.Diagnostics.Metrics.Meter"/> used to record domain event metrics.
    /// </summary>
    public static readonly Meter Meter = new(ActivitySourceName, Version);

    /// <summary>
    /// Specifies the metric counter tracking the total number of domain events dispatched.
    /// </summary>
    public static readonly Counter<long> DispatchedEventsCounter =
        Meter.CreateCounter<long>(
            "domain_events.dispatched",
            unit: "{events}",
            description: "Total count of domain events dispatched");

    /// <summary>
    /// Specifies the metric histogram tracking execution durations of domain event dispatch operations.
    /// </summary>
    public static readonly Histogram<double> DispatchDurationHistogram =
        Meter.CreateHistogram<double>(
            "domain_events.dispatch_duration",
            unit: "ms",
            description: "Duration of domain event dispatch in milliseconds");

    /// <summary>
    /// Defines OpenTelemetry semantic attribute keys for domain event spans and metrics.
    /// </summary>
    public static class Attributes
    {
        /// <summary>
        /// Specifies the semantic attribute key for the unique domain event identifier.
        /// </summary>
        public const string EventId = "domain_event.id";

        /// <summary>
        /// Specifies the semantic attribute key for the domain event CLR type name.
        /// </summary>
        public const string EventType = "domain_event.type";

        /// <summary>
        /// Specifies the semantic attribute key for the UTC occurrence timestamp of the domain event.
        /// </summary>
        public const string OccurredAt = "domain_event.occurred_at";

        /// <summary>
        /// Specifies the semantic attribute key for the tenant identifier in multi-tenant environments.
        /// </summary>
        public const string TenantId = "tenant.id";
    }

    /// <summary>
    /// Records the dispatch of a domain event with optional tenant context and custom tags.
    /// </summary>
    /// <param name="eventType">The CLR name or type of the domain event.</param>
    /// <param name="tenantId">The optional tenant identifier for multi-tenant isolation.</param>
    public static void RecordDomainEventDispatched(string eventType, string? tenantId = null)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        if (string.IsNullOrEmpty(tenantId))
        {
            DispatchedEventsCounter.Add(1, new KeyValuePair<string, object?>("event_type", eventType));
        }
        else
        {
            DispatchedEventsCounter.Add(
                1,
                new KeyValuePair<string, object?>("event_type", eventType),
                new KeyValuePair<string, object?>(Attributes.TenantId, tenantId));
        }
    }

    /// <summary>
    /// Records the duration of a domain event dispatch batch in milliseconds with optional tenant context.
    /// </summary>
    /// <param name="durationMs">The execution duration in milliseconds.</param>
    /// <param name="batchSize">The count of domain events in the batch.</param>
    /// <param name="tenantId">The optional tenant identifier for multi-tenant isolation.</param>
    public static void RecordDispatchDuration(double durationMs, int batchSize, string? tenantId = null)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            DispatchDurationHistogram.Record(durationMs, new KeyValuePair<string, object?>("batch_size", batchSize));
        }
        else
        {
            DispatchDurationHistogram.Record(
                durationMs,
                new KeyValuePair<string, object?>("batch_size", batchSize),
                new KeyValuePair<string, object?>(Attributes.TenantId, tenantId));
        }
    }
}
