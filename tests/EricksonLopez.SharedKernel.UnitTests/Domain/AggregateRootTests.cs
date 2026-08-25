// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public class AggregateRootTests
{
    private sealed record TestEvent : DomainEvent;

    private sealed record AnotherTestEvent : DomainEvent;

    private class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(Guid id) : base(id)
        {
        }

        public void DoSomething()
        {
            RaiseDomainEvent(new TestEvent());
        }

        public void DoSomethingElse()
        {
            RaiseDomainEvent(new AnotherTestEvent());
        }

        public void RaiseNull()
        {
            RaiseDomainEvent(null!);
        }
    }

    [Fact]
    public void Constructor_WithValidId_SetsIdProperty()
    {
        var id = Guid.NewGuid();
        var aggregate = new TestAggregateRoot(id);

        aggregate.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithDefaultId_ThrowsArgumentException()
    {
        var act = () => new TestAggregateRoot(Guid.Empty);

        var ex = act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .Which;

        ex.Message.Should().Contain("Entity identity cannot be default.");
    }

    [Fact]
    public void DrainDomainEvents_BeforeAnyEventIsRaised_ReturnsEmptyArray()
    {
        // Verifies lazy allocation: returns Array.Empty<IDomainEvent>() when _domainEvents is null
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        var events = aggregate.DrainDomainEvents();

        events.Should().BeEmpty();
    }

    [Fact]
    public void DrainDomainEvents_BeforeAnyEventIsRaised_ProducesZeroHeapAllocations()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        // Warm up JIT execution path
        _ = aggregate.DrainDomainEvents();

        var beforeAllocation = GC.GetAllocatedBytesForCurrentThread();
        var events = aggregate.DrainDomainEvents();
        var afterAllocation = GC.GetAllocatedBytesForCurrentThread();

        (afterAllocation - beforeAllocation).Should().Be(0,
            because: "DrainDomainEvents on an unpopulated aggregate root must return cached Array.Empty<IDomainEvent>() with zero heap allocations.");
        events.Should().BeEmpty();
    }

    [Fact]
    public void DrainDomainEvents_WhenEventRaised_ReturnsEventAndDetachesBuffer()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.DoSomething();

        var events = aggregate.DrainDomainEvents();

        events.Should().ContainSingle()
            .Which.Should().BeOfType<TestEvent>();

        // Subsequent call returns empty because buffer was detached
        var secondDrain = aggregate.DrainDomainEvents();
        secondDrain.Should().BeEmpty();
    }

    [Fact]
    public void DrainDomainEvents_MultipleEvents_MaintainsOrder()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        aggregate.DoSomething();
        aggregate.DoSomethingElse();
        aggregate.DoSomething();

        var events = aggregate.DrainDomainEvents();

        events.Should().HaveCount(3);
        events[0].Should().BeOfType<TestEvent>();
        events[1].Should().BeOfType<AnotherTestEvent>();
        events[2].Should().BeOfType<TestEvent>();
    }

    [Fact]
    public void RaiseDomainEvent_WhenNull_ShouldThrowArgumentNullException()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        var act = () => aggregate.RaiseNull();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("domainEvent");
    }

    [Fact]
    public void DrainDomainEvents_MultipleCycles_WorksCorrectly()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        // Cycle 1
        aggregate.DoSomething();
        var events1 = aggregate.DrainDomainEvents();
        events1.Should().HaveCount(1);
        aggregate.DrainDomainEvents().Should().BeEmpty();

        // Cycle 2
        aggregate.DoSomethingElse();
        aggregate.DoSomething();
        var events2 = aggregate.DrainDomainEvents();
        events2.Should().HaveCount(2);
        aggregate.DrainDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_ShouldImplement_IAggregateRoot_And_IHasDomainEvents()
    {
        // Verifies runtime type casting / pattern matching compatibility for domain consumers
        // (complements static assembly reflection rules enforced in ArchitectureTests).
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        (aggregate is IAggregateRoot).Should().BeTrue();
        (aggregate is IHasDomainEvents).Should().BeTrue();
    }

    [Fact]
    public void IHasDomainEvents_PolymorphicDispatch_ShouldWorkWithoutGenericParameters()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.DoSomething();

        IHasDomainEvents entityWithEvents = aggregate;
        var events = entityWithEvents.DrainDomainEvents();

        events.Should().ContainSingle()
            .Which.Should().BeOfType<TestEvent>();

        entityWithEvents.DrainDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void IAggregateRoot_PolymorphicDispatch_DrainsDomainEventsCorrectly()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.DoSomething();

        IAggregateRoot aggregateRoot = aggregate;
        var events = aggregateRoot.DrainDomainEvents();

        events.Should().ContainSingle()
            .Which.Should().BeOfType<TestEvent>();

        aggregateRoot.DrainDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task DrainDomainEvents_ConcurrentDraining_MaintainsBufferDetachmentIntegrity()
    {
        // ADR-011 documents that aggregates represent transactional single-threaded consistency boundaries.
        // This test verifies that even under synchronized concurrent drain attempts, callers either receive the detached
        // event batch or an empty collection without throwing unhandled collection mutation exceptions.
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.DoSomething();
        aggregate.DoSomethingElse();

        const int concurrencyLevel = 50;
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var collectedBatches = new System.Collections.Concurrent.ConcurrentBag<IReadOnlyList<IDomainEvent>>();

        var tasks = Enumerable.Range(0, concurrencyLevel).Select(async _ =>
        {
            await startSignal.Task;
            var events = aggregate.DrainDomainEvents();
            collectedBatches.Add(events);
        }).ToArray();

        // Release all concurrent tasks simultaneously without blocking ThreadPool threads
        startSignal.SetResult();

        await Task.WhenAll(tasks);

        // Exactly one thread should receive the 2 recorded events; all other threads receive empty collections
        var nonEmptyBatches = collectedBatches.Where(b => b.Count > 0).ToList();
        nonEmptyBatches.Should().ContainSingle(
            because: "Only a single caller should successfully detach the populated event buffer.");
        nonEmptyBatches[0].Should().HaveCount(2);

        // Subsequent drain is permanently empty
        aggregate.DrainDomainEvents().Should().BeEmpty();
    }
}



