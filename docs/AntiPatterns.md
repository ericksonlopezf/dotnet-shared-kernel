# Anti-Patterns — EricksonLopez.SharedKernel

Common mistakes and anti-patterns to avoid when using the library.

> [!IMPORTANT]
> This guide applies to the v2.0 API: `Entity<TId>`, `AggregateRoot<TId>`, `IDomainEvent`.

---

## 1. Raising Domain Events from an Entity that is not an AggregateRoot

```csharp
// ❌ Anti-pattern — Entity<TId> does not have RaiseDomainEvent
public sealed class OrderLine : Entity<Guid>
{
    public void ChangeQuantity(int quantity)
    {
        Quantity = quantity;
        // Cannot call RaiseDomainEvent here — it is not available
        // Attempting this is a design error
    }
}
```

**Solution:** Delegate to the parent AggregateRoot:

```csharp
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];

    public void ChangeLineQuantity(Guid lineId, int quantity)
    {
        var line = _lines.First(l => l.Id == lineId);
        line.ChangeQuantity(quantity); // entity mutates its state internally
        RaiseDomainEvent(new OrderLineQuantityChangedEvent(Id, lineId, quantity)); // ✅
    }
}
```

---

## 2. Using `class` instead of `record` for Domain Events

```csharp
// ❌ Anti-pattern — class is mutable
public class OrderPlacedEvent : IDomainEvent
{
    public Guid OrderId { get; set; }        // Can be mutated after creation
    public DateTime PlacedAt { get; set; }  // Dangerous in event-driven systems
}

// ✅ Correct — record is immutable by default
public sealed record OrderPlacedEvent(Guid OrderId, DateTime PlacedAt) : IDomainEvent;
```

**Why it matters:** Domain events represent facts from the past — they must be immutable.

---

## 3. Comparing entities by attributes instead of by Id

```csharp
// ❌ Anti-pattern — comparison by attribute value
bool isSameCustomer = customer1.Email == customer2.Email;

// ✅ Correct — comparison by domain identity
bool isSameCustomer = customer1 == customer2; // uses Id + concrete type
```

**Why it matters:** In DDD, identity defines the entity, not its attributes. A customer who changes their email is still the same customer.

---

## 4. Omitting `ClearDomainEvents()` after dispatch

```csharp
// ❌ Anti-pattern — events are never cleared
public async Task SaveChanges(Order order)
{
    await _db.SaveChangesAsync();
    foreach (var ev in order.DomainEvents)
        await _publisher.Publish(ev);
    // We forgot ClearDomainEvents() → the next SaveChanges
    // will dispatch the same events again
}

// ✅ Correct
public async Task SaveChanges(Order order)
{
    await _db.SaveChangesAsync();
    var events = order.DomainEvents.ToList();
    order.ClearDomainEvents();              // clear before publishing
    foreach (var ev in events)
        await _publisher.Publish(ev);
}
```

---

## 5. Using `AggregateRoot<TId>` for DTOs or View Models

```csharp
// ❌ Anti-pattern — a DTO is not a domain entity
public sealed class OrderSummaryDto : AggregateRoot<Guid>
{
    public string Title { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

// ✅ Correct — DTOs are POCOs or records
public sealed record OrderSummaryDto(Guid Id, string Title, decimal Total);
```

**Why it matters:** `AggregateRoot<TId>` imposes domain semantics (identity, domain events). A DTO neither needs nor should have those semantics.

---

## 6. Instantiating an AggregateRoot with the constructor and forgetting the event

```csharp
// ❌ Anti-pattern — constructor omits the domain event
var order = new Order { Id = Guid.NewGuid(), Description = "My order" };
// OrderPlacedEvent was never raised — the system doesn't know this was created

// ✅ Correct — use the factory method that guarantees the event
var order = Order.Place(Guid.NewGuid(), "My order");
// OrderPlacedEvent is in DomainEvents ✓
```

---

## 7. Sharing an AggregateRoot instance across threads without synchronization

```csharp
// ❌ Anti-pattern — concurrent access without synchronization
Parallel.ForEach(items, item =>
{
    order.AddItem(item);  // RaiseDomainEvent internally → race condition
});

// ✅ Correct — the AggregateRoot is single-threaded by design
// The command handler must guarantee exclusive access
foreach (var item in items)
    order.AddItem(item);
```

**Why it matters:** `AggregateRoot<TId>` is deliberately NOT thread-safe (ADR-011). The transactional consistency boundary guarantees exclusivity at the command level.

---

## 8. Ignoring `IsTransient()` when persisting

```csharp
// ❌ Anti-pattern — persisting an entity without an Id
var order = new Order(); // Id is Guid.Empty (transient)
await _repo.Save(order); // ← will save with Id = Guid.Empty in the DB

// ✅ Correct — check before persisting
var order = Order.Place(Guid.NewGuid(), "Description");
if (order.IsTransient())
    throw new InvalidOperationException("Cannot persist a transient aggregate.");
await _repo.Save(order);
```

---

## 9. Adding infrastructure metadata to the Domain Event

```csharp
// ❌ Anti-pattern — infrastructure metadata in the domain event
public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CorrelationId,    // ← infrastructure
    string TraceId,        // ← infrastructure
    DateTime OccurredAt,   // ← infrastructure (debatable)
    int RetryCount         // ← infrastructure
) : IDomainEvent;

// ✅ Correct — only what the domain needs to express
public sealed record OrderPlacedEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount
) : IDomainEvent;
// Infrastructure metadata goes in the envelope (Outbox, MassTransit, etc.)
```

---

## 10. Inheriting from another concrete class that already inherits from Entity

```csharp
// ❌ Anti-pattern — inheritance between concrete entities
public sealed class Order : AggregateRoot<Guid> { ... }
public sealed class SpecialOrder : Order { ... } // ← implementation inheritance

// Problem: SpecialOrder.GetType() != Order.GetType()
// → An Order and a SpecialOrder with the same Id are NOT equal
// → Violates DDD equality semantics

// ✅ Correct — use composition or different modeling
public sealed class Order : AggregateRoot<Guid>
{
    public OrderType Type { get; private set; } // composition by value
}

public enum OrderType { Standard, Special, Express }
```
