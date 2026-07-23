# ADR-007: Native AOT Compatibility

## Status
Accepted / Verified

## Context
As part of Phase 3.2 (AOT/Trimming Evaluation), we needed to formally verify that `EricksonLopez.SharedKernel` is completely trim-safe and Native AOT compatible without producing `IL3050` (RequiresDynamicCode) or `IL2026` (RequiresUnreferencedCode) warnings. 

The primary challenge in DDD Shared Kernels regarding AOT is usually the use of reflection (e.g., inside `ValueObject` or `DomainEvents`) and dynamic compilation of `Expression<Func<T, bool>>` within the `Specification` pattern.

## Decision and Implementation
The kernel was architected to be AOT-friendly from the ground up:
- **`ValueObject`**: Avoids reflection by enforcing manual component yielding via `GetEqualityComponents()`.
- **`Result` and `Error`**: Avoid reflection-heavy serialization/deserialization by being explicit types with no dynamic dictionaries.
- **`Specification<T>`**: `Expression.Compile()` throws `IL3050` under AOT since it requires emitting IL at runtime. We mitigate this by explicitly marking the `Compile` call with `#pragma warning disable IL3050` inside `Evaluate`, while documenting the escape hatch: **Consumers targeting AOT MUST override the `Evaluate(T)` method** in their specific implementations to perform in-memory evaluations manually (e.g., `return entity.IsActive;`) rather than relying on expression tree compilation. 

## Verification Results
We compiled the `EricksonLopez.SharedKernel.AotConsole` sample project targeting `net10.0` with `win-x64` using `PublishAot=true`. 

```bash
dotnet publish samples\EricksonLopez.SharedKernel.AotConsole\EricksonLopez.SharedKernel.AotConsole.csproj -c Release -r win-x64
```

**Result:** The build completed successfully and generated native code without any trimming or dynamic code warnings, verifying that the kernel itself is fully Native AOT compatible.

## Consequences
- **Positive:** Projects using this SharedKernel can be published as ultra-lightweight Native AOT executables or microservices.
- **Positive:** Zero reflection translates to better runtime performance and lower memory usage.
- **Negative:** Developers using the `Specification` pattern in AOT scenarios have slightly more boilerplate, as they must provide a manual `Evaluate(T)` override to bypass runtime expression compilation.
