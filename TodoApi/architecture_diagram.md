# Azi Cytora BFI - Architecture Diagrams

This document serves as an index to all architecture diagrams for the Azi Cytora BFI solution. Each diagram is available in its own file for easier navigation and reference.

[← Back to Documentation Home](README.md)

---

## Quick Navigation

| # | Diagram | Description | File |
|---|---------|-------------|------|
| 1 | [System Architecture](#1-system-architecture) | High-level system with external integrations | [View →](Diagrams/01_System_Architecture.md) |
| 2 | [Component Architecture](#2-component-architecture) | Clean Architecture layers and dependencies | [View →](Diagrams/02_Component_Architecture.md) |
| 3 | [Dependency Injection](#3-dependency-injection) | DI container setup and service registration | [View →](Diagrams/03_Dependency_Injection.md) |
| 4 | [Task Orchestration](#4-task-orchestration) | Background task execution model | [View →](Diagrams/04_Task_Orchestration.md) |
| 5 | [Message Flow State Machine](#5-message-flow-state-machine) | 11-state workflow with transitions | [View →](Diagrams/05_Message_Flow.md) |
| 6 | [Standard Flow Sequence](#6-standard-flow-sequence) | Email → Saperion workflow | [View →](Diagrams/06_Standard_Flow.md) |
| 7 | [CTP Alternative Flow](#7-ctp-alternative-flow) | CTP routing and exception handling | [View →](Diagrams/07_CTP_Flow.md) |
| 8 | [Data Model ERD](#8-data-model-erd) | Database schema and relationships | [View →](Diagrams/08_Data_Model.md) |
| 9 | [Deployment Architecture](#9-deployment-architecture) | Kubernetes deployment structure | [View →](Diagrams/09_Deployment.md) |
| 10 | [Configuration Hierarchy](#10-configuration-hierarchy) | Configuration sources and precedence | [View →](Diagrams/10_Configuration.md) |
| 11 | [Error Handling Flow](#11-error-handling-flow) | Error recovery and retry strategies | [View →](Diagrams/11_Error_Handling.md) |
| 12 | [Integration Points](#12-integration-points) | Complete external system integration map | [View →](Diagrams/12_Integration_Points.md) |

---

## Diagram Categories

### System Overview
- **System Architecture** - Shows all external systems (Email, Cytora, I90, CTP, Saperion) and how they connect to the Azi Cytora BFI service
- **Component Architecture** - Illustrates the Clean Architecture pattern with Domain, Infrastructure, and BackgroundTasks layers
- **Integration Points** - Comprehensive map of all external integrations with authentication flows

### Application Flow
- **Message Flow State Machine** - The 11-state workflow that every message follows through the system
- **Standard Flow Sequence** - Step-by-step sequence for messages routed to Saperion (80% of traffic)
- **CTP Alternative Flow** - Alternative routing for 200+ occupation codes requiring CTP validation

### Technical Implementation
- **Dependency Injection** - How services are registered and resolved in the DI container
- **Task Orchestration** - Execution model for 7 scheduled tasks and 3 message consumers
- **Error Handling Flow** - Retry logic, dead letter queues, and graceful degradation

### Infrastructure
- **Deployment Architecture** - Kubernetes pods, services, ConfigMaps, Secrets, and health checks
- **Configuration Hierarchy** - How appsettings files are layered and merged
- **Data Model ERD** - Entity relationships for CytoraMessage, CytoraMessageResponse, and related tables

---

## Diagram Summaries

### 1. System Architecture
Shows the high-level architecture with all external system integrations and infrastructure components.

**Key Components:**
- External systems: Email (Graph API), Cytora, I90, CTP, Saperion
- Infrastructure: Kubernetes pod, RabbitMQ, SQL Server, ConfigMaps, Secrets
- Communication patterns: REST APIs, Message Queues, Database connections

[**View Full Diagram →**](Diagrams/01_System_Architecture.md)

---

### 2. Component Architecture
Illustrates the Clean Architecture pattern with Domain, Infrastructure, and BackgroundTasks layers.

**Key Components:**
- Presentation Layer: 7 scheduled tasks + 3 message consumers
- Domain Layer: Entities, DTOs, Interfaces, Value Objects
- Infrastructure Layer: DbContext, Repositories, Services, Migrations
- Dependency flow from outer layers to inner layers

[**View Full Diagram →**](Diagrams/02_Component_Architecture.md)

---

### 3. Dependency Injection
Shows how the DI container is configured and services are registered in the startup sequence.

**Key Registrations:**
- DbContext (Scoped) - Entity Framework Core
- Repositories (Scoped) - Data access layer
- Services (Scoped/Singleton) - Business logic and API clients
- Background Tasks (Singleton) - 7 scheduled tasks
- Message Consumers (Scoped) - 3 RabbitMQ consumers

[**View Full Diagram →**](Diagrams/03_Dependency_Injection.md)

---

### 4. Task Orchestration
Demonstrates the execution model for background tasks with timer-based polling and scope management.

**Key Concepts:**
- Each task runs independently on configurable timer intervals
- Service scopes created per execution for proper DbContext lifecycle
- CancellationToken support for graceful shutdown
- Parallel execution of all tasks and consumers

[**View Full Diagram →**](Diagrams/04_Task_Orchestration.md)

---

### 5. Message Flow State Machine
The 11-state workflow state machine that every CytoraMessage follows through the system.

**Workflow States:**
1. EmailReceived → 2. SendingMessage → 3. MessageSent → 4. ReturnReceivedFromCytora
5. ClientSentToBeCreated → 6. I90ClientKeyUpdated → 7. FastQuoteSent
8. I90PolicyNumberUpdated → 9. SaperionMessageSent (Standard) OR CTPMessageSent (Alternative)
10. IntegrationCompleted
11. Error (with error message logging)

[**View Full Diagram →**](Diagrams/05_Message_Flow.md)

---

### 6. Standard Flow Sequence
Step-by-step sequence diagram for the standard flow (email → Saperion) covering ~80% of messages.

**Flow Steps:**
1. RetrieveEmailTask fetches emails via Graph API
2. PushEmailToCytoraTask sends email to Cytora for AI assessment
3. RetrieveJSONFromCytoraTask polls for results
4. CreateClientTask sends MessageBridgeRequest to I90
5. CreateFastQuoteTask receives client key, creates fast quote
6. PushEmailToSaperionTask archives to document management system
7. Status updates to IntegrationCompleted (StatusId=10)

[**View Full Diagram →**](Diagrams/06_Standard_Flow.md)

---

### 7. CTP Alternative Flow
Alternative flow for 200+ occupation codes requiring CTP platform validation instead of Saperion.

**Key Differences:**
- Routing decision based on OccupationCode lookup in StaticAlternativeFlowConfiguration table
- SendMessageToCTPFlowTask publishes to Cytora.CTPExceptions queue
- ConsumerCTPExceptions handles validation errors and updates status
- Failed validation returns message to Cytora UI for manual review
- Average time: 15-20 minutes (vs 10-15 minutes for standard flow)

[**View Full Diagram →**](Diagrams/07_CTP_Flow.md)

---

### 8. Data Model ERD
Entity-Relationship Diagram showing database schema and table relationships.

**Core Tables:**
- **CytoraMessage**: Main entity (Id, CytoraId, StatusId, Subject, Message, timestamps)
- **CytoraMessageStatus**: Workflow states (1-11)
- **CytoraMessageResponse**: Cytora AI results (OccupationCode, EircodeIdentifier, JsonCytora, etc.)
- **Address**: Location data (Eircode, AddressLine)
- **StaticAlternativeFlowConfiguration**: CTP routing rules (OccupationCode → FlowIdentifier)

[**View Full Diagram →**](Diagrams/08_Data_Model.md)

---

### 9. Deployment Architecture
Kubernetes deployment structure with pods, services, ConfigMaps, Secrets, and health checks.

**Infrastructure:**
- Pod: cytorabfi-backgroundtasks (single container, .NET 6)
- Service: NodePort on port 80
- ConfigMap: api-config (appsettings.cytorabfi-backgroundtasks.json)
- Secret: secret-appsettings (passwords, API keys, connection strings)
- Health Check: /health endpoint for liveness/readiness probes
- Docker Registry: af-dev.azire-tools.we1.azure.aztec.cloud.allianz

[**View Full Diagram →**](Diagrams/09_Deployment.md)

---

### 10. Configuration Hierarchy
Configuration sources and precedence order from appsettings.json to environment variables.

**Priority Order (Last Wins):**
1. appsettings.json (base configuration)
2. appsettings.{ENVIRONMENT}.json (K8SDEV/TEST/UAT/PROD)
3. secrets/appsettings.secrets.json (Kubernetes Secret mount)
4. appsettings.cytorabfi-backgroundtasks.json (Kubernetes ConfigMap mount)
5. Environment variables (ASPNETCORE_ENVIRONMENT, etc.)

**Configuration Bindings:**
- CytoraBackgroundTasksSettingsWrapper (task intervals, IsEnabled flags)
- CytoraServicesSettingsWrapper (Cytora API URLs, credentials)
- RabbitMQConfigSettings (queue names, consumers)
- ConnectionStrings (SQL Server)

[**View Full Diagram →**](Diagrams/10_Configuration.md)

---

### 11. Error Handling Flow
Error recovery and retry strategies for transient errors, database failures, and pod crashes.

**Error Scenarios:**
- **Transient Errors**: Retry up to max count with configurable interval
- **Permanent Errors**: Log error, continue to next message
- **Database Connection Errors**: EF Core retry logic (1-10 attempts, up to 30s wait)
- **OutOfMemoryException**: Let pod crash, Kubernetes restarts automatically
- **Dead Letter Queue**: Messages moved after max retry count exceeded
- **CancellationToken**: Graceful shutdown on pod termination

[**View Full Diagram →**](Diagrams/11_Error_Handling.md)

---

### 12. Integration Points
Comprehensive map of all external system integrations with authentication flows.

**Integrations:**
1. **Microsoft Graph API** - OAuth 2.0 with Azure AD for email retrieval
2. **Cytora Platform** - JWT token authentication (Client Credentials flow) for risk assessment
3. **I90 Enterprise System** - MessageBridge with RabbitMQ for client/policy management
4. **CTP Trading Platform** - RabbitMQ queue (Cytora.CTPExceptions) for policy validation
5. **Saperion DMS** - Queue-based document archival
6. **Infrastructure** - SQL Server (EF Core), RabbitMQ (MassTransit), Azi Distributed Logger (REST API)

[**View Full Diagram →**](Diagrams/12_Integration_Points.md)
    S8: I90PolicyNumberUpdated (8)
    S9: MessageToSaperionSent (9)
    S10: ReturnReceivedFromCytoraForCTPFlow (10)
    S11: MessageToCTPFlowSent (11)
    S99: Error (99)

    S1 --> S2: RetrieveEmailTask saves
    S2 --> S3: PushEmailToCytoraTask uploads
    S3 --> S4: RetrieveJSONFromCytoraTask<br/>(Standard Flow)
    S3 --> S10: RetrieveJSONFromCytoraTask<br/>(CTP Flow - Occupation Code Match)
    
    S4 --> S5: CreateClientTask publishes
    S5 --> S6: ConsumerI90ClientKey updates
    S6 --> S7: CreateFastQuoteTask sends
    S7 --> S8: ConsumerI90PolicyNumber updates
    S8 --> S9: PushEmailToSaperionTask archives
    
    S10 --> S11: SendMessageToCTPFlowTask publishes
    S11 --> S3: ConsumerCTPExceptions<br/>(Validation Failed - Manual Review)
    
    S9 --> [*]: Standard Flow Complete
    S11 --> [*]: CTP Flow Complete
    
    S2 --> S99: Upload Failure
    S3 --> S99: Cytora Error
    S4 --> S99: Processing Error
    S5 --> S99: I90 Integration Error
    
    S99 --> [*]: Manual Intervention Required

    note right of S3
        Decision Point:
        Check occupation code
        in StaticAlternativeFlowConfiguration
    end note

    note right of S11
        CTP validates policy
        If errors → back to S3
    end note
```

---

## 6. Standard Flow Sequence Diagram

```mermaid
sequenceDiagram
    participant Mailbox as Shared Mailbox
    participant Task1 as RetrieveEmailTask
    participant DB as SQL Server
    participant Task2 as PushEmailToCytoraTask
    participant Cytora as Cytora Platform
    participant Task3 as RetrieveJSONFromCytoraTask
    participant Task4 as CreateClientTask
    participant RMQ as RabbitMQ
    participant I90 as I90 System
    participant Task5 as CreateFastQuoteTask
    participant Task8 as PushEmailToSaperionTask
    participant Saperion as Saperion DMS

    rect rgb(200, 220, 240)
        Note over Mailbox,Task1: Phase 1: Email Ingestion
        Mailbox->>Task1: Email arrives in inbox
        Task1->>Mailbox: Graph API: Get email content
        Task1->>DB: INSERT CytoraMessage (StatusId=1)
        Task1->>Mailbox: Graph API: Move to archive
    end

    rect rgb(220, 240, 200)
        Note over Task2,Cytora: Phase 2: Upload to Cytora
        Task2->>DB: SELECT * WHERE StatusId=1
        Task2->>DB: UPDATE StatusId=2 (Sending)
        Task2->>Cytora: POST GetPreSignedUrl
---

## How to Use This Documentation

1. **Start here** - Use the quick navigation table above to find specific diagrams
2. **Click "View →"** links to see individual diagram files with detailed explanations
3. **Follow references** - Each diagram file includes cross-references to related diagrams
4. **Print friendly** - Individual files are optimized for printing and PDF export

## Related Documentation

- [Comprehensive Documentation](COMPREHENSIVE_DOCUMENTATION.md) - Complete system guide
- [Quick Reference](QUICK_REFERENCE.md) - Cheat sheet for developers and ops
- [README](README.md) - Documentation home

---

**Document Version:** 2.0 (Index Format)  
**Last Updated:** January 21, 2026  
**Change History:**
- v2.0 (Jan 21, 2026): Converted to index format with individual diagram files
- v1.0 (Jan 20, 2026): Original monolithic format with all 12 diagrams inline

