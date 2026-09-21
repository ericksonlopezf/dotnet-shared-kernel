# Level 06 — Dapper Strongly-Typed ID Persistence

In Level 06, we integrate zero-allocation strongly-typed identifiers and BCL date/time types with Dapper using `EricksonLopez.SharedKernel.Dapper`.

---

## 1. Registering Dapper Type Handlers

```csharp
using Dapper;
using System.Data;
using EricksonLopez.SharedKernel.Dapper;

// Register all StrongId and BCL date/time handlers
DapperStrongIdRegistry.RegisterStrongIdsFromAssembly(typeof(OrderId).Assembly);
DapperBclTypeHandlerRegistry.RegisterBclTypeHandlers();
```

---

## 2. Type-Safe Parameterization and Querying

```csharp
public async Task<Order?> GetOrderByIdAsync(IDbConnection db, OrderId orderId)
{
    const string sql = @"
        SELECT id, customer_id, total_amount, created_at
        FROM orders
        WHERE id = @OrderId;";

    // Strongly-typed IDs are mapped seamlessly by Dapper without boxing or reflection overhead
    return await db.QuerySingleOrDefaultAsync<Order>(sql, new { OrderId = orderId });
}
```
