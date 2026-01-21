# Configuration Hierarchy

[← Back to Diagrams Index](../ARCHITECTURE_DIAGRAMS.md)

---

## Diagram

```mermaid
graph LR
    subgraph "Configuration Sources (Priority Order)"
        A[1. appsettings.json<br/>Base Configuration]
        B[2. appsettings.ENVIRONMENT.json<br/>K8SDEV/TEST/UAT/PROD]
        C[3. secrets/appsettings.secrets.json<br/>Kubernetes Secret]
        D[4. appsettings.cytorabfi-backgroundtasks.json<br/>Kubernetes ConfigMap]
        E[5. Environment Variables<br/>ASPNETCORE_ENVIRONMENT]
    end

    A --> B
    B --> C
    C --> D
    D --> E

    E --> Final[Final Configuration<br/>IConfiguration]

    subgraph "Configuration Bindings"
        Final --> Settings1[CytoraBackgroundTasksSettingsWrapper]
        Final --> Settings2[CytoraServicesSettingsWrapper]
        Final --> Settings3[RabbitMQConfigSettings]
        Final --> Settings4[ConnectionStrings]
    end

    Settings1 --> Tasks[Background Tasks<br/>IsEnabled, TimePeriod, etc.]
    Settings2 --> APIs[API Endpoints<br/>Cytora URLs, Credentials]
    Settings3 --> MQ[RabbitMQ<br/>Queues, Consumers]
    Settings4 --> DB[SQL Server<br/>Connection String]

    style Final fill:#4CAF50,color:#fff
    style C fill:#F44336,color:#fff
    style D fill:#FF9800,color:#000
```

---

See [Configuration Hierarchy](10_Configuration.md) for complete configuration details.

---

**Last Updated:** January 21, 2026
