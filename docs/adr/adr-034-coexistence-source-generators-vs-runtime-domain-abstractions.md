# ADR-034: Coexistence of Source-Generated Primitives and Runtime Domain Models

## Status
Accepted — August 2026

## Context
The `EricksonLopez.*` ecosystem addresses domain modeling and Primitive Obsession through two distinct mechanisms:
1. Compile-time metaprogramming via Roslyn Source Generators (`EricksonLopez.DomainPrimitives`).
2. Structural runtime building blocks for Domain-Driven Design (`EricksonLopez.SharedKernel`).

The architectural boundary between these two packages was reviewed to determine if they should be consolidated or maintained as independent components.

## Decision
Maintain both packages as independent, loosely-coupled components with clear separation of concerns:

1. **`EricksonLopez.DomainPrimitives` (Compile-Time Tooling)**:
   - Owns Roslyn Source Generators, Analyzers, and CodeFixes.
   - Generates boilerplate for atomic identifiers (`[StrongId]`) and validated scalar wrappers at compile time.
   - Zero runtime overhead and zero third-party dependencies.

2. **`EricksonLopez.SharedKernel` (Runtime DDD Core)**:
   - Owns structural runtime DDD patterns: `Entity<TId>`, `AggregateRoot<TId>`, `DomainEvent`, and `IDomainEvent` lifecycle contracts.
   - Generic constraint for entity identity remains universal: `where TId : notnull, IEquatable<TId>`.
   - Allows seamless consumption of both source-generated identifiers and hand-crafted structs.

## Consequences
- **Positive**: Zero coupling between the Roslyn compiler toolchain and core domain entities. Developers can adopt compile-time generation without pulling in the entire DDD SharedKernel framework.
- **Interoperability**: Binary compatibility is guaranteed via standard `IEquatable<T>` contracts.
