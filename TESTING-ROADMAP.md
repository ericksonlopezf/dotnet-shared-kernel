# Framework Testing Roadmap

## 1. Objectives

This document serves as the **single source of truth, execution guide, reproducible evidence, and idempotent tracking mechanism** for the testing, cleanup, and mutation testing strategy of `EricksonLopez.SharedKernel`.

The framework adheres to the following quality gates:

| Metric | Target | Actual | Acceptance Gate | Status |
|---|---:|---:|:---:|:---:|
| **Line Coverage** | **100%** | **100%** | Mandatory | **PASSED** |
| **Branch Coverage** | **100%** | **100%** | Mandatory | **PASSED** |
| **Method Coverage** | **100%** | **100%** | Mandatory | **PASSED** |
| **Mutation Score** | **100%** | **100%** | Mandatory | **PASSED** |

---

## 2. Framework Structure

The framework is partitioned into high-cohesion, low-coupling packages following Domain-Driven Design (DDD) principles and Clean Architecture contracts:

```
src/
├── EricksonLopez.SharedKernel/                  # Core DDD primitives: Entity, AggregateRoot, DomainEvent, IDomainEventDispatcher
├── EricksonLopez.SharedKernel.Dapper/           # Dapper type handlers and compile-time AOT registries for StrongIds
├── EricksonLopez.SharedKernel.EntityFrameworkCore/ # EF Core interceptors, value converters, and model configuration extensions
├── EricksonLopez.SharedKernel.Json/             # System.Text.Json converters and converter factories for StrongIds
├── EricksonLopez.SharedKernel.OpenTelemetry/    # Observability, Activity tracing, and metrics for domain event pipelines
├── EricksonLopez.SharedKernel.SourceGenerators/ # Roslyn Incremental Source Generators for StrongIds and Dapper registrations
└── EricksonLopez.SharedKernel.Testing/          # Test spies and assertion extensions (DomainEventCollector)
```

---

## 3. Public API Surface

| Package | Key Public Types / APIs | Role / Contract |
|---|---|---|
| `EricksonLopez.SharedKernel` | `Entity<TId>`, `IEntity<TId>`, `IEntity` | Identity-based domain entity baseline |
| `EricksonLopez.SharedKernel` | `AggregateRoot<TId>`, `IAggregateRoot`, `IHasDomainEvents` | Consistency boundary with encapsulated event collection |
| `EricksonLopez.SharedKernel` | `DomainEvent`, `IDomainEventDispatcher` | Immutable time-ordered domain event baseline and dispatch contract |
| `EricksonLopez.SharedKernel.Dapper` | `DapperStrongIdRegistry`, `StrongIdTypeHandler<TSelf, TValue>` | Dapper primitive mapping and AOT registration |
| `EricksonLopez.SharedKernel.EntityFrameworkCore` | `DomainEventsInterceptor`, `StrongIdValueConverter<TId, TValue>` | EF Core lifecycle interception and value conversions |
| `EricksonLopez.SharedKernel.EntityFrameworkCore` | `SharedKernelModelConfigurationExtensions`, `SharedKernelEntityFrameworkServiceCollectionExtensions` | EF Core model configuration and DI registration extensions |
| `EricksonLopez.SharedKernel.Json` | `StrongIdJsonConverter<TSelf, TValue>`, `StrongIdJsonConverterFactory` | System.Text.Json serialization and converter discovery |
| `EricksonLopez.SharedKernel.OpenTelemetry` | `OpenTelemetryDomainEventDispatcher`, `SharedKernelInstrumentation`, `SharedKernelOpenTelemetryExtensions` | Distributed tracing and metrics decoration |
| `EricksonLopez.SharedKernel.SourceGenerators` | `StrongIdGenerator`, `DapperRegistrationGenerator` | Incremental Roslyn code generation for StrongIds and Dapper |
| `EricksonLopez.SharedKernel.Testing` | `DomainEventCollector`, `AggregateRootTestExtensions` | In-memory test spy and fluent event assertion harness |

---

## 4. Work Unit Tracking Matrix

