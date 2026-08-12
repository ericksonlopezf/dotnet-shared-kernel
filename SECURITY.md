# Security Policy

## Supported Versions

| Version | Supported |
|---|---|
| 1.1.x | ✅ |
| 1.0.x | ✅ |

## Reporting a Vulnerability

If you discover a security vulnerability, **please do not open a public issue.**

Instead, we encourage you to use [GitHub Private Vulnerability Reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability) directly in this repository.

Alternatively, you can email **[ericksonlopezf@gmail.com](mailto:ericksonlopezf@gmail.com)** with:

1. **Description** of the vulnerability
2. **Steps to reproduce** (or a proof-of-concept)
3. **Impact assessment** — what an attacker could achieve
4. **Suggested fix** (optional)

### What to expect

- **Acknowledgement** within 48 hours.
- **Resolution target** within 14 days for critical issues, 30 days for others.
- **Coordinated Vulnerability Disclosure (CVD):** We ask for a reasonable embargo period before public disclosure to allow consumers to update.
- Credit in the release notes (unless you prefer to remain anonymous).

## Scope

This policy covers the `EricksonLopez.SharedKernel` NuGet package and its source code.

**Zero external runtime dependencies**: This library has no external runtime NuGet dependencies on any of its supported TFMs (`net8.0`, `net9.0`, `net10.0`). The only build-time dependency is `Microsoft.SourceLink.GitHub`, which does not ship in the final package.

## Supply Chain Security

This package implements multiple layers of supply chain security:

### Strong Name Signing

Assemblies are signed with a Strong Name key (`EricksonLopez.snk`) when the key is available. The key is injected from the `SNK_KEY` GitHub Actions secret (base64-encoded) and never committed to the repository.

The signing configuration in `Directory.Build.props`:
```xml
<SignAssembly Condition="Exists('$(MSBuildThisFileDirectory)EricksonLopez.snk')">true</SignAssembly>
<AssemblyOriginatorKeyFile Condition="Exists('$(MSBuildThisFileDirectory)EricksonLopez.snk')">$(MSBuildThisFileDirectory)EricksonLopez.snk</AssemblyOriginatorKeyFile>
```

### Sigstore Provenance Attestation

Every published NuGet package receives a [Sigstore](https://www.sigstore.dev/) provenance attestation via `actions/attest-build-provenance`. This allows consumers to cryptographically verify the build provenance of the package.

### NuGet Trusted Publishing (OIDC)

Package publishing uses NuGet Trusted Publishing via `NuGet/login`, which authenticates using GitHub Actions OIDC tokens — no static API keys are stored as secrets.

### Deterministic Builds

Builds are deterministic (`<Deterministic>true</Deterministic>`) and `ContinuousIntegrationBuild` is enabled in CI, ensuring packages are reproducible.

### NuGet Audit

`NuGetAudit=true` is configured at the `all` level with `low` severity threshold, so dependency vulnerabilities are flagged at build time.

### Dependabot

Dependabot is configured to check both NuGet and GitHub Actions dependencies weekly, automatically opening PRs for security updates.

