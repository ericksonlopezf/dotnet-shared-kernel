# Level 04 — Repository & Unit of Work Contracts

In Level 04, we define pure application port contracts for domain aggregate persistence.

---

## 1. Repository Contract

```csharp
using EricksonLopez.SharedKernel;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    void Remove(Order order);
}
```

---

## 2. Unit of Work Contract

```csharp
using EricksonLopez.SharedKernel;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```
