// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using Xunit;

#pragma warning disable CA1806 // Do not ignore method results

namespace EricksonLopez.SharedKernel.UnitTests.Adversarial;

public class RedTeamTests
{
    private class TestEntity : Entity<string>
    {
        public TestEntity(string id) : base(id) { }
    }

    private class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id) : base(id) { }
        public void DoSomething() => RaiseDomainEvent(new TestEvent());
    }

    private record TestEvent : DomainEvent;

    [Fact]
    public void Entity_GivenNullStringId_ShouldThrow()
    {
        Action act = () => new TestEntity(null!);
        act.Should().Throw<ArgumentException>(); // Tests nullability boundary bypass
    }

    [Fact]
    public void AggregateRoot_RequeueEvents_GivenNullEvent_ShouldThrow()
    {
        var agg = new TestAggregate(Guid.NewGuid());
        var events = new List<IDomainEvent> { new TestEvent(), null! };
        Action act = () => agg.RequeueDomainEvents(events);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AggregateRoot_RequeueEvents_GivenRepeatedEvents_ShouldThrowArgumentException()
    {
        var agg = new TestAggregate(Guid.NewGuid());
        var ev = new TestEvent();
        var events = new List<IDomainEvent> { ev, ev };
        Action act = () => agg.RequeueDomainEvents(events);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AggregateRoot_DrainEvents_Repeated_ShouldReturnEmpty()
    {
        var agg = new TestAggregate(Guid.NewGuid());
        agg.DoSomething();
        var first = agg.DrainDomainEvents();
        var second = agg.DrainDomainEvents();

        first.Should().HaveCount(1);
        second.Should().BeEmpty();
    }
}
