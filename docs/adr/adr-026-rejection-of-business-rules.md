# ADR-026: Rejection of `IBusinessRule` / `IDomainRule` Abstractions

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of rule evaluation abstractions in favor of guard clauses, Result-first operations, and Specifications.

---

## Context

Patterns like `IBusinessRule { bool IsBroken(); string Message { get; } }` or `IBusinessRule<T>` are sometimes proposed for encapsulating validation and domain invariants.

## Problem

1. **Duplication with Guard Clauses & Methods:** In modern C#, invariant enforcement inside Aggregate factory methods and domain methods is clearer and faster via direct guard clauses:
   ```csharp
   if (amount <= 0)
       throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
   ```
2. **Duplication with `Result<T>` and `Specification<T>`:**
   - For functional validation flows: `EricksonLopez.Result` handles error composition.
   - For query predicates and composable business criteria: `EricksonLopez.Specification` handles declarative rules.
3. **Allocation Overhead:** Instantiating rule objects for every state check adds unnecessary heap pressure on performance-critical paths.

## Decision

**Explicitly reject `IBusinessRule`, `IDomainRule`, and `IRule<T>` from `EricksonLopez.SharedKernel`.**

## Architectural Placement

- Use **Guard Clauses** in factory and mutation methods for critical invariants.
- Use **`EricksonLopez.Result`** for operations that can fail gracefully.
- Use **`EricksonLopez.Specification`** for queryable or composable criteria.

## Consequences

- **Positive:** Zero allocation overhead for state invariant checks.
- **Positive:** Avoids cognitive overload and overlapping patterns for validation.
