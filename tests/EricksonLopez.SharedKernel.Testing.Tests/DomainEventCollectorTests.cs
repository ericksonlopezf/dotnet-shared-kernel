// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.SharedKernel.Testing.Tests;

public sealed class DomainEventCollectorTests
{
    private sealed record OrderCreatedEvent(Guid OrderId, decimal Amount) : DomainEvent;

    private sealed record OrderCancelledEvent(Guid OrderId, string Reason) : DomainEvent;

    private sealed record OrderShippedEvent(Guid OrderId) : DomainEvent;

    private sealed class OrderAggregate : AggregateRoot<Guid>
    {
        public OrderAggregate(Guid id) : base(id) { }

        public void Create(decimal amount)
        {
            RaiseDomainEvent(new OrderCreatedEvent(Id, amount));
        }

        public void Cancel(string reason)
        {
            RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
        }

        public void Ship()
        {
            RaiseDomainEvent(new OrderShippedEvent(Id));
        }
    }

    #region AggregateRootTestExtensions Tests

    [Fact]
    public void CollectEvents_WithNullAggregate_ThrowsArgumentNullException()
    {
        OrderAggregate nullAggregate = null!;

        var act = () => nullAggregate.CollectEvents();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("aggregate");
    }

    [Fact]
    public void CollectEvents_ExtensionMethod_PopulatesCollectorAndDrainsEvents()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId);
        order.Create(50.0m);
        order.Cancel("Out of stock");

        var collector = order.CollectEvents();

        collector.CollectedEvents.Should().HaveCount(2);
        collector.OfType<OrderCreatedEvent>().Should().HaveCount(1);
        collector.OfType<OrderCancelledEvent>().Should().HaveCount(1);
        order.DrainDomainEvents().Should().BeEmpty();
    }

    #endregion

    #region DomainEventCollector Core Tests

    [Fact]
    public void CollectFrom_WithNullAggregate_ThrowsArgumentNullException()
    {
        var collector = new DomainEventCollector();

        var act = () => collector.CollectFrom<Guid>(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("aggregate");
    }

    [Fact]
    public void CollectFrom_SupportsMethodChainingAndAccumulation()
    {
        var order1 = new OrderAggregate(Guid.NewGuid());
        order1.Create(100m);

        var order2 = new OrderAggregate(Guid.NewGuid());
        order2.Ship();

        var collector = new DomainEventCollector();
        var returned = collector.CollectFrom(order1).CollectFrom(order2);

        returned.Should().BeSameAs(collector);
        collector.CollectedEvents.Should().HaveCount(2);
        collector.OfType<OrderCreatedEvent>().Should().HaveCount(1);
        collector.OfType<OrderShippedEvent>().Should().HaveCount(1);
    }

    [Fact]
    public void OfType_WhenNoEventsMatch_ReturnsEmptyEnumerable()
    {
        var order = new OrderAggregate(Guid.NewGuid());
        order.Create(10m);

        var collector = order.CollectEvents();

        collector.OfType<OrderShippedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void ExpectEvent_WithoutPredicate_ReturnsFirstEmittedMatchingEvent()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId);
        order.Create(100m);
        order.Create(200m);

        var collector = order.CollectEvents();

        var created = collector.ExpectEvent<OrderCreatedEvent>();
        created.OrderId.Should().Be(orderId);
        created.Amount.Should().Be(100m);
    }

    [Fact]
    public void ExpectEvent_WithoutPredicate_WhenNoEventsMatch_ThrowsInvalidOperationException()
    {
        var order = new OrderAggregate(Guid.NewGuid());
        order.Create(200m);

        var collector = order.CollectEvents();

        var act = () => collector.ExpectEvent<OrderShippedEvent>();
        var ex = act.Should().Throw<InvalidOperationException>().Which;

        ex.Message.Should().Be("Expected domain event of type 'OrderShippedEvent', but none was recorded matching the criteria.");
    }

    [Fact]
    public void ExpectEvent_WithPredicate_WhenMultipleEventsExist_FindsSpecificMatchingEventNotFirst()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId);
        order.Cancel("First Reason");
        order.Cancel("Second Target Reason");
        order.Cancel("Third Reason");

        var collector = order.CollectEvents();

        var evt = collector.ExpectEvent<OrderCancelledEvent>(e => e.Reason == "Second Target Reason");
        evt.Reason.Should().Be("Second Target Reason");
    }

    [Fact]
    public void ExpectEvent_WithPredicate_WhenTypeMatchesButPredicateFails_ThrowsInvalidOperationException()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId);
        order.Cancel("Customer Request");

        var collector = order.CollectEvents();

        var act = () => collector.ExpectEvent<OrderCancelledEvent>(e => e.Reason == "Fraud suspicion");
        var ex = act.Should().Throw<InvalidOperationException>().Which;

        ex.Message.Should().Be("Expected domain event of type 'OrderCancelledEvent', but none was recorded matching the criteria.");
    }

    [Fact]
    public void Reset_ClearsAllCollectedEvents()
    {
        var order = new OrderAggregate(Guid.NewGuid());
        order.Create(300m);
        order.Cancel("Price mismatch");

        var collector = order.CollectEvents();
        collector.CollectedEvents.Should().HaveCount(2);

        collector.Reset();

        collector.CollectedEvents.Should().BeEmpty();
        collector.OfType<OrderCreatedEvent>().Should().BeEmpty();
    }

    #endregion

    #region Property-Based Testing (FsCheck)

    [Property]
    public Property CollectFrom_PreservesEventCountAndOrder(PositiveInt count)
    {
        var n = Math.Min(count.Get, 50); // Bound generation for test speed
        var order = new OrderAggregate(Guid.NewGuid());

        for (var i = 0; i < n; i++)
        {
            order.Create(i * 10m);
        }

        var collector = order.CollectEvents();

        var countMatches = collector.CollectedEvents.Count == n;
        var orderMatches = collector.OfType<OrderCreatedEvent>()
            .Select((e, idx) => e.Amount == idx * 10m)
            .All(x => x);

        return (countMatches && orderMatches).ToProperty();
    }

    #endregion
}

