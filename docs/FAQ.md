# FAQ — EricksonLopez.SharedKernel

Frequently asked questions about the library, its design, and its usage.

---

## 1. What does the library include?

Three types in the EricksonLopez.SharedKernel namespace:

| Type | Description |
|---|---|
| `Entity<TId>` | Base for domain entities with identity-based equality |
| `AggregateRoot<TId>` | Base for aggregate roots with domain event support |
| `IDomainEvent` | Marker interface for domain events |

---

## 2. What does the library NOT include?

By explicit design decision (see ADRs in `docs/decisions/`), it does **not** include:

- ✗ Result Pattern / Error types — separated to `EricksonLopez.Result` (ADR-014)
- ✗ ValueObject — separated to `EricksonLopez.DomainPrimitives` (ADR-017)
- ✗ Specification Pattern — separated to `EricksonLopez.Specifications` (ADR-008)
- ✗ UnitOfWork / Repositories — belong to Infrastructure
- ✗ Outbox / Publisher — belong to Infrastructure
- ✗ PaginationParameters / PagedList — separated to `EricksonLopez.Pagination` (ADR-016)
- ✗ External dependencies — the library is zero-dependency

---

## 3. What type can I use for TId?

Any type that satisfies `notnull, IEquatable<TId>`. Examples:

```csharp
Entity<Guid>            // most common
Entity<int>             // legacy databases with int PKs
Entity<long>            // high-scale systems
Entity<string>          // alphanumeric ID systems
Entity<OrderId>         // Strongly Typed Id (record struct) — recommended
```

---

## 4. Why is `Id` `protected init` and not `public set`?

Entity identity is **immutable by DDD definition**. An entity that can change its Id after creation violates the fundamental domain identity principle. The `init` modifier guarantees the Id is assigned exactly once (during construction) and cannot be changed afterward.

---

## 5. What is a "transient" entity?

An entity is transient when its `Id` holds the `default` value of the type — e.g. `Guid.Empty` for `Entity<Guid>`, or `0` for `Entity<int>`. This indicates the entity has not yet been assigned a persistent identity.

```csharp
var e = new MyEntity(Guid.Empty);
e.IsTransient(); // true

var e2 = new MyEntity(Guid.NewGuid());
e2.IsTransient(); // false
```

**Important:** Two transient entities are NEVER equal to each other, even if they share the same default Id value. This preserves the DDD invariant that equality requires a real identity.

---

## 6. Why is `RaiseDomainEvent` `protected` and not `public`?

Because domain events must originate **inside** the aggregate, as a direct consequence of a business operation. Allowing external code to raise events would break DDD encapsulation: the aggregate would become a passive data container rather than a guardian of its invariants.

---

## 7. Can I use `Entity<TId>` with EF Core?

Yes. EF Core can configure `Id` as the primary key:

```csharp
modelBuilder.Entity<Order>()
    .HasKey(o => o.Id);
```

**Important consideration:** If you use **lazy loading proxies**, EF Core creates dynamic subclasses. `Entity<TId>` equality uses `GetType()` (without proxy-unwrapping), meaning a proxy and the real entity would be of different types. This is a deliberate design decision (ADR-015): proxy-unwrapping is the responsibility of the Infrastructure layer.

---

## 8. Is the library NativeAOT compatible?

Yes. `IsAotCompatible=true` and `IsTrimmable=true` are active on **all** supported TFMs (`net8.0`, `net9.0`, `net10.0`). It uses no dynamic reflection or runtime code generation.

---

## 9. Can an AggregateRoot raise multiple domain events?

Yes. `RaiseDomainEvent` can be called multiple times and events accumulate in order of arrival:

```csharp
var order = Order.Place(Guid.NewGuid(), "alice@example.com");
order.AddLine(new OrderLine(...));
order.ApplyDiscount(10m);

order.DomainEvents.Count; // 3 — one per operation
```

---

## 10. When should I call `ClearDomainEvents()`?

In the **Infrastructure layer**, after persisting state **and** before (or immediately after) publishing events. The standard pattern is:

1. Persist with `SaveChangesAsync()`
2. Copy `DomainEvents` to a temporary list
3. Call `ClearDomainEvents()`
4. Publish from the temporary list

Clearing before publishing prevents re-dispatch if the Publisher fails and the UoW needs to retry.

---

## 11. What happens if I call `ClearDomainEvents()` before raising any events?

Nothing. It is a safe, idempotent operation. If no events have been raised, the internal collection is `null` (lazy allocation) and the method is a no-op.

---

## 12. Why don't domain events have a timestamp or correlationId?

By explicit design (see the XML documentation of `IDomainEvent`): those metadata fields belong to the **messaging infrastructure** (Outbox envelope, Kafka headers, etc.), not to the domain event itself. The domain only expresses what happened, not how it is transported or when it was processed.

---

## 13. Can I inherit from both `AggregateRoot<TId>` and `Entity<TId>`?

No. `AggregateRoot<TId>` already inherits from `Entity<TId>`. C# does not support multiple inheritance. If a type is an AggregateRoot, use `AggregateRoot<TId>`. If it is just an entity (no events), use `Entity<TId>`.

---

## 14. Does the library have breaking changes between versions?

See the [CHANGELOG](../CHANGELOG.md) and the [Migration Guide](MigrationGuide.md). Version v2.0 removed several types (Result, ValueObject, Specification, Testing project) that have been separated into their own libraries or excluded permanently.
