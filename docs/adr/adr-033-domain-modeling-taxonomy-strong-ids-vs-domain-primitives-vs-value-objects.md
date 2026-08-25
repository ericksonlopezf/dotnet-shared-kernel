# ADR-033: Domain Modeling Taxonomy — StronglyTypedIds vs DomainPrimitives vs ValueObjects

## Status
Accepted — August 2026

## Context
In Domain-Driven Design (DDD) with .NET 10 / C# 14, domain models require strongly-typed structures to eliminate Primitive Obsession.
Across the `EricksonLopez.*` ecosystem, three separate packages handle distinct aspects of domain types:
1. `EricksonLopez.SharedKernel`: Exposes `IStrongId<T>` and `StrongId<TSelf, TValue>` for entity/aggregate identity.
2. `EricksonLopez.DomainPrimitives`: Exposes atomic, single-property domain primitives validated via the Result pattern (`Email`, `PhoneNumber`, `Sku`).
3. `EricksonLopez.ValueObjects`: Exposes multi-property composite value objects with domain behavior and operations (`Money`, `Address`, `DateRange`, `FiscalTaxDetails`).

Without clear taxonomy rules, developers risk confusing these constructs (e.g., modeling IDs as complex Value Objects, or modeling composite domain concepts as scalar primitives).

## Decision
Establish a strict 3-tier DDD taxonomy governing the ecosystem:

```
                                  DDD CONCEPT
                                       │
         ┌─────────────────────────────┼─────────────────────────────┐
         ▼                             ▼                             ▼
┌──────────────────┐          ┌──────────────────┐          ┌──────────────────┐
│  StronglyTypedId │          │ Domain Primitive │          │   Value Object   │
│  (SharedKernel)  │          │(DomainPrimitives)│          │  (ValueObjects)  │
├──────────────────┤          ├──────────────────┤          ├──────────────────┤
│ • Entity Identity│          │ • Atomic Field   │          │ • Composite      │
│ • PK / FK        │          │ • Invariant      │          │   Concept        │
│ • Guid / long    │          │   Validation     │          │ • Multi-property │
│ • Zero-alloc     │          │ • Result Pattern │          │ • Operations     │
│   struct         │          │ • Email, SKU     │          │ • Money, Address │
└──────────────────┘          └──────────────────┘          └──────────────────┘
```

### 1. Strongly-Typed IDs (`IStrongId<T>`)
- **Responsibility**: Pure identity representation of `Entity<TId>` and `AggregateRoot<TId>`.
- **Implementation**: Must be declared as `readonly record struct` implementing `IStrongId<TValue>`.
- **Heap Allocation**: Zero bytes (`0 B`).
- **Persistence**: Direct 1:1 scalar mapping via Dapper TypeHandlers (`DapperStrongIdRegistry`) and EF Core Value Converters (`ConfigureStrongIdsFromAssembly`).
- **Forbidden**: Complex business rules, cross-field invariants, or mutable state.

### 2. Domain Primitives (`EricksonLopez.DomainPrimitives`)
- **Responsibility**: Atomic domain attributes that enforce business invariants and normalization at construction time.
- **Implementation**: `readonly record struct` (or `sealed record class`) created exclusively via factory methods returning `Result<T>`.
- **Validation**: Enforces formatting, length, character sets, and semantic rules (no exceptions for control flow).
- **Persistence**: Maps to single database column (`varchar`, `decimal`, etc.).
- **Forbidden**: Modeling compound multi-field concepts or acting as entity identities.

### 3. Composite Value Objects (`EricksonLopez.ValueObjects`)
- **Responsibility**: Rich domain concepts composed of multiple attributes with domain operations and structural equality.
- **Implementation**: `sealed record class` or `readonly struct` with domain behavior methods (e.g., `Money.Add(Money other)`, `DateRange.Overlaps(DateRange other)`).
- **Persistence**: Mapped via EF Core `OwnsOne`/`ComplexProperty`, Dapper multi-column mapping, or JSONB type handlers.
- **Forbidden**: Having independent lifecycle or identity (PK/FK).

## Decision Matrix

| Evaluation Criteria | Use | Canonical Package |
|---|:---:|---|
| Primary or Foreign Key of an Entity / Aggregate Root | **`StronglyTypedId`** | `EricksonLopez.SharedKernel` |
| Single scalar attribute with strict formatting/validation invariants | **`DomainPrimitive`** | `EricksonLopez.DomainPrimitives` |
| Compound concept (2+ fields) with arithmetic or domain operations | **`ValueObject`** | `EricksonLopez.ValueObjects` |

## Consequences

### Positive
- Strict separation of concerns between Identity, Validation, and Behavior.
- Maximum performance: Zero-allocation structs for IDs prevent heap pollution during bulk data hydration.
- Consistent architecture across all microservices and enterprise applications.

### Negative
- Developers must understand the 3 distinct roles rather than applying a generic Value Object wrapper to all types.

## References
- Clean Architecture & DDD Invariants
- Ecosystem Governance Audit: `M-05`
