# ADR-013: Entity ID Initialization

## Status
Superseded — Implementation changed; see Consequences below.

## Context
In `Entity<TId>`, the `Id` property is defined as:
```csharp
public TId Id { get; protected init; } = default!;
```
The use of `= default!` suppresses the compiler warning about uninitialized non-nullable properties (`CS8618`). An alternative design would be to require the ID in a constructor and validate it against `EqualityComparer<TId>.Default`.

## Decision
~~We accept the `= default!` initialization pattern along with the `protected init` setter.~~

**Superseded:** The implementation was changed so `Id` is a getter-only property set exclusively through the constructor:

```csharp
public TId Id { get; }  // get-only; assigned in protected Entity(TId id) constructor
```

The constructor validates that `id` is not `default(TId)` and throws `ArgumentException` if so. The `protected init` accessor was removed to prevent derived classes from bypassing the constructor-level validation. The `= default!` suppressor is no longer present.

## Consequences
- **Positive:** Full compatibility with ORMs (EF Core, Dapper) via constructor-based hydration (constructor injection in EF Core 7+ is fully supported).
- **Positive:** Constructor guard eliminates the default-ID footgun that the original `= default!` approach enabled.
- **Negative:** Parameterless constructors in derived types will fail to compile unless they invoke `base(id)` with a valid identifier.
