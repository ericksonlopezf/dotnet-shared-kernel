# API Reference — EricksonLopez.SharedKernel

Complete technical reference for the public API surface of `EricksonLopez.SharedKernel`.

**Namespace:** `EricksonLopez.SharedKernel`  
**Assembly:** `EricksonLopez.SharedKernel.dll`  
**Target Frameworks:** `net8.0`, `net9.0`, `net10.0`  
**Trimming & AOT Status:** `IsAotCompatible=true`, `IsTrimmable=true` (Zero Reflection)

---

## Table of Contents

1. [IStrongId<TSelf, TValue>](#istrongidtself-tvalue)
2. [IEntity<TId>](#ientitytid)
3. [Entity<TId>](#entitytid)
4. [IHasDomainEvents](#ihasdomainevents)
5. [IAggregateRoot](#iaggregateroot)
6. [AggregateRoot<TId>](#aggregateroottid)
7. [DomainEvent](#domainevent)
8. [ValueObject](#valueobject)
9. [ValueObjectAttribute](#valueobjectattribute)

---

## `IStrongId<TSelf, TValue>`

Defines a contract for strongly-typed domain entity identifiers using the Curiously Recurring Template Pattern (CRTP).

### Declaration

```csharp
public interface IStrongId<TSelf, TValue> : IEquatable<TSelf>
    where TSelf : notnull, IStrongId<TSelf, TValue>
    where TValue : notnull, IEquatable<TValue>
```

### Type Parameters

- `TSelf`: The concrete identifier type implementing this interface.
- `TValue`: The underlying primitive identity value type (e.g., `Guid`, `long`, `string`, `int`).

### Properties

#### `Value`

```csharp
TValue Value { get; }
```

- **Description:** Gets the underlying primitive value of this identifier.
- **Return Type:** `TValue`

### Remarks

Strongly-Typed IDs eliminate Primitive Obsession by wrapping raw primitive values into domain-specific, compile-time type-safe identifiers, preventing accidental interchange of IDs across different entity types. They are typically implemented as `readonly record struct`s to achieve zero-allocation value semantics and automatic structural equality.

### Code Example

```csharp
public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>;
public readonly record struct CustomerId(Guid Value) : IStrongId<CustomerId, Guid>;

// Compile-time safety prevents accidental ID swaps:
public void Process(OrderId orderId, CustomerId customerId) { ... }
```

---

## `IEntity<TId>`

Defines the core contract for domain entities with a generic identifier.

### Declaration

```csharp
public interface IEntity<TId> where TId : notnull, IEquatable<TId>
```

### Type Parameters

- `TId`: The type of the entity identifier.

### Properties

#### `Id`

```csharp
TId Id { get; }
```

- **Description:** Gets the unique identifier of this entity.
- **Return Type:** `TId`

---

## `Entity<TId>`

Represents an abstract domain entity whose identity is defined by a unique identifier.

### Declaration

```csharp
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : notnull, IEquatable<TId>
```

### Properties

#### `Id`

```csharp
public TId Id { get; }
```

- **Description:** Gets the unique identifier of this entity.
- **Modifier:** Getter-only — set exclusively via constructor, guaranteeing immutability after initialization.

### Constructors

#### `Entity(TId id)`

```csharp
protected Entity(TId id)
```

- **Description:** Initializes a new instance of `Entity<TId>` with the specified identifier.
- **Parameters:**
  - `id`: The unique identifier of the entity.
- **Exceptions:**
  - `ArgumentException`: Thrown if `id` is `null` or `default(TId)`.

### Methods

#### `Equals(Entity<TId>? other)`

```csharp
public virtual bool Equals(Entity<TId>? other)
```

- **Description:** Determines whether the specified entity is equal to the current entity based on concrete runtime type (`GetType()`) and identifier (`Id`).
- **Parameters:** `other` (`Entity<TId>?`): The entity to compare.
- **Return:** `bool` — `true` if runtime types match and `Id`s are equal; otherwise `false`.

#### `Equals(object? obj)`

```csharp
public override bool Equals(object? obj)
```

- **Description:** Override of `object.Equals`. Delegates to `Equals(Entity<TId>?)`.

#### `GetHashCode()`

```csharp
public override int GetHashCode()
```

- **Description:** Calculates the hash code for this entity based on its runtime type and identifier.
- **Return:** `int` — Combined hash code: `HashCode.Combine(GetType(), EqualityComparer<TId>.Default.GetHashCode(Id))`.

### Operators

#### `operator ==`

```csharp
public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
```

- **Description:** Evaluates semantic equality between two entities.

#### `operator !=`

```csharp
public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
```

- **Description:** Evaluates semantic inequality between two entities.

---

## `IHasDomainEvents`

Defines a non-generic contract for domain objects that atomically drain their accumulated domain events.

### Declaration

```csharp
public interface IHasDomainEvents
```

### Methods

#### `DrainDomainEvents()`

```csharp
IReadOnlyList<IDomainEvent> DrainDomainEvents();
```

- **Description:** Transfers and clears all pending domain events recorded by this instance in a single atomic operation. After the call, the aggregate's internal event buffer is reset.
- **Return Type:** `IReadOnlyList<IDomainEvent>` — a read-only snapshot of pending domain events in emission order, or an empty collection if no events were recorded.
- **Remarks:** The operation is atomic: events are snapshotted and the buffer is cleared in one step. Infrastructure (Unit of Work, EF Core interceptors, Outbox processors) should call `DrainDomainEvents()` once per commit — there is no separate `ClearDomainEvents()` or public `DomainEvents` property.

---

## `IAggregateRoot`

Marker contract representing an Aggregate Root in Domain-Driven Design.

### Declaration

```csharp
public interface IAggregateRoot : IHasDomainEvents;
```

### Remarks

Inherits `IHasDomainEvents` to enable non-generic polymorphic event draining by persistence, Unit of Work, and Outbox infrastructure via `DrainDomainEvents()`.

---

## `AggregateRoot<TId>`

Represents an aggregate root — the transactional consistency boundary in Domain-Driven Design.

### Declaration

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull, IEquatable<TId>
```

### Constructors

#### `AggregateRoot(TId id)`

```csharp
protected AggregateRoot(TId id) : base(id)
```

- **Description:** Initializes a new aggregate root with the specified identifier.
- **Exceptions:**
  - `ArgumentException`: Thrown if `id` is `null` or `default(TId)`.

### Methods

#### `RaiseDomainEvent(IDomainEvent domainEvent)`

```csharp
protected void RaiseDomainEvent(IDomainEvent domainEvent)
```

- **Description:** Records a domain event inside the aggregate. Uses lazy initialization for the internal backing list.
- **Parameters:**
  - `domainEvent` (`IDomainEvent`): The domain event to record.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `domainEvent` is `null`.

#### `DrainDomainEvents()`

```csharp
public IReadOnlyList<IDomainEvent> DrainDomainEvents()
```

- **Description:** Atomically snapshots and clears all pending domain events. After the call the internal buffer is reset to `null`.
- **Return:** `IReadOnlyList<IDomainEvent>` — pending events in emission order, or `Array.Empty<IDomainEvent>()` if no events were raised.
- **Remarks:** Implements `IHasDomainEvents.DrainDomainEvents()`. Call once per transactional commit boundary. There is no separate `ClearDomainEvents()` or public `DomainEvents` property.

---

## `DomainEvent`

Abstract record representing a domain event with a time-ordered cryptographic identity.

### Declaration

```csharp
public abstract record DomainEvent
```

### Properties

#### `Id` (Primary)

```csharp
public EventId Id { get; }
```

- **Description:** Gets the unique, time-ordered identifier of the domain event. Type is `EventId` (a `readonly record struct` wrapping `Guid` from `EricksonLopez.Events.Contracts`). Uses UUIDv7 on .NET 9+ for sequential database indexing.
- **Return Type:** `EventId`

#### `OccurredAt` (Primary)

```csharp
public DateTimeOffset OccurredAt { get; }
```

- **Description:** Gets the UTC timestamp at which the domain event occurred.

#### `EventId` (Backward-Compat Alias)

```csharp
public Guid EventId => Id.Value;
```

- **Description:** Gets the underlying `Guid` value of `Id`. Provided as a backward-compatibility alias.

#### `OccurredOn` (Backward-Compat Alias)

```csharp
public DateTimeOffset OccurredOn => OccurredAt;
```

- **Description:** Alias for `OccurredAt`. Provided as a backward-compatibility alias.

### Constructors

#### `DomainEvent()` (Parameterless)

```csharp
protected DomainEvent()
```

- **Description:** Initializes a new instance with a new time-ordered `EventId.New()` and `DateTimeOffset.UtcNow`.

#### `DomainEvent(EventId id, DateTimeOffset occurredAt)`

```csharp
protected DomainEvent(EventId id, DateTimeOffset occurredAt)
```

- **Description:** Initializes with explicit identity and timestamp. Use for deterministic reconstruction.
- **Exceptions:** `ArgumentException` if `id.IsEmpty` or `occurredAt == default`.

#### `DomainEvent(Guid eventId, DateTimeOffset occurredOn)` (Rehydration)

```csharp
protected DomainEvent(Guid eventId, DateTimeOffset occurredOn)
```

- **Description:** Rehydration constructor from raw `Guid` and timestamp (e.g., when deserializing from a database).
- **Exceptions:** `ArgumentException` if `eventId == Guid.Empty` or `occurredOn == default`.

### Code Example

```csharp
public sealed record OrderPlacedEvent(OrderId OrderId, decimal Amount) : DomainEvent;
```

---

## `ValueObject`

Base abstract record for Domain-Driven Design Value Objects.

### Declaration

```csharp
public abstract record ValueObject;
```

### Remarks

A Value Object is an immutable conceptual whole defined entirely by its structural attributes rather than an explicit identity (`Entity<TId>.Id`). Value Objects leverage compiler-generated structural equality (`IEquatable<T>`), deterministic hashing, and `with`-expression non-destructive mutation.

### Code Example

```csharp
public sealed record Address(string Street, string City, string PostalCode) : ValueObject;

var addr1 = new Address("123 Main St", "Springfield", "97477");
var addr2 = addr1 with { City = "Shelbyville" };
```

---

## `ValueObjectAttribute`

Static metadata anchor for Roslyn Source Generators, AOT Mappers, and Trimmer analyzers.

### Declaration

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ValueObjectAttribute : Attribute;
```

### Code Example

```csharp
[ValueObject]
public readonly record struct Money(decimal Amount, string Currency);
```

---

## `SharedKernelModelConfigurationExtensions` (EF Core)

**Package:** `EricksonLopez.SharedKernel.EntityFrameworkCore`  
**Namespace:** `Microsoft.EntityFrameworkCore`

Provides extension methods on `ModelConfigurationBuilder` and `ModelBuilder` to seamlessly configure `IStrongId<TSelf, TValue>` value converters and event draining conventions.

### Methods

- `ConfigureStrongId<TId, TValue>(this ModelConfigurationBuilder)`: Explicit, AOT-safe registration of a `StrongIdValueConverter<TId, TValue>`.
- `ConfigureStrongIdsFromAssembly(this ModelConfigurationBuilder, Assembly)`: Reflection-based bulk scanning and converter configuration across an assembly.
- `ConfigureStrongIdsFromAssemblies(this ModelConfigurationBuilder, params Assembly[])`: Multi-assembly bulk registration.
- `IgnoreDomainEvents(this ModelBuilder)`: Defensive convention registering `IHasDomainEvents.DrainDomainEvents` as ignored in the model metadata.

---

## `DapperStrongIdRegistry` (Dapper)

**Package:** `EricksonLopez.SharedKernel.Dapper`  
**Namespace:** `EricksonLopez.SharedKernel.Dapper`

Static registry for Dapper `SqlMapper.TypeHandler` registrations.

### Methods

- `Register<TSelf, TValue>()`: AOT-safe, zero-reflection registration for an explicit strongly-typed ID.
- `RegisterFromAssembly(Assembly)`: Reflection-based scanning of an assembly to automatically register all concrete `IStrongId<TSelf, TValue>` handlers.
- `RegisterFromAssemblies(params Assembly[])`: Multi-assembly scanning overload.

