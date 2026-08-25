# ADR-022: Rejection of Security and Identity Abstractions (`ICurrentUser`, `UserId`, `Claims`)

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of authentication/authorization concepts from Tier 0 SharedKernel.

---

## Context

Proposals sometimes suggest embedding user identity or security context interfaces (`ICurrentUser`, `ISecurityContext`, `UserId`) in the Shared Kernel so domain logic can verify "who is executing this action."

## Problem

1. **Domain vs. Security Context:** The core domain model models business rules and state changes (e.g. `Order.Approve(Manager manager)`), not HTTP headers, JWT tokens, or `ClaimsPrincipal`.
2. **Infrastructure/Web Coupling:** `ClaimsPrincipal`, JWT authentication, and ASP.NET Core identity are presentation and infrastructure concerns. Coupling the Shared Kernel to identity structures contaminates the domain.
3. **Impaired Testability:** Domain unit tests would require mocking user security contexts or HTTP state, creating friction for pure business logic verification.

## Decision

**Explicitly reject all security, user context, and authorization abstractions from `EricksonLopez.SharedKernel`.**

## Architectural Placement

Security and current user context resolution belong to the **Application layer** (e.g., Command Handlers, Pipeline Behaviors) and **Infrastructure layer** (ASP.NET Core HTTP Context middleware).

## Consequences

- **Positive:** Domain models are 100% testable in isolation without security harnesses.
- **Positive:** Zero dependency on web hosting or identity frameworks.
