# Architecture Guide â€” EricksonLopez.SharedKernel

This guide describes the architecture of the library and how its components fit within the context of Domain-Driven Design (DDD).

---

## General Architecture

The library exposes a minimal set of primitives that act as the foundation of the Domain layer in a Clean Architecture.

```mermaid
classDiagram
    class IEquatable~T~ {
        <<interface>>
        +Equals(other: T) bool
    }

    class IDomainEvent {
        <<interface>>
    }

    class Entity~TId~ {
        <<abstract>>
        +Id: TId [protected init]
        +IsTransient() bool
        +Equals(other: Entity~TId~?) bool
        +Equals(obj: object?) bool
        +GetHashCode() int
        +operator ==(left, right) bool
        +operator !=(left, right) bool
    }

    class AggregateRoot~TId~ {
        <<abstract>>
        -_domainEvents: List~IDomainEvent~? [lazy]
        +DomainEvents: IReadOnlyCollection~IDomainEvent~
        #RaiseDomainEvent(domainEvent: IDomainEvent)
        +ClearDomainEvents()
    }

    IEquatable <|.. Entity : implements
    Entity <|-- AggregateRoot : inherits
    AggregateRoot --> IDomainEvent : produces
```

---

## Entity State Diagram

```mermaid
stateDiagram-v2
    [*] --> Transient : new Entity(default Id)
    Transient --> Persistent : Id = RealValue (in constructor)
    Persistent --> Persistent : State changes (non-Id fields)
    note right of Transient
        IsTransient() = true
        GetHashCode() = instance hash
        Equals() = always false (even with same default Id)
    end note
    note right of Persistent
        IsTransient() = false
        GetHashCode() = HashCode.Combine(Type, Id)
        Equals() = true if same Type + Id
    end note
```

---

## Main Flow â€” Domain Event Generation and Dispatch

```mermaid
sequenceDiagram
    participant App as Application Layer<br/>(Command Handler)
    participant AR as AggregateRoot<br/>(Domain Layer)
    participant UoW as UnitOfWork / Repository<br/>(Infrastructure)
    participant DB as Database<br/>(Infrastructure)
    participant Pub as Publisher / Mediator<br/>(Infrastructure)

    App->>AR: Execute use case (e.g. Order.Place())
    AR->>AR: Validate business invariants
    AR->>AR: Mutate internal state
    AR->>AR: RaiseDomainEvent(new OrderPlacedEvent())

    App->>UoW: SaveChangesAsync()
    UoW->>DB: Persist aggregate state
    DB-->>UoW: OK

    UoW->>AR: Read DomainEvents (snapshot)
    UoW->>AR: ClearDomainEvents()

    loop For each IDomainEvent
        UoW->>Pub: Publish(domainEvent)
        Pub-->>UoW: OK
    end

    UoW-->>App: OK
    App-->>App: Return result to client
```

---

## Error Flow â€” RaiseDomainEvent with null

```mermaid
sequenceDiagram
    participant AR as AggregateRoot
    participant Guard as ArgumentNullException.ThrowIfNull

    AR->>Guard: RaiseDomainEvent(null!)
    Guard-->>AR: throws ArgumentNullException
    Note over AR: domainEvent = null<br/>â†’ immediate ArgumentNullException
```

---

## Pipeline Diagram â€” UnitOfWork Pattern

```mermaid
flowchart TD
    A[Command Handler] --> B[AggregateRoot.Method]
    B --> C{Invariants OK?}
    C -- No --> D[throw InvalidOperationException]
    C -- Yes --> E[Mutate internal state]
    E --> F[RaiseDomainEvent]
    F --> G[DomainEvents accumulated]
    G --> H[UoW.SaveChangesAsync]
    H --> I[Persist to DB]
    I --> J{DB OK?}
    J -- No --> K[throw / rollback]
    J -- Yes --> L[Snapshot = DomainEvents.ToList]
    L --> M[ClearDomainEvents]
    M --> N[Publish snapshot events]
    N --> O{Publisher OK?}
    O -- No --> P[Outbox retry / dead letter]
    O -- Yes --> Q[Success]
```

---

## Component Dependencies

```mermaid
graph LR
    A["AggregateRoot&lt;TId&gt;"] -->|inherits| B["Entity&lt;TId&gt;"]
    B -->|implements| C["IEquatable&lt;Entity&lt;TId&gt;&gt;"]
    A -->|produces| D["IDomainEvent"]

    E["Your Aggregate (e.g. Order)"] -->|inherits| A
    F["Your Entity (e.g. OrderLine)"] -->|inherits| B
    G["Your Event (e.g. OrderPlacedEvent)"] -->|implements| D
```

---

## Architectural Layers

```mermaid
graph TD
    subgraph "External (Frameworks, UI, DB)"
        EF["EF Core / Dapper"]
        MQ["MediatR / RabbitMQ"]
    end

    subgraph "Infrastructure (your project)"
        Repo["Repositories"]
        UoW2["UnitOfWork"]
        Pub2["Publisher"]
    end

    subgraph "Application (your project)"
        CH["Command Handlers"]
        QH["Query Handlers"]
    end

    subgraph "Domain (this library + your code)"
        AGG["AggregateRoot&lt;TId&gt;"]
        ENT["Entity&lt;TId&gt;"]
        EVT["IDomainEvent"]
        TUS["Your Aggregates / Entities / Events"]
    end

    External --> Infrastructure
    Infrastructure --> Application
    Application --> Domain
    Domain -..->|no dependencies| X["(nothing)"]
```

**Rule:** Arrows point inward. The Domain has no external dependencies.

---

## Transactional Boundary

In DDD, the `AggregateRoot` is the transactional boundary. The library enforces this concept by allowing **only** aggregates (not base entities) to register `IDomainEvent`.

```
Database transaction
â”‚
â””â”€ UnitOfWork.SaveChangesAsync()
      â”‚
      â”œâ”€ Persists all AggregateRoot state changes
      â””â”€ Dispatches all AggregateRoot DomainEvents
            (or saves them to an Outbox within the same transaction)
```

One transaction = one AggregateRoot (the golden rule of DDD).

---

## Integration with Infrastructure

The library provides the domain contract. Infrastructure implements the adapters:

| Infrastructure Component | Uses from the library |
|---|---|
| Repository | `Entity<TId>.Id` as the key |
| UnitOfWork | `AggregateRoot<TId>.DomainEvents` + `ClearDomainEvents()` |
| Publisher / Outbox | Receives `IDomainEvent` |
| Event Handlers | Receive the concrete `IDomainEvent` |