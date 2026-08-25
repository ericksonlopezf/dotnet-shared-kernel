# ADR-019: Rejection of `IUnitOfWork`

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of transaction coordination abstractions from Tier 0 SharedKernel.

---

## Context

`IUnitOfWork` is a well-known pattern for maintaining a list of objects affected by a business transaction and coordinating the writing out of changes.

Proposals often suggest adding:
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
}
```

## Problem

1. **Infrastructure & Transactional Concern:** Units of work govern relational database transactions, distributed transactions, or outbox persistence. The core domain layer has no concept of database connections or commits.
2. **False Abstraction:** An EF Core `DbContext` is already a Unit of Work. Wrapping it in a generic `IUnitOfWork` creates unnecessary indirection without adding value. For Dapper/ADO.NET, a custom UoW looks completely different (`DbTransaction`).
3. **AggregateRoot Contract is Sufficient:** The domain contract for deferred side-effects is already fulfilled by `AggregateRoot<TId>` via `DrainDomainEvents()` — the single atomic operation that snapshots and clears all pending domain events. Infrastructure interceptors or Unit of Work handlers consume this via the `IHasDomainEvents` interface directly.

## Decision

**Explicitly reject `IUnitOfWork` from `EricksonLopez.SharedKernel`.**

## Architectural Placement

`IUnitOfWork` contracts and implementations belong exclusively to the **Infrastructure layer** or application-specific persistence wrappers of the consumer.

## Consequences

- **Positive:** No database lifecycle or transaction model constraints imposed by SharedKernel.
- **Positive:** Works seamlessly with any persistence technology (EF Core, Dapper, Marten, MongoDB, in-memory tests).
