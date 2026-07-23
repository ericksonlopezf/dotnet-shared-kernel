# Governance Model

This document outlines the governance model for the `EricksonLopez.SharedKernel` project and its related ecosystem.

## Benevolent Dictator For Life (BDFL)

Currently, this project follows the BDFL governance model. 
Erickson López (@ericksonlopezf) acts as the BDFL and has the final say on architectural decisions, roadmap priorities, and merging pull requests.

As the project and community grow, this model may evolve into a Core Team consensus model.

## Roles and Responsibilities

### Users
Anyone who uses the library. We encourage users to participate by:
- Providing feedback via GitHub Discussions.
- Reporting bugs via Issues.
- Suggesting new features.

### Contributors
Anyone who submits a Pull Request that gets merged. Contributors are expected to:
- Follow the [Contributing Guidelines](CONTRIBUTING.md).
- Adhere to the [Code of Conduct](CODE_OF_CONDUCT.md).
- Write high-quality, tested code.

### Triagers
Community members who have shown consistent involvement and are granted issue triage permissions. Triagers can:
- Label issues and PRs.
- Close invalid or duplicate issues.
- Request more information from authors.

*To become a Triager, simply stay active in the issue tracker and help other users.*

### Core Maintainers
Individuals who have push access to the repository and can merge PRs. Currently, this role is limited to the BDFL. 

## Communication Channels

- **Bug Reports:** Use [GitHub Issues](https://github.com/ericksonlopezf/dotnet-shared-kernel/issues).
- **Feature Requests:** Open an Issue for discussion before starting work.
- **Q&A / General Discussion:** Use [GitHub Discussions](https://github.com/ericksonlopezf/dotnet-shared-kernel/discussions).
- **Security Vulnerabilities:** Follow the process in [SECURITY.md](SECURITY.md).

## Decision Making Process

1. **Proposal:** Major architectural changes should be proposed as an issue.
2. **Discussion:** The community discusses the proposal.
3. **Decision:** The BDFL makes a final decision. If approved, an ADR (Architecture Decision Record) is created in `docs/decisions/`.
4. **Implementation:** Code is written and submitted via a PR.
