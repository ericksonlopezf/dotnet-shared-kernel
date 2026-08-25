# Level 00 — Architecture & Foundational DDD Philosophy

Welcome to the **EricksonLopez.SharedKernel** interactive showcase.

---

## 🎯 The Architectural Role of a Tier-0 Shared Kernel

In enterprise Domain-Driven Design (DDD) and Clean Architecture systems, the Shared Kernel acts as the **sovereign foundational substrate (Tier-0)** that defines:
1. **Core Domain Primitives**: Strongly-typed entity identifiers, base `Entity<TId>`, and `AggregateRoot<TId>`.
2. **Domain Event Lifecycles**: Zero-allocation domain event collection, raising, and clearing invariants.
3. **Repository & Unit of Work Contracts**: Pure application-level port interfaces decoupled from database drivers.
4. **Zero Functional Dependencies**: No third-party package couplings in core domain contracts.

```mermaid
graph TD
    SK[EricksonLopez.SharedKernel (Tier-0)]
    DP[EricksonLopez.DomainPrimitives]
    VO[EricksonLopez.ValueObjects]
    EV[EricksonLopez.Events]
    RES[EricksonLopez.Result]

    DP --> SK
    VO --> SK
    EV --> SK
    RES --> SK
```
