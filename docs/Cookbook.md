# Cookbook — EricksonLopez.SharedKernel

A collection of practical recipes derived strictly from the library's public API.

> [!NOTE]
> All recipes use only `Entity<TId>`, `AggregateRoot<TId>`, and `IDomainEvent`.

---

## Recipe 1 — Create a basic Entity

**Problem:** Model a domain object that has identity but is not an Aggregate Root.

**Solution:** Inherit from `Entity<TId>`.

```csharp
using EricksonLopez.SharedKernel;

public sealed class OrderLine : Entity<Guid>
{
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; private set; }

    public OrderLine(Guid id, string productName, decimal unitPrice, int quantity)
    {
        Id = id;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity));
        Quantity = newQuantity;
        // Cannot raise events — delegate to the parent AggregateRoot
    }
}
```

**Good practices:**
- Use `sealed` unless explicit inheritance is needed
- `private` setters to protect invariants
- Entities can have behavior (methods) but do not raise events

**Common mistakes:**
- Using `record` instead of `class` for mutable entities — records promote immutability, entities can change state

---

## Recipe 2 — Create an Aggregate Root with a factory method

**Problem:** Model the entry point of an aggregate that guarantees a valid state at creation.

**Solution:** Inherit from `AggregateRoot<TId>` and use a static factory method.

```csharp
using EricksonLopez.SharedKernel;

public sealed record OrderPlacedEvent(Guid OrderId, string CustomerEmail) : IDomainEvent;

public sealed class Order : AggregateRoot<Guid>
{
    public string CustomerEmail { get; private set; } = string.Empty;
    public DateTime PlacedAt { get; private set; }

    private Order() { } // For EF Core / hydration from DB

    public static Order Place(Guid id, string customerEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);

        var order = new Order
        {
            Id = id,
            CustomerEmail = customerEmail,
            PlacedAt = DateTime.UtcNow
        };

        order.RaiseDomainEvent(new OrderPlacedEvent(id, customerEmail));

        return order;
    }
}
```

**Good practices:**
- Private or parameterless constructor for hydration
- Factory method that validates invariants before creating
- `RaiseDomainEvent` is called AFTER validating and applying state

---

## Recipe 3 — Multiple events in one operation

**Problem:** A business operation produces more than one domain event.

**Solution:** Call `RaiseDomainEvent` multiple times within the same method.

```csharp
public sealed record OrderItemAddedEvent(Guid OrderId, Guid ItemId, int Qty) : IDomainEvent;
public sealed record OrderTotalUpdatedEvent(Guid OrderId, decimal NewTotal) : IDomainEvent;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];
    public decimal Total => _lines.Sum(l => l.UnitPrice * l.Quantity);

    public void AddItem(OrderLine item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _lines.Add(item);

        // Multiple events — the UoW will dispatch all of them
        RaiseDomainEvent(new OrderItemAddedEvent(Id, item.Id, item.Quantity));
        RaiseDomainEvent(new OrderTotalUpdatedEvent(Id, Total));
    }
}
```

**Explanation:** `DomainEvents` maintains insertion order. Infrastructure dispatches in that same order.

---

## Recipe 4 — Verify entity equality

**Problem:** Determine if two in-memory instances represent the same domain entity.

**Solution:** Use the `==` operator or `Equals`.

```csharp
var id = Guid.NewGuid();
var product1 = new Product(id, "Laptop", 1000m);
var product2 = new Product(id, "Updated Laptop", 1200m);

bool isSame = (product1 == product2);    // true — same type + Id
bool isSameE = product1.Equals(product2); // true

// Different types are never equal even if they share an Id:
var customer = new Customer(id, "Alice");
product1.Equals(customer); // false
```

**Note:** The name and price can change. If the `Id` and concrete type are the same, they are the same entity.

---

## Recipe 5 — Detect transient entities

**Problem:** Distinguish between a newly created entity (without an Id) and an existing one (with an Id).

**Solution:** Use `IsTransient()`.

```csharp
var transient = new Product(Guid.Empty, "No Id", 0m);
transient.IsTransient(); // true

var persisted = new Product(Guid.NewGuid(), "With Id", 99m);
persisted.IsTransient(); // false

// Guard clause before persisting:
if (product.IsTransient())
    throw new InvalidOperationException("Assign an Id before persisting.");
```

---

## Recipe 6 — Use entities in HashSet and Dictionary

**Problem:** Use entities in collections that require correct hashing.

**Solution:** `Entity<TId>` implements `GetHashCode()` correctly — compatible with `HashSet<T>` and `Dictionary<TKey, T>`.

