# ADR-007: Native AOT Compatibility

## Status
Accepted / Verified

## Context
As part of Phase 3.2 (AOT/Trimming Evaluation), we needed to formally verify that `EricksonLopez.SharedKernel` is completely trim-safe and Native AOT compatible without producing `IL3050` (RequiresDynamicCode) or `IL2026` (RequiresUnreferencedCode) warnings. 

The primary challenge in DDD Shared Kernels regarding AOT is usually the use of reflection (e.g., inside `ValueObject` or `DomainEvents`) and dynamic compilation of `Expression<Func<T, bool>>` within the `Specification` pattern.

## Decision and Implementation
The kernel was architected to be AOT-friendly from the ground up:
- **`ValueObject`**: We explicitly rejected a reflection-based `ValueObject` class (see ADR-003).
- **`Result` and `Error`**: Avoid reflection-heavy serialization/deserialization by being explicit types with no dynamic dictionaries.
- **`Specification<T>`**: We explicitly rejected the `Specification` pattern for this library (see ADR-008) because `Expression.Compile()` throws `IL3050` under AOT. 

## Verification Results
We compile the `EricksonLopez.SharedKernel.AotConsole` sample project targeting `net10.0` with `win-x64` using `PublishAot=true` in our CI pipeline. 

```bash
dotnet publish samples\EricksonLopez.SharedKernel.AotConsole\EricksonLopez.SharedKernel.AotConsole.csproj -c Release -r win-x64
```

**Result:** The build completes successfully and generates native code without any trimming or dynamic code warnings, verifying that the kernel itself is fully Native AOT compatible on .NET 10.

*Note on Multi-Targeting:* As decided in ADR-009, this library also targets `.net8.0` and `.net9.0`. However, the `<IsAotCompatible>` and `<IsTrimmable>` properties are only conditionally applied to the `net10.0` target because the AOT tooling and BCL annotations are most mature there. Native AOT for .NET 8 and 9 is provided as best-effort.

## Consequences
- **Positive:** Projects using this SharedKernel can be published as ultra-lightweight Native AOT executables or microservices.
- **Positive:** Zero reflection translates to better runtime performance and lower memory usage.