| Unit ID | Unit Name | Type | Status | Tests Passed | Line Coverage | Branch Coverage | Method Coverage | Mutation Score |
|---|---|---|:---:|---:|---:|---:|---:|---:|
| **U01** | `Core.Entity` | `PUBLIC_API` | `DONE` | 33 | 100% | 100% | 100% | 100% |
| **U02** | `Core.AggregateRoot` | `PUBLIC_API` | `DONE` | 13 | 100% | 100% | 100% | 100% |
| **U03** | `Core.DomainEvent` | `PUBLIC_API` | `DONE` | 14 | 100% | 100% | 100% | 100% |
| **U04** | `Dapper.StrongIdRegistry` | `PUBLIC_API` | `DONE` | 10 | 100% | 100% | 100% | 100% |
| **U05** | `Dapper.StrongIdTypeHandler` | `INTERNAL_API` | `DONE` | 12 | 100% | 100% | 100% | 100% |
| **U06** | `EFCore.DomainEventsInterceptor` | `PUBLIC_API` | `DONE` | 38 | 100% | 100% | 100% | 100% |
| **U07** | `EFCore.StrongIdValueConverter` | `PUBLIC_API` | `DONE` | 16 | 100% | 100% | 100% | 100% |
| **U08** | `EFCore.ModelExtensions` | `EXTENSION` | `DONE` | 16 | 100% | 100% | 100% | 100% |
| **U09** | `Json.StrongIdJsonConverter` | `PUBLIC_API` | `DONE` | 55 | 100% | 100% | 100% | 100% |
| **U10** | `OpenTelemetry.Observability` | `PUBLIC_API` | `DONE` | 12 | 100% | 100% | 100% | 100% |
| **U11** | `SourceGenerators.StrongIdGenerator` | `GENERATOR` | `DONE` | 16 | 100% | 100% | 100% | 100% |
| **U12** | `SourceGenerators.DapperRegistrationGenerator` | `GENERATOR` | `DONE` | 10 | 100% | 100% | 100% | 100% |
| **U13** | `Testing.Utilities` | `PUBLIC_API` | `DONE` | 33 | 100% | 100% | 100% | 100% |

---

## 5. Execution Cycle per Unit

Each work unit was processed strictly according to the 17-step idempotent protocol:

1. **READ**: Read `TESTING-ROADMAP.md` to identify the active unit.
2. **RECONCILE**: Inspect current source code and existing tests against the roadmap.
3. **ANALYZE**: Determine contracts, preconditions, postconditions, branch logic, invariants, and edge cases.
4. **PLAN**: Formulate test specifications (xUnit, NSubstitute, AwesomeAssertions, FsCheck, AutoFixture).
5. **CLEAN**: Execute clean build and remove stale coverage/stryker artifacts.
6. **IMPLEMENT / IMPROVE TESTS**: Implement missing test cases and tighten existing assertions.
7. **BUILD**: Rebuild project and test project with zero warnings.
8. **TEST**: Execute tests and verify 100% pass rate.
9. **COVERAGE**: Measure line, branch, and method coverage (target 100%).
10. **MUTATION**: Run Stryker mutation testing for the unit.
11. **FIX**: Analyze surviving mutants, eliminate them with targeted tests or refactorings.
12. **CLEAN**: Perform clean build.
13. **VERIFY**: Re-execute test and coverage suites from clean state.
14. **DOCUMENT**: Record metrics, evidence, exclusions, and decisions in `TESTING-ROADMAP.md`.
15. **CLOSE**: Mark unit as `DONE`.
16. **RESET CONTEXT**: Clear transient execution state.
17. **NEXT UNIT**: Advance to the next pending unit.

---

## 6. Source Generators & Compilers

- **`StrongIdGenerator`**:
  - Validates `[StrongId]`, `[StrongId<T>]`, and `IStrongId<TSelf, TValue>` interfaces.
  - Generates partial definitions for `record struct`, `record class`, `struct`, and `class`.
  - Synthesizes `Value` property, `PrimitiveName`, `IsDefault`, `Create`, `TryCreate`, `New`, `Empty`, `IsEmpty`, conversion operators, and structural equality.
  - Fully verified across 16 unit tests, 100% line/branch/method coverage, 100% mutation score.

- **`DapperRegistrationGenerator`**:
  - Scans all `IStrongId<TSelf, TValue>` implementations in compilation.
  - Generates zero-reflection AOT bulk registration extension `GeneratedDapperStrongIdRegistryExtensions.RegisterAllGeneratedStrongIds()`.
  - Fully verified across 10 unit tests, 100% line/branch/method coverage, 100% mutation score.

---

## 7. Stryker Configuration

Stryker mutation thresholds configured across all configs:

```json
{
  "thresholds": {
    "high": 100,
    "low": 98,
    "break": 95
  }
}
```

---

## 8. Justified Exclusions

All exclusions are documented on a case-by-case basis with architectural rationale:

