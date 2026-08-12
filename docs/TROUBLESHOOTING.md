# Troubleshooting — EricksonLopez.SharedKernel

Common problem resolution guide.

---

## Error: Cannot assign `Id` outside the constructor

**Symptom:**
```
CS0200: Property or indexer 'Entity<TId>.Id' cannot be assigned to — it is read only
```

**Cause:** `Id` is `protected init`. It can only be assigned in the derived class constructor.

**Solution:**
```csharp
// ❌ Incorrect
public sealed class MyEntity : Entity<Guid>
{
    public void ChangeId(Guid newId)
    {
        Id = newId; // CS0200
    }
}

// ✅ Correct — assign in the constructor
public sealed class MyEntity : Entity<Guid>
{
    public MyEntity(Guid id)
    {
        Id = id; // Valid — we are in the constructor
    }
}
```

---

## Error: Domain events are not dispatched

**Symptom:** Domain events accumulate in `DomainEvents` but handlers never execute.

**Cause:** The library provides event storage, but **not** the dispatch mechanism. Dispatching is the responsibility of the Infrastructure layer.

**Solution:** In your UnitOfWork or DbContext interceptor, after `SaveChangesAsync()`:

```csharp
// 1. Get the event snapshot
var events = aggregate.DomainEvents.ToList();

// 2. Clear (before publishing to avoid re-dispatch)
aggregate.ClearDomainEvents();

// 3. Publish via your mechanism (MediatR, Outbox, etc.)
foreach (var ev in events)
    await _publisher.Publish(ev);
```

---

## Error: `RaiseDomainEvent(null)` throws `ArgumentNullException`

**Symptom:** `System.ArgumentNullException: Value cannot be null. (Parameter 'domainEvent')`

**Cause:** Attempted to register a null event. `RaiseDomainEvent` protects against this explicitly.

**Solution:** Always instantiate the event before passing it:

```csharp
// ❌ Incorrect
RaiseDomainEvent(null!);

// ✅ Correct
RaiseDomainEvent(new OrderPlacedEvent(Id));
```

---

## Problem: Two entities with the same Id are not equal

**Symptom:** `entity1 == entity2` returns `false` even though they have the same `Id`.

**Cause A — Different types:**
Equality requires the same concrete type (`GetType()`) AND the same `Id`. If they are different subclasses with the same Id, they are not equal (correct DDD behavior).

```csharp
// Given: class Order : AggregateRoot<Guid> and class InvoiceOrder : Order
var id = Guid.NewGuid();
var order = new Order(id);
var invoiceOrder = new InvoiceOrder(id);
order == invoiceOrder; // false — different types
```

**Cause B — Transient entities:**
If the `Id` is the `default` value (e.g. `Guid.Empty`), the entity is transient. Two transient entities are never equal, even if they share the same default Id.

```csharp
var e1 = new MyEntity(Guid.Empty); // transient
var e2 = new MyEntity(Guid.Empty); // transient
e1 == e2; // false — DDD invariant
```

**Solution:** Assign a real Id before comparing.

---

## Problem: `GetHashCode()` of a transient entity causes unexpected behavior in collections

**Symptom:** The entity is added to a `HashSet` and after assigning an Id, lookups don't work.

**Cause:** `GetHashCode()` on transient entities uses `base.GetHashCode()` (memory reference hash). Since `Id` is `protected init`, it cannot be changed after construction — if you add a transient entity to a HashSet without a real Id, subsequent lookups with a different instance (even with the same Id) will fail.

**Solution:** Only add entities to hash-based collections when they already have a definitive Id.

---

## Problem: EF Core lazy loading proxies and incorrect equality

**Symptom:** `order.Customer == customer` returns `false` when both have the same Id.

**Cause:** EF Core uses dynamic proxies for lazy loading. The proxy is a subclass of the real type. `Entity<TId>.Equals` uses `GetType()`, so `CustomerProxy` and `Customer` are different types → not equal.

**Solution:** Configure EF Core to avoid unnecessary lazy loading, or compare by `Id` directly when proxies are involved:

```csharp
// Instead of:
if (order.Customer == customer) { ... }

// Use:
if (order.Customer.Id == customer.Id) { ... }
```

See ADR-015 in `docs/decisions/` for the full rationale.

---

## Problem: The project does not compile with NativeAOT

**Symptom:** Warnings or errors when publishing with `dotnet publish -r <rid> --aot`.

**Probable cause:** The consuming code (not the library) uses non-AOT-compatible APIs.

**Verification:** The library itself has `IsAotCompatible=true` on all TFMs. If there are trimming or AOT warnings, verify the Infrastructure/Application layer code in the consuming project.

---

## Problem: `DomainEvents` always returns an empty collection

**Symptom:** After calling aggregate methods, `DomainEvents.Count == 0`.

**Cause:** `RaiseDomainEvent` is `protected`. If the business method does not call `RaiseDomainEvent` internally, no event is registered.

**Solution:** Verify that AggregateRoot methods call `RaiseDomainEvent` where appropriate:

```csharp
public sealed class Order : AggregateRoot<Guid>
{
    public static Order Place(Guid id)
    {
        var order = new Order { Id = id };
        order.RaiseDomainEvent(new OrderPlacedEvent(id)); // ← must be here
        return order;
    }
}
```
