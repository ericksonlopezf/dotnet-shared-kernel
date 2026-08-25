# Features Catalog & Specifications

---

## 1. Package Inventory & Core Types

### 1. `EricksonLopez.SharedKernel`
- `Entity<TId>`: Base class for domain entities with strongly-typed identity.
- `AggregateRoot<TId>`: Base class for aggregate roots with domain event encapsulation.
- `IEntityId<T>`: Contract for strongly-typed single-value identifiers.
- `IDomainEvent`: Marker interface for immutable domain events.
- `IRepository<TEntity>` & `IUnitOfWork`: Application port interfaces.

### 2. `EricksonLopez.SharedKernel.EntityFrameworkCore`
- `DomainEventsInterceptor`: Intercepts `DbContext.SaveChangesAsync` to extract and dispatch aggregate domain events.

### 3. `EricksonLopez.SharedKernel.Dapper`
- PostgreSQL `UNNEST` zero-allocation batch parameter builder.
- Type handlers for strongly-typed identifiers.

### 4. `EricksonLopez.SharedKernel.SourceGenerators`
- Incremental Roslyn source generator for `[StronglyTypedId]`.

### 5. `EricksonLopez.SharedKernel.OpenTelemetry` & `Testing`
- OpenTelemetry activity enrichment and test assertions for domain events.
