# Project Roadmap

This roadmap provides a high-level overview of the planned features and ecosystem packages for the `EricksonLopez.SharedKernel`.

## 📌 Phase 1: The Core Foundation (Current - v1.x)

- ✅ **Result Pattern** — Functional error handling without exceptions.
- ✅ **Domain Primitives** — `Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`.
- ✅ **Query Abstractions** — `Specification<T>`, `PaginationParameters`, `PagedList<T>`.
- ✅ **Native AOT & Trimming Support** — Zero-reflection, fully trimmable.
- ✅ **CI/CD Pipeline** — Automated testing, coverage, and NuGet publishing.

## 🚀 Phase 2: Refinements (v1.2.x - v1.5.x)

- 📋 **Async Result Extensions** — Native `await` support on Result types to avoid `.Map()` boilerplate.
- 📋 **Result Validation Builder** — Fluent API for building compound validation errors.
- 📋 **Value Object Source Generators** — Optional package for generating zero-allocation `Equals` and `GetHashCode` for complex value objects.

## 🌌 The Ecosystem Expansion (v2.x)

The `SharedKernel` is designed to be the foundation of a broader ecosystem. The following packages are planned to be built on top of it:

| Package | Description | Status |
|---|---|---|
| **SharedKernel** | DDD abstractions + Result pattern | ✅ Published |
| **DomainPrimitives** | Value Objects with Source Generators | 📋 Planned |
| **SqlBuilder** | SQL-first query builder for Dapper | 📋 Planned |
| **Outbox** | Transactional Messaging (Outbox + Inbox) | 📋 Planned |

> **Note:** This roadmap is a living document and is subject to change based on community feedback and priorities. If you'd like to influence the roadmap, please join the conversation in [GitHub Discussions](https://github.com/ericksonlopezf/dotnet-shared-kernel/discussions).
