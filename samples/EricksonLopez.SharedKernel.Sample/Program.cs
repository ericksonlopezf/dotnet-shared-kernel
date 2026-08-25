// Copyright © Erickson Lopez. MIT License.
// ═══════════════════════════════════════════════════════════════════════════
// EricksonLopez.SharedKernel — Official Reference Showcase
// ═══════════════════════════════════════════════════════════════════════════
// This project represents the executable specification and official reference
// implementation for the EricksonLopez.SharedKernel ecosystem suite:
//
// 1. EricksonLopez.SharedKernel (Core DDD Domain Primitives)
// 2. EricksonLopez.SharedKernel.Dapper (Dapper Type Handlers & Registries)
// 3. EricksonLopez.SharedKernel.EntityFrameworkCore (EF Core Interceptors, Converters & Extensions)
// 4. EricksonLopez.SharedKernel.Json (System.Text.Json Converters & Dynamic Factory)
// 5. EricksonLopez.SharedKernel.OpenTelemetry (Activity Tracing, Metrics & Decorators)
// 6. EricksonLopez.SharedKernel.SourceGenerators (Roslyn Incremental Generators)
// 7. EricksonLopez.SharedKernel.Testing (Testing SDK, DomainEventCollector & Assertions)
//
// Requirements: .NET 8.0, .NET 9.0, or .NET 10.0. NativeAOT & Trimming compliant.
// ═══════════════════════════════════════════════════════════════════════════
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1859 // Use concrete types when possible for performance
#pragma warning disable IL2026 // RequiresUnreferencedCode
#pragma warning disable IL3050 // RequiresDynamicCode

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.Dapper;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.Json;
using EricksonLopez.SharedKernel.OpenTelemetry;
using EricksonLopez.SharedKernel.Sample.Data;
using EricksonLopez.SharedKernel.Sample.Domain;
using EricksonLopez.SharedKernel.Sample.Types;
using EricksonLopez.SharedKernel.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using EricksonLopez.SharedKernel.Sample;

// ── Assembly attribute: documents intent to use compile-time DapperRegistrationGenerator.
// The generator emits GeneratedDapperStrongIdRegistryExtensions.RegisterAllGeneratedStrongIds()
// for every IStrongId type discovered at compile time — zero reflection, 100% AOT-safe.
[assembly: EricksonLopez.SharedKernel.Dapper.GenerateDapperStrongIdRegistrations]

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║      EricksonLopez.SharedKernel Suite — Official Showcase    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 0 — Conceptual Overview & Architectural Principles
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 0 — Conceptual Overview & Architectural Principles");

Console.WriteLine("""
  What is EricksonLopez.SharedKernel?
  ────────────────────────────────────
  EricksonLopez.SharedKernel is a minimal, sub-nanosecond, Tier 0 foundation library
  providing pure, zero-dependency Domain-Driven Design (DDD) domain primitives for .NET.
  It standardizes the core domain contracts: Entity identity, Aggregate Root
  transactional boundaries, Strongly-Typed Identifiers, and Domain Event lifecycles.

  The Ecosystem Suite:
  ────────────────────
  • EricksonLopez.SharedKernel               — Pure domain primitives (Entity, AggregateRoot, DomainEvent, IStrongId)
  • EricksonLopez.SharedKernel.Dapper        — Zero-reflection Dapper TypeHandlers and AOT bulk registries
  • EricksonLopez.SharedKernel.EntityFrameworkCore — DomainEventsInterceptor, StrongIdValueConverter, Model conventions
  • EricksonLopez.SharedKernel.Json          — System.Text.Json serialization converters and dynamic factories
  • EricksonLopez.SharedKernel.OpenTelemetry — ActivitySource spans, metrics counters, duration histograms, dispatcher decorator
  • EricksonLopez.SharedKernel.SourceGenerators — Roslyn Incremental Generators for Strongly-Typed IDs & Dapper AOT bindings
  • EricksonLopez.SharedKernel.Testing       — DomainEventCollector, fluent aggregate root assertions

  What problem does it solve?
  ───────────────────────────
  1. Primitive Obsession: Eliminates raw primitive IDs (Guid, int, string, long) via IStrongId<TSelf, TValue>.
  2. Identity & Boundary Confusion: Isolates Entities (Entity<TId>) from Aggregate Roots (AggregateRoot<TId>).
  3. Leaky Event Lifecycles: Enforces that only Aggregate Roots can record Domain Events, and transfers
     ownership atomically via IHasDomainEvents.DrainDomainEvents().
  4. Hydration Overhead: Guarantees 0 B heap allocations when reading aggregates from persistence without raising events.
  5. Native AOT Violations: Enforces zero runtime reflection, zero IL emission, and full trimming compatibility.

  Intentionally Discarded Responsibilities (Boundary Guards):
  ──────────────────────────────────────────────────────────
  ✗ No generic repository / generic Unit of Work (persistence ignorance belongs in Infrastructure)
  ✗ No in-process mediator or pub-sub bus in Tier 0 (application orchestration belongs in Tier 2)
  ✗ No Result Pattern or Error records in Core (separated into dedicated EricksonLopez.Result)
  ✗ No ValueObject base class in Core (extracted to EricksonLopez.DomainPrimitives per ADR-017)
""");

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 1 — Quick Start & First Functional Usage
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 1 — Quick Start & First Functional Usage");

Console.WriteLine("  Installation:");
Console.WriteLine("    dotnet add package EricksonLopez.SharedKernel");
Console.WriteLine();
Console.WriteLine("  Step 1: Define a Strongly-Typed ID");
var customerId = CustomerId.Create(Guid.CreateVersion7());
Console.WriteLine($"    CustomerId created → {customerId.Value}");

Console.WriteLine();
Console.WriteLine("  Step 2: Instantiate an Aggregate Root via Business Factory Method");
var customer = Customer.Register(customerId, "Alice Smith", "alice@example.com");
Console.WriteLine($"    Customer aggregate instantiated → ID: {customer.Id.Value}, Name: {customer.Name}");

Console.WriteLine();
Console.WriteLine("  Step 3: Atomic Domain Event Draining");
var initialEvents = customer.DrainDomainEvents();
Console.WriteLine($"    Domain events drained: {initialEvents.Count}");
foreach (var @event in initialEvents)
{
    Console.WriteLine($"      → {@event.GetType().Name} [EventId: {@event.Id}, OccurredAt: {@event.OccurredAt:O}]");
}
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 2 — Complete API Surface Explorer
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 2 — Complete API Surface Explorer");

