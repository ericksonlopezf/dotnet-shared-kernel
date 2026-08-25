# ADR-009: Target Framework Strategy

## Status
Accepted

## Context
The `EricksonLopez.SharedKernel` library must balance the adoption of modern C# features, AOT constraints, and compatibility with consumer projects.

## Decision
We will target the modern, officially supported Microsoft framework versions concurrently: `.net8.0`, `.net9.0`, and `.net10.0`.

This formally overrides the initial design constraint ("Target Framework: net10.0 exclusivamente"). While targeting exclusively `.net10.0` would simplify the project file and guarantee uniform AOT compliance, dropping `.net8.0` and `.net9.0` would alienate consumers who have not yet migrated to the very latest .NET version. Since `.NET 8.0` is an LTS (Long Term Support) release, it must be supported.

To ensure AOT compliance, `IsAotCompatible` and `IsTrimmable` are conditionally evaluated for `net10.0` during the build where the tooling is most mature.

## Consequences
- **Positive:** Broad compatibility across all active modern .NET applications.
- **Negative:** Slightly more complex `.csproj` with conditional property groups.
