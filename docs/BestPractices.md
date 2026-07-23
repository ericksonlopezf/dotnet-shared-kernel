# Best Practices

This document outlines the recommended practices when using `EricksonLopez.SharedKernel`.

## 1. Do Not Use Exceptions for Expected Failures
Exceptions are for **exceptional** situations (bugs, infrastructure failures, out of memory). For expected domain failures (e.g., "User not found", "Insufficient funds"), always return a `Result<T>` containing an `Error`.

## 2. Prefer .Match() Over .IsSuccess / .IsFailure
While `.IsSuccess` and `.IsFailure` are available, using `.Match()` forces the consumer to handle both the success and the failure paths. This prevents accidental ignoring of errors.

## 3. Raise Domain Events Only in Aggregates
Do not raise domain events from regular `Entity<TId>` or from application services. Only `AggregateRoot<TId>` has the `RaiseDomainEvent` method because it is the consistency boundary.

## 4. Design Pure Value Objects
Value Objects should be immutable. When a value object needs to change, return a new instance of it. Override `GetEqualityComponents()` to define equality. If a value object is in a critical hot-path, override `Equals` and `GetHashCode` directly to prevent boxing allocations.

## 5. Use Error Types Semantically
Use the provided `ErrorType` enum to convey meaning, not just HTTP status codes.
* `ErrorType.Conflict` for concurrency issues or domain rule violations (e.g. "Username already taken").
* `ErrorType.Validation` for input formatting or simple structural rules.
* `ErrorType.Unexpected` for wrapped exceptions.

## 6. Combine Multiple Results
When you have multiple independent operations that can fail, use `Result.Combine` to execute them all and aggregate their errors, rather than failing on the first one.
