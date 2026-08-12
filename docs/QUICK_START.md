# Quick Start — EricksonLopez.SharedKernel

> Quick start guide to use the library in 3 steps.

## 1. Installation

```bash
dotnet add package EricksonLopez.SharedKernel
```

**Supported TFMs:** `net8.0` · `net9.0` · `net10.0`
**NativeAOT:** `IsAotCompatible=true` on all TFMs
**Trimming:** `IsTrimmable=true` on all TFMs
**Dependencies:** none

---

## 2. Minimum Setup

The library **requires no configuration or dependency injection**.
There are no options, builders, or middleware to register. It is plug-and-play.

---

## 3. First Functional Usage

### Create a domain entity

```csharp
using EricksonLopez.SharedKernel;

public sealed class Product : Entity<Guid>
{
    public string Name { get; }

    public Product(Guid id, string name)
    {
        Id = id;       // 'Id' is protected init — assign in the constructor
        Name = name;
    }
}

// Usage:
var product = new Product(Guid.NewGuid(), "Laptop Pro");
Console.WriteLine(product.Id);            // Guid
Console.WriteLine(product.IsTransient()); // false
```

### Create an Aggregate Root with Domain Events

```csharp
using EricksonLopez.SharedKernel;

// 1. Define the domain event
public sealed record OrderPlacedEvent(Guid OrderId) : IDomainEvent;

// 2. Define the Aggregate Root
public sealed class Order : AggregateRoot<Guid>
{
    public string Description { get; private set; }

    public static Order Place(Guid id, string description)
    {
        var order = new Order { Id = id, Description = description };
        order.RaiseDomainEvent(new OrderPlacedEvent(id));
        return order;
    }
}

// 3. Usage
var order = Order.Place(Guid.NewGuid(), "My first order");
Console.WriteLine(order.DomainEvents.Count);  // 1

// 4. Infrastructure: dispatch and clear (Unit of Work)
foreach (var ev in order.DomainEvents)
    Console.WriteLine(ev.GetType().Name);  // "OrderPlacedEvent"

order.ClearDomainEvents();
Console.WriteLine(order.DomainEvents.Count);  // 0
```

---

## Next Steps

- [Getting Started](GETTING_STARTED.md) — complete step-by-step guide
- [Cookbook](Cookbook.md) — recipes by scenario
- [API Reference](API_REFERENCE.md) — complete technical reference
- [Best Practices](BestPractices.md) — recommended patterns
- [Anti-Patterns](AntiPatterns.md) — common mistakes to avoid
