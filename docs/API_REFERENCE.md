# API Reference — EricksonLopez.SharedKernel

Complete technical reference for the library's public API.

**Namespace:** `EricksonLopez.SharedKernel`
**Assembly:** `EricksonLopez.SharedKernel.dll`
**Targets:** `net8.0`, `net9.0`, `net10.0`

---

## `Entity<TId>`

Abstract base class for domain entities. An entity is defined by its identity, not its attributes.

### Declaration

```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull, IEquatable<TId>
```

### TId Constraint

`TId` must satisfy: `notnull, IEquatable<TId>`.

| Type | Valid | Notes |
|---|---|---|
| `Guid` | ✅ | Most common |
| `int` | ✅ | Legacy databases |
| `long` | ✅ | High-scale systems |
| `string` | ✅ | Alphanumeric IDs |
| `record struct OrderId(Guid Value)` | ✅ | Strongly Typed Id (recommended) |

---

### Property: `Id`

```csharp
public TId Id { get; protected init; }
```

**Description:** Gets the unique identifier of this entity.

**Remarks:**
- It is `protected init` — can only be assigned in the derived class constructor, exactly once.
- Preserves the DDD invariant: entity identity is immutable.
- Default value is `default!` (e.g. `Guid.Empty` for `Entity<Guid>`).

**Example:**
```csharp
public sealed class Product : Entity<Guid>
{
    public Product(Guid id, string name)
    {
        Id = id;   // ✅ valid in the constructor
        Name = name;
    }

    public string Name { get; }
}

var product = new Product(Guid.NewGuid(), "Laptop");
Console.WriteLine(product.Id); // non-empty Guid
```

---

### Method: `IsTransient()`

```csharp
public bool IsTransient()
```

**Description:** Determines whether the entity is transient (has not been assigned a persistent identity).

**Return:** `true` if `Id` equals `default(TId)` (e.g. `Guid.Empty`, `0`, `null`); `false` otherwise.

**When to use:** In guard clauses before persisting, or to distinguish new entities from existing ones.

**Performance:** O(1), uses `EqualityComparer<TId>.Default.Equals` — no boxing for `struct` types implementing `IEquatable<T>`.

**Example:**
```csharp
var newProduct = new Product(Guid.Empty, "No Id");
Console.WriteLine(newProduct.IsTransient()); // true

var existingProduct = new Product(Guid.NewGuid(), "With Id");
Console.WriteLine(existingProduct.IsTransient()); // false
```

---

### Method: `Equals(Entity<TId>?)`

```csharp
public virtual bool Equals(Entity<TId>? other)
```

**Description:** Determines whether the specified entity is equal to the current entity.

**Parameters:**
- `other` — The other entity to compare. May be `null`.

**Return:** `true` if both entities have the same concrete type (`GetType()`) and the same non-transient `Id`; `false` otherwise.

**Equality rules:**
- Two entities of different types are never equal (even with the same `Id`)
- A transient entity is never equal to any other entity (even with the same default Id)
- Reference equality (`ReferenceEquals`) returns `true` directly

**Example:**
```csharp
var id = Guid.NewGuid();
var e1 = new Customer(id, "Alice");
var e2 = new Customer(id, "Alice Alias");
e1.Equals(e2); // true — same type, same Id

var t1 = new Customer(Guid.Empty, "A");
var t2 = new Customer(Guid.Empty, "B");
t1.Equals(t2); // false — both are transient
```

---

### Method: `Equals(object?)`

```csharp
public override bool Equals(object? obj)
```

**Description:** Override of `object.Equals`. Delegates to `Equals(Entity<TId>?)`.

**Return:** `true` if `obj` is an `Entity<TId>` with the same type and same Id; `false` otherwise.

---

### Method: `GetHashCode()`

```csharp
public override int GetHashCode()
```

**Description:** Calculates the hash code for this entity.

**Return:** `HashCode.Combine(GetType(), Id.GetHashCode())` for entities with a real Id. For transient entities, returns `base.GetHashCode()` (instance memory hash) to ensure stability in collections.

**Performance:** O(1). For Strongly Typed Ids with `IEquatable<T>`, no boxing occurs.

---

### Operator: `==`

```csharp
public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
```

**Description:** Compares two entities using semantic equality. Handles `null` correctly.

**Return:** `true` if both are `null`, or if `left.Equals(right)` is `true`; `false` otherwise.

---

### Operator: `!=`

```csharp
public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
```

**Description:** Negation of the `==` operator.

**Return:** `!(left == right)`.

---

---

## `AggregateRoot<TId>`

Abstract base class for aggregate roots — the transactional consistency boundary in DDD. Inherits all members of `Entity<TId>`.

