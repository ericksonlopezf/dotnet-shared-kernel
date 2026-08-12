// ═══════════════════════════════════════════════════════════════════════════
// EricksonLopez.SharedKernel — Official Showcase
// ═══════════════════════════════════════════════════════════════════════════
// This project is the reference implementation of the library.
// It contains progressive examples covering the entire public API.
//
// Public API (v1.1.0):
//   • Entity<TId>          — domain entity with identity-based equality
//   • AggregateRoot<TId>   — consistency boundary + domain events
//   • IDomainEvent         — marker interface for domain events
//
// Requirements: .NET 10+ (net10.0). Compatible with NativeAOT and Trimming.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using EricksonLopez.SharedKernel;

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║         EricksonLopez.SharedKernel — Official Showcase       ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 0 — Conceptual
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 0 — Conceptual");

Console.WriteLine("""
  What is the library?
  ────────────────────
  EricksonLopez.SharedKernel is a Shared Kernel for .NET applications
  based on Domain-Driven Design (DDD). It provides the fundamental building blocks
  of the domain: entity identity, transactional consistency boundary,
  and domain events contract.

  What problem does it solve?
  ───────────────────────
  In systems with multiple microservices or modules, the same abstractions
  (Entity, AggregateRoot, IDomainEvent) are redefined in each project
  inconsistently. This library standardizes the domain core by eliminating
  that duplication and guaranteeing DDD architectural invariants.

  Why does it exist?
  ────────────────
  To ensure that ONLY aggregate roots (AggregateRoot<TId>) can
  emit domain events — the most important invariant of DDD — and that the
  identity of entities is immutable by design, not by convention.

  Advantages:
  ─────────
  ✔ Zero dependencies — only the .NET runtime
  ✔ NativeAOT compatible (IsAotCompatible=true in all TFMs)
  ✔ Trimming compatible (IsTrimmable=true in all TFMs)
  ✔ Multi-TFM: net8.0 / net9.0 / net10.0
  ✔ Zero allocation on read-only hydration of aggregates
  ✔ Lazy allocation of the domain events collection
  ✔ Semantic equality by concrete type + Id (not by reference)
  ✔ Overloaded == and != operators

  Disadvantages / Intentionally limited scope:
  ────────────────────────────────────────────────
  ✗ Does not include Result Pattern, ValueObject, Specification
  ✗ Does not include UnitOfWork, Repositories, Outbox
  ✗ Does not include event dispatching infrastructure
  These patterns belong to the Infrastructure layer, not the domain core.

  Comparison with alternatives:
  ─────────────────────────────
  • Ardalis.SharedKernel   — more complete but with external dependencies
  • MediatR domain events  — coupled with MediatR
  • This library           — minimalist, zero-dep, AOT-first, pure DDD
""");

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 1 — Quick Start
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 1 — Quick Start");

Console.WriteLine("  Installation:");
Console.WriteLine("    dotnet add package EricksonLopez.SharedKernel");
Console.WriteLine();
Console.WriteLine("  Minimal Configuration:");
Console.WriteLine("    No configuration required. No dependency injection required.");
Console.WriteLine("    The library is just base abstractions — no IoC, no middleware.");
Console.WriteLine();
Console.WriteLine("  First functional use:");

// Create an entity with Guid as Id (the most common case)
var productId = Guid.NewGuid();
var product = new Product(productId, "Laptop Pro", 1299.99m);

Console.WriteLine($"    Product created → Id: {product.Id}, Name: {product.Name}, Price: {product.Price:C}");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 2 — Complete Configuration / API Explorer
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 2 — Complete Configuration / API Explorer");

Console.WriteLine("  The library has no options, builders, or configuration extensions.");
Console.WriteLine("  It is 'plug-and-play'. Everything is based on inheritance from Entity<TId>");
Console.WriteLine("  or AggregateRoot<TId>.");
Console.WriteLine();
Console.WriteLine("  Public members of Entity<TId>:");
Console.WriteLine("    • Id                          — immutable identity (protected init)");
Console.WriteLine("    • IsTransient()               — detects if Id is the default value");
Console.WriteLine("    • Equals(Entity<TId>?)        — semantic equality");
Console.WriteLine("    • Equals(object?)             — object.Equals override");
Console.WriteLine("    • GetHashCode()               — hash by concrete type + Id");
Console.WriteLine("    • operator ==                 — delegates to Equals");
Console.WriteLine("    • operator !=                 — negation of ==");
Console.WriteLine();
Console.WriteLine("  Public members of AggregateRoot<TId> (inherits all from Entity):");
Console.WriteLine("    • DomainEvents                — IReadOnlyCollection<IDomainEvent> (lazy)");
Console.WriteLine("    • RaiseDomainEvent(event)     — protected: only the aggregate itself can call it");
Console.WriteLine("    • ClearDomainEvents()         — public: for the infrastructure layer");
Console.WriteLine();
Console.WriteLine("  TId Constraint: notnull, IEquatable<TId>");
Console.WriteLine("  Supports: Guid, int, string, long, record struct (Strongly Typed Id)");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 3 — Real Use Cases
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 3 — Real Use Cases");

