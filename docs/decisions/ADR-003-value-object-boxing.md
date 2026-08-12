# ADR-003: Value Object Boxing Acceptance and Base Class Rejection

## Status
Accepted

## Context
Traditionally, DDD frameworks (like those popularized by Jimmy Bogard or Vladimir Khorikov) include a `ValueObject` base class that uses reflection or an abstract method to provide structural equality:

```csharp
protected abstract IEnumerable<object> GetEqualityComponents();
```

While convenient, this approach introduces a severe performance penalty on the hot path. If a Value Object contains value types (`int`, `decimal`, `DateTime`, `Guid`, etc.), yielding them as `object` forces the .NET runtime to perform **Boxing** (allocating a wrapper object on the heap). 

Consequently, every equality check (`==`, `Equals`, or dictionary lookups) generates memory garbage. In high-throughput or low-latency systems, this unnecessary heap allocation triggers frequent Garbage Collection (GC) cycles, degrading overall application performance.

## Decision
We **strictly reject** providing a `ValueObject` base class in the `SharedKernel`.

Instead, developers **must** leverage modern C# language features (C# 9+) to model Value Objects:

1. **`readonly record struct`**: Should be the default choice for most Value Objects. They provide true zero-allocation on the heap, immutability, and compiler-generated structural equality out of the box.
   ```csharp
   public readonly record struct Money(decimal Amount, string Currency);
   ```
2. **`record`**: For larger Value Objects where passing by reference is more efficient than passing by value (copying the struct).
   ```csharp
   public record Address(string Street, string City, string Country, string ZipCode);
   ```

By using native `record` types, the Roslyn compiler automatically synthesizes the highly optimized `IEquatable<T>` implementation for structural equality without any reflection or boxing.

## Consequences
- **Positive:** Guarantees **Zero-Allocation** and zero reflection for Value Object equality operations.
- **Positive:** Full alignment with **Native AOT** compilation principles.
- **Positive:** Keeps the SharedKernel exceptionally lightweight and native to modern .NET paradigms.
- **Negative:** Developers migrating from legacy DDD codebases will need to adapt their models from `class : ValueObject` to `record` or `readonly record struct`.
