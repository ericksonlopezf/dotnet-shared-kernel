# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [3.0.0] — 2026-08-25

### ⚠ Breaking Changes

- **BC-001: Removal of Parameterless Constructor on `Entity<TId>`**
  - **Previous State:** `Entity<TId>` declared an implicit protected parameterless constructor (`protected Entity() { }`) allowing instantiation via object initializers.
  - **Current State:** `Entity<TId>` now provides a single constructor requiring a non-default identifier: `protected Entity(TId id)`.
  - **Impact:** Any entity subclass lacking an explicit constructor invoking `base(id)` will fail compilation with `CS7036`.
  - **Migration:** Add a constructor in derived entities passing `id` to `base(id)`.
    ```csharp
    // Before (v2.0.0)
    public class Order : Entity<OrderId> { }
    var order = new Order { Id = orderId };

    // After (Current)
    public class Order : Entity<OrderId>
    {
        public Order(OrderId id) : base(id) { }
    }
    ```

- **BC-002: Removal of `init` Accessor on `Entity<TId>.Id`**
  - **Previous State:** `public TId Id { get; protected init; } = default!;`
  - **Current State:** `public TId Id { get; }` (get-only property).
  - **Impact:** Direct property initialization `new Order { Id = orderId }` fails compilation with `CS0200`.
  - **Migration:** Supply `id` via the constructor parameter.

- **BC-003: Default Entity Identifier Rejection with `ArgumentException`**
  - **Previous State:** Instantiating an entity with `default(TId)` (e.g., `Guid.Empty` or `0`) was permitted and treated as a transient entity.
  - **Current State:** The constructor validates `EqualityComparer<TId>.Default.Equals(id, default!)` and throws `ArgumentException("Entity identity cannot be default.", nameof(id))`.
  - **Impact:** Code creating uninitialized/transient entities with default ID values will crash at runtime.
  - **Migration:** Ensure all entities receive a valid, non-default identifier at creation time (e.g., generate `Guid.NewGuid()` / UUIDv7 before instantiation).

- **BC-004: Removal of `Entity<TId>.IsTransient()` Method**
  - **Previous State:** `public bool IsTransient()` was exposed on `Entity<TId>`.
  - **Current State:** `IsTransient()` has been removed completely, as entities are required to be non-transient by domain invariants.
  - **Impact:** Calls to `entity.IsTransient()` fail compilation with `CS1061`.
  - **Migration:** Remove calls to `IsTransient()`. Use persistence state tracking (e.g., EF Core `EntityEntry.State`) if tracking transient state in infrastructure.

- **BC-005: Revised Entity Equality and HashCode Semantics**
  - **Previous State:** Transient entities (default Id) returned `false` for all equality comparisons and used `RuntimeHelpers.GetHashCode(this)` for hash code generation.
  - **Current State:** Equality is strictly determined by concrete type matching and `EqualityComparer<TId>.Default.Equals(Id, other.Id)`. `GetHashCode()` delegates directly to `HashCode.Combine(GetType(), Id)`.
  - **Impact:** Eliminates special-casing for transient entities in hash sets and dictionaries.

- **BC-006: Removal of Parameterless Constructor on `AggregateRoot<TId>`**
  - **Previous State:** `AggregateRoot<TId>` inherited the parameterless constructor from `Entity<TId>`.
  - **Current State:** `protected AggregateRoot(TId id) : base(id)` is now the only constructor.
  - **Impact:** Aggregate root subclasses must explicitly invoke `base(id)`.
  - **Migration:**
    ```csharp
    // Before (v2.0.0)
    public sealed class Order : AggregateRoot<OrderId> { }

    // After (Current)
    public sealed class Order : AggregateRoot<OrderId>
    {
        public Order(OrderId id) : base(id) { }
    }
    ```

- **BC-007: Removal of `AggregateRoot<TId>.DomainEvents` Property**
  - **Previous State:** `public IReadOnlyCollection<IDomainEvent> DomainEvents => ...` exposed pending events.
  - **Current State:** Property removed. Replaced by `IReadOnlyList<IDomainEvent> DrainDomainEvents()` defined on `IHasDomainEvents`.
  - **Impact:** Reading `aggregate.DomainEvents` fails compilation with `CS1061`.
  - **Migration:** Call `aggregate.DrainDomainEvents()` to atomically retrieve and clear pending events.

- **BC-008: Removal of `AggregateRoot<TId>.ClearDomainEvents()` Method**
  - **Previous State:** `public void ClearDomainEvents()` cleared pending events.
  - **Current State:** Method removed. Handled atomically via `DrainDomainEvents()`.
  - **Impact:** Calling `aggregate.ClearDomainEvents()` fails compilation with `CS1061`.
  - **Migration:** Replace separate read and clear operations with a single call to `aggregate.DrainDomainEvents()`.

