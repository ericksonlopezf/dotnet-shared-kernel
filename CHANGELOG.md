# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0](https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.1.0...v2.0.0) (2026-08-12)


### ⚠ BREAKING CHANGES

* redesign core architecture and remove obsolete classes

### ✨ Features

* add AggregateRoot domain primitive, expand test coverage, and document allocation analysis ([9ca7053](https://github.com/ericksonlopezf/dotnet-shared-kernel/commit/9ca7053a9c7195db4ae6446943634e5bd16d16cf))
* redesign core architecture and remove obsolete classes ([e9a8142](https://github.com/ericksonlopezf/dotnet-shared-kernel/commit/e9a8142ecd2cbb2af7320455a8c0837b3d0397de))


### 📖 Documentation

* match codecov badge style with others ([9ca7053](https://github.com/ericksonlopezf/dotnet-shared-kernel/commit/9ca7053a9c7195db4ae6446943634e5bd16d16cf))
* match codecov badge style with others ([0f20d17](https://github.com/ericksonlopezf/dotnet-shared-kernel/commit/0f20d17e3b4c73d2d14663cc5a5bc2b937705b0f))

## [Unreleased]

## [2.0.0] — Unreleased

> [!IMPORTANT]
> This is a **breaking release**. All types added in v1.0.0 that were not domain primitives
> (`Result<T>`, `Error`, `ValueObject`, `Specification<T>`, `PaginationParameters`, `PagedList<T>`)
> have been permanently removed. Only `Entity<TId>`, `AggregateRoot<TId>`, and `IDomainEvent` remain.

### Removed
- `EricksonLopez.SharedKernel.Testing` project — fluent assertion helpers eliminated entirely.
- `Result<T>` / `Result` — Result pattern removed from this library (see ADR-014). Consumers requiring a Result type should adopt a dedicated library.
- Implicit dependency on any Result-related package — the library now has **zero** external runtime dependencies on all supported TFMs.

### Changed
- `AggregateRoot<TId>.DomainEvents` now returns the underlying `ReadOnlyCollection` directly via `AsReadOnly()` instead of copying to an `Array.Empty` wrapper, eliminating unnecessary indirection.
- `PackageTags` cleaned up — removed `result-pattern` tag that no longer applies (ADR-014 follow-up).

### Added
- 15 Architecture Decision Records (ADRs) under `docs/decisions/` documenting all key design decisions.
- `docs/Architecture.md` — full architecture guide with Mermaid diagrams.
- `docs/API_REFERENCE.md` — complete public API reference.
- `docs/PERFORMANCE_GUIDE.md` — lazy allocation analysis, NativeAOT startup benefits, and benchmark guide.
- `docs/MigrationGuide.md` — migration guide from v1.0.0, manual implementations, and `Ardalis.SharedKernel`.
- `docs/BestPractices.md`, `docs/AntiPatterns.md`, `docs/Cookbook.md`, `docs/FAQ.md`, `docs/GETTING_STARTED.md`, `docs/QUICK_START.md`, `docs/TROUBLESHOOTING.md`.
- `EricksonLopez.SharedKernel.ArchitectureTests` — architecture enforcement tests using `NetArchTest.Rules`.
- `EricksonLopez.SharedKernel.Benchmarks` — BenchmarkDotNet benchmarks for equality, hashing, and zero-alloc domain event access.
- `samples/EricksonLopez.SharedKernel.Sample/` — runnable reference sample project.
- `samples/EricksonLopez.SharedKernel.AotConsole/` — NativeAOT compatibility verification sample used as the AOT gate in CI.
- SonarCloud integration to CI (`dotnet-build-test.yml`).
- Stryker mutation testing JSON reporter.
- `stryker-config.json` with thresholds: break=95, low=98, high=100.

### Fixed
- Architectural audit corrections applied across 11 identified findings (commit `13a4854`).

---

## [1.1.0] — 2026-07-23

### Added
- `AggregateRoot<TId>` domain primitive — transactional consistency boundary with lazy-allocated domain event collection.

### Fixed
- CI: Base64 decoding of the Strong Name key (SNK_KEY secret) made robust against newlines and empty secrets.

---

## [1.0.1] — 2026-07-21

### Changed
- Bumped `AwesomeAssertions` from `9.4.0` to `9.5.0` (Dependabot PR #9).

---

## [1.0.0] — 2026-07-16

### Added
- Initial project release.
- `Entity<TId>` — generic abstract base entity with identity-based equality, `IsTransient()`, `GetHashCode()`, and `==`/`!=` operator overloads.
- `IDomainEvent` — marker interface for domain events.
- CI/CD workflows — GitHub Actions for build, test, coverage, and NuGet publishing triggered by version tags.

---

[Unreleased]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.1.0...HEAD
[2.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/ericksonlopezf/dotnet-shared-kernel/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/ericksonlopezf/dotnet-shared-kernel/releases/tag/v1.0.0
