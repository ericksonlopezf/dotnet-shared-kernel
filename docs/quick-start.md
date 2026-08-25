# Quick Start — EricksonLopez.SharedKernel

> Fast track guide to implementing clean DDD domain models with zero dependencies.

---

## 1. Installation

```bash
dotnet add package EricksonLopez.SharedKernel
```

**Supported Target Frameworks:** `net8.0`, `net9.0`, `net10.0`  
**Native AOT & Trimming:** 100% Compatible (`IsAotCompatible=true`, `IsTrimmable=true`)  
**External Dependencies:** 0 (Pure .NET BCL)

---

## 2. Minimal Setup

`EricksonLopez.SharedKernel` requires no container configuration, options registration, or middleware setup. All primitives are pure domain building blocks.

---

## 3. First Functional Domain Model

### Step A: Define a Strongly-Typed ID & Value Object

```csharp
using EricksonLopez.SharedKernel;

// 1. Strongly-Typed ID
public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>;

// 2. Value Object
[ValueObject]
public sealed record Money(decimal Amount, string Currency) : ValueObject;
```

### Step B: Define a Domain Event

```csharp
// 3. Domain Event inheriting DomainEvent (provides UUIDv7 EventId & OccurredOn UTC)
public sealed record OrderPlacedEvent(OrderId OrderId, Money Total) : DomainEvent;
```

### Step C: Define the Aggregate Root

```csharp
// 4. Aggregate Root with transactional boundary
public sealed class Order : AggregateRoot<OrderId>
{
    public Money Total { get; private set; }

    public Order(OrderId id, Money total) : base(id)
    {
        Total = total;
    }

    public static Order Place(OrderId id, Money total)
    {
        var order = new Order(id, total);
        order.RaiseDomainEvent(new OrderPlacedEvent(id, total));
        return order;
    }
}
```

### Step D: Execute and Dispatch

```csharp
// Instantiate Aggregate Root
var orderId = new OrderId(Guid.NewGuid());
var order = Order.Place(orderId, new Money(150.00m, "USD"));

Console.WriteLine($"Order ID: {order.Id.Value}");

// Non-generic polymorphic event collection by Infrastructure.
// DrainDomainEvents() is atomic: snapshots all pending events and resets
// the buffer in one call. There is no separate ClearDomainEvents() or
// public DomainEvents property on IHasDomainEvents.
IHasDomainEvents eventCarrier = order;
var events = eventCarrier.DrainDomainEvents();
foreach (var domainEvent in events)
{
    Console.WriteLine($"Event: {domainEvent.GetType().Name} | UUID: {domainEvent.Id.Value}");
}

Console.WriteLine($"Events after drain: {eventCarrier.DrainDomainEvents().Count}"); // → 0
```

---

## Next Steps

- [Getting Started Guide](getting-started.md) — Comprehensive architectural walkthrough
- [Cookbook](cookbook.md) — Practical DDD recipes for real-world scenarios
- [API Reference](api-reference.md) — Full technical specification
- [Architecture Guide](architecture.md) — Architectural principles and diagrams
- [Performance Guide](performance-guide.md) — Benchmarks and zero-allocation analysis
