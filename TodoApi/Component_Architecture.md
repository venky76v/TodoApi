```mermaid
graph TB
    subgraph "Presentation Layer"
        Tasks[Background Tasks<br/>7 Scheduled Services<br/>3 Message Consumers]
        Health[Health Checks<br/>/health endpoint]
    end

    subgraph "Application Layer"
        Config[Configuration<br/>Settings Wrappers]
        Extensions[Service Extensions<br/>DI Registration]
    end

    subgraph "Domain Layer"
        Entities[Entities<br/>CytoraMessage<br/>CytoraMessageStatus<br/>CytoraMessageResponse]
        DTOs[DTOs<br/>Data Transfer Objects]
        Interfaces[Interfaces<br/>Repository & Service Contracts]
        ValueObjects[Value Objects<br/>Address]
    end

    subgraph "Infrastructure Layer"
        DbContext[CytoraDbContext<br/>EF Core]
        Repos[Repositories<br/>Data Access]
        Services[Services<br/>CytoraService<br/>CytoraApiService<br/>EmailService]
        Migrations[EF Migrations<br/>Schema Versioning]
    end

    subgraph "External Dependencies"
        SQL[(SQL Server)]
        MQ[RabbitMQ]
        APIs[External APIs<br/>Cytora, Graph API]
    end

    Tasks --> Config
    Tasks --> Extensions
    Tasks --> Interfaces
    
    Extensions --> Services
    Extensions --> Repos
    
    Services --> Interfaces
    Services --> DTOs
    Services --> Repos
    
    Repos --> DbContext
    Repos --> Entities
    
    DbContext --> Migrations
    DbContext --> SQL
    
    Services --> APIs
    Tasks --> MQ

    style Entities fill:#E91E63,color:#fff
    style Services fill:#3F51B5,color:#fff
    style Tasks fill:#4CAF50,color:#fff