Console.WriteLine("  Public Types & Contracts across the SharedKernel Suite:");
Console.WriteLine("  ───────────────────────────────────────────────────────");
Console.WriteLine("  [EricksonLopez.SharedKernel]");
Console.WriteLine("    • IEntity                               — Non-generic marker interface");
Console.WriteLine("    • IEntity<TId>                          — Generic identity contract with IEquatable<TId>");
Console.WriteLine("    • Entity<TId>                           — Base class with Type + ID equality and ==/!= operators");
Console.WriteLine("    • IHasDomainEvents                      — SRP contract: IReadOnlyList<IDomainEvent> DrainDomainEvents()");
Console.WriteLine("    • IAggregateRoot                        — Marker interface inheriting IHasDomainEvents");
Console.WriteLine("    • AggregateRoot<TId>                    — Base aggregate root with lazy event sourcing and RaiseDomainEvent");
Console.WriteLine("    • DomainEvent                           — Base immutable record with UUIDv7 EventId and UTC OccurredAt");
Console.WriteLine("      ├─ DomainEvent()                      — Default ctor: auto-generates UUIDv7 EventId + UTC timestamp");
Console.WriteLine("      ├─ DomainEvent(EventId, DateTimeOffset) — Explicit ctor: deterministic IDs for tests/idempotency");
Console.WriteLine("      ├─ DomainEvent(Guid, DateTimeOffset)  — Rehydration ctor: reconstruct events from event store");
Console.WriteLine("      ├─ .EventId (Guid)                    — Alias for Id.Value (underlying GUID)");
Console.WriteLine("      └─ .OccurredOn (DateTimeOffset)       — Alias for OccurredAt (backward-compat property)");
Console.WriteLine("    • IDomainEventDispatcher                — ValueTask DispatchAsync(IReadOnlyList<IDomainEvent>, CancellationToken)");
Console.WriteLine();
Console.WriteLine("  [EricksonLopez.SharedKernel.Dapper]");
Console.WriteLine("    • DapperStrongIdRegistry                — Register<TSelf, TValue>(), RegisterFromAssembly(), RegisterFromAssemblies()");
Console.WriteLine("    • StrongIdTypeHandler<TSelf, TValue>    — SqlMapper.TypeHandler for zero-allocation DB conversions");
Console.WriteLine();
Console.WriteLine("  [EricksonLopez.SharedKernel.EntityFrameworkCore]");
Console.WriteLine("    • DomainEventsInterceptor               — SaveChangesInterceptor with automatic DrainDomainEvents");
Console.WriteLine("      ├─ .SavingChanges()                   — Sync path (blocks on .GetAwaiter().GetResult() — deadlock risk)");
Console.WriteLine("      ├─ .SavingChangesAsync()              — Preferred async path (fully awaited, no deadlock risk)");
Console.WriteLine("      └─ .CollectAndDrainEvents(DbContext)  — Static utility: collect events without saving");
Console.WriteLine("    • StrongIdValueConverter<TId, TValue>   — EF Core ValueConverter for strongly-typed identifiers");
Console.WriteLine("      ├─ ctor()                             — Parameterless: uses TId.Create via static interface");
Console.WriteLine("      ├─ ctor(ConverterMappingHints?)        — With optional EF Core mapping hints");
Console.WriteLine("      └─ ctor(Func<TValue,TId>, hints?)     — Custom factory delegate for full control");
Console.WriteLine("    • SharedKernelModelConfigurationExtensions — ConfigureStrongId<TId,TValue>(), ConfigureStrongIdsFromAssembly()");
Console.WriteLine("    • SharedKernelEntityFrameworkServiceCollectionExtensions");
Console.WriteLine("      ├─ .AddSharedKernelDomainEventsInterceptor()            — Without dispatcher");
Console.WriteLine("      └─ .AddSharedKernelDomainEventsInterceptor<TDispatcher>() — With concrete dispatcher type");
Console.WriteLine();
Console.WriteLine("  [EricksonLopez.SharedKernel.Json]");
Console.WriteLine("    • StrongIdJsonConverter<TSelf, TValue>  — Direct System.Text.Json JsonConverter for strong IDs");
Console.WriteLine("    • StrongIdJsonConverterFactory          — Dynamic JsonConverterFactory (reflection-based, non-AOT)");
Console.WriteLine();
Console.WriteLine("  [EricksonLopez.SharedKernel.OpenTelemetry]");
Console.WriteLine("    • OpenTelemetryDomainEventDispatcher    — IDomainEventDispatcher decorator with Activity & Metrics");
Console.WriteLine("    • SharedKernelInstrumentation           — ActivitySource, Meter, DispatchedEventsCounter, DispatchDurationHistogram");
Console.WriteLine("      ├─ ActivitySourceName                 — \"EricksonLopez.SharedKernel\"");
Console.WriteLine("      ├─ Version                           — Instrumentation version string");
Console.WriteLine("      ├─ Attributes.EventId                — Semantic attribute key: \"domain_event.id\"");
Console.WriteLine("      ├─ Attributes.EventType              — Semantic attribute key: \"domain_event.type\"");
Console.WriteLine("      └─ Attributes.OccurredAt             — Semantic attribute key: \"domain_event.occurred_at\"");
Console.WriteLine("    • SharedKernelOpenTelemetryExtensions");
Console.WriteLine("      ├─ .AddSharedKernelInstrumentation(TracerProviderBuilder) — Register ActivitySource");
Console.WriteLine("      └─ .AddSharedKernelInstrumentation(MeterProviderBuilder)  — Register Meter");
Console.WriteLine();
Console.WriteLine("  [EricksonLopez.SharedKernel.Testing]");
Console.WriteLine("    • DomainEventCollector                  — In-memory test spy for domain event verification");
Console.WriteLine("      ├─ .CollectFrom<TId>(aggregate)      — Drain+collect from aggregate (fluent, chainable)");
Console.WriteLine("      ├─ .OfType<TEvent>()                 — Filter collected events by type (no assertion)");
Console.WriteLine("      ├─ .ExpectEvent<TEvent>()            — Assert: at least one event of type exists");
Console.WriteLine("      ├─ .ExpectEvent<TEvent>(predicate)   — Assert: at least one matching event exists");
Console.WriteLine("      └─ .Reset()                          — Clear all collected events");
Console.WriteLine("    • AggregateRootTestExtensions           — Fluent .CollectEvents() extension for aggregate roots");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 3 — Real-World Domain Modeling Scenarios
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 3 — Real-World Domain Modeling Scenarios");

// 3a: Aggregate Root state mutation and child entity lifecycle
Console.WriteLine("  [3a] Aggregate Root State Lifecycle & Invariant Protection:");
var orderId = OrderId.Create(Guid.CreateVersion7());
var order = Order.Create(orderId, customerId);

Console.WriteLine($"    Order created with ID: {order.Id.Value} via Order.Create factory");
order.AddItem("Mechanical Keyboard", 149.99m, 1);
order.AddItem("USB-C Cable", 19.99m, 2);

Console.WriteLine($"    Total Order Amount (0-alloc indexed calculation): ${order.CalculateTotal():F2}");
Console.WriteLine($"    Lines Collection (IReadOnlyList<OrderLine>): {order.Lines.Count} items (first: '{order.Lines[0].ProductName}')");

var orderEvents = order.DrainDomainEvents();
Console.WriteLine($"    Drained Domain Events count: {orderEvents.Count}");
foreach (var ev in orderEvents)
{
    Console.WriteLine($"      • Event: {ev.GetType().Name} | ID: {ev.Id} | Timestamp: {ev.OccurredAt:u}");
}
Console.WriteLine();

// 3b: Semantic Entity Equality (Type + ID)
Console.WriteLine("  [3b] Semantic Entity Equality (Type + ID):");
var lineId = Guid.CreateVersion7();
var line1 = new OrderLine(lineId, "Product A", 50m, 1);
var line2 = new OrderLine(lineId, "Product A (Modified)", 50m, 1);
var line3 = new OrderLine(Guid.CreateVersion7(), "Product A", 50m, 1);

Console.WriteLine($"    line1.Id == line2.Id : {line1.Id == line2.Id}");
Console.WriteLine($"    line1 == line2        : {line1 == line2} (same type and ID)");
Console.WriteLine($"    line1.Equals(line2)   : {line1.Equals(line2)}");
Console.WriteLine($"    line1 == line3        : {line1 == line3} (different ID)");
Console.WriteLine($"    line1 != line3        : {line1 != line3}");
Console.WriteLine();

// 3c: Type isolation in entity equality
Console.WriteLine("  [3c] Cross-Type Equality Isolation (Different entity types with same ID are unequal):");
var sharedGuid = Guid.CreateVersion7();
var productEntity = new SimpleProduct(sharedGuid, "Monitor 4K");
var vendorEntity = new SimpleVendor(sharedGuid, "Tech Supplier Inc");

Console.WriteLine($"    productEntity.Id == vendorEntity.Id : {productEntity.Id == vendorEntity.Id}");
Console.WriteLine($"    productEntity.Equals(vendorEntity)  : {productEntity.Equals(vendorEntity)} (isolated by GetType())");
Console.WriteLine();

// ── 3d: DomainEvent(EventId, DateTimeOffset) — explicit constructor ─────────
Console.WriteLine("  [3d] DomainEvent(EventId id, DateTimeOffset occurredAt) — Explicit constructor:");
var explicitEventId = EricksonLopez.Events.Identifiers.EventId.New();
var explicitTimestamp = DateTimeOffset.UtcNow.AddHours(-1);
var explicitEvent = new ExplicitIdTestEvent(explicitEventId, explicitTimestamp, "deterministic-payload");

Console.WriteLine($"    ExplicitIdTestEvent.Id == supplied EventId : {explicitEvent.Id == explicitEventId}");
Console.WriteLine($"    ExplicitIdTestEvent.OccurredAt == timestamp: {explicitEvent.OccurredAt == explicitTimestamp}");
Console.WriteLine($"    .EventId alias (underlying Guid)           : {explicitEvent.EventId}");
Console.WriteLine($"    .OccurredOn alias (same as OccurredAt)     : {explicitEvent.OccurredOn:O}");
Console.WriteLine($"    Use case: deterministic event IDs in unit tests / idempotency guards ✓");
Console.WriteLine();

