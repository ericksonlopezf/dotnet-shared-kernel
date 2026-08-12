# ADR-016: Extraction of Pagination to Dedicated Package

**Date:** 2026-08-12
**Status:** Accepted
**Deciders:** Erickson Lopez

## Context

In version 1.x of the `EricksonLopez.SharedKernel`, the types `PaginationParameters` and `PagedList<T>` were included as part of the core library. These types provided a standardized way to handle pagination parameters (page number, page size) and return paginated result sets.

However, a core architectural principle of this Shared Kernel (defined in ADR-002) is to maintain a strict focus on foundational Domain-Driven Design (DDD) primitives (`Entity<TId>`, `AggregateRoot<TId>`, `IDomainEvent`). 

Pagination is primarily an application and infrastructure concern (Query layer, UI projection, SQL offset/fetch), rather than a core domain primitive.

## Decision

We have decided to remove all pagination-related types (`PaginationParameters`, `PagedList<T>`) from `EricksonLopez.SharedKernel` in version 2.0 and extract them into their own dedicated package: **`EricksonLopez.Pagination`**.

Reasons:
1. **Separation of Concerns:** Pagination is a query-side concern (CQRS). The Shared Kernel is focused on the write-side domain model. Mixing them violates the Single Responsibility Principle of the package.
2. **Lean Core:** Removing non-domain features keeps the Shared Kernel minimal, focused, and aligned with its primary goal of providing foundational DDD primitives.
3. **Opt-in Functionality:** Consumers who only need the domain primitives do not have to pull in pagination abstractions they may not use.

## Consequences

### Positive
- The `SharedKernel` is further purified to strictly domain concerns.
- `EricksonLopez.Pagination` can evolve independently, potentially adding infrastructure-specific integrations (e.g., EF Core async pagination extensions) without bloating the Shared Kernel.

### Negative
- **BREAKING CHANGE:** Consumers of v1.x who relied on `PaginationParameters` or `PagedList<T>` will experience broken builds upon upgrading to v2.0.

## Migration

Consumers who used pagination types via the Shared Kernel must add a direct dependency to the new package:

```xml
<PackageReference Include="EricksonLopez.Pagination" Version="x.y.z" />
```
