# Project Roadmap

This roadmap reflects the current state and completed milestones of `EricksonLopez.SharedKernel`.

> [!NOTE]
> Per the project's strict architecture principles (see ADR-002, ADR-014), this library is intentionally minimal. Features and abstractions not listed here were deliberately excluded.

---

## Phase 1 — The Core Foundation (v1.x → v2.0)

| Feature | Status |
|---|---|
| `Entity<TId>` — identity-based equality with `IsTransient()`, `==`/`!=` operators | ✅ Completed (v1.0.0) |
| `IDomainEvent` — marker interface for domain events | ✅ Completed (v1.0.0) |
| `AggregateRoot<TId>` — consistency boundary with lazy domain event allocation | ✅ Completed (v1.1.0) |
| NativeAOT and Trimming compatibility (`IsAotCompatible=true`, `IsTrimmable=true`) on all TFMs | ✅ Completed (v1.1.0) |
| CI/CD Pipeline — GitHub Actions (build, test, coverage, mutation, NuGet publishing) | ✅ Completed (v1.1.0) |
| 15 Architecture Decision Records (ADRs) | ✅ Completed (v2.0) |
| Full documentation suite (`/docs/`) | ✅ Completed (v2.0) |
| Architecture enforcement tests (`NetArchTest.Rules`) | ✅ Completed (v2.0) |
| BenchmarkDotNet performance benchmarks | ✅ Completed (v2.0) |
| SonarCloud static analysis integration | ✅ Completed (v2.0) |

---

## The Ecosystem Expansion (v2.x)

The `SharedKernel` is designed to be the foundational primitive of a broader ecosystem. The following packages have been extracted or built on top of it to handle specific concerns:

| Package | Description | Status |
|---|---|---|
| **`EricksonLopez.SharedKernel`** | Core DDD abstractions (`Entity`, `AggregateRoot`, `IDomainEvent`) | ✅ Published |
| **`EricksonLopez.DomainPrimitives`** | `ValueObject` base class and Source Generators for Strong Typed Ids | ✅ Published |
| **`EricksonLopez.Result`** | Result Pattern / Functional error handling | ✅ Published |
| **`EricksonLopez.Specification`** | Specification pattern for queries | ✅ Published |
| **`EricksonLopez.Pagination`** | Abstractions for offset/keyset pagination | ✅ Published |

---

> If you'd like to influence the project direction, please join the conversation in [GitHub Discussions](https://github.com/ericksonlopezf/dotnet-shared-kernel/discussions).

