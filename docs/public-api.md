# Public API Surface — EricksonLopez.SharedKernel.*

> **Source of truth**: This document is derived exclusively from source code analysis of `src/` at version `3.0.0` (2026-08-25).
> Last analyzed: `EricksonLopez.SharedKernel.slnx` — 7 packable source projects.

---

## Table of Contents

1. [Package Overview](#package-overview)
2. [EricksonLopez.SharedKernel](#package-1-erickssonlopezsaredkernel)
3. [EricksonLopez.SharedKernel.Dapper](#package-2-erickssonlopezsharedkerneldapper)
4. [EricksonLopez.SharedKernel.EntityFrameworkCore](#package-3-erickssonlopezsharedkernelentityframeworkcore)
5. [EricksonLopez.SharedKernel.Json](#package-4-erickssonlopezsharedkerneljson)
6. [EricksonLopez.SharedKernel.OpenTelemetry](#package-5-erickssonlopezsharedkernelopentelemetry)
7. [EricksonLopez.SharedKernel.SourceGenerators](#package-6-erickssonlopezsharedkernelsourcegenerators)
8. [EricksonLopez.SharedKernel.Testing](#package-7-erickssonlopezsharedkerneltesting)
9. [AOT and Trimming Compatibility Matrix](#aot-and-trimming-compatibility-matrix)

---

## Package Overview

| Package | Namespace | Version | Target Frameworks | AOT Safe |
|---|---|---|---|---|
| `EricksonLopez.SharedKernel` | `EricksonLopez.SharedKernel` | `3.0.0` | `net8.0`, `net9.0`, `net10.0` | Yes |
| `EricksonLopez.SharedKernel.Dapper` | `EricksonLopez.SharedKernel.Dapper` | `3.0.0` | `net8.0`, `net9.0`, `net10.0` | Partial |
| `EricksonLopez.SharedKernel.EntityFrameworkCore` | `EricksonLopez.SharedKernel.EntityFrameworkCore` | `3.0.0` | `net8.0`, `net9.0`, `net10.0` | Partial |
| `EricksonLopez.SharedKernel.Json` | `EricksonLopez.SharedKernel.Json` | `3.0.0` | `net8.0`, `net9.0`, `net10.0` | No |
| `EricksonLopez.SharedKernel.OpenTelemetry` | `EricksonLopez.SharedKernel.OpenTelemetry` | `3.0.0` | `net8.0`, `net9.0`, `net10.0` | Yes |
| `EricksonLopez.SharedKernel.SourceGenerators` | `EricksonLopez.SharedKernel.SourceGenerators` | `3.0.0` | `netstandard2.0` | N/A (compile-time) |
| `EricksonLopez.SharedKernel.Testing` | `EricksonLopez.SharedKernel.Testing` | `3.0.0` | `net8.0`, `net9.0`, `net10.0` | Yes |

---

## Package 1: `EricksonLopez.SharedKernel`

**Namespace:** `EricksonLopez.SharedKernel`
**Dependencies:** `EricksonLopez.Events.Contracts`, `EricksonLopez.DomainPrimitives.Abstractions` (Tier-0 Foundation Contracts)
**AOT / Trimming:** `IsAotCompatible=true`, `IsTrimmable=true`

This is the **Tier 0 foundation** package. All other packages in the ecosystem depend on it.

### `IStrongId<TSelf, TValue>`

```csharp
public interface IStrongId<TSelf, TValue> : IEquatable<TSelf>
    where TSelf : notnull, IStrongId<TSelf, TValue>
    where TValue : notnull, IEquatable<TValue>
```

Contract for strongly-typed domain entity identifiers using the Curiously Recurring Template Pattern (CRTP).

| Member | Type | Description |
|---|---|---|
| `Value` | `TValue { get; }` | The underlying primitive value of this identifier |
| `Create(TValue value)` | `static abstract TSelf` | Factory method to construct the identifier from its primitive value |

**Usage:**
```csharp
public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>
{
    public static OrderId Create(Guid value) => new(value);
}
```

---

### `IEntity<TId>`

```csharp
public interface IEntity<TId> where TId : notnull, IEquatable<TId>
```

Core contract for domain entities with a generic identifier.

| Member | Type | Description |
|---|---|---|
| `Id` | `TId { get; }` | The unique identifier of this entity |

---

### `Entity<TId>` (abstract class)

```csharp
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : notnull, IEquatable<TId>
```

Abstract domain entity base class. Identity is defined by a unique identifier; equality is identity-based.

| Member | Signature | Description |
|---|---|---|
| `Id` | `public TId Id { get; }` | Unique identifier; immutable after construction (set via constructor only) |
| Constructor | `protected Entity(TId id)` | Initializes the entity; throws `ArgumentException` if `id` is `default(TId)` |
| `Equals(Entity<TId>? other)` | `public virtual bool` | Identity equality using runtime type (`GetType()`) and `Id` |
| `Equals(object? obj)` | `public override bool` | Delegates to typed overload |
| `GetHashCode()` | `public override int` | `HashCode.Combine(GetType(), EqualityComparer<TId>.Default.GetHashCode(Id))` |
| `operator ==` | `public static bool` | Semantic equality between two entities |
| `operator !=` | `public static bool` | Semantic inequality between two entities |

---

### `IHasDomainEvents`

```csharp
public interface IHasDomainEvents
```

Non-generic contract for domain objects that record domain events. Used by persistence infrastructure for polymorphic event collection.

| Member | Signature | Description |
|---|---|---|
| `DrainDomainEvents()` | `IReadOnlyList<IDomainEvent>` | Atomically snapshots and clears all pending domain events |

---

### `IAggregateRoot`

```csharp
public interface IAggregateRoot : IHasDomainEvents;
```

Marker interface representing an Aggregate Root boundary in DDD. Inherits `IHasDomainEvents` for infrastructure-level polymorphic event draining.

---

### `AggregateRoot<TId>` (abstract class)

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull, IEquatable<TId>
```

Aggregate root base class — the transactional consistency boundary in DDD.

| Member | Signature | Description |
|---|---|---|
| Constructor | `protected AggregateRoot(TId id) : base(id)` | Initializes the aggregate with the specified identifier |
| `DrainDomainEvents()` | `public IReadOnlyList<IDomainEvent>` | Atomically snapshots and clears pending events (lazy: 0 B allocations when no events raised) |
| `RaiseDomainEvent(IDomainEvent)` | `protected void` | Records a domain event inside the aggregate; throws `ArgumentNullException` if null |

**Lazy allocation invariant:** `_domainEvents` list is `null` until the first `RaiseDomainEvent` call. Hydrating an aggregate without raising events produces zero heap allocations.

---

### `DomainEvent` (abstract record)

```csharp
public abstract record DomainEvent : IDomainEvent
```

Abstract record representing a domain event with a time-ordered cryptographic identity.

| Member | Type | Description |
|---|---|---|
| `Id` | `EventId { get; init; }` | UUIDv7 on .NET 9+; `Guid.NewGuid()` on .NET 8. Sequential for B-Tree indexing. Alias: `EventId` property returns `Id.Value` (`Guid`) |
| `OccurredAt` | `DateTimeOffset { get; init; }` | UTC timestamp of event occurrence (defaults to `DateTimeOffset.UtcNow`). Alias: `OccurredOn` |

**Usage:**
```csharp
public sealed record OrderPlacedEvent(OrderId OrderId, decimal Amount) : DomainEvent;
```

---

### `IDomainEventDispatcher`

```csharp
public interface IDomainEventDispatcher
```

Contract for dispatching domain events collected from domain entities. Implemented by infrastructure or decorated by `OpenTelemetryDomainEventDispatcher`.

| Member | Signature | Description |
|---|---|---|
| `DispatchAsync` | `ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)` | Dispatches a batch of domain events in emission order |

---

## Package 2: `EricksonLopez.SharedKernel.Dapper`

**Namespace:** `EricksonLopez.SharedKernel.Dapper`
**Dependencies:** `EricksonLopez.SharedKernel`, `Dapper`, `EricksonLopez.DomainPrimitives`

### `DapperStrongIdRegistry` (static class)

```csharp
public static class DapperStrongIdRegistry
```

Static registry for Dapper `SqlMapper.TypeHandler` registrations. Registration must occur once during application startup/composition.

| Method | AOT Safe | Description |
|---|---|---|
| `Register<TSelf, TValue>()` | Yes | AOT-safe, zero-reflection explicit registration for a single strongly-typed ID |
| `RegisterFromAssembly(Assembly assembly)` | No (IL2026, IL3050) | Reflection-based scanning; registers handlers for all `IStrongId<,>` types in an assembly |
| `RegisterFromAssemblies(params Assembly[] assemblies)` | No (IL2026, IL3050) | Multi-assembly overload of `RegisterFromAssembly` |

**AOT-safe startup registration:**
```csharp
// In Program.cs or DI composition root:
DapperStrongIdRegistry.Register<OrderId, Guid>();
DapperStrongIdRegistry.Register<CustomerId, Guid>();
```

**Reflection-based (non-AOT) bulk registration:**
```csharp
// Only in non-AOT environments:
DapperStrongIdRegistry.RegisterFromAssembly(typeof(OrderId).Assembly);
```

---

### `StrongIdTypeHandler<TSelf, TValue>` (sealed class)

```csharp
public sealed class StrongIdTypeHandler<TSelf, TValue> : SqlMapper.TypeHandler<TSelf>
    where TSelf : notnull, IStrongId<TSelf, TValue>
    where TValue : notnull, IEquatable<TValue>
```

Dapper `TypeHandler` that maps between a strongly-typed domain ID and its underlying database primitive value.

| Method | Description |
|---|---|
| `SetValue(IDbDataParameter parameter, TSelf? value)` | Writes `value.Value` to the parameter; writes `DBNull.Value` for null |
| `Parse(object value)` | Reads the database value and constructs `TSelf` via `TSelf.Create(primitive)`; throws `DataException` on type mismatch or null |

---

## Package 3: `EricksonLopez.SharedKernel.EntityFrameworkCore`

**Namespace:** `EricksonLopez.SharedKernel.EntityFrameworkCore`
**Extension Method Namespace:** `Microsoft.Extensions.DependencyInjection`
**Dependencies:** `EricksonLopez.SharedKernel`, `Microsoft.EntityFrameworkCore`, `EricksonLopez.DomainPrimitives`

### `StrongIdValueConverter<TId, TValue>` (class)

```csharp
public class StrongIdValueConverter<TId, TValue> : ValueConverter<TId, TValue>
    where TId : notnull, IStrongId<TId, TValue>
    where TValue : notnull, IEquatable<TValue>
```

EF Core `ValueConverter` that maps strongly-typed domain IDs to their underlying database primitive column values using zero-reflection, Native AOT-safe lambda expressions.

| Constructor | Description |
|---|---|
| `StrongIdValueConverter()` | Parameterless, AOT-safe. Uses `TId.Create` static interface method |
| `StrongIdValueConverter(ConverterMappingHints? mappingHints)` | AOT-safe with optional EF Core mapping hints |
| `StrongIdValueConverter(Func<TValue, TId> factory, ConverterMappingHints? mappingHints = null)` | Custom factory delegate overload |

---

### `SharedKernelModelConfigurationExtensions` (static class)

Extension methods on `ModelConfigurationBuilder` and `ModelBuilder`.
**Namespace:** `Microsoft.EntityFrameworkCore`

| Method | AOT Safe | Description |
|---|---|---|
| `ConfigureStrongId<TId, TValue>(this ModelConfigurationBuilder)` | Yes | Explicit, AOT-safe registration of a `StrongIdValueConverter<TId, TValue>` |
| `ConfigureStrongIdsFromAssembly(this ModelConfigurationBuilder, Assembly)` | No (IL2026, IL3050) | Reflection-based bulk scanning and converter registration |
| `ConfigureStrongIdsFromAssemblies(this ModelConfigurationBuilder, params Assembly[])` | No (IL2026, IL3050) | Multi-assembly bulk registration overload |
| `IgnoreDomainEvents(this ModelBuilder)` | Yes | Defensive convention — registers `IHasDomainEvents.DrainDomainEvents` as ignored in EF Core model metadata |

**AOT-safe model configuration:**
```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder
        .ConfigureStrongId<OrderId, Guid>()
        .ConfigureStrongId<CustomerId, Guid>();
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.IgnoreDomainEvents();
}
```

---

### `DomainEventsInterceptor` (sealed class)

```csharp
public sealed class DomainEventsInterceptor : SaveChangesInterceptor
```

EF Core `SaveChangesInterceptor` that automatically collects and drains domain events from all tracked `IHasDomainEvents` entities before saving changes.

| Method | Signature | Description |
|---|---|---|
| Constructor | `DomainEventsInterceptor(IDomainEventDispatcher? dispatcher = null)` | Optional dispatcher injection; interceptor drains events even without a dispatcher |
| `SavingChanges` | `override InterceptionResult<int>` | Synchronous path. **Deadlock risk** in `SynchronizationContext`-bearing environments. See ADR-031 |
| `SavingChangesAsync` | `override async ValueTask<InterceptionResult<int>>` | Preferred async path. No deadlock risk. Use `SaveChangesAsync` |
| `CollectAndDrainEvents` | `public static IReadOnlyList<IDomainEvent>` | Utility: collects and drains events from all tracked `IHasDomainEvents` entities in a `DbContext` |

> **WARNING:** Always prefer `SavingChangesAsync` in async EF Core pipelines. The synchronous path blocks on `.GetAwaiter().GetResult()` which risks deadlocks in legacy ASP.NET or Windows Forms environments. See [ADR-031](decisions/ADR-031-sync-dispatcher-policy.md).

---

### `SharedKernelEntityFrameworkServiceCollectionExtensions` (static class)

**Namespace:** `Microsoft.Extensions.DependencyInjection`

| Method | Description |
|---|---|
| `AddSharedKernelDomainEventsInterceptor(this IServiceCollection)` | Registers `DomainEventsInterceptor` as a scoped `ISaveChangesInterceptor` (no dispatcher) |
| `AddSharedKernelDomainEventsInterceptor<TDispatcher>(this IServiceCollection)` | Registers `TDispatcher` as `IDomainEventDispatcher` and `DomainEventsInterceptor` as scoped interceptor |

**DI registration:**
```csharp
services.AddSharedKernelDomainEventsInterceptor<MyMediatorDispatcher>();
```

---

## Package 4: `EricksonLopez.SharedKernel.Json`

**Namespace:** `EricksonLopez.SharedKernel.Json`
**Dependencies:** `EricksonLopez.DomainPrimitives`, `System.Text.Json`
**AOT / Trimming:** No — both types require dynamic code and are annotated `[RequiresDynamicCode]` / `[RequiresUnreferencedCode]`

### `StrongIdJsonConverter<TSelf, TValue>` (sealed class)

```csharp
[RequiresDynamicCode("...")]
[RequiresUnreferencedCode("...")]
public sealed class StrongIdJsonConverter<TSelf, TValue> : JsonConverter<TSelf>
    where TSelf : notnull, IStrongId<TSelf, TValue>
    where TValue : notnull, IEquatable<TValue>
```

`System.Text.Json` converter that serializes and deserializes a strongly-typed ID as its underlying primitive value.

| Method | Description |
|---|---|
| `Read(ref Utf8JsonReader, Type, JsonSerializerOptions)` | Deserializes JSON into `TValue`, then calls `TSelf.Create(primitive)`; throws `JsonException` on null or invalid values |
| `Write(Utf8JsonWriter, TSelf, JsonSerializerOptions)` | Serializes `value.Value` using the underlying `TValue` serializer |

---

### `StrongIdJsonConverterFactory` (sealed class)

```csharp
[RequiresDynamicCode("...")]
[RequiresUnreferencedCode("...")]
public sealed class StrongIdJsonConverterFactory : JsonConverterFactory
```

Dynamically creates `StrongIdJsonConverter<TSelf, TValue>` instances for any type implementing `IStrongId<,>`. Caches converters in a `ConcurrentDictionary<Type, JsonConverter>`.

| Method | Description |
|---|---|
| `CanConvert(Type typeToConvert)` | Returns `true` if `typeToConvert` implements `IStrongId<,>` |
| `CreateConverter(Type typeToConvert, JsonSerializerOptions options)` | Constructs and caches a `StrongIdJsonConverter<TSelf, TValue>` via reflection |

**Registration (non-AOT environments only):**
```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new StrongIdJsonConverterFactory());
```

---

## Package 5: `EricksonLopez.SharedKernel.OpenTelemetry`

**Namespace:** `EricksonLopez.SharedKernel.OpenTelemetry`
**Extension Method Namespace:** `Microsoft.Extensions.DependencyInjection`
**Dependencies:** `EricksonLopez.SharedKernel`, `OpenTelemetry`, `OpenTelemetry.Api`
**AOT / Trimming:** Yes — fully compatible

### `SharedKernelInstrumentation` (static class)

```csharp
public static class SharedKernelInstrumentation
```

Pre-configured OpenTelemetry `ActivitySource` and `Meter` for `EricksonLopez.SharedKernel` domain event dispatching.

| Member | Type | Value / Description |
|---|---|---|
| `ActivitySourceName` | `const string` | `"EricksonLopez.SharedKernel"` |
| `Version` | `const string` | `"1.2.0"` |
| `ActivitySource` | `ActivitySource` | Pre-initialized; used for span creation by `OpenTelemetryDomainEventDispatcher` |
| `Meter` | `Meter` | Pre-initialized; exposes `domain_events.*` metrics |
| `DispatchedEventsCounter` | `Counter<long>` | Metric: `domain_events.dispatched` (unit: `{events}`) |
| `DispatchDurationHistogram` | `Histogram<double>` | Metric: `domain_events.dispatch_duration` (unit: `ms`) |

#### `SharedKernelInstrumentation.Attributes` (nested static class)

| Constant | Value | Description |
|---|---|---|
| `EventId` | `"domain_event.id"` | Span tag for the unique event identifier (UUID) |
| `EventType` | `"domain_event.type"` | Span tag for the CLR type name of the domain event |
| `OccurredAt` | `"domain_event.occurred_at"` | Span tag for the UTC occurrence timestamp |

---

### `OpenTelemetryDomainEventDispatcher` (sealed class)

```csharp
public sealed class OpenTelemetryDomainEventDispatcher : IDomainEventDispatcher
```

Decorator implementing `IDomainEventDispatcher` that wraps an inner dispatcher with OpenTelemetry tracing (per-batch and per-event Activity spans) and metrics collection.

| Member | Signature | Description |
|---|---|---|
| Constructor | `OpenTelemetryDomainEventDispatcher(IDomainEventDispatcher inner)` | Wraps the inner dispatcher; throws `ArgumentNullException` if null |
| `DispatchAsync` | `async ValueTask DispatchAsync(IReadOnlyList<IDomainEvent>, CancellationToken)` | Creates batch span, per-event child spans with semantic tags, records `DispatchedEventsCounter` and `DispatchDurationHistogram`, then delegates to `_inner` |

**Produced span hierarchy:**
```
DomainEvents.DispatchBatch [batch_size=N]
  +-- DomainEvent OrderPlacedEvent [domain_event.id, domain_event.type, domain_event.occurred_at]
  +-- DomainEvent CustomerUpdatedEvent [...]
```

---

### `SharedKernelOpenTelemetryExtensions` (static class)

**Namespace:** `Microsoft.Extensions.DependencyInjection`

| Method | Description |
|---|---|
| `AddSharedKernelInstrumentation(this TracerProviderBuilder)` | Registers `SharedKernelInstrumentation.ActivitySourceName` into the `TracerProvider` |
| `AddSharedKernelInstrumentation(this MeterProviderBuilder)` | Registers `SharedKernelInstrumentation.ActivitySourceName` into the `MeterProvider` |

**OpenTelemetry setup:**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSharedKernelInstrumentation())
    .WithMetrics(metrics => metrics.AddSharedKernelInstrumentation());
```

---

## Package 6: `EricksonLopez.SharedKernel.SourceGenerators`

**Namespace:** `EricksonLopez.SharedKernel.SourceGenerators`
**Type:** Roslyn Incremental Source Generator (compile-time only)
**Target Framework:** `netstandard2.0`
**AOT / Trimming:** N/A — generates source at compile time; does not contribute runtime types

### `StrongIdGenerator` (sealed, `[Generator]`)

```csharp
[Generator]
public sealed class StrongIdGenerator : IIncrementalGenerator
```

Produces Strongly-Typed ID boilerplate for types decorated with `[StrongId]`.

**Generated members per decorated type:**
- `public static TSelf Create(TValue value)` — factory method required by `IStrongId<TSelf, TValue>`
- `IFormattable`, `ISpanFormattable`, `IParsable<TSelf>` implementations
- `ToString()` override
- Implicit/explicit conversion operators

---

### `DapperRegistrationGenerator` (sealed, `[Generator]`)

```csharp
[Generator]
public sealed class DapperRegistrationGenerator : IIncrementalGenerator
```

Scans all `IStrongId<TSelf, TValue>` types at compile time and generates a zero-reflection, 100% Native AOT and Trimming compatible Dapper bulk registration method.

**Trigger:** Assembly or type decorated with `[GenerateDapperStrongIdRegistrations]`.

**Generated output:**
```csharp
// Auto-generated; do not edit.
public static void RegisterAllDapperHandlers()
{
    DapperStrongIdRegistry.Register<OrderId, Guid>();
    DapperStrongIdRegistry.Register<CustomerId, Guid>();
    DapperStrongIdRegistry.Register<ProductId, int>();
}
```

---

## Package 7: `EricksonLopez.SharedKernel.Testing`

**Namespace:** `EricksonLopez.SharedKernel.Testing`
**Dependencies:** `EricksonLopez.SharedKernel`
**AOT / Trimming:** Yes (testing library; not used in production)

### `DomainEventCollector` (sealed class)

```csharp
public sealed class DomainEventCollector
```

In-memory test spy for collecting and asserting domain events emitted by `AggregateRoot<TId>`.

| Member | Signature | Description |
|---|---|---|
| `CollectedEvents` | `IReadOnlyList<IDomainEvent>` | All domain events recorded in order of emission |
| `CollectFrom<TId>(AggregateRoot<TId>)` | `DomainEventCollector` | Drains all pending domain events from the aggregate; returns `this` for chaining |
| `OfType<TEvent>()` | `IEnumerable<TEvent>` | Filters collected events by domain event type |
| `ExpectEvent<TEvent>(Func<TEvent, bool>? predicate = null)` | `TEvent` | Returns the first matching event; throws `InvalidOperationException` if none found |
| `Reset()` | `void` | Clears all collected events |

---

### `AggregateRootTestExtensions` (static class)

```csharp
public static class AggregateRootTestExtensions
```

| Method | Signature | Description |
|---|---|---|
| `CollectEvents<TId>` | `(this AggregateRoot<TId>) -> DomainEventCollector` | Drains all pending events and returns a populated `DomainEventCollector` |

**Test usage pattern (xUnit):**
```csharp
var orderId = new OrderId(Guid.NewGuid());
var order = Order.Place(orderId, 100.00m);

var collector = order.CollectEvents();

var evt = collector.ExpectEvent<OrderPlacedEvent>(e => e.OrderId == orderId);
Assert.Equal(100.00m, evt.Amount);

// Multi-aggregate collection:
var collector2 = new DomainEventCollector()
    .CollectFrom(order)
    .CollectFrom(anotherOrder);
```

---

## AOT and Trimming Compatibility Matrix

| Package | Reflection-requiring APIs | AOT-Safe Alternatives |
|---|---|---|
| `SharedKernel` | None | N/A — all APIs are AOT-safe |
| `Dapper` | `RegisterFromAssembly`, `RegisterFromAssemblies` | `Register<TSelf, TValue>()` or `DapperRegistrationGenerator` source generator |
| `EntityFrameworkCore` | `ConfigureStrongIdsFromAssembly`, `ConfigureStrongIdsFromAssemblies` | `ConfigureStrongId<TId, TValue>()` explicit configuration |
| `Json` | `StrongIdJsonConverter<,>`, `StrongIdJsonConverterFactory` | No AOT-safe alternative provided in this package |
| `OpenTelemetry` | None | N/A — all APIs are AOT-safe |
| `SourceGenerators` | N/A (compile-time) | This package IS the AOT alternative for Dapper |
| `Testing` | None | N/A — test-only, not in production binaries |

> For Native AOT deployments (`PublishAot=true`), avoid all reflection-based registration APIs. Use explicit `Register<TSelf, TValue>()` calls or the `DapperRegistrationGenerator` source generator. See [ADR-007](decisions/ADR-007-native-aot-compatibility.md) for the formal AOT compatibility policy.
