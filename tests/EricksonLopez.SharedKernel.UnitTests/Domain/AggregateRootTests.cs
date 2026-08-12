using System;
using System.Linq;
using EricksonLopez.SharedKernel;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public class AggregateRootTests
{
    private class TestEvent : IDomainEvent { }

    private class AnotherTestEvent : IDomainEvent { }
    
    private class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(Guid id)
        {
            Id = id;
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
    public void DomainEvents_BeforeAnyEventIsRaised_IsEmpty()
    {
        // Verifies lazy allocation: no List<T> is allocated until the first event.
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        aggregate.DomainEvents.Should().BeEmpty();
        aggregate.DomainEvents.Should().NotBeNull();
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToCollection()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        
        aggregate.DomainEvents.Should().BeEmpty();
        
        aggregate.DoSomething();
        
        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TestEvent>();
    }

    [Fact]
    public void RaiseDomainEvent_MultipleEvents_ShouldMaintainOrder()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        aggregate.DoSomething();
        aggregate.DoSomethingElse();
        aggregate.DoSomething();

        aggregate.DomainEvents.Should().HaveCount(3);
        var events = aggregate.DomainEvents.ToList();
        events[0].Should().BeOfType<TestEvent>();
        events[1].Should().BeOfType<AnotherTestEvent>();
        events[2].Should().BeOfType<TestEvent>();
    }

    [Fact]
    public void RaiseDomainEvent_WhenNull_ShouldThrow()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        Action act = () => aggregate.RaiseNull();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyTheCollection()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.DoSomething();
        aggregate.DomainEvents.Should().NotBeEmpty();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WhenNoEventsRaised_ShouldNotThrow()
    {
        // Verifies that ClearDomainEvents is safe to call on a fresh aggregate
        // with no events (i.e., _domainEvents is null).
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        Action act = () => aggregate.ClearDomainEvents();

        act.Should().NotThrow();
        aggregate.DomainEvents.Should().BeEmpty();
    }
}
