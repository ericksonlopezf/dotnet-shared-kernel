# CI/CD & Build Pipeline

---

## 1. Automated Workflow Topology

GitHub Actions workflow matrix for `EricksonLopez.SharedKernel`:
- `ci.yml`: Root orchestrator.
- `dotnet-build-test.yml`: SDK .NET 10 compilation, SonarCloud, and Coverlet code coverage ($\ge 99\%$).
- `aot-smoke-test.yml`: Standalone NativeAOT compiler test harness.
- `mutation-testing.yml`: Stryker.NET mutation testing quality gate ($\ge 95\%$).
- `publish.yml`: NuGet.org OIDC deployment and Sigstore provenance attestation.
- `release-please.yml`: Google Release Please Conventional Commits automation.
- `repo-compliance.yml`: Architecture and kebab-case compliance gate.
- `benchmarks.yml` & `weekly-benchmarks.yml`: Performance baseline tracking.
