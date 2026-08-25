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
We compile the `tests/EricksonLopez.SharedKernel.NativeAotTests` and `samples/EricksonLopez.SharedKernel.Sample` projects targeting `net10.0` with `win-x64`/`linux-x64` using `PublishAot=true` in our CI pipeline. 

```bash
dotnet publish tests\EricksonLopez.SharedKernel.NativeAotTests\EricksonLopez.SharedKernel.NativeAotTests.csproj -c Release -r win-x64 -p:PublishAot=true
```

**Result:** The build completes successfully and generates native code without any trimming or dynamic code warnings, verifying that the kernel itself is fully Native AOT compatible on .NET 10.

*Note on Multi-Targeting:* As decided in ADR-009, this library targets `.net8.0`, `.net9.0`, and `.net10.0`. The `<IsAotCompatible>` and `<IsTrimmable>` properties are enabled unconditionally across all supported TFMs in `EricksonLopez.SharedKernel.csproj`, guaranteeing compile-time trimming and AOT verification on all active .NET targets.


## Consequences
- **Positive:** Projects using this SharedKernel can be published as ultra-lightweight Native AOT executables or microservices.
- **Positive:** Zero reflection translates to better runtime performance and lower memory usage.
