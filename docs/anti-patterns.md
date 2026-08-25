# Anti-Patterns & Common Pitfalls

---

## 1. Prohibited Anti-Patterns in SharedKernel

### ❌ Anti-Pattern 1: Leaking Infrastructure Types into Domain Primitives
```csharp
// BAD: Domain entity references EF Core or HTTP headers
public class User : Entity<Guid>
{
    public DbContext DbContext { get; set; } // VIOLATION!
}

// GOOD: Entities encapsulate pure business invariants and state
```

### ❌ Anti-Pattern 2: Heavy Inheritance Hierarchies
```csharp
// BAD: Deep polymorphic hierarchy for entities
public class AuditedSoftDeletableTenantEntity<TId> : Entity<TId> { ... }

// GOOD: Composition over inheritance with explicit value objects and interceptors
```
