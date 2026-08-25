# Contributing to EricksonLopez.SharedKernel

Thank you for your interest in contributing! This document provides guidelines for contributing to this project.

## Getting Started

1. **Fork** the repository
2. **Clone** your fork locally
3. Create a **branch**: `git checkout -b feature/your-feature develop`
4. Make your changes
5. **Run tests** to ensure nothing is broken
6. **Commit** with a conventional commit message
7. **Push** to your fork and create a **Pull Request** against `develop`

## Prerequisites

Before contributing, ensure you have the following installed:
- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** — version `10.0.302` or compatible (see `global.json`)
- An IDE such as Visual Studio, JetBrains Rider, or VS Code with the C# Dev Kit

## Development Setup

```bash
# Clone
git clone https://github.com/<your-username>/dotnet-shared-kernel.git
cd dotnet-shared-kernel

# Restore dependencies
dotnet restore

# Build (Release configuration)
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run with code coverage
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

> [!NOTE]
> There are no custom build scripts (`build.ps1` / `build.sh`). Use the standard `dotnet` CLI commands above.

## Code Standards

This project enforces strict code quality. Your PR must pass all of these:

- **TreatWarningsAsErrors** — zero warnings allowed
- **Nullable enabled** — all reference types must be annotated
- **WarningLevel 5** — maximum warning sensitivity
- **EnforceCodeStyleInBuild** — code style rules are build errors
- **EditorConfig** — formatting rules in `.editorconfig`

### Branching Strategy

CI runs on `main` and `develop` branches. Use the following prefixes:
- `feature/*` — For new abstractions or features.
- `fix/*` — For bug fixes.
- `docs/*` — For documentation updates.
- `chore/*` — For tooling, CI/CD, or dependency updates.

Pull Requests should target the `develop` branch.

### Commit Conventions

We strictly follow [Conventional Commits](https://www.conventionalcommits.org/). Your commit messages should be structured as follows:

```
<type>[optional scope]: <description>
```

**Allowed types:** `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.

## Test Structure

This repository separates testing concerns into distinct projects:

1. **`EricksonLopez.SharedKernel.UnitTests`** — Standard unit tests for all domain primitives (`Entity<TId>`, `AggregateRoot<TId>`, `DomainEvent`, `IStrongId`, `ValueObject`).
2. **`EricksonLopez.SharedKernel.ArchitectureTests`** — Architecture enforcement tests using `NetArchTest.Rules` to verify boundary constraints (zero external dependencies, zero sibling leaks, immutable identities).
3. **`EricksonLopez.SharedKernel.IntegrationTests`** — Native AOT compilation and execution verification.
4. **`EricksonLopez.SharedKernel.Benchmarks`** — Performance benchmarks using `BenchmarkDotNet` to prove zero-allocation claims. Run in `Release` mode before submitting performance-related PRs.

```bash
# Run unit tests only
dotnet test tests/EricksonLopez.SharedKernel.UnitTests/EricksonLopez.SharedKernel.UnitTests.csproj --configuration Release

# Run architecture tests only
dotnet test tests/EricksonLopez.SharedKernel.ArchitectureTests/EricksonLopez.SharedKernel.ArchitectureTests.csproj --configuration Release

# Run integration tests only
dotnet test tests/EricksonLopez.SharedKernel.IntegrationTests/EricksonLopez.SharedKernel.IntegrationTests.csproj --configuration Release

# Run benchmarks
dotnet run --project benchmarks/EricksonLopez.SharedKernel.Benchmarks/EricksonLopez.SharedKernel.Benchmarks.csproj --configuration Release
```

## Mutation Testing (Stryker)

The CI pipeline runs Stryker mutation testing with the following thresholds:

| Threshold | Value |
|---|---|
| Break (CI failure) | 95% |
| Low (warning) | 98% |
| High (target) | 100% |

To run Stryker locally:

```bash
dotnet tool restore
dotnet stryker
```

## NativeAOT Verification

Before merging any PR that touches the `src/` directory, CI verifies that the library compiles without AOT warnings:

```bash
dotnet publish tests/EricksonLopez.SharedKernel.NativeAotTests/EricksonLopez.SharedKernel.NativeAotTests.csproj \
  -c Release -r linux-x64 -p:PublishAot=true
```

Any `IL3050` or `IL2026` warnings will fail the build. Ensure your changes do not introduce reflection or dynamic code.

## Pull Request Guidelines

- **One concern per PR** — keep changes focused
- **Include tests** — all new code must have unit tests
- **Update CHANGELOG.md** — add your changes under `[Unreleased]`
- **Follow SemVer** — breaking changes require a major version bump discussion
- **No `bin/` or `obj/`** — ensure build artifacts are not committed

## What to Contribute

- 🐛 Bug fixes with a failing test that proves the fix
- 📖 Documentation improvements
- ⚡ Performance improvements (with benchmark evidence)
- ✨ New abstractions (open an issue first to discuss the design)

## What NOT to Contribute

- ❌ External runtime dependencies — this package has zero external dependencies by design ([ADR-002](docs/decisions/ADR-002-zero-functional-dependencies.md), [ADR-014](docs/decisions/ADR-014-removal-of-result-dependency.md))
- ❌ Custom `DomainException` or classes inheriting from `System.Exception` for domain logic control flow — PRs introducing business exceptions will be rejected per [ADR-028](docs/decisions/ADR-028-result-first-architecture-over-domain-exceptions.md). Domain and application validation errors must be represented via `Result<T>` and `Error` records in consumer tiers.
- ❌ Generic repository markers (`IRepository<TEntity, TId>`) or Unit of Work abstractions in Tier 0 — rejected per [ADR-018](docs/decisions/ADR-018-rejection-of-generic-repository.md) and [ADR-019](docs/decisions/ADR-019-rejection-of-unit-of-work.md).
- ❌ Breaking API changes without prior discussion
- ❌ Generated files or build artifacts
- ❌ Reflection-based code that breaks NativeAOT/Trimming compatibility ([ADR-007](docs/decisions/ADR-007-native-aot-compatibility.md))

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
