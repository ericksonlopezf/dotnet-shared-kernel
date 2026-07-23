# Contributing to EricksonLopez.SharedKernel

Thank you for your interest in contributing! This document provides guidelines for contributing to this project.

## Getting Started

1. **Fork** the repository
2. **Clone** your fork locally
3. Create a **branch** (see Branching Strategy): `git checkout -b feature/your-feature develop`
4. Make your changes
5. **Run tests** to ensure nothing is broken
6. **Commit** with a clear message
7. **Push** to your fork and create a **Pull Request** against `develop`

## Prerequisites

Before contributing, ensure you have the following installed:
- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
- An IDE such as Visual Studio, JetBrains Rider, or VS Code with the C# Dev Kit.

## Development Setup

```bash
# Clone
git clone https://github.com/<your-username>/dotnet-shared-kernel.git
cd dotnet-shared-kernel

# Run the unified build script (restores, builds, formats, and tests)
# On Windows:
.\build.ps1

# On Linux/macOS:
./build.sh
```

## Code Standards

This project enforces strict code quality. Your PR must pass all of these:

- **TreatWarningsAsErrors** — zero warnings allowed
- **Nullable enabled** — all reference types must be annotated
- **WarningLevel 5** — maximum warning sensitivity
- **EnforceCodeStyleInBuild** — code style rules are build errors
- **EditorConfig** — formatting rules in `.editorconfig`

### Branching Strategy

We use the following prefixes for branches:
- `feature/*` — For new abstractions or features.
- `fix/*` — For bug fixes.
- `docs/*` — For documentation updates.
- `chore/*` — For tooling, CI/CD, or dependency updates.

### Commit Conventions

We strictly follow [Conventional Commits](https://www.conventionalcommits.org/). Your commit messages should be structured as follows:

```
<type>[optional scope]: <description>
```
**Allowed types:** `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.

### Snapshot Testing (Verify.Xunit)

We use `Verify.Xunit` to ensure that our output formats (like `ToString()` on errors) do not change unexpectedly. If you modify a format, a snapshot test will fail and a `.received.txt` file will be generated.
To accept the new snapshot:
1. Review the `.received.txt` file to ensure the new format is correct.
2. Rename the `.received.txt` file to `.verified.txt` (overwriting the old one).
3. Commit the updated `.verified.txt` file.

## Test Structure

This repository separates testing concerns into three projects:
1. **`EricksonLopez.SharedKernel.UnitTests`** — Standard unit tests for all domain and Result primitives.
2. **`EricksonLopez.SharedKernel.Benchmarks`** — Performance benchmarks using `BenchmarkDotNet` to prove zero-allocation claims. Run these in `Release` mode before submitting performance-related PRs.
3. **`EricksonLopez.SharedKernel.Testing`** — A specialized library containing fluent assertions (e.g. `ShouldBeSuccess`) intended to be distributed as a NuGet package for consumers.

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

- ❌ External dependencies — this package has zero external dependencies by design
- ❌ Breaking API changes without prior discussion
- ❌ Generated files or build artifacts

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
