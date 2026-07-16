# EricksonLopez.SharedKernel

[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopezf/dotnet-shared-kernel/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)

A shared kernel for DDD-based .NET applications. Provides battle-tested abstractions for Clean Architecture projects: **Entity**, **ValueObject**, **Result pattern**, **Domain Events**, **Specification pattern**, and **Pagination**.

---

## Why this exists

Every enterprise .NET project I build needs the same foundational types. Instead of copying them across repositories, this package is the single source of truth.

Design principles:
- **Zero external dependencies** in the main library
- **Immutable by default** — ValueObject, PagedList, Error are all sealed/records
- **Composable** — Result supports Map/Bind; Specification supports And/Or/Not with `&`, `|`, `!` operators
- **No magic** — every abstraction is readable and debuggable without a framework

---

## Installation

```bash
dotnet add package EricksonLopez.SharedKernel
```

---

## Quick Start

### Result Pattern

```csharp
// Define errors in a static class per domain concept
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    public static readonly Error NameEmpty =
        Error.Validation("User.NameEmpty", "Name cannot be empty.");
}

// Return Result instead of throwing
public Result<User> GetUser(Guid id)
{
    var user = _repository.Find(id);
    return user is null ? UserErrors.NotFound(id) : user;
}

// Caller handles explicitly
var result = GetUser(id);
if (result.IsFailure)
    return result.Error; // propagate up

// Monadic chaining
var result = GetUser(id)
    .Map(u => new UserDto(u.Name, u.Email))
    .Bind(dto => Validate(dto));
```

### ValueObject

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

var usd100 = new Money(100m, "USD");
var usd100b = new Money(100m, "USD");
Console.WriteLine(usd100 == usd100b); // true — structural equality
```

### Entity with Domain Events

```csharp
public sealed class Order : Entity<Guid>
{
    private Order() { }

    public string Description { get; private set; } = string.Empty;

    public static Order Create(string description)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Description = description
        };
        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id));
        return order;
    }
}

// Domain event
public sealed record OrderCreatedEvent(Guid OrderId) : IDomainEvent;

// In your Unit of Work / repository — after SaveChanges:
foreach (var entity in entities)
{
    var events = entity.DomainEvents.ToList();
    entity.ClearDomainEvents();
    foreach (var domainEvent in events)
        await _publisher.Publish(domainEvent, cancellationToken);
}
```

### Specification Pattern

```csharp
public sealed class ActiveUserSpec : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.IsActive;
}

public sealed class PremiumUserSpec : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.Tier == UserTier.Premium;
}

// Compose
var spec = new ActiveUserSpec() & new PremiumUserSpec();

// In-memory evaluation
var eligibleUsers = users.Where(spec.IsSatisfiedBy);

// As LINQ expression (for Dapper/EF)
var expression = spec.ToExpression(); // Expression<Func<User, bool>>
```

### Pagination

```csharp
// In your query handler
public async Task<PagedList<ProductDto>> Handle(GetProductsQuery query)
{
    var parameters = PaginationParameters.Of(query.Page, query.PageSize);

    var items = await _connection.QueryAsync<ProductDto>(
        "SELECT * FROM products LIMIT @limit OFFSET @offset",
        new { limit = parameters.PageSize, offset = parameters.Skip });

    var total = await _connection.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM products");

    return PagedList<ProductDto>.Create(items, total, parameters);
}

// PagedList<T> metadata
page.TotalCount   // Total items across all pages
page.TotalPages   // Ceiling(TotalCount / PageSize)
page.HasNextPage  // true if not on last page
page.Map(dto => new ProductResponse(dto.Id, dto.Name)) // project without losing metadata
```

---

## API Reference

### `Entity<TId>`

| Member | Description |
|---|---|
| `TId Id` | Entity identifier |
| `IReadOnlyList<IDomainEvent> DomainEvents` | Pending events since last dispatch |
| `RaiseDomainEvent(IDomainEvent)` | Queue a domain event |
| `ClearDomainEvents()` | Clear after dispatch (call from infra layer) |

### `ValueObject`

| Member | Description |
|---|---|
| `GetEqualityComponents()` | Override to yield equality fields |
| `==` / `!=` / `Equals` | Structural equality |

### `Result` / `Result<T>`

| Member | Description |
|---|---|
| `IsSuccess` / `IsFailure` | State inspection |
| `Value` | The success value. Throws on failure. |
| `Error` | The failure error. `Error.None` on success. |
| `Map<TNext>(Func<T, TNext>)` | Transform value on success, propagate error |
| `Bind<TNext>(Func<T, Result<TNext>>)` | Chain operations returning Result |

### `Error`

| Factory | ErrorType | HTTP equivalent |
|---|---|---|
| `Error.Failure(code, desc)` | `Failure` | 500 |
| `Error.Validation(code, desc)` | `Validation` | 400 |
| `Error.NotFound(code, desc)` | `NotFound` | 404 |
| `Error.Conflict(code, desc)` | `Conflict` | 409 |
| `Error.Unauthorized(code, desc)` | `Unauthorized` | 401 |
| `Error.Forbidden(code, desc)` | `Forbidden` | 403 |

### `Specification<T>`

| Member | Description |
|---|---|
| `ToExpression()` | Expression tree for LINQ-to-SQL |
| `IsSatisfiedBy(T)` | In-memory evaluation (cached compile) |
| `And(spec)` / `&` | Logical AND composition |
| `Or(spec)` / `\|` | Logical OR composition |
| `Not()` / `!` | Logical NOT |

### `PagedList<T>`

| Member | Description |
|---|---|
| `Items` | Items on the current page |
| `TotalCount` | Total items in the data source |
| `TotalPages` | Ceiling(TotalCount / PageSize) |
| `HasPreviousPage` / `HasNextPage` | Navigation flags |
| `Map<TResult>(Func<T, TResult>)` | Project items, preserve metadata |
| `PagedList<T>.Create(items, total, params)` | Factory |
| `PagedList<T>.Empty(params)` | Empty page factory |

---

## Architecture Decisions

Design rationale is documented as ADRs in [`docs/decisions/`](docs/decisions/):

- [ADR-001: Result Pattern — Explicit Failure over Exceptions](docs/decisions/ADR-001-result-pattern.md)

---

## License

MIT © [Erickson López](https://github.com/ericksonlopezf)
