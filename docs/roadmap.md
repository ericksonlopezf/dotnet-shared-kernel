# Strategic Roadmap & Milestones

---

## 1. Release Timeline & Enhancements

### v4.0.0 — Production Release (Current — 2026-09-20)
- Core `Entity<TId>`, `AggregateRoot<TId>`, `IStrongId<TSelf, TValue>`, and `IDomainEvent`.
- Zero `[Obsolete]` policy across all domain and infrastructure contracts.
- Roslyn Analyzers and `.editorconfig` zero-suppression enforcement.
- EF Core domain events interceptor and Dapper UNNEST batch parameter generator.
- OpenTelemetry Activity tracing and BCL metrics.
- 100% NativeAOT trimming safety, 100% test pass rate, and full mutation testing strategy.

### v3.0.0 — Production Release (2026-08-25)
- Core `Entity<TId>`, `AggregateRoot<TId>`, `IStrongId<TSelf, TValue>`, and `IDomainEvent`.
- EF Core domain events interceptor and Dapper UNNEST batch parameter generator.
- Compile-time incremental source generator for `[StronglyTypedId]`.
- OpenTelemetry Activity tracing and BCL metrics.
- 100% NativeAOT trimming safety and $\ge 99\%$ test coverage.
