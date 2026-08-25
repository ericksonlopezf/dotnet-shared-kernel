# ADR-018: Rejection of Generic Repository (`IRepository<T>`)

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of persistence abstraction anti-patterns from Tier 0 SharedKernel.

---

## Context

The Generic Repository pattern (`IRepository<T>`, `IRepository<TAggregate, TId>`) is frequently introduced in Shared Kernel libraries with the intent of standardizing data access across Bounded Contexts.

Typical proposed contract:
```csharp
public interface IRepository<T, TId> where T : AggregateRoot<TId>
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

## Problem

1. **Persistence Ignorance Misinterpretation:** Persistence ignorance dictates that domain models must not know how they are persisted; it does **not** mean the SharedKernel domain layer should abstract database CRUD mechanisms.
2. **Leaky Abstraction & Scope Creep:** Generic repository methods invariably leak infrastructure concerns (e.g., query cancellation, paging tokens, tracking behaviors, or transaction boundaries).
3. **Loss of Domain Semantics:** In DDD, repositories are domain-specific collections (e.g., `IOrderRepository.GetPendingOrdersForCustomerAsync()`). A generic CRUD interface destroys aggregate boundary intention and encourages anemic domain models.
4. **CQRS Incompatibility:** Command and Query pipelines require fundamentally different data access shapes. Generic repositories force read/write symmetric coupling.

## Decision

**Explicitly reject `IRepository<T>` from `EricksonLopez.SharedKernel`.**

No repository abstractions or CRUD interfaces shall exist in this library.

## Architectural Placement

Repository interfaces belong in each specific **Bounded Context's Domain/Application layer** as specific contracts (`IOrderRepository`), while their concrete implementations belong in the **Infrastructure layer** (EF Core, Dapper).

## Consequences

- **Positive:** Zero coupling between Bounded Contexts regarding database access paradigms.
- **Positive:** Preserves the Tier 0 purity of `EricksonLopez.SharedKernel`.
- **Negative:** Consumidores must define their own specific repository contracts per aggregate (which aligns with DDD best practices).