- **BC-009: Extraction and Namespace Relocation of `IDomainEvent`**
  - **Previous State:** `public interface IDomainEvent` was defined in `EricksonLopez.SharedKernel` namespace within `EricksonLopez.SharedKernel.dll`.
  - **Current State:** Interface moved to `EricksonLopez.Events.Contracts` package and namespace.
  - **Impact:** Types implementing `IDomainEvent` must reference `EricksonLopez.Events.Contracts` and add `using EricksonLopez.Events.Contracts;` or derive from the `DomainEvent` base record.
  - **Migration:** Add package reference to `EricksonLopez.Events.Contracts` and update using directives, or inherit `EricksonLopez.SharedKernel.DomainEvent`.

- **BC-010: Introduction of Tier-0 Package Dependencies**
  - **Previous State:** `EricksonLopez.SharedKernel` had zero package dependencies.
  - **Current State:** `EricksonLopez.SharedKernel` references `EricksonLopez.Events.Contracts` and `EricksonLopez.DomainPrimitives.Abstractions`.
  - **Impact:** Consuming applications will receive transitive dependencies to foundation contract packages.

- **BC-011: Build Configuration: `ImplicitUsings` Set to `disable`**
  - **Previous State:** `<ImplicitUsings>enable</ImplicitUsings>` was enabled in `Directory.Build.props`.
  - **Current State:** `<ImplicitUsings>disable</ImplicitUsings>` is configured for strict type visibility.
  - **Impact:** Consuming projects sharing repository build properties must declare explicit `using` statements.

### Added

- `IEntity` — Non-generic marker contract for polymorphic domain entities.
- `IEntity<TId>` — Generic entity identity contract exposing `TId Id { get; }`.
- `IHasDomainEvents` — Non-generic polymorphic contract for domain event draining via `IReadOnlyList<IDomainEvent> DrainDomainEvents()`.
- `IAggregateRoot` — Marker interface inheriting `IHasDomainEvents`.
- `DomainEvent` — Abstract record for time-ordered domain events. Provides `Id` (`EventId`, UUIDv7 on .NET 9+), `OccurredAt` (`DateTimeOffset` UTC), and backward-compatibility aliases `EventId` (`Guid`) and `OccurredOn` (`DateTimeOffset`).
- `IDomainEventDispatcher` — Port contract for asynchronous batch domain event dispatching (`ValueTask DispatchAsync(IReadOnlyList<IDomainEvent>, CancellationToken)`).
- `EricksonLopez.SharedKernel.Dapper` — Dapper `SqlMapper.TypeHandler` support for strongly-typed identifiers (`DapperStrongIdRegistry`, `StrongIdTypeHandler<TSelf, TValue>`).
- `EricksonLopez.SharedKernel.EntityFrameworkCore` — EF Core integration (`DomainEventsInterceptor`, `StrongIdValueConverter<TId, TValue>`, `SharedKernelModelConfigurationExtensions`, `SharedKernelEntityFrameworkServiceCollectionExtensions`).
- `EricksonLopez.SharedKernel.Json` — System.Text.Json serialization support (`StrongIdJsonConverter<TSelf, TValue>`, `StrongIdJsonConverterFactory`).
- `EricksonLopez.SharedKernel.OpenTelemetry` — ActivitySource and Meter tracing decorator for `IDomainEventDispatcher` (`OpenTelemetryDomainEventDispatcher`, `SharedKernelInstrumentation`, `SharedKernelOpenTelemetryExtensions`).
- `EricksonLopez.SharedKernel.SourceGenerators` — Incremental source generators (`StrongIdGenerator`, `DapperRegistrationGenerator`).
- `EricksonLopez.SharedKernel.Testing` — Testing utilities (`DomainEventCollector`, `AggregateRootTestExtensions`).
- Comprehensive documentation suite: `docs/architecture.md`, `docs/api-reference.md`, `docs/public-api.md`, `docs/ecosystem.md`, `docs/ci-cd.md`, `docs/cookbook.md`, `docs/best-practices.md`, `docs/anti-patterns.md`, `docs/getting-started.md`, `docs/quick-start.md`, `docs/faq.md`, `docs/troubleshooting.md`, `docs/performance-guide.md`, `docs/migration-guide.md`, `docs/analysis/allocations.md`.
- 36 Architecture Decision Records (ADRs) under `docs/decisions/`.

