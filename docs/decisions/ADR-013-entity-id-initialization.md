# ADR-013: Entity ID Initialization

## Status
Accepted

## Context
In `Entity<TId>`, the `Id` property is defined as:
```csharp
public TId Id { get; protected init; } = default!;
```
The use of `= default!` suppresses the compiler warning about uninitialized non-nullable properties (`CS8618`). An alternative design would be to require the ID in a constructor and validate it against `EqualityComparer<TId>.Default`.

## Decision
We accept the `= default!` initialization pattern along with the `protected init` setter.

This is a pragmatic trade-off to support ORMs (like Entity Framework Core) that often require a parameterless constructor (or rely on uninitialized object instantiation) to hydrate entities from the database before setting their properties via reflection or `init` accessors.

To mitigate the risk of creating a transient entity with a truly default ID, consumers should provide factory methods or specific constructors in their derived `AggregateRoot` or `Entity` classes that enforce ID assignment.

## Consequences
- **Positive:** Full compatibility with ORMs (EF Core, Dapper) that require property-based hydration.
- **Negative:** It is technically possible for a developer to instantiate a derived entity using a parameterless constructor without providing an ID, resulting in a default ID which might bypass some compile-time nullability guarantees.
