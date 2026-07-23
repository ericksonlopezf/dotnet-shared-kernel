# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-07-23

### Added
- Multi-targeting support for `netstandard2.0` and `net10.0`, allowing backward compatibility with legacy .NET while preserving modern C# 13 features via PolySharp.
- CI workflow (GitHub Actions) — build, test, and code coverage on PR/push.
- Publish workflow (GitHub Actions) — NuGet publish triggered by version tags.
- Additional Architectural Decision Records (ADRs) under `docs/decisions/`.

### Changed
- **Testing Architecture**: Renamed `EricksonLopez.SharedKernel.Tests` to `EricksonLopez.SharedKernel.UnitTests` to strictly enforce separation of test types. Included `Verify.Xunit` to enable snapshot testing and configured `coverlet` to cleanly exclude test assemblies from coverage reports (maintaining a pristine 100% metric for production code).
- `Specification<T>`: Replaced `Expression.Invoke()` with a custom `ExpressionVisitor` (`ParameterRebinder`) for composite specifications (And/Or). This fixes compatibility with modern ORMs like Entity Framework Core while keeping the NativeAOT support intact.
- `Result.Success()`: Optimized non-generic success factory to return a cached static instance, making the happy path completely zero-alloc.
- Adapted `Math.Clamp` and `ArgumentNullException.ThrowIfNull` fallbacks for `netstandard2.0` environments.

## [1.0.1] - 2026-07-21

### Changed
- Bumped `AwesomeAssertions` from 9.4.0 to 9.5.0 (Merge PR #9).

## [1.0.0] - 2026-07-16

### Added
- Initial project release with core DDD abstractions.
- `Entity<TId>` — Generic base entity with identity-based equality and domain events lifecycle.
- `AggregateRoot<TId>` — Consistency boundary with `RaiseDomainEvent` support.
- `IDomainEvent` — Marker interface for domain events.
- `ValueObject` — Structural equality via `GetEqualityComponents()`.
- `Error` — Structured error record with code, description, and `ErrorType`.
- `Result` and `Result<T>` — Discriminated union result type with `Map`, `Bind`, and fluent extensions.
- `ISpecification<T>` and `Specification<T>` — Specification pattern with And/Or/Not composition.
- `PaginationParameters` and `PagedList<T>` — Paginated result structures.

[Unreleased]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/releases/tag/v1.0.0
