// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace EricksonLopez.SharedKernel.OpenTelemetry.Tests;

public sealed record HierarchyTestEventA(string Name) : DomainEvent;
public sealed record HierarchyTestEventB(int Code) : DomainEvent;
public sealed record HierarchyTestEventC(bool Flag) : DomainEvent;

[Collection("OpenTelemetry")]
public class OpenTelemetryActivityHierarchyTests
{
    [Fact]
    public void Dispatch_MultipleEvents_CreatesSiblingActivitiesUnderBatchParent()
    {
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSharedKernelInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var inner = Substitute.For<IDomainEventDispatcher>();
        var sut = new OpenTelemetryDomainEventDispatcher(inner);

        var events = new IDomainEvent[]
        {
            new HierarchyTestEventA("A"),
            new HierarchyTestEventB(42),
            new HierarchyTestEventC(true)
        };

        sut.Dispatch(events);

        tracerProvider.ForceFlush();

        var eventActivities = exportedActivities
            .Where(a => a.OperationName.StartsWith("DomainEvent HierarchyTestEvent", StringComparison.Ordinal))
            .ToList();

        eventActivities.Should().HaveCount(3);

        var firstEvent = eventActivities.First();
        var batchActivity = exportedActivities.FirstOrDefault(a => a.OperationName == "DomainEvents.DispatchBatch" && a.TraceId == firstEvent.TraceId);
        batchActivity.Should().NotBeNull();

        foreach (var eventActivity in eventActivities)
        {
            eventActivity.ParentId.Should().Be(batchActivity!.Id,
                because: "All child event activities in a batch must be direct siblings under the batch activity parent.");
        }

        inner.Received(1).Dispatch(events);
    }
}
