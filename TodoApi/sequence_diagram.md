```mermaid
sequenceDiagram
    autonumber
    participant Host as ".NET Host"
    participant Task as "PushEmailToCytoraTask (BackgroundService)"
    participant SP as "IServiceProvider"
    participant Scope as "IServiceScope"
    participant Repo as "IEmailRepository"
    participant DB as "CytoraDbContext"
    participant Service as "ICytoraService"
    participant API as "Cytora API"
    participant Logger as "ILogger"

    Host->>Task: StartAsync(CancellationToken)
    Task->>Task: ExecuteAsync(CancellationToken)

    loop Polling Loop (until CancellationToken cancelled)
        Task->>Logger: LogInformation("Starting email push cycle")

        Task->>SP: CreateScope()
        SP-->>Task: scope (IServiceScope)

        Task->>Scope: GetRequiredService<IEmailRepository>()
        Scope-->>Task: Repo

        Task->>Scope: GetRequiredService<ICytoraService>()
        Scope-->>Task: Service

        Task->>Repo: GetUnprocessedEmailsAsync()
        Repo->>DB: Query Emails where Status='New' (limit 100)
        DB-->>Repo: List<Email>
        Repo-->>Task: unprocessedEmails

        alt Emails found
            loop For each unprocessedEmail
                Task->>Task: Map Email -> CytoraMessageDto
                Note over Task: Create CytoraMessageDto(email.Name, email.Message, email.MailboxIdentifier)

                Task->>Service: PushSubmissionAsync(cytoraMessageDto)
                Service->>API: POST /api/submissions (JSON payload)

                alt Success (2xx)
                    API-->>Service: 202 Accepted (submissionId)
                    Service-->>Task: CytoraResponse(submissionId)

                    Task->>Repo: UpdateEmailStatusAsync(email.Id, "PushedToCytora", submissionId)
                    Repo->>DB: UPDATE Emails set Status, SubmissionId where Id=email.Id
                    DB-->>Repo: OK
                    Repo-->>Task: OK

                    Task->>Logger: LogInformation("Pushed email {EmailId} to Cytora", email.Id)

                else API Failure (4xx/5xx)
                    API-->>Service: Error response
                    Service-->>Task: throws CytoraApiException

                    Task->>Logger: LogError(ex, "Failed to push email {EmailId}", email.Id)

                    Task->>Repo: UpdateEmailStatusAsync(email.Id, "Failed")
                    Repo->>DB: UPDATE Emails set Status='Failed', RetryCount=RetryCount+1 where Id=email.Id
                    DB-->>Repo: OK
                    Repo-->>Task: OK
                end
            end

            Task->>Repo: SaveChangesAsync()
            Repo->>DB: COMMIT
            DB-->>Repo: OK
            Repo-->>Task: OK

        else No emails found
            Task->>Logger: LogDebug("No unprocessed emails found")
        end

        Task->>Scope: Dispose()
        Task->>Logger: LogInformation("Completed push cycle, waiting {Interval} minutes", 5)
        Task->>Task: Delay(TimeSpan.FromMinutes(5), stoppingToken)

        alt CancellationToken triggered
            Note over Host,Task: Graceful shutdown requested
            Task->>Logger: LogInformation("Shutdown requested")
        end
    end

    Task->>Logger: LogInformation("Task stopped")
    Host->>Task: StopAsync(CancellationToken)
