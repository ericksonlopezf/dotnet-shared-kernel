# Project Roadmap

This roadmap reflects the evolutionary milestones, completed architectural phases, and active quality initiatives of `EricksonLopez.SharedKernel`.

> [!NOTE]
> Per the project's strict architecture principles (see [ADR-002](docs/decisions/ADR-002-zero-functional-dependencies.md), [ADR-014](docs/decisions/ADR-014-removal-of-result-dependency.md), and [ADR Discards](docs/adr-discards.md)), this library is intentionally minimal. Features and abstractions not listed here were deliberately excluded and documented in the discard catalogue.

---

## 🗺️ Evolutionary Milestones

| Milestone | Scope & Description | Status |
|---|---|---|
| **v1.0.0** | Initial Foundation: `Entity<TId>`, `DomainEvent`, GitHub Actions CI/CD for build, test, and NuGet publishing. | ✅ **Released** |
| **v1.0.1** | Quality Maintenance: Dependency updates (`AwesomeAssertions 9.5.0`), build pipeline fixes. | ✅ **Released** |
| **v1.1.0** | Aggregate Roots & Security: `AggregateRoot<TId>` with lazy event collection, Strong Naming automation, Package Validation baseline. | ✅ **Released** |
| **v2.0.0** | Core Extraction: Extraction of pagination and ValueObject, zero runtime dependencies. | ✅ **Released** |
| **v3.0.0** | Production Release: `IStrongId<TSelf, TValue>`, `IEntity<TId>`, `IHasDomainEvents`, `IAggregateRoot`, `ValueObject`, `ValueObjectAttribute`, 100% Mutation Score, OpenTelemetry, EF Core interceptors, Dapper UNNEST, SourceGenerators, Native AOT compilation gate. | ✅ **Released (2026-08-25)** |

---

## Active Initiatives

### v1.1.x — Correctness & Documentation (Patch)

Items identified by the functional parity audit (2026-08-19):

| Item | Type | Status |
|---|---|---|
| XML doc `DomainEventsInterceptor.SavingChanges` — deadlock risk warning | Documentation | ✅ Done |
| XML doc `DomainEventsInterceptor.SavingChangesAsync` — preferred path | Documentation | ✅ Done |
| Fix `IgnoreDomainEvents()` — removed inert `.Ignore()` call + accurate XML doc | Bug Fix | ✅ Done |
| Cookbook Recipe 6 — corrected to use real `DrainDomainEvents()` API | Bug Fix | ✅ Done |
| ADR-005 marked Superseded by ADR-014 | ADR Hygiene | ✅ Done |
| ADR-012 marked Superseded by ADR-028 | ADR Hygiene | ✅ Done |
| `DapperStrongIdRegistry.RegisterFromAssembly` — AOT warning in XML doc | Documentation | ✅ Done |
| Cookbook Recipe 11 — `IgnoreDomainEvents()` usage guide | Documentation | ✅ Done |
| ADR-031 — Sync dispatcher policy (Option A: document, no breaking change) | ADR | ✅ Done |

### v1.2.0 — Competitive Improvements

| Item | Type | Status |
|---|---|---|
| Benchmark public comparativo vs Ardalis.SharedKernel (`CompetitiveParityBenchmarks`) | Performance | ✅ Done |
| EF Core `ConfigureStrongIdsFromAssembly` & `ConfigureStrongIdsFromAssemblies` bulk registration | API | ✅ Done |
| Dapper `DapperStrongIdRegistry.RegisterFromAssemblies` multi-assembly scanning | API | ✅ Done |

### v2.x — Strategic Expansion

| Package / Feature | Scope | Status |
|---|---|---|
| `EricksonLopez.SharedKernel.SourceGenerators` | Source generator for `IStrongId` boilerplate | Planned |
| `EricksonLopez.SharedKernel.Analyzers` | Roslyn analyzer enforcing entity invariants | Planned |
| `EricksonLopez.SharedKernel.OpenTelemetry` | Domain events as OTel spans/metrics (no core contamination) | Researching |
| `EricksonLopez.Outbox` (Tier 3 Framework Integration) | Integrated via `DomainEventsInterceptor` and `IDomainEventDispatcher` in `EricksonLopez.Outbox` | Architectural Alignment Confirmed |

---


## Completed Initiatives

