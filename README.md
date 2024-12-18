# TodoApi

This is a **.NET core API** deployed to **Azure** as an **Azure App Service** instance, then this Azure App Service is exposed using **Azure API management**. **Azure API management** can be used for the fo11owing:

1. Centralized API Gateway
- Acts as a single entry point for managing multiple APIs.
- Enables easy integration across internal and external systems.
2. Security and Access Control
- Authentication & Authorization: Supports OAuth, JWT validation, and Azure Active Directory integration.
- Rate Limiting & Throttling: Prevents abuse by limiting API usage.
- IP Filtering: Allows access only from trusted sources.
- Data Masking: Redacts sensitive information in responses.
3. Developer Enablement
- Developer Portal: A customizable portal for developers to explore, test, and subscribe to APIs.
- Interactive Documentation: Automatically generates OpenAPI/Swagger documentation.
- Sandbox Testing: Developers can test APIs without impacting production environments.
4. Monitoring and Analytics
- Tracks usage metrics, latency, errors, and trends.
- Provides insights through dashboards and integrations with tools like Azure Monitor or Application Insights.
