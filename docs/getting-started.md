# Getting Started — EricksonLopez.SharedKernel

Welcome to **EricksonLopez.SharedKernel** — the foundational DDD Shared Kernel for .NET enterprise architectures.

This guide walks you through building an end-to-end domain model with Entities, Aggregate Roots, Strongly-Typed IDs, Value Objects, and Domain Events.

---

## What You Will Learn

1. The architectural role of Tier 0 foundation libraries
2. How to eliminate Primitive Obsession using `IStrongId<TSelf, TValue>`
3. How to model structural domain concepts with `ValueObject`
4. How to encapsulate state and identity with `Entity<TId>`
5. How to enforce transactional boundaries with `AggregateRoot<TId>`
6. How to raise time-ordered domain events with `DomainEvent`
7. How to implement non-generic polymorphic event collection in Infrastructure using `IHasDomainEvents`

---

## Step 1 — Installation

```bash
dotnet add package EricksonLopez.SharedKernel
```

| Target Framework | Native AOT | Trimming | Dependencies |
|---|---|---|---|
| `net8.0` | ✅ Fully Supported | ✅ `IsTrimmable=true` | Tier-0 Foundation Contracts only |
| `net9.0` | ✅ Fully Supported | ✅ `IsTrimmable=true` | Tier-0 Foundation Contracts only |
| `net10.0` | ✅ Fully Supported | ✅ `IsTrimmable=true` | Tier-0 Foundation Contracts only |

---

## Step 2 — Understanding the Mental Model

```
Your Domain Layer
│
├── AggregateRoot<TId>   ← Transactional Consistency Boundary & Event Recorder
│     ├── Entity<TId>    ← Identity-based Domain Objects (Type + ID Equality)
│     ├── ValueObject    ← Structural Value Objects (Attribute Equality + with-mutations)
│     └── DomainEvent    ← Sequential UUIDv7 Domain Event Facts (UTC OccurredOn)
│
└── IStrongId<TSelf, TValue> ← Strongly-Typed Entity Identifiers (CRTP)
```

**Golden Rules:**
- Only `AggregateRoot<TId>` can call `RaiseDomainEvent` — child entities delegate event emission to their root.
- Identifiers are immutable (set exclusively via constructor) and cannot be `null` or `default(TId)`.
- Value Objects are immutable records with structural equality.
- Domain Events carry time-ordered UUIDv7 `Id` (type `EventId`, on .NET 9+) and UTC `OccurredAt` timestamps. Backward-compat aliases `EventId` and `OccurredOn` are available.

---

## Step 3 — Strongly-Typed IDs & Value Objects

### Strongly-Typed ID

```csharp
using EricksonLopez.SharedKernel;

// Eliminates primitive obsession
public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>;
public readonly record struct CustomerId(Guid Value) : IStrongId<CustomerId, Guid>;
```

### Value Object

```csharp
[ValueObject]
public sealed record Address(string Street, string City, string PostalCode) : ValueObject;

[ValueObject]
public sealed record Money(decimal Amount, string Currency) : ValueObject;
```

---

## Step 4 — Domain Entities & Aggregate Roots

### Child Entity

```csharp
public sealed class OrderLine : Entity<Guid>
{
    public string ProductName { get; }
    public Money UnitPrice { get; }
    public int Quantity { get; }

    public OrderLine(Guid id, string productName, Money unitPrice, int quantity) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentNullException.ThrowIfNull(unitPrice);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal LineTotal => UnitPrice.Amount * Quantity;
}
```

### Domain Event

```csharp
public sealed record OrderPlacedEvent(OrderId OrderId, CustomerId CustomerId, decimal Total) : DomainEvent;
```

### Aggregate Root

```csharp
public sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderLine> _lines = [];

    public CustomerId CustomerId { get; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Order(OrderId id, CustomerId customerId) : base(id)
    {
        CustomerId = customerId;
    }

    public static Order Place(OrderId id, CustomerId customerId)
    {
        var order = new Order(id, customerId);
        order.RaiseDomainEvent(new OrderPlacedEvent(id, customerId, 0m));
        return order;
    }

    public void AddLine(string productName, Money unitPrice, int quantity)
    {
        var line = new OrderLine(Guid.NewGuid(), productName, unitPrice, quantity);
        _lines.Add(line);
    }
}
```

---

## Step 5 — Infrastructure Integration (Unit of Work / Outbox)

The library provides non-generic `IHasDomainEvents` and `IAggregateRoot` contracts to decouple infrastructure from concrete generic entity types:

```csharp
public async Task CommitAndDispatchAsync(IHasDomainEvents aggregate, IOutboxStore outbox)
{
    // DrainDomainEvents() atomically snapshots all pending events and resets the buffer.
    // There is no separate ClearDomainEvents() or public DomainEvents property.
    var events = aggregate.DrainDomainEvents();

    foreach (var domainEvent in events)
    {
        await outbox.AppendAsync(new OutboxMessage
        {
            Id = domainEvent.Id.Value,      // EventId (Guid)
            OccurredAt = domainEvent.OccurredAt,
            EventType = domainEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(domainEvent)
        });
    }
}
```

---

## Next Steps

| Resource | Description |
|---|---|
| [Cookbook](cookbook.md) | Practical DDD recipes for real-world scenarios |
| [API Reference](api-reference.md) | Complete technical documentation |
| [Best Practices](best-practices.md) | Recommended patterns and architectural guidelines |
| [Anti-Patterns](anti-patterns.md) | Common DDD mistakes and architectural anti-patterns |
| [Architecture Guide](architecture.md) | Detailed topology, diagrams, and component lifecycles |
| [Performance Guide](performance-guide.md) | Allocation benchmarks and optimization profiles |
| [Showcase](../samples/EricksonLopez.SharedKernel.Sample/Program.cs) | Runnable reference implementation |
