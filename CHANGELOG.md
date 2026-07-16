# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