// ── Case 3a: AggregateRoot raising domain events ──
Console.WriteLine("  [3a] AggregateRoot → RaiseDomainEvent → DomainEvents");

var orderId = Guid.NewGuid();
var order = Order.Place(orderId, "Laptop Pro x2");

Console.WriteLine($"    Order created → Id: {order.Id}");
Console.WriteLine($"    Pending events before persisting: {order.DomainEvents.Count}");

foreach (var domainEvent in order.DomainEvents)
    Console.WriteLine($"      → {domainEvent.GetType().Name}");

// The infrastructure (UnitOfWork) would: read events → persist → publish → clear
order.ClearDomainEvents();
Console.WriteLine($"    Events after ClearDomainEvents (post-commit): {order.DomainEvents.Count}");
Console.WriteLine();

// ── Case 3b: Semantic equality by Id ──
Console.WriteLine("  [3b] Semantic equality — two instances, same Id");

var sharedId = Guid.NewGuid();
var customerA = new Customer(sharedId, "Alice");
var customerB = new Customer(sharedId, "Alice (clone)");

// Although the name differs, equality is based ONLY on concrete type + Id
Console.WriteLine($"    customerA.Id == customerB.Id : {customerA.Id == customerB.Id}");
Console.WriteLine($"    customerA == customerB        : {customerA == customerB}");         // true
Console.WriteLine($"    customerA != customerB        : {customerA != customerB}");         // false
Console.WriteLine($"    customerA.Equals(customerB)   : {customerA.Equals(customerB)}");   // true
Console.WriteLine();

// ── Case 3c: Different types are not equal even if they share Id ──
Console.WriteLine("  [3c] Different types are never equal (even if they share Id)");

var differentTypeId = Guid.NewGuid();
var prod = new Product(differentTypeId, "Item", 0m);
var cust = new Customer(differentTypeId, "Item");

Console.WriteLine($"    Product.Id == Customer.Id     : {prod.Id == cust.Id}");      // true (Guid == Guid)
Console.WriteLine($"    product.Equals(customer)      : {prod.Equals(cust)}");       // false — different type
Console.WriteLine();

// ── Case 3d: IsTransient ──
Console.WriteLine("  [3d] IsTransient — entity without persistent Id");

var transientProduct = new Product(Guid.Empty, "No Id", 0m); // Id = Guid.Empty = default
var persistedProduct = new Product(Guid.NewGuid(), "With Id", 50m);

Console.WriteLine($"    transientProduct.IsTransient() : {transientProduct.IsTransient()}");   // true
Console.WriteLine($"    persistedProduct.IsTransient() : {persistedProduct.IsTransient()}");   // false

// Two transient entities are NEVER equal to each other
var transient1 = new Product(Guid.Empty, "A", 0m);
var transient2 = new Product(Guid.Empty, "B", 0m);
Console.WriteLine($"    transient1 == transient2       : {transient1 == transient2}");  // false — DDD invariant
Console.WriteLine();

// ── Case 3e: GetHashCode ──
Console.WriteLine("  [3e] GetHashCode — for use in collections (HashSet, Dictionary)");

var hashId = Guid.NewGuid();
var e1 = new Customer(hashId, "Bob");
var e2 = new Customer(hashId, "Bob Alias");

Console.WriteLine($"    e1.GetHashCode() == e2.GetHashCode() : {e1.GetHashCode() == e2.GetHashCode()}"); // true
Console.WriteLine($"    HashSet can deduplicate by Id:");

var set = new HashSet<Customer> { e1, e2 }; // e2 is not added, same hash + equals
Console.WriteLine($"      HashSet.Count = {set.Count} (expected: 1)");
Console.WriteLine();

// ── Case 3f: == and != operators with null ──
Console.WriteLine("  [3f] == and != operators with null");

