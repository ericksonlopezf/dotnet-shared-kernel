# 0001: Value Object Boxing Acceptance

## Status
Accepted

## Context
In the `ValueObject` base class, structural equality is determined by overriding `GetEqualityComponents()`, which yields an `IEnumerable<object?>`. This design causes value types (e.g., `int`, `decimal`, `Guid`) to be boxed when yielded, creating memory allocations on the heap.

We debated whether to solve this using a Source Generator (to generate zero-boxing `Equals` methods automatically) or by changing the abstraction.

## Decision
We will **accept** the boxing trade-off for the default implementation and officially document the *escape hatch* pattern for performance-critical scenarios (hot paths).

The official guidance is:
1. Use the default `GetEqualityComponents()` for 95% of Value Objects (simple, easy to write, clean).
2. For Value Objects heavily used in tight loops (hot paths), override `Equals(ValueObject?)` and `GetHashCode()` manually to prevent boxing.

We will not introduce a Source Generator into the `SharedKernel` library because it breaks the "Zero dependencies" rule and adds unnecessary complexity for a problem that rarely impacts typical LOB (Line of Business) applications.

## Consequences
- **Positive:** Keeps the SharedKernel extremely simple and dependency-free.
- **Positive:** Developer experience (DX) is excellent for the common case.
- **Negative:** Minor heap allocations occur when comparing Value Objects with value-type properties using the default method.
