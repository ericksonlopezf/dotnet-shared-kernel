# ADR-008: Rejection of Specification Pattern

## Status
Accepted

## Context
The `Specification<T>` pattern typically utilizes `Expression<Func<T, bool>>` to allow compiling expressions for in-memory evaluation and translation to SQL by ORMs.

## Decision
We explicitly exclude `Specification<T>` from `EricksonLopez.SharedKernel`.

The use of `Expression.Compile()` requires generating IL at runtime, which throws an `IL3050` warning and is fundamentally incompatible with Native AOT compilation. Suppressing this warning hides a fatal crash that would occur at runtime in an AOT environment.

## Consequences
- **Positive:** The `SharedKernel` is 100% Native AOT compatible without suppressions.
- **Negative:** Projects requiring the Specification pattern must implement it in a separate, Tier 2 library.
- **Resolution:** The Specification pattern has been extracted to its own dedicated package, `EricksonLopez.Specifications`. Consumers who need this functionality should add a direct `PackageReference` to that library.