### 1. Domain Primitives & Core Contracts
- `Entity<TId>`: Base class for domain entities with identity-based equality (`Type` + `Id`), `==`/`!=` operators, and zero-boxing hash codes.
- `IEntity<TId>`: Generic contract exposing `Id` for polymorphic access.
- `AggregateRoot<TId>`: Transactional consistency boundary with lazy-allocated event sourcing (`0 B` heap allocation on read-only hydration).
- `IAggregateRoot`: Marker interface for Aggregate Roots, inheriting `IHasDomainEvents`.
- `IHasDomainEvents`: Non-generic contract for polymorphic inspection, collection, and clearing of domain events by infrastructure.
- `DomainEvent`: Time-ordered domain event base record with sequential UUIDv7 identity (`EventId` on .NET 9+) and UTC timestamp (`OccurredOn`).
- `ValueObject`: Base abstract record for multi-attribute value objects with compiler-generated structural equality and with-mutation support.
- `ValueObjectAttribute`: Static metadata attribute for Native AOT mappers and source generators.
- `IStrongId<TSelf, TValue>`: CRTP-based contract for strongly-typed entity identifiers.

### 2. Quality & Architecture Governance
- **100% Code Coverage**: 100.0% line, branch, and method coverage verified with Coverlet.
- **100% Mutation Testing**: Stryker.NET mutation testing score of 100.0% across all primitives.
- **Architecture Enforcement**: `NetArchTest.Rules` verifying zero external dependencies, zero sibling leaks, and pure BCL isolation.
- **Native AOT Certification**: `aot-smoke-test.yml` workflow compiling native binaries with `PublishAot=true` and `TreatWarningsAsErrors=true`.
- **Formal ADR Catalog**: 29 formal Architecture Decision Records (`docs/decisions/`) and master catalog of discards (`docs/adr-discards.md`).

---

## Long-Term Maintenance & LTS Policy

| Initiative | Description | Policy |
|---|---|---|
| **Zero Breaking Changes** | Public API is frozen to the core domain primitives. All future enhancements must adhere strictly to SemVer. | Strict |
| **Active .NET LTS Support** | Multi-target active .NET versions (`net8.0`, `net9.0`, `net10.0`). Deprecate older TFMs only when officially reaching Microsoft End-of-Life ([ADR-009](docs/decisions/ADR-009-target-framework-strategy.md)). | Continuous |
| **Zero External Dependencies** | Refuse any external runtime NuGet dependencies in Tier 0. Only .NET BCL types are permitted. | Invariant |
| **Continuous AOT Compatibility** | Zero reflection, zero expression trees, and continuous verification via Native AOT smoke testing. | Invariant |

---

## 🏛️ Ecosystem Package Map

The `SharedKernel` is the foundational Tier 0 primitive of the broader `EricksonLopez.*` ecosystem:

```
Tier 0 (Foundations):
├── EricksonLopez.SharedKernel       [Entity, AggregateRoot, DomainEvent, ValueObject, IStrongId] (BCL Only)
├── EricksonLopez.DomainPrimitives   [SourceGen Primitives, StronglyTypedId Tooling] (BCL Only)
└── EricksonLopez.Result             [Result<T>, Error] (BCL Only)

Tier 1 (Domain Behaviors & Envelopes):
├── EricksonLopez.ValueObjects       [Money, Range, Address, Fiscal VOs] (Depends on SharedKernel)
├── EricksonLopez.Specification      [Specification<T>] (Depends on SharedKernel)
└── EricksonLopez.Events             [IIntegrationEvent, EventEnvelope] (Depends on SharedKernel)

Tier 2 (Application & Workflows):
├── EricksonLopez.Mediator           [ICommand, IQuery, IHandler] (Depends on Result)
├── EricksonLopez.Processes          [ISaga, IProcess] (Depends on Mediator)
├── EricksonLopez.Mapper             [IMapper] (BCL Only)
└── EricksonLopez.Pagination         [PagedList<T>, PaginationParameters] (BCL Only)

Tier 3 (Infrastructure & Persistence):
├── EricksonLopez.Messaging          [IMessageTransport, Consumers] (Depends on Events)
├── EricksonLopez.Outbox             [OutboxMessage, IOutboxStore] (Depends on Events)
└── EricksonLopez.SqlBuilder         [ISqlBuilder, Dapper Extensions] (BCL Only)
```

For in-depth details, refer to the [Ecosystem Reference Architecture](docs/ecosystem.md).
