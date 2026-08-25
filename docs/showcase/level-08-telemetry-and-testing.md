# Level 08 — OpenTelemetry & Testing Doubles

In Level 08, we monitor domain operations and write isolated tests using `EricksonLopez.SharedKernel.OpenTelemetry` and `EricksonLopez.SharedKernel.Testing`.

---

## 1. Domain Event Test Assertions

```csharp
using EricksonLopez.SharedKernel.Testing;
using Xunit;

public class OrderTests
{
    [Fact]
    public void CreateOrder_ShouldRaise_OrderCreatedDomainEvent()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid());

        order.ShouldHaveRaisedDomainEvent<OrderCreatedDomainEvent>();
    }
}
```
