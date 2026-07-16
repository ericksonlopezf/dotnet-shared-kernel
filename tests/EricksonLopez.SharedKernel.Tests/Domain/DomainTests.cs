using EricksonLopez.SharedKernel.Domain;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Domain;

// ─── Test doubles ─────────────────────────────────────────────────────────────

file sealed class Money(decimal amount, string currency) : ValueObject
{
    public decimal Amount { get; } = amount;
    public string Currency { get; } = currency;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

file sealed record OrderCreatedEvent(Guid OrderId) : IDomainEvent;

file sealed class Order : Entity<Guid>
{
    public string Description { get; private set; } = string.Empty;

    public static Order Create(Guid id, string description)
    {
        var order = new Order { Id = id, Description = description };
        order.RaiseDomainEvent(new OrderCreatedEvent(id));
        return order;
    }
}

// ─── ValueObject tests ────────────────────────────────────────────────────────

public sealed class ValueObjectTests
{
    [Fact]
    public void TwoValueObjects_WithSameComponents_ShouldBeEqual()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithDifferentComponents_ShouldNotBeEqual()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(200m, "USD");

        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithDifferentCurrency_ShouldNotBeEqual()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        money1.Should().NotBe(money2);
    }

    [Fact]
    public void SameReference_ShouldBeEqual()
    {
        var money = new Money(50m, "GBP");
        money.Equals(money).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_EqualToNull_ShouldBeFalse()
    {
        var money = new Money(100m, "USD");
        money.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualValueObjects_ShouldHaveSameHashCode()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }
}

// ─── Entity tests ─────────────────────────────────────────────────────────────

public sealed class EntityTests
{
    [Fact]
    public void TwoEntities_WithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var order1 = Order.Create(id, "Order A");
        var order2 = Order.Create(id, "Order B");

        order1.Should().Be(order2);
        (order1 == order2).Should().BeTrue();
    }

    [Fact]
    public void TwoEntities_WithDifferentIds_ShouldNotBeEqual()
    {
        var order1 = Order.Create(Guid.NewGuid(), "Order A");
        var order2 = Order.Create(Guid.NewGuid(), "Order A");

        order1.Should().NotBe(order2);
    }

    [Fact]
    public void CreatingEntity_ShouldRaiseDomainEvent()
    {
        var id = Guid.NewGuid();
        var order = Order.Create(id, "Order A");

        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents[0].Should().BeOfType<OrderCreatedEvent>()
            .Which.OrderId.Should().Be(id);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var order = Order.Create(Guid.NewGuid(), "Order A");
        order.ClearDomainEvents();

        order.DomainEvents.Should().BeEmpty();
    }
}
