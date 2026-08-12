# Testing Strategy

This repository follows a strict testing strategy designed to maintain highest-level reliability for the Shared Kernel primitives.

## 1. Principles
- **FIRST Principle**: All tests must be Fast, Independent, Repeatable, Self-Validating, and Timely.
- **No I/O dependencies**: Tests in this suite must run completely in memory without hitting the file system, network, or real databases.

## 2. Naming Conventions
Tests must follow the `Method_Scenario_Result` naming convention:
- `Equals_WithProxyType_ReturnsTrue`
- `RaiseDomainEvent_WhenCalledConcurrently_ShouldBeThreadSafe`

## 3. Libraries Used
- **xUnit**: As the core test runner.
- **FluentAssertions / AwesomeAssertions**: Used for declarative and expressive assertions.
- **NetArchTest.Rules**: Used in `ArchitectureTests` to enforce Clean Architecture boundaries (e.g. no coupling to `MediatR` or `EntityFrameworkCore`).

## 4. Mutation Testing (Stryker.NET)
We employ Mutation Testing to verify the quality of our assertions.
- **Thresholds**: 
  - Target: `100%` (High)
  - Break Build: `95%`
- All business rules must have assertions that break if the logic is mutated. Suppressions (`// Stryker disable`) are not allowed without explicit technical justification in the PR.

## 5. Running the Tests
To execute the unit tests:
```bash
dotnet test
```

To run mutation testing:
```bash
dotnet tool restore
dotnet stryker
```
