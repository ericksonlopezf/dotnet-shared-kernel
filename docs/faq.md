# Frequently Asked Questions (FAQ) — EricksonLopez.SharedKernel

---

## 1. What types does `EricksonLopez.SharedKernel` provide?

The library provides 9 core Domain-Driven Design building blocks:

| Type | Kind | Purpose |
|---|---|---|
| `IStrongId<TSelf, TValue>` | `interface` | Strongly-typed entity identifier contract (CRTP) |
| `IEntity<TId>` | `interface` | Generic entity contract exposing `Id` |
| `Entity<TId>` | `abstract class` | Base entity class with type + ID semantic equality |
| `IHasDomainEvents` | `interface` | Non-generic contract for atomically draining (snapshot + clear) domain events via `DrainDomainEvents()` |
| `IAggregateRoot` | `interface` | Marker contract for Aggregate Roots inheriting `IHasDomainEvents` |
| `AggregateRoot<TId>` | `abstract class` | Transactional boundary base class with lazy event allocation |
| `DomainEvent` | `abstract record` | Time-ordered domain event base record with `Id` (UUIDv7) and `OccurredAt` (UTC) |
| `ValueObject` | `abstract record` | Base record for structural value objects |
| `ValueObjectAttribute` | `sealed class` | Metadata attribute for Native AOT and source generators |

---

## 2. Why are `Result<T>` and `Error` not included in this package?

In accordance with Architecture Decision Record **[ADR-014](decisions/ADR-014-removal-of-result-dependency.md)**, functional error handling types were extracted to the dedicated `EricksonLopez.Result` package. This preserves `EricksonLopez.SharedKernel` as a pure, zero-dependency Tier 0 foundation.

---

## 3. How does `AggregateRoot<TId>` achieve 0 bytes allocation on hydration?

`AggregateRoot<TId>` initializes its private `_domainEvents` backing list lazily. When an entity is hydrated from the database in a read-only scenario (no business methods called), `_domainEvents` remains `null`. `DrainDomainEvents()` returns `[]` (`Array.Empty<IDomainEvent>()`) in this case, producing zero heap allocations.

---

## 4. Why does `DomainEvent` use UUIDv7 on .NET 9+?

UUIDv7 embeds a millisecond-precision Unix timestamp in the most significant 48 bits of the GUID. This creates time-ordered, sequentially increasing values, which prevents index fragmentation and improves B-Tree write performance in PostgreSQL, SQL Server, and SQLite.

---

## 5. Can I use custom primitive types for Strongly-Typed IDs?

Yes. `IStrongId<TSelf, TValue>` supports any `TValue` that implements `IEquatable<TValue>` (e.g., `Guid`, `long`, `string`, `int`).

```csharp
public readonly record struct SequenceId(long Value) : IStrongId<SequenceId, long>;
public readonly record struct SkuCode(string Value) : IStrongId<SkuCode, string>;
```

---

## 6. How should Unit of Work dispatch domain events?

Call `DrainDomainEvents()` — it atomically snapshots all pending events and resets the aggregate's internal buffer in a single call. There is no separate `ClearDomainEvents()` or public `DomainEvents` property:

```csharp
var events = aggregate.DrainDomainEvents();
await outbox.SaveAsync(events);
```
