# Level 05 — Entity Framework Core Persistence Integration

In Level 05, we integrate domain models with EF Core using `EricksonLopez.SharedKernel.EntityFrameworkCore`.

---

## 1. Domain Event Interceptor

Automatically dispatch or save domain events during `SaveChangesAsync`:

```csharp
using Microsoft.EntityFrameworkCore;
using EricksonLopez.SharedKernel.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddDomainEventsInterceptor();
    }
}
```
