# ADR-011: Rejection of ConcurrentQueue for Domain Events

## Status
Accepted

## Context
A previous implementation of `AggregateRoot<TId>` utilized `ConcurrentQueue<IDomainEvent>` for its `_domainEvents` collection to presumably offer thread safety when raising events.

## Decision
We explicitly reject the use of `ConcurrentQueue` and any concurrent collections for `AggregateRoot` internal state.

The Aggregate Root pattern in DDD acts as a consistency boundary for a single transaction. By definition, a single instance of an Aggregate Root should not be concurrently mutated by multiple threads. The application layer (Command Handlers) must ensure exclusive access (e.g. optimistic concurrency via row versions, or locking) before mutating the aggregate.

Furthermore, `ConcurrentQueue` allocates memory upon instantiation and its `ToArray()` method allocates a new array on every call, violating our strict performance requirement for zero-allocation hydration.

## Consequences
- **Positive:** Restores lazy, zero-allocation behavior via `List<IDomainEvent>?`.
- **Negative:** None. Aggregate roots are not thread-safe, as intended.
