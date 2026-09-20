// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.SharedKernel;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public sealed record SampleRaceEvent(int Sequence) : DomainEvent;

public sealed class SampleRaceAggregate : AggregateRoot<Guid>
{
    public SampleRaceAggregate(Guid id) : base(id) { }

    public void AddEvent(int sequence)
    {
        RaiseDomainEvent(new SampleRaceEvent(sequence));
    }
}

public class DomainEventsLiveViewRaceTests
{
    [Fact]
    public void EnumerationDuringRaiseDomainEvent_DoesNotThrowCollectionModifiedException()
    {
        var aggregate = new SampleRaceAggregate(Guid.NewGuid());
        for (var i = 0; i < 50; i++)
        {
            aggregate.AddEvent(i);
        }

        var events = aggregate.DomainEvents;
        var act = () =>
        {
            foreach (var evt in events)
            {
                aggregate.AddEvent(999);
            }
        };

        act.Should().NotThrow<InvalidOperationException>();
    }

}
