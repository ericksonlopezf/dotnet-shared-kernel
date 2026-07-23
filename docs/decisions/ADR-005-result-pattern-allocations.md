# ADR-005: Result Pattern Allocations Optimization

## Status
Accepted

## Context
As part of the Type Analysis (Phase 3.1), we analyzed the `Result` and `Result<T>` classes for potential heap allocations on the hot paths (happy path and failure path).
Because `Result` is a `class` and not a `struct` (a deliberate decision to allow inheritance for `Result<T>` and reference semantics), calling `Result.Success()` previously executed `new(true, Error.None)`, causing an allocation for every successful void operation.

## Decision
We optimized the non-generic `Result` by caching a static readonly instance of the success state:

```csharp
private static readonly Result _success = new(true, Error.None);
public static Result Success() => _success;
```

For `Result<T>`, we cannot cache a generic success value because the `TValue` varies per invocation. However, for typical CQS architectures, Command Handlers return the non-generic `Result`, which means the majority of high-throughput mutating operations will now be zero-alloc on the happy path.

## Consequences
- **Positive:** `Result.Success()` is now zero-alloc.
- **Positive:** Reduces GC pressure in high-throughput applications.
- **Negative:** None.
