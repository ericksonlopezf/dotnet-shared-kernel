# Level 01 — Entities & Strongly-Typed Identifiers

In Level 01, we model domain entities with strongly-typed identifiers to eliminate Primitive Obsession.

---

## 1. Strongly-Typed ID Definition

```csharp
using EricksonLopez.SharedKernel;

public readonly record struct OrderId(Guid Value) : IEntityId<Guid>
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId Empty => new(Guid.Empty);
}
```

---

## 2. Defining an Entity

```csharp
using EricksonLopez.SharedKernel;

public sealed class Customer : Entity<Guid>
{
    public string FullName { get; private set; }
    public string Email { get; private set; }

    public Customer(Guid id, string fullName, string email) : base(id)
    {
        FullName = fullName;
        Email = email;
    }
}
```