### Declaration

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull, IEquatable<TId>
```

**Inheritance:** All members of `Entity<TId>` are available.

**Thread Safety:** NOT thread-safe by design. The command handler must guarantee exclusive access.

---

### Property: `DomainEvents`

```csharp
public IReadOnlyCollection<IDomainEvent> DomainEvents
```

**Description:** Gets the read-only collection of pending domain events.

**Return:** `IReadOnlyCollection<IDomainEvent>` — never `null`.

**Remarks:**
- **Lazy allocation:** The internal list (`List<IDomainEvent>`) is NOT allocated until the first call to `RaiseDomainEvent`. Aggregates hydrated in read-only mode produce **zero bytes** of heap allocation for this collection.
- Returns `Array.Empty<IDomainEvent>()` when no events exist (no allocation).
- When events exist, returns `_domainEvents.AsReadOnly()` — without copying the list.

**Performance:** O(1) — direct access to the internal collection or the static empty array.

**Example:**
```csharp
var order = Order.Place(Guid.NewGuid(), "My order");
Console.WriteLine(order.DomainEvents.Count); // 1

// Zero alloc:
var freshAggregate = new EmptyAggregate(Guid.NewGuid());
Console.WriteLine(freshAggregate.DomainEvents.Count); // 0 — no internal allocation
```

---

### Method: `RaiseDomainEvent(IDomainEvent)`

```csharp
protected void RaiseDomainEvent(IDomainEvent domainEvent)
```

**Description:** Registers a domain event in the aggregate's internal collection. The event will be dispatched by the infrastructure layer after persisting changes.

**Parameters:**
- `domainEvent` — The event to register. Cannot be `null`.

**Exceptions:**
- `ArgumentNullException` — if `domainEvent` is `null`.

**Modifier:** `protected` — only the AggregateRoot itself can call it. This is the fundamental guarantee: events emerge from inside the aggregate as a consequence of business operations, not injected from outside.

**Performance:** First call: allocates `List<IDomainEvent>` and adds the event. Subsequent calls: only `List.Add` — O(1) amortized.

**When to use:** Inside AggregateRoot business methods, after verifying and applying invariants.

**When NOT to use:** Do not call in the hydration constructor (e.g. the parameterless constructor for EF Core). Events are raised only when a state change occurs, not when state is reconstructed from the database.

**Example:**
```csharp
public sealed class Order : AggregateRoot<Guid>
{
    private Order() { } // For hydration — does NOT raise events

    public static Order Place(Guid id)
    {
        var order = new Order { Id = id };
        order.RaiseDomainEvent(new OrderPlacedEvent(id)); // ✅ after validating
        return order;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Already cancelled.");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason)); // ✅
    }
}
```

---

### Method: `ClearDomainEvents()`

```csharp
public void ClearDomainEvents()
```

**Description:** Clears all pending domain events. Should be called by the infrastructure layer after successfully dispatching events.

**Return:** `void`.

**Exceptions:** None — idempotent. If no events exist (internal collection is `null`), it does nothing.

**Modifier:** `public` — accessible from the Infrastructure layer.

**Performance:** O(n) where n = number of events (equivalent to `List.Clear()`). If no events, O(1) (null check).

**Common mistakes:**
```csharp
// ❌ Incorrect — clearing before taking the snapshot
order.ClearDomainEvents();
foreach (var ev in order.DomainEvents) // ← empty!
    await _publisher.Publish(ev);

// ✅ Correct — snapshot first
var events = order.DomainEvents.ToList();
order.ClearDomainEvents();
foreach (var ev in events)
    await _publisher.Publish(ev);
```

---

---

## `IDomainEvent`

Marker interface for domain events.

### Declaration

```csharp
public interface IDomainEvent
```

**Description:** Empty interface that acts as a type contract for identifying domain events. Has no members.

**When to implement:** On any `record` (preferred) or `class` that represents something meaningful that happened in the domain.

**When NOT to implement:** Do not use for DTOs, ViewModels, integration events (outbox messages), or commands. Only for events that emerge from the domain.

**Design rationale:** Infrastructure metadata (timestamp, correlationId, eventId, retry count) belongs to the messaging envelope (Outbox row, MassTransit envelope, etc.), not to the domain event. The domain event expresses only the domain fact that occurred.

**Example:**
```csharp
// ✅ Recommended — immutable record
public sealed record OrderPlacedEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount
) : IDomainEvent;

// ✅ Multiple events on the same aggregate
public sealed record OrderItemAddedEvent(Guid OrderId, Guid ItemId, int Quantity) : IDomainEvent;
public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent;
public sealed record OrderShippedEvent(Guid OrderId, string TrackingNumber) : IDomainEvent;
```
