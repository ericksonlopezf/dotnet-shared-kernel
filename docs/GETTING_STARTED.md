# Getting Started — EricksonLopez.SharedKernel

Welcome to **EricksonLopez.SharedKernel** — the DDD Shared Kernel for .NET.

This guide takes you from zero to a working domain model with Entity, AggregateRoot, and IDomainEvent.

---

## What you will learn

1. What the library provides and what it intentionally does NOT provide
2. How to model domain entities with `Entity<TId>`
3. How to model aggregate roots with `AggregateRoot<TId>`
4. How to define domain events with `IDomainEvent`
5. How to integrate the result with the infrastructure layer

---

## Step 1 — Installation

```bash
dotnet add package EricksonLopez.SharedKernel
```

| TFM | Supported |
|---|---|
| net8.0 | ✅ |
| net9.0 | ✅ |
| net10.0 | ✅ |

The library is **zero-dependency**, **NativeAOT-compatible**, and **Trimming-compatible**.

---

## Step 2 — Understand the mental model

```
Your Domain Layer
│
├── AggregateRoot<TId>  ← transactional consistency boundary
│     └── Entity<TId>  ← immutable identity by Id + concrete type
│           └── IDomainEvent ← something that happened in the domain
│
└── (no external dependencies)
```

**Golden rules:**
- Only `AggregateRoot<TId>` can raise `IDomainEvent`
- Child `Entity<TId>` objects delegate to the AggregateRoot if they need to emit events
- The library does **NOT** include UnitOfWork, Repositories, or Publisher — those belong to Infrastructure

---

## Step 3 — Your first entity

```csharp
using EricksonLopez.SharedKernel;

public sealed class OrderLine : Entity<Guid>
{
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }

    public OrderLine(Guid id, string productName, decimal unitPrice, int quantity)
    {
        Id = id;                    // Id is protected init — assign here
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal TotalPrice => UnitPrice * Quantity;
}
```

**Why `sealed`?** In DDD, entities rarely need implementation inheritance. `sealed` also improves JIT compiler performance.

---

## Step 4 — Your first domain event

```csharp
using EricksonLopez.SharedKernel;

// Domain events MUST be records (immutable by design)
// Do not include infrastructure metadata (timestamp, correlationId) here
public sealed record OrderPlacedEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount
) : IDomainEvent;
```

**Why `record`?** Events are facts from the past — they don't change. `record` in C# is immutable by default and has value equality, which is correct for events.

---

## Step 5 — Your first Aggregate Root

```csharp
using EricksonLopez.SharedKernel;
using System;
using System.Collections.Generic;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];

    public string CustomerEmail { get; private set; }
    public decimal Total => _lines.Sum(l => l.TotalPrice);
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { } // For EF Core / hydration

    // Factory method — guarantees valid state at creation
    public static Order Place(Guid id, string customerEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);

        var order = new Order
        {
            Id = id,
            CustomerEmail = customerEmail
        };

        // Only the AggregateRoot can raise events
        order.RaiseDomainEvent(new OrderPlacedEvent(id, customerEmail, 0m));

        return order;
    }

    public void AddLine(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        _lines.Add(line);
    }
}
```

---

## Step 6 — Integration with Infrastructure

The library provides the contract. Infrastructure does the dispatching:

```csharp
// Conceptual example of a UnitOfWork / DbContext interceptor

public async Task SaveChangesAsync(Order order)
{
    // 1. Persist state
    await _dbContext.SaveChangesAsync();

    // 2. Get events BEFORE clearing
    var events = order.DomainEvents.ToList();

    // 3. Clear to avoid re-dispatch on the next SaveChanges
    order.ClearDomainEvents();

    // 4. Publish via your preferred mechanism (MediatR, Outbox, etc.)
    foreach (var domainEvent in events)
    {
        // await _publisher.Publish(domainEvent);
        Console.WriteLine($"Publishing: {domainEvent.GetType().Name}");
    }
}
```

**Critical order:** Clearing BEFORE publishing protects against re-dispatch if publishing fails mid-way. The Outbox Pattern resolves this with guaranteed delivery.

---

## Step 7 — Strongly Typed Ids (recommended pattern)

```csharp
// Avoid mixing Guids across different entity types
public readonly record struct OrderId(Guid Value);
public readonly record struct CustomerId(Guid Value);

public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }

    public Order(OrderId id, CustomerId customerId)
    {
        Id = id;
        CustomerId = customerId;
    }
}

// The compiler detects if you mix OrderId with CustomerId
var order = new Order(new OrderId(Guid.NewGuid()), new CustomerId(Guid.NewGuid()));
```

---

## Next Steps

| Resource | Description |
|---|---|
| [API Reference](API_REFERENCE.md) | Technical reference for all public members |
| [Cookbook](Cookbook.md) | Recipes by scenario (equality, hydration, multi-event...) |
| [Best Practices](BestPractices.md) | Recommended DDD patterns |
| [Anti-Patterns](AntiPatterns.md) | Common mistakes to avoid |
| [Architecture Guide](Architecture.md) | Diagrams and main flow |
| [Migration Guide](MigrationGuide.md) | Migrating from other libraries or implementations |
| [Showcase](../samples/EricksonLopez.SharedKernel.Sample/Program.cs) | Runnable code with all use-case levels |
