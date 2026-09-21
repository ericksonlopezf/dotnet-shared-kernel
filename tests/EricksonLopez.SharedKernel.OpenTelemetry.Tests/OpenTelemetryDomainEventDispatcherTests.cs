// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace EricksonLopez.SharedKernel.OpenTelemetry.Tests;

public sealed record TestOrderPlacedEvent(Guid OrderId, decimal Amount) : DomainEvent;

[Collection("OpenTelemetry")]
public class OpenTelemetryDomainEventDispatcherTests
{
    [Fact]
    public void Constructor_WithNullInner_ThrowsArgumentNullException()
    {
        var act = () => new OpenTelemetryDomainEventDispatcher(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inner");
    }

    [Fact]
    public async Task DispatchAsync_WhenEventsAreDispatched_CreatesActivitiesWithExpectedTags()
    {
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSharedKernelInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var innerDispatcher = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(innerDispatcher);

        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 199.99m);
        var events = new IDomainEvent[] { domainEvent };

        await sut.DispatchAsync(events, CancellationToken.None);

        tracerProvider.ForceFlush();

        exportedActivities.Should().NotBeEmpty();

        var batchActivity = exportedActivities.FirstOrDefault(a => a.OperationName == "DomainEvents.DispatchBatch");
        batchActivity.Should().NotBeNull();
        batchActivity!.GetTagItem("domain_events.batch_size").Should().Be(1);
        batchActivity.Status.Should().Be(ActivityStatusCode.Ok);

        var eventActivity = exportedActivities.FirstOrDefault(a => a.OperationName.Contains(nameof(TestOrderPlacedEvent)));
        eventActivity.Should().NotBeNull();
        eventActivity!.GetTagItem(SharedKernelInstrumentation.Attributes.EventId).Should().Be(domainEvent.Id.ToString());
        eventActivity.GetTagItem(SharedKernelInstrumentation.Attributes.EventType).Should().Be(nameof(TestOrderPlacedEvent));
        eventActivity.GetTagItem(SharedKernelInstrumentation.Attributes.OccurredAt).Should().Be(domainEvent.OccurredAt.ToString("O"));
        eventActivity.Status.Should().Be(ActivityStatusCode.Ok);

        await innerDispatcher.Received(1).DispatchAsync(events, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_WhenEventsAreDispatched_RecordsExpectedMetrics()
    {
        var counterMeasurements = new List<(long value, KeyValuePair<string, object?> tag)>();
        var histogramMeasurements = new List<(double value, KeyValuePair<string, object?> tag)>();

        using var meterListener = new System.Diagnostics.Metrics.MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == SharedKernelInstrumentation.ActivitySourceName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "domain_events.dispatched")
            {
                var tag = tags.ToArray().FirstOrDefault(t => t.Key == "event_type");
                counterMeasurements.Add((measurement, tag));
            }
        });

        meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "domain_events.dispatch_duration")
            {
                var tag = tags.ToArray().FirstOrDefault(t => t.Key == "batch_size");
                histogramMeasurements.Add((measurement, tag));
            }
        });

        meterListener.Start();

        var innerDispatcher = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(innerDispatcher);

        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 100m);
        await sut.DispatchAsync(new[] { domainEvent }, CancellationToken.None);

        meterListener.RecordObservableInstruments();

        counterMeasurements.Should().ContainSingle();
        counterMeasurements[0].value.Should().Be(1);
        counterMeasurements[0].tag.Key.Should().Be("event_type");
        counterMeasurements[0].tag.Value.Should().Be(nameof(TestOrderPlacedEvent));

        histogramMeasurements.Should().ContainSingle();
        histogramMeasurements[0].value.Should().BeGreaterThanOrEqualTo(0);
        histogramMeasurements[0].tag.Key.Should().Be("batch_size");
        histogramMeasurements[0].tag.Value.Should().Be(1);
    }

    [Fact]
    public async Task DispatchAsync_WhenNullEvents_ThrowsArgumentNullException()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);

        var act = async () => await sut.DispatchAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("domainEvents");
    }

    [Fact]
    public async Task DispatchAsync_WhenEmptyEvents_CompletesWithoutDispatching()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);

        await sut.DispatchAsync(Array.Empty<IDomainEvent>(), CancellationToken.None);

        await inner.DidNotReceive().DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_WhenInnerDispatcherThrows_SetsErrorStatusAndRethrows()
    {
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSharedKernelInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var inner = Substitute.For<IDomainEventDispatcher>();
        inner.DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask(Task.FromException(new InvalidOperationException("Simulated broker failure"))));

        var sut = new OpenTelemetryDomainEventDispatcher(inner);
        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 50.0m);

        var act = async () => await sut.DispatchAsync(new[] { domainEvent }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Simulated broker failure");

        tracerProvider.ForceFlush();

        var batchActivity = exportedActivities.FirstOrDefault(a => a.OperationName == "DomainEvents.DispatchBatch");
        batchActivity.Should().NotBeNull();
        batchActivity!.Status.Should().Be(ActivityStatusCode.Error);
        batchActivity.StatusDescription.Should().Be("Simulated broker failure");
        batchActivity.Events.Should().Contain(e => e.Name == "exception");

        var eventActivity = exportedActivities.FirstOrDefault(a => a.OperationName.Contains(nameof(TestOrderPlacedEvent)));
        eventActivity.Should().NotBeNull();
        eventActivity!.Status.Should().Be(ActivityStatusCode.Error);
        eventActivity.StatusDescription.Should().Be("Simulated broker failure");
        eventActivity.Events.Should().Contain(e => e.Name == "exception");
    }

    [Fact]
    public void Instrumentation_Metadata_HasExpectedUnitsAndDescriptions()
    {
        SharedKernelInstrumentation.DispatchedEventsCounter.Unit.Should().Be("{events}");
        SharedKernelInstrumentation.DispatchedEventsCounter.Description.Should().Be("Total count of domain events dispatched");
        SharedKernelInstrumentation.DispatchDurationHistogram.Unit.Should().Be("ms");
        SharedKernelInstrumentation.DispatchDurationHistogram.Description.Should().Be("Duration of domain event dispatch in milliseconds");
    }

    [Fact]
    public void AddSharedKernelInstrumentation_WithValidBuilders_ConfiguresTracerAndMeter()
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSharedKernelInstrumentation()
            .Build();

        tracerProvider.Should().NotBeNull();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddSharedKernelInstrumentation()
            .Build();

        meterProvider.Should().NotBeNull();
    }

    [Fact]
    public async Task DispatchAsync_WhenNoTracerProviderRegistered_DispatchesSuccessfullyWithoutNullReference()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);

        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 25m);
        var events = new IDomainEvent[] { domainEvent };

        await sut.DispatchAsync(events, CancellationToken.None);

        await inner.Received(1).DispatchAsync(events, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_WhenNoTracerProviderRegisteredAndInnerThrows_RethrowsExceptionSafely()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        inner.DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask(Task.FromException(new InvalidOperationException("No-tracer failure"))));

        var sut = new OpenTelemetryDomainEventDispatcher(inner);
        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 25m);

        var act = async () => await sut.DispatchAsync(new[] { domainEvent }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No-tracer failure");
    }

    [Fact]
    public void AddSharedKernelInstrumentation_WithNullTracerBuilder_ThrowsArgumentNullException()
    {
        TracerProviderBuilder builder = null!;

        var act = () => builder.AddSharedKernelInstrumentation();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Fact]
    public void AddSharedKernelInstrumentation_WithNullMeterBuilder_ThrowsArgumentNullException()
    {
        MeterProviderBuilder builder = null!;

        var act = () => builder.AddSharedKernelInstrumentation();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Fact]
    public void Dispatch_Synchronous_WhenEventsAreDispatched_CreatesActivitiesWithExpectedTagsAndRecordsMetrics()
    {
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSharedKernelInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var counterMeasurements = new List<(long value, string? eventType)>();
        var histogramMeasurements = new List<(double value, int? batchSize)>();

        using var meterListener = new System.Diagnostics.Metrics.MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == SharedKernelInstrumentation.ActivitySourceName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "domain_events.dispatched")
            {
                var tag = tags.ToArray().FirstOrDefault(t => t.Key == "event_type");
                counterMeasurements.Add((measurement, (string?)tag.Value));
            }
        });

        meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "domain_events.dispatch_duration")
            {
                var tag = tags.ToArray().FirstOrDefault(t => t.Key == "batch_size");
                histogramMeasurements.Add((measurement, (int?)tag.Value));
            }
        });

        meterListener.Start();

        var innerDispatcher = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(innerDispatcher);

        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 199.99m);
        var events = new IDomainEvent[] { domainEvent };

        sut.Dispatch(events);

        tracerProvider.ForceFlush();

        exportedActivities.Should().NotBeEmpty();
        var batchActivity = exportedActivities.FirstOrDefault(a => a.OperationName == "DomainEvents.DispatchBatch");
        batchActivity.Should().NotBeNull();
        batchActivity!.GetTagItem("domain_events.batch_size").Should().Be(1);
        batchActivity.Status.Should().Be(ActivityStatusCode.Ok);

        var eventActivity = exportedActivities.FirstOrDefault(a => a.OperationName.Contains(nameof(TestOrderPlacedEvent)));
        eventActivity.Should().NotBeNull();
        eventActivity!.GetTagItem(SharedKernelInstrumentation.Attributes.EventId).Should().Be(domainEvent.Id.ToString());
        eventActivity.GetTagItem(SharedKernelInstrumentation.Attributes.EventType).Should().Be(nameof(TestOrderPlacedEvent));
        eventActivity.GetTagItem(SharedKernelInstrumentation.Attributes.OccurredAt).Should().Be(domainEvent.OccurredAt.ToString("O"));
        eventActivity.Status.Should().Be(ActivityStatusCode.Ok);
        eventActivity.ParentId.Should().Be(batchActivity.Id);

        innerDispatcher.Received(1).Dispatch(events);

        counterMeasurements.Should().ContainSingle();
        counterMeasurements[0].value.Should().Be(1);
        counterMeasurements[0].eventType.Should().Be(nameof(TestOrderPlacedEvent));

        histogramMeasurements.Should().ContainSingle();
        histogramMeasurements[0].value.Should().BeGreaterThanOrEqualTo(0);
        histogramMeasurements[0].batchSize.Should().Be(1);
    }

    [Fact]
    public void Dispatch_Synchronous_WhenNullEvents_ThrowsArgumentNullException()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);

        var act = () => sut.Dispatch(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("domainEvents");
    }

    [Fact]
    public async Task Dispatch_WhenContainsNullElement_ThrowsArgumentException_WithExactIndexAndParamName()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);
        var validEvt = new TestOrderPlacedEvent(Guid.NewGuid(), 10m);

        // Index 0
        var actSync0 = () => sut.Dispatch(new IDomainEvent[] { null! });
        actSync0.Should().Throw<ArgumentException>()
            .WithParameterName("domainEvents")
            .WithMessage("Domain event at index 0 cannot be null. (Parameter 'domainEvents')");

        var actAsync0 = async () => await sut.DispatchAsync(new IDomainEvent[] { null! });
        await actAsync0.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("domainEvents")
            .WithMessage("Domain event at index 0 cannot be null. (Parameter 'domainEvents')");

        // Index 1
        var actSync1 = () => sut.Dispatch(new IDomainEvent[] { validEvt, null! });
        actSync1.Should().Throw<ArgumentException>()
            .WithParameterName("domainEvents")
            .WithMessage("Domain event at index 1 cannot be null. (Parameter 'domainEvents')");

        var actAsync1 = async () => await sut.DispatchAsync(new IDomainEvent[] { validEvt, null! });
        await actAsync1.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("domainEvents")
            .WithMessage("Domain event at index 1 cannot be null. (Parameter 'domainEvents')");
    }

    [Fact]
    public void Dispatch_Synchronous_WhenEmptyEvents_CompletesWithoutCallingInner()
    {
        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);

        sut.Dispatch(Array.Empty<IDomainEvent>());
        inner.DidNotReceive().Dispatch(Arg.Any<IReadOnlyList<IDomainEvent>>());
    }

    [Fact]
    public void Dispatch_Synchronous_WhenInnerThrows_SetsErrorStatusAndRethrows()
    {
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSharedKernelInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var inner = Substitute.For<IDomainEventDispatcher>();
        inner.When(d => d.Dispatch(Arg.Any<IReadOnlyList<IDomainEvent>>()))
            .Do(_ => throw new InvalidOperationException("Sync broker failure"));

        var sut = new OpenTelemetryDomainEventDispatcher(inner);
        var domainEvent = new TestOrderPlacedEvent(Guid.NewGuid(), 50.0m);

        var act = () => sut.Dispatch(new[] { domainEvent });
        act.Should().Throw<InvalidOperationException>().WithMessage("Sync broker failure");

        tracerProvider.ForceFlush();

        var batchActivity = exportedActivities.FirstOrDefault(a => a.OperationName == "DomainEvents.DispatchBatch");
        batchActivity.Should().NotBeNull();
        batchActivity!.Status.Should().Be(ActivityStatusCode.Error);
        batchActivity.StatusDescription.Should().Be("Sync broker failure");
        batchActivity.Events.Should().Contain(e => e.Name == "exception");

        var eventActivity = exportedActivities.FirstOrDefault(a => a.OperationName.Contains(nameof(TestOrderPlacedEvent)));
        eventActivity.Should().NotBeNull();
        eventActivity!.Status.Should().Be(ActivityStatusCode.Error);
        eventActivity.StatusDescription.Should().Be("Sync broker failure");
        eventActivity.Events.Should().Contain(e => e.Name == "exception");
    }

    [Fact]
    public void Constructor_WithCustomActivitySourceAndMeter_CreatesExpectedInstrumentsOnCustomMeter()
    {
        using var customMeter = new System.Diagnostics.Metrics.Meter("Custom.SharedKernel");
        using var customActivitySource = new ActivitySource("Custom.SharedKernel");
        var inner = Substitute.For<IDomainEventDispatcher>();

        var sut = new OpenTelemetryDomainEventDispatcher(inner, customActivitySource, customMeter);
        sut.Should().NotBeNull();

        var counterField = typeof(OpenTelemetryDomainEventDispatcher).GetField("_dispatchedEventsCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var counter = (System.Diagnostics.Metrics.Counter<long>?)counterField?.GetValue(sut);
        counter.Should().NotBeNull();
        counter!.Name.Should().Be("domain_events.dispatched");
        counter.Unit.Should().Be("{events}");
        counter.Description.Should().Be("Total count of domain events dispatched");

        var histField = typeof(OpenTelemetryDomainEventDispatcher).GetField("_dispatchDurationHistogram", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hist = (System.Diagnostics.Metrics.Histogram<double>?)histField?.GetValue(sut);
        hist.Should().NotBeNull();
        hist!.Name.Should().Be("domain_events.dispatch_duration");
        hist.Unit.Should().Be("ms");
        hist.Description.Should().Be("Duration of domain event dispatch in milliseconds");
    }

    [Fact]
    public void RecordDomainEventDispatched_WithNullEventType_ThrowsArgumentNullException()
    {
        var act = () => SharedKernelInstrumentation.RecordDomainEventDispatched(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("eventType");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("tenant-123", true)]
    public void RecordDomainEventDispatched_RecordsExpectedTags(string? tenantId, bool shouldHaveTenantTag)
    {
        var measurements = new List<KeyValuePair<string, object?>[]>();
        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (i, l) =>
        {
            if (i.Name == "domain_events.dispatched") l.EnableMeasurementEvents(i);
        };
        listener.SetMeasurementEventCallback<long>((i, val, tags, state) =>
        {
            measurements.Add(tags.ToArray());
        });
        listener.Start();

        SharedKernelInstrumentation.RecordDomainEventDispatched("TestEvent", tenantId);

        measurements.Should().ContainSingle();
        var recordedTags = measurements[0];
        recordedTags.Should().Contain(t => t.Key == "event_type" && (string?)t.Value == "TestEvent");

        if (shouldHaveTenantTag)
        {
            recordedTags.Should().Contain(t => t.Key == SharedKernelInstrumentation.Attributes.TenantId && (string?)t.Value == tenantId);
        }
        else
        {
            recordedTags.Should().NotContain(t => t.Key == SharedKernelInstrumentation.Attributes.TenantId);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("tenant-456", true)]
    public void RecordDispatchDuration_RecordsExpectedTags(string? tenantId, bool shouldHaveTenantTag)
    {
        var measurements = new List<KeyValuePair<string, object?>[]>();
        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (i, l) =>
        {
            if (i.Name == "domain_events.dispatch_duration") l.EnableMeasurementEvents(i);
        };
        listener.SetMeasurementEventCallback<double>((i, val, tags, state) =>
        {
            measurements.Add(tags.ToArray());
        });
        listener.Start();

        SharedKernelInstrumentation.RecordDispatchDuration(42.5, 3, tenantId);

        measurements.Should().ContainSingle();
        var recordedTags = measurements[0];
        recordedTags.Should().Contain(t => t.Key == "batch_size" && (int?)t.Value == 3);

        if (shouldHaveTenantTag)
        {
            recordedTags.Should().Contain(t => t.Key == SharedKernelInstrumentation.Attributes.TenantId && (string?)t.Value == tenantId);
        }
        else
        {
            recordedTags.Should().NotContain(t => t.Key == SharedKernelInstrumentation.Attributes.TenantId);
        }
    }
}

