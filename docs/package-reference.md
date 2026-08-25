# Package Reference & Dependency Hierarchy

---

## 1. NuGet Packages

| Package | Description | Dependencies |
|---|---|---|
| [`EricksonLopez.SharedKernel`](https://nuget.org/packages/EricksonLopez.SharedKernel) | Foundational Tier-0 DDD building blocks (`Entity`, `AggregateRoot`, `IEntityId`) | None (BCL Only) |
| [`EricksonLopez.SharedKernel.EntityFrameworkCore`](https://nuget.org/packages/EricksonLopez.SharedKernel.EntityFrameworkCore) | EF Core domain event interceptor | `SharedKernel`, `EF Core` |
| [`EricksonLopez.SharedKernel.Dapper`](https://nuget.org/packages/EricksonLopez.SharedKernel.Dapper) | Dapper PostgreSQL UNNEST batch helper | `SharedKernel`, `Dapper` |
| [`EricksonLopez.SharedKernel.Json`](https://nuget.org/packages/EricksonLopez.SharedKernel.Json) | System.Text.Json converters for strongly typed IDs | `SharedKernel` |
| [`EricksonLopez.SharedKernel.SourceGenerators`](https://nuget.org/packages/EricksonLopez.SharedKernel.SourceGenerators) | Incremental Roslyn source generator | Roslyn 4.8 |
| [`EricksonLopez.SharedKernel.OpenTelemetry`](https://nuget.org/packages/EricksonLopez.SharedKernel.OpenTelemetry) | W3C activity tracing | `SharedKernel` |
| [`EricksonLopez.SharedKernel.Testing`](https://nuget.org/packages/EricksonLopez.SharedKernel.Testing) | Fluent assertions for domain events | `SharedKernel` |
