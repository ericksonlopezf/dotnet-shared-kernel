# ADR-027: Rejection of Generic `IEntity<TId>` Interface

**Date:** 2026-08-15  
**Status:** Rejected / Deferred indefinitely until explicit consumer demand arises  
**Deciders:** Erickson Lopez  
**Backlog Reference:** BL-007  
**Alias:** ADR-0007  

---

## Context

In enterprise domain modeling, frameworks and Object-Relational Mappers (ORMs) frequently introduce marker interfaces or base abstractions such as `IEntity<TId>` or `Entity<TId>` to enforce a unified identity contract across all aggregates and entities.

Within our architectural stack—built on Domain-Driven Design (DDD), Clean Architecture, and Dapper over PostgreSQL—imposing a global `IEntity<TId>` creates several structural problems:

1. **Coupling to Generic Repositories:** Generic entity interfaces inevitably incentivize generic repositories (`IRepository<TEntity, TId>`), which violate DDD aggregate boundaries, leak CRUD semantics into the domain, and dilute aggregate-specific business invariants.
2. **Identity Variance Restrictions:** Domain models leverage Strongly Typed IDs implemented as immutable value objects or `readonly record struct` types (e.g., `OrderId`, `CustomerId`). Enforcing a generic identity contract introduces unnecessary type parameters, covariance/contravariance overhead, and generic constraints throughout Domain and Application layers.
3. **Data Access Pragmatism:** Because data persistence is handled via pure SQL queries and micro-mapping through Dapper (optimized with PostgreSQL functions and UNNEST batches), persistence does not require polymorphic entity abstractions for tracking or key extraction.

## Decision

We will not implement a base `IEntity<TId>` interface in the Domain layer:

- Each Entity and Aggregate Root must manage and expose its own Strongly Typed ID explicitly.
- Repositories must remain aggregate-specific interfaces (e.g., `IOrderRepository`, `ICustomerRepository`) defined exclusively in the Application layer or Domain model as contracts.
- Equality and identity comparisons must be handled directly within domain boundaries using native C# records or explicit value object comparison semantics.

```csharp
// Recommended: Explicit, strongly-typed domain aggregate without generic identity abstractions
public readonly record struct InvoiceId(Guid Value)
{
    public static InvoiceId New() => new(Guid.NewGuid());
    public static InvoiceId From(Guid value) => new(value);
}

public sealed class Invoice : AggregateRoot<InvoiceId>
{
    public TenantId TenantId { get; private set; }
    public InvoiceStatus Status { get; private set; }

    private Invoice(InvoiceId id, TenantId tenantId)
    {
        Id = id;
        TenantId = tenantId;
        Status = InvoiceStatus.Draft;
    }

    public static Result<Invoice> Create(TenantId tenantId)
    {
        if (tenantId == TenantId.Empty)
            return Result.Failure<Invoice>(InvoiceErrors.InvalidTenant);

        return Result.Success(new Invoice(InvoiceId.New(), tenantId));
    }
}
```

## Consequences

### Positive
- **High Cohesion:** Prevents the proliferation of anemic generic repository antipatterns.
- **Type Safety:** Eliminates generic type noise (`TId`) across services, handlers, and specifications.
- **Native Dapper Compatibility:** Raw SQL mappings bind directly to concrete entity properties or factory methods without polymorphic casting.
- **Domain Autonomy:** Aggregates remain completely uncoupled from artificial framework-level abstractions.

### Negative / Trade-offs
- **No Unified Identity Reflection:** Cross-cutting infrastructure operations that require extracting an ID via reflection cannot rely on a single interface. These scenarios must be addressed explicitly via Domain Events or specialized Application-level wrappers.
