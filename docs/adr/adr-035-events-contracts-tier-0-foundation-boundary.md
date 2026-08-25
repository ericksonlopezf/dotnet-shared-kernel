# ADR-035: Elevation of Events.Contracts to Tier-0 Foundation Dependency

## Status
Accepted — August 2026

## Context
In Clean Architecture and Domain-Driven Design (DDD), `EricksonLopez.SharedKernel` provides core building blocks for rich domain models, specifically `AggregateRoot<TId>` and `DomainEvent`.

The domain event lifecycle requires an abstraction (`IDomainEvent`) to enable decoupling between event creation inside domain entities and event dispatching/transporting in application and infrastructure layers.

`EricksonLopez.Events.Contracts` defines `IDomainEvent`, `IEvent`, `EventId`, `EventVersion`, and `EventMetadata`. Because `EricksonLopez.SharedKernel` references `EricksonLopez.Events.Contracts`, an ecosystem audit evaluated whether this cross-repository dependency violates pure domain layering or represents a legitimate Tier-0 Foundation relationship.

## Decision
Formalize `EricksonLopez.Events.Contracts` as an immutable **Tier-0 Foundation Contract** package across the entire `EricksonLopez.*` ecosystem:

1. **Pure Interface Segregation**:
   - `EricksonLopez.Events.Contracts` contains zero external dependencies and zero internal ecosystem dependencies.
   - It contains strictly pure contracts, strong identifiers, and immutable value records.
   - It contains no runtime dispatchers, no reflection, and no serialization frameworks.

2. **Domain Layering Integrity**:
   - `EricksonLopez.SharedKernel.DomainEvent` implements `EricksonLopez.Events.IDomainEvent`.
   - Domain models inherit from `DomainEvent` without coupling to any dispatcher implementation (`EricksonLopez.Events`), broker transport (`EricksonLopez.Messaging`), or transactional outbox (`EricksonLopez.Outbox`).

3. **Stability Guarantees**:
   - `EricksonLopez.Events.Contracts` follows strict SemVer with a >95% backward binary compatibility guarantee.
   - No breaking changes are permitted without formal ecosystem-wide deprecation cycles.

## Consequences
- **Positive**: Complete polymorphism across the ecosystem. Domain events created in the domain layer can seamlessly flow into `EricksonLopez.Events` (in-process), `EricksonLopez.Events.Outbox` (transactional persistence), or `EricksonLopez.Messaging.Events` (distributed brokers) without mapping or reflection.
- **Positive**: Consumers can choose to reference only `SharedKernel` + `Events.Contracts` and write their own custom dispatching pipeline without installing any additional infrastructure NuGet packages.
- **Maintenance**: `Events.Contracts` must remain strictly dependency-free to preserve its Tier-0 Foundation classification.
