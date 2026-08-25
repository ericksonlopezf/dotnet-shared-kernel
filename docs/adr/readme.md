# Architectural Decision Records (ADRs)

This directory documents all architectural decision records for `EricksonLopez.SharedKernel`.

---

## 📜 Index of Decisions

| ADR | Title | Status |
|---|---|---|
| [ADR-001](adr-001-result-pattern.md) | Result Pattern Integration Strategy | Accepted |
| [ADR-002](adr-002-zero-functional-dependencies.md) | Zero Functional Dependencies Policy | Accepted |
| [ADR-003](adr-003-value-object-boxing.md) | Value Object Struct vs Class Boxing Policy | Accepted |
| [ADR-004](adr-004-validation-error-design.md) | Validation Error Design | Accepted |
| [ADR-005](adr-005-result-pattern-allocations.md) | Result Pattern Zero-Allocation Policy | Accepted |
| [ADR-006](adr-006-performance-analysis.md) | Performance Analysis & Benchmark Gates | Accepted |
| [ADR-007](adr-007-native-aot-compatibility.md) | NativeAOT & Trimming Compatibility | Accepted |
| [ADR-008](adr-008-rejection-of-specification.md) | Rejection of Specification Pattern in Tier-0 | Accepted |
| [ADR-009](adr-009-target-framework-strategy.md) | Target Framework Strategy (.NET 8, 9, 10) | Accepted |
| [ADR-010](adr-010-lazy-allocation-domain-events.md) | Lazy Allocation for Domain Events | Accepted |
| [ADR-011](adr-011-rejection-of-concurrent-queue.md) | Rejection of ConcurrentQueue for Domain Events | Accepted |
| [ADR-012](adr-012-domain-error-omission.md) | Domain Error Omission in Core | Accepted |
| [ADR-013](adr-013-entity-id-initialization.md) | Entity ID Initialization & Immutability | Accepted |
| [ADR-014](adr-014-removal-of-result-dependency.md) | Decoupling Direct Result Dependency | Accepted |
| [ADR-015](adr-015-rejection-of-proxy-unboxing.md) | Rejection of Proxy Unboxing | Accepted |
| [ADR-016](adr-016-extraction-of-pagination.md) | Extraction of Pagination to Application Layer | Accepted |
| [ADR-017](adr-017-extraction-of-valueobject.md) | Extraction of Complex Value Objects | Accepted |
| [ADR-018](adr-018-rejection-of-generic-repository.md) | Rejection of Generic Repository Pattern | Accepted |
| [ADR-019](adr-019-rejection-of-unit-of-work.md) | Unit of Work Boundary Strategy | Accepted |
| [ADR-020](adr-020-rejection-of-auditing-fields.md) | Rejection of Intrusive Auditing Fields in Core Entity | Accepted |
| [ADR-021](adr-021-rejection-of-tenancy.md) | Rejection of Multi-Tenancy Coupling in Core | Accepted |
| [ADR-022](adr-022-rejection-of-security-abstractions.md) | Rejection of Security Abstractions in Domain | Accepted |
| [ADR-023](adr-023-rejection-of-soft-delete.md) | Rejection of Implicit Soft Delete | Accepted |
| [ADR-024](adr-024-rejection-of-clock-abstraction.md) | Rejection of Custom Clock Abstraction (Use TimeProvider) | Accepted |
| [ADR-025](adr-025-rejection-of-domain-service-marker.md) | Rejection of Domain Service Marker Interfaces | Accepted |
| [ADR-026](adr-026-rejection-of-business-rules.md) | Rejection of Custom Business Rules Engine | Accepted |
| [ADR-027](adr-027-rejection-of-generic-entity-interface.md) | Rejection of Generic Entity Interfaces | Accepted |
| [ADR-028](adr-028-result-first-architecture-over-domain-exceptions.md) | Result-First Architecture over Domain Exceptions | Accepted |
| [ADR-029](adr-029-mutation-testing-bcl-redundancy-policy.md) | Mutation Testing BCL Redundancy Policy | Accepted |
| [ADR-030](adr-030-method-scenario-result-test-naming.md) | Method_Scenario_Result Test Naming Convention | Accepted |
| [ADR-031](adr-031-sync-dispatcher-policy.md) | Synchronous Dispatcher Policy | Accepted |
| [ADR-032](adr-032-outbox-pattern-ecosystem-boundary.md) | Outbox Pattern Ecosystem Boundary | Accepted |
| [ADR-033](adr-033-domain-modeling-taxonomy-strong-ids-vs-domain-primitives-vs-value-objects.md) | Domain Modeling Taxonomy: Strong IDs vs Primitives vs Value Objects | Accepted |
| [ADR-034](adr-034-coexistence-source-generators-vs-runtime-domain-abstractions.md) | Source Generators vs Runtime Domain Abstractions | Accepted |
| [ADR-035](adr-035-events-contracts-tier-0-foundation-boundary.md) | Events Contracts Tier-0 Foundation Boundary | Accepted |
| [ADR-036](adr-036-rejection-of-custom-caching-abstraction.md) | Rejection of Custom Caching Abstractions | Accepted |