---

## [2.0.0] — 2026-08-12

> [!IMPORTANT]
> This is a **breaking release**. All types added in v1.0.0 that were not domain primitives
> (`Result<T>`, `Error`, `ValueObject`, `Specification<T>`, `PaginationParameters`, `PagedList<T>`)
> were permanently removed. The library now has **zero external runtime dependencies**.

### ⚠ Breaking Changes

- **Redesigned core architecture** — removed all non-primitive types from the core package.
- `Result<T>` / `Result` — Result pattern removed. See [ADR-014](docs/decisions/ADR-014-removal-of-result-dependency.md). Consumers should adopt a dedicated Result library.
- `Error` struct and attributes (`ErrorDefinitionAttribute`, `ResultFactoryAttribute`) — removed.
- `ResultExtensions` — monadic and pipeline extension methods removed.
- `ValueObject` — Extracted to `EricksonLopez.DomainPrimitives` ecosystem package. See [ADR-017](docs/decisions/ADR-017-extraction-of-valueobject.md).
- `Specification<T>` / `ISpecification<T>` — Extracted/removed. See [ADR-008](docs/decisions/ADR-008-rejection-of-specification.md) and [ADR-016](docs/decisions/ADR-016-extraction-of-pagination.md).
- `PaginationParameters` / `PagedList<T>` — Extracted. See [ADR-016](docs/decisions/ADR-016-extraction-of-pagination.md).
- `EricksonLopez.SharedKernel.Testing` package (containing `ResultAssertions`) — removed from core solution.
- Namespace consolidation — `Entity<TId>`, `AggregateRoot<TId>`, and `IDomainEvent` moved from `EricksonLopez.SharedKernel.Domain` to root namespace `EricksonLopez.SharedKernel`.

### ✨ Features

- Add `AggregateRoot<TId>` domain primitive — transactional consistency boundary with lazy-allocated domain event collection ([9ca7053](https://github.com/ericksonlopezf/dotnet-shared-kernel/commit/9ca7053a9c7195db4ae6446943634e5bd16d16cf)).
- Redesign core architecture and remove obsolete classes ([958a385](https://github.com/ericksonlopezf/dotnet-shared-kernel/commit/958a3851e5f713492e152d38cd793592d0a676eb)).

### 📖 Documentation

- Match Codecov badge style with all other badges.
- `docs/Architecture.md`, `docs/API_REFERENCE.md`, `docs/PERFORMANCE_GUIDE.md`, `docs/MigrationGuide.md`, `docs/BestPractices.md`, `docs/AntiPatterns.md`, `docs/Cookbook.md`, `docs/FAQ.md`, `docs/GETTING_STARTED.md`, `docs/QUICK_START.md`, `docs/TROUBLESHOOTING.md`.
- 15 Architecture Decision Records (ADRs) under `docs/decisions/` documenting all key design decisions.

### 🔧 Maintenance

- SonarCloud integration added to CI (`dotnet-build-test.yml`).
- Stryker mutation testing JSON reporter added.
- `stryker-config.json` with thresholds: break=95, low=98, high=100.
- `EricksonLopez.SharedKernel.ArchitectureTests` — architecture enforcement tests via `NetArchTest.Rules`.
- `EricksonLopez.SharedKernel.Benchmarks` — BenchmarkDotNet benchmarks for equality, hashing, and zero-alloc domain event access.
- `samples/EricksonLopez.SharedKernel.AotConsole/` — NativeAOT compatibility verification sample used as the AOT gate in CI.

### 🐛 Fixed

- Architectural audit corrections applied across 11 identified findings (commit `13a4854`).

---

## [1.1.0] — 2026-07-23

### Added
- `AggregateRoot<TId>` domain primitive — transactional consistency boundary with lazy-allocated domain event collection.

### Fixed
- CI: Base64 decoding of the Strong Name key (SNK_KEY secret) made robust against newlines and empty secrets.

---

## [1.0.1] — 2026-07-21

### Changed
- Bumped `AwesomeAssertions` from `9.4.0` to `9.5.0` (Dependabot PR #9).

---

## [1.0.0] — 2026-07-16

### Added
- Initial project release.
- `Entity<TId>` — generic abstract base entity with identity-based equality, `GetHashCode()`, and `==`/`!=` operator overloads.
- `DomainEvent` — domain events base type.
- CI/CD workflows — GitHub Actions for build, test, coverage, and NuGet publishing triggered by version tags.

---

[Unreleased]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v2.0.0...v3.0.0
[2.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.1.0...v2.0.0
[1.1.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/releases/tag/v1.0.0