Customer? nullCustomer = null;
Customer someCustomer = new Customer(Guid.NewGuid(), "Charlie");

Console.WriteLine($"    null == null                   : {nullCustomer == null}");    // true
Console.WriteLine($"    someCustomer == null           : {someCustomer == null}");    // false
Console.WriteLine($"    someCustomer != null           : {someCustomer != null}");    // true
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 4 — Advanced Integration (Transaction Boundaries)
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 4 — Advanced Integration (Transaction Boundaries)");

Console.WriteLine("""
  The library provides the contracts; the infrastructure implements the dispatch.
  The standard integration pattern with Unit of Work is:

    1. Application Layer executes the use case on the AggregateRoot
    2. AggregateRoot validates invariants and calls RaiseDomainEvent(...)
    3. Application Layer calls UnitOfWork.SaveChangesAsync()
    4. UnitOfWork persists the state in the database
    5. UnitOfWork reads AggregateRoot.DomainEvents
    6. UnitOfWork calls AggregateRoot.ClearDomainEvents()
    7. UnitOfWork publishes each event via Publisher / Mediator / Outbox
    8. If publication fails, it can be retried from the Outbox

  Documented GAP: The library does not provide UnitOfWork, Repositories, or Outbox.
  These components belong to the Infrastructure layer of the consuming project.
""");

// Demonstration of the contract from the domain side:
Console.WriteLine("  Demonstration of the contract from the domain:");

var invoice = Invoice.Create(Guid.NewGuid(), 2500m);
Console.WriteLine($"    Invoice created. Pending events: {invoice.DomainEvents.Count}");

// Simulate what the infrastructure UnitOfWork would do:
var eventSnapshot = new List<IDomainEvent>(invoice.DomainEvents); // read before clearing
invoice.ClearDomainEvents();                                        // clear to avoid re-dispatching

Console.WriteLine($"    UoW took snapshot: {eventSnapshot.Count} event(s)");
Console.WriteLine($"    UoW cleared the aggregate: {invoice.DomainEvents.Count} pending event(s)");
Console.WriteLine($"    UoW would publish: {eventSnapshot[0].GetType().Name}");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 5 — Processing
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 5 — Processing");

Console.WriteLine("""
  Documented GAP: The library does not include background processing, batch
  processing, schedulers, or concurrency. It is restricted to the domain core.

  What the library DOES offer for processing:
  ────────────────────────────────────────────────────
  • ClearDomainEvents() — idempotent operation: safe to call multiple
    times. If there are no events, it does nothing (no exception, no alloc).

  • DomainEvents — IReadOnlyCollection<IDomainEvent>:
    The collection is read-only. The infrastructure layer CANNOT
    modify it directly; it MUST use ClearDomainEvents().

  Thread-safety design (by design, NOT thread-safe):
  ──────────────────────────────────────────────────
  AggregateRoot is deliberately NOT thread-safe. The command handler
  of the application layer must guarantee exclusive access before
  mutating the aggregate. This is the standard DDD contract.
""");

// Demonstration of idempotent ClearDomainEvents:
var freshAggregate = new Order { };
freshAggregate.ClearDomainEvents(); // no previous events — does not throw exception
Console.WriteLine($"  ClearDomainEvents() on aggregate without events: no exception ✓");
Console.WriteLine($"  DomainEvents.Count = {freshAggregate.DomainEvents.Count}");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 6 — Error Handling
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 6 — Error Handling");

Console.WriteLine("""
  Documented GAP: The library does not provide Result Pattern, retry policies,
  dead letters, or backoff. It was an explicit design decision (ADR-014)
  to maintain zero dependencies and pure domain.

  What the library DOES offer for error handling:
  ────────────────────────────────────────────────────────
  • ArgumentNullException.ThrowIfNull() in RaiseDomainEvent(null):
    If an attempt is made to raise a null event, the AggregateRoot throws
    ArgumentNullException immediately.

  • IsTransient() as guard clause:
    Command handlers can verify if an aggregate is transient
    before operating on it.

  Recommended error handling strategies for the consumer:
  ─────────────────────────────────────────────────────────────────
  • For expected domain errors → implement Result<T> in your project
  • For retry / dead letter → Polly + Outbox infrastructure
  • For validation → use IsTransient() as a precondition
""");

