# ADR-010: Lazy Allocation for Domain Events

## Status
Accepted

## Context
The `AggregateRoot<TId>` base class handles domain event recording and subsequent draining by infrastructure. When fetching thousands of read-only aggregates from a database using Dapper or EF Core, allocating an empty collection for domain events per aggregate creates significant GC pressure (heap allocation).

## Decision
We implement a strictly lazy allocation strategy. The internal field `List<IDomainEvent>? _domainEvents` remains `null` until the first event is explicitly raised via `RaiseDomainEvent()`. The public API is `DrainDomainEvents()` which atomically snapshots and clears events; it returns `Array.Empty<IDomainEvent>()` when no events are pending (zero allocation path).

We rejected using `ConcurrentQueue<IDomainEvent>` because it forces eager allocation (e.g., `new()`) and implies concurrent thread safety that an Aggregate Root does not provide or need.

## Consequences
- **Positive:** Zero bytes allocated for domain events on read-only entity hydration.
- **Negative:** Minor additional logic for `null` checks on read (`_domainEvents is null or { Count: 0 }` → return `Array.Empty`).
