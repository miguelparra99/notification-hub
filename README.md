\# NotificationHub



A multi-channel notification service built with .NET 10 and Clean Architecture. It handles email, SMS and push delivery behind a single API, with template rendering, provider abstraction and automatic retries.



\## Why this exists



Most applications eventually need to notify users, and that logic tends to get scattered across the codebase — SMTP calls in one service, an SMS gateway in another, no shared retry policy, no delivery history. NotificationHub centralises it: one endpoint, pluggable providers, and a full audit trail of every delivery attempt.



\## Features



\- \*\*Multi-channel delivery\*\* — email (SMTP), SMS and push, resolved at runtime per notification

\- \*\*Template engine\*\* — reusable templates with `{{placeholder}}` substitution and validation of missing values

\- \*\*Automatic retries\*\* — failed deliveries are rescheduled with exponential backoff (1, 2, 4 minutes), capped at three attempts

\- \*\*Delivery history\*\* — every attempt is persisted with provider, timestamp and error detail

\- \*\*Provider abstraction\*\* — adding a new gateway means implementing one interface; nothing else changes

\- \*\*Domain-driven state\*\* — status transitions are enforced by the domain model, not by callers



\## Architecture



The solution follows Clean Architecture, with dependencies pointing inward:



```

NotificationHub.Domain          entities, value rules, domain exceptions — no external dependencies

NotificationHub.Application     use cases, DTOs, provider and repository interfaces

NotificationHub.Infrastructure  EF Core persistence, SMTP and simulated providers

NotificationHub.Api             controllers, exception middleware, dependency wiring

```



The domain layer holds behaviour, not just data. A `Notification` cannot be marked as sent from an invalid state, retry scheduling lives inside the aggregate, and delivery attempts are appended through the aggregate root rather than manipulated directly.



\## Tech stack



.NET 10 · ASP.NET Core · Entity Framework Core · SQL Server · FluentValidation · MailKit · xUnit · Docker



\## Getting started



\*\*Prerequisites:\*\* .NET 10 SDK, Docker.



```bash

\# start SQL Server

docker run -e "ACCEPT\_EULA=Y" -e "MSSQL\_SA\_PASSWORD=<your-password>" \\

&#x20; -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest



\# configure

cp src/NotificationHub.Api/appsettings.Development.example.json \\

&#x20;  src/NotificationHub.Api/appsettings.Development.json

\# then set your password in the connection string



\# run — migrations are applied automatically in development

dotnet run --project src/NotificationHub.Api

```



Swagger UI is available at `https://localhost:7213/swagger`.



\## API



\### Send a notification



```http

POST /api/v1/notifications

```



```json

{

&#x20; "channel": 2,

&#x20; "recipient": "+593987654321",

&#x20; "body": "Your verification code is 482913"

}

```



Channels: `1` email · `2` SMS · `3` push. Email requires a `subject`. Instead of a raw `body`, you can supply a `templateCode` with `templateValues`.



```json

{

&#x20; "id": "9a3f209a-10d5-49b5-9262-df2d18326f40",

&#x20; "channel": "Sms",

&#x20; "status": "Failed",

&#x20; "attemptCount": 1,

&#x20; "nextRetryAt": "2026-08-14T17:46:02Z",

&#x20; "attempts": \[

&#x20;   {

&#x20;     "attemptNumber": 1,

&#x20;     "succeeded": false,

&#x20;     "providerName": "SimulatedSms",

&#x20;     "errorMessage": "Simulated provider timeout."

&#x20;   }

&#x20; ]

}

```



\### Retrieve a notification



```http

GET /api/v1/notifications/{id}

```



Returns the notification with its full delivery history.



\## Notes



SMS and push currently use a simulated provider that fails intermittently, so retry behaviour can be observed without external credentials. Swapping in a real gateway means implementing `INotificationSender` and registering it — no changes to the domain or application layers.



\## License



MIT

