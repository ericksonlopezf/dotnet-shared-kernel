# Migration Guide — EricksonLopez.SharedKernel

Migration and upgrade guide for adopting `EricksonLopez.SharedKernel` in your .NET applications.

---

## 1. Migrating to Current Public API

### Summary of Contracts

| Concept | Base Type / Interface in EricksonLopez.SharedKernel | Purpose |
|---|---|---|
| Strongly-Typed ID | `IStrongId<TSelf, TValue>` | Type-safe identity wrapping primitives (`Guid`, `long`, etc.) |
| Entity Interface | `IEntity<TId>` | Core generic contract exposing `Id` |
| Domain Entity | `Entity<TId>` | Base class with type + ID equality |
| Domain Event Contract | `IHasDomainEvents` | Non-generic SRP contract for atomically draining events via `DrainDomainEvents()` |
| Aggregate Root Marker | `IAggregateRoot` | Marker interface inheriting `IHasDomainEvents` |
| Aggregate Root Base | `AggregateRoot<TId>` | Consistency boundary with lazy event sourcing |
| Domain Event Base | `DomainEvent` | Abstract record with `Id` (UUIDv7, type `EventId`) and `OccurredAt` (UTC) |
| Value Object Base | `ValueObject` | Base record for structural multi-attribute concepts |
| Value Object Metadata | `ValueObjectAttribute` | Static attribute for Native AOT and source generators |

---

## 2. Upgrading Legacy Domain Models

### From Custom Entities

```csharp
// Before (Mutable / Inconsistent)
public abstract class LegacyEntity
{
    public Guid Id { get; set; }
}

// After (Immutable Identity + Type Guard)
using EricksonLopez.SharedKernel;

public sealed class Order : AggregateRoot<OrderId>
{
    public Order(OrderId id) : base(id) { }
}
```

### From Marker Interfaces to `DomainEvent` Base Record

```csharp
// Before (Plain interface without sequential identity)
public interface IDomainEvent { }
public record OrderPlacedEvent(Guid OrderId) : IDomainEvent;

// After (Inheriting DomainEvent with UUIDv7 Id and UTC OccurredAt; aliases EventId/OccurredOn available)
using EricksonLopez.SharedKernel;

public sealed record OrderPlacedEvent(OrderId OrderId) : DomainEvent;
```

---

## 3. Strongly-Typed Identifier Adoption

Adopting `IStrongId<TSelf, TValue>` can be done incrementally on an entity-by-entity basis:

```csharp
// Step 1: Define Strongly Typed ID
public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>;

// Step 2: Use in Aggregate Root
public sealed class Order : AggregateRoot<OrderId>
{
    public Order(OrderId id) : base(id) { }
}
```
