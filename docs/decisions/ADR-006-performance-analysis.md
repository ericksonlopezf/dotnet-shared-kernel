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

### 2. `ValueObject` Boxing
- **Scenario**: When a `ValueObject` contains value-type properties (e.g., `int`, `decimal`, `DateTime`) and implements `GetEqualityComponents()` by yielding them as `IEnumerable<object?>`.
- **Result**: We observed heap allocations (boxing) occurring on each yielded value type during `Equals` or `GetHashCode` calls.
- **Action**: Per ADR 0001, we accept this trade-off for developer experience in the majority of use cases. For hot paths, consumers are instructed to override `Equals` and `GetHashCode` directly, which drops allocations to **0 bytes**.

### 3. `Specification<T>`
- **Scenario**: In-memory evaluation via `Evaluate(T)`.
- **Result**: The initial call to `Compile()` incurs significant allocation and compilation time.
- **Action**: The `_compiledExpression` is now cached using a thread-safe `lock` mechanism, ensuring that subsequent calls for the same specification instance are extremely fast and allocate **0 bytes**.

## Consequences
- **Positive:** We have empirically proven that the `SharedKernel` meets the strict "minimal allocations" constraint on the happy path.
- **Positive:** Known bottlenecks (boxing, expression compilation) are documented with established "escape hatch" patterns.
