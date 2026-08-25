# ADR-001: Result Pattern — Explicit Failure over Exceptions

**Status:** Superseded by [ADR-014](ADR-014-removal-of-result-dependency.md)
**Date:** 2026-07-15
**Author:** Erickson Lopez

> [!NOTE]
> The Result pattern described in this ADR was removed from this library in v2.0. See ADR-014 for the decision rationale. This ADR is preserved for historical reference.



## Context

In enterprise applications, operations frequently fail for expected, domain-level reasons:
a user is not found, a payment is declined, a name fails validation.

Two common approaches:

1. **Throw exceptions** — `throw new UserNotFoundException(id)`
2. **Return explicit results** — `return Result.Failure(UserErrors.NotFound(id))`

Exceptions were designed for _unexpected_ failures (programming errors, infrastructure issues).
Using them for business logic creates tight coupling between callers and the exception hierarchy,
makes control flow invisible, and forces callers to use try/catch as a mechanism for branching.

## Decision

This library uses the **Result pattern** (`Result<T>`) for all domain operations.

**Rules:**
- Methods that can fail for expected reasons return `Result<T>`, never throw.
- Only unexpected failures (null reference, argument out of range, database connectivity) are exceptions.
- `Error` is a value type (record) with a `Code` (machine-readable) and `Description` (human-readable).
- Errors are categorized by `ErrorType` to allow mapping to HTTP status codes at the presentation layer.

## Consequences

**Positive:**
- Callers are forced to handle failure at the type level — the compiler enforces it.
- No hidden control flow. All failure paths are visible in signatures.
- Monadic composition (`Map`, `Bind`) enables chaining without nesting.
- Error types map naturally to HTTP responses: `NotFound` → 404, `Validation` → 400, etc.

**Negative:**
- More verbose than throwing exceptions.
- Requires discipline — it's possible to access `.Value` without checking `.IsSuccess` (will throw at runtime).
- Cannot use `async/await` with the same Result cleanly without extension methods (future consideration).

## Alternatives considered

- **OneOf** library — too heavyweight and adds external dependency.
- **FluentResults** — good library but would hide this implementation decision.
- **Throwing exceptions** — rejected for domain logic as explained above.
