# ADR-024: Rejection of Custom Clock Abstraction (`IClock`, `IDateTimeProvider`)

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Adoption of standard BCL `TimeProvider` over proprietary clock interfaces.

---

## Context

Prior to .NET 8, mocking `DateTime.UtcNow` required custom clock abstractions like:
```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

## Problem

1. **Redundant Abstraction:** Since .NET 8, the BCL provides the official, abstract `System.TimeProvider` class along with `TimeProvider.System` and `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider`.
2. **Domain Clock Anti-Pattern:** Pure domain models should not inject time providers. Timestamps should be passed as parameters into factory methods or domain behaviors (e.g. `Order.Place(DateTimeOffset placedAt)`).

## Decision

**Explicitly reject custom `IClock` or `IDateTimeProvider` abstractions from `EricksonLopez.SharedKernel`.**

Consumers needing time virtualization should utilize the standard BCL `System.TimeProvider`.

## Architectural Placement

`System.TimeProvider` lives in `System` (BCL). It is injected into Application Layer services (e.g., Command Handlers) and passed explicitly to Domain methods when needed.

## Consequences

- **Positive:** Full alignment with modern .NET BCL standards.
- **Positive:** Zero duplicated abstractions or custom mocking libraries needed.
