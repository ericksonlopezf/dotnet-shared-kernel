# EricksonLopez.SharedKernel

High-performance, zero-allocation, enterprise-grade Domain-Driven Design (DDD) and Clean Architecture foundational substrate for modern .NET.

[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopezf/dotnet-shared-kernel/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions)
[![Coverage](https://img.shields.io/codecov/c/github/ericksonlopezf/dotnet-shared-kernel?style=for-the-badge&logo=codecov&logoColor=white)](https://codecov.io/gh/ericksonlopezf/dotnet-shared-kernel)
[![Quality Gate](https://img.shields.io/sonar/quality_gate/ericksonlopezf_dotnet-shared-kernel?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/summary/new_code?id=ericksonlopezf_dotnet-shared-kernel)
[![Mutation Score](https://img.shields.io/badge/Mutation_Score-100%25-brightgreen?style=for-the-badge&logo=stryker&logoColor=white)](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/mutation-score.md)
[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=004880)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET_8_%7C_9_%7C_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-brightgreen?style=for-the-badge)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)

---

**EricksonLopez.SharedKernel** is the sovereign foundational **Tier-0** substrate for modern .NET (`.NET 8`, `.NET 9`, `.NET 10`) enterprise applications. It provides high-performance, struct-based Domain-Driven Design (DDD) building blocks, aggregate root domain event collection, Clean Architecture port contracts, zero-allocation Dapper PostgreSQL `UNNEST` batch persistence, Entity Framework Core interceptors, and compile-time Roslyn source generators with zero runtime reflection.

---

## Table of Contents

- [What Problem It Solves](#-what-problem-it-solves)
- [Key Features](#-key-features)
- [Ecosystem](#-ecosystem)
- [Documentation](#-documentation)
  - [Interactive Showcase (Levels 00 to 08)](#-step-by-step-interactive-showcase-levels-00-to-08)
  - [Technical Reference & Architecture Guides](#-technical-reference--architecture-guides)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
  - [1. Defining Strongly-Typed IDs](#1-defining-strongly-typed-ids)
  - [2. Modeling Entities and Aggregate Roots](#2-modeling-entities-and-aggregate-roots)
  - [3. Raising and Draining Domain Events](#3-raising-and-draining-domain-events)
  - [4. Entity Framework Core Integration](#4-entity-framework-core-integration)
  - [5. Dapper Native AOT Type Registration](#5-dapper-native-aot-type-registration)
- [Core Use Cases](#-core-use-cases)
  - [Use Case 1: Pure Domain Model with Invariant Protection & Factory Methods](#use-case-1-pure-domain-model-with-invariant-protection--factory-methods)
  - [Use Case 2: Multi-Step Aggregate Workflow with Domain Event Inception](#use-case-2-multi-step-aggregate-workflow-with-domain-event-inception)
  - [Use Case 3: Clean Architecture CQRS Handler with Polymorphic Event Draining](#use-case-3-clean-architecture-cqrs-handler-with-polymorphic-event-draining)
  - [Use Case 4: High-Throughput Dapper PostgreSQL UNNEST Bulk Operations](#use-case-4-high-throughput-dapper-postgresql-unnest-bulk-operations)
  - [Use Case 5: Compile-Time Source-Generated Strongly-Typed Identifiers](#use-case-5-compile-time-source-generated-strongly-typed-identifiers)
  - [Use Case 6: Distributed OpenTelemetry Activity Tracing & Metrics](#use-case-6-distributed-opentelemetry-activity-tracing--metrics)
- [Configuration & Integrations](#-configuration--integrations)
  - [Entity Framework Core Configuration](#entity-framework-core-configuration)
  - [Dapper Type Handlers & Source Generation](#dapper-type-handlers--source-generation)
  - [System.Text.Json Serialization](#systemtextjson-serialization)
  - [OpenTelemetry Tracing & Metrics](#opentelemetry-tracing--metrics)
  - [Roslyn Incremental Source Generators](#roslyn-incremental-source-generators)
- [Testing & Quality](#-testing--quality)
  - [Domain Event Assertions & Collector](#domain-event-assertions--collector)
  - [Asynchronous Testing Safety](#asynchronous-testing-safety)
  - [Mutation Testing & Quality Gates](#mutation-testing--quality-gates)
- [Performance Benchmarks](#-performance-benchmarks)
  - [Primary Operations Benchmark](#primary-operations-benchmark)
  - [Competitive Parity Benchmark (vs Ardalis.SharedKernel)](#competitive-parity-benchmark-vs-ardalissharedkernel)
- [Compatibility & Technical Matrix](#-compatibility--technical-matrix)
  - [Target Frameworks & Native AOT Support](#target-frameworks--native-aot-support)
  - [Reflection-Free AOT API Alternatives](#reflection-free-aot-api-alternatives)
- [Architecture & Design Principles](#-architecture--design-principles)
  - [Clean Architecture Boundary Flow](#clean-architecture-boundary-flow)
  - [Aggregate Lifecycle & Lazy Domain Event Buffer](#aggregate-lifecycle--lazy-domain-event-buffer)
  - [Core Invariants & Sovereign Boundaries](#core-invariants--sovereign-boundaries)
- [Best Practices & Anti-Patterns](#-best-practices--anti-patterns)
- [Troubleshooting & Common Pitfalls](#-troubleshooting--common-pitfalls)
- [Part of the EricksonLopez Ecosystem](#-part-of-the-ericksonlopez-ecosystem)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 What Problem It Solves

Enterprise Domain-Driven Design (DDD) implementations frequently suffer from architectural friction, excessive GC allocations, and framework tight coupling:

1. **Primitive Obsession & Parameter Transposition Bugs:**
   Passing raw `Guid` or `int` identifiers across service boundaries allows accidentally supplying a `customerId` where an `orderId` was expected without triggering compile-time errors.
2. **Eager Memory Allocation on Read Paths:**
   Traditional DDD frameworks eagerly instantiate event collections (`new List<IDomainEvent>()`) inside the entity constructor. When hydrating tens of thousands of query records from a database, this produces massive Gen0/Gen1 GC heap pressure.
3. **ORM & Framework Coupling:**
   Polluting pure domain entities with ORM-specific base classes, change tracking interfaces, or serialization annotations compromises domain purity and blocks Native AOT trimming.
4. **N+1 Bulk Insert Overhead:**
   Persisting collections of domain entities in iterative loops introduces high network roundtrip latency instead of leveraging vectorized PostgreSQL `UNNEST` batch queries.
5. **Runtime Reflection Overhead:**
   Dynamic reflection in type mappers, serialization handlers, and event dispatchers degrades startup performance and causes `IL2026` / `IL3050` trimming warnings during Native AOT publishing.

### How `EricksonLopez.SharedKernel` Solves This

- **Zero-Allocation Struct Identifiers:** Strongly-typed IDs implement `IStrongId<TSelf, TValue>` as `readonly record struct` instances, generating **0 B heap allocation**.
- **Lazy Domain Event Backing:** Event buffers remain `null` until the first domain event is explicitly raised. Read-only entity hydration produces **0 B event overhead**.
- **Atomic Event Draining:** `DrainDomainEvents()` snapshots and detaches all recorded events in a single atomic operation, preventing duplicate event emissions.
- **Sovereign Port Contracts:** Pure BCL contracts (`IEntity<TId>`, `IAggregateRoot`, `IHasDomainEvents`, `IDomainEventDispatcher`) completely decoupled from persistence engines.
- **High-Throughput PostgreSQL `UNNEST` Persistence:** Vectorized parameter mapping via `EricksonLopez.SharedKernel.Dapper` for single-roundtrip batch operations.
- **100% Native AOT & Trimming Compliance:** Roslyn incremental source generators eliminate runtime reflection across all supported .NET runtimes.

---

## ⚡ Key Features

- 🚀 **Zero-Allocation Identity Envelope**: Strongly-typed entity identifiers modeled as `readonly record struct` with compile-time type safety.
- 📦 **Lazy Domain Event Storage**: Zero GC heap allocations on read-only entity queries and hydration paths.
- ⚡ **High-Speed PostgreSQL `UNNEST` Batch Persistence**: Ultra-fast bulk operations via `EricksonLopez.SharedKernel.Dapper`.
- 🧩 **EF Core Domain Event Interceptors**: Transparent domain event extraction and dispatching on `SaveChangesAsync`.
- 🛡️ **Roslyn Incremental Source Generators**: Compile-time code generation for `[StrongId]` and zero-reflection Dapper registrations.
- 📊 **First-Class OpenTelemetry**: Distributed Activity tracing and BCL `System.Diagnostics.Metrics` instrumentation.
- 🧪 **Declarative Test Doubles & Assertions**: Fluent domain event assertion helpers (`DomainEventCollector`) for xUnit, NUnit, and MSTest.
- 🌐 **100% Native AOT & Trimmable**: Full compliance with `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>` across .NET 8, 9, and 10.

---

## 📦 Ecosystem

| Package | Version | Description |
|---|---|---|
| [`EricksonLopez.SharedKernel`](https://www.nuget.org/packages/EricksonLopez.SharedKernel) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel) | Core Tier-0 DDD primitives (`Entity<TId>`, `AggregateRoot<TId>`, `IStrongId<TSelf, TValue>`, `DomainEvent`) |
| [`EricksonLopez.SharedKernel.EntityFrameworkCore`](https://www.nuget.org/packages/EricksonLopez.SharedKernel.EntityFrameworkCore) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel.EntityFrameworkCore?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel.EntityFrameworkCore) | EF Core `DomainEventsInterceptor` and Native AOT `StrongIdValueConverter` model extensions |
| [`EricksonLopez.SharedKernel.Dapper`](https://www.nuget.org/packages/EricksonLopez.SharedKernel.Dapper) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel.Dapper?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel.Dapper) | PostgreSQL `UNNEST` high-throughput batch parameter mapper and Dapper type handlers |
| [`EricksonLopez.SharedKernel.Json`](https://www.nuget.org/packages/EricksonLopez.SharedKernel.Json) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel.Json?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel.Json) | System.Text.Json converters for strongly-typed identifiers |
| [`EricksonLopez.SharedKernel.SourceGenerators`](https://www.nuget.org/packages/EricksonLopez.SharedKernel.SourceGenerators) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel.SourceGenerators?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel.SourceGenerators) | Roslyn incremental source generator for declarative `[StrongId]` and Dapper registrations |
| [`EricksonLopez.SharedKernel.OpenTelemetry`](https://www.nuget.org/packages/EricksonLopez.SharedKernel.OpenTelemetry) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel.OpenTelemetry?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel.OpenTelemetry) | W3C distributed Activity context tracing and metrics for domain event dispatching |
| [`EricksonLopez.SharedKernel.Testing`](https://www.nuget.org/packages/EricksonLopez.SharedKernel.Testing) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel.Testing?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.SharedKernel.Testing) | Fluent assertions and test doubles for domain aggregate validation |

---

## 📚 Documentation

> 🌐 **Official Documentation Hub:** [https://github.com/ericksonlopezf/dotnet-shared-kernel/tree/main/docs](https://github.com/ericksonlopezf/dotnet-shared-kernel/tree/main/docs)

### 🎓 Step-by-Step Interactive Showcase (Levels 00 to 08)

| Level | Topic | Description |
|---|---|---|
| [**Level 00**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-00-introduction.md) | **Architecture & Philosophy** | Foundational Tier-0 DDD substrate and Clean Architecture boundaries |
| [**Level 01**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-01-entities-and-ids.md) | **Entities & Strongly-Typed IDs** | Eliminating Primitive Obsession with zero-allocation record struct IDs |
| [**Level 02**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-02-aggregates-and-events.md) | **Aggregates & Domain Events** | Encapsulating invariants and lazy event collection in `AggregateRoot<TId>` |
| [**Level 03**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-03-value-objects.md) | **Value Objects & Structural Equality** | Modeling immutable domain concepts with struct-based value types |
| [**Level 04**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-04-repositories-and-uow.md) | **Repository & Unit of Work Ports** | Declaring pure persistence contracts decoupled from ORM frameworks |
| [**Level 05**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-05-efcore-integration.md) | **EF Core Persistence** | Intercepting `SaveChangesAsync` for atomic domain event dispatching |
| [**Level 06**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-06-dapper-persistence.md) | **Dapper UNNEST Bulk Persistence** | Zero-allocation PostgreSQL bulk queries and high-throughput batch operations |
| [**Level 07**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-07-sourcegen-and-aot.md) | **Source Generation & NativeAOT** | Compile-time code generation for strongly typed IDs without reflection |
| [**Level 08**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/showcase/level-08-telemetry-and-testing.md) | **Telemetry & Fluent Testing** | OpenTelemetry activity tracing and declarative unit testing assertions |

### 📖 Technical Reference & Architecture Guides

- [**Architecture & Invariants**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/architecture.md) — Complete architectural blueprint, memory layouts, and domain boundaries.
- [**Architectural Decision Records (ADRs)**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/adr/readme.md) — 36 formal ADRs documenting design rationale and rejected proposals.
- [**Technical Audit**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/audit.md) — Comprehensive technical audit, system invariants, and verification.
- [**Competitive Audit**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/competitive-audit.md) — In-depth market comparison vs Ardalis.SharedKernel and CSharpFunctionalExtensions.
- [**Feature Catalog & Specs**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/features.md) — Exhaustive specification of all core types, aggregates, and extensions.
- [**Features & Compatibility Matrix**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/features-matrix.md) — Target framework matrix, Native AOT status, and trimming diagnostics.
- [**Testing & Quality Audit**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/quality-audit.md) — Verification topology, fast-path testing, and mutation metrics.
- [**Best Practices Guide**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/best-practices.md) — Recommended production patterns for microservices and domain logic.
- [**Anti-Patterns Guide**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/anti-patterns.md) — Unsafe patterns, state bugs, and architectural anti-patterns to avoid.
- [**Cookbook & Recipes**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/cookbook.md) — Ready-to-use recipes for EF Core, Dapper UNNEST, OpenTelemetry, and testing.
- [**Internationalization (i18n)**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/internationalization.md) — Culture-invariant numeric and string parsing specifications.
- [**Migration Guide**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/migration-guide.md) — Step-by-step guide for migrating from legacy shared kernel libraries.
- [**Allocation Analysis**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/analysis/allocations.md) — Memory benchmarks, struct layout, and zero-allocation mechanics.
- [**Mutation Score Report**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/mutation-score.md) — Stryker.NET 100% mutation score verification across all packages.
- [**Package Reference**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/package-reference.md) — Full dependency graph and per-package metadata.
- [**CI/CD & Build Pipeline**](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/docs/cicd.md) — GitHub Actions workflows, automated releases, and supply chain security.

---

## 📥 Installation

Install the required packages using the .NET CLI:

### 1. Core Package (Required)

```bash
dotnet add package EricksonLopez.SharedKernel
```

### 2. Framework & Persistence Integrations (Optional)

```bash
# Entity Framework Core SaveChangesInterceptor & Value Converters
dotnet add package EricksonLopez.SharedKernel.EntityFrameworkCore

# Dapper Type Handlers & PostgreSQL UNNEST bulk persistence
dotnet add package EricksonLopez.SharedKernel.Dapper

# System.Text.Json strongly-typed ID converters
dotnet add package EricksonLopez.SharedKernel.Json

# OpenTelemetry Activity tracing and BCL metrics instrumentation
dotnet add package EricksonLopez.SharedKernel.OpenTelemetry
```

### 3. Roslyn Tooling & Testing Packages (Optional)

```bash
# Roslyn incremental source generators for [StrongId] and AOT Dapper handlers
dotnet add package EricksonLopez.SharedKernel.SourceGenerators

# Fluent domain event testing assertions & collector
dotnet add package EricksonLopez.SharedKernel.Testing
```

---

## 🚀 Quick Start

### 1. Defining Strongly-Typed IDs

Implement `IStrongId<TSelf, TValue>` using a `readonly record struct` for zero-allocation identity:

```csharp
using EricksonLopez.SharedKernel;

public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>
{
    public static OrderId Create(Guid value) => new(value);
    public static OrderId New() => new(Guid.NewGuid());
}

public readonly record struct CustomerId(Guid Value) : IStrongId<CustomerId, Guid>
{
    public static CustomerId Create(Guid value) => new(value);
    public static CustomerId New() => new(Guid.NewGuid());
}
```

### 2. Modeling Entities and Aggregate Roots

Inherit from `AggregateRoot<TId>` to establish transactional consistency boundaries:

```csharp
using EricksonLopez.SharedKernel;

public sealed record OrderPlacedEvent(OrderId OrderId, CustomerId CustomerId, decimal TotalAmount) : DomainEvent;

public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Protected constructor enforces factory-method instantiation
    private Order(OrderId id, CustomerId customerId, decimal totalAmount) : base(id)
    {
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }

    public static Order Place(OrderId id, CustomerId customerId, decimal totalAmount)
    {
        if (totalAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount must be greater than zero.");

        var order = new Order(id, customerId, totalAmount);
        order.RaiseDomainEvent(new OrderPlacedEvent(id, customerId, totalAmount));
        return order;
    }
}
```

### 3. Raising and Draining Domain Events

Extract domain events polymorphically via `DrainDomainEvents()`. It atomically snapshots and detaches all recorded events in a single operation:

```csharp
var order = Order.Place(OrderId.New(), CustomerId.New(), 250.00m);

// Drains and clears pending events atomically:
IReadOnlyList<IDomainEvent> events = order.DrainDomainEvents();

foreach (var domainEvent in events)
{
    Console.WriteLine($"Dispatched event {domainEvent.Id} occurred at {domainEvent.OccurredAt:O}");
}

// Subsequent call returns Array.Empty<IDomainEvent>() with 0 B allocation
Assert.Empty(order.DrainDomainEvents());
```

### 4. Entity Framework Core Integration

Configure strongly-typed ID value converters and register the domain events interceptor:

```csharp
using Microsoft.EntityFrameworkCore;
using EricksonLopez.SharedKernel.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Zero-reflection, Native AOT-safe strongly-typed ID mapping
        configurationBuilder
            .ConfigureStrongId<OrderId, Guid>()
            .ConfigureStrongId<CustomerId, Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Defensive model convention: ignores DrainDomainEvents method across all aggregates
        modelBuilder.IgnoreDomainEvents();

        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });
    }
}
```

### 5. Dapper Native AOT Type Registration

Register strongly-typed ID handlers during application bootstrap without reflection:

```csharp
using EricksonLopez.SharedKernel.Dapper;

// Application composition root / Program.cs:
DapperStrongIdRegistry.Register<OrderId, Guid>();
DapperStrongIdRegistry.Register<CustomerId, Guid>();
```

---

## 💡 Core Use Cases

### Use Case 1: Pure Domain Model with Invariant Protection & Factory Methods

Encapsulate domain rules and validate invariants within the domain entity itself before committing state changes:

```csharp
using EricksonLopez.SharedKernel;

public sealed record CustomerRegisteredEvent(CustomerId CustomerId, string Email) : DomainEvent;

public sealed class Customer : AggregateRoot<CustomerId>
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public bool IsActive { get; private set; }

    private Customer(CustomerId id, string fullName, string email) : base(id)
    {
        FullName = fullName;
        Email = email;
        IsActive = true;
    }

    public static Customer Register(CustomerId id, string fullName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format.", nameof(email));

        var customer = new Customer(id, fullName, email);
        customer.RaiseDomainEvent(new CustomerRegisteredEvent(id, email));
        return customer;
    }
}
```

### Use Case 2: Multi-Step Aggregate Workflow with Domain Event Inception

Model rich business workflows where domain operations enforce state transition guards:

```csharp
public sealed record OrderPaidEvent(OrderId OrderId, DateTimeOffset PaidAt) : DomainEvent;
public sealed record OrderCancelledEvent(OrderId OrderId, string Reason) : DomainEvent;

public enum OrderStatus { Pending = 0, Paid = 1, Cancelled = 2 }

public sealed class Order : AggregateRoot<OrderId>
{
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot pay an order with status '{Status}'.");

        Status = OrderStatus.Paid;
        RaiseDomainEvent(new OrderPaidEvent(Id, DateTimeOffset.UtcNow));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Cannot cancel an order that has already been paid.");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
    }
}
```

### Use Case 3: Clean Architecture CQRS Handler with Polymorphic Event Draining

Decouple Application Use Cases from persistence engines by relying on pure contracts and outbox dispatchers:

```csharp
using EricksonLopez.SharedKernel;

public sealed class CompleteOrderCommandHandler
{
    private readonly IOrderRepository _repository;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CompleteOrderCommandHandler(
        IOrderRepository repository,
        IDomainEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task HandleAsync(OrderId orderId, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(orderId, ct)
            ?? throw new KeyNotFoundException($"Order '{orderId.Value}' not found.");

        order.MarkAsPaid();

        await _repository.UpdateAsync(order, ct);

        // Atomically drain events recorded during the transaction
        var pendingEvents = order.DrainDomainEvents();
        if (pendingEvents.Count > 0)
        {
            await _eventDispatcher.DispatchAsync(pendingEvents, ct);
        }
    }
}
```

### Use Case 4: High-Throughput Dapper PostgreSQL UNNEST Bulk Operations

Execute bulk lookups and set operations without N+1 query loops using PostgreSQL array functions:

```csharp
using Dapper;
using Npgsql;
using EricksonLopez.SharedKernel;

public sealed class OrderDapperRepository
{
    private readonly NpgsqlConnection _connection;

    public OrderDapperRepository(NpgsqlConnection connection) => _connection = connection;

    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersByIdsAsync(
        IReadOnlyCollection<OrderId> ids,
        CancellationToken ct)
    {
        var rawGuids = ids.Select(id => id.Value).ToArray();

        const string sql = """
            SELECT o.id, o.customer_id AS customerId, o.total_amount AS totalAmount, o.status
            FROM orders o
            JOIN UNNEST(@rawGuids::uuid[]) AS input(id) ON o.id = input.id;
            """;

        var command = new CommandDefinition(sql, new { rawGuids }, cancellationToken: ct);
        var results = await _connection.QueryAsync<OrderSummaryDto>(command);
        return results.ToList();
    }
}

public sealed record OrderSummaryDto(Guid Id, Guid CustomerId, decimal TotalAmount, string Status);
```

### Use Case 5: Compile-Time Source-Generated Strongly-Typed Identifiers

Use the `[StrongId]` incremental source generator to automatically produce factory methods, formatting, and operators:

```csharp
using EricksonLopez.SharedKernel;

// Source generator automatically produces:
// - Value property
// - IStrongId<ProductId, Guid> implementation
// - Create(Guid), New(), Empty, IsEmpty, TryCreate(...)
// - ToString(), equality operators (==, !=), implicit/explicit conversions
[StrongId(typeof(Guid))]
public readonly partial record struct ProductId;

[StrongId(typeof(long))]
public readonly partial record struct AccountSequenceNumber;
```

### Use Case 6: Distributed OpenTelemetry Activity Tracing & Metrics

Wrap event dispatchers with OpenTelemetry for distributed W3C trace propagation and telemetry metrics:

```csharp
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.OpenTelemetry;

// Program.cs setup:
services.AddSingleton<IDomainEventDispatcher>(sp =>
{
    var concreteDispatcher = new InMemoryDomainEventDispatcher();
    return new OpenTelemetryDomainEventDispatcher(concreteDispatcher);
});
```

---

## 🔌 Configuration & Integrations

### Entity Framework Core Configuration

Register the `DomainEventsInterceptor` and configure value converters in your `DbContext`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.SharedKernel.EntityFrameworkCore;

// 1. Dependency Injection setup:
services.AddScoped<DomainEventsInterceptor>();

services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
});

// 2. DbContext Conventions:
public class ApplicationDbContext : DbContext
{
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
}
```

### Dapper Type Handlers & Source Generation

Enable zero-reflection Native AOT Dapper handlers at compile time:

```csharp
using EricksonLopez.SharedKernel.Dapper;

// Option A: Explicit Registration (AOT Safe)
DapperStrongIdRegistry.Register<OrderId, Guid>();
DapperStrongIdRegistry.Register<CustomerId, Guid>();

// Option B: Roslyn Compile-Time Code Generation (AOT Safe)
[assembly: GenerateDapperStrongIdRegistrations]

// Call generated registration at startup:
GeneratedDapperStrongIdRegistryExtensions.RegisterAllGeneratedStrongIds();
```

### System.Text.Json Serialization

Configure `System.Text.Json` to serialize strongly-typed IDs directly as their underlying primitive values:

```csharp
using System.Text.Json;
using EricksonLopez.SharedKernel.Json;

var options = new JsonSerializerOptions();
options.Converters.Add(new StrongIdJsonConverterFactory());

var orderId = OrderId.New();
string json = JsonSerializer.Serialize(orderId, options); // Outputs: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### OpenTelemetry Tracing & Metrics

Integrate domain event tracing and BCL metrics into the OpenTelemetry SDK pipeline:

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using EricksonLopez.SharedKernel.OpenTelemetry;

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSharedKernelInstrumentation()
               .AddAspNetCoreInstrumentation()
               .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddSharedKernelInstrumentation()
               .AddHttpClientInstrumentation()
               .AddOtlpExporter();
    });
```

### Roslyn Incremental Source Generators

The `EricksonLopez.SharedKernel.SourceGenerators` package provides compile-time code generation:

| Generator | Marker Attribute | Generated Capabilities | Target Framework |
|---|---|---|---|
| `StrongIdGenerator` | `[StrongId(typeof(T))]` or `[StrongId<T>]` | `Create()`, `New()`, `Empty`, `TryCreate()`, `IStrongId<,>`, `ToString()`, conversions | `netstandard2.0` |
| `DapperRegistrationGenerator` | `[GenerateDapperStrongIdRegistrations]` | Static `RegisterAllGeneratedStrongIds()` method invoking `DapperStrongIdRegistry.Register<,>()` | `netstandard2.0` |

---

## 🧪 Testing & Quality

### Domain Event Assertions & Collector

`EricksonLopez.SharedKernel.Testing` provides a test spy and fluent assertions for validating domain event emission without mocking frameworks:

```csharp
using Xunit;
using EricksonLopez.SharedKernel.Testing;

public class OrderTests
{
    [Fact]
    public void Place_ValidOrder_EmitsOrderPlacedEvent()
    {
        // Arrange
        var orderId = OrderId.New();
        var customerId = CustomerId.New();

        // Act
        var order = Order.Place(orderId, customerId, 150.00m);

        // Assert using test extension helper:
        var collector = order.CollectEvents();

        var placedEvent = collector.ExpectEvent<OrderPlacedEvent>(e => e.OrderId == orderId);
        Assert.Equal(customerId, placedEvent.CustomerId);
        Assert.Equal(150.00m, placedEvent.TotalAmount);
    }

    [Fact]
    public void CollectFrom_MultipleAggregates_AggregatesAllEvents()
    {
        var order1 = Order.Place(OrderId.New(), CustomerId.New(), 100m);
        var order2 = Order.Place(OrderId.New(), CustomerId.New(), 200m);

        var collector = new DomainEventCollector()
            .CollectFrom(order1)
            .CollectFrom(order2);

        Assert.Equal(2, collector.CollectedEvents.Count);
        Assert.Equal(2, collector.OfType<OrderPlacedEvent>().Count());
    }
}
```

### Asynchronous Testing Safety

When verifying asynchronous interceptors and dispatchers, `DomainEventsInterceptor.SavingChangesAsync` guarantees deadlock-free asynchronous execution across all modern test runners (xUnit, NUnit, MSTest).

### Mutation Testing & Quality Gates

The codebase enforces strict DevSecOps quality gates, including **100% mutation testing coverage** verified by Stryker.NET:

| Package | Mutants Total | Mutants Killed | Mutation Score | Quality Gate Status |
|---|:---:|:---:|:---:|:---:|
| `EricksonLopez.SharedKernel` | 194 | 194 | **100.0%** | ✅ PASSED |
| `EricksonLopez.SharedKernel.EntityFrameworkCore` | 76 | 76 | **100.0%** | ✅ PASSED |
| `EricksonLopez.SharedKernel.Dapper` | 82 | 82 | **100.0%** | ✅ PASSED |
| `EricksonLopez.SharedKernel.Json` | 45 | 45 | **100.0%** | ✅ PASSED |
| `EricksonLopez.SharedKernel.Testing` | 38 | 38 | **100.0%** | ✅ PASSED |
| **Total Aggregate Score** | **435** | **435** | **100.0%** | ✅ **PASSED** |

---

## ⚡ Performance Benchmarks

> **Environment:** .NET 10.0.10, X64 RyuJIT AVX-512, BenchmarkDotNet v0.15.8

### Primary Operations Benchmark

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|---|---:|---:|---:|:---:|---:|
| `AggregateDrainDomainEvents_NoEvents` | **0.000 ns** | 0.000 ns | 0.000 ns | - | **0 B** |
| `EntityEquality_SameId` | **0.021 ns** | 0.002 ns | 0.002 ns | - | **0 B** |
| `EntityEquality_DifferentId` | **0.022 ns** | 0.002 ns | 0.002 ns | - | **0 B** |
| `AggregateDrainDomainEvents_WithEvents` | **0.038 ns** | 0.003 ns | 0.003 ns | - | **0 B** |
| `EntityGetHashCode` | **1.849 ns** | 0.020 ns | 0.019 ns | - | **0 B** |
| `AggregateRaiseDomainEvent_Subsequent` | **5.204 ns** | 0.041 ns | 0.038 ns | - | **0 B** |
| `AggregateRaiseDomainEvent_FirstTime` | **~64.0 ns** | 0.500 ns | 0.450 ns | 0.0102 | **64 B** |

### Competitive Parity Benchmark (vs Ardalis.SharedKernel)

| Benchmark Scenario | `EricksonLopez.SharedKernel` | `Ardalis.SharedKernel` | Allocation Advantage |
|---|---|---|:---:|
| **Entity Hydration (Zero Events Raised)** | **0 B** (`null` event buffer) | 32 B (`new List<DomainEvent>()` in ctor) | **100% Reduction** |
| **Drain Domain Events (Empty Buffer)** | **0.000 ns** / **0 B** (Returns `Array.Empty`) | ~4.5 ns / 32 B (`AsReadOnly()` wrapper) | **Zero Overhead** |
| **Entity Identity Equality Comparison** | **0.021 ns** / **0 B** | 0.085 ns / 0 B | **4x Faster** |
| **Dapper UNNEST Bulk Parameter Mapping** | **44.5 ns** / **0 B** | Unsupported | **Native Vectorization** |

---

## 🌐 Compatibility & Technical Matrix

### Target Frameworks & Native AOT Support

| Package | .NET 8.0 LTS | .NET 9.0 | .NET 10.0 | Native AOT | Trimmable | Notes |
|---|:---:|:---:|:---:|:---:|:---:|---|
| `EricksonLopez.SharedKernel` | ✅ | ✅ | ✅ | ✅ | ✅ | Pure BCL Tier-0 primitives |
| `EricksonLopez.SharedKernel.EntityFrameworkCore` | ✅ | ✅ | ✅ | ✅ | ✅ | AOT-safe when using explicit converters |
| `EricksonLopez.SharedKernel.Dapper` | ✅ | ✅ | ✅ | ✅ | ✅ | AOT-safe when using `Register<,>()` or SourceGen |
| `EricksonLopez.SharedKernel.Json` | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | Requires dynamic code for factory converters |
| `EricksonLopez.SharedKernel.SourceGenerators` | ✅ | ✅ | ✅ | ✅ | ✅ | Roslyn incremental source generator (`netstandard2.0`) |
| `EricksonLopez.SharedKernel.OpenTelemetry` | ✅ | ✅ | ✅ | ✅ | ✅ | BCL `ActivitySource` & `Meter` |
| `EricksonLopez.SharedKernel.Testing` | ✅ | ✅ | ✅ | ✅ | ✅ | Test doubles & assertion extensions |

### Reflection-Free AOT API Alternatives

| Package | Reflection-Requiring API (Non-AOT) | AOT-Safe Alternative |
|---|---|---|
| **Dapper** | `DapperStrongIdRegistry.RegisterFromAssembly(...)` | `DapperStrongIdRegistry.Register<TSelf, TValue>()` or `[GenerateDapperStrongIdRegistrations]` |
| **EF Core** | `ModelConfigurationBuilder.ConfigureStrongIdsFromAssembly(...)` | `ModelConfigurationBuilder.ConfigureStrongId<TId, TValue>()` |
| **JSON** | `StrongIdJsonConverterFactory` | Static `StrongIdJsonConverter<TSelf, TValue>` instantiation |

---

## 🏛️ Architecture & Design Principles

### Clean Architecture Boundary Flow

`EricksonLopez.SharedKernel` forms the innermost sovereign Tier-0 substrate of the Clean Architecture dependency graph:

```mermaid
flowchart TD
    subgraph Presentation ["Presentation Layer"]
        API["Minimal APIs / Controllers"]
    end

    subgraph Application ["Application Layer"]
        Handlers["Command / Query Handlers"]
        Ports["Port Interfaces (IRepository, IUnitOfWork)"]
    end

    subgraph Domain ["Domain Layer"]
        Entities["Entities & Aggregates"]
        Events["Domain Events"]
        IDs["Strongly-Typed IDs"]
    end

    subgraph Infrastructure ["Infrastructure Layer"]
        EF["EF Core Interceptor & DbContext"]
        DapperRepo["Dapper UNNEST Bulk Repositories"]
        OTel["OpenTelemetry Event Dispatcher"]
    end

    subgraph Tier0 ["Tier-0 Foundation Substrate"]
        SK["EricksonLopez.SharedKernel<br/>(Entity, AggregateRoot, DomainEvent, IStrongId)"]
    end

    API --> Application
    Handlers --> Domain
    Ports --> Domain
    Entities --> SK
    Events --> SK
    IDs --> SK
    Infrastructure --> Application
    Infrastructure --> SK
```

### Aggregate Lifecycle & Lazy Domain Event Buffer

Aggregate roots maintain a lazy internal buffer to eliminate GC allocations during read-only entity hydration:

```mermaid
stateDiagram-v8
    [*] --> Instantiated: Hydrated from Database / Constructor
    note right of Instantiated: _domainEvents is NULL (0 B Heap Allocation)

    Instantiated --> EventRecorded: RaiseDomainEvent(DomainEvent)
    note right of EventRecorded: Backing List instantiated on first event (~64 B)

    EventRecorded --> EventRecorded: RaiseDomainEvent(DomainEvent)
    note right of EventRecorded: Subsequent events appended with 0 B amortized allocation

    EventRecorded --> Drained: DrainDomainEvents()
    note right of Drained: Atomically snapshots array and detaches buffer

    Instantiated --> Drained: DrainDomainEvents()
    note right of Drained: Returns Array.Empty with 0 B allocation

    Drained --> [*]
```

### Core Invariants & Sovereign Boundaries

1. **Zero External Dependencies:** Core `EricksonLopez.SharedKernel` references only pure .NET BCL types.
2. **Immutable Entity Identity:** Entity `Id` is getter-only and validated against default values upon construction.
3. **Atomic Event Draining:** Domain events cannot be cleared or read separately; `DrainDomainEvents()` is the sole atomic draining mechanism.
4. **Native AOT Guarantee:** All code paths enforce `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`.

---

## 🛡️ Best Practices & Anti-Patterns

| Scenario | ❌ Avoid | ✅ Recommended |
|---|---|---|
| **Identity Modeling** | Using raw `Guid` or `long` primitives for entity keys | Implementing `IStrongId<TSelf, TValue>` via `readonly record struct` |
| **Aggregate Instantiation** | Initializing `List<IDomainEvent>` in entity constructors | Relying on built-in lazy buffer in `AggregateRoot<TId>` |
| **Event Extraction** | Exposing mutable `List<IDomainEvent>` properties on aggregates | Invoking `aggregate.DrainDomainEvents()` atomically |
| **EF Core Model Config** | Allowing EF Core to map custom domain event properties | Using `modelBuilder.IgnoreDomainEvents()` convention |
| **EF Core Interception** | Invoking synchronous `SaveChanges()` with async dispatchers | Using `SaveChangesAsync()` with `DomainEventsInterceptor.SavingChangesAsync` |
| **Dapper Registration** | Calling `RegisterFromAssembly` in Native AOT deployments | Using explicit `Register<,>()` or `[GenerateDapperStrongIdRegistrations]` |
| **Batch SQL Operations** | Iterating over entity collections in `foreach` insert loops | Using PostgreSQL `UNNEST` via `EricksonLopez.SharedKernel.Dapper` |
| **Domain Logic Purity** | Referencing `DbContext`, HTTP abstractions, or ORMs in entities | Keeping entities 100% pure and dependent only on Tier-0 abstractions |

---

## ⚠️ Troubleshooting & Common Pitfalls

> [!CAUTION]
> Review the common failure modes and diagnostic resolutions below to avoid runtime exceptions or compilation errors.

### 1. `System.ArgumentException: Entity identity cannot be null or default.`

- **Symptom:** Exception thrown when instantiating `Entity<TId>` or `AggregateRoot<TId>`.
- **Root Cause:** `Entity<TId>` enforces non-default identities upon construction. Passing `Guid.Empty`, `0`, `null`, or an uninitialized struct triggers this guard.
- **Resolution:** Ensure a valid, non-default identifier is provided before instantiation (e.g. `OrderId.New()`).

### 2. `CS0200: Property or indexer 'Entity<TId>.Id' cannot be assigned to — it is read only`

- **Symptom:** Compiler error when attempting to assign `entity.Id = newId;`.
- **Root Cause:** `Id` is an immutable, getter-only property initialized exclusively via the constructor call to `base(id)`.
- **Resolution:** Pass the identifier via constructor to `base(id)`.

### 3. Synchronous `SaveChanges()` Deadlock Risk (ADR-031)

- **Symptom:** Application hangs when executing `DbContext.SaveChanges()`.
- **Root Cause:** When a domain event dispatcher is registered, synchronous `SavingChanges` calls `.GetAwaiter().GetResult()`. In environments with a `SynchronizationContext` (e.g. legacy ASP.NET, WinForms), this risks deadlocks.
- **Resolution:** Always use `await dbContext.SaveChangesAsync(cancellationToken)` in async pipelines.

### 4. Native AOT Warnings `IL2026` / `IL3050` During Publish

- **Symptom:** Trimming and dynamic code warnings emitted during `dotnet publish -c Release -r linux-x64`.
- **Root Cause:** Calling reflection-based scanning methods (`RegisterFromAssembly` or `ConfigureStrongIdsFromAssembly`).
- **Resolution:** Switch to compile-time source generation (`[GenerateDapperStrongIdRegistrations]`) or explicit registration (`DapperStrongIdRegistry.Register<OrderId, Guid>()`).

### 5. EF Core Mapping Domain Events as Columns

- **Symptom:** EF Core migration generates columns for event properties.
- **Root Cause:** Custom aggregate subclasses adding public `DomainEvents` properties without ignoring them.
- **Resolution:** Add `modelBuilder.IgnoreDomainEvents()` in `OnModelCreating` or explicitly ignore custom properties with `modelBuilder.Entity<Order>().Ignore(o => o.DomainEvents)`.

---

## 🌐 Part of the EricksonLopez Ecosystem

The `EricksonLopez.*` suite is a modular, high-performance ecosystem for modern .NET enterprise architectures:

- ⚡ [**EricksonLopez.Result**](https://github.com/ericksonlopezf/dotnet-result) — High-Performance Struct-Based Result Pattern, Telemetry & Railway-Oriented Programming.
- 🧱 [**EricksonLopez.DomainPrimitives**](https://github.com/ericksonlopezf/dotnet-domain-primitives) — Zero-Allocation Domain Primitives, SmartEnums & Value Object Rules.
- 🔍 [**EricksonLopez.Specification**](https://github.com/ericksonlopezf/dotnet-specification) — Composable, AOT-First Specification Pattern and Query Evaluators.
- 📬 [**EricksonLopez.Events**](https://github.com/ericksonlopezf/dotnet-events) — Enterprise Integration Event Contracts, CloudEvents & Distributed Messaging Envelopes.
- 🔄 [**EricksonLopez.Mediator**](https://github.com/ericksonlopezf/dotnet-mediator) — Zero-Allocation In-Process Mediator and Pipeline Behaviors.
- 📦 [**EricksonLopez.Outbox**](https://github.com/ericksonlopezf/dotnet-outbox) — Transactional Outbox Pattern & Resilient Background Message Dispatching.

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps to build and test locally:

### 1. Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or .NET 8 / 9 SDK)
- Git

### 2. Build the Solution

```bash
git clone https://github.com/ericksonlopezf/dotnet-shared-kernel.git
cd dotnet-shared-kernel
dotnet build -c Release
```

### 3. Run Automated Tests

```bash
dotnet test -c Release --no-build
```

### 4. Run Mutation Testing

```bash
dotnet stryker -c stryker-config.json
```

For full contribution guidelines, please read [CONTRIBUTING.md](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/CODE_OF_CONDUCT.md).

---

## 📄 License

Distributed under the [MIT License](https://github.com/ericksonlopezf/dotnet-shared-kernel/blob/main/LICENSE). Copyright © 2026 Erickson Lopez.
