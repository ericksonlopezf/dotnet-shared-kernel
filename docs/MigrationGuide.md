# Migration Guide

This guide will help you migrate your existing code to `EricksonLopez.SharedKernel`, focusing particularly on adopting the `Result` pattern and Domain-Driven Design (DDD) primitives.

## 1. Moving Away from Exceptions for Control Flow

If your application previously threw exceptions for expected domain violations (like "User not found" or "Invalid email"), you should migrate these to the `Result` pattern.

### Before (Exceptions)
```csharp
public User GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        throw new NotFoundException($"User {id} not found.");
    
    if (!user.IsActive)
        throw new BusinessRuleException("User is not active.");
        
    return user;
}
```

### After (Result Pattern)
```csharp
public Result<User> GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        return Error.NotFound("User.NotFound", $"User {id} not found.");
    
    if (!user.IsActive)
        return Error.Conflict("User.Inactive", "User is not active.");
        
    return user; // Implicitly converts to Result<User>.Success(user)
}
```

## 2. Migrating from FluentResults / CSharpFunctionalExtensions

If you are coming from other popular Result pattern libraries, the concepts are very similar but the API might differ slightly.

### Key Differences
- **Immutability:** Our `Result` and `Error` types are strictly immutable.
- **Errors:** We don't have multiple error types (no `IError` interfaces). Everything is a unified `sealed record Error` with an `ErrorType` enum (e.g., `Validation`, `NotFound`, `Conflict`).
- **Success without Value:** We use `Result` (non-generic) instead of `Result.Ok()`.
- **Match instead of Switch/If:** We strongly encourage the use of the `.Match()` fluent extension to handle both success and failure branches exhaustively.

### Example Translation

**From CSharpFunctionalExtensions:**
```csharp
Result<User> result = Result.Failure<User>("User not found");
```

**To EricksonLopez.SharedKernel:**
```csharp
Result<User> result = Error.NotFound("User.NotFound", "User not found");
```

## 3. Adopting Entity and AggregateRoot

When migrating your domain entities, inherit from `Entity<TId>` or `AggregateRoot<TId>`.

### Before
```csharp
public class Order
{
    public Guid Id { get; set; }
    // ...
}
```

### After
```csharp
public sealed class Order : AggregateRoot<Guid>
{
    // Id is protected set in the base class, initialize it via constructor or factory
    private Order(Guid id) 
    {
        Id = id;
    }
    
    public static Order Create(Guid id)
    {
        var order = new Order(id);
        order.RaiseDomainEvent(new OrderCreated(id)); // Domain events now belong to AggregateRoot
        return order;
    }
}
```

**Note:** Entities no longer have the `RaiseDomainEvent` method. That responsibility has been moved exclusively to `AggregateRoot<TId>` to enforce consistency boundaries.

## 4. Value Objects

If you used custom structural equality in your types, migrate to `ValueObject` by overriding `GetEqualityComponents()`.

```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }

    public Address(string street, string city)
    {
        Street = street;
        City = city;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
    }
}
```
