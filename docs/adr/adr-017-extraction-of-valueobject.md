# ADR-017: Extraction of ValueObject to Dedicated Package

**Date:** 2026-08-12
**Status:** Accepted
**Deciders:** Erickson Lopez

## Context

In version 1.x of the `EricksonLopez.SharedKernel`, the `ValueObject` base class was included. This class historically provided a way to define objects whose equality is based on their attributes rather than an identity, usually by implementing equality checks via reflection over the object's properties.

However, modern C# (since C# 9.0) introduced `record` types, which natively provide value-based equality. Furthermore, relying on base classes with dynamic reflection for equality violates the zero-reflection goals of Native AOT compilation (ADR-007). The ecosystem has also shifted towards using Source Generators for defining strongly typed IDs and complex value objects with zero runtime allocation overhead.

## Decision

We have decided to remove the `ValueObject` base class from the core `EricksonLopez.SharedKernel` in version 2.0 and extract it into its own dedicated package: **`EricksonLopez.DomainPrimitives`**.

Reasons:
1. **Separation of Concerns:** The core Shared Kernel is strictly focused on identity-based primitives (`Entity`, `AggregateRoot`) and `IDomainEvent`. 
2. **Native AOT Focus:** Moving `ValueObject` out allows the core Shared Kernel to remain 100% reflection-free and Native AOT compatible without suppressions.
3. **Advanced Tooling:** The new `EricksonLopez.DomainPrimitives` package will focus on modern implementations of value objects, specifically leveraging Roslyn Source Generators to provide zero-allocation `Equals` and `GetHashCode` implementations for strongly typed Ids, which goes beyond the scope of a simple base class.

## Consequences

### Positive
- The `SharedKernel` remains minimal, reflection-free, and AOT-compatible.
- `EricksonLopez.DomainPrimitives` can evolve advanced Source Generator features without bloating the core abstractions.

### Negative
- **BREAKING CHANGE:** Consumers of v1.x who inherited from `ValueObject` will experience broken builds upon upgrading to v2.0.

## Migration

Consumers who used the `ValueObject` base class must add a direct dependency to the new package:

```xml
<PackageReference Include="EricksonLopez.DomainPrimitives" Version="x.y.z" />
```

Alternatively, consumers can migrate their `ValueObject` classes to standard C# `record` types, which provide native value-equality without any base class dependency.
