// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.NativeAotTests.Types;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591 // Missing XML comment — test harness

Console.WriteLine("=================================================");
Console.WriteLine(" EricksonLopez.SharedKernel NativeAOT Test Suite ");
Console.WriteLine("=================================================");

int passedTests = 0;

void Assert([DoesNotReturnIf(false)] bool condition, string testName)
{
    if (!condition)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] {testName}");
        Console.ResetColor();
        throw new InvalidOperationException($"Assertion failed for: {testName}");
    }

    passedTests++;
    Console.WriteLine($"[PASS] {testName}");
}

// ── 1. Entity<TId> Tests ────────────────────────────────────────────────────
Console.WriteLine("\n--- Running Entity<TId> Tests ---");

var entityId1 = Guid.NewGuid();
var entityId2 = Guid.NewGuid();
var entityA1 = new OrderEntity(entityId1, "Item A");
var entityA2 = new OrderEntity(entityId1, "Item A Modified");
var entityB = new OrderEntity(entityId2, "Item B");
var differentTypeEntity = new CustomerEntity(entityId1, "Customer 1");

Assert(entityA1.Id == entityId1, "Entity.Id returns assigned identifier");
Assert(entityA1.Equals(entityA2), "Entity.Equals(other) returns true for same ID and same Type");
Assert(entityA1.Equals((object)entityA2), "Entity.Equals(object) returns true for same ID and same Type");
Assert(entityA1 == entityA2, "operator == returns true for same ID and same Type");
Assert(!(entityA1 != entityA2), "operator != returns false for same ID and same Type");
Assert(entityA1.GetHashCode() == entityA2.GetHashCode(), "Entity.GetHashCode() matches for same ID and same Type");

Assert(!entityA1.Equals(entityB), "Entity.Equals returns false for different ID");
Assert(entityA1 != entityB, "operator != returns true for different ID");
Assert(!entityA1.Equals(differentTypeEntity), "Entity.Equals returns false for different concrete Type with same ID");
Assert(!entityA1.Equals(null), "Entity.Equals(null) returns false");
Assert(!entityA1.Equals(new object()), "Entity.Equals(nonEntityObject) returns false");

OrderEntity? nullEntity1 = null;
OrderEntity? nullEntity2 = null;
Assert(nullEntity1 == nullEntity2, "operator == returns true for two null entities");
Assert(entityA1 != nullEntity1, "operator != returns true for entity and null");

bool defaultIdThrows = false;
try
{
    _ = new OrderEntity(Guid.Empty, "Invalid");
}
catch (ArgumentException)
{
    defaultIdThrows = true;
}
Assert(defaultIdThrows, "Entity constructor throws ArgumentException on default/empty identity");

IEntity<Guid> ientity = entityA1;
Assert(ientity.Id == entityId1, "IEntity<TId>.Id is accessible via interface");

// ── 2. AggregateRoot<TId> & DomainEvent Tests ───────────────────────────────
Console.WriteLine("\n--- Running AggregateRoot<TId> & DomainEvent Tests ---");

var aggId = Guid.NewGuid();
var emptyAggregate = OrderAggregate.Hydrate(aggId, "Empty Order");

var drainedEmpty = emptyAggregate.DrainDomainEvents();
Assert(drainedEmpty != null, "AggregateRoot.DrainDomainEvents is never null");
Assert(drainedEmpty.Count == 0, "AggregateRoot.DrainDomainEvents is empty before any event is raised (zero alloc)");

var activeAggregate = OrderAggregate.Create(aggId, "Active Order");
activeAggregate.AddItem("Second Item");

bool nullEventThrows = false;
try
{
    activeAggregate.RaiseInvalidNullEvent();
}
catch (ArgumentNullException)
{
    nullEventThrows = true;
}
Assert(nullEventThrows, "RaiseDomainEvent throws ArgumentNullException when null event is provided");

// Polymorphic access via IHasDomainEvents and IAggregateRoot
IAggregateRoot iAggregateRoot = activeAggregate;
IHasDomainEvents iHasDomainEvents = activeAggregate;
Assert(iAggregateRoot != null, "AggregateRoot implements IAggregateRoot");

var activeEvents = iHasDomainEvents.DrainDomainEvents();
Assert(activeEvents.Count == 2, "IHasDomainEvents.DrainDomainEvents exposes and detaches recorded events");

var firstEvent = activeEvents[0];
Assert(firstEvent is OrderPlacedDomainEvent, "DomainEvent is of correct concrete type");
Assert(!firstEvent.Id.IsEmpty, "DomainEvent.Id is generated and non-empty");
Assert(firstEvent.OccurredAt <= DateTimeOffset.UtcNow, "DomainEvent.OccurredAt timestamp is valid UTC");

var secondDrain = activeAggregate.DrainDomainEvents();
Assert(secondDrain.Count == 0, "Subsequent DrainDomainEvents returns empty collection");

// ── 3. Strongly Typed ID (IStrongId) Tests ─────────────────────────────────
Console.WriteLine("\n--- Running Strongly-Typed ID Tests ---");

var strongGuid = Guid.NewGuid();
var strongOrderId1 = OrderStrongId.Create(strongGuid);
var strongOrderId2 = new OrderStrongId(strongGuid);
var strongOrderId3 = new OrderStrongId(Guid.NewGuid());

