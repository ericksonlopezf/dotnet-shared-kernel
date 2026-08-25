# Level 02 — Aggregate Roots & Domain Events

In Level 02, we encapsulate domain invariants and collect domain events within `AggregateRoot<TId>`.

---

## 1. Aggregate Root Implementation

```csharp
using EricksonLopez.SharedKernel;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount { get; private set; }

    private Order(Guid id, Guid customerId) : base(id)
    {
        CustomerId = customerId;
    }

    public static Order Create(Guid id, Guid customerId)
    {
        var order = new Order(id, customerId);
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(id, customerId, DateTimeOffset.UtcNow));
        return order;
    }

    public void AddItem(string productSku, decimal price, int quantity)
    {
        _items.Add(new OrderItem(productSku, price, quantity));
        TotalAmount += price * quantity;
        RaiseDomainEvent(new OrderItemAddedDomainEvent(Id, productSku, price, quantity));
    }
}
```

---

## 2. Dispatching Domain Events

Application services retrieve and clear raised domain events atomically:

```csharp
IReadOnlyCollection<IDomainEvent> events = order.PullDomainEvents();
// events are cleared from aggregate state, ready for dispatching
```
