# Cookbook — EricksonLopez.SharedKernel

A comprehensive collection of production-ready Domain-Driven Design recipes strictly derived from the public API of `EricksonLopez.SharedKernel`.

---

## Recipe 1 — Modeling Strongly-Typed Entity Identifiers

**Problem:** Prevent accidental parameter transposition (e.g. passing a `CustomerId` into an `OrderId` parameter).

**Solution:** Implement `IStrongId<TSelf, TValue>` using a `readonly record struct`.

```csharp
using EricksonLopez.SharedKernel;

public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>;
public readonly record struct CustomerId(Guid Value) : IStrongId<CustomerId, Guid>;

public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; }

    public Order(OrderId id, CustomerId customerId) : base(id)
    {
        CustomerId = customerId;
    }
}
```

---

## Recipe 2 — Modeling Multi-Attribute Value Objects

**Problem:** Model conceptual wholes defined by attributes rather than an identity, supporting structural equality and immutability.

**Solution:** Inherit from `ValueObject` and optionally decorate with `[ValueObject]`.

```csharp
using EricksonLopez.SharedKernel;

[ValueObject]
public sealed record Address(string Street, string City, string PostalCode) : ValueObject;

[ValueObject]
public sealed record Money(decimal Amount, string Currency) : ValueObject;

// Usage:
var price = new Money(100m, "USD");
var discountedPrice = price with { Amount = 80m }; // Non-destructive mutation
```

---

## Recipe 3 — Aggregate Root with Invariant Factory Method

**Problem:** Ensure that an Aggregate Root is always born in a valid state and automatically records an inception domain event.

**Solution:** Use a private/protected constructor with a static factory method calling `RaiseDomainEvent`.

```csharp
using EricksonLopez.SharedKernel;

public sealed record CustomerRegisteredEvent(CustomerId CustomerId, string Name) : DomainEvent;

public sealed class Customer : AggregateRoot<CustomerId>
{
    public string Name { get; private set; }
    public Address Address { get; private set; }

    public Customer(CustomerId id, string name, Address address) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(address);

        Name = name;
        Address = address;
    }

    public static Customer Register(CustomerId id, string name, Address address)
    {
        var customer = new Customer(id, name, address);
        customer.RaiseDomainEvent(new CustomerRegisteredEvent(id, name));
        return customer;
    }
}
```

---

## Recipe 4 — Emitting Domain Events with UUIDv7 Identifiers

**Problem:** Domain events must be time-ordered for sequential database index locality and audit trails.

**Solution:** Inherit from `DomainEvent` which automatically initializes `Id` (`EventId` type, UUIDv7 on .NET 9+) and `OccurredAt` (UTC). Backward-compat aliases `EventId` (`Guid`) and `OccurredOn` (`DateTimeOffset`) are available.

```csharp
using EricksonLopez.SharedKernel;

public sealed record OrderLineAddedEvent(
    OrderId OrderId,
    string ProductName,
    decimal Price,
    int Quantity
) : DomainEvent;

public sealed class Order : AggregateRoot<OrderId>
{
    public void AddItem(string productName, Money price, int quantity)
    {
        // Business validation...
        RaiseDomainEvent(new OrderLineAddedEvent(Id, productName, price.Amount, quantity));
    }
}
```

---

## Recipe 5 — Testing Entity vs Value Object Equality

**Problem:** Verify that entities compare by Type + ID, while Value Objects compare by all attribute values.

**Solution:** Use standard `==` or `.Equals()` operators.

```csharp
// Entity equality:
var id = new CustomerId(Guid.NewGuid());
var custA = new Customer(id, "Alice", new Address("St 1", "City", "101"));
var custB = new Customer(id, "Alice Updated", new Address("St 2", "Other", "202"));

bool entitiesEqual = (custA == custB); // true — same concrete type and ID

// Value Object equality:
var addrA = new Address("St 1", "City", "101");
var addrB = new Address("St 1", "City", "101");
var addrC = new Address("St 2", "City", "101");

bool voEqual = (addrA == addrB); // true — all attributes match
bool voDifferent = (addrA == addrC); // false — street differs
```

---

## Recipe 6 — Non-Generic Polymorphic Event Dispatching

**Problem:** Infrastructure (Unit of Work, EF Core interceptors, Outbox) needs to collect and clear domain events without knowing the generic `TId` of each aggregate.

**Solution:** Cast the aggregate to `IHasDomainEvents` and call `DrainDomainEvents()`, which atomically snapshots and clears the internal event buffer in a single call.