| Code Location | Category | Reason / Rationale | Date |
|---|---|---|---|
| `SharedKernelEntityFrameworkServiceCollectionExtensions.cs:L25,L42` | Defensive Null Check | `services.AddScoped` already validates null `services` argument with identical exception semantics | 2026-08-20 |
| `StrongIdJsonConverter.cs:L64` | Defensive Null Check | `JsonSerializer.Serialize` internally validates and throws `ArgumentNullException` for null `Utf8JsonWriter` | 2026-08-20 |
| `OpenTelemetryDomainEventDispatcher.cs:L65` | Async Infrastructure | `ConfigureAwait(false)` is library infrastructure to prevent synchronization context capture | 2026-08-20 |
| `OpenTelemetryDomainEventDispatcher.cs:L78` | Resource Timing | `stopwatch.Stop()` freezes local timer before elapsed measurement; local stopwatch is discarded | 2026-08-20 |
| `AggregateRootTestExtensions.cs:L24` | Defensive Null Check | `collector.CollectFrom` already validates and throws `ArgumentNullException` for null `aggregate` | 2026-08-20 |
| `StrongIdGenerator.cs:L70,L88` | Roslyn Symbol Null Check | Defensive null check for Roslyn AST symbol resolution | 2026-08-20 |
| `DapperRegistrationGenerator.cs:L83` | Roslyn Symbol Null Check | Defensive null check for Roslyn attribute class symbol | 2026-08-20 |

---

## 9. Full Suite Test & Integration Proof

```
Solution Test Run Summary (EricksonLopez.SharedKernel.slnx):

  EricksonLopez.SharedKernel.UnitTests.dll (net10.0)              ->  98 Passed (1 s)    [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.Dapper.Tests.dll (net10.0)            ->  22 Passed (965 ms) [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.dll         ->  70 Passed (5 s)    [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.Json.Tests.dll (net10.0)              ->  55 Passed (264 ms) [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.OpenTelemetry.Tests.dll (net10.0)     ->  12 Passed (441 ms) [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.SourceGenerators.Tests.dll (net10.0)  ->  26 Passed (2 s)    [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.Testing.Tests.dll (net8.0, 9.0, 10.0) ->  33 Passed (1.7 s)  [100% Line, 100% Branch, 100% Method]
  EricksonLopez.SharedKernel.ArchitectureTests.dll (net10.0)      ->  20 Passed (101 ms)
  EricksonLopez.SharedKernel.IntegrationTests.dll (net10.0 AOT)    ->   2 Passed (6 m)

Total Tests: 338
Total Passed: 338 (100%)
Total Failed: 0
Total Skipped: 0
Trimming/AOT Warnings (IL2026, IL3050): 0
Compiler Warnings (TreatWarningsAsErrors=true): 0
```

---

## 10. History & Audit Log

| Timestamp | Unit | Action | Summary / Outcome |
|---|---|---|---|
| 2026-08-22 | Global | Initialization | Framework testing roadmap initialized and verified against codebase architecture. |
| 2026-08-22 | U01-U03 | Verification | Verified `EricksonLopez.SharedKernel` core domain abstractions (98 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | U04-U05 | Verification | Verified `EricksonLopez.SharedKernel.Dapper` type handlers & registry (22 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | U06-U08 | Verification | Verified `EricksonLopez.SharedKernel.EntityFrameworkCore` interceptor & converters (70 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | U09 | Verification | Verified `EricksonLopez.SharedKernel.Json` converters & factories (55 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | U10 | Verification | Verified `EricksonLopez.SharedKernel.OpenTelemetry` tracing & metrics (12 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | U11-U12 | Verification | Verified `EricksonLopez.SharedKernel.SourceGenerators` incremental code generation (26 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | U13 | Verification | Verified `EricksonLopez.SharedKernel.Testing` multi-target test harness (33 tests, 100% coverage, 100% mutation score). |
| 2026-08-22 | Global | Regression Proof | Full solution build, architecture verification (20/20), and Native AOT integration publish passed with 0 warnings. |

---

## 11. Completion Criteria

A unit is marked `DONE` if and only if:
- [x] Public and internal contracts are fully specified and tested.
- [x] Preconditions, postconditions, and invariants are asserted.
- [x] Edge cases, null, default, and boundary inputs are covered.
- [x] 100% of tests pass (338/338).
- [x] Line Coverage >= 100%.
- [x] Branch Coverage >= 100%.
- [x] Method Coverage >= 100%.
- [x] Mutation Score = 100%.
- [x] All surviving mutants are eliminated or justified.
- [x] Verified from a clean rebuild (`dotnet clean && dotnet build`).
- [x] Evidence recorded in this roadmap.