// Demonstration of null protection in RaiseDomainEvent:
try
{
    var badAggregate = new BrokenAggregate(Guid.NewGuid());
    badAggregate.TryRaiseNull();
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"  RaiseDomainEvent(null) → ArgumentNullException: '{ex.ParamName}'");
}
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 7 — Scalability
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 7 — Scalability");

Console.WriteLine("""
  Documented GAP: The library does not provide horizontal scalability
  mechanisms (sharding, partitioning, distributed locking).

  What the library DOES offer for scalability:
  ────────────────────────────────────────────────────
  • NativeAOT compatible → startup in milliseconds in containers
    (critical for scale-to-zero in Kubernetes / Azure Container Apps)

  • Zero allocation on read-only hydration:
    Reading an AggregateRoot from the database without calling any
    business method produces ZERO bytes of heap allocation for the
    _domainEvents collection (lazy initialization).

  • Deterministic hashing:
    GetHashCode() produces deterministic values by type + Id,
    allowing correct use in ConcurrentDictionary, MemoryCache,
    and parallel collections.

  • Trimming compatible → minimal bundles in production
""");

// Demonstration of lazy allocation (zero alloc on read):
var hydratedAggregate = new Order(); // simulates hydration from DB without state changes
Console.WriteLine($"  Hydrated aggregate without state changes:");
Console.WriteLine($"    DomainEvents.Count = {hydratedAggregate.DomainEvents.Count} (zero alloc)");
Console.WriteLine($"    DomainEvents == Array.Empty? : {ReferenceEquals(hydratedAggregate.DomainEvents, Array.Empty<IDomainEvent>())} ✓");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 8 — Customization (Custom TId)
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 8 — Customization (Custom TId)");

Console.WriteLine("  The library supports any type as TId that satisfies");
Console.WriteLine("  the constraint: notnull, IEquatable<TId>");
Console.WriteLine();

// ── 8a: TId = Guid (the most common) ──
var guidEntity = new Product(Guid.NewGuid(), "Widget", 9.99m);
Console.WriteLine($"  [8a] TId = Guid       → {guidEntity.Id}");

// ── 8b: TId = int ──
var intEntity = new IntIdProduct(42, "Basic Widget", 4.99m);
Console.WriteLine($"  [8b] TId = int        → {intEntity.Id}");

// ── 8c: TId = string ──
var stringEntity = new StringIdCustomer("USR-ALPHA-001", "Dave");
Console.WriteLine($"  [8c] TId = string     → {stringEntity.Id}");

// ── 8d: TId = long ──
var longEntity = new LongIdOrder(9_999_999_999L);
Console.WriteLine($"  [8d] TId = long       → {longEntity.Id}");

// ── 8e: TId = record struct (Strongly Typed Id) — recommended pattern ──
var strongId = new OrderId(Guid.NewGuid());
var stronglyTypedOrder = new StronglyTypedOrder(strongId, "Premium Order");
Console.WriteLine($"  [8e] TId = OrderId (record struct) → {stronglyTypedOrder.Id}");
Console.WriteLine($"       IsTransient (default OrderId): {new StronglyTypedOrder(default, "X").IsTransient()}");
Console.WriteLine();

// ── 8f: Equality with Strongly Typed Id ──
var sid = new OrderId(Guid.NewGuid());
var sto1 = new StronglyTypedOrder(sid, "Alpha");
var sto2 = new StronglyTypedOrder(sid, "Beta");
Console.WriteLine($"  [8f] StronglyTyped equality: sto1 == sto2 = {sto1 == sto2}"); // true
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 9 — Official Extensions / Integrations
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 9 — Official Extensions / Integrations");

Console.WriteLine("""
  Documented GAP: The library has no official integration packages
  with brokers (RabbitMQ, Kafka, Azure Service Bus) or ORMs (EF Core).

  Documented integration patterns (to be implemented by the consumer):
  ───────────────────────────────────────────────────────────────────────

  With Entity Framework Core:
    • Configure the Id setter as protected in the entity and use
      EF Core Value Converters if Strongly Typed Ids are used
    • Use .HasKey(e => e.Id) in the ModelBuilder
    • For lazy loading proxies: EF Core creates dynamic subclasses.
      Entity<TId> equality uses GetType() (not proxy-unwrapping)
      because proxy-unwrapping is the responsibility of the infrastructure.
      → See ADR-015 in docs/decisions/

  With MediatR (domain events dispatch):
    • After SaveChangesAsync(), iterate AggregateRoot.DomainEvents,
      publish each via IPublisher.Publish(), then ClearDomainEvents()

  With Outbox Pattern:
    • Serialize DomainEvents to the Outbox table before SaveChangesAsync()
    • The worker reads the Outbox table and publishes with guarantee
""");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 10 — Enterprise Architecture
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 10 — Enterprise Architecture");

