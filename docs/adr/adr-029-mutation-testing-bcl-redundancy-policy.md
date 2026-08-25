# ADR-029: Mutation Testing Strategy and BCL Redundancy Policy

**Date:** 2026-08-16  
**Status:** Accepted  
**Deciders:** Erickson Lopez  
**Backlog Reference:** QA-001  
**Alias:** ADR-0029  

---

## Context

Mutation testing via Stryker.NET is employed across `EricksonLopez.SharedKernel` and all adapter packages (`.Dapper`, `.EntityFrameworkCore`, `.Json`) to verify the semantic discrimination power of our test suites.

In high-performance .NET libraries, defensive guard clauses (such as `ArgumentNullException.ThrowIfNull(writer)` or `ArgumentNullException.ThrowIfNull(services)`) protect API boundaries against null inputs before passing references to downstream Base Class Library (BCL) APIs (e.g. `JsonSerializer.Serialize`, `IServiceCollection.AddScoped`).

When Stryker mutates or removes such an explicit guard clause, the downstream BCL method immediately throws the identical `ArgumentNullException`. Because the resulting exception type and behavioral outcome are indistinguishable, the mutant survives despite 100% functional test coverage. Without an explicit policy, developers might resort to suppressing mutation testing entirely or adding fragile, reflection-heavy assertions checking internal parameter names of downstream BCL methods.

## Decision

We establish a formalized **Mutation Testing & BCL Redundancy Policy** across all projects:

1. **Thresholds & Target Mutation Scores:**
   - **High (Target):** 100%
   - **Low (Warning):** 98%
   - **Break Build:** 95%

2. **BCL Redundancy Directives (`// Stryker disable once`):**
   - The use of `// Stryker disable once` is permitted **only** when a mutant is technically undetectable due to redundant downstream BCL behavior.
   - Every suppression directive **must** include an inline technical comment explaining the exact BCL redundancy reason.

```csharp
// Compliant suppression with technical explanation
// Stryker disable once Statement: JsonSerializer.Serialize also throws ArgumentNullException for null writer
ArgumentNullException.ThrowIfNull(writer);
```

3. **Exclusion Whitelist vs. Blacklist:**

| Classification | Category | Description | Policy |
|---|---|---|---|
| **Whitelist** | BCL Redundancy | Guard clauses immediately backed by identical BCL checks | Permitted with inline explanation |
| **Whitelist** | `ConfigureAwait(false)` | Task synchronization context continuation settings | Excluded in `stryker-config.json` |
| **Whitelist** | Native Runtime / Memory | `ILLink.Descriptors.xml` and JIT intrinsic paths | Excluded from mutation analysis |
| **Blacklist** | Domain Invariants | Entity equality, Strongly-Typed ID validation, Event queuing | **Strictly prohibited** from suppression |
| **Blacklist** | State Transitions | Aggregate root state modification, buffer clearance | **Strictly prohibited** from suppression |
| **Blacklist** | Flow Controls | If/else conditions affecting domain or mapping outcomes | **Strictly prohibited** from suppression |

4. **Continuous Verification:**
   - All PRs and release builds must maintain mutation scores $\ge 98\%$.
   - Any new `// Stryker disable` directive introduced without technical justification will be rejected during architectural review.

## Consequences

### Positive
- Prevents artificial test pollution designed solely to kill unkillable BCL-redundant mutants.
- Maintains clarity and precision in mutation test reporting.
- Establishes a transparent, auditable standard for code quality.

### Negative
- Requires developers to document the technical rationale whenever encountering BCL redundancies.

## References
- [Stryker.NET Documentation](https://stryker-mutator.io/docs/stryker-net/configuration/)
- [Microsoft .NET Engineering Guidelines on Guard Clauses](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
