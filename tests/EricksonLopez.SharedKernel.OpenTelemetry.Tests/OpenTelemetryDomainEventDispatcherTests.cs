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
}