Console.WriteLine("""
  This library is designed to sit at the absolute core of the system
  (Domain Layer), without depending on any external framework.

  Compatible architectures:
  ──────────────────────────
  ✔ Clean Architecture  — lives in the innermost circle (Domain)
  ✔ Hexagonal / Ports & Adapters — core of the hexagon
  ✔ Onion Architecture — Domain layer (no outward dependencies)
  ✔ CQRS               — Commands mutate AggregateRoots and raise events;
                          Queries do not need AggregateRoot (can use DTOs)
  ✔ Event Sourcing      — IDomainEvent is compatible with the event interface;
                          the Event Store is the responsibility of infrastructure

  Dependency rules in Clean Architecture:
  ─────────────────────────────────────────────
  [External] → [Infrastructure] → [Application] → [Domain (this library)]
                                                         ↑
                                               No outgoing arrows

  Tips for multi-module enterprise solutions:
  ────────────────────────────────────────────────────
  • Share this library via NuGet (not via direct Project Reference)
    between Bounded Contexts to maintain decoupling
  • Each Bounded Context can have its own Aggregates inheriting
    from this common base
  • DO NOT use AggregateRoot from one BC directly in another BC —
    communicate via IDomainEvent (event translation between contexts)
""");

// Demonstration of multiple Bounded Contexts sharing the same base:
var inventoryItem = new InventoryItem(Guid.NewGuid(), "SKU-001", 100);
var salesOrder    = new SalesAggregate(Guid.NewGuid(), inventoryItem.Id);

