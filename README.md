# EricksonLopez.SharedKernel

[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.SharedKernel?style=for-the-badge&logo=nuget&logoColor=white&color=004880)](https://www.nuget.org/packages/EricksonLopez.SharedKernel)
[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopezf/dotnet-shared-kernel/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions)
[![Coverage](https://img.shields.io/codecov/c/github/ericksonlopezf/dotnet-shared-kernel?style=for-the-badge&logo=codecov&logoColor=white)](https://codecov.io/gh/ericksonlopezf/dotnet-shared-kernel)
[![Mutation Score](https://img.shields.io/badge/Mutation_Score-%E2%89%A5100%25-brightgreen?style=for-the-badge&logo=stryker&logoColor=white)](https://github.com/ericksonlopezf/dotnet-shared-kernel/actions/workflows/mutation-testing.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_8_%7C_9_%7C_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-brightgreen?style=for-the-badge)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)

A minimal, high-performance Shared Kernel for DDD-based .NET applications. Provides zero-dependency, AOT-compatible domain primitives: `Entity<TId>`, `AggregateRoot<TId>`, and `IDomainEvent`. Designed for Clean Architecture and CQRS.

## ⚡ Performance

> BenchmarkDotNet v0.14.0 · .NET 10.0 · X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
>
> Focus: Extreme low allocation for domain event sourcing.

### Event Sourcing — Lazy Allocation

| Method | Mean | Allocated | Alloc Ratio |
|---|---:|---:|---:|
| `RaiseEvent_FirstTime` | **12.4 ns** | **64 B** | **1.0×** |
| `RaiseEvent_Subsequent` | **5.2 ns** | **0 B** | **0.0×** |
| `ClearEvents` | **1.1 ns** | **0 B** | **0.0×** |

**Key takeaways:**
- **Zero-allocation** domain events collection until the first event is raised.
- Minimal `64 B` allocation strictly for the backing list on the first event.
- **O(1)** clearing of events with absolutely zero allocations.

→ [Performance Guide](docs/PERFORMANCE_GUIDE.md)

## Key Features

*   **Zero Dependencies:** Keep your core domain pure. No third-party dependencies.
*   **AOT Ready:** `IsAotCompatible=true` and `IsTrimmable=true` are active on all supported TFMs, providing real Native AOT compatibility analysis at compile time.
*   **Lazy Allocation:** Domain event collections allocate memory strictly when the first event is raised, optimizing memory footprint.
*   **Identity Equality:** Pre-optimized `GetHashCode()` and equality operators (`==`, `!=`) out of the box for Domain Entities.

## What's Included

| Type | Description |
|---|---|
| `Entity<TId>` | Abstract base for domain entities with identity-based equality (type + Id). Includes `IsTransient()`, `==`/`!=` operators, and an optimized `GetHashCode()`. |
| `AggregateRoot<TId>` | Transactional consistency boundary. Inherits `Entity<TId>` and adds domain event support with lazy allocation. |
| `IDomainEvent` | Marker interface for domain events. |

> [!WARNING]
> This library intentionally does **not** include infrastructure abstractions (UnitOfWork, Repositories) or additional patterns (Result, ValueObject, Specification). The goal is to keep the domain core completely pure and dependency-free.

## .NET Framework Support Policy

All library packages target `net8.0`, `net9.0`, and `net10.0`.

> **Support Policy**: This library supports only .NET frameworks with **active official support from Microsoft**. A framework version is included in `TargetFrameworks` as long as it appears on the [Microsoft .NET Support Policy page](https://dotnet.microsoft.com/platform/support/policy/dotnet-core) under **Active** or **Maintenance** status. Framework versions are removed from `TargetFrameworks` when they reach their official end-of-life date as defined by Microsoft — not before, and not after.
>
> | Framework | Type | Microsoft Support End Date | Status |
> |---|---|---|---|
> | .NET 8 | LTS | November 10, 2026 | ✅ Supported |
> | .NET 9 | STS | **November 10, 2026** | ✅ Supported |
> | .NET 10 | LTS | November 2028 | ✅ Supported |

## Quick Start

1. Install the package:
    ```bash
    dotnet add package EricksonLopez.SharedKernel
    ```

2. Start using the domain primitives:
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

## Showcase

The repository includes a runnable reference project at `samples/EricksonLopez.SharedKernel.Sample/`. It demonstrates all public API members across multiple use-case levels.

```bash
dotnet run --project samples/EricksonLopez.SharedKernel.Sample/EricksonLopez.SharedKernel.Sample.csproj
```

## Documentation

| Topic | Link |
|-------|------|
| Quick Start | [docs/QUICK_START.md](docs/QUICK_START.md) |
| Getting Started | [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md) |
| API Reference | [docs/API_REFERENCE.md](docs/API_REFERENCE.md) |
| Cookbook | [docs/Cookbook.md](docs/Cookbook.md) |
| Best Practices | [docs/BestPractices.md](docs/BestPractices.md) |
| Anti-Patterns | [docs/AntiPatterns.md](docs/AntiPatterns.md) |
| Architecture Guide | [docs/Architecture.md](docs/Architecture.md) |
| Performance Guide | [docs/PERFORMANCE_GUIDE.md](docs/PERFORMANCE_GUIDE.md) |
| Migration Guide | [docs/MigrationGuide.md](docs/MigrationGuide.md) |
| CI/CD Pipeline | [docs/ci-cd.md](docs/ci-cd.md) |
| FAQ | [docs/FAQ.md](docs/FAQ.md) |
| Troubleshooting | [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) |
| Design Decisions (ADRs) | [docs/decisions/](docs/decisions/) |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for information on building the project, running tests, and contributing guidelines.

## Security

Please review [SECURITY.md](SECURITY.md) for details on our security policies and how to report vulnerabilities.

## Code of Conduct

We follow the Contributor Covenant. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for details.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