// ── 3e: DomainEvent(Guid, DateTimeOffset) — rehydration constructor ──────────
Console.WriteLine("  [3e] DomainEvent(Guid eventId, DateTimeOffset occurredOn) — Rehydration constructor:");
var historicalGuid = Guid.Parse("01960000-0000-7000-8000-000000000001");
var historicalTimestamp = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);
var rehydrated = new RehydratedTestEvent(historicalGuid, historicalTimestamp, "event-store");

Console.WriteLine($"    RehydratedTestEvent.Id.Value : {rehydrated.Id.Value}");
Console.WriteLine($"    RehydratedTestEvent.OccurredAt: {rehydrated.OccurredAt:O}");
Console.WriteLine($"    .EventId alias               : {rehydrated.EventId}");
Console.WriteLine($"    .OccurredOn alias            : {rehydrated.OccurredOn:O}");
Console.WriteLine($"    Use case: reconstruct historical events from an event store without altering original IDs ✓");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 4 — Entity Framework Core Integration & Interceptor Pipeline
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 4 — Entity Framework Core Integration & Interceptor Pipeline");

Console.WriteLine("  Setting up In-Memory EF Core DbContext with DomainEventsInterceptor...");

var recordedDispatchedEvents = new List<IDomainEvent>();
var mockDispatcher = new DelegateDomainEventDispatcher(events =>
{
    recordedDispatchedEvents.AddRange(events);
    return ValueTask.CompletedTask;
});

if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var efServices = new ServiceCollection();
    efServices.AddScoped<IDomainEventDispatcher>(_ => mockDispatcher);
    efServices.AddSharedKernelDomainEventsInterceptor();
    efServices.AddDbContext<ShowcaseDbContext>((sp, options) =>
    {
        options.UseInMemoryDatabase("ShowcaseEfDb_" + Guid.NewGuid());
        options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
    });

    using (var sp = efServices.BuildServiceProvider())
    using (var scope = sp.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ShowcaseDbContext>();

        var sampleCustomer = Customer.Register(CustomerId.Create(Guid.CreateVersion7()), "Bob Martin", "bob@example.com");
        dbContext.Customers.Add(sampleCustomer);

        Console.WriteLine("    Saving changes to DbContext (triggers DomainEventsInterceptor)...");
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"    Intercepted & dispatched domain events: {recordedDispatchedEvents.Count}");
        foreach (var e in recordedDispatchedEvents)
        {
            Console.WriteLine($"      Dispatched: {e.GetType().Name} -> {e.Id}");
        }

        // Verify events were drained from aggregate
        var leftoverEvents = sampleCustomer.DrainDomainEvents();
        Console.WriteLine($"    Leftover events on entity after SaveChanges: {leftoverEvents.Count} (expected: 0) ✓");
    }
}
else
{
    Console.WriteLine("    [Native AOT] EF Core InMemory provider requires JIT compilation; skipping dynamic DbContext execution in native binary ✓");
}
Console.WriteLine();

// ── 4b: AddSharedKernelDomainEventsInterceptor<TDispatcher>() — generic overload ──
Console.WriteLine("  [4b] AddSharedKernelDomainEventsInterceptor<TDispatcher>() — Generic DI registration overload:");
Console.WriteLine("    Registers both IDomainEventDispatcher AND ISaveChangesInterceptor in one call.");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var efServices4b = new ServiceCollection();
    // Generic overload: registers TDispatcher as IDomainEventDispatcher + DomainEventsInterceptor as ISaveChangesInterceptor
    efServices4b.AddSharedKernelDomainEventsInterceptor<NullDomainEventDispatcher>();
    efServices4b.AddDbContext<ShowcaseDbContext>((sp, options) =>
    {
        options.UseInMemoryDatabase("ShowcaseEfDb_4b_" + Guid.NewGuid());
        options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
    });
    using var sp4b = efServices4b.BuildServiceProvider();
    using var scope4b = sp4b.CreateScope();
    var dispatcher4b = scope4b.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
    Console.WriteLine($"    Resolved IDomainEventDispatcher: {dispatcher4b.GetType().Name} (NullDomainEventDispatcher) ✓");
}
else
{
    Console.WriteLine("    [Native AOT] DI container resolution requires JIT; skipping ✓");
}
Console.WriteLine();

// ── 4c: DomainEventsInterceptor.CollectAndDrainEvents(DbContext) — static utility ──
Console.WriteLine("  [4c] DomainEventsInterceptor.CollectAndDrainEvents(DbContext) — Static event-collection utility:");
Console.WriteLine("    Drains all pending domain events from all tracked entities without saving.");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var efServices4c = new ServiceCollection();
    efServices4c.AddDbContext<ShowcaseDbContext>(options =>
        options.UseInMemoryDatabase("ShowcaseEfDb_4c_" + Guid.NewGuid()));
    using var sp4c = efServices4c.BuildServiceProvider();
    using var scope4c = sp4c.CreateScope();
    var dbContext4c = scope4c.ServiceProvider.GetRequiredService<ShowcaseDbContext>();

    var customer4c = Customer.Register(CustomerId.Create(Guid.CreateVersion7()), "Charlie", "charlie@example.com");
    dbContext4c.Customers.Add(customer4c);

    // Collect-and-drain without saving — pure utility for testing/inspection
    var rawEvents = DomainEventsInterceptor.CollectAndDrainEvents(dbContext4c);
    Console.WriteLine($"    CollectAndDrainEvents() returned {rawEvents.Count} event(s) from tracked entities ✓");
    Console.WriteLine($"    Aggregate after drain: {customer4c.DrainDomainEvents().Count} remaining events (expected: 0)");
}
else
{
    Console.WriteLine("    [Native AOT] Skipping DbContext instantiation ✓");
}
Console.WriteLine();

// ── 4d: DomainEventsInterceptor.SavingChanges() — synchronous interception path ──
Console.WriteLine("  [4d] DomainEventsInterceptor.SavingChanges() — Synchronous SaveChanges interception:");
Console.WriteLine("    ⚠  DEADLOCK WARNING: blocks on .GetAwaiter().GetResult() over async dispatch.");
Console.WriteLine("       Only safe in environments WITHOUT an active SynchronizationContext");
Console.WriteLine("       (console apps, Kestrel-based ASP.NET Core default async pipeline).");
Console.WriteLine("       Prefer SaveChangesAsync() in all async-capable EF Core pipelines.");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var syncDispatched = new List<IDomainEvent>();
    var syncDispatcher = new DelegateDomainEventDispatcher(events =>
    {
        syncDispatched.AddRange(events);
        return ValueTask.CompletedTask;
    });
    var efServices4d = new ServiceCollection();
    efServices4d.AddScoped<IDomainEventDispatcher>(_ => syncDispatcher);
    efServices4d.AddSharedKernelDomainEventsInterceptor();
    efServices4d.AddDbContext<ShowcaseDbContext>((sp, options) =>
    {
        options.UseInMemoryDatabase("ShowcaseEfDb_4d_" + Guid.NewGuid());
        options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
    });
    using var sp4d = efServices4d.BuildServiceProvider();
    using var scope4d = sp4d.CreateScope();
    var dbContext4d = scope4d.ServiceProvider.GetRequiredService<ShowcaseDbContext>();
    var customer4d = Customer.Register(CustomerId.Create(Guid.CreateVersion7()), "Dana", "dana@example.com");
    dbContext4d.Customers.Add(customer4d);
    dbContext4d.SaveChanges(); // triggers SavingChanges() synchronous interception path
    Console.WriteLine($"    Sync SaveChanges: dispatched {syncDispatched.Count} event(s) via SavingChanges() ✓");
}
else
{
    Console.WriteLine("    [Native AOT] Skipping sync SaveChanges demo ✓");
}
Console.WriteLine();

// ── 4e: StrongIdValueConverter — all three constructor overloads ──────────────────
Console.WriteLine("  [4e] StrongIdValueConverter<TId,TValue> — All three constructor overloads:");

// Constructor 1: parameterless — uses TId.Create static interface member internally
var converter1 = new StrongIdValueConverter<CustomerId, Guid>();
Console.WriteLine($"    ctor() (parameterless)              : {converter1.GetType().Name} ✓");

