# Memory Allocations & Performance Analysis

This document provides a formal analysis of the memory allocation characteristics of the core types in `EricksonLopez.SharedKernel`.

## 1. `Result` and `Result<T>`

### Happy Path (Success)
- **`Result.Success()`:** Returns a cached static singleton (`_success`). **Zero allocations.**
- **`Result.Success<T>(T value)`:** Allocates a new `Result<T>` instance on the heap (1 object). This is unavoidable for reference types (classes) but is extremely lightweight (only contains a `bool`, `Error`, and `T`).

### Failure Path
- **`Result.Failure(Error error)` / `Result.Failure<T>(Error error)`:** Allocates a new `Result` or `Result<T>` instance. The `Error` object itself is also an allocation, but this is the failure path where allocations are acceptable for rich diagnostic context.

### Extension Methods & Pipeline
- **`Map`, `Bind`, `Match`:** Using these fluent methods on `Result<T>` allocates the delegate if capturing local variables (closures), and allocates a new `Result<TNext>` if the method returns a new result type.
- **`Combine`:** Allocates a `List<Error>` only if there are failures. On the happy path, it only allocates the params array (if not pre-allocated) and a `ReadOnlyCollection` wrapper for values.

## 2. `Error`

The `Error` type is a `sealed record`.

- **Basic Creation (`Error.Validation("Code", "Msg")`)**: Allocates exactly 1 object.
- **InnerErrors Collection (`_innerErrors`)**: 
  - **Happy Path:** By default, simple errors do not allocate any collections. The backing field `_innerErrors` remains `null`.
  - **Accessing `InnerErrors` when null:** Returns `Array.Empty<Error>()` which is a zero-allocation singleton.
  - **With inner errors:** Allocates the params array passed to the factory and stores it.

## 3. `ValueObject`

- **`Equals` and `GetHashCode`**: The default implementation relies on `GetEqualityComponents()` which returns an `IEnumerable<object?>`.
- **Boxing Penalty:** If a component is a value type (`int`, `decimal`, `Guid`, etc.), yielding it as `object?` will **box** the value, causing a heap allocation per comparison.
- **Mitigation:** As documented, for `ValueObject` types on a critical hot-path (e.g., millions of comparisons per second), developers should directly override `Equals(ValueObject? other)` and `GetHashCode()` to perform direct field comparisons without boxing.

## 4. `Specification<T>`

- **`ToExpression()`:** Allocates `Expression` tree nodes. This is expected as it's building a query tree for an ORM (EF Core/Dapper) and should ideally be cached by the consumer or the ORM.
- **`IsSatisfiedBy` (In-Memory Evaluation):** 
  - **Base implementation:** Calls `Expression.Compile()`, which is a very heavy operation (compiles IL at runtime via the JIT). We cache the compiled delegate using a lock to amortize this cost, but the initial compilation allocates significantly.
  - **NativeAOT Compatibility:** The base implementation with `Expression.Compile()` is **not** NativeAOT safe.
  - **Composite Specifications (`And`, `Or`, `Not`):** These override `Evaluate` to call `.IsSatisfiedBy` on their children directly. **Zero allocations** beyond the initial creation of the composite specification.
  - **Leaf Specifications:** Must override `Evaluate` manually to avoid `Expression.Compile()` allocations and to be NativeAOT compatible.

## 5. Domain Events (`AggregateRoot<TId>`)

- **`DomainEvents` property:** Uses a lazy-initialized field (`_readOnlyDomainEvents ??= _domainEvents.AsReadOnly()`).
- **Happy Path:** Before `DomainEvents` is accessed, it only allocates the backing `List<IDomainEvent>` (with capacity 0).
- **Accessing:** Allocates a `ReadOnlyCollection` wrapper exactly once per aggregate instance lifecycle.

## Conclusion

The `SharedKernel` library is highly optimized for the **Happy Path**. A successful operation returning a non-generic `Result` incurs zero allocations. Collections and expensive objects (like `ReadOnlyCollection` for events or `List` for inner errors) are strictly lazy-initialized.
