# ADR-020: Rejection of Auditing Fields (`IAuditable`, `CreatedAt`, `CreatedBy`)

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of cross-cutting metadata and auditing traits from Entity/Aggregate base types.

---

## Context

Audit metadata fields (such as `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `RowVersion`) are ubiquitous across enterprise databases.

Proposals often suggest adding:
```csharp
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
```

## Problem

1. **Persistence Metadata vs. Business Domain:** Creation and modification timestamps are audit log artifacts, not intrinsic domain concepts. When a domain concept genuinely requires time (e.g. `OrderPlacedOn`), it should be explicitly modeled as a first-class property of that entity, not as generic plumbing.
2. **Context-Specific Schemes:** Different Bounded Contexts use different user identifiers (`Guid`, `string`, `UserId`, `int`, service account names). Imposing an auditing interface forces an opinionated identity scheme.
3. **Infrastructure Automations:** Audit fields are populated automatically via EF Core `SaveChangesInterceptor`, database triggers, or SQL default constraints. Polluting domain entity signatures with these setters compromises encapsulation.

## Decision

**Explicitly reject `IAuditable`, auditing traits, and base audit properties from `EricksonLopez.SharedKernel`.**

## Architectural Placement

Auditing belongs in the **Infrastructure layer** via ORM interceptors or dedicated persistence models, or modeled explicitly as domain properties when business logic depends on them.

## Consequences

- **Positive:** Domain entity signatures remain uncluttered and focused purely on business invariants.
- **Positive:** No assumptions regarding temporal precision or identity format.
