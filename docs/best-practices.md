# Best Practices & Production Guidelines

---

## 1. Domain Modeling Best Practices

1. **Keep Entities Focused**: Aggregates define consistency boundaries. Do not design monolithic aggregate roots.
2. **Raise Domain Events for State Changes**: Never mutate aggregate state without raising a corresponding domain event if external systems depend on that state change.
3. **Use Strongly-Typed IDs**: Prevent parameter transposition errors (passing `customerId` instead of `orderId`) by wrapping primitives in strongly-typed ID record structs.
