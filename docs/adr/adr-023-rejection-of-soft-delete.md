# ADR-023: Rejection of `ISoftDeletable`

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of database soft-delete interfaces from Tier 0 SharedKernel.

---

## Context

Soft delete (marking records as inactive via flags like `IsDeleted` or `DeletedAt` rather than executing physical `DELETE` statements) is a common storage practice.

Proposals often suggest adding:
```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
```

## Problem

1. **Persistence Mechanism Masquerading as Domain:** In Domain-Driven Design, entity lifecycle transitions have business meaning (e.g. `Order.Cancel()`, `Subscription.Expire()`, `Employee.Terminate()`). A generic boolean `IsDeleted` flag obscures domain language.
2. **Implementation Belongs to Persistence:** The physical mechanism of soft delete (e.g. EF Core Global Query Filters or SQL views) is an ORM/database detail.
3. **Not Universal:** Many aggregates are append-only (event sourcing) or undergo permanent archiving.

## Decision

**Explicitly reject `ISoftDeletable` and soft-delete traits from `EricksonLopez.SharedKernel`.**

## Architectural Placement

Model entity status transitions explicitly as business methods on the Aggregate Root. If physical soft-deletion is needed at the database level, implement it via EF Core Global Query Filters or database triggers in the **Infrastructure layer**.

## Consequences

- **Positive:** Domain models express clear business state transitions rather than generic deletion flags.
- **Positive:** No leakage of ORM query filter conventions into Tier 0.
