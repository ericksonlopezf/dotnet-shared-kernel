# Architecture

This document describes the foundational architectural patterns implemented in `EricksonLopez.SharedKernel`.

## The Result Pattern

The `Result` and `Result<T>` types provide a functional approach to error handling, completely avoiding the use of exceptions for expected domain failures.

```mermaid
sequenceDiagram
    participant Controller as API Controller
    participant Service as Application Service
    participant Domain as Domain Model
    
    Controller->>Service: Handle(Command)
    Service->>Domain: ExecuteBusinessLogic()
    
    alt Success Path
        Domain-->>Service: Result.Success(Value)
        Service-->>Controller: Match(onSuccess)
        Controller-->>Client: 200 OK
    else Expected Failure (e.g. Validation, NotFound)
        Domain-->>Service: Result.Failure(Error)
        Service-->>Controller: Match(onFailure)
        Controller-->>Client: 400/404/409 (Mapped from ErrorType)
    end
```

### Result Pipeline Flow

```mermaid
flowchart TD
    Start[Start Operation] --> Validate[Ensure(predicate)]
    Validate -- Failure --> Error[MapError / Return]
    Validate -- Success --> Map[Map(dto)]
    Map --> Tap[Tap(side_effect)]
    Tap --> End[Return Result<T>]
    
    style Error fill:#ff9999,stroke:#333,stroke-width:2px
    style End fill:#99ccff,stroke:#333,stroke-width:2px
```

## Domain Events & Outbox Pattern

The `AggregateRoot` is the only entity allowed to raise `IDomainEvent`s. This ensures the consistency boundary is respected. The events are typically dispatched by the infrastructure layer (Unit of Work) and persisted using the Outbox pattern.

```mermaid
sequenceDiagram
    participant Command as Command Handler
    participant Aggregate as AggregateRoot
    participant UoW as Unit of Work
    participant Db as Database
    participant Outbox as Outbox Table
    participant Publisher as Event Publisher

    Command->>Aggregate: DoBusinessOperation()
    Aggregate->>Aggregate: RaiseDomainEvent(Event)
    Aggregate-->>Command: Result
    
    Command->>UoW: SaveChangesAsync()
    UoW->>Db: Begin Transaction
    UoW->>Db: Save Aggregate Changes
    
    UoW->>Aggregate: DomainEvents
    Aggregate-->>UoW: List<IDomainEvent>
    UoW->>Aggregate: ClearDomainEvents()
    
    UoW->>Outbox: Save Events as Outbox Messages
    UoW->>Db: Commit Transaction
    
    Note over Outbox, Publisher: Background Worker
    Outbox->>Publisher: Process Pending Messages
    Publisher-->>Subscribers: Publish(Event)
```
