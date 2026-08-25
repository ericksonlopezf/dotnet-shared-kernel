# Bounded Context & Layer Boundaries

---

## 1. Domain vs Infrastructure Segregation

```mermaid
classDiagram
    class DomainLayer {
        +Entity~TId~
        +AggregateRoot~TId~
        +IDomainEvent
        +IEntityId~T~
    }
    class ApplicationLayer {
        +IRepository~TEntity~
        +IUnitOfWork
    }
    class InfrastructureLayer {
        +DomainEventsInterceptor
        +DapperUnnestExtensions
        +JsonConverters
    }

    DomainLayer <|-- ApplicationLayer
    ApplicationLayer <|-- InfrastructureLayer
```

- **Domain Layer**: Completely isolated from database connection strings, ORM contexts, and serializer options.
- **Application Layer**: Declares persistence port interfaces (`IRepository`, `IUnitOfWork`).
- **Infrastructure Layer**: Implements adapters using EF Core or Dapper.