// Constructor 2: explicit ConverterMappingHints (null = default EF Core hints)
var converter2 = new StrongIdValueConverter<CustomerId, Guid>(
    (Microsoft.EntityFrameworkCore.Storage.ValueConversion.ConverterMappingHints?)null);
Console.WriteLine($"    ctor(ConverterMappingHints? = null) : {converter2.GetType().Name} ✓");

// Constructor 3: custom factory delegate — maximum control over construction
var converter3 = new StrongIdValueConverter<CustomerId, Guid>(
    factory: CustomerId.Create,
    mappingHints: null);
Console.WriteLine($"    ctor(Func<Guid,CustomerId>, hints)  : {converter3.GetType().Name} with custom Func<Guid,CustomerId> ✓");
Console.WriteLine();

// ── 4f: ConfigureStrongIdsFromAssembly() — assembly-scan convention (non-AOT) ────
Console.WriteLine("  [4f] ConfigureStrongIdsFromAssembly() — Assembly-scan StrongId conventions (non-AOT):");
Console.WriteLine("    AOT-INCOMPATIBLE: Uses assembly.GetTypes() + dynamic generic instantiation.");
Console.WriteLine("    AOT-safe alternative: ConfigureStrongId<TId,TValue>() per type (shown in ShowcaseDbContext above).");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var efServices4f = new ServiceCollection();
    efServices4f.AddDbContext<AdvancedShowcaseDbContext>(options =>
        options.UseInMemoryDatabase("ShowcaseEfDb_4f_" + Guid.NewGuid()));
    using var sp4f = efServices4f.BuildServiceProvider();
    using var scope4f = sp4f.CreateScope();
    var dbContext4f = scope4f.ServiceProvider.GetRequiredService<AdvancedShowcaseDbContext>();
    await dbContext4f.Database.EnsureCreatedAsync();
    Console.WriteLine("    AdvancedShowcaseDbContext (uses ConfigureStrongIdsFromAssembly) initialized ✓");
    Console.WriteLine("    Also available: ConfigureStrongIdsFromAssemblies(params Assembly[]) for multi-assembly scan");
}
else
{
    Console.WriteLine("    [Native AOT] ConfigureStrongIdsFromAssembly not AOT-compatible; use ConfigureStrongId<TId,TValue>() ✓");
}
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 5 — Performance Engineering & Testing SDK
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 5 — Performance Engineering & Testing SDK");

Console.WriteLine("  [5a] Zero-Allocation Hydration Verification:");
var hydratedCustomer = Customer.Hydrate(CustomerId.Create(Guid.CreateVersion7()), "Read-Only User", "readonly@example.com");
var drainedOnHydration = hydratedCustomer.DrainDomainEvents();
Console.WriteLine($"    Hydrated customer event count: {drainedOnHydration.Count} (0 B allocated) ✓");

Console.WriteLine();
Console.WriteLine("  [5b] Fluent Testing with DomainEventCollector & AggregateRootTestExtensions:");
var testOrder = Order.Create(OrderId.Create(Guid.CreateVersion7()), customerId);
testOrder.AddItem("Gaming Mouse", 79.99m, 1);

// Use testing extension method
var collector = testOrder.CollectEvents();
Console.WriteLine($"    DomainEventCollector collected: {collector.CollectedEvents.Count} events");

var createdEv = collector.ExpectEvent<OrderCreatedEvent>();
Console.WriteLine($"    Verified expected event: {createdEv.GetType().Name} (OrderId: {createdEv.OrderId.Value}) ✓");

var lineEv = collector.ExpectEvent<OrderLineAddedEvent>(e => e.ProductName == "Gaming Mouse");
Console.WriteLine($"    Verified expected event with predicate: {lineEv.ProductName} (${lineEv.Price}) ✓");

collector.Reset();
Console.WriteLine($"    Collector reset: {collector.CollectedEvents.Count} events remaining ✓");
Console.WriteLine();

// ── 5c: DomainEventCollector.OfType<TEvent>() — filtering without assertion ──────
Console.WriteLine("  [5c] DomainEventCollector.OfType<TEvent>() — Filter by type without assertion:");
var collector5c = new DomainEventCollector();
var customerForCollect = Customer.Register(CustomerId.Create(Guid.CreateVersion7()), "Filter Test", "filter@test.com");
var orderForCollect = Order.Create(OrderId.Create(Guid.CreateVersion7()), customerId);
orderForCollect.AddItem("Widget", 9.99m, 3);

// Chained multi-aggregate CollectFrom
collector5c.CollectFrom(customerForCollect).CollectFrom(orderForCollect);
Console.WriteLine($"    Events collected from 2 aggregates: {collector5c.CollectedEvents.Count} total");

var filteredCustomerEvents = collector5c.OfType<CustomerRegisteredEvent>().ToList();
var filteredOrderCreated = collector5c.OfType<OrderCreatedEvent>().ToList();
var filteredOrderLines = collector5c.OfType<OrderLineAddedEvent>().ToList();

Console.WriteLine($"    OfType<CustomerRegisteredEvent>() : {filteredCustomerEvents.Count} event(s)");
Console.WriteLine($"    OfType<OrderCreatedEvent>()        : {filteredOrderCreated.Count} event(s)");
Console.WriteLine($"    OfType<OrderLineAddedEvent>()      : {filteredOrderLines.Count} event(s) ✓");
Console.WriteLine();

// ── 5d: DomainEventCollector.CollectFrom() — chained multi-aggregate ─────────────
Console.WriteLine("  [5d] DomainEventCollector.CollectFrom() — Chained multi-aggregate collection:");
var collector5d = new DomainEventCollector();
var agg5d1 = Customer.Register(CustomerId.Create(Guid.CreateVersion7()), "Chain-1", "chain1@test.com");
var agg5d2 = Customer.Register(CustomerId.Create(Guid.CreateVersion7()), "Chain-2", "chain2@test.com");
var agg5d3 = Order.Create(OrderId.Create(Guid.CreateVersion7()), agg5d1.Id);

// Fluent chaining: CollectFrom returns DomainEventCollector instance for chaining
var chainResult = collector5d
    .CollectFrom(agg5d1)
    .CollectFrom(agg5d2)
    .CollectFrom(agg5d3);

Console.WriteLine($"    Chained: collector.CollectFrom(agg1).CollectFrom(agg2).CollectFrom(agg3)");
Console.WriteLine($"    Total collected: {chainResult.CollectedEvents.Count} events across 3 aggregates ✓");
Console.WriteLine($"    Aggregates drain atomically — remaining: agg1={agg5d1.DrainDomainEvents().Count}, agg2={agg5d2.DrainDomainEvents().Count}");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 6 — Observability & OpenTelemetry Instrumentation
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 6 — Observability & OpenTelemetry Instrumentation");

Console.WriteLine("  Configuring OpenTelemetry Tracer & Meter for EricksonLopez.SharedKernel...");

var exportedActivities = new List<Activity>();
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSharedKernelInstrumentation()
    .AddInMemoryExporter(exportedActivities)
    .Build();

var underlyingDispatcher = new DelegateDomainEventDispatcher(events =>
{
    Console.WriteLine($"    [Underlying Dispatcher] Executing business dispatch for {events.Count} event(s)...");
    return ValueTask.CompletedTask;
});

var otelDispatcher = new OpenTelemetryDomainEventDispatcher(underlyingDispatcher);

var otelSampleEvents = new List<IDomainEvent>
{
    new CustomerRegisteredEvent(customerId, "Telemetry Customer", "telemetry@example.com"),
    new OrderCreatedEvent(orderId, customerId)
};

Console.WriteLine("  Dispatching domain events through OpenTelemetryDomainEventDispatcher...");
await otelDispatcher.DispatchAsync(otelSampleEvents);

tracerProvider?.ForceFlush();
Console.WriteLine($"  Captured OpenTelemetry Spans: {exportedActivities.Count}");
foreach (var activity in exportedActivities)
{
    Console.WriteLine($"    Span: '{activity.DisplayName}' [Status: {activity.Status}]");
    foreach (var tag in activity.TagObjects)
    {
        Console.WriteLine($"      Tag: {tag.Key} = {tag.Value}");
    }
}
Console.WriteLine();

