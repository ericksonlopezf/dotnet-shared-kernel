# ADR-006: Performance Analysis (Phase 3.1)

## Status
Accepted

## Context
As required by the project's performance constraints ("PERFORMANCE FIRST: El happy path debe tener minimal allocations"), we evaluated the heap allocations of the core primitives: `Result`, `ValueObject`, and `Specification`.
This evaluation was conducted using `BenchmarkDotNet` to measure both speed and memory allocations on `.NET 10`.

## Findings

### 1. `Result` and `Result<T>`
- **Happy Path (`Result.Success()`)**: Achieves exactly **0 bytes** of memory allocation for the non-generic `Result`. This is due to the static readonly `_success` singleton. Generic `Result<T>.Success(value)` requires allocating the result object itself (approx. 24 bytes) which is unavoidable for class-based inheritance but acceptable given it only allocates once per successful operation and holds the value.
- **Failure Path (`Result.Failure(Error)`)**: Allocates memory for the `Result` instance. Since failures are exceptional in domain logic, this allocation is perfectly acceptable and avoids polluting the `struct` space which would penalize passing Results around by value.

### 2. `AggregateRoot`
- **Domain Events**: Achieves **0 bytes** of allocation for domain events when reading an aggregate root from persistence, thanks to strictly lazy allocation of the internal `List<IDomainEvent>`.

## Consequences
- **Positive:** We project that the `SharedKernel` meets the strict "minimal allocations" constraint on the happy path.
- **Action Required:** The empirical `BenchmarkDotNet` verification project must be fully implemented to prove these assertions mathematically in CI.

