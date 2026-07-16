# Contributing to EricksonLopez.SharedKernel

Thank you for your interest in contributing! This document provides guidelines for contributing to this project.

## Getting Started

1. **Fork** the repository
2. **Clone** your fork locally
3. Create a **feature branch** from `develop`: `git checkout -b feature/your-feature develop`
4. Make your changes
5. **Run tests** to ensure nothing is broken
6. **Commit** with a clear message
7. **Push** to your fork and create a **Pull Request** against `develop`

## Development Setup

```bash
# Clone
git clone https://github.com/<your-username>/dotnet-shared-kernel.git
cd dotnet-shared-kernel

# Restore and build
dotnet restore
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run benchmarks
dotnet run --project benchmarks/EricksonLopez.SharedKernel.Benchmarks --configuration Release
```

## Code Standards

This project enforces strict code quality. Your PR must pass all of these:

- **TreatWarningsAsErrors** — zero warnings allowed
- **Nullable enabled** — all reference types must be annotated
- **WarningLevel 5** — maximum warning sensitivity
- **EnforceCodeStyleInBuild** — code style rules are build errors
- **EditorConfig** — formatting rules in `.editorconfig`

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
