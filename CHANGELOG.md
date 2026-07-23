# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Multi-targeting support for `netstandard2.0` and `net10.0`, allowing backward compatibility with legacy .NET while preserving modern C# 13 features via PolySharp.
- Three Architectural Decision Records (ADRs) under `docs/decisions/`.

### Changed
- `Specification<T>`: Replaced `Expression.Invoke()` with a custom `ExpressionVisitor` (`ParameterRebinder`) for composite specifications (And/Or). This fixes compatibility with modern ORMs like Entity Framework Core while keeping the NativeAOT support intact.
- `Result.Success()`: Optimized non-generic success factory to return a cached static instance, making the happy path completely zero-alloc.
- Adapted `Math.Clamp` and `ArgumentNullException.ThrowIfNull` fallbacks for `netstandard2.0` environments.

## [1.0.0] — 2026-08-01

### Added
- `Entity<TId>` — Generic base entity with identity-based equality and domain events lifecycle
- `IDomainEvent` — Marker interface for domain events
- `ValueObject` — Structural equality via `GetEqualityComponents()`
- `Error` — Structured error record with code, description, and `ErrorType`
- `Result` / `Result<T>` — Discriminated union result type with `Map` and `Bind` support
- `ISpecification<T>` / `Specification<T>` — Specification pattern with And/Or/Not composition and operator overloads (`&`, `|`, `!`)
- `PaginationParameters` — Page/PageSize parameters with `Skip` computation and clamped `MaxPageSize`
- `PagedList<T>` — Paginated result with metadata and `Map` projection
- CI workflow (GitHub Actions) — build, test, and code coverage on PR/push
- Publish workflow (GitHub Actions) — NuGet publish triggered by version tags

[Unreleased]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/releases/tag/v1.0.0
