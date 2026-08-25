# ADR-021: Rejection of Multi-Tenancy Abstractions (`ITenant`, `TenantId`)

**Date:** 2026-08-15  
**Status:** Rejected / Excluded  
**Deciders:** Erickson Lopez  
**Context:** Architectural Audit — Discard of multi-tenant interfaces from Tier 0 SharedKernel.

---

## Context

Multi-tenancy is common in SaaS architectures. Requests often arise to embed `ITenantEntity`, `TenantId`, or `IMustHaveTenant` directly into the Shared Kernel.

## Problem

1. **Non-Universal Concern:** Not all systems, microservices, or Bounded Contexts are multi-tenant. Systems with database-per-tenant or schema-per-tenant architectures do not need `TenantId` on domain entities at all.
2. **Coupling to Multi-Tenancy Strategy:** Different architectures handle tenancy differently:
   - Row-level security (RLS in PostgreSQL/SQL Server)
   - Global Query Filters in EF Core
   - Composite keys (`(TenantId, EntityId)`)
   - Ambient headers in HTTP middleware
3. **Premature Abstraction:** Embedding tenancy in Tier 0 forces single-tenant services or internal background processors to carry dead abstractions.

## Decision

**Explicitly reject `ITenant`, `TenantId`, and related multi-tenant interfaces from `EricksonLopez.SharedKernel`.**

## Architectural Placement

Multi-tenancy should be handled via a dedicated package (`EricksonLopez.Tenancy` if needed in the future) or implemented in each consumer's **Application and Infrastructure layers**.

## Consequences

- **Positive:** `EricksonLopez.SharedKernel` remains lean and applicable to single-tenant, multi-tenant, and isolated microservices alike.
- **Positive:** Zero opinions on tenant isolation strategies.
