# ADR-012: DomainError Omission

## Status
Accepted

## Context
The initial architectural design document proposed a specific `DomainError` type (a `readonly record struct` with code and message fields) to represent domain failures. However, the library utilizes `EricksonLopez.Result` (Tier 0) for its result pattern, which already includes a comprehensive `Error` type (a `sealed record`) that provides:
- `Code`
- `Description`
- `ErrorType` (Failure, Validation, NotFound, etc.)
- `InnerErrors` (for compound errors)

## Decision
We explicitly omitted the creation of a `DomainError` type in the `SharedKernel`.

Instead, all domain logic should directly use `EricksonLopez.Result.Error` to model domain failures. 

## Consequences
- **Positive:** Eliminates semantic duplication between `DomainError` and `Result.Error`.
- **Positive:** Simplifies the API surface; developers only need to understand one unified error type for all failure modes across the application tiers.
- **Negative:** Minor deviation from the initial design document, but aligns perfectly with the actual builder prompt specification.