Assert(strongOrderId1.Value == strongGuid, "IStrongId.Value retrieves primitive underlying value");
Assert(strongOrderId1 == strongOrderId2, "StrongId structural equality holds for same value");
Assert(strongOrderId1 != strongOrderId3, "StrongId inequality holds for different value");

var strongAggregate = new StronglyTypedOrderAggregate(strongOrderId1);
Assert(strongAggregate.Id.Value == strongGuid, "AggregateRoot<IStrongId> works seamlessly");

// ── 4. ValueObject Tests (Class & Struct) ──────────────────────────────────
Console.WriteLine("\n--- Running ValueObject Tests ---");

// 4a. Reference ValueObject (Class)
var addr1 = new AddressVo("123 Main St", "Metropolis", "10001");
var addr2 = new AddressVo("123 Main St", "Metropolis", "10001");
var addr3 = new AddressVo("456 Other St", "Metropolis", "10001");

Assert(addr1 == addr2, "Reference ValueObject equality holds for matching properties");
Assert(addr1 != addr3, "Reference ValueObject inequality holds for different properties");
Assert(addr1.GetHashCode() == addr2.GetHashCode(), "Reference ValueObject GetHashCode matches for equal instances");

var modifiedAddr = addr1 with { Street = "789 New St" };
Assert(modifiedAddr.Street == "789 New St", "Reference ValueObject with-expression non-destructive mutation works");
Assert(modifiedAddr.City == "Metropolis", "Reference ValueObject with-expression preserves unmodified properties");
Assert(addr1.Street == "123 Main St", "Original Reference ValueObject remains unchanged (immutable)");

// 4b. Struct ValueObject (Stack-allocated readonly record struct)
var money1 = new MoneyVo(100.00m, "USD");
var money2 = new MoneyVo(100.00m, "USD");
var money3 = new MoneyVo(200.00m, "EUR");

Assert(money1 == money2, "Struct ValueObject equality holds for matching fields");
Assert(money1 != money3, "Struct ValueObject inequality holds for different fields");
Assert(money1.GetHashCode() == money2.GetHashCode(), "Struct ValueObject GetHashCode matches for equal instances");

var modifiedMoney = money1 with { Amount = 150.00m };
Assert(modifiedMoney.Amount == 150.00m, "Struct ValueObject with-expression non-destructive mutation works");
Assert(modifiedMoney.Currency == "USD", "Struct ValueObject with-expression preserves unmodified fields");
Assert(money1.Amount == 100.00m, "Original Struct ValueObject remains unchanged (immutable)");

var structVoAttr = typeof(MoneyVo).GetCustomAttribute<ValueObjectAttribute>();
Assert(structVoAttr != null, "ValueObjectAttribute on struct is preserved under NativeAOT trimming");

Console.WriteLine("\n=================================================");
Console.WriteLine($" ALL {passedTests} NATIVE AOT SUITE TESTS PASSED SUCCESSFULLY! ");
Console.WriteLine("=== AOT Validator: OK ===");
Console.WriteLine("=================================================");

namespace EricksonLopez.SharedKernel.NativeAotTests.Types
{

    public sealed record OrderPlacedDomainEvent(Guid OrderId, string Name) : DomainEvent;

    public sealed record OrderItemAddedDomainEvent(string ItemName) : DomainEvent;

    public sealed class OrderEntity : Entity<Guid>
    {
        public string Name { get; }

        public OrderEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    public sealed class CustomerEntity : Entity<Guid>
    {
        public string Name { get; }

        public CustomerEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    public sealed class OrderAggregate : AggregateRoot<Guid>
    {
        public string Name { get; private set; }

        private OrderAggregate(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public static OrderAggregate Create(Guid id, string name)
        {
            var agg = new OrderAggregate(id, name);
            agg.RaiseDomainEvent(new OrderPlacedDomainEvent(id, name));
            return agg;
        }

        public static OrderAggregate Hydrate(Guid id, string name)
        {
            return new OrderAggregate(id, name);
        }

        public void AddItem(string itemName)
        {
            RaiseDomainEvent(new OrderItemAddedDomainEvent(itemName));
        }

        public void RaiseInvalidNullEvent()
        {
            RaiseDomainEvent(null!);
        }
    }

    public readonly record struct OrderStrongId(Guid Value) : IStrongId<OrderStrongId, Guid>
    {
        public static string PrimitiveName => nameof(OrderStrongId);
        public bool IsDefault => Value == Guid.Empty;
        public static OrderStrongId Empty => new(Guid.Empty);
        public static OrderStrongId Create() => new(Guid.NewGuid());
        public static OrderStrongId Create(Guid value) => new(value);
        public static bool TryCreate(Guid value, out OrderStrongId result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    public sealed class StronglyTypedOrderAggregate : AggregateRoot<OrderStrongId>
    {
        public StronglyTypedOrderAggregate(OrderStrongId id) : base(id) { }
    }

    public sealed record AddressVo(string Street, string City, string ZipCode) : ValueObject;

    [ValueObject]
    public readonly record struct MoneyVo(decimal Amount, string Currency);
}


