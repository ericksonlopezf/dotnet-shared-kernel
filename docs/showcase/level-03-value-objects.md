# Level 03 — Pure Value Objects & Record Invariants

In Level 03, we explore immutable value objects and structural equality.

---

## 1. Struct vs. Class Value Objects

```csharp
using EricksonLopez.SharedKernel;

// Value object with structural equality
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");
            
        return new Money(Amount + other.Amount, Currency);
    }
}
```

---

## 2. Structural Equality Guarantee

`readonly record struct` provides zero-allocation structural equality out of the box without boilerplate `GetEqualityComponents()` overrides.
