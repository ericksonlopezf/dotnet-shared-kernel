namespace EricksonLopez.SharedKernel.UnitTests.Domain;

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
file sealed record OrderShippedEvent(Guid OrderId) : IDomainEvent;
file sealed record OrderCancelledEvent(Guid OrderId) : IDomainEvent;

file sealed class Order : AggregateRoot<Guid>
{
    public string Description { get; private set; } = string.Empty;

    public static Order Create(Guid id, string description)
    {
        var order = new Order { Id = id, Description = description };
        order.RaiseDomainEvent(new OrderCreatedEvent(id));
        return order;
    }

    public static Order CreateEmpty(Guid id)
        => new() { Id = id };

    public void Ship() => RaiseDomainEvent(new OrderShippedEvent(Id));
    public void Cancel() => RaiseDomainEvent(new OrderCancelledEvent(Id));
    public void RaiseNullEvent() => RaiseDomainEvent(null!);
}

file sealed class NullComponentMoney(decimal amount, string? currency) : ValueObject
{
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return amount;
        yield return currency;
    }
}

file sealed class LineItem : Entity<Guid>
{
    public string ProductName { get; private set; } = string.Empty;

    public static LineItem Create(Guid id, string productName)
        => new() { Id = id, ProductName = productName };
}

// ─── ValueObject tests ────────────────────────────────────────────────────────

public sealed class ValueObjectTests
{
    [Fact]
    public void TwoValueObjects_WithSameComponents_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);
        var money2 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithDifferentComponents_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);
        var money2 = new Money(TestValues.Domain.AlternativeAmount, TestValues.Domain.Currency);

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithDifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);
        var money2 = new Money(TestValues.Domain.Amount, TestValues.Domain.AlternativeCurrency);

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void SameReference_ShouldBeEqual()
    {
        // Arrange
        var money = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        money.Equals(money).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (money == money).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Fact]
    public void ValueObject_EqualToNull_ShouldBeFalse()
    {
        // Arrange
        var money = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        money.Equals(null).Should().BeFalse();
        (money == null).Should().BeFalse();
    }

    [Fact]
    public void EqualValueObjects_ShouldHaveSameHashCode()
    {
        // Arrange
        var money1 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);
        var money2 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Fact]
    public void ValueObject_EqualsDifferentType_ShouldBeFalse()
    {
        // Arrange
        var money = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        money.Equals(new object()).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_EqualityOperator_Nulls_ShouldBeHandled()
    {
        // Arrange
        Money? m1 = null;
        Money? m2 = null;
        var m3 = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        (m1 == m2).Should().BeTrue();
        (m1 == m3).Should().BeFalse();
        (m3 == m1).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_WithNullComponent_EqualityIsSymmetric()
    {
        // Arrange
        var a = new NullComponentMoney(TestValues.Domain.Amount, null);
        var b = new NullComponentMoney(TestValues.Domain.Amount, null);
        var c = new NullComponentMoney(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act & Assert
        a.Should().Be(b, "two value objects with identical null components must be equal");
        a.Should().NotBe(c, "null currency and non-null currency produce different components");
        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
    }

    [Fact]
    public void ValueObject_GetHashCode_IsStableWithinSession()
    {
        // Arrange
        var money = new Money(TestValues.Domain.Amount, TestValues.Domain.Currency);

        // Act
        var hash1 = money.GetHashCode();
        var hash2 = money.GetHashCode();
        var hash3 = money.GetHashCode();

        // Assert
        hash1.Should().Be(hash2).And.Be(hash3);
    }
}

// ─── Entity tests ─────────────────────────────────────────────────────────────

public sealed class EntityTests
{
    [Fact]
    public void TwoEntities_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var item1 = LineItem.Create(id, TestValues.Domain.ProductName);
        var item2 = LineItem.Create(id, TestValues.Domain.AlternativeProductName);

        // Act & Assert
        item1.Should().Be(item2);
        (item1 == item2).Should().BeTrue();
    }

    [Fact]
    public void TwoEntities_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var item1 = LineItem.Create(Guid.NewGuid(), TestValues.Domain.ProductName);
        var item2 = LineItem.Create(Guid.NewGuid(), TestValues.Domain.ProductName);

        // Act & Assert
        item1.Should().NotBe(item2);
        (item1 != item2).Should().BeTrue();
    }

    [Fact]
    public void Entity_EqualsNull_ShouldBeFalse()
    {
        // Arrange
        var item = LineItem.Create(Guid.NewGuid(), TestValues.Domain.ProductName);

        // Act & Assert
        item.Equals(null).Should().BeFalse();
        (item == null).Should().BeFalse();
        (null == item).Should().BeFalse();
        (item != null).Should().BeTrue();
        (null != item).Should().BeTrue();
    }

    [Fact]
    public void Entity_EqualsDifferentType_ShouldBeFalse()
    {
        // Arrange
        var item = LineItem.Create(Guid.NewGuid(), TestValues.Domain.ProductName);

        // Act & Assert
        item.Equals(new object()).Should().BeFalse();
    }

    [Fact]
    public void Entity_SameReference_ShouldBeEqual()
    {
        // Arrange
        var item = LineItem.Create(Guid.NewGuid(), TestValues.Domain.ProductName);

        // Act & Assert
        item.Equals(item).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (item == item).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Fact]
    public void Entity_GetHashCode_ShouldMatchIdHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var item = LineItem.Create(id, TestValues.Domain.ProductName);

        // Act & Assert
        item.GetHashCode().Should().Be(HashCode.Combine(item.GetType(), id));
    }
}

// ─── AggregateRoot tests ──────────────────────────────────────────────────────

public sealed class AggregateRootTests
{
    [Fact]
    public void CreatingAggregate_ShouldRaiseDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var order = Order.Create(id, TestValues.Domain.OrderDescription);

