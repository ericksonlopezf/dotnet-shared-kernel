# ADR-014: Removal of EricksonLopez.Result Dependency from SharedKernel

**Date:** 2026-08-11
**Status:** Accepted
**Deciders:** Erickson Lopez
**Context:** FINDING-009 from architectural audit — commit ccd9dd8 removed this dependency without formal documentation.

---

## Context

The original SharedKernel design (v1.0.x) included `EricksonLopez.Result` as a Tier 0 dependency, following the Railway-Oriented Programming pattern (see ADR-001). It was used in `DomainError` construction and as the standard way to return domain operation results.

## Decision

**`EricksonLopez.Result` was removed from `EricksonLopez.SharedKernel` in commit `ccd9dd8`.**

Reasons:
1. **Dependency inversion:** A Tier 1 SharedKernel must have zero dependencies on other packages. Tier 0 should be limited to .NET BCL only. `EricksonLopez.Result` being a separate package creates a transitive dependency that consumers did not request.
2. **Result pattern is application-level:** The Result/Railway pattern is an application concern (Use Cases, Command Handlers). Domain primitives (Entity, AggregateRoot, IDomainEvent) do not need to return Results — they raise domain events and throw domain exceptions for invariant violations.
3. **Package independence:** Consumers who want `EricksonLopez.Result` can take a direct dependency without pulling it through `EricksonLopez.SharedKernel`.
4. **ADR-002 compliance:** ADR-002 mandates "Zero functional dependencies." EricksonLopez.Result was the only non-BCL dependency and its removal brings the SharedKernel into full ADR-002 compliance.

## Consequences

### Positive
- SharedKernel now has **zero non-BCL dependencies** (ADR-002 fully satisfied).
- Reduced transitive dependency graph for all consumers.
- Result pattern remains available as a standalone package.

### Negative
- **BREAKING**: Consumers who relied on `EricksonLopez.Result` being re-exported via `EricksonLopez.SharedKernel` must add an explicit `PackageReference` to `EricksonLopez.Result`.
- ADR-001 (Result Pattern) is contextually impacted — the pattern is still the preferred approach but is no longer a SharedKernel dependency.
- The AOT sample project (`AotConsole`) uses `EricksonLopez.Result` directly and must declare its own PackageReference.

## Migration

Consumers who used `EricksonLopez.Result` via SharedKernel should add:
```xml
<PackageReference Include="EricksonLopez.Result" Version="x.y.z" />
```

---

*Related:* ADR-001, ADR-002, FINDING-009 (Architectural Audit 2026-08-11)