```csharp
using EricksonLopez.SharedKernel;

public static async Task DispatchEventsAsync(
    IHasDomainEvents aggregate,
    Func<IDomainEvent, Task> publisher)
{
    // DrainDomainEvents() atomically snapshots all pending events
    // and detaches the internal buffer — preventing double-emission
    // on subsequent calls (returns Array.Empty if no events remain).
    var events = aggregate.DrainDomainEvents();

    foreach (var domainEvent in events)
    {
        await publisher(domainEvent);
    }
}
```

> **Note:** Do not separate the snapshot and clear steps manually — `DrainDomainEvents()` is the single atomic operation provided by the API. There is no `ClearDomainEvents()` or `DomainEvents` property.



---

## Recipe 7 — Multi-Bounded Context Autonomous References

**Problem:** Prevent tight coupling and object graph leakage across Bounded Contexts.

**Solution:** Reference foreign aggregates exclusively by their strongly-typed identity.

```csharp
// Sales Bounded Context
public readonly record struct InventorySku(string Value) : IStrongId<InventorySku, string>;

public sealed class SalesOrder : AggregateRoot<OrderId>
{
    public InventorySku ItemSku { get; } // Reference only by ID

    public SalesOrder(OrderId id, InventorySku itemSku) : base(id)
    {
        ItemSku = itemSku;
    }
}
```

---

## Recipe 8 — High-Performance Dapper Batch Operations (PostgreSQL UNNEST)

**Problem:** Execute high-performance batch queries using Strongly-Typed IDs without runtime reflection or boxing.

**Solution:** Extract `.Value` arrays directly for PostgreSQL `UNNEST` / `ANY(@Ids)` queries.

```csharp
using Dapper;
using Npgsql;

public async Task<IReadOnlyList<OrderDto>> GetOrdersByIdsAsync(
    NpgsqlConnection connection,
    IEnumerable<OrderId> orderIds)
{
    var rawGuids = orderIds.Select(id => id.Value).ToArray();

    const string sql = """
        SELECT id, customer_id, total, status
        FROM orders
        WHERE id = ANY(@rawGuids);
        """;

    var results = await connection.QueryAsync<OrderDto>(sql, new { rawGuids });
    return results.ToList();
}
```

---

## Recipe 9 — Custom Strongly-Typed IDs with Diverse Primitive Types

**Problem:** Support diverse database primary key types (integers, strings, 64-bit longs).

**Solution:** Implement `IStrongId<TSelf, TValue>` for the specific primitive.

```csharp
// 64-bit integer identity for high-volume logs / ledger
public readonly record struct TransactionSequence(long Value) : IStrongId<TransactionSequence, long>;

// String-based alphanumeric code
public readonly record struct DepartmentCode(string Value) : IStrongId<DepartmentCode, string>;

// Integer identity for legacy tables
public readonly record struct LegacyId(int Value) : IStrongId<LegacyId, int>;
```

---

## Recipe 10 — Entity Framework Core Value Converters for Strongly-Typed IDs

**Problem:** Persist Strongly-Typed IDs cleanly into relational databases via EF Core.

**Solution:** Configure EF Core `HasConversion` in your `EntityTypeConfiguration`.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
               .HasConversion(id => id.Value, value => new OrderId(value));

        builder.Property(o => o.CustomerId)
               .HasConversion(id => id.Value, value => new CustomerId(value));
    }
}
```

---

## Recipe 11 — Preventing EF Core from Mapping Custom Domain Event Properties

**Problem:** You have added a public property (e.g., `IReadOnlyList<IDomainEvent> DomainEvents { get; }`) to your own aggregate subclass to expose domain event state, and EF Core attempts to map it as a shadow column or navigation property.

**Solution:** Explicitly ignore the property per entity type in `OnModelCreating`.

```csharp
using Microsoft.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Explicitly ignore your own publicly exposed domain event property.
    // Do this for each entity type that exposes domain event state as a public property.
    modelBuilder.Entity<Order>().Ignore(o => o.DomainEvents);
    modelBuilder.Entity<Customer>().Ignore(c => c.DomainEvents);

    // ... rest of model configuration
}
```

Alternatively, use the `IgnoreDomainEvents()` extension as a convention starter:

```csharp
// This is a no-op for the built-in AggregateRoot<TId> (its _domainEvents field
// is private). Use it as a reminder / convention anchor if your team subclasses
// expose domain events as public properties.
modelBuilder.IgnoreDomainEvents();
```

> **When is this needed?**
> The built-in `AggregateRoot<TId>` stores domain events in a `private` field and
> exposes them only via `DrainDomainEvents()` (a method, not a property). EF Core
> does not map methods — so no configuration is required when using the default
> `AggregateRoot<TId>` without custom public event properties.
>
> Only configure explicit `Ignore()` when your aggregate subclass adds a public
> property of type `IReadOnlyList<IDomainEvent>` or similar.