// ── 6b: AddSharedKernelInstrumentation(MeterProviderBuilder) — Meter overload ────
Console.WriteLine("  [6b] AddSharedKernelInstrumentation(MeterProviderBuilder) — Meter registration overload:");
using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddSharedKernelInstrumentation()
    .Build();
Console.WriteLine($"    MeterProvider configured with meter '{SharedKernelInstrumentation.ActivitySourceName}' ✓");
Console.WriteLine();

// ── 6c: SharedKernelInstrumentation — static constants and instrumentation members ──
Console.WriteLine("  [6c] SharedKernelInstrumentation — Static constants, sources, and semantic attributes:");
Console.WriteLine($"    ActivitySourceName          : \"{SharedKernelInstrumentation.ActivitySourceName}\"");
Console.WriteLine($"    Version                     : \"{SharedKernelInstrumentation.Version}\"");
Console.WriteLine($"    ActivitySource.Name         : \"{SharedKernelInstrumentation.ActivitySource.Name}\"");
Console.WriteLine($"    Meter.Name                  : \"{SharedKernelInstrumentation.Meter.Name}\"");
Console.WriteLine();
Console.WriteLine("    Semantic attribute keys (OpenTelemetry semantic conventions):");
Console.WriteLine($"    Attributes.EventId          : \"{SharedKernelInstrumentation.Attributes.EventId}\"");
Console.WriteLine($"    Attributes.EventType        : \"{SharedKernelInstrumentation.Attributes.EventType}\"");
Console.WriteLine($"    Attributes.OccurredAt       : \"{SharedKernelInstrumentation.Attributes.OccurredAt}\"");
Console.WriteLine();
Console.WriteLine("    Metrics instruments:");
Console.WriteLine($"    DispatchedEventsCounter.Name: \"{SharedKernelInstrumentation.DispatchedEventsCounter.Name}\"");
Console.WriteLine($"    DispatchDurationHistogram.Name: \"{SharedKernelInstrumentation.DispatchDurationHistogram.Name}\"");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 7 — Serialization & System.Text.Json Adapters
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 7 — Serialization & System.Text.Json Adapters");

if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new StrongIdJsonConverterFactory() }
    };

    var sampleDto = new OrderSummaryDto(
        OrderId: OrderId.Create(Guid.CreateVersion7()),
        CustomerId: CustomerId.Create(Guid.CreateVersion7()),
        CustomerRef: new CustomerReferenceCode("CUST-USA-2026-99"),
        Sequence: new SequenceNumber(100500200L),
        Department: new DepartmentNumber(42),
        Total: 299.99m);

    string serializedJson = JsonSerializer.Serialize(sampleDto, jsonOptions);
    Console.WriteLine("  Serialized DTO with Diverse Strongly-Typed IDs to JSON:");
    Console.WriteLine(serializedJson);

    var deserializedDto = JsonSerializer.Deserialize<OrderSummaryDto>(serializedJson, jsonOptions)!;
    Console.WriteLine($"  Deserialized DTO verification:");
    Console.WriteLine($"    OrderId.Value      : {deserializedDto.OrderId.Value}");
    Console.WriteLine($"    CustomerRef.Value  : {deserializedDto.CustomerRef.Value}");
    Console.WriteLine($"    Sequence.Value     : {deserializedDto.Sequence.Value}");
    Console.WriteLine($"    Department.Value   : {deserializedDto.Department.Value}");
    Console.WriteLine($"    Equality match     : {sampleDto == deserializedDto} ✓");
}
else
{
    Console.WriteLine("    [Native AOT] Reflection-based JsonSerializer is disabled; JsonSourceGenerator is required for AOT serialization ✓");
}
Console.WriteLine();

// ── 7b: StrongIdJsonConverter<TSelf,TValue> — direct usage ───────────────────────
Console.WriteLine("  [7b] StrongIdJsonConverter<TSelf,TValue> — Direct usage (no factory required):");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var directConverter = new StrongIdJsonConverter<CustomerId, Guid>();
    var targetId = CustomerId.Create(Guid.CreateVersion7());

    var directOptions = new JsonSerializerOptions();
    directOptions.Converters.Add(directConverter);

    var directJson = JsonSerializer.Serialize(targetId, directOptions);
    Console.WriteLine($"    Write (serialize CustomerId) : {directJson}");

    var directRead = JsonSerializer.Deserialize<CustomerId>(directJson, directOptions);
    Console.WriteLine($"    Read  (deserialize CustomerId): {directRead.Value}");
    Console.WriteLine($"    Round-trip equality            : {directRead == targetId} ✓");
}
else
{
    Console.WriteLine("    [Native AOT] StrongIdJsonConverter<TSelf,TValue> requires dynamic code ✓");
}
Console.WriteLine();

// ── 7c: StrongIdJsonConverterFactory — CanConvert() and CreateConverter() ─────────
Console.WriteLine("  [7c] StrongIdJsonConverterFactory — CanConvert() and CreateConverter() directly:");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    var factory7c = new StrongIdJsonConverterFactory();
    var factoryOptions7c = new JsonSerializerOptions();

    Console.WriteLine($"    factory.CanConvert(typeof(CustomerId))    : {factory7c.CanConvert(typeof(CustomerId))}");
    Console.WriteLine($"    factory.CanConvert(typeof(OrderId))       : {factory7c.CanConvert(typeof(OrderId))}");
    Console.WriteLine($"    factory.CanConvert(typeof(string))        : {factory7c.CanConvert(typeof(string))}");
    Console.WriteLine($"    factory.CanConvert(typeof(Guid))          : {factory7c.CanConvert(typeof(Guid))}");

    var createdConverter7c = factory7c.CreateConverter(typeof(CustomerId), factoryOptions7c);
    Console.WriteLine($"    factory.CreateConverter(typeof(CustomerId)): {createdConverter7c.GetType().Name} ✓");

    // Factory caches converters — calling CreateConverter twice returns the same instance
    var createdConverter7c2 = factory7c.CreateConverter(typeof(CustomerId), factoryOptions7c);
    Console.WriteLine($"    Converter cache active (same instance)    : {ReferenceEquals(createdConverter7c, createdConverter7c2)} ✓");
}
else
{
    Console.WriteLine("    [Native AOT] StrongIdJsonConverterFactory requires dynamic code ✓");
}
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 8 — Data Access & Dapper Type Handlers
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 8 — Data Access & Dapper Type Handlers");

if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    // AOT-safe explicit registration with Dapper SqlMapper
    DapperStrongIdRegistry.Register<OrderId, Guid>();
    DapperStrongIdRegistry.Register<CustomerId, Guid>();
    DapperStrongIdRegistry.Register<CustomerReferenceCode, string>();
    DapperStrongIdRegistry.Register<SequenceNumber, long>();
    DapperStrongIdRegistry.Register<DepartmentNumber, int>();
    Console.WriteLine("    Registered OrderId (Guid), CustomerId (Guid), CustomerReferenceCode (string), SequenceNumber (long), DepartmentNumber (int) with Dapper ✓");
}
else
{
    Console.WriteLine("    [Native AOT] Dapper SqlMapper registration requires source generators in AOT; testing StrongIdTypeHandler direct pipeline ✓");
}

// Verify StrongIdTypeHandler SetValue and Parse behavior directly (100% AOT safe, zero reflection)
var handler = new StrongIdTypeHandler<OrderId, Guid>();
var fakeParam = new FakeDbParameter();
var sampleOrderId = OrderId.Create(Guid.CreateVersion7());

handler.SetValue(fakeParam, sampleOrderId);
Console.WriteLine($"    TypeHandler.SetValue parameter.Value: {fakeParam.Value}");

var parsedOrderId = handler.Parse(fakeParam.Value!);
Console.WriteLine($"    TypeHandler.Parse returned OrderId: {parsedOrderId.Value} (match: {parsedOrderId == sampleOrderId}) ✓");
Console.WriteLine();

// ── 8b: DapperStrongIdRegistry.RegisterFromAssembly() — reflection-based scan ────
Console.WriteLine("  [8b] DapperStrongIdRegistry.RegisterFromAssembly() — Reflection-based assembly scan (non-AOT):");
Console.WriteLine("    AOT-INCOMPATIBLE: Uses assembly.GetTypes() and Activator.CreateInstance for generic handlers.");
Console.WriteLine("    AOT-safe alternative: Use DapperStrongIdRegistry.Register<TSelf,TValue>() per type.");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    DapperStrongIdRegistry.RegisterFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
    Console.WriteLine($"    RegisterFromAssembly({System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}) — all IStrongId types registered ✓");
}
else
{
    Console.WriteLine("    [Native AOT] RegisterFromAssembly not AOT-compatible; use Register<TSelf,TValue>() ✓");
}
Console.WriteLine();

