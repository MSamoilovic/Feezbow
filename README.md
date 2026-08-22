# Feezbow

Feezbow is a backend platform for household organization and collaborative task management. It is built on the .NET 8 stack, following Clean Architecture principles and the CQRS pattern.

Beyond a classic Kanban (Trello-style) module for projects, boards, columns, and tasks, the system also covers a broader household domain — chores, calendar, budgets, bills with cost splitting, pantry, shopping lists, recipes, and meal plans. On top of all that sits a layer of agentic AI features powered by Anthropic Claude models, turning free text, receipts, and PDF documents into structured data.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Running the Project](#running-the-project)
- [Configuration](#configuration)
- [Database and Migrations](#database-and-migrations)
- [Testing](#testing)
- [API and Realtime](#api-and-realtime)
- [Background Services](#background-services)
- [AI Capabilities](#ai-capabilities)

## Features

The domain is split into several feature modules (`Feezbow.Application/Features/`), each responsible for a single self-contained area.

### Projects and Tasks

- Projects with membership, roles (`ProjectRole`), status, and visibility
- Boards, columns, and tasks (Kanban model) with labels, priorities, and task types
- Comments, attachments, and an activity log
- Realtime change synchronization over SignalR

### Household Domain

- Household profile and membership with roles (`HouseholdRole`)
- Chores with scheduling and recurrence
- Household calendar and events
- Budgets and bills with cost splitting (bill splits) and payment tracking
- Pantry, shopping lists, recipes, and meal plans
- A unified household dashboard

### System Capabilities

- JWT authentication with email confirmation and authorization policies
- Caching (Redis as the primary store, in-memory as a fallback)
- User notifications
- OData endpoints for advanced querying
- API versioning and Swagger/OpenAPI documentation
- Structured logging (Serilog) with a Seq sink
- A health check endpoint and background services for reminders, recurring tasks/bills, and the weekly digest

## Tech Stack

| Area              | Technology                                            |
| ----------------- | ----------------------------------------------------- |
| Runtime           | .NET 8 (ASP.NET Core)                                 |
| Language          | C#                                                    |
| Database          | PostgreSQL 16 (Npgsql + EF Core 9)                    |
| Cache             | Redis (StackExchange.Redis) with in-memory fallback   |
| Mediator / CQRS   | MediatR 12                                             |
| Validation        | FluentValidation 12                                   |
| Realtime          | SignalR                                               |
| Authentication    | ASP.NET Core Identity + JWT Bearer                    |
| AI                | Anthropic.SDK (Claude models)                         |
| PDF / OCR input   | PdfPig                                                 |
| Resilience        | Polly                                                 |
| API versioning    | Asp.Versioning                                        |
| Documentation     | Swashbuckle (Swagger) + OData.Swagger                 |
| Logging           | Serilog (Console, File, Seq)                          |
| Testing           | xUnit + Moq                                           |

## Architecture

The project follows Clean Architecture with four layers; dependencies point inward:

```
API            -> Application -> Domain
Infrastructure -> Application -> Domain
```

- **Domain** — entities, value objects, domain events, repository and service interfaces, business rule exceptions
- **Application** — CQRS handlers (MediatR), commands and queries organized by feature, MediatR pipeline behaviors, cache key constants
- **Infrastructure** — EF Core + Npgsql, repository implementations, Redis/in-memory cache, JWT, email, SignalR, AI service
- **API** — controllers, middleware pipeline, DI configuration, `Program.cs`

A few conventions the project deliberately enforces:

- All business logic goes through MediatR; controllers stay thin and only forward commands and queries.
- Handlers never inject `IUnitOfWork` directly — repository access flows exclusively through domain services (`Feezbow.Domain/Services`).
- Every command has a companion FluentValidation validator in the same feature folder; validation runs automatically through `ValidationBehavior` before the handler is invoked.
- Queries use `ICacheService.GetOrSetAsync`, and commands call `ICacheService.RemoveAsync` after mutations.

## Project Structure

```
Feezbow.sln
Feezbow.API/                 Presentation layer (controllers, middleware, DI, Program.cs)
Feezbow.Application/         CQRS handlers, commands/queries per feature, pipeline behaviors
Feezbow.Domain/             Entities, value objects, domain services, interfaces, enums
Feezbow.Infrastructure/     EF Core, repositories, cache, JWT, email, SignalR, AI
Feezbow.Domain.Tests/       Domain unit tests
Feezbow.Application.Tests/  Application layer unit tests
Feezbow.Integration.Tests/  Integration tests
docker/                      Docker helper files (init.sql, etc.)
docker-compose.yml           Local stack orchestration
Dockerfile                   API image build
```

## Prerequisites

- .NET SDK 8.0 (the version is pinned in `global.json`)
- PostgreSQL 16 (locally or via Docker)
- Redis (optional; enabled through configuration)
- Docker and Docker Compose (to run the full stack)

## Running the Project

### Environment Setup
A
Copy the example configuration and fill in the values:

```bash
cp .env.example .env
```

At a minimum you must set `POSTGRES_PASSWORD` and `JWT_KEY`; if you use the AI features, also add an Anthropic API key. Without `POSTGRES_PASSWORD` and `JWT_KEY`, `docker compose` will not even start (the values are required).

### Running the API Locally

Requires a running PostgreSQL and, optionally, Redis configured through `.env`:

```bash
dotnet build Feezbow.sln
dotnet run --project Feezbow.API
```

### Docker (Full Stack)

```bash
# Core services (postgres, redis, api)
docker compose up -d

# With additional tools (mailpit, pgadmin)
docker compose --profile tools up -d
```

The API is exposed on the port from `API_PORT` (default 5000), while inside the container it listens on 8080.

## Configuration

Secrets and environment-specific values live in the `.env` file, which is not under version control. The main sections in `appsettings.json`:

| Section                                | Notable keys                                                                         |
| -------------------------------------- | ------------------------------------------------------------------------------------ |
| `ConnectionStrings:DefaultConnection`  | PostgreSQL DSN                                                                        |
| `Redis`                                | `Enabled`, `ConnectionString`, `InstanceName`, `DefaultExpirationMinutes`            |
| `Jwt`                                  | `Key`, `Issuer`, `Audience`, `ExpiryMinutes`                                          |
| `Cors:AllowedOrigins`                  | Frontend URL whitelist                                                               |
| `EmailConfirmation:TokenLifespanHours` | Email token TTL                                                                       |
| `Email`                                | SMTP host, port, credentials, sender                                                 |
| `SignalR`                              | Typed via `SignalROptions`                                                            |
| `Anthropic`                            | `ApiKey`, `DefaultModel`, `MaxTokensDefault`, feature flags, `MinConfidenceScore`    |

## Database and Migrations

Migrations are applied automatically on application startup (`context.Database.Migrate()`), so during development you usually don't need to do anything by hand.

Adding a new migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project Feezbow.Infrastructure \
  --startup-project Feezbow.API
```

## Testing

```bash
# All tests
dotnet test

# A single test project
dotnet test Feezbow.Application.Tests
dotnet test Feezbow.Domain.Tests

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

Conventions: xUnit + Moq, AAA pattern (Arrange / Act / Assert), one `[Fact]` per scenario. Tests live alongside the feature they cover.

## API and Realtime

- **Versioning** — via URL segment and the `X-Api-Version` header (default version 1.0)
- **Documentation** — Swagger UI available in the Development environment
- **Health check** — `GET /health`
- **OData** — additional endpoints for advanced querying

SignalR hubs (all require authentication):

| Path                  | Hub               | Purpose                        |
| --------------------- | ----------------- | ------------------------------ |
| `/hubs/boards`        | `BoardHub`        | Realtime board changes         |
| `/hubs/projects`      | `ProjectHub`      | Realtime project changes       |
| `/hubs/notifications` | `NotificationHub` | User notifications             |
| `/hubs/presence`      | `PresenceHub`     | Presence tracking              |

Group naming convention: `Board:{id}`, `Project:{id}`, `User:{id}`.

## Background Services

Several `IHostedService` implementations run in the background and are registered in `Feezbow.Infrastructure/ConfigureServices.cs`:

- `DueTaskReminderService` — reminders for tasks approaching their due date
- `RecurringTaskGeneratorJob` — generation of recurring tasks
- `RecurringBillGeneratorJob` — generation of recurring bills
- `DigestScheduler` — triggers the weekly household AI digest

## AI Capabilities

Feezbow integrates Anthropic Claude models through `AnthropicAiService` in the Infrastructure layer, behind the `IAIService` abstraction in the application layer. Each AI feature has its own feature flag in the `Anthropic` configuration and can be toggled independently. Suggestions whose confidence falls below the `MinConfidenceScore` threshold are discarded before reaching the user, and calls to the model are guarded by Polly resilience policies.

Supported features:

- **Recipe parsing** — extracting structured recipes from free text
- **Task parsing** — creating tasks from natural language
- **Pantry receipt parsing** — recognizing items from grocery receipts
- **Bill OCR** — extracting bill line items from a PDF or image for cost splitting
- **Weekly digest** — a summary of household activity, triggered by a background scheduler
- **Meal plan suggestion** — generating menu suggestions
- **Shopping list cleanup** — normalizing and deduplicating items

All AI features can be disabled globally by setting `Anthropic:Enabled` to `false`, after which the application keeps running without a single call to the external model.
