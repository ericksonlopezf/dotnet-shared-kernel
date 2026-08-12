# ADR-010: Lazy Allocation for Domain Events

## Status
Accepted

## Context
The `AggregateRoot<TId>` base class exposes a `DomainEvents` collection. When fetching thousands of read-only aggregates from a database using Dapper or EF Core, allocating an empty collection for domain events per aggregate creates significant GC pressure (heap allocation).

## Decision
We implement a strictly lazy allocation strategy. The internal field `List<IDomainEvent>? _domainEvents` remains `null` until the first event is explicitly raised via `RaiseDomainEvent()`. 

We rejected using `ConcurrentQueue<IDomainEvent>` because it forces eager allocation (e.g., `new()`) and implies concurrent thread safety that an Aggregate Root does not provide or need.

## Consequences
- **Positive:** Zero bytes allocated for domain events on read-only entity hydration.
- **Negative:** Minor additional logic for `null` checks on read (`_domainEvents?.AsReadOnly() ?? Array.Empty<IDomainEvent>()`).
