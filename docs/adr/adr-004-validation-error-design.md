# ADR-004: Validation Error Design

## Status
Accepted

## Context
We evaluated whether to create a dedicated `ValidationError` type (e.g., inheriting from `Error`) to model validation failures with fields such as `Field`, `AttemptedValue`, and `Message`. 

Currently, validation failures are represented using `Error.Validation(code, description)`, and complex/multiple validation failures are represented by passing child errors to the `InnerErrors` parameter.

## Decision
We will **not** create a dedicated `ValidationError` type.

The official convention for field-level validations is to map the field name into the `Code` property and group them using `InnerErrors`.

**Example:**
```csharp
var error = Error.Validation("User.Invalid", "Validation failed",
    Error.Validation("User.Name.Required", "Name is required"),
    Error.Validation("User.Email.InvalidFormat", "Invalid email format"));
```

We chose this approach to maintain the `Error` type as a `sealed record`. Adding derived types or a weakly-typed `Metadata` dictionary would break the `zero-alloc` lean nature of the `Error` object in the happy paths and simple failure paths.

## Consequences
- **Positive:** `Error` remains a simple, single, and highly optimized `sealed record`.
- **Positive:** Pattern matching and API consistency is maximized (only one `Error` type to handle).
- **Negative:** Consumers cannot easily access strongly-typed properties like `Field` or `AttemptedValue` without parsing the `Code` string.
