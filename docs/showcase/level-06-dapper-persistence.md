# Level 06 — Dapper High-Throughput Batch Persistence

In Level 06, we execute zero-allocation bulk queries and PostgreSQL `UNNEST` batch persistence using `EricksonLopez.SharedKernel.Dapper`.

---

## 1. Batch Inserts with PostgreSQL `UNNEST`

```csharp
using Dapper;
using System.Data;
using EricksonLopez.SharedKernel.Dapper;

public async Task BatchInsertOrdersAsync(IDbConnection db, IEnumerable<Order> orders)
{
    const string sql = @"
        INSERT INTO orders (id, customer_id, total_amount)
        SELECT * FROM UNNEST(@Ids, @CustomerIds, @Totals);";

    await db.ExecuteAsync(sql, orders.ToUnnestParameters());
}
```
