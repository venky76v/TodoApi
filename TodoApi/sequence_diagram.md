```mermaid
sequenceDiagram
    autonumber
    participant Host as .NET Host
    participant Task as PushEmailToCytoraTask<br/>(BackgroundService)
    participant SP as IServiceProvider
    participant Scope as Service Scope
    participant Repo as IEmailRepository
    participant DB as CytoraDbContext
    participant Service as ICytoraService
    participant API as Cytora API
    participant Logger as ILogger

    Host->>Task: StartAsync(CancellationToken)
    activate Task
    
    Task->>Task: ExecuteAsync(CancellationToken)
    
    loop Polling Loop (until CancellationToken cancelled)
        Task->>Logger: LogInformation("Starting email push cycle")
        
        Task->>SP: CreateScope()
        activate Scope
        SP-->>Task: IServiceScope
        
        Task->>Scope: GetRequiredService<IEmailRepository>()
        Scope-->>Task: EmailRepository instance
        
        Task->>Scope: GetRequiredService<ICytoraService>()
        Scope-->>Task: CytoraService instance
        
        Task->>Repo: GetUnprocessedEmailsAsync()
        activate Repo
        Repo->>DB: SELECT * FROM Emails<br/>WHERE Status = 'New'<br/>LIMIT 100
        activate DB
        DB-->>Repo: List<Email>
        deactivate DB
        Repo-->>Task: unprocessedEmails
        deactivate Repo
        
        alt Emails found
            loop For each unprocessedEmail
                Task->>Task: Map Email → CytoraMessageDto
                Note over Task: new CytoraMessageDto(<br/>email.Name,<br/>email.Message,<br/>email.MailboxIdentifier)
                
                Task->>Service: PushSubmissionAsync(cytoraMessageDto)
                activate Service
                Service->>API: POST /api/submissions<br/>Body: JSON(cytoraMessageDto)
                activate API
                
                alt Success (2xx)
                    API-->>Service: 202 Accepted<br/>{submissionId: "12345"}
                    deactivate API
                    Service-->>Task: CytoraResponse(submissionId)
                    deactivate Service
                    
                    Task->>Repo: UpdateEmailStatusAsync(email.Id, "PushedToCytora", submissionId)
                    activate Repo
                    Repo->>DB: UPDATE Emails<br/>SET Status='PushedToCytora',<br/>SubmissionId='12345'<br/>WHERE Id = email.Id
                    DB-->>Repo: 1 row affected
                    Repo-->>Task: Success
                    deactivate Repo
                    
                    Task->>Logger: LogInformation("Pushed email {EmailId} to Cytora", email.Id)
                    
                else API Failure (4xx/5xx)
                    API-->>Service: 500 Internal Server Error
                    deactivate API
                    Service-->>Task: throws CytoraApiException
                    deactivate Service
                    
                    Task->>Task: catch (CytoraApiException ex)
                    Task->>Logger: LogError(ex, "Failed to push email {EmailId}", email.Id)
                    
                    Task->>Repo: UpdateEmailStatusAsync(email.Id, "Failed")
                    activate Repo
                    Repo->>DB: UPDATE Emails<br/>SET Status='Failed',<br/>RetryCount++
                    DB-->>Repo: 1 row affected
                    Repo-->>Task: Success
                    deactivate Repo
                end
            end
            
            Task->>Repo: SaveChangesAsync()
            activate Repo
            Repo->>DB: COMMIT transaction
            DB-->>Repo: Success
            Repo-->>Task: Success
            deactivate Repo
            
        else No emails found
            Task->>Logger: LogDebug("No unprocessed emails found")
        end
        
        Task->>Scope: Dispose()
        deactivate Scope
        Note over Task,Scope: DbContext disposed,<br/>connections released
        
        Task->>Logger: LogInformation("Completed push cycle, waiting {Interval} minutes", 5)
        Task->>Task: await Task.Delay(<br/>TimeSpan.FromMinutes(5),<br/>stoppingToken)
        
        alt CancellationToken triggered
            Note over Host,Task: Graceful shutdown requested
            Task->>Logger: LogInformation("Shutdown requested")
        end
    end
    
    Task->>Logger: LogInformation("Task stopped")
    deactivate Task
    Host->>Task: StopAsync(CancellationToken)
```
