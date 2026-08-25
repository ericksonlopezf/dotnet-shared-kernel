# ADR-025: Rejection of `IDomainService` Marker Interface

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of empty marker interfaces without semantic or behavioral contracts.

---

## Context

Domain-Driven Design identifies Domain Services for logic that spans multiple entities or doesn't belong naturally inside an aggregate root. Some libraries declare:
```csharp
public interface IDomainService;
```

## Problem

1. **Zero Behavioral Value:** An empty marker interface provides no compile-time guarantees, methods, or runtime optimizations.
2. **Naming Convention is Sufficient:** In C#, class names and namespaces (`TaxCalculationDomainService` or `PricingService`) convey domain role directly without marker bloat.
3. **Misuse Risk:** Marker interfaces can encourage improper dependency injection or accidental cross-layer leaks.

## Decision

**Explicitly reject `IDomainService` as a marker interface in `EricksonLopez.SharedKernel`.**

## Architectural Placement

Domain Services should be written directly as concrete classes or specific interfaces within each **Bounded Context's Domain layer**.

## Consequences

- **Positive:** Keeps the SharedKernel public API minimal and strictly focused on actionable primitives.
