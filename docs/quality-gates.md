# DevSecOps Quality Gates Specification

---

## 1. Mandatory Quality Gates

1. **Gate 1: Build & Diagnostics** (`TreatWarningsAsErrors=true`, CS1591 XML docs)
2. **Gate 2: Fast-Path Unit Tests** (100% pass on .NET 10)
3. **Gate 3: Code Coverage** ($\ge 99\%$ Coverlet + Codecov)
4. **Gate 4: SonarCloud Static Code Analysis** (Quality Gate 'A', 0 vulnerabilities)
5. **Gate 5: NativeAOT Smoke Test** (`PublishAot=true` standalone Linux-x64 binary)
6. **Gate 6: Stryker Mutation Testing** ($\ge 95\%$ break threshold before release)
