// Copyright © Erickson Lopez. MIT License.
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Builders;

/// <summary>
/// Fluent test builder for constructing parameterized collections of <see cref="CustomerAggregate"/> instances
/// populated with deterministic revision events for high-volume and lifecycle testing.
/// </summary>
public sealed class AggregateTestBuilder
{
    private int _aggregateCount = 1;
    private int _eventsPerAggregate = 1;
    private string _namePrefix = "User";

    /// <summary>
    /// Creates a new instance of <see cref="AggregateTestBuilder"/>.
    /// </summary>
    public static AggregateTestBuilder Create() => new();

    /// <summary>
    /// Sets the total number of aggregate root instances to generate.
    /// </summary>
    public AggregateTestBuilder WithAggregateCount(int count)
    {
        _aggregateCount = count;
        return this;
    }

    /// <summary>
    /// Sets the number of domain events to raise on each aggregate root instance.
    /// </summary>
    public AggregateTestBuilder WithEventsPerAggregate(int eventsPerAggregate)
    {
        _eventsPerAggregate = eventsPerAggregate;
        return this;
    }

    /// <summary>
    /// Sets the customer name prefix.
    /// </summary>
    public AggregateTestBuilder WithNamePrefix(string prefix)
    {
        _namePrefix = prefix;
        return this;
    }

    /// <summary>
    /// Builds the list of configured <see cref="CustomerAggregate"/> instances with raised domain events.
    /// </summary>
    public List<CustomerAggregate> Build()
    {
        var aggregates = new List<CustomerAggregate>(_aggregateCount);
        for (var i = 0; i < _aggregateCount; i++)
        {
            var aggregate = new CustomerAggregate(CustomerId.New(), $"{_namePrefix} {i}");
            for (var j = 1; j < _eventsPerAggregate; j++)
            {
                aggregate.UpdateName($"{_namePrefix} {i} - Revision {j}");
            }
            aggregates.Add(aggregate);
        }
        return aggregates;
    }

    /// <summary>
    /// Verifies the structural grouping and chronological emission order of drained domain events in O(N) time.
    /// </summary>
    public void VerifyDrainedEvents(IReadOnlyList<CustomerAggregate> aggregates, IReadOnlyList<IDomainEvent> drainedEvents)
    {
        drainedEvents.Should().HaveCount(_aggregateCount * _eventsPerAggregate);

        var eventsByAggregate = new Dictionary<CustomerId, List<IDomainEvent>>(_aggregateCount);
        foreach (var e in drainedEvents)
        {
            CustomerId id = e switch
            {
                CustomerRegisteredEvent cre => cre.CustomerId,
                CustomerNameUpdatedEvent cue => cue.CustomerId,
                _ => throw new System.InvalidOperationException("Unknown event type")
            };
            if (!eventsByAggregate.TryGetValue(id, out var list))
            {
                list = new List<IDomainEvent>(_eventsPerAggregate);
                eventsByAggregate[id] = list;
            }
            list.Add(e);
        }

        eventsByAggregate.Should().HaveCount(_aggregateCount);

        for (var i = 0; i < _aggregateCount; i++)
        {
            var aggregate = aggregates[i];
            var aggregateEvents = eventsByAggregate[aggregate.Id];
            
            aggregateEvents.Should().HaveCount(_eventsPerAggregate);
            aggregateEvents[0].Should().BeOfType<CustomerRegisteredEvent>();

            for (var j = 1; j < _eventsPerAggregate; j++)
            {
                var updateEvent = aggregateEvents[j].Should().BeOfType<CustomerNameUpdatedEvent>().Subject;
                updateEvent.NewName.Should().Be($"{_namePrefix} {i} - Revision {j}");
            }
        }
    }
}
