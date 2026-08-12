# Best Practices — EricksonLopez.SharedKernel

Recommended practices for using the library correctly in DDD projects.

> [!IMPORTANT]
> This guide applies to the v2.0 API which contains: `Entity<TId>`, `AggregateRoot<TId>`, `IDomainEvent`.
> The types `Result<T>`, `ValueObject`, `Error`, and `Specification<T>` are not part of this library.

---

## 1. Use `sealed` on concrete entities and aggregates

```csharp
// ✅ Recommended
public sealed class Order : AggregateRoot<Guid> { ... }
public sealed class OrderLine : Entity<Guid> { ... }
```

**Why:** `sealed` prevents accidental inheritance, improves JIT performance, and communicates design intent. In DDD, implementation inheritance between entities usually indicates an incorrect design.

---

## 2. Use factory methods on AggregateRoots

```csharp
// ✅ Recommended
public sealed class Order : AggregateRoot<Guid>
{
    private Order() { }  // Private constructor — for EF Core / hydration

    public static Order Place(Guid id, string customerEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);

        var order = new Order { Id = id, CustomerEmail = customerEmail };
        order.RaiseDomainEvent(new OrderPlacedEvent(id, customerEmail));
        return order;
    }
}

// ❌ Not recommended — public constructor that does not raise events
public Order(Guid id, string email)
{
    Id = id;
    CustomerEmail = email;
    // Is the event raised? Always? Under what conditions?
}
```

**Why:** The factory method guarantees the aggregate is born in a valid state, invariants are verified, and events are raised consistently.

---

## 3. Raise Domain Events ONLY from AggregateRoot

```csharp
// ✅ Correct — only the AggregateRoot raises events
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];

    public void AddLine(OrderLine line)
    {
        _lines.Add(line);
        RaiseDomainEvent(new OrderLineAddedEvent(Id, line.Id)); // ✅
    }
}

// ❌ Incorrect — a child Entity cannot raise events
public sealed class OrderLine : Entity<Guid>
{
    // No access to RaiseDomainEvent — correct by design
}
```

**Why:** The AggregateRoot is the transactional consistency boundary. Events represent changes the aggregate guaranteed atomically.

---

## 4. Use records for Domain Events

```csharp
// ✅ Recommended — record is immutable and has value equality
public sealed record OrderPlacedEvent(Guid OrderId, string CustomerEmail) : IDomainEvent;

// ❌ Not recommended — class is mutable by default
public sealed class OrderPlacedEvent : IDomainEvent
{
    public Guid OrderId { get; set; }  // Can be mutated after creation
}
```

**Why:** Domain events represent facts from the past. They are immutable by nature. `record` in C# expresses this directly.

---

## 5. Use Strongly Typed Ids to avoid Primitive Obsession

```csharp
// ✅ Recommended
public readonly record struct OrderId(Guid Value);
public readonly record struct CustomerId(Guid Value);

public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }
}

// The compiler prevents mixing Ids of different types:
// Order.Place(customerId, orderId); // ← Compile error
```

**Why:** Prevents runtime errors from incorrectly passed parameters (e.g. passing a `CustomerId` where an `OrderId` is expected).

---

## 6. Clear Domain Events in the Infrastructure layer

```csharp
// ✅ Correct pattern in UnitOfWork
public async Task SaveChangesAsync()
{
    await _dbContext.SaveChangesAsync(); // 1. Persist first

    var aggregates = _dbContext.ChangeTracker
        .Entries<AggregateRoot<Guid>>()  // Get all aggregates
        .Select(e => e.Entity)
        .Where(a => a.DomainEvents.Any());

    foreach (var agg in aggregates)
    {
        var events = agg.DomainEvents.ToList(); // 2. Snapshot
        agg.ClearDomainEvents();                // 3. Clear
        foreach (var ev in events)
            await _publisher.Publish(ev);       // 4. Publish
    }
}
```

---

## 7. Do not type-check concrete types in Domain Event handlers

```csharp
// ❌ Incorrect — coupling to concrete types
void Handle(IDomainEvent ev)
{
    if (ev is OrderPlacedEvent placed)
        ProcessOrderPlaced(placed);
    else if (ev is OrderCancelledEvent cancelled)
        ProcessOrderCancelled(cancelled);
}

// ✅ Correct — use the language's type system / MediatR / etc.
void Handle(OrderPlacedEvent ev) => ProcessOrderPlaced(ev);
void Handle(OrderCancelledEvent ev) => ProcessOrderCancelled(ev);
```

---

## 8. Keep Domain Events in the same namespace as the Aggregate

```
MyDomain/
├── Orders/
│   ├── Order.cs                    ← AggregateRoot<OrderId>
│   ├── OrderLine.cs                ← Entity<OrderLineId>
│   ├── OrderPlacedEvent.cs         ← IDomainEvent
│   └── OrderLineAddedEvent.cs      ← IDomainEvent
```

**Why:** Events are part of the domain vocabulary, not infrastructure. Placing them near the aggregate that raises them communicates their purpose.

---

## 9. Do not use `AggregateRoot<TId>` for paginated collections or DTOs

```csharp
// ❌ Incorrect — query results are not AggregateRoots
public sealed class OrderSummary : AggregateRoot<Guid> { ... }

// ✅ Correct — query results are simple DTOs
public sealed record OrderSummary(Guid Id, string Description, decimal Total);
```

**Why:** CQRS separates the write side (Commands → Aggregates) from the read side (Queries → DTOs). DTOs do not need domain identity or domain events.

---

## 10. Never modify `DomainEvents` directly

The `DomainEvents` collection is `IReadOnlyCollection<IDomainEvent>`. It cannot be modified from outside the aggregate. To remove events, use `ClearDomainEvents()`. To add events, the aggregate itself must call `RaiseDomainEvent` in its business methods.
