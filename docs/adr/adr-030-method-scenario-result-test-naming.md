# ADR-030: Adoption of Method_Scenario_Result (Osherove) Test Naming Convention

**Date:** 2026-08-17  
**Status:** Accepted  
**Deciders:** Erickson Lopez  
**Backlog Reference:** QA-002  
**Alias:** ADR-0030  

---

## Context

In high-reliability .NET libraries and enterprise architectures, test suites fulfill two fundamental roles:
1. **Automated Verification:** Guaranteeing that domain invariants, adapters, serialization, and infrastructural integration behave predictably under all operational scenarios.
2. **Living Specifications (Executable Documentation):** Documenting the system's exact capabilities, failure modes, boundary constraints, and business rules in a human-readable form that outlives traditional documentation.

Standard .NET coding conventions enforced by Roslyn style analyzers (specifically rule **IDE1006: Naming Rule Violation**) mandate that all methods follow strict PascalCase without delimiters or special characters.

While strictly appropriate for production APIs (src/), applying monolithic PascalCase to test methods (e.g., EqualsWhenComparingTransientEntitiesWithDifferentIdsReturnsFalse) severely degrades readability and diagnostic velocity:
- **CI/CD Triage Friction:** When a test fails in headless CI pipelines (GitHub Actions, Azure DevOps, GitLab CI) or CLI runners (dotnet test), developers must instantly parse what failed, under what input state, and what the expected outcome was without opening the test implementation file.
- **Visual Congestion in Test Explorers:** Without clear structural delimiters, long descriptive names become illegible walls of text in IDE Test Explorers.
- **Ambiguity in Responsibility:** Unstructured naming leads to vague titles (TestEquality, CheckException, VerifySave) that obscure the exact scenario under test.

## Decision

We officially adopt the **Method_Scenario_Result** pattern (the Osherove naming convention, based on Roy Osherove's *The Art of Unit Testing*) as the mandatory architectural standard for all unit, integration, and adapter test methods across the repository.

### 1. Tripartite Structural Taxonomy

Every test method name must consist of exactly three segments separated by single underscores (_):

\text{MethodUnderTest}\_\text{StateUnderTestOrScenario}\_\text{ExpectedBehaviorOrResult}

| Segment | Role | Description | Examples |
|---|---|---|---|
| **1. Method / Unit** | Target Under Test | The exact method, property, constructor, or operational unit being exercised. | Equals, Parse, AddDomainEvent, SaveChangesAsync |
| **2. Scenario** | State / Precondition | The specific input parameter, initial state, edge case, or context under test. | WhenReflexive, WithNullEvent, WhenTransient, WithHighConcurrency |
| **3. Result** | Expected Outcome | The deterministic outcome, return value, state mutation, or exception thrown. | ReturnsTrue, ThrowsArgumentNullException, EmitsDomainEvent, PersistsState |

#### Canonical Examples:
- Equals_WhenReflexive_MaintainsMathematicalIdentity
- AddDomainEvent_WithNullEvent_ThrowsArgumentNullException
- ClearDomainEvents_WhenCalled_RemovesAllQueuedEvents
- SaveChangesAsync_WithConcurrentAggregates_DispatchesEventsAtomically

### 2. Local Suppression of Analyzer Rule IDE1006

To eliminate false-positive build failures under <TreatWarningsAsErrors>true</TreatWarningsAsErrors> without compromising production standards, we establish a **local scoping policy**:

1. **Scoped Bounded Suppression:** We disable IDE1006 **exclusively within the 	ests/ directory tree** using a local 	ests/.editorconfig.
2. **Production Invariance:** The root .editorconfig preserves strict PascalCase enforcement across all production assemblies (src/).

#### Configuration (	ests/.editorconfig):
`ini
[*.cs]
# Disables IDE1006 (Naming Rule Violation) specifically for test projects
# to permit the tripartite Method_Scenario_Result (Osherove) pattern,
# enabling tests to act as human-readable living specifications in CI/CD.
dotnet_diagnostic.IDE1006.severity = none
`

### 3. Naming Anti-Patterns (Prohibited)

| Anti-Pattern | Bad Example | Compliant Replacement |
|---|---|---|
| **Vague Action Prefix** | TestEquals() | Equals_WhenEntitiesHaveSameId_ReturnsTrue |
| **Monolithic PascalCase** | ParseWithInvalidGuidThrows() | Parse_WithInvalidGuidString_ThrowsFormatException |
| **Missing Expected Outcome** | Save_WhenValid() | Save_WhenValidState_PersistsEntityAndClearsEvents |
| **Arbitrary Underscore Count** | Entity_Id_Should_Not_Be_Empty() | Constructor_WithDefaultId_GeneratesValidIdentifier |

## Consequences

### Positive
- **Instant CI Diagnostic Velocity:** CI build logs and Test Explorers immediately communicate the failing unit, the failing context, and the violated expectation without opening source code.
- **Living Architecture Documentation:** Test suites serve as unambiguous, executable domain specifications.
- **Clean Separation of Concerns:** Production code strictly follows Microsoft BCL PascalCase standards, while test code follows domain-driven specification naming.
- **Zero Build Friction:** Eliminates compiler/analyzer warnings under zero-tolerance QA policies (TreatWarningsAsErrors).

### Negative
- **Deviates from BCL Production Conventions:** Test method names deliberately violate standard PascalCase method rules.
- **Requires Governance via EditorConfig:** Teams must maintain the isolated 	ests/.editorconfig across all testing directories.

## References
- Osherove, Roy. *The Art of Unit Testing: with examples in C#*. Manning Publications.
- Microsoft Learn: [IDE1006 - Naming Rule Violation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide1006)
- Martin, Robert C. *Clean Code: A Handbook of Agile Software Craftsmanship*. Prentice Hall.
