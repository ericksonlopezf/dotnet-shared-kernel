# Troubleshooting — EricksonLopez.SharedKernel

Common issues, diagnostic guidance, and resolutions when working with `EricksonLopez.SharedKernel`.

---

## 1. Error: `Entity identity cannot be null or default.`

**Symptom:**
```
System.ArgumentException: Entity identity cannot be null or default. (Parameter 'id')
```

**Cause:**
`Entity<TId>` enforces non-default identities upon instantiation. Passing `Guid.Empty`, `0`, `null`, or an uninitialized `default` strongly-typed ID triggers this guard.

**Resolution:**
Generate a valid, non-default identifier before instantiating the entity or aggregate:

```csharp
// ❌ Throws ArgumentException
var order = new Order(new OrderId(Guid.Empty), customerId);

// ✅ Correct
var order = new Order(new OrderId(Guid.NewGuid()), customerId);
```

---

## 2. Error: `Cannot assign 'Entity<TId>.Id' — it is read only`

**Symptom:**
```
CS0200: Property or indexer 'Entity<TId>.Id' cannot be assigned to — it is read only
```

**Cause:**
`Id` is a getter-only property set exclusively via the constructor call to `base(id)`. This enforces DDD identity immutability at the type level.

**Resolution:**
Pass the identifier via constructor to the `base(id)` call:

```csharp
// ❌ Incorrect
public void SetId(OrderId id) => Id = id;

// ✅ Correct
public sealed class Order : AggregateRoot<OrderId>
{
    public Order(OrderId id) : base(id) { }
}
```

---

## 3. Error: Domain events are never dispatched to handlers

**Symptom:**
`DomainEvents` contains items in memory, but no background worker or subscriber receives them.

**Cause:**
`EricksonLopez.SharedKernel` is a pure Tier 0 domain library and deliberately contains no message broker or event bus transport. Dispatching is handled by the Infrastructure layer (e.g., Unit of Work or Outbox processor).

**Resolution:**
Integrate an event collector in your infrastructure layer using `DrainDomainEvents()`, which atomically snapshots and clears all pending events:

```csharp
public async Task SaveChangesAndDispatchAsync(IHasDomainEvents aggregate, IOutboxStore outbox)
{
    // DrainDomainEvents() is the only API: no separate DomainEvents property or ClearDomainEvents().
    var events = aggregate.DrainDomainEvents();

    foreach (var domainEvent in events)
    {
        await outbox.AppendAsync(domainEvent);
    }
}
```

---

## 4. Error: `RaiseDomainEvent(null)` throws `ArgumentNullException`

**Symptom:**
```
System.ArgumentNullException: Value cannot be null. (Parameter 'domainEvent')
```

**Cause:**
`AggregateRoot<TId>.RaiseDomainEvent` explicitly guards against null domain events.

**Resolution:**
Always construct a concrete `DomainEvent` instance before raising:

```csharp
// ✅ Correct
RaiseDomainEvent(new OrderPlacedEvent(Id, total));
```

---

## 5. Issue: Two entities with identical IDs evaluate as unequal

**Symptom:**
`entity1 == entity2` evaluates to `false`.

**Cause:**
The two instances belong to different concrete runtime types (`GetType()`). In DDD, entities are defined by the combination of their concrete domain type and their unique ID.

```csharp
var id = Guid.NewGuid();
var product = new Product(id, "Keyboard");
var supplier = new Supplier(id, "TechCorp");

bool equal = (product.Equals(supplier)); // Returns false (Product != Supplier)
```
