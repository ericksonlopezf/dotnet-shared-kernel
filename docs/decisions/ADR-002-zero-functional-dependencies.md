# ADR-002: Zero Functional Dependencies Exception for netstandard2.0

## Status
**Accepted**

## Context
The `EricksonLopez.SharedKernel` library has a strict non-negotiable architectural rule: **Zero External Dependencies**. The library must not depend on application frameworks, ORMs, or validation libraries (e.g., ASP.NET Core, EF Core, Dapper, MediatR). 

However, the library is required to multi-target both modern .NET (e.g., `net10.0`) and `netstandard2.0` to ensure maximum compatibility with older enterprise systems. Modern C# features (like `init` properties, records, and `required` members) and specific BCL types (like `ValueTask` or `HashCode`) are not natively available in `netstandard2.0`.

## Decision
We modify the architectural rule from "Zero External Dependencies" to **"Zero Functional Dependencies"**.

We allow the inclusion of **compatibility backport packages** (such as `PolySharp`, `System.Threading.Tasks.Extensions`, and `Microsoft.Bcl.HashCode`) under the following strict conditions:
1. They are used **exclusively** for the `netstandard2.0` target via conditional `<PackageReference>`.
2. They do not leak into the public API footprint.
3. The public API surface remains 100% identical across all Target Framework Monikers (TFMs).
4. Modern targets (`net8.0`, `net9.0`, `net10.0`) remain completely free of these dependencies.
5. Build-time only dependencies like `PolySharp` must specify `<PrivateAssets>all</PrivateAssets>` so they do not propagate to consumers.

## Consequences
- **Positive:** We maintain a single codebase using modern C# features while still supporting legacy `netstandard2.0` consumers.
- **Positive:** Modern .NET consumers get a completely dependency-free, lean assembly.
- **Negative:** The `.csproj` file becomes slightly more complex with conditional property groups and package references.
- **Negative:** We must carefully test the library in both modern and legacy contexts to ensure behavior parity.
