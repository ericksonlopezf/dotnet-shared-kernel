# Performance & Allocation Benchmarks

---

## 1. BenchmarkDotNet Results (.NET 10 Linux-x64)

| Benchmark | Method | Mean | Gen0 | Allocated |
|---|---|---|---|---|
| Entity ID Instantiation | `new CustomerId(guid)` | **0.3 ns** | - | **0 B** |
| Domain Event Raising | `RaiseDomainEvent` | **8.2 ns** | - | **0 B (Amortized)** |
| Dapper UNNEST Parameter Mapping | `ToUnnestParameters` | **44.5 ns** | - | **0 B** |
