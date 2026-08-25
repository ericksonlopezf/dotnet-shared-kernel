# Master Catalog of Discarded Architectural Decisions (ADR Discards)

> **Repository:** `EricksonLopez.SharedKernel`  
> **Role:** Foundation Tier 0 Domain Primitives  
> **Scope:** Clean Architecture, DDD, Native AOT, Performance Engineering

This document consolidates all formal architectural rejections and discards. To preserve the Tier 0 purity, zero-dependency policy, and sub-nanosecond performance of `EricksonLopez.SharedKernel`, the following 28 concepts have been systematically evaluated and rejected.

---

## Summary Matrix of Discarded Features

| ID | Concept / Pattern | Reason for Discard | Correct Architectural Home | Formal ADR |
|---|---|---|---|---|
| **D01** | Generic Repository (`IRepository<T>`) | Persistence ignorance violation; leaker of infrastructure concerns; damages DDD aggregate boundaries. | Infrastructure Layer of Consumer | [ADR-018](decisions/ADR-018-rejection-of-generic-repository.md) |
| **D02** | Unit of Work (`IUnitOfWork`) | Transaction coordination is a database concern; `DbContext` or custom UoW already handles this; AR event collection is sufficient. | Infrastructure Layer of Consumer | [ADR-019](decisions/ADR-019-rejection-of-unit-of-work.md) |
| **D03** | Auditing Traits (`IAuditable`, `CreatedAt`, `CreatedBy`) | Persistence metadata disguised as domain; varies per Bounded Context; best automated by ORM interceptors. | Infrastructure Interceptors / EF Core | [ADR-020](decisions/ADR-020-rejection-of-auditing-fields.md) |
| **D04** | Multi-Tenancy (`ITenant`, `TenantId`) | Not universally applicable; binds single-tenant apps to tenancy models; multiple conflicting tenancy strategies. | Consumer Domain / Application / Inf | [ADR-021](decisions/ADR-021-rejection-of-tenancy.md) |
| **D05** | Security / User Identity (`ICurrentUser`, `UserId`, `Claims`) | Domain models business logic, not HTTP authentication or JWT claims; prevents isolated unit testing. | Application & Infrastructure Layers | [ADR-022](decisions/ADR-022-rejection-of-security-abstractions.md) |
| **D06** | Soft Delete (`ISoftDeletable`, `DeletedAt`) | Persistence detail; obscures domain state transitions (`Cancel()`, `Archive()`); handled via ORM Global Query Filters. | Consumer Domain State / Infrastructure | [ADR-023](decisions/ADR-023-rejection-of-soft-delete.md) |
| **D07** | Custom Clock (`IClock`, `IDateTimeProvider`) | Redundant with standard BCL `System.TimeProvider` (.NET 8+); domain methods should take explicit timestamps. | BCL `System.TimeProvider` | [ADR-024](decisions/ADR-024-rejection-of-clock-abstraction.md) |
| **D08** | Domain Service Marker (`IDomainService`) | Empty marker interface adds zero behavioral value; type naming conventions are sufficient. | Consumer Domain Layer | [ADR-025](decisions/ADR-025-rejection-of-domain-service-marker.md) |
| **D09** | Business Rule Abstractions (`IBusinessRule`, `IDomainRule`) | Overlaps with Guard Clauses, `EricksonLopez.Result`, and `Specification`; adds heap allocation on state checks. | Guard Clauses / Result / Specification | [ADR-026](decisions/ADR-026-rejection-of-business-rules.md) |
| **D10** | Value Object Base Class (Reflection-based) | Boxing overhead on struct types; reflection incompatible with Native AOT; C# records solve this natively. | `EricksonLopez.DomainPrimitives` / C# records | [ADR-003](decisions/ADR-003-value-object-boxing.md), [ADR-017](decisions/ADR-017-extraction-of-valueobject.md) |
| **D11** | Specification Pattern (`Specification<T>`) | `Expression.Compile()` generates IL at runtime (`IL3050`), crashing under Native AOT. | `EricksonLopez.Specification` | [ADR-008](decisions/ADR-008-rejection-of-specification.md) |
| **D12** | Thread-Safe Event Queue (`ConcurrentQueue<IDomainEvent>`) | Eager allocation on read-only aggregates; Aggregate Roots are single-threaded consistency boundaries by design. | Not Applicable (Keep `List<T>?` lazy) | [ADR-011](decisions/ADR-011-rejection-of-concurrent-queue.md) |
| **D13** | Domain Error Types (`DomainError`, `Error`) | Extracted into functional error library to avoid cross-tier coupling. | `EricksonLopez.Result` | [ADR-012](decisions/ADR-012-domain-error-omission.md), [ADR-014](decisions/ADR-014-removal-of-result-dependency.md) |
| **D14** | ORM Proxy Unboxing (`GetUnproxiedType()`) | Leaked Castle DynamicProxy into domain layer; string allocations in hot `Equals()` path. | Infrastructure Equality Comparers | [ADR-015](decisions/ADR-015-rejection-of-proxy-unboxing.md) |
| **D15** | Keyset / Offset Pagination (`PagedList<T>`) | Application/presentation concern; extracted to specialized package. | `EricksonLopez.Pagination` | [ADR-016](decisions/ADR-016-extraction-of-pagination.md) |
| **D16** | Outbox Message Entity (`OutboxMessage`) | Infrastructure transactional persistence pattern; depends on serialization and relational databases. | `EricksonLopez.Outbox` | Anti-Feature Matrix |
| **D17** | Messaging Bus / Dispatcher (`IEventBus`) | Distributed transport infrastructure concern (RabbitMQ, Kafka, Azure Service Bus). | `EricksonLopez.Messaging` | Anti-Feature Matrix |
| **D18** | Mediator / CQRS (`ICommand`, `IQuery`, `IMediator`) | In-process application orchestration pattern; not core domain primitives. | `EricksonLopez.Mediator` | Anti-Feature Matrix |
| **D19** | Object Mapper (`IMapper`) | Data transformation concern for DTOs and view models. | `EricksonLopez.Mapper` | Anti-Feature Matrix |
| **D20** | SQL Builder / Query Generator | Database query construction infrastructure. | `EricksonLopez.SqlBuilder` | Anti-Feature Matrix |
| **D21** | Integration Event Metadata in `DomainEvent` | Headers, CorrelationId, and Envelopes belong in the messaging envelope, not the domain contract. | `EricksonLopez.Events` | API Design Decisions |
| **D22** | Service Locator / Ambient DI | Anti-pattern; hides dependencies and introduces global mutable state. | Constructor Injection | [Anti-Patterns](anti-patterns.md) |
| **D23** | Concurrency Tokens / RowVersion | ORM and optimistic concurrency control detail; not universal domain identity. | Infrastructure Layer | Anti-Feature Matrix |
| **D24** | Roslyn Source Generators in Core Package | Adds heavy Roslyn toolchain dependencies; core SharedKernel must be zero-friction. | `EricksonLopez.DomainPrimitives` | SourceGen Decision |
| **D25** | Logging Abstractions (`ILogger`) | Cross-cutting infrastructure concern; utilize standard BCL `Microsoft.Extensions.Logging.Abstractions` if needed. | BCL `ILogger` | Anti-Feature Matrix |
| **D26** | Caching Abstractions (`ICache`) | Infrastructure storage and expiration concern. | BCL `IMemoryCache` / Distributed Cache | Anti-Feature Matrix |
| **D27** | Heavyweight Generic Repositories & Full CRUD Interfaces | Overengineering antipattern; incentivizes generic repos; restricts ID variance. Lightweight `IEntity<TId>` is provided strictly for polymorphic identity access. | Aggregate-Specific Repositories | [ADR-027](decisions/ADR-027-rejection-of-generic-entity-interface.md) |
| **D28** | Domain Exceptions (`DomainException`) | Violates Result-first determinism; hidden control flow; heavy stack trace allocation in high-throughput paths. | `EricksonLopez.Result` / `Error` records | [ADR-028](decisions/ADR-028-result-first-architecture-over-domain-exceptions.md) |

---

## Architectural Principles Enforced by Discards

1. **Tier 0 Foundation:** `EricksonLopez.SharedKernel` must depend **only** on the .NET Base Class Library (BCL).
2. **Zero Allocation by Default:** Read-only entity hydration produces 0 bytes of event-collection memory allocations.
3. **100% Native AOT & Trimming:** No reflection, no runtime IL emission (`IL3050`), and unconditional compile-time trim safety.
4. **Single-Responsibility Micro-Libraries:** Features are encapsulated in dedicated packages rather than bloated into a monolithic god-package.
