# EricksonLopez.SharedKernel

A minimal Shared Kernel for DDD-based .NET applications. Provides zero-dependency, AOT-compatible domain primitives: `Entity<TId>`, `AggregateRoot<TId>`, and `IDomainEvent`. Designed for Clean Architecture and CQRS.

[![CI](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions/workflows/ci.yml/badge.svg)](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.SharedKernel)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![codecov](https://codecov.io/gh/ericksonlopezf/dotnet-shared-kernel/graph/badge.svg)](https://codecov.io/gh/ericksonlopezf/dotnet-shared-kernel)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## What's Included

The library exposes **three types** in the `EricksonLopez.SharedKernel` namespace:

| Type | Description |
|---|---|
| `Entity<TId>` | Abstract base for domain entities with identity-based equality (type + Id). Includes `IsTransient()`, `==`/`!=` operators, and an optimized `GetHashCode()`. |
| `AggregateRoot<TId>` | Transactional consistency boundary. Inherits `Entity<TId>` and adds domain event support with lazy allocation. |
| `IDomainEvent` | Marker interface for domain events. |

> [!WARNING]
> This library intentionally does **not** include infrastructure abstractions (UnitOfWork, Repositories) or additional patterns (Result, ValueObject, Specification). The goal is to keep the domain core completely pure and dependency-free.

> [!NOTE]
> **AOT Compatibility:** `IsAotCompatible=true` and `IsTrimmable=true` are active on **all** supported TFMs (`net8.0`, `net9.0`, `net10.0`), providing real AOT compatibility analysis at compile time for every target platform.

---

## Supported Frameworks

| Framework | Supported | NativeAOT | Trimming |
|---|---|---|---|
| net8.0 | ✅ | ✅ | ✅ |
| net9.0 | ✅ | ✅ | ✅ |
| net10.0 | ✅ | ✅ | ✅ |

---

## Quick Start

```bash
dotnet add package EricksonLopez.SharedKernel
```

```csharp
using EricksonLopez.SharedKernel;

// 1. Define a domain event
public sealed record OrderPlacedEvent(Guid OrderId) : IDomainEvent;

// 2. Define an Aggregate Root
public sealed class Order : AggregateRoot<Guid>
{
    private Order() { }

    public static Order Place(Guid id)
    {
        var order = new Order { Id = id };
        order.RaiseDomainEvent(new OrderPlacedEvent(id));
        return order;
    }
}

// 3. Use it
var order = Order.Place(Guid.NewGuid());
Console.WriteLine(order.DomainEvents.Count); // 1
order.ClearDomainEvents();
```

---

## Documentation

| Document | Description |
|---|---|
| [Quick Start](docs/QUICK_START.md) | Installation and first use |
| [Getting Started](docs/GETTING_STARTED.md) | Full step-by-step guide |
| [API Reference](docs/API_REFERENCE.md) | Technical reference for all public members |
| [Cookbook](docs/Cookbook.md) | Recipes by scenario |
| [Best Practices](docs/BestPractices.md) | Recommended DDD patterns |
| [Anti-Patterns](docs/AntiPatterns.md) | Common mistakes to avoid |
| [Architecture Guide](docs/Architecture.md) | Diagrams and main flows |
| [Performance Guide](docs/PERFORMANCE_GUIDE.md) | Lazy allocation, AOT, benchmarks |
| [Migration Guide](docs/MigrationGuide.md) | Migrating from v1.0.0 or other libraries |
| [CI/CD](docs/ci-cd.md) | Build, test, and publish pipeline |
| [FAQ](docs/FAQ.md) | Frequently asked questions |
| [Troubleshooting](docs/TROUBLESHOOTING.md) | Problem resolution guide |
| [ADRs](docs/decisions/) | Architecture Decision Records |

---

## Showcase

The repository includes a runnable reference project at:

```
samples/EricksonLopez.SharedKernel.Sample/
```

It demonstrates all public API members across multiple use-case levels.

```bash
dotnet run --project samples/EricksonLopez.SharedKernel.Sample/EricksonLopez.SharedKernel.Sample.csproj
```

---

## Contributing

Contributions are welcome! Please read:

- [CONTRIBUTING.md](CONTRIBUTING.md) — development setup, branching strategy, and PR guidelines
- [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) — community standards
- [SECURITY.md](SECURITY.md) — how to report vulnerabilities

---

## License

MIT License — © 2026 Erickson López. See [LICENSE](LICENSE) for details.
