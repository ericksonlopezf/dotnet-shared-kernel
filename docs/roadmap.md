# Strategic Roadmap & Milestones

---

## 1. Release Timeline & Enhancements

### v3.0.0 — Production Release (Current)
- Core `Entity<TId>`, `AggregateRoot<TId>`, `IEntityId<T>`, and `IDomainEvent`.
- EF Core domain events interceptor and Dapper UNNEST batch parameter generator.
- Compile-time incremental source generator for `[StronglyTypedId]`.
- OpenTelemetry Activity tracing and BCL metrics.
- 100% NativeAOT trimming safety and $\ge 99\%$ test coverage.

### v3.1.0 — Planned Enhancements
- High-speed SIMD vector validation for multi-property value object equality.
- Source-generated binary serializers for zero-allocation domain event logging.