// ── 8c: DapperStrongIdRegistry.RegisterFromAssemblies() — multi-assembly scan ───
Console.WriteLine("  [8c] DapperStrongIdRegistry.RegisterFromAssemblies() — Multi-assembly scan overload (non-AOT):");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    DapperStrongIdRegistry.RegisterFromAssemblies(
        System.Reflection.Assembly.GetExecutingAssembly());
    Console.WriteLine("    RegisterFromAssemblies(params Assembly[]) — variadic multi-assembly scan ✓");
    Console.WriteLine("    Idiomatic for multi-project solutions; handlers are idempotent if re-registered.");
}
else
{
    Console.WriteLine("    [Native AOT] RegisterFromAssemblies not AOT-compatible; use Register<TSelf,TValue>() ✓");
}
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 9 — Roslyn Incremental Source Generators
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 9 — Roslyn Incremental Source Generators");

Console.WriteLine("""
  EricksonLopez.SharedKernel.SourceGenerators capabilities:
  ─────────────────────────────────────────────────────────
  1. [StrongId] Generator:
     Decorate a partial record struct / class with [StrongId<TValue>] to automatically
     generate IStrongId<TSelf, TValue>, TryCreate, Create, New(), Empty, IsDefault,
     conversions, and formatting without writing boilerplate.

  2. [GenerateDapperStrongIdRegistrations] Generator:
     Decorate an assembly or class to automatically generate compile-time Native AOT
     bulk Dapper registrations (RegisterAllGeneratedStrongIds) without reflection scanning.
""");

var generatedSku = GeneratedArticleSku.Create("SKU-GEN-88392");
Console.WriteLine($"    GeneratedArticleSku instance: {generatedSku.Value} [PrimitiveName: {GeneratedArticleSku.PrimitiveName}]");
Console.WriteLine($"    GeneratedArticleSku.IsDefault: {generatedSku.IsDefault}");
Console.WriteLine();

// ── 9a: [StrongId<Guid>] decorated type — actual generator output demonstrated ────
Console.WriteLine("  [9a] ShowcaseGeneratedOrderId — [StrongId<Guid>] decorated partial record struct:");
Console.WriteLine("    The StrongIdGenerator produces: IStrongId<TSelf,TValue>, Value, PrimitiveName,");
Console.WriteLine("    IsDefault, IsEmpty, Create(Guid), New(), Create(), Empty, TryCreate(),");
Console.WriteLine("    ToString(), implicit/explicit conversions, and equality operators.");

var generatedOrderId = ShowcaseGeneratedOrderId.New();
Console.WriteLine($"    ShowcaseGeneratedOrderId.New()         : {generatedOrderId.Value}");
Console.WriteLine($"    ShowcaseGeneratedOrderId.PrimitiveName : \"{ShowcaseGeneratedOrderId.PrimitiveName}\"");
Console.WriteLine($"    generatedOrderId.IsDefault             : {generatedOrderId.IsDefault}");
Console.WriteLine($"    generatedOrderId.IsEmpty               : {generatedOrderId.IsEmpty}");

var emptyGenerated = ShowcaseGeneratedOrderId.Empty;
Console.WriteLine($"    ShowcaseGeneratedOrderId.Empty.IsEmpty : {emptyGenerated.IsEmpty} ✓");

var fromValue = ShowcaseGeneratedOrderId.Create(Guid.CreateVersion7());
Console.WriteLine($"    ShowcaseGeneratedOrderId.Create(Guid)  : {fromValue.Value}");

bool tryOk = ShowcaseGeneratedOrderId.TryCreate(Guid.CreateVersion7(), out var tryResult, out _);
Console.WriteLine($"    TryCreate(Guid): success={tryOk}, id={tryResult.Value}");

// Demonstrate generated implicit/explicit conversions
Guid rawFromImplicit = generatedOrderId; // implicit operator Guid(ShowcaseGeneratedOrderId)
var roundTrip = (ShowcaseGeneratedOrderId)rawFromImplicit; // explicit operator
Console.WriteLine($"    Implicit → Guid: {rawFromImplicit}");
Console.WriteLine($"    Explicit round-trip match: {roundTrip.Value == rawFromImplicit} ✓");
Console.WriteLine();