Console.WriteLine($"  Inventory BC → InventoryItem.Id : {inventoryItem.Id}");
Console.WriteLine($"  Sales BC     → SalesOrder references InventoryItemId: {salesOrder.InventoryItemId}");
Console.WriteLine($"  Are they the same entity? {inventoryItem.Id == salesOrder.InventoryItemId} (same Id reference, different types)");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║             ✅ Showcase executed successfully                ║");
Console.WriteLine("║             All levels 0-10 completed                        ║");
Console.WriteLine("║             All API members demonstrated                     ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

// ═══════════════════════════════════════════════════════════════════════════
// SHOWCASE TYPES
// They only use public inventory APIs: Entity<TId>, AggregateRoot<TId>, IDomainEvent
// ═══════════════════════════════════════════════════════════════════════════

// ── Showcase Entities ──────────────────────────────────────────────────────

/// <summary>Domain entity with TId = Guid.</summary>
sealed class Product : Entity<Guid>
{
    public string Name { get; }
    public decimal Price { get; }

    public Product(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}

/// <summary>Entity with TId = Guid for the customer domain.</summary>
sealed class Customer : Entity<Guid>
{
    public string Name { get; }

    public Customer(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

/// <summary>Entity with TId = int — demonstrates primitive type support.</summary>
sealed class IntIdProduct : Entity<int>
{
    public string Name { get; }
    public decimal Price { get; }

    public IntIdProduct(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}

/// <summary>Entity with TId = string.</summary>
sealed class StringIdCustomer : Entity<string>
{
    public string Name { get; }

    public StringIdCustomer(string id, string name)
    {
        Id = id;
        Name = name;
    }
}

/// <summary>Entity with TId = long.</summary>
sealed class LongIdOrder : Entity<long>
{
    public LongIdOrder(long id)
    {
        Id = id;
    }
}

/// <summary>
/// Strongly Typed Id — recommended pattern to avoid primitive obsession.
/// Must implement IEquatable(T) (fulfilled by record struct automatically).
/// </summary>
readonly record struct OrderId(Guid Value);

/// <summary>Entity with Strongly Typed Id (record struct).</summary>
sealed class StronglyTypedOrder : Entity<OrderId>
{
    public string Description { get; }

    public StronglyTypedOrder(OrderId id, string description)
    {
        Id = id;
        Description = description;
    }
}

// ── Showcase Aggregate Roots ───────────────────────────────────────────────

/// <summary>
/// Domain event: Order placed.
/// Domain events should be records (immutable by design).
/// </summary>
sealed record OrderPlacedEvent(Guid OrderId, string Description, DateTime PlacedAt) : IDomainEvent;

/// <summary>Domain event: item line added to an Order.</summary>
sealed record OrderItemAddedEvent(Guid OrderId, string ItemName) : IDomainEvent;

/// <summary>
/// Aggregate Root of the orders domain.
/// Demonstrates: RaiseDomainEvent, DomainEvents, ClearDomainEvents, factory method.
/// </summary>
sealed class Order : AggregateRoot<Guid>
{
    public string Description { get; private set; } = string.Empty;
    public DateTime PlacedAt { get; private set; }

    // Parameterless constructor for hydration (e.g., EF Core, tests)
    public Order() { }

    /// <summary>
    /// Factory method — recommended pattern in DDD.
    /// Guarantees that the aggregate always is born in a valid state
    /// and that events are raised at the correct time.
    /// </summary>
    public static Order Place(Guid id, string description)
    {
        var order = new Order
        {
            Id = id,
            Description = description,
            PlacedAt = DateTime.UtcNow
        };

        // RaiseDomainEvent is protected — only the aggregate itself can call it
        order.RaiseDomainEvent(new OrderPlacedEvent(id, description, order.PlacedAt));

        return order;
    }

    /// <summary>Adds an item to the order and raises the corresponding event.</summary>
    public void AddItem(string itemName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);

        // Actual business logic would go here (add to internal collection, etc.)
        RaiseDomainEvent(new OrderItemAddedEvent(Id, itemName));
    }
}

/// <summary>Domain event: invoice created.</summary>
sealed record InvoiceCreatedEvent(Guid InvoiceId, decimal Amount) : IDomainEvent;

/// <summary>Aggregate Root of the billing domain.</summary>
sealed class Invoice : AggregateRoot<Guid>
{
    public decimal Amount { get; private set; }

    private Invoice() { }

    public static Invoice Create(Guid id, decimal amount)
    {
        var invoice = new Invoice { Id = id, Amount = amount };
        invoice.RaiseDomainEvent(new InvoiceCreatedEvent(id, amount));
        return invoice;
    }
}

/// <summary>
/// Auxiliary aggregate to demonstrate the ArgumentNullException guard
/// in RaiseDomainEvent(null).
/// </summary>
sealed class BrokenAggregate : AggregateRoot<Guid>
{
    public BrokenAggregate(Guid id) => Id = id;

    public void TryRaiseNull() => RaiseDomainEvent(null!);
}

// ── Types for Level 10 (multi-bounded context) ─────────────────────────────

/// <summary>Domain event: inventory item reserved.</summary>
sealed record InventoryItemReservedEvent(Guid ItemId) : IDomainEvent;

/// <summary>Aggregate Root of the Inventory Bounded Context.</summary>
sealed class InventoryItem : AggregateRoot<Guid>
{
    public string Sku { get; private set; }
    public int Stock { get; private set; }

    public InventoryItem(Guid id, string sku, int stock)
    {
        Id = id;
        Sku = sku;
        Stock = stock;
    }

    public void Reserve(int quantity)
    {
        if (quantity > Stock)
            throw new InvalidOperationException("Insufficient stock.");

        Stock -= quantity;
        RaiseDomainEvent(new InventoryItemReservedEvent(Id));
    }
}

/// <summary>Domain event: sale initiated.</summary>
sealed record SaleInitiatedEvent(Guid SaleId, Guid InventoryItemId) : IDomainEvent;

/// <summary>
/// Aggregate Root of the Sales Bounded Context.
/// It only keeps a reference to the Id of the InventoryItem (not the whole object).
/// </summary>
sealed class SalesAggregate : AggregateRoot<Guid>
{
    /// <summary>
    /// Reference to the Id of the InventoryItem in the Inventory BC.
    /// A BC should never directly reference the aggregate of another BC,
    /// only its Id (principle of autonomy between Bounded Contexts).
    /// </summary>
    public Guid InventoryItemId { get; private set; }

    public SalesAggregate(Guid id, Guid inventoryItemId)
    {
        Id = id;
        InventoryItemId = inventoryItemId;
        RaiseDomainEvent(new SaleInitiatedEvent(id, inventoryItemId));
    }
}

// ── Showcase Utility ───────────────────────────────────────────────────────

/// <summary>Utility class to format the Showcase output.</summary>
static class Showcase
{
    public static void PrintHeader(string title)
    {
        Console.WriteLine($"┌─ {title} {new string('─', Math.Max(0, 60 - title.Length))}");
        Console.WriteLine();
    }
}
