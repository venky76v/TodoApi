```mermaid
graph TB
    subgraph "Azi.Project.CytoraBFI.BackgroundTasks"
        Program["Program.cs<br/>(Host Builder)"]
        Startup["Startup.cs<br/>(DI Configuration)"]
        
        subgraph "Background Services"
            RetrieveEmail["RetrieveEmailTask"]
            PushToCytora["PushEmailToCytoraTask"]
            RetrieveISO["RetrieveISOInfoFromCytoraTask"]
            CreateClient["CreateClientTask"]
            CreateQuote["CreateFastQuoteTask"]
            PushToSaperion["PushEmailToSaperionTask"]
            SendToCTP["SendMessageToCTPFlowTask"]
            ConsumeExceptions["ConsumeITCPEExceptions"]
            ConsumeClientKey["ConsumerISOClientKey"]
            ConsumePolicyNum["ConsumerISOPolicyNumber"]
        end
        
        Config["appsettings.json<br/>Environment Variables"]
    end
    
    subgraph "Azi.Project.CytoraBFI.Infrastructure"
        DbContext["CytoraDbContext<br/>(EF Core)"]
        
        subgraph "Repositories"
            EmailRepo["EmailRepository"]
            ClientRepo["ClientRepository"]
            QuoteRepo["QuoteRepository"]
            PolicyRepo["PolicyRepository"]
        end
        
        subgraph "External Services"
            CytoraService["CytoraService<br/>(HTTP Client)"]
            EmailService["EmailService<br/>(IMAP/SMTP)"]
            SaperionService["SaperionService<br/>(HTTP Client)"]
            CTPService["CTPFlowService<br/>(HTTP Client)"]
            QueueService["MessageQueueService<br/>(RabbitMQ/Azure SB)"]
        end
        
        Migrations["EF Migrations"]
        SeedData["SeedData.cs"]
        Configs["Configuration POCOs"]
    end
    
    subgraph "Azi.Project.CytoraBFI.Domain"
        Dtos["DTOs<br/>(CytoraMessageDto, etc.)"]
        Entities["Entities<br/>(Email, Client, Policy)"]
        ValueObjects["Value Objects"]
        Interfaces["Interfaces<br/>(IRepository, IService)"]
        IntegrationEvents["Integration Events"]
    end
    
    subgraph "External Systems"
        Mailbox["Email Server<br/>(IMAP/POP3)"]
        CytoraAPI["Cytora API<br/>(Insurance Platform)"]
        CTPAPI["CTP Flow API<br/>(Policy Workflow)"]
        SaperionAPI["Saperion API<br/>(Document Mgmt)"]
        ITCPEQ["ITCPE Queue<br/>(Exceptions)"]
        ISOQueues["ISO Queues<br/>(ClientKey, PolicyNum)"]
        SQLServer["SQL Server<br/>(CytoraBFI DB)"]
    end
    
    subgraph "Deployment"
        Docker["Docker Container"]
        K8s["Kubernetes<br/>(cytorabfi-backgroundtasks.yaml)"]
    end
    
    Program --> Startup
    Startup --> Config
    Startup --> RetrieveEmail
    Startup --> PushToCytora
    Startup --> RetrieveISO
    Startup --> CreateClient
    Startup --> CreateQuote
    Startup --> PushToSaperion
    Startup --> SendToCTP
    Startup --> ConsumeExceptions
    Startup --> ConsumeClientKey
    Startup --> ConsumePolicyNum
    
    RetrieveEmail --> EmailService
    RetrieveEmail --> EmailRepo
    
    PushToCytora --> EmailRepo
    PushToCytora --> CytoraService
    
    RetrieveISO --> CytoraService
    RetrieveISO --> ClientRepo
    
    CreateClient --> ClientRepo
    CreateClient --> CytoraService
    
    CreateQuote --> QuoteRepo
    CreateQuote --> CytoraService
    CreateQuote --> CTPService
    
    PushToSaperion --> EmailRepo
    PushToSaperion --> SaperionService
    
    SendToCTP --> PolicyRepo
    SendToCTP --> CTPService
    
    ConsumeExceptions --> QueueService
    ConsumeClientKey --> QueueService
    ConsumeClientKey --> ClientRepo
    ConsumePolicyNum --> QueueService
    ConsumePolicyNum --> PolicyRepo
    
    EmailRepo --> DbContext
    ClientRepo --> DbContext
    QuoteRepo --> DbContext
    PolicyRepo --> DbContext
    
    DbContext --> Entities
    DbContext --> Migrations
    DbContext --> SeedData
    
    EmailRepo --> Interfaces
    ClientRepo --> Interfaces
    CytoraService --> Interfaces
    EmailService --> Interfaces
    
    Dtos -.-> "Used by all layers" -.-> BackgroundTasks
    
    EmailService --> Mailbox
    CytoraService --> CytoraAPI
    CTPService --> CTPAPI
    SaperionService --> SaperionAPI
    QueueService --> ITCPEQ
    QueueService --> ISOQueues
    DbContext --> SQLServer
    
    Program --> Docker
    Docker --> K8s
    
    Configs --> Config
    
    classDef backgroundTask fill:#e1f5ff,stroke:#0078d4,stroke-width:2px
    classDef infrastructure fill:#fff4e1,stroke:#ff9800,stroke-width:2px
    classDef domain fill:#e8f5e9,stroke:#4caf50,stroke-width:2px
    classDef external fill:#fce4ec,stroke:#e91e63,stroke-width:2px
    classDef deployment fill:#f3e5f5,stroke:#9c27b0,stroke-width:2px
    
    class RetrieveEmail,PushToCytora,RetrieveISO,CreateClient,CreateQuote,PushToSaperion,SendToCTP,ConsumeExceptions,ConsumeClientKey,ConsumePolicyNum backgroundTask
    class DbContext,EmailRepo,ClientRepo,QuoteRepo,PolicyRepo,CytoraService,EmailService,SaperionService,CTPService,QueueService infrastructure
    class Dtos,Entities,ValueObjects,Interfaces,IntegrationEvents domain
    class Mailbox,CytoraAPI,CTPAPI,SaperionAPI,ITCPEQ,ISOQueues,SQLServer external
    class Docker,K8s deployment