        // Assert
        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents[0].Should().BeOfType<OrderCreatedEvent>()
            .Which.OrderId.Should().Be(id);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);

        // Act
        order.ClearDomainEvents();

        // Assert
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order1 = Order.Create(id, TestValues.Domain.OrderDescription);
        var order2 = Order.Create(id, TestValues.Domain.AlternativeOrderDescription);

        // Act & Assert
        order1.Should().Be(order2);
        (order1 == order2).Should().BeTrue();
    }

    [Fact]
    public void TwoAggregates_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);
        var order2 = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);

        // Act & Assert
        order1.Should().NotBe(order2);
    }

    [Fact]
    public void AggregateRoot_RaiseNullDomainEvent_ShouldThrow()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);

        // Act
        var action = () => order.RaiseNullEvent();

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly_CannotBeCastToMutableList()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);

        // Act
        var domainEventsList = order.DomainEvents as List<IDomainEvent>;
        
        // Assert
        domainEventsList.Should().BeNull("DomainEvents should be safely wrapped in a ReadOnlyCollection, preventing mutation.");
    }
    
    [Fact]
    public void DomainEvents_WhenCastToIListAndMutated_ShouldThrowNotSupportedException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);
        var ilist = (IList<IDomainEvent>)order.DomainEvents;
        
        // Act
        Action act = () => ilist.Add(new OrderCreatedEvent(Guid.NewGuid()));

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void AggregateRoot_InitialState_ShouldHaveNoDomainEvents()
    {
        // Arrange & Act
        var order = Order.CreateEmpty(Guid.NewGuid());

        // Assert
        order.DomainEvents.Should().BeEmpty(
            "a freshly created aggregate must start with an empty event list");
    }

    [Fact]
    public void AggregateRoot_MultipleEvents_ShouldPreserveRaiseOrder()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order = Order.Create(id, TestValues.Domain.OrderDescription);
        
        // Act
        order.Ship();
        order.Cancel();

        // Assert
        var events = order.DomainEvents;
        events.Should().HaveCount(3);
        events[0].Should().BeOfType<OrderCreatedEvent>();
        events[1].Should().BeOfType<OrderShippedEvent>();
        events[2].Should().BeOfType<OrderCancelledEvent>();
    }

    [Fact]
    public void AggregateRoot_ClearThenRaise_ShouldAccumulateNewEventsOnly()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);

        // Act
        order.ClearDomainEvents();
        order.Ship();

        // Assert
        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents[0].Should().BeOfType<OrderShippedEvent>();
    }

    [Fact]
    public void AggregateRoot_ConcurrentRaiseDomainEvent_ShouldNotLoseEvents()
    {
        // Arrange
        var aggregate = Order.CreateEmpty(Guid.NewGuid());
        const int expectedEventCount = 10_000;

        // Act
        Parallel.For(0, expectedEventCount, _ =>
        {
            aggregate.Ship();
        });

        // Assert
        aggregate.DomainEvents.Should().HaveCount(expectedEventCount,
            "all concurrently raised events must be safely recorded");
    }

    [Fact]
    public void AggregateRoot_ConcurrentClearAndRaise_ShouldNotThrow()
    {
        // Arrange
        var aggregate = Order.CreateEmpty(Guid.NewGuid());
        const int expectedEventCount = 10_000;

        // Act
        var act = () =>
        {
            Parallel.For(0, expectedEventCount, i =>
            {
                aggregate.Ship();
                if (i % 10 == 0)
                {
                    aggregate.ClearDomainEvents();
                }
            });
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AggregateRoot_ClearEvents_WhenAlreadyEmpty_ShouldBeIdempotent()
    {
        // Arrange
        var order = Order.CreateEmpty(Guid.NewGuid());

        // Act — clearing an already-empty aggregate must not throw
        var act = () =>
        {
            order.ClearDomainEvents();
            order.ClearDomainEvents();
        };

        // Assert
        act.Should().NotThrow();
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_DomainEvents_ReturnedArray_IsSnapshot_NotLiveReference()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), TestValues.Domain.OrderDescription);
        var snapshot = order.DomainEvents; // take the snapshot

        // Act — raise a new event after taking the snapshot
        order.Ship();

        // Assert — the previously captured snapshot must not have grown
        snapshot.Should().HaveCount(1,
            "DomainEvents returns a copy (ToArray), not a live reference to the internal list");
        order.DomainEvents.Should().HaveCount(2);
    }
}

// ─── Additional Entity tests ──────────────────────────────────────────────────

public sealed class EntityTypeDiscriminationTests
{
    // Test doubles: two different Entity types that happen to share the same Id type
    private sealed class Order2 : Entity<Guid>
    {
        public static Order2 Create(Guid id) => new() { Id = id };
    }

    private sealed class Customer : Entity<Guid>
    {
        public static Customer Create(Guid id) => new() { Id = id };
    }

    [Fact]
    public void Entity_SameId_DifferentConcreteTypes_ShouldNotBeEqual()
    {
        // Arrange — shared Id between two unrelated entity types
        var id = Guid.NewGuid();
        var order = Order2.Create(id);
        var customer = Customer.Create(id);

        // Act & Assert
        order.Equals(customer).Should().BeFalse(
            "equality requires same GetType() — different domain concepts are never equal");
        (order == (Entity<Guid>)customer).Should().BeFalse();
    }

    [Fact]
    public void Entity_GetHashCode_DifferentTypes_SameId_ProduceDifferentHashes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order = Order2.Create(id);
        var customer = Customer.Create(id);

        // Act & Assert
        order.GetHashCode().Should().NotBe(customer.GetHashCode(),
            "GetType() participates in the hash — different types must not collide");
    }
}

