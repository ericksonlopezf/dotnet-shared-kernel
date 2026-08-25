# ADR-002: Zero Functional Dependencies Exception for netstandard2.0

## Status
**Superseded** by ADR-009 (Target Framework Strategy).

## Context
Originally, the library targeted both `net10.0` and `netstandard2.0`, which required build-time polyfills like `PolySharp`. 

## Decision
As per ADR-009, this library targets supported modern .NET versions (`net8.0`, `net9.0`, `net10.0`). The exceptions for `netstandard2.0` polyfills are no longer applicable. 

Furthermore, per ADR-014, the historical dependency on `EricksonLopez.Result` was completely removed. The strict **Zero Functional Dependencies** rule is now in absolute effect for all third-party NuGet packages.

> **ADR-035 Amendment (August 2026):** The Zero External Dependencies rule applies only to *third-party* NuGet packages. First-party Tier-0 Foundation Contracts (`EricksonLopez.Events.Contracts`, `EricksonLopez.DomainPrimitives.Abstractions`) are explicitly classified as Tier-0 and are permitted as production dependencies of `EricksonLopez.SharedKernel`. See [ADR-035](ADR-035-events-contracts-tier-0-foundation-boundary.md) for rationale.

