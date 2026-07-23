# EricksonLopez.SharedKernel

[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=004880)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopezf/dotnet-shared-kernel/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions)
[![Coverage](https://codecov.io/gh/ericksonlopezf/dotnet-shared-kernel/graph/badge.svg)](https://codecov.io/gh/ericksonlopezf/dotnet-shared-kernel)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_10_%7C_Standard_2.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-brightgreen?style=for-the-badge)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)

A shared kernel for DDD-based .NET applications. Provides battle-tested abstractions for Clean Architecture projects: **Entity**, **AggregateRoot**, **ValueObject**, **Result pattern**, **Domain Events**, **Specification pattern**, and **Pagination**.

**Key Features:**
- ⚡ **Zero external dependencies**
- 🔒 **Immutable by default** — ValueObject, Error, PagedList are sealed/records
- 🚀 **Zero-alloc happy path** — `Result.Success()` is cached
- 🔗 **Fluent pipelines** — Result supports `Map`, `Bind`, `Match`, `Tap`, `Ensure`, `Recover`, `Try`, `Combine`
- 🚀 **NativeAOT + Trimming compatible** — `IsAotCompatible` and `IsTrimmable` enabled
- ⚙️ **Async-first** — Full `Task<Result<T>>` and `ValueTask<Result<T>>` extension methods
- 🧩 **No magic** — every abstraction is readable and debuggable

---

## Table of Contents
- [Installation](#installation)
- [Quick Start](#quick-start)
  - [Result Pattern](#result-pattern)
  - [Error Types](#error-types)
  - [AggregateRoot & Entity](#aggregateroot--entity)
  - [ValueObject](#valueobject)
  - [Specification Pattern](#specification-pattern)
  - [Pagination](#pagination)
- [API Reference](#api-reference)
- [NativeAOT Compatibility](#nativeaot-compatibility)
- [Part of the EricksonLopez Ecosystem](#part-of-the-ericksonlopez-ecosystem)
- [Architecture Decisions & Guides](#architecture-decisions--guides)
- [FAQ & Troubleshooting](#faq--troubleshooting)
- [License](#license)

---

## Installation

```bash
dotnet add package EricksonLopez.SharedKernel
```

> Requires **.NET 10** or **.NET Standard 2.0** compatible frameworks (e.g., .NET Framework 4.6.1+, .NET Core 2.0+).

---

## Quick Start

### Result Pattern

```csharp
// Define errors as a static class per domain concept
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    public static readonly Error NameEmpty =
        Error.Validation("User.NameEmpty", "Name cannot be empty.");

    public static readonly Error Inactive =
        Error.Forbidden("User.Inactive", "User is not active.");
}

// Return Result instead of throwing
public Result<User> GetUser(Guid id)
{
    var user = _repository.Find(id);
    return user is null ? UserErrors.NotFound(id) : user;
}
```

**Fluent pipeline:**

```csharp
var result = GetUser(id)
    .Ensure(u => u.IsActive, UserErrors.Inactive)
    .Map(u => new UserDto(u.Name, u.Email))
    .Tap(dto => _cache.Set(id, dto))
    .TapError(e => _logger.LogWarning("Failed: {Error}", e));
```

**Pattern matching with Match:**

```csharp
return result.Match(
    user => Ok(user),
    error => Problem(error.Description));
```

**Try-pattern (idiomatic .NET):**

```csharp
if (result.TryGetValue(out var user))
    Console.WriteLine(user.Name);

var name = GetUser(id)
    .Map(u => u.Name)
    .GetValueOrDefault("Unknown");
```

**Destructuring:**

```csharp
var (ok, user, error) = GetUser(id);
if (ok) Console.WriteLine(user.Name);
```

**Exception bridge:**

```csharp
var result = Result.Try(
    () => JsonSerializer.Deserialize<Config>(json),
    ex => Error.Unexpected("Config.ParseFailed", ex.Message));
```

**Async pipelines (with ConfigureAwait(false)):**

```csharp
var result = await _repository.GetById(id)   // Task<Result<User>>
    .Ensure(u => u.IsActive, UserErrors.Inactive)
    .Map(u => u.ToDto())
    .Tap(dto => _cache.SetAsync(id, dto))
    .Recover(e => _fallbackRepo.GetById(id));
```

### Error Types

```csharp
Error.Failure(code, description)       // Generic domain error
Error.Validation(code, description)    // Input validation
Error.NotFound(code, description)      // Resource not found
Error.Conflict(code, description)      // State conflict
Error.Unauthorized(code, description)  // Authentication required
Error.Forbidden(code, description)     // Insufficient permissions
Error.Unavailable(code, description)   // Service unavailable
Error.Unexpected(code, description)    // System error / exceptions
```

**Compound errors (e.g., multiple validation failures):**

```csharp
var error = Error.Validation("User.Invalid", "Validation failed",
    Error.Validation("User.Name.Required", "Name is required"),
    Error.Validation("User.Email.Invalid", "Invalid email format"));

error.HasInnerErrors   // true
error.InnerErrors      // [Name.Required, Email.Invalid]
```

**Combining multiple results:**

```csharp
var result = Result.Combine(
    ValidateName(name),
    ValidateEmail(email),
    ValidateAge(age));
// Returns success if all pass, or compound error with all failures

// Typed combine into tuples:
var (user, account) = Result.Combine(GetUser(id), GetAccount(id)).Value;
```

### AggregateRoot & Entity

```csharp
// AggregateRoot — the only entry point for Domain Events
public sealed class Order : AggregateRoot<Guid>
{
    public string Description { get; private set; }

    private Order(Guid id, string description)
    {
        Id = id;
        Description = description;
    }

    public static Order Create(Guid id, string description)
    {
        var order = new Order(id, description);
        order.RaiseDomainEvent(new OrderCreated(id));
        return order;
    }
}

// Entity — identity-only, no domain events
public sealed class LineItem : Entity<Guid>
{
    public string ProductName { get; private set; } = string.Empty;
}

// Domain event
public sealed record OrderCreated(Guid OrderId) : IDomainEvent;

// In your Unit of Work — after SaveChanges:
foreach (var aggregate in aggregates)
{
    var events = aggregate.DomainEvents.ToList();
    aggregate.ClearDomainEvents();
    foreach (var domainEvent in events)
        await _publisher.Publish(domainEvent);
}
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

    // Optional: override for zero-boxing equality on hot paths
    // public override bool Equals(ValueObject? other)
    //     => other is Money m && Amount == m.Amount && Currency == m.Currency;
    // public override int GetHashCode()
    //     => HashCode.Combine(Amount, Currency);
}
```

### Specification Pattern

```csharp
public sealed class ActiveUserSpec : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.IsActive;

    // Optional: NativeAOT-safe override
    protected override bool Evaluate(User candidate)
        => candidate.IsActive;
}

// Compose with operators
var spec = new ActiveUserSpec() & new PremiumUserSpec();

// In-memory evaluation
var eligible = users.Where(spec.IsSatisfiedBy);

// LINQ-to-SQL (EF Core / Dapper)
var expression = spec.ToExpression();
```

### Pagination

```csharp
var parameters = PaginationParameters.Of(page: 2, pageSize: 10);

var items = await _connection.QueryAsync<ProductDto>(sql,
    new { limit = parameters.PageSize, offset = parameters.Skip });
var total = await _connection.ExecuteScalarAsync<int>(countSql);

var page = PagedList<ProductDto>.Create(items, total, parameters);

page.TotalCount    // Total items across all pages
page.TotalPages    // Ceiling(TotalCount / PageSize)
page.HasNextPage   // Navigation flag
page.Map(dto => new ProductResponse(dto.Id, dto.Name))  // Project preserving metadata
```

---

## API Reference

### Domain

| Type | Members | Description |
|---|---|---|
| `Entity<TId>` | `Id`, `==`/`!=` | Identity-based equality |
| `AggregateRoot<TId>` | `RaiseDomainEvent()`, `DomainEvents`, `ClearDomainEvents()` | Consistency boundary + event publishing |
| `ValueObject` | `GetEqualityComponents()`, virtual `Equals` | Structural equality |
| `IDomainEvent` | marker interface | Domain event contract |

### Result

| Member | Result | Result\<T\> | Description |
|---|---|---|---|
| `IsSuccess` / `IsFailure` | ✅ | ✅ | State inspection |
| `Error` | ✅ | ✅ | The failure error (`Error.None` on success) |
| `Value` | — | ✅ | Success value (throws on failure) |
| `Map<TNext>(Func)` | — | ✅ | Transform value |
| `Bind<TNext>(Func)` | — | ✅ | Chain Result-returning operations |
| `Match<TOut>(onSuccess, onFailure)` | ✅ | ✅ | Exhaustive handling |
| `Tap(Action)` | ✅ | ✅ | Side effect on success |
| `TapError(Action)` | ✅ | ✅ | Side effect on failure |
| `Ensure(predicate, error)` | ✅ | ✅ | Post-condition validation |
| `Recover(Func)` | — | ✅ | Fallback on failure |
| `Finally(Action)` | ✅ | ✅ | Always execute |
| `MapError(Func)` | ✅ | ✅ | Transform the error |
| `TryGetValue(out T)` | — | ✅ | Try-pattern |
| `TryGetError(out Error)` | ✅ | ✅ | Try-pattern |
| `GetValueOrDefault(T)` | — | ✅ | Safe access |
| `GetValueOrDefault(Func)` | — | ✅ | Safe access with fallback logic |
| `ToResult()` | — | ✅ | Drop value (Result\<T\> → Result) |
| `Deconstruct` | — | ✅ | `var (ok, value, error) = result;` |
| `Try(Action, errorHandler)` | ✅ | ✅ | Exception → Error bridge |
| `Combine(params Result[])` | ✅ | ✅ | Aggregate results |

### Error

| Factory | ErrorType | Semantic |
|---|---|---|
| `Error.Failure(code, desc)` | `Failure` | Generic domain error |
| `Error.Validation(code, desc)` | `Validation` | Input validation |
| `Error.NotFound(code, desc)` | `NotFound` | Resource not found |
| `Error.Conflict(code, desc)` | `Conflict` | State conflict |
| `Error.Unauthorized(code, desc)` | `Unauthorized` | Authentication required |
| `Error.Forbidden(code, desc)` | `Forbidden` | Insufficient permissions |
| `Error.Unavailable(code, desc)` | `Unavailable` | Service unavailable |
| `Error.Unexpected(code, desc)` | `Unexpected` | System error |

All factories have an overload with `params Error[] innerErrors` for compound errors.

### Specification

| Member | Description |
|---|---|
| `ToExpression()` | Expression tree for LINQ-to-SQL |
| `IsSatisfiedBy(T)` | In-memory evaluation via `Evaluate()` |
| `Evaluate(T)` | `protected virtual` — override for NativeAOT |
| `And(spec)` / `&` | Logical AND |
| `Or(spec)` / `\|` | Logical OR |
| `Not()` / `!` | Logical NOT |

### Pagination

| Member | Description |
|---|---|
| `PagedList<T>.Create(items, total, params)` | Factory |
| `PagedList<T>.Empty(params)` | Empty page |
| `Items`, `TotalCount`, `TotalPages` | Page data |
| `HasPreviousPage` / `HasNextPage` | Navigation |
| `Map<TResult>(Func)` | Project preserving metadata |

---

## NativeAOT Compatibility

This library is fully NativeAOT and trimming compatible:

```xml
<IsTrimmable>true</IsTrimmable>
<IsAotCompatible>true</IsAotCompatible>
```

**Specification in NativeAOT:** The default `Evaluate()` method uses `Expression.Compile()` (requires JIT). For NativeAOT, override `Evaluate()` in your leaf specifications:

```csharp
public sealed class ActiveSpec : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
        => p => p.IsActive;

    // NativeAOT-safe: no Expression.Compile()
    protected override bool Evaluate(Product candidate)
        => candidate.IsActive;
}
```

Composite specifications (And, Or, Not) are **automatically NativeAOT-safe** — they delegate to children's `IsSatisfiedBy()` without compiling.

---

## Performance Benchmarks

The `SharedKernel` is strictly optimized for low latency and minimal allocations. We use `BenchmarkDotNet` to ensure the happy path is completely allocation-free (`0 bytes`).

| Operation | Allocation | Note |
|---|---|---|
| `Result.Success()` | **0 B** | Cached static instance |
| `Result.Failure()` | ~24 B | Exceptional path |
| `ValueObject.Equals` (hot path) | **0 B** | When `Equals` is manually overridden (see ADR) |
| `Specification.IsSatisfiedBy` | **0 B** | Cached compiled expression lock |

For details, see [ADR-006: Performance Analysis](docs/decisions/ADR-006-performance-analysis.md).

---

## Samples

Check the [samples/](samples) directory for working examples:

- **EricksonLopez.SharedKernel.Sample**: Standard Web API project demonstrating the Result pattern and Specification pattern.
- **EricksonLopez.SharedKernel.AotConsole**: A Native AOT console app demonstrating trimming and zero-reflection value objects.

---

## Part of the EricksonLopez Ecosystem

SharedKernel is the foundational layer of a modular .NET ecosystem:

| Package | Description | Depends on SharedKernel | Status |
|---|---|---|---|
| **SharedKernel** | DDD abstractions + Result pattern | — (this library) | ✅ Published |
| **DomainPrimitives** | Value Objects with Source Generators | ✅ | 📋 Planned |
| **SqlBuilder** | SQL-first query builder for Dapper | ✅ | 📋 Planned |
| **Outbox** | Transactional Messaging (Outbox + Inbox) | ✅ | 📋 Planned |

---

## Architecture Decisions & Guides

Design rationale and guides are documented in the `docs` folder:

- 📖 **[Cookbook: Patterns and Examples](docs/Cookbook.md)**
- 📖 **[Best Practices and Anti-Patterns](docs/BestPractices.md)**

**Architectural Decision Records (ADRs):**
- [ADR-001: Result Pattern](docs/decisions/ADR-001-result-pattern.md)
- [ADR-002: Zero Functional Dependencies](docs/decisions/ADR-002-zero-functional-dependencies.md)
- [ADR-003: Value Object Boxing Acceptance](docs/decisions/ADR-003-value-object-boxing.md)
- [ADR-004: Validation Error Design](docs/decisions/ADR-004-validation-error-design.md)
- [ADR-005: Result Pattern Allocations Optimization](docs/decisions/ADR-005-result-pattern-allocations.md)
- [ADR-006: Performance Analysis](docs/decisions/ADR-006-performance-analysis.md)
- [ADR-007: Native AOT Compatibility](docs/decisions/ADR-007-native-aot-compatibility.md)

---

## License

MIT © [Erickson López](https://github.com/ericksonlopezf)

---

## FAQ & Troubleshooting

**Q: Why is `Result` a class and not a `struct`?**
A: `Result` is a class to provide inheritance (`Result<TValue> : Result`) and allow null-checks when used as a reference type. The allocations are minimal because `Result.Success()` uses a cached singleton and `Error` uses `IReadOnlyList<Error>?` which is null in the happy path.

**Q: How do I handle multiple validation errors?**
A: You can pass multiple inner errors to `Error.Validation` using the `params Error[] innerErrors` overload. See [Error Types](#error-types) for examples.

**Q: I modified a `ToString()` method and my PR build failed with `VerifyException`. What should I do?**
A: We use `Verify.Xunit` for snapshot testing. If you intentionally changed the output format, you must review the generated `.received.txt` file and rename it to `.verified.txt` to accept the new snapshot. See our [CONTRIBUTING.md](CONTRIBUTING.md) for detailed instructions.
