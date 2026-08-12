# Migration Guide — EricksonLopez.SharedKernel

Migration guide for adopting or upgrading `EricksonLopez.SharedKernel` in your project.

> [!IMPORTANT]
> This guide covers the v2.0 API which contains: `Entity<TId>`, `AggregateRoot<TId>`, `IDomainEvent`.
> The types `Result<T>`, `ValueObject`, `Error`, `Specification<T>`, and `PaginationParameters` were removed from this library in v2.0.
> If you are using them, refer to separate dedicated libraries.

---

## Migrating from v1.0.0 to v2.0

### Breaking Changes

The following types were **removed** from `EricksonLopez.SharedKernel` in v2.0:

| Removed Type | Reason | Alternative |
|---|---|---|
| `Result<T>` / `Result` | Separated to its own library | A dedicated Result library |
| `Error` / `ErrorType` | Separated to its own library | A dedicated Result library |
| `ValueObject` | Separated to its own library | `EricksonLopez.DomainPrimitives` |
| `Specification<T>` | Separated (ADR-008) | Custom implementation or `Ardalis.Specification` |
| `PaginationParameters` | Removed | Custom implementation |
| `PagedList<T>` | Removed | Custom implementation |

### Using directive migration

If your project has:
```csharp
using EricksonLopez.SharedKernel.Results;      // ← remove
using EricksonLopez.SharedKernel.Domain;       // ← remove
using EricksonLopez.SharedKernel.Specifications; // ← remove
```

Replace with:
```csharp
using EricksonLopez.SharedKernel;              // ← only valid namespace
```

---

## Migrating from a manual Entity implementation

If you have your own `Entity` class:

### Before (manual implementation)

```csharp
public abstract class Entity
{
    public Guid Id { get; set; }  // mutable — incorrect for DDD

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

**Problems:** Mutable Id, does not consider the concrete type, does not handle transient entities.

### After (using the library)

```csharp
using EricksonLopez.SharedKernel;

public sealed class Order : AggregateRoot<Guid>
{
    // Id is protected init — immutable by design
    // Equals and GetHashCode are correctly implemented
    // == and != operators also available

    public static Order Create(Guid id)
    {
        return new Order { Id = id };
    }
}
```

**Benefits:** Immutable Id, equality by type + Id, correct handling of transient entities, overloaded operators.

---

## Migrating from a manual AggregateRoot implementation

### Before (manual implementation with event List)

```csharp
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> Events => _events.AsReadOnly();

    protected void AddEvent(IDomainEvent ev) => _events.Add(ev);
    public void ClearEvents() => _events.Clear();
}
```

**Problem:** `List<T>` is always allocated, even if no events are raised (not lazy).

### After (using the library)

```csharp
using EricksonLopez.SharedKernel;

// AggregateRoot<TId> already includes:
// - DomainEvents (lazy — zero alloc if no events)
// - RaiseDomainEvent(IDomainEvent) — protected
// - ClearDomainEvents() — public

public sealed class Order : AggregateRoot<Guid>
{
    // Only business logic here
}
```

**Benefits:** Lazy allocation (zero alloc for read-only aggregates), null guard in `RaiseDomainEvent`.

---

## Migrating from Ardalis.SharedKernel

```csharp
// Ardalis.SharedKernel
using Ardalis.SharedKernel;

public class Order : EntityBase, IAggregateRoot
{
    public void PlaceOrder()
    {
        RegisterDomainEvent(new OrderPlacedEvent());
    }
}
```

```csharp
// EricksonLopez.SharedKernel
using EricksonLopez.SharedKernel;

public sealed class Order : AggregateRoot<int>  // generic TId
{
    public static Order Place(int id)
    {
        var order = new Order { Id = id };
        order.RaiseDomainEvent(new OrderPlacedEvent(id));
        return order;
    }
}
```

**Key differences:**
- `EntityBase` → `Entity<TId>` (generic: choose your Id type)
- `IAggregateRoot` marker → `AggregateRoot<TId>` base class (includes the logic)
- `RegisterDomainEvent` → `RaiseDomainEvent` (same concept, different name)

---

## Adopting Strongly Typed Ids (recommended upgrade)

### Before — Naked Guid Id

```csharp
public sealed class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    // Is this an OrderId? A CustomerId? The compiler doesn't know.
}
```

### After — Strongly Typed Ids

```csharp
public readonly record struct OrderId(Guid Value);
public readonly record struct CustomerId(Guid Value);

public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    // The compiler validates that you don't mix OrderId with CustomerId
}
```

**Incremental migration:** You can migrate one type at a time. `AggregateRoot<Guid>` and `AggregateRoot<OrderId>` are independent — there is no dependency between them.