```csharp
var id = Guid.NewGuid();
var e1 = new Customer(id, "Bob");
var e2 = new Customer(id, "Bob Alias"); // same Id

// HashSet deduplicates correctly:
var set = new HashSet<Customer> { e1, e2 };
Console.WriteLine(set.Count); // 1 — e2 is not added (same "entity")

// Dictionary:
var dict = new Dictionary<Customer, string> { [e1] = "active" };
Console.WriteLine(dict[e2]); // "active" — e2 accesses e1's entry
```

**Warning:** Do not add transient entities (`Id == default`) to `HashSet` or `Dictionary`. Their hash codes are based on the memory reference, not the Id — correct behavior but may be surprising.

---

## Recipe 7 — Dispatch and clear events (UnitOfWork pattern)

**Problem:** Process domain events generated during a SaveChanges without processing them multiple times.

**Solution:** Access `DomainEvents`, take a snapshot, clear with `ClearDomainEvents()`, then publish.

```csharp
// In your DbContext interceptor / UnitOfWork
public async Task DispatchAndClearAsync(AggregateRoot<Guid> aggregate)
{
    if (!aggregate.DomainEvents.Any())
        return;

    // 1. Snapshot before clearing
    var events = aggregate.DomainEvents.ToList();

    // 2. Clear — prevents re-dispatch on the next cycle
    aggregate.ClearDomainEvents();

    // 3. Publish from the snapshot
    foreach (var domainEvent in events)
    {
        // await _publisher.Publish(domainEvent);
        Console.WriteLine($"Publishing: {domainEvent.GetType().Name}");
    }
}
```

**Good practices:** Clear BEFORE publishing. If publishing fails mid-way, the Outbox Pattern guarantees events are retried from persistence, not from memory.

---

## Recipe 8 — Strongly Typed Id (record struct)

**Problem:** Avoid confusing Ids of different types in method signatures (primitive obsession).

**Solution:** Define a `readonly record struct` for each Id type.

```csharp
using EricksonLopez.SharedKernel;

public readonly record struct OrderId(Guid Value);
public readonly record struct CustomerId(Guid Value);

public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }

    public Order(OrderId id, CustomerId customerId)
    {
        Id = id;
        CustomerId = customerId;
    }
}

// The compiler prevents type mixing:
var orderId = new OrderId(Guid.NewGuid());
var customerId = new CustomerId(Guid.NewGuid());
var order = new Order(orderId, customerId);

// This would not compile:
// new Order(customerId, orderId); // ← compile error ✓
```

**Benefits:** Compile-time safety, zero runtime cost (`record struct` = stack allocation).

---

## Recipe 9 — TId with int type (legacy databases)

**Problem:** Integrate with a database that uses `int` as the primary key.

**Solution:** Use `Entity<int>` or `AggregateRoot<int>`.

```csharp
using EricksonLopez.SharedKernel;

public sealed record ProductCreatedEvent(int ProductId) : IDomainEvent;

public sealed class Product : AggregateRoot<int>
{
    public string Name { get; private set; } = string.Empty;

    public static Product Create(int id, string name)
    {
        var product = new Product { Id = id, Name = name };
        product.RaiseDomainEvent(new ProductCreatedEvent(id));
        return product;
    }
}

var product = Product.Create(42, "Widget");
product.IsTransient(); // false — 42 != default(int) = 0
```

**Note:** The entity is transient when `Id == 0` (default value of `int`). Make sure to assign the Id from the DB before using it in logic.

---

## Recipe 10 — Define multiple domain events for the same aggregate

**Problem:** An AggregateRoot has multiple operations, each producing a different event.

**Solution:** Define one record per event and raise them from each business method.

```csharp
using EricksonLopez.SharedKernel;

// Events of the Order aggregate
public sealed record OrderPlacedEvent(Guid OrderId) : IDomainEvent;
public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent;
public sealed record OrderShippedEvent(Guid OrderId, string TrackingNumber) : IDomainEvent;

public sealed class Order : AggregateRoot<Guid>
{
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    private Order() { }

    public static Order Place(Guid id)
    {
        var order = new Order { Id = id };
        order.RaiseDomainEvent(new OrderPlacedEvent(id));
        return order;
    }

    public void Cancel(string reason)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be cancelled.");
        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be shipped.");
        Status = OrderStatus.Shipped;
        RaiseDomainEvent(new OrderShippedEvent(Id, trackingNumber));
    }
}

public enum OrderStatus { Pending, Shipped, Cancelled }
```

**Good practices:** One event per significant state change. Do not use a single generic "OrderChanged" event with a type enum — it loses expressiveness and makes handlers harder to write.