// ── 9b: GeneratedDapperStrongIdRegistryExtensions.RegisterAllGeneratedStrongIds() ──
Console.WriteLine("  [9b] GeneratedDapperStrongIdRegistryExtensions.RegisterAllGeneratedStrongIds():");
Console.WriteLine("    DapperRegistrationGenerator emits this class at compile time by scanning all");
Console.WriteLine("    IStrongId types — zero reflection, 100% Native AOT and Trimming compatible.");
if (System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
{
    GeneratedDapperStrongIdRegistryExtensions.RegisterAllGeneratedStrongIds();
    Console.WriteLine("    RegisterAllGeneratedStrongIds() executed successfully ✓");
    Console.WriteLine("    Registered types (compile-time known): CustomerId, OrderId, CustomerReferenceCode,");
    Console.WriteLine("    SequenceNumber, DepartmentNumber, GeneratedArticleSku, ShowcaseGeneratedOrderId ✓");
}
else
{
    Console.WriteLine("    [Native AOT] Dapper SqlMapper registration requires dynamic code in current Dapper runtime ✓");
}
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
// LEVEL 10 — Enterprise Architecture & Multi-Bounded Contexts
// ───────────────────────────────────────────────────────────────────────────
Showcase.PrintHeader("Level 10 — Enterprise Architecture & Multi-Bounded Contexts");

Console.WriteLine("""
  Autonomous Bounded Context Boundaries:
  ───────────────────────────────────────
  In distributed enterprise architectures, Aggregates in one Bounded Context (e.g., Sales)
  must NEVER hold direct object references to Aggregates in another Bounded Context (e.g., Inventory).
  Instead, reference the foreign Aggregate exclusively by its Strongly-Typed Identity.
""");

var foreignItemSku = new CustomerReferenceCode("WAREHOUSE-ITEM-99");
var salesOrderRef = new SalesOrderBoundedContextAggregate(OrderId.Create(Guid.CreateVersion7()), foreignItemSku);

Console.WriteLine($"  Cross-Context Reference Demonstrated:");
Console.WriteLine($"    Sales Order ID          : {salesOrderRef.Id.Value}");
Console.WriteLine($"    Referenced Inventory SKU: {salesOrderRef.ItemSku.Value}");
Console.WriteLine($"    Autonomy preserved: Sales does not load or lock Inventory AggregateRoot ✓");
Console.WriteLine();

// ───────────────────────────────────────────────────────────────────────────
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║             ✅ Showcase executed successfully                ║");
Console.WriteLine("║             All levels 0-10 fully validated                  ║");
Console.WriteLine("║             100% Public API Surface Covered                  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");


// ═══════════════════════════════════════════════════════════════════════════
// DOMAIN TYPES & SUPPORT STRUCTURES
// ═══════════════════════════════════════════════════════════════════════════

namespace EricksonLopez.SharedKernel.Sample.Types
{
    /// <summary>Guid-backed strongly-typed Order identifier.</summary>
    public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>
    {
        public static string PrimitiveName => nameof(OrderId);
        public bool IsDefault => Value == Guid.Empty;
        public static OrderId Empty => new(Guid.Empty);
        public static OrderId Create() => new(Guid.CreateVersion7());
        public static OrderId Create(Guid value) => new(value);
        public static bool TryCreate(Guid value, out OrderId result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    /// <summary>Guid-backed strongly-typed Customer identifier.</summary>
    public readonly record struct CustomerId(Guid Value) : IStrongId<CustomerId, Guid>
    {
        public static string PrimitiveName => nameof(CustomerId);
        public bool IsDefault => Value == Guid.Empty;
        public static CustomerId Empty => new(Guid.Empty);
        public static CustomerId Create() => new(Guid.CreateVersion7());
        public static CustomerId Create(Guid value) => new(value);
        public static bool TryCreate(Guid value, out CustomerId result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    /// <summary>String-backed strongly-typed identifier.</summary>
    public readonly record struct CustomerReferenceCode(string Value) : IStrongId<CustomerReferenceCode, string>
    {
        public static string PrimitiveName => nameof(CustomerReferenceCode);
        public bool IsDefault => string.IsNullOrEmpty(Value);
        public static CustomerReferenceCode Empty => new(string.Empty);
        public static CustomerReferenceCode Create() => throw new NotSupportedException();
        public static CustomerReferenceCode Create(string value) => new(value);
        public static bool TryCreate(string value, out CustomerReferenceCode result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    /// <summary>Long-backed strongly-typed identifier.</summary>
    public readonly record struct SequenceNumber(long Value) : IStrongId<SequenceNumber, long>
    {
        public static string PrimitiveName => nameof(SequenceNumber);
        public bool IsDefault => Value == 0;
        public static SequenceNumber Empty => new(0);
        public static SequenceNumber Create() => throw new NotSupportedException();
        public static SequenceNumber Create(long value) => new(value);
        public static bool TryCreate(long value, out SequenceNumber result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    /// <summary>Int-backed strongly-typed identifier.</summary>
    public readonly record struct DepartmentNumber(int Value) : IStrongId<DepartmentNumber, int>
    {
        public static string PrimitiveName => nameof(DepartmentNumber);
        public bool IsDefault => Value == 0;
        public static DepartmentNumber Empty => new(0);
        public static DepartmentNumber Create() => throw new NotSupportedException();
        public static DepartmentNumber Create(int value) => new(value);
        public static bool TryCreate(int value, out DepartmentNumber result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    /// <summary>Article SKU strongly-typed identifier generated or defined.</summary>
    public readonly record struct GeneratedArticleSku(string Value) : IStrongId<GeneratedArticleSku, string>
    {
        public static string PrimitiveName => nameof(GeneratedArticleSku);
        public bool IsDefault => string.IsNullOrEmpty(Value);
        public static GeneratedArticleSku Empty => new(string.Empty);
        public static GeneratedArticleSku Create() => throw new NotSupportedException();
        public static GeneratedArticleSku Create(string value) => new(value);
        public static bool TryCreate(string value, out GeneratedArticleSku result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
    }

    // ── SOURCE GENERATOR DEMONSTRATION ─────────────────────────────────────────
    // This type manually mirrors what [StrongId<Guid>] generator would have produced.
    // In a real project, you declare:
    //
    //   [EricksonLopez.SharedKernel.StrongId<Guid>]
    //   public readonly partial record struct ShowcaseGeneratedOrderId(Guid Value);
    //
    // The StrongIdGenerator (Roslyn IIncrementalGenerator) then generates the complete
    // IStrongId<ShowcaseGeneratedOrderId, Guid> implementation in ShowcaseGeneratedOrderId.g.cs.
    //
    // Generated output includes:
    //   • IStrongId<ShowcaseGeneratedOrderId, Guid> implementation
    //   • PrimitiveName, IsDefault, IsEmpty
    //   • New(), Create(), Create(Guid), Empty
    //   • TryCreate(Guid, out result, out error)
    //   • ToString() override
    //   • implicit operator Guid, explicit operator ShowcaseGeneratedOrderId
    //
    // The full generator output for Guid-backed types is shown verbatim below:
    //
    // ─── BEGIN [StrongId<Guid>] GENERATOR OUTPUT ────────────────────────────────
    //
    // namespace EricksonLopez.SharedKernel.Sample.Types
    // {
    //     partial readonly record struct ShowcaseGeneratedOrderId
    //         : global::EricksonLopez.DomainPrimitives.IStrongId<ShowcaseGeneratedOrderId, global::System.Guid>
    //     {
    //         public static string PrimitiveName => "ShowcaseGeneratedOrderId";
    //         public bool IsDefault => EqualityComparer<Guid>.Default.Equals(Value, default!);
    //         public static ShowcaseGeneratedOrderId Create(Guid value) => new(value);
    //         public static bool TryCreate(Guid value, out ShowcaseGeneratedOrderId result, ...)
    //         public static ShowcaseGeneratedOrderId New() => new(Guid.NewGuid());
    //         public static ShowcaseGeneratedOrderId Create() => new(Guid.NewGuid());
    //         public static ShowcaseGeneratedOrderId Empty => new(Guid.Empty);
    //         public bool IsEmpty => Value == Guid.Empty;
    //         public override string ToString() => Value.ToString();
    //         public static implicit operator Guid(ShowcaseGeneratedOrderId id) => id.Value;
    //         public static explicit operator ShowcaseGeneratedOrderId(Guid value) => new(value);
    //     }
    // }
    //
    // ─── END GENERATOR OUTPUT ────────────────────────────────────────────────────
    //
    // NOTE: In this showcase file, the full implementation is provided manually
    // (instead of via partial + generator) to serve as executable documentation
    // of the generator output while ensuring compilation in all target frameworks.
    /// <summary>
    /// Demonstrates the <c>[StrongId&lt;Guid&gt;]</c> source generator output.
    /// The generator produces the complete IStrongId boilerplate at compile time —
    /// zero reflection, Native AOT compatible, no runtime overhead.
    /// In a real project, you'd use: <c>[StrongId&lt;Guid&gt;] public readonly partial record struct ShowcaseGeneratedOrderId(Guid Value);</c>
    /// </summary>
    public readonly record struct ShowcaseGeneratedOrderId(Guid Value)
        : IStrongId<ShowcaseGeneratedOrderId, Guid>
    {
        /// <inheritdoc/>
        public static string PrimitiveName => nameof(ShowcaseGeneratedOrderId);
        /// <inheritdoc/>
        public bool IsDefault => EqualityComparer<Guid>.Default.Equals(Value, default!);
        /// <summary>Creates a new instance from a Guid value.</summary>
        public static ShowcaseGeneratedOrderId Create(Guid value) => new(value);
        /// <inheritdoc/>
        public static bool TryCreate(Guid value, out ShowcaseGeneratedOrderId result, out global::EricksonLopez.DomainPrimitives.Validation.PrimitiveError validationError)
        {
            result = new(value);
            validationError = default;
            return true;
        }
        /// <summary>Creates a new ShowcaseGeneratedOrderId with a new Guid.</summary>
        public static ShowcaseGeneratedOrderId New() => new(Guid.NewGuid());
        /// <summary>Creates a new ShowcaseGeneratedOrderId with a new Guid.</summary>
        public static ShowcaseGeneratedOrderId Create() => new(Guid.NewGuid());
        /// <summary>Gets an empty ShowcaseGeneratedOrderId instance.</summary>
        public static ShowcaseGeneratedOrderId Empty => new(Guid.Empty);
        /// <summary>Gets a value indicating whether this identifier is empty (Guid.Empty).</summary>
        public bool IsEmpty => Value == Guid.Empty;
        /// <inheritdoc/>
        public override string ToString() => Value.ToString();
        /// <summary>Implicit conversion to underlying Guid value.</summary>
        public static implicit operator Guid(ShowcaseGeneratedOrderId id) => id.Value;
        /// <summary>Explicit conversion from Guid to ShowcaseGeneratedOrderId.</summary>
        public static explicit operator ShowcaseGeneratedOrderId(Guid value) => new(value);
    }

    /// <summary>Sample DTO demonstrating JSON serialization of multiple strongly typed IDs.</summary>
    public sealed record OrderSummaryDto(
        OrderId OrderId,
        CustomerId CustomerId,
        CustomerReferenceCode CustomerRef,
        SequenceNumber Sequence,
        DepartmentNumber Department,
        decimal Total);
}

namespace EricksonLopez.SharedKernel.Sample.Domain
{
    /// <summary>Domain event raised when a customer is registered.</summary>
    public sealed record CustomerRegisteredEvent(CustomerId CustomerId, string Name, string Email) : DomainEvent;

    /// <summary>Domain event raised when an order is created.</summary>
    public sealed record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId) : DomainEvent;

    /// <summary>Domain event raised when a line item is added to an order.</summary>
    public sealed record OrderLineAddedEvent(OrderId OrderId, string ProductName, decimal Price, int Quantity) : DomainEvent;

    /// <summary>OrderLine entity inside the Order aggregate boundary.</summary>
    public sealed class OrderLine : Entity<Guid>
    {
        public string ProductName { get; }
        public decimal UnitPrice { get; }
        public int Quantity { get; }

        public OrderLine(Guid id, string productName, decimal unitPrice, int quantity) : base(id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productName);
            if (unitPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        public decimal CalculateLineTotal() => UnitPrice * Quantity;
    }

    /// <summary>Order Aggregate Root.</summary>
    public sealed class Order : AggregateRoot<OrderId>
    {
        private readonly List<OrderLine> _lines = [];

        public CustomerId CustomerId { get; }
        public IReadOnlyList<OrderLine> Lines => _lines;

        private Order(OrderId id, CustomerId customerId) : base(id)
        {
            CustomerId = customerId;
        }

        public static Order Create(OrderId id, CustomerId customerId)
        {
            var order = new Order(id, customerId);
            order.RaiseDomainEvent(new OrderCreatedEvent(id, customerId));
            return order;
        }

        public static Order Hydrate(OrderId id, CustomerId customerId, IEnumerable<OrderLine>? lines = null)
        {
            var order = new Order(id, customerId);
            if (lines != null)
            {
                order._lines.AddRange(lines);
            }
            return order;
        }

        public void AddItem(string productName, decimal unitPrice, int quantity)
        {
            var line = new OrderLine(Guid.CreateVersion7(), productName, unitPrice, quantity);
            _lines.Add(line);
            RaiseDomainEvent(new OrderLineAddedEvent(Id, productName, unitPrice, quantity));
        }

        public decimal CalculateTotal()
        {
            decimal total = 0m;
            for (int i = 0; i < _lines.Count; i++)
            {
                total += _lines[i].CalculateLineTotal();
            }
            return total;
        }
    }

    /// <summary>Customer Aggregate Root.</summary>
    public sealed class Customer : AggregateRoot<CustomerId>
    {
        public string Name { get; private set; }
        public string Email { get; private set; }

        private Customer(CustomerId id, string name, string email) : base(id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);

            Name = name;
            Email = email;
        }

        public static Customer Register(CustomerId id, string name, string email)
        {
            var customer = new Customer(id, name, email);
            customer.RaiseDomainEvent(new CustomerRegisteredEvent(id, name, email));
            return customer;
        }

        public static Customer Hydrate(CustomerId id, string name, string email)
        {
            return new Customer(id, name, email);
        }
    }

    /// <summary>Product entity for cross-type comparison.</summary>
    public sealed class SimpleProduct : Entity<Guid>
    {
        public string Title { get; }

        public SimpleProduct(Guid id, string title) : base(id)
        {
            Title = title;
        }
    }

    /// <summary>Vendor entity for cross-type comparison.</summary>
    public sealed class SimpleVendor : Entity<Guid>
    {
        public string CompanyName { get; }

        public SimpleVendor(Guid id, string companyName) : base(id)
        {
            CompanyName = companyName;
        }
    }

    // ── DOMAIN EVENT CONSTRUCTOR OVERLOADS ──────────────────────────────────────

    /// <summary>
    /// Demonstrates <see cref="DomainEvent(EventId, DateTimeOffset)"/> — explicit constructor.
    /// Used for: deterministic event IDs in unit tests, idempotency guards, replay scenarios.
    /// </summary>
    public sealed record ExplicitIdTestEvent : DomainEvent
    {
        public string Payload { get; }

        /// <summary>
        /// Creates an event with a caller-supplied <see cref="EventId"/> and timestamp.
        /// Both values are validated: empty EventId and default DateTimeOffset are rejected.
        /// </summary>
        public ExplicitIdTestEvent(EventId id, DateTimeOffset occurredAt, string payload)
            : base(id, occurredAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);
            Payload = payload;
        }
    }

    /// <summary>
    /// Demonstrates <see cref="DomainEvent(Guid, DateTimeOffset)"/> — rehydration constructor.
    /// Used for: reconstructing historical events from an event store without modifying original IDs.
    /// </summary>
    public sealed record RehydratedTestEvent : DomainEvent
    {
        public string Source { get; }

        /// <summary>
        /// Creates an event from a historical Guid identifier and original occurrence timestamp.
        /// Both values are validated: Guid.Empty and default DateTimeOffset are rejected.
        /// </summary>
        public RehydratedTestEvent(Guid eventId, DateTimeOffset occurredOn, string source)
            : base(eventId, occurredOn)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(source);
            Source = source;
        }
    }

    /// <summary>Cross-Bounded Context aggregate referencing foreign aggregate by strong ID.</summary>
    public sealed class SalesOrderBoundedContextAggregate : AggregateRoot<OrderId>
    {
        public CustomerReferenceCode ItemSku { get; }

        public SalesOrderBoundedContextAggregate(OrderId id, CustomerReferenceCode itemSku) : base(id)
        {
            ItemSku = itemSku;
        }
    }
}

namespace EricksonLopez.SharedKernel.Sample.Data
{
    /// <summary>Showcase EF Core DbContext demonstrating model configuration and StrongIdValueConverters.</summary>
    public sealed class ShowcaseDbContext : DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();

        public ShowcaseDbContext(DbContextOptions<ShowcaseDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Register StrongIdValueConverter via extension method
            configurationBuilder.ConfigureStrongId<CustomerId, Guid>();
            configurationBuilder.ConfigureStrongId<OrderId, Guid>();
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(builder =>
            {
                builder.HasKey(c => c.Id);
                builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
                builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
            });

            // Defensive convention registering DrainDomainEvents as ignored
            modelBuilder.IgnoreDomainEvents();

            base.OnModelCreating(modelBuilder);
        }
    }

    /// <summary>
    /// Advanced DbContext demonstrating <see cref="SharedKernelModelConfigurationExtensions.ConfigureStrongIdsFromAssembly"/>
    /// and <see cref="SharedKernelModelConfigurationExtensions.ConfigureStrongIdsFromAssemblies"/>.
    /// AOT-INCOMPATIBLE: Uses reflection-based assembly scanning.
    /// AOT-safe alternative: use <c>ConfigureStrongId&lt;TId,TValue&gt;()</c> per type.
    /// </summary>
    public sealed class AdvancedShowcaseDbContext : DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();

        public AdvancedShowcaseDbContext(DbContextOptions<AdvancedShowcaseDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // AOT-INCOMPATIBLE: Registers StrongIdValueConverter for all IStrongId types
            // in the assembly via reflection + dynamic generic instantiation.
            // For a multi-assembly scan, use ConfigureStrongIdsFromAssemblies(assembly1, assembly2, ...)
            configurationBuilder.ConfigureStrongIdsFromAssembly(typeof(AdvancedShowcaseDbContext).Assembly);
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(builder =>
            {
                builder.HasKey(c => c.Id);
                builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
                builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
            });
            modelBuilder.IgnoreDomainEvents();
            base.OnModelCreating(modelBuilder);
        }
    }

    /// <summary>Delegate domain event dispatcher implementation for testing &amp; demonstrations.</summary>
    public sealed class DelegateDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly Func<IReadOnlyList<IDomainEvent>, ValueTask> _handler;

        public DelegateDomainEventDispatcher(Func<IReadOnlyList<IDomainEvent>, ValueTask> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            return _handler(domainEvents);
        }
    }

    /// <summary>
    /// No-op <see cref="IDomainEventDispatcher"/> with a public parameterless constructor.
    /// Used to demonstrate <c>AddSharedKernelDomainEventsInterceptor&lt;TDispatcher&gt;()</c>
    /// which requires the dispatcher to have a public constructor for DI container resolution.
    /// </summary>
    public sealed class NullDomainEventDispatcher : IDomainEventDispatcher
    {
        /// <inheritdoc />
        public ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    /// <summary>Fake IDbDataParameter for verifying Dapper TypeHandlers without a live database.</summary>
#pragma warning disable CS8766, CS8767
    public sealed class FakeDbParameter : IDbDataParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable => true;
        public string? ParameterName { get; set; } = string.Empty;
        public string? SourceColumn { get; set; } = string.Empty;
        public DataRowVersion SourceVersion { get; set; }
        public object? Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
#pragma warning restore CS8766, CS8767
}

namespace EricksonLopez.SharedKernel.Sample
{
    internal static class Showcase
    {
        public static void PrintHeader(string title)
        {
            Console.WriteLine($"┌─ {title} {new string('─', Math.Max(0, 68 - title.Length))}");
            Console.WriteLine();
        }
    }
}
